using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace BankServiceViewer;

public class ConnectionDialog : Form
{
    private readonly TextBox _dataSourceTextBox;
    private readonly TextBox _userIdTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly ComboBox _databaseComboBox;
    private readonly Button _connectButton;
    private readonly Button _okButton;
    private readonly Label _statusLabel;

    public string ConnectionString
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_databaseComboBox.Text))
            {
                throw new InvalidOperationException("Lütfen bir veri tabanı seçin.");
            }

            var builder = new SqlConnectionStringBuilder(BuildBaseConnectionString())
            {
                InitialCatalog = _databaseComboBox.Text
            };

            return builder.ConnectionString;
        }
    }

    public ConnectionDialog()
    {
        Text = "Veri Tabanı Bağlantısı";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        _dataSourceTextBox = new TextBox { Width = 280 };
        _userIdTextBox = new TextBox { Width = 280 };
        _passwordTextBox = new TextBox { Width = 280, UseSystemPasswordChar = true };
        _databaseComboBox = new ComboBox
        {
            Width = 280,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };

        _connectButton = new Button { Text = "Bağlantı Aç", AutoSize = true };
        _okButton = new Button { Text = "Tamam", AutoSize = true, Enabled = false, DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "İptal", AutoSize = true, DialogResult = DialogResult.Cancel };

        _statusLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.DarkSlateGray,
            Text = "Bilgileri girip 'Bağlantı Aç' ile veri tabanlarını listeleyin."
        };

        AcceptButton = _okButton;
        CancelButton = cancelButton;

        _connectButton.Click += async (_, _) => await LoadDatabasesAsync();
        _okButton.Click += (_, _) => EnsureDatabaseSelection();

        var layout = BuildLayout(cancelButton);
        Controls.Add(layout);

        PrefillFromConfiguration();
    }

    private Control BuildLayout(Control cancelButton)
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 6,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(4),
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label { Text = "Data Source", AutoSize = true, Margin = new Padding(3, 6, 3, 6) }, 0, 0);
        layout.Controls.Add(_dataSourceTextBox, 1, 0);
        layout.Controls.Add(new Label { Text = "User ID", AutoSize = true, Margin = new Padding(3, 6, 3, 6) }, 0, 1);
        layout.Controls.Add(_userIdTextBox, 1, 1);
        layout.Controls.Add(new Label { Text = "Password", AutoSize = true, Margin = new Padding(3, 6, 3, 6) }, 0, 2);
        layout.Controls.Add(_passwordTextBox, 1, 2);
        layout.Controls.Add(new Label { Text = "Veri Tabanı", AutoSize = true, Margin = new Padding(3, 6, 3, 6) }, 0, 3);
        layout.Controls.Add(_databaseComboBox, 1, 3);

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };

        buttonPanel.Controls.Add(_connectButton);
        buttonPanel.Controls.Add(_okButton);
        buttonPanel.Controls.Add(cancelButton);

        layout.Controls.Add(buttonPanel, 0, 4);
        layout.SetColumnSpan(buttonPanel, 2);

        layout.Controls.Add(_statusLabel, 0, 5);
        layout.SetColumnSpan(_statusLabel, 2);

        return layout;
    }

    private async Task LoadDatabasesAsync()
    {
        if (string.IsNullOrWhiteSpace(_dataSourceTextBox.Text) || string.IsNullOrWhiteSpace(_userIdTextBox.Text))
        {
            MessageBox.Show(this, "Data Source ve User ID boş bırakılamaz.", "Eksik bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ToggleLoadingState(true);
        try
        {
            _databaseComboBox.Items.Clear();

            using var connection = new SqlConnection(BuildBaseConnectionString());
            using var command = new SqlCommand("SELECT name FROM sys.databases ORDER BY name", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _databaseComboBox.Items.Add(reader.GetString(0));
            }

            if (_databaseComboBox.Items.Count > 0)
            {
                _databaseComboBox.Enabled = true;
                _databaseComboBox.SelectedIndex = 0;
                _okButton.Enabled = true;
                _statusLabel.Text = "Veri tabanları yüklendi. Lütfen seçim yapın.";
                _statusLabel.ForeColor = Color.DarkGreen;
            }
            else
            {
                _databaseComboBox.Enabled = false;
                _okButton.Enabled = false;
                _statusLabel.Text = "Herhangi bir veri tabanı bulunamadı.";
                _statusLabel.ForeColor = Color.Firebrick;
            }
        }
        catch (Exception ex)
        {
            _databaseComboBox.Enabled = false;
            _okButton.Enabled = false;
            _statusLabel.Text = "Bağlantı başarısız.";
            _statusLabel.ForeColor = Color.Firebrick;
            MessageBox.Show(this, ex.Message, "Bağlantı hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleLoadingState(false);
        }
    }

    private void ToggleLoadingState(bool isLoading)
    {
        _connectButton.Enabled = !isLoading;
        _okButton.Enabled = !isLoading && _databaseComboBox.Enabled;
        UseWaitCursor = isLoading;
        Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
    }

    private string BuildBaseConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = _dataSourceTextBox.Text.Trim(),
            UserID = _userIdTextBox.Text.Trim(),
            Password = _passwordTextBox.Text,
            TrustServerCertificate = true,
            InitialCatalog = "master"
        };

        return builder.ConnectionString;
    }

    private void EnsureDatabaseSelection()
    {
        if (!string.IsNullOrWhiteSpace(_databaseComboBox.Text))
        {
            return;
        }

        MessageBox.Show(this, "Lütfen bir veri tabanı seçin.", "Eksik seçim",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        DialogResult = DialogResult.None;
    }

    private void PrefillFromConfiguration()
    {
        var configuredConnection = ConfigurationManager.ConnectionStrings["BankServiceDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            return;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(configuredConnection);
            _dataSourceTextBox.Text = builder.DataSource;
            _userIdTextBox.Text = builder.UserID;
            _passwordTextBox.Text = builder.Password;
            if (!string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                _databaseComboBox.Items.Add(builder.InitialCatalog);
                _databaseComboBox.SelectedIndex = 0;
                _databaseComboBox.Enabled = true;
                _okButton.Enabled = true;
            }
        }
        catch
        {
            // Ignore malformed configuration entries; user can type values manually.
        }
    }
}

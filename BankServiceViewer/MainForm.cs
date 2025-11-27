using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BankServiceViewer.Data;

namespace BankServiceViewer;

public partial class MainForm : Form
{
    private readonly ISqlSettingsRepository _repository;
    private bool _connectionEstablished;
    private bool HasSelectionColumn => _settingsGrid.Columns.Contains(SelectionColumnName);
    private static readonly string[] VisibleColumns =
    {
        "New_BankServiceSettingsId",
        "new_FirmIdName",
        "OwningBusinessUnitName",
        "new_BankIdName",
        "BankServiceSettinName",
        "new_ModuleIdName",
        "new_UserName",
        "new_Password",
        "new_new_EncryptPassword",
        "new_SecureToken",
        "new_FirmCode",
        "new_PrivateKey",
        "new_RefreshToken",
        "new_RefreshTokenDate"
    };

    private const string SelectionColumnName = "SelectRow";

    public MainForm()
    {
        InitializeComponent();
        _repository = new SqlSettingsRepository();
        Load += async (_, _) => await InitializeAndLoadAsync();
    }

    private async Task InitializeAndLoadAsync()
    {
        if (!TryEstablishConnection())
        {
            Close();
            return;
        }

        await RefreshDataAsync();
    }

    private bool TryEstablishConnection()
    {
        using var dialog = new ConnectionDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _repository.UpdateConnectionString(dialog.ConnectionString);
        _connectionEstablished = true;
        return true;
    }

    private async Task RefreshDataAsync()
    {
        if (!_connectionEstablished)
        {
            MessageBox.Show(this, "Lütfen önce veri tabanı bağlantısını yapın.", "Bağlantı yok", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        ToggleButtonsWhileLoading(true);
        try
        {
            DataTable settings = await _repository.LoadSettingsAsync();
            AddEncryptedPasswords(settings);
            _settingsGrid.DataSource = settings;
            ConfigureGridColumns();
            ApplyRowSpacing();
            ResetSelections();
            UpdateButtonState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Veri yüklenirken hata oluştu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleButtonsWhileLoading(false);
        }
    }

    private static void AddEncryptedPasswords(DataTable settings)
    {
        const string encryptedColumnName = "new_new_EncryptPassword";
        if (!settings.Columns.Contains(encryptedColumnName))
        {
            var encryptedColumn = settings.Columns.Add(encryptedColumnName, typeof(string));
            encryptedColumn.ReadOnly = true;
        }

        foreach (DataRow row in settings.Rows)
        {
            string password = row["new_Password"]?.ToString() ?? string.Empty;
            row[encryptedColumnName] = Encrypt.EncryptString(password);
        }
    }

    private void ToggleButtonsWhileLoading(bool isLoading)
    {
        _refreshButton.Enabled = !isLoading;
        _primaryActionButton.Enabled = !isLoading && HasCheckedRows();
        _secondaryActionButton.Enabled = !isLoading && HasCheckedRows();
        _selectAllButton.Enabled = !isLoading && _settingsGrid.Rows.Count > 0;
        Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
    }

    private void UpdateButtonState()
    {
        bool hasSelection = HasCheckedRows();
        _primaryActionButton.Enabled = hasSelection;
        _secondaryActionButton.Enabled = hasSelection;
        _selectAllButton.Enabled = _settingsGrid.Rows.Count > 0;
    }

    private void HandlePrimaryAction()
    {
        var selectedRows = GetCheckedDataRows().ToList();

        if (selectedRows.Count == 0)
        {
            MessageBox.Show(this, "Lütfen en az bir satır seçin.", "Seçim yok", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string message = "Seçilen satırlar için işleminizi burada uygulayabilirsiniz.\n\n" +
                         string.Join("\n", selectedRows.Select(r => r[0]?.ToString() ?? "<boş>"));
        MessageBox.Show(this, message, "İşlem Önizleme", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void HandleSecondaryAction()
    {
        var selectedRow = GetCheckedDataRows().FirstOrDefault();

        if (selectedRow is null)
        {
            MessageBox.Show(this, "Geçerli bir satır bulunamadı.", "Satır yok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var details = new SettingDetailsDialog(selectedRow);
        details.ShowDialog(this);
    }

    private void ConfigureGridColumns()
    {
        EnsureSelectionColumn();

        foreach (DataGridViewColumn column in _settingsGrid.Columns)
        {
            if (column.Name == SelectionColumnName)
            {
                column.DisplayIndex = 0;
                column.ReadOnly = false;
                continue;
            }

            string columnKey = column.DataPropertyName ?? column.Name;
            bool isVisible = VisibleColumns.Contains(columnKey, StringComparer.OrdinalIgnoreCase);
            column.Visible = isVisible;
            column.ReadOnly = true;

            if (isVisible)
            {
                column.DisplayIndex = GetDisplayIndex(columnKey) + 1;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                column.Width = Math.Max(column.Width, 150);
            }
        }
    }

    private void EnsureSelectionColumn()
    {
        if (_settingsGrid.Columns.Contains(SelectionColumnName))
        {
            return;
        }

        var selectionColumn = new DataGridViewCheckBoxColumn
        {
            Name = SelectionColumnName,
            HeaderText = string.Empty,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            ReadOnly = false,
            Frozen = true
        };

        _settingsGrid.Columns.Insert(0, selectionColumn);
    }

    private IEnumerable<DataRow> GetCheckedDataRows()
    {
        if (!HasSelectionColumn)
        {
            return Enumerable.Empty<DataRow>();
        }

        return _settingsGrid.Rows
            .Cast<DataGridViewRow>()
            .Where(IsRowChecked)
            .Select(row => row.DataBoundItem as DataRowView)
            .Where(view => view?.Row is not null)
            .Select(view => view!.Row);
    }

    private bool HasCheckedRows()
    {
        return HasSelectionColumn && _settingsGrid.Rows.Cast<DataGridViewRow>().Any(IsRowChecked);
    }

    private bool IsRowChecked(DataGridViewRow row)
    {
        return row.DataGridView?.Columns.Contains(SelectionColumnName) == true &&
               row.Cells[SelectionColumnName].Value is bool isChecked && isChecked;
    }

    private static int GetDisplayIndex(string columnKey)
    {
        int index = Array.FindIndex(VisibleColumns, column =>
            string.Equals(column, columnKey, StringComparison.OrdinalIgnoreCase));

        return index >= 0 ? index : VisibleColumns.Length;
    }

    private void ResetSelections()
    {
        if (!HasSelectionColumn)
        {
            return;
        }

        foreach (DataGridViewRow row in _settingsGrid.Rows)
        {
            row.Cells[SelectionColumnName].Value = false;
            row.Selected = false;
        }
    }

    private void SelectAllRows()
    {
        if (!HasSelectionColumn)
        {
            return;
        }

        foreach (DataGridViewRow row in _settingsGrid.Rows)
        {
            row.Cells[SelectionColumnName].Value = true;
            row.Selected = true;
        }

        UpdateButtonState();
    }

    private void ApplyRowSpacing()
    {
        foreach (DataGridViewRow row in _settingsGrid.Rows)
        {
            row.Height = _settingsGrid.RowTemplate.Height;
            row.DefaultCellStyle.Padding = _settingsGrid.RowTemplate.DefaultCellStyle.Padding;
        }
    }

    private void SettingsGridOnCurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (_settingsGrid.IsCurrentCellDirty && _settingsGrid.CurrentCell is DataGridViewCheckBoxCell)
        {
            _settingsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void SettingsGridOnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (_settingsGrid.Columns[e.ColumnIndex].Name == SelectionColumnName)
        {
            var row = _settingsGrid.Rows[e.RowIndex];
            row.Selected = IsRowChecked(row);
            UpdateButtonState();
        }
    }
}

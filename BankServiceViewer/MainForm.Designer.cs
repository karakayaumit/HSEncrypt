using System.Drawing;
using System.Windows.Forms;

namespace BankServiceViewer;

public partial class MainForm
{
    private DataGridView _settingsGrid = null!;
    private Button _refreshButton = null!;
    private Button _primaryActionButton = null!;
    private Button _secondaryActionButton = null!;
    private TableLayoutPanel _layout = null!;
    private FlowLayoutPanel _buttonPanel = null!;

    private void InitializeComponent()
    {
        _settingsGrid = new DataGridView();
        _refreshButton = new Button();
        _primaryActionButton = new Button();
        _secondaryActionButton = new Button();
        _layout = new TableLayoutPanel();
        _buttonPanel = new FlowLayoutPanel();
        SuspendLayout();

        _settingsGrid.AllowUserToAddRows = false;
        _settingsGrid.AllowUserToDeleteRows = false;
        _settingsGrid.AllowUserToOrderColumns = true;
        _settingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _settingsGrid.Dock = DockStyle.Fill;
        _settingsGrid.MultiSelect = true;
        _settingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _settingsGrid.RowHeadersVisible = false;
        _settingsGrid.SelectionChanged += (_, _) => UpdateButtonState();
        _settingsGrid.CurrentCellDirtyStateChanged += SettingsGridOnCurrentCellDirtyStateChanged;
        _settingsGrid.CellValueChanged += SettingsGridOnCellValueChanged;

        _refreshButton.Text = "Yenile";
        _refreshButton.AutoSize = true;
        _refreshButton.Margin = new Padding(5);
        _refreshButton.Click += async (_, _) => await RefreshDataAsync();

        _primaryActionButton.Text = "Seçilenleri İşle";
        _primaryActionButton.AutoSize = true;
        _primaryActionButton.Margin = new Padding(5);
        _primaryActionButton.Enabled = false;
        _primaryActionButton.Click += (_, _) => HandlePrimaryAction();

        _secondaryActionButton.Text = "Detay Göster";
        _secondaryActionButton.AutoSize = true;
        _secondaryActionButton.Margin = new Padding(5);
        _secondaryActionButton.Enabled = false;
        _secondaryActionButton.Click += (_, _) => HandleSecondaryAction();

        _buttonPanel.Dock = DockStyle.Fill;
        _buttonPanel.AutoSize = true;
        _buttonPanel.FlowDirection = FlowDirection.LeftToRight;
        _buttonPanel.Controls.Add(_refreshButton);
        _buttonPanel.Controls.Add(_primaryActionButton);
        _buttonPanel.Controls.Add(_secondaryActionButton);

        _layout.ColumnCount = 1;
        _layout.RowCount = 2;
        _layout.Dock = DockStyle.Fill;
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(_settingsGrid, 0, 0);
        _layout.Controls.Add(_buttonPanel, 0, 1);

        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Controls.Add(_layout);
        Text = "Bank Service Settings";

        ResumeLayout(false);
        PerformLayout();
    }
}

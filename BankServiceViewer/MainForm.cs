using System.Data;
using System.Linq;
using System.Windows.Forms;
using BankServiceViewer.Data;

namespace BankServiceViewer;

public partial class MainForm : Form
{
    private readonly ISqlSettingsRepository _repository;
    private bool _connectionEstablished;

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
            _settingsGrid.DataSource = settings;
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

    private void ToggleButtonsWhileLoading(bool isLoading)
    {
        _refreshButton.Enabled = !isLoading;
        _primaryActionButton.Enabled = !isLoading && _settingsGrid.SelectedRows.Count > 0;
        _secondaryActionButton.Enabled = !isLoading && _settingsGrid.SelectedRows.Count > 0;
        Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
    }

    private void UpdateButtonState()
    {
        bool hasSelection = _settingsGrid.SelectedRows.Count > 0;
        _primaryActionButton.Enabled = hasSelection;
        _secondaryActionButton.Enabled = hasSelection;
    }

    private void HandlePrimaryAction()
    {
        var selectedRows = _settingsGrid.SelectedRows
            .Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as DataRowView)
            .Where(view => view?.Row is not null)
            .Select(view => view!.Row)
            .ToList();

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
        if (_settingsGrid.CurrentRow?.DataBoundItem is not DataRowView view || view.Row is null)
        {
            MessageBox.Show(this, "Geçerli bir satır bulunamadı.", "Satır yok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var details = new SettingDetailsDialog(view.Row);
        details.ShowDialog(this);
    }
}

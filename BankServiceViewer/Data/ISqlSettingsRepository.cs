using System.Data;

namespace BankServiceViewer.Data;

public interface ISqlSettingsRepository
{
    void UpdateConnectionString(string connectionString);
    Task<DataTable> LoadSettingsAsync();
}

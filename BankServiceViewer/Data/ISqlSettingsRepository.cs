using System.Data;

namespace BankServiceViewer.Data;

public interface ISqlSettingsRepository
{
    Task<DataTable> LoadSettingsAsync();
}

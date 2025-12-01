using System.Collections.Generic;
using System.Data;

namespace BankServiceViewer.Data;

public interface ISqlSettingsRepository
{
    void UpdateConnectionString(string connectionString);
    Task<DataTable> LoadSettingsAsync();
    Task<int> UpdatePasswordsAsync(IEnumerable<(Guid Id, string EncryptedPassword)> updates);
}

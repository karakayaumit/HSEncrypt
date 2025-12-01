using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BankServiceViewer.Data;

public class SqlSettingsRepository : ISqlSettingsRepository
{
    private const string DefaultConnectionString =
        "Server=localhost;Database=YourDatabase;Integrated Security=True;TrustServerCertificate=True;";

    private readonly string _connectionString;
    private string _currentConnectionString;
    private bool _backupCreated;

    public SqlSettingsRepository()
    {
        _connectionString = ConfigurationManager.ConnectionStrings["BankServiceDb"]?.ConnectionString
                             ?? DefaultConnectionString;
        _currentConnectionString = _connectionString;
    }

    public void UpdateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Bağlantı cümlesi boş olamaz.", nameof(connectionString));
        }

        _currentConnectionString = connectionString;
    }

    public async Task<DataTable> LoadSettingsAsync()
    {
        const string query = "SELECT * FROM vNew_BankServiceSettings WHERE DeletionStateCode = 0";
        var table = new DataTable();

        using var connection = new SqlConnection(_currentConnectionString);
        await connection.OpenAsync();

        if (!_backupCreated)
        {
            await CreateBackupIfNeededAsync(connection);
            _backupCreated = true;
        }

        using var command = new SqlCommand(query, connection);
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(table);

        return table;
    }

    public async Task<int> UpdatePasswordsAsync(IEnumerable<(Guid Id, string EncryptedPassword)> updates)
    {
        if (updates is null)
        {
            throw new ArgumentNullException(nameof(updates));
        }

        var updatesList = updates.ToList();

        if (updatesList.Count == 0)
        {
            return 0;
        }

        const string updateQuery =
            "UPDATE vNew_BankServiceSettings SET new_Password = @Password WHERE New_BankServiceSettingsId = @Id";

        using var connection = new SqlConnection(_currentConnectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        int affectedRows = 0;

        try
        {
            foreach (var (id, encryptedPassword) in updatesList)
            {
                using var command = new SqlCommand(updateQuery, connection, transaction);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Password", encryptedPassword);

                affectedRows += await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return affectedRows;
    }

    private static async Task CreateBackupIfNeededAsync(SqlConnection connection)
    {
        string backupTableName = await GetAvailableBackupTableNameAsync(connection);

        string backupQuery = $"SELECT * INTO [{backupTableName}] FROM vNew_BankServiceSettings WHERE DeletionStateCode = 0";
        using var backupCommand = new SqlCommand(backupQuery, connection);
        await backupCommand.ExecuteNonQueryAsync();
    }

    private static async Task<string> GetAvailableBackupTableNameAsync(SqlConnection connection)
    {
        int attempt = 0;

        while (true)
        {
            string suffix = attempt switch
            {
                0 => string.Empty,
                1 => "2",
                2 => "3",
                _ => (attempt + 1).ToString()
            };

            string candidate = $"TEMP{suffix}_vNew_BankServiceSettings";

            if (!await TableExistsAsync(connection, candidate))
            {
                return candidate;
            }

            attempt++;
        }
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, string tableName)
    {
        const string existsQuery = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";

        using var command = new SqlCommand(existsQuery, connection);
        command.Parameters.AddWithValue("@tableName", tableName);

        return await command.ExecuteScalarAsync() is not null;
    }
}

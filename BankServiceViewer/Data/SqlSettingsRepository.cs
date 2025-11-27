using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BankServiceViewer.Data;

public class SqlSettingsRepository : ISqlSettingsRepository
{
    private const string DefaultConnectionString =
        "Server=localhost;Database=YourDatabase;Integrated Security=True;TrustServerCertificate=True;";

    private readonly string _connectionString;

    public SqlSettingsRepository()
    {
        _connectionString = ConfigurationManager.ConnectionStrings["BankServiceDb"]?.ConnectionString
                             ?? DefaultConnectionString;
    }

    public async Task<DataTable> LoadSettingsAsync()
    {
        const string query = "SELECT * FROM vNew_BankServiceSettings";
        var table = new DataTable();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        using var adapter = new SqlDataAdapter(command);

        await connection.OpenAsync();
        adapter.Fill(table);

        return table;
    }
}

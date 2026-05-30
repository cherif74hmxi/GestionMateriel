using Microsoft.Data.Sqlite;

namespace GestionMateriel.DAL;

public sealed class Connection
{
    private static readonly Lazy<Connection> Instance = new(() => new Connection());

    public static string DbChemin { get; } = Path.Combine(AppContext.BaseDirectory, "GestionMateriel.db");

    private readonly string _connectionString;

    private Connection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DbChemin,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        _connectionString = builder.ToString();
    }

    public static Connection GetInstance()
    {
        return Instance.Value;
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void CloseConnection(SqliteConnection? connection)
    {
        if (connection == null)
        {
            return;
        }

        if (connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }

        connection.Dispose();
    }
}

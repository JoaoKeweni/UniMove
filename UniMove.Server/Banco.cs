using Microsoft.Data.Sqlite;

namespace UniMove.Server;

public static class Banco
{
    private const string ConnectionString = "Data Source=unimove.db";

    public static readonly object Lock = new();

    public static void Inicializar()
    {
        using SqliteConnection conexao = Abrir();
        SqliteCommand cmd = conexao.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Nome  TEXT PRIMARY KEY,
                Senha TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Caronas (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                Motorista TEXT NOT NULL,
                Origem    TEXT NOT NULL,
                Destino   TEXT NOT NULL,
                Horario   TEXT NOT NULL,
                Vagas     INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Reservas (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                IdCarona   INTEGER NOT NULL,
                Passageiro TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Banco SQLite pronto (unimove.db).");
    }

    public static SqliteConnection Abrir()
    {
        var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();
        return conexao;
    }
}

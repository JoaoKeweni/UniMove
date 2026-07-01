using Microsoft.Data.Sqlite;

namespace UniMove.Server;

/// <summary>
/// Ponto único de acesso ao banco SQLite local (arquivo unimove.db).
/// Cria as tabelas na inicialização e fornece conexões abertas.
/// </summary>
public static class Banco
{
    private const string ConnectionString = "Data Source=unimove.db";

    /// <summary>Trava global de escrita — serializa o acesso ao banco entre os serviços.</summary>
    public static readonly object Lock = new();

    /// <summary>Cria o arquivo do banco e as tabelas caso ainda não existam.</summary>
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

    /// <summary>Abre e retorna uma nova conexão com o banco.</summary>
    public static SqliteConnection Abrir()
    {
        var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();
        return conexao;
    }
}

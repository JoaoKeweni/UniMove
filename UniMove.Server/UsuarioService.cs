using Microsoft.Data.Sqlite;

namespace UniMove.Server;

public class UsuarioService
{
    public (bool Sucesso, string Mensagem) Registrar(string nome, string senha)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(senha))
            return (false, "Informe nome e senha.");

        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();

            SqliteCommand existe = conexao.CreateCommand();
            existe.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE Nome = $nome";
            existe.Parameters.AddWithValue("$nome", nome);
            if (Convert.ToInt64(existe.ExecuteScalar()) > 0)
                return (false, "Já existe um usuário com esse nome.");

            SqliteCommand inserir = conexao.CreateCommand();
            inserir.CommandText = "INSERT INTO Usuarios (Nome, Senha) VALUES ($nome, $senha)";
            inserir.Parameters.AddWithValue("$nome", nome);
            inserir.Parameters.AddWithValue("$senha", senha);
            inserir.ExecuteNonQuery();

            return (true, "Usuário registrado com sucesso! Faça o login.");
        }
    }

    public (bool Sucesso, string Mensagem) Login(string nome, string senha)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(senha))
            return (false, "Informe nome e senha.");

        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();

            SqliteCommand cmd = conexao.CreateCommand();
            cmd.CommandText = "SELECT Senha FROM Usuarios WHERE Nome = $nome";
            cmd.Parameters.AddWithValue("$nome", nome);

            if (cmd.ExecuteScalar() is not string senhaArmazenada)
                return (false, "Usuário não encontrado.");

            return senhaArmazenada == senha
                ? (true, "Login realizado com sucesso.")
                : (false, "Senha incorreta.");
        }
    }
}

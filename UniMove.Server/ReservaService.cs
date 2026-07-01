using Microsoft.Data.Sqlite;
using UniMove.Shared;

namespace UniMove.Server;

/// <summary>
/// Regra de negócio das reservas, persistida no SQLite.
/// A reserva (checagem de vaga + decremento + registro) é feita numa única
/// transação, protegida por <see cref="Banco.Lock"/>.
/// </summary>
public class ReservaService
{
    /// <summary>
    /// Tenta registrar uma reserva. Retorna sucesso e uma mensagem descritiva.
    /// </summary>
    public (bool Sucesso, string Mensagem) Reservar(Reserva reserva)
    {
        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();
            using SqliteTransaction transacao = conexao.BeginTransaction();

            SqliteCommand consulta = conexao.CreateCommand();
            consulta.Transaction = transacao;
            consulta.CommandText = "SELECT Vagas FROM Caronas WHERE Id = $id";
            consulta.Parameters.AddWithValue("$id", reserva.IdCarona);

            object? valor = consulta.ExecuteScalar();
            if (valor is null)
                return (false, "Carona não encontrada.");

            if (Convert.ToInt32(valor) <= 0)
                return (false, "Não existem vagas disponíveis nesta carona.");

            SqliteCommand atualizar = conexao.CreateCommand();
            atualizar.Transaction = transacao;
            atualizar.CommandText = "UPDATE Caronas SET Vagas = Vagas - 1 WHERE Id = $id";
            atualizar.Parameters.AddWithValue("$id", reserva.IdCarona);
            atualizar.ExecuteNonQuery();

            SqliteCommand inserir = conexao.CreateCommand();
            inserir.Transaction = transacao;
            inserir.CommandText =
                "INSERT INTO Reservas (IdCarona, Passageiro) VALUES ($id, $p)";
            inserir.Parameters.AddWithValue("$id", reserva.IdCarona);
            inserir.Parameters.AddWithValue("$p", reserva.Passageiro);
            inserir.ExecuteNonQuery();

            transacao.Commit();
            return (true, $"Reserva confirmada para {reserva.Passageiro}.");
        }
    }

    /// <summary>Retorna os nomes dos passageiros que reservaram vaga na carona informada.</summary>
    public List<string> Passageiros(int idCarona)
    {
        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();
            SqliteCommand cmd = conexao.CreateCommand();
            cmd.CommandText =
                "SELECT Passageiro FROM Reservas WHERE IdCarona = $id ORDER BY Id";
            cmd.Parameters.AddWithValue("$id", idCarona);

            var passageiros = new List<string>();
            using SqliteDataReader leitor = cmd.ExecuteReader();
            while (leitor.Read())
                passageiros.Add(leitor.GetString(0));

            return passageiros;
        }
    }
}

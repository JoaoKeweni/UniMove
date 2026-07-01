using Microsoft.Data.Sqlite;
using UniMove.Shared;

namespace UniMove.Server;

/// <summary>Resultado de uma tentativa de reservar vaga em uma carona.</summary>
public enum ResultadoVaga
{
    Reservada,
    SemVagas,
    CaronaNaoEncontrada
}

/// <summary>
/// Regra de negócio das caronas, persistida no SQLite.
/// O acesso de escrita é serializado por <see cref="Banco.Lock"/>.
/// </summary>
public class CaronaService
{
    /// <summary>Publica uma nova carona e retorna com o Id gerado pelo banco.</summary>
    public Carona Publicar(Carona carona)
    {
        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();
            SqliteCommand cmd = conexao.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Caronas (Motorista, Origem, Destino, Horario, Vagas)
                VALUES ($m, $o, $d, $h, $v);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$m", carona.Motorista);
            cmd.Parameters.AddWithValue("$o", carona.Origem);
            cmd.Parameters.AddWithValue("$d", carona.Destino);
            cmd.Parameters.AddWithValue("$h", carona.Horario);
            cmd.Parameters.AddWithValue("$v", carona.Vagas);

            carona.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return carona;
        }
    }

    /// <summary>
    /// Retorna as caronas cadastradas. Se <paramref name="destino"/> for
    /// informado, filtra pelo campus de destino (independente da origem).
    /// Se <paramref name="apenasComVagas"/> for true, oculta caronas lotadas.
    /// </summary>
    public List<Carona> Listar(string? destino = null, bool apenasComVagas = false)
    {
        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();
            SqliteCommand cmd = conexao.CreateCommand();

            var condicoes = new List<string>();
            bool filtrarDestino = !string.IsNullOrWhiteSpace(destino);
            if (filtrarDestino)
            {
                condicoes.Add("Destino = $d");
                cmd.Parameters.AddWithValue("$d", destino!);
            }
            if (apenasComVagas)
                condicoes.Add("Vagas > 0");

            string where = condicoes.Count > 0 ? " WHERE " + string.Join(" AND ", condicoes) : string.Empty;
            cmd.CommandText =
                $"SELECT Id, Motorista, Origem, Destino, Horario, Vagas FROM Caronas{where} ORDER BY Id";

            var caronas = new List<Carona>();
            using SqliteDataReader leitor = cmd.ExecuteReader();
            while (leitor.Read())
            {
                caronas.Add(new Carona
                {
                    Id = leitor.GetInt32(0),
                    Motorista = leitor.GetString(1),
                    Origem = leitor.GetString(2),
                    Destino = leitor.GetString(3),
                    Horario = leitor.GetString(4),
                    Vagas = leitor.GetInt32(5)
                });
            }
            return caronas;
        }
    }

    /// <summary>Tenta reservar uma vaga, diminuindo o número de vagas de forma atômica.</summary>
    public ResultadoVaga ReservarVaga(int idCarona)
    {
        lock (Banco.Lock)
        {
            using SqliteConnection conexao = Banco.Abrir();

            SqliteCommand consulta = conexao.CreateCommand();
            consulta.CommandText = "SELECT Vagas FROM Caronas WHERE Id = $id";
            consulta.Parameters.AddWithValue("$id", idCarona);

            object? valor = consulta.ExecuteScalar();
            if (valor is null)
                return ResultadoVaga.CaronaNaoEncontrada;

            if (Convert.ToInt32(valor) <= 0)
                return ResultadoVaga.SemVagas;

            SqliteCommand atualizar = conexao.CreateCommand();
            atualizar.CommandText = "UPDATE Caronas SET Vagas = Vagas - 1 WHERE Id = $id";
            atualizar.Parameters.AddWithValue("$id", idCarona);
            atualizar.ExecuteNonQuery();

            return ResultadoVaga.Reservada;
        }
    }
}

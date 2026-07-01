namespace UniMove.Shared;

/// <summary>
/// Representa a reserva de uma vaga em uma carona.
/// </summary>
public class Reserva
{
    public int IdCarona { get; set; }
    public string Passageiro { get; set; } = string.Empty;
}

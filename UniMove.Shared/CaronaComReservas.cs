namespace UniMove.Shared;

/// <summary>
/// Agrupa uma carona com a lista de passageiros que reservaram vaga nela.
/// Usado pelo painel que mostra quem ofereceu e quem reservou cada carona.
/// </summary>
public class CaronaComReservas
{
    public Carona Carona { get; set; } = new();
    public List<string> Passageiros { get; set; } = new();
}

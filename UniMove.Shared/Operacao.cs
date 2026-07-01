namespace UniMove.Shared;

/// <summary>
/// Operações suportadas no protocolo de comunicação entre cliente e servidor.
/// </summary>
public enum Operacao
{
    Publicar,
    Buscar,
    Reservar,
    Resposta,
    Erro,
    Reservas,
    Registrar,
    Login
}

namespace UniMove.Shared;

/// <summary>
/// Credenciais simples de um usuário. Usado tanto no registro quanto no login.
/// (Sistema de demonstração: a senha é comparada em texto puro.)
/// </summary>
public class Usuario
{
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

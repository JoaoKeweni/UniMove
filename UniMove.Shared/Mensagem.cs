using System.Text.Json;

namespace UniMove.Shared;

public class Mensagem
{
    public Operacao Operacao { get; set; }
    public string Dados { get; set; } = string.Empty;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Mensagem() { }

    public Mensagem(Operacao operacao, string dados)
    {
        Operacao = operacao;
        Dados = dados;
    }

    public static Mensagem Criar<T>(Operacao operacao, T conteudo)
        => new(operacao, JsonSerializer.Serialize(conteudo, JsonOptions));

    public static Mensagem Texto(Operacao operacao, string texto)
        => new(operacao, texto);

    public T? ObterDados<T>()
        => JsonSerializer.Deserialize<T>(Dados, JsonOptions);

    public string Serializar()
        => JsonSerializer.Serialize(this, JsonOptions);

    public static Mensagem? Desserializar(string json)
        => JsonSerializer.Deserialize<Mensagem>(json, JsonOptions);
}

using System.Text.Json;

namespace UniMove.Shared;

/// <summary>
/// Envelope de comunicação trocado entre cliente e servidor.
/// O campo <see cref="Dados"/> carrega o payload já serializado em JSON.
/// </summary>
public class Mensagem
{
    public Operacao Operacao { get; set; }
    public string Dados { get; set; } = string.Empty;

    /// <summary>Opções de serialização compartilhadas por todo o projeto.</summary>
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

    /// <summary>Cria uma mensagem serializando o objeto informado no campo Dados.</summary>
    public static Mensagem Criar<T>(Operacao operacao, T conteudo)
        => new(operacao, JsonSerializer.Serialize(conteudo, JsonOptions));

    /// <summary>Cria uma mensagem com um texto simples como conteúdo.</summary>
    public static Mensagem Texto(Operacao operacao, string texto)
        => new(operacao, texto);

    /// <summary>Desserializa o campo Dados para o tipo informado.</summary>
    public T? ObterDados<T>()
        => JsonSerializer.Deserialize<T>(Dados, JsonOptions);

    /// <summary>Serializa a mensagem completa para JSON.</summary>
    public string Serializar()
        => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>Desserializa uma mensagem completa a partir de JSON.</summary>
    public static Mensagem? Desserializar(string json)
        => JsonSerializer.Deserialize<Mensagem>(json, JsonOptions);
}

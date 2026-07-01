using System.Net.Sockets;
using System.Text;
using UniMove.Shared;

namespace UniMove.Client;

/// <summary>
/// Cliente TCP responsável por enviar mensagens ao servidor e ler a resposta.
/// Utiliza uma conexão por requisição (padrão request/response), o que mantém
/// o código simples e adequado para a demonstração.
/// </summary>
public class SocketCliente
{
    public string Host { get; set; } = "127.0.0.1";
    public int Porta { get; set; } = 5000;

    /// <summary>Envia uma mensagem e retorna a resposta do servidor.</summary>
    public Mensagem Enviar(Mensagem mensagem)
    {
        using var cliente = new TcpClient();
        cliente.Connect(Host, Porta);

        using NetworkStream stream = cliente.GetStream();
        using var leitor = new StreamReader(stream, Encoding.UTF8);
        using var escritor = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

        escritor.WriteLine(mensagem.Serializar());

        string? resposta = leitor.ReadLine();
        if (string.IsNullOrWhiteSpace(resposta))
            return Mensagem.Texto(Operacao.Erro, "Sem resposta do servidor.");

        return Mensagem.Desserializar(resposta)
               ?? Mensagem.Texto(Operacao.Erro, "Resposta inválida do servidor.");
    }
}

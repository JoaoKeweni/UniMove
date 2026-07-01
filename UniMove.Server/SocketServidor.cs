using System.Net;
using System.Net.Sockets;

namespace UniMove.Server;

/// <summary>
/// Servidor TCP que aceita múltiplos clientes simultaneamente.
/// Cada conexão é atendida por um <see cref="ClienteHandler"/> em uma Task própria.
/// </summary>
public class SocketServidor
{
    private readonly TcpListener _listener;
    private readonly CaronaService _caronaService = new();
    private readonly ReservaService _reservaService = new();
    private readonly UsuarioService _usuarioService = new();

    public SocketServidor(int porta)
    {
        _listener = new TcpListener(IPAddress.Any, porta);
    }

    /// <summary>Inicia o loop de aceitação de conexões (bloqueante).</summary>
    public async Task IniciarAsync()
    {
        Banco.Inicializar();
        _listener.Start();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Servidor iniciado.");
        Console.WriteLine($"Aguardando conexões na porta {((IPEndPoint)_listener.LocalEndpoint).Port}...");

        while (true)
        {
            TcpClient cliente = await _listener.AcceptTcpClientAsync();
            var handler = new ClienteHandler(cliente, _caronaService, _reservaService, _usuarioService);
            _ = Task.Run(handler.Atender);
        }
    }
}

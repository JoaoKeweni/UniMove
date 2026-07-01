using System.Net.Sockets;
using System.Text;
using UniMove.Shared;

namespace UniMove.Server;

/// <summary>
/// Atende um único cliente conectado, lendo mensagens JSON delimitadas por
/// quebra de linha e delegando cada operação para os serviços de negócio.
/// </summary>
public class ClienteHandler
{
    private readonly TcpClient _cliente;
    private readonly CaronaService _caronaService;
    private readonly ReservaService _reservaService;
    private readonly UsuarioService _usuarioService;
    private readonly string _endereco;

    public ClienteHandler(TcpClient cliente, CaronaService caronaService,
                          ReservaService reservaService, UsuarioService usuarioService)
    {
        _cliente = cliente;
        _caronaService = caronaService;
        _reservaService = reservaService;
        _usuarioService = usuarioService;
        _endereco = cliente.Client.RemoteEndPoint?.ToString() ?? "desconhecido";
    }

    /// <summary>Loop principal de atendimento do cliente.</summary>
    public void Atender()
    {
        Log($"Cliente conectado. ({_endereco})");

        try
        {
            using NetworkStream stream = _cliente.GetStream();
            using var leitor = new StreamReader(stream, Encoding.UTF8);
            using var escritor = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            string? linha;
            while ((linha = leitor.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                Mensagem resposta = ProcessarLinha(linha);
                escritor.WriteLine(resposta.Serializar());
            }
        }
        catch (IOException)
        {
            // Conexão encerrada abruptamente pelo cliente.
        }
        catch (Exception ex)
        {
            Log($"Erro ao atender cliente: {ex.Message}");
        }
        finally
        {
            _cliente.Close();
            Log($"Cliente desconectado. ({_endereco})");
        }
    }

    private Mensagem ProcessarLinha(string linha)
    {
        try
        {
            Mensagem? mensagem = Mensagem.Desserializar(linha);
            if (mensagem is null)
                return Mensagem.Texto(Operacao.Erro, "Mensagem inválida.");

            return mensagem.Operacao switch
            {
                Operacao.Publicar => TratarPublicar(mensagem),
                Operacao.Buscar => TratarBuscar(mensagem),
                Operacao.Reservar => TratarReservar(mensagem),
                Operacao.Reservas => TratarReservas(),
                Operacao.Registrar => TratarRegistrar(mensagem),
                Operacao.Login => TratarLogin(mensagem),
                _ => Mensagem.Texto(Operacao.Erro, "Operação não suportada.")
            };
        }
        catch (Exception ex)
        {
            Log($"Erro ao processar mensagem: {ex.Message}");
            return Mensagem.Texto(Operacao.Erro, "Erro ao processar a solicitação.");
        }
    }

    private Mensagem TratarPublicar(Mensagem mensagem)
    {
        Log("Publicando carona...");
        Carona? carona = mensagem.ObterDados<Carona>();
        if (carona is null || string.IsNullOrWhiteSpace(carona.Motorista))
            return Mensagem.Texto(Operacao.Erro, "Dados da carona inválidos.");

        if (carona.Vagas <= 0)
            return Mensagem.Texto(Operacao.Erro, "A carona deve ter ao menos uma vaga.");

        Carona publicada = _caronaService.Publicar(carona);
        Log($"Carona #{publicada.Id} publicada por {publicada.Motorista} ({publicada.Origem} -> {publicada.Destino}).");
        return Mensagem.Texto(Operacao.Resposta, $"Carona publicada com sucesso! (#{publicada.Id})");
    }

    private Mensagem TratarBuscar(Mensagem mensagem)
    {
        string destino = mensagem.Dados?.Trim() ?? string.Empty;
        Log(string.IsNullOrEmpty(destino)
            ? "Buscando caronas (todos os campi)..."
            : $"Buscando caronas para '{destino}'...");

        List<Carona> caronas = _caronaService.Listar(destino);
        return Mensagem.Criar(Operacao.Resposta, caronas);
    }

    private Mensagem TratarReservas()
    {
        Log("Listando reservas (painel motorista/passageiros)...");
        List<CaronaComReservas> painel = _caronaService.Listar()
            .Select(c => new CaronaComReservas
            {
                Carona = c,
                Passageiros = _reservaService.Passageiros(c.Id)
            })
            .ToList();

        return Mensagem.Criar(Operacao.Resposta, painel);
    }

    private Mensagem TratarRegistrar(Mensagem mensagem)
    {
        Usuario? usuario = mensagem.ObterDados<Usuario>();
        if (usuario is null)
            return Mensagem.Texto(Operacao.Erro, "Dados de registro inválidos.");

        Log($"Registrando usuário '{usuario.Nome}'...");
        (bool sucesso, string texto) = _usuarioService.Registrar(usuario.Nome, usuario.Senha);
        return Mensagem.Texto(sucesso ? Operacao.Resposta : Operacao.Erro, texto);
    }

    private Mensagem TratarLogin(Mensagem mensagem)
    {
        Usuario? usuario = mensagem.ObterDados<Usuario>();
        if (usuario is null)
            return Mensagem.Texto(Operacao.Erro, "Dados de login inválidos.");

        Log($"Login do usuário '{usuario.Nome}'...");
        (bool sucesso, string texto) = _usuarioService.Login(usuario.Nome, usuario.Senha);
        return Mensagem.Texto(sucesso ? Operacao.Resposta : Operacao.Erro, texto);
    }

    private Mensagem TratarReservar(Mensagem mensagem)
    {
        Reserva? reserva = mensagem.ObterDados<Reserva>();
        if (reserva is null || string.IsNullOrWhiteSpace(reserva.Passageiro))
            return Mensagem.Texto(Operacao.Erro, "Dados da reserva inválidos.");

        (bool sucesso, string texto) = _reservaService.Reservar(reserva);

        if (sucesso)
        {
            Log($"Reserva realizada. Carona #{reserva.IdCarona} para {reserva.Passageiro}.");
            return Mensagem.Texto(Operacao.Resposta, texto);
        }

        Log($"Reserva recusada. Carona #{reserva.IdCarona}: {texto}");
        return Mensagem.Texto(Operacao.Erro, texto);
    }

    private static void Log(string texto)
        => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {texto}");
}

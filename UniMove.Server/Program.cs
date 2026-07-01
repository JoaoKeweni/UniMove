using UniMove.Server;

const int Porta = 5000;

Console.Title = "UniMove - Servidor";
Console.WriteLine("=== UniMove :: Servidor de Caronas ===");

var servidor = new SocketServidor(Porta);

try
{
    await servidor.IniciarAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Falha no servidor: {ex.Message}");
}

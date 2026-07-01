namespace UniMove.Client;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var cliente = new SocketCliente();

        using var login = new TelaLogin(cliente);
        if (login.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new TelaPrincipal(cliente, login.NomeUsuario));
        }
    }
}

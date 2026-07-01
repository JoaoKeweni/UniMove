namespace UniMove.Client;

/// <summary>Tela inicial: título UniMove e os dois botões principais.</summary>
public class TelaPrincipal : Form
{
    private readonly SocketCliente _cliente;
    private readonly string _nomeUsuario;

    public TelaPrincipal(SocketCliente cliente, string nomeUsuario)
    {
        _cliente = cliente;
        _nomeUsuario = nomeUsuario;
        ConfigurarJanela();
        ConstruirLayout();
    }

    private void ConfigurarJanela()
    {
        Text = "UniMove";
        BackColor = Ui.Fundo;
        Size = new Size(560, 540);
        MinimumSize = new Size(480, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = Ui.FonteCampo;
    }

    private void ConstruirLayout()
    {
        Panel barra = CriarBarraSuperior();
        Controls.Add(barra);

        var subtitulo = new Label
        {
            Text = $"Olá, {_nomeUsuario}! Caronas universitárias, simples e distribuídas.",
            Font = Ui.FonteRotulo,
            ForeColor = Ui.Cinza,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 60
        };
        Controls.Add(subtitulo);
        subtitulo.BringToFront();

        Button btnPublicar = Ui.BotaoArredondado("🚗  Publicar Carona", Ui.Azul, 320, 70);
        Button btnBuscar = Ui.BotaoArredondado("🔎  Buscar Caronas", Ui.Verde, 320, 70);
        Button btnReservas = Ui.BotaoArredondado("📋  Painel de Reservas", Ui.AzulEscuro, 320, 70);

        btnPublicar.Click += (_, _) => new TelaPublicar(_cliente, _nomeUsuario).ShowDialog(this);
        btnBuscar.Click += (_, _) => new TelaBuscar(_cliente, _nomeUsuario).ShowDialog(this);
        btnReservas.Click += (_, _) => new TelaReservas(_cliente, _nomeUsuario).ShowDialog(this);

        var painel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 30, 0, 0),
            AutoScroll = true
        };
        painel.Controls.Add(EnvolverCentralizado(btnPublicar, painel));
        painel.Controls.Add(EnvolverCentralizado(btnBuscar, painel));
        painel.Controls.Add(EnvolverCentralizado(btnReservas, painel));
        Controls.Add(painel);
        painel.BringToFront();
    }

    private Panel CriarBarraSuperior()
    {
        var barra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Ui.Azul
        };

        var titulo = new Label
        {
            Text = "UniMove",
            Font = Ui.FonteTitulo,
            ForeColor = Color.White,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        barra.Controls.Add(titulo);
        return barra;
    }

    private static Panel EnvolverCentralizado(Control controle, Control pai)
    {
        var wrapper = new Panel
        {
            Width = pai.ClientSize.Width,
            Height = controle.Height + 20,
            Margin = new Padding(0)
        };
        controle.Location = new Point((wrapper.Width - controle.Width) / 2, 10);
        controle.Anchor = AnchorStyles.Top;
        wrapper.Controls.Add(controle);
        pai.SizeChanged += (_, _) =>
        {
            wrapper.Width = pai.ClientSize.Width;
            controle.Location = new Point((wrapper.Width - controle.Width) / 2, 10);
        };
        return wrapper;
    }
}

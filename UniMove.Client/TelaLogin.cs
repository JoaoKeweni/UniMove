using UniMove.Shared;

namespace UniMove.Client;

/// <summary>
/// Tela de entrada: login e registro simples (nome + senha).
/// Em caso de login bem-sucedido, define <see cref="NomeUsuario"/> e fecha
/// com DialogResult.OK para que o programa abra a tela principal.
/// </summary>
public class TelaLogin : Form
{
    private readonly SocketCliente _cliente;

    private readonly TextBox _txtNome = new()
    {
        Font = Ui.FonteCampo,
        Width = 300,
        BorderStyle = BorderStyle.FixedSingle
    };

    private readonly TextBox _txtSenha = new()
    {
        Font = Ui.FonteCampo,
        Width = 300,
        BorderStyle = BorderStyle.FixedSingle,
        UseSystemPasswordChar = true
    };

    /// <summary>Nome do usuário autenticado (válido após login com sucesso).</summary>
    public string NomeUsuario { get; private set; } = string.Empty;

    public TelaLogin(SocketCliente cliente)
    {
        _cliente = cliente;
        ConfigurarJanela();
        ConstruirLayout();
    }

    private void ConfigurarJanela()
    {
        Text = "UniMove - Acesso";
        BackColor = Ui.Fundo;
        Size = new Size(440, 460);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = Ui.FonteCampo;
    }

    private void ConstruirLayout()
    {
        Controls.Add(CriarBarra());

        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 30, 40, 20),
            ColumnCount = 1
        };
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        painel.Controls.Add(Rotulo("Nome"));
        painel.Controls.Add(_txtNome);
        painel.Controls.Add(Rotulo("Senha"));
        painel.Controls.Add(_txtSenha);

        Button btnEntrar = Ui.BotaoArredondado("Entrar", Ui.Azul, 300, 50);
        btnEntrar.Margin = new Padding(0, 30, 0, 0);
        btnEntrar.Click += (_, _) => Entrar();

        Button btnRegistrar = Ui.BotaoArredondado("Criar conta", Ui.Verde, 300, 46);
        btnRegistrar.Margin = new Padding(0, 12, 0, 0);
        btnRegistrar.Click += (_, _) => Registrar();

        painel.Controls.Add(btnEntrar);
        painel.Controls.Add(btnRegistrar);

        Controls.Add(painel);
        painel.BringToFront();

        AcceptButton = btnEntrar;
    }

    private void Entrar()
    {
        Usuario? credenciais = LerCredenciais();
        if (credenciais is null) return;

        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Criar(Operacao.Login, credenciais));
            if (resposta.Operacao == Operacao.Resposta)
            {
                NomeUsuario = credenciais.Nome;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Aviso(resposta.Dados);
            }
        }
        catch (Exception ex)
        {
            Aviso($"Não foi possível conectar ao servidor.\n{ex.Message}");
        }
    }

    private void Registrar()
    {
        Usuario? credenciais = LerCredenciais();
        if (credenciais is null) return;

        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Criar(Operacao.Registrar, credenciais));
            if (resposta.Operacao == Operacao.Resposta)
                Sucesso(resposta.Dados);
            else
                Aviso(resposta.Dados);
        }
        catch (Exception ex)
        {
            Aviso($"Não foi possível conectar ao servidor.\n{ex.Message}");
        }
    }

    private Usuario? LerCredenciais()
    {
        if (string.IsNullOrWhiteSpace(_txtNome.Text) || string.IsNullOrWhiteSpace(_txtSenha.Text))
        {
            Aviso("Informe nome e senha.");
            return null;
        }

        return new Usuario
        {
            Nome = _txtNome.Text.Trim(),
            Senha = _txtSenha.Text
        };
    }

    private Panel CriarBarra()
    {
        var barra = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Ui.Azul };
        barra.Controls.Add(new Label
        {
            Text = "UniMove",
            Font = Ui.FonteTitulo,
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
        return barra;
    }

    private static Label Rotulo(string texto) => new()
    {
        Text = texto,
        Font = Ui.FonteRotulo,
        ForeColor = Ui.Texto,
        AutoSize = true,
        Margin = new Padding(0, 14, 0, 4)
    };

    private void Sucesso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void Aviso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

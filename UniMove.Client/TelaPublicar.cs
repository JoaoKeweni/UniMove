using UniMove.Shared;

namespace UniMove.Client;

public class TelaPublicar : Form
{
    private readonly SocketCliente _cliente;

    private readonly TextBox _txtMotorista = CriarCampo();
    private readonly TextBox _txtOrigem = CriarCampo();
    private readonly ComboBox _cbDestino = new()
    {
        Font = Ui.FonteCampo,
        Width = 380,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly TextBox _txtHorario = CriarCampo();
    private readonly NumericUpDown _numVagas = new()
    {
        Minimum = 1,
        Maximum = 20,
        Value = 1,
        Font = Ui.FonteCampo,
        Width = 80
    };

    public TelaPublicar(SocketCliente cliente, string nomeUsuario)
    {
        _cliente = cliente;
        _txtMotorista.Text = nomeUsuario;
        _cbDestino.Items.AddRange(Campi.Todos);
        ConfigurarJanela();
        ConstruirLayout();
    }

    private void ConfigurarJanela()
    {
        Text = "UniMove - Publicar Carona";
        BackColor = Ui.Fundo;
        Size = new Size(480, 560);
        MinimumSize = new Size(440, 540);
        StartPosition = FormStartPosition.CenterParent;
        Font = Ui.FonteCampo;
    }

    private void ConstruirLayout()
    {
        Controls.Add(CriarBarra("Publicar Carona"));

        var tabela = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 30, 30, 20),
            ColumnCount = 1,
            AutoScroll = true
        };
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        tabela.Controls.Add(Rotulo("Nome do motorista"));
        tabela.Controls.Add(_txtMotorista);
        tabela.Controls.Add(Rotulo("Origem (de onde você sai)"));
        tabela.Controls.Add(_txtOrigem);
        tabela.Controls.Add(Rotulo("Campus de destino"));
        tabela.Controls.Add(_cbDestino);
        tabela.Controls.Add(Rotulo("Horário"));
        tabela.Controls.Add(_txtHorario);
        tabela.Controls.Add(Rotulo("Número de vagas"));
        tabela.Controls.Add(_numVagas);

        Button btnPublicar = Ui.BotaoArredondado("Publicar Carona", Ui.Azul, 260, 55);
        btnPublicar.Margin = new Padding(0, 25, 0, 0);
        btnPublicar.Click += (_, _) => Publicar();
        tabela.Controls.Add(btnPublicar);

        Controls.Add(tabela);
        tabela.BringToFront();
    }

    private void Publicar()
    {
        if (string.IsNullOrWhiteSpace(_txtMotorista.Text) ||
            string.IsNullOrWhiteSpace(_txtOrigem.Text) ||
            _cbDestino.SelectedItem is null ||
            string.IsNullOrWhiteSpace(_txtHorario.Text))
        {
            Aviso("Preencha todos os campos e escolha o campus de destino.");
            return;
        }

        var carona = new Carona
        {
            Motorista = _txtMotorista.Text.Trim(),
            Origem = _txtOrigem.Text.Trim(),
            Destino = _cbDestino.SelectedItem!.ToString()!,
            Horario = _txtHorario.Text.Trim(),
            Vagas = (int)_numVagas.Value
        };

        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Criar(Operacao.Publicar, carona));
            if (resposta.Operacao == Operacao.Resposta)
            {
                Sucesso(resposta.Dados);
                LimparCampos();
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

    private void LimparCampos()
    {
        _txtOrigem.Clear();
        _cbDestino.SelectedIndex = -1;
        _txtHorario.Clear();
        _numVagas.Value = 1;
    }

    private Panel CriarBarra(string texto)
    {
        var barra = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Ui.Azul };
        barra.Controls.Add(new Label
        {
            Text = texto,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
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
        Margin = new Padding(0, 12, 0, 4)
    };

    private static TextBox CriarCampo() => new()
    {
        Font = Ui.FonteCampo,
        Width = 380,
        BorderStyle = BorderStyle.FixedSingle
    };

    private void Sucesso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void Aviso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

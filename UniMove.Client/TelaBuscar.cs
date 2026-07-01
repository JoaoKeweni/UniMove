using UniMove.Shared;

namespace UniMove.Client;

/// <summary>
/// Tela para buscar caronas por campus de destino e reservar uma vaga.
/// A busca é feita pelo local aonde o passageiro quer chegar, independente
/// da origem do motorista.
/// </summary>
public class TelaBuscar : Form
{
    private const string TodosOsCampi = "Todos os campi";

    private readonly SocketCliente _cliente;
    private readonly DataGridView _grade = new();
    private readonly ComboBox _cbDestino = new()
    {
        Font = Ui.FonteCampo,
        Width = 320,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly TextBox _txtPassageiro = new()
    {
        Font = Ui.FonteCampo,
        Width = 260,
        BorderStyle = BorderStyle.FixedSingle
    };

    public TelaBuscar(SocketCliente cliente, string nomeUsuario)
    {
        _cliente = cliente;
        _txtPassageiro.Text = nomeUsuario;

        _cbDestino.Items.Add(TodosOsCampi);
        _cbDestino.Items.AddRange(Campi.Todos);
        _cbDestino.SelectedIndex = 0;

        ConfigurarJanela();
        ConstruirLayout();
        Load += (_, _) => AtualizarLista();
    }

    private void ConfigurarJanela()
    {
        Text = "UniMove - Buscar Caronas";
        BackColor = Ui.Fundo;
        Size = new Size(780, 600);
        MinimumSize = new Size(660, 520);
        StartPosition = FormStartPosition.CenterParent;
        Font = Ui.FonteCampo;
    }

    private void ConstruirLayout()
    {
        Panel filtro = CriarFiltro();
        ConfigurarGrade();

        var rodape = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Ui.Fundo };

        var lbl = new Label
        {
            Text = "Nome do passageiro",
            Font = Ui.FonteRotulo,
            ForeColor = Ui.Texto,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        _txtPassageiro.Location = new Point(20, 40);

        Button btnReservar = Ui.BotaoArredondado("Reservar", Ui.Verde, 160, 48);
        btnReservar.Location = new Point(300, 34);
        btnReservar.Click += (_, _) => Reservar();

        Button btnAtualizar = Ui.BotaoArredondado("Atualizar Lista", Ui.Azul, 160, 48);
        btnAtualizar.Location = new Point(480, 34);
        btnAtualizar.Click += (_, _) => AtualizarLista();

        rodape.Controls.AddRange(new Control[] { lbl, _txtPassageiro, btnReservar, btnAtualizar });

        // Área central: o grid preenche o restante e o filtro fica no topo dela.
        var centro = new Panel { Dock = DockStyle.Fill };
        var painelGrade = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        painelGrade.Controls.Add(_grade);
        centro.Controls.Add(painelGrade);
        centro.Controls.Add(filtro);

        // Ordem no formulário: topo (barra), base (rodapé) e centro preenchendo.
        Controls.Add(centro);
        Controls.Add(rodape);
        Controls.Add(CriarBarra("Buscar Caronas"));
    }

    private Panel CriarFiltro()
    {
        var painel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Ui.Fundo };

        var lbl = new Label
        {
            Text = "Quero ir para:",
            Font = Ui.FonteRotulo,
            ForeColor = Ui.Texto,
            AutoSize = true,
            Location = new Point(20, 25)
        };
        _cbDestino.Location = new Point(130, 21);
        _cbDestino.SelectedIndexChanged += (_, _) => AtualizarLista();

        Button btnBuscar = Ui.BotaoArredondado("Buscar", Ui.Azul, 120, 40);
        btnBuscar.Location = new Point(470, 18);
        btnBuscar.Click += (_, _) => AtualizarLista();

        painel.Controls.AddRange(new Control[] { lbl, _cbDestino, btnBuscar });
        return painel;
    }

    private void ConfigurarGrade()
    {
        _grade.Dock = DockStyle.Fill;
        _grade.ReadOnly = true;
        _grade.AllowUserToAddRows = false;
        _grade.AllowUserToDeleteRows = false;
        _grade.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grade.MultiSelect = false;
        _grade.RowHeadersVisible = false;
        _grade.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grade.BackgroundColor = Color.White;
        _grade.BorderStyle = BorderStyle.None;
        _grade.EnableHeadersVisualStyles = false;
        _grade.ColumnHeadersDefaultCellStyle.BackColor = Ui.Azul;
        _grade.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grade.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _grade.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grade.ColumnHeadersHeight = 40;
        _grade.RowTemplate.Height = 34;

        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", Visible = false });
        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Motorista", HeaderText = "Motorista" });
        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Origem", HeaderText = "Origem (saída)" });
        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Destino", HeaderText = "Campus destino" });
        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Horario", HeaderText = "Horário" });
        _grade.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vagas", HeaderText = "Vagas" });
    }

    private string DestinoSelecionado()
    {
        string? escolha = _cbDestino.SelectedItem?.ToString();
        return string.IsNullOrEmpty(escolha) || escolha == TodosOsCampi
            ? string.Empty
            : escolha;
    }

    private void AtualizarLista()
    {
        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Texto(Operacao.Buscar, DestinoSelecionado()));
            if (resposta.Operacao != Operacao.Resposta)
            {
                Aviso(resposta.Dados);
                return;
            }

            List<Carona> caronas = resposta.ObterDados<List<Carona>>() ?? new List<Carona>();
            _grade.Rows.Clear();
            foreach (Carona c in caronas)
                _grade.Rows.Add(c.Id, c.Motorista, c.Origem, c.Destino, c.Horario, c.Vagas);
        }
        catch (Exception ex)
        {
            Aviso($"Não foi possível conectar ao servidor.\n{ex.Message}");
        }
    }

    private void Reservar()
    {
        if (_grade.CurrentRow is null)
        {
            Aviso("Selecione uma carona na lista.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtPassageiro.Text))
        {
            Aviso("Informe o nome do passageiro.");
            return;
        }

        int idCarona = Convert.ToInt32(_grade.CurrentRow.Cells["Id"].Value);
        var reserva = new Reserva
        {
            IdCarona = idCarona,
            Passageiro = _txtPassageiro.Text.Trim()
        };

        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Criar(Operacao.Reservar, reserva));
            if (resposta.Operacao == Operacao.Resposta)
            {
                Sucesso(resposta.Dados);
                AtualizarLista();
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

    private void Sucesso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void Aviso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

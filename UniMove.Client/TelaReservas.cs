using UniMove.Shared;

namespace UniMove.Client;

public class TelaReservas : Form
{
    private readonly SocketCliente _cliente;
    private readonly string _nomeUsuario;
    private readonly DataGridView _grade = new();

    public TelaReservas(SocketCliente cliente, string nomeUsuario)
    {
        _cliente = cliente;
        _nomeUsuario = nomeUsuario;
        ConfigurarJanela();
        ConstruirLayout();
        Load += (_, _) => Atualizar();
    }

    private void ConfigurarJanela()
    {
        Text = "UniMove - Painel de Reservas";
        BackColor = Ui.Fundo;
        Size = new Size(820, 560);
        MinimumSize = new Size(680, 480);
        StartPosition = FormStartPosition.CenterParent;
        Font = Ui.FonteCampo;
    }

    private void ConstruirLayout()
    {
        Controls.Add(CriarBarra("Painel de Reservas"));

        ConfigurarGrade();

        var rodape = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Ui.Fundo };
        Button btnAtualizar = Ui.BotaoArredondado("Atualizar", Ui.Azul, 180, 48);
        btnAtualizar.Location = new Point(20, 20);
        btnAtualizar.Click += (_, _) => Atualizar();
        rodape.Controls.Add(btnAtualizar);

        var painelGrade = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        painelGrade.Controls.Add(_grade);

        Controls.Add(painelGrade);
        Controls.Add(rodape);
        painelGrade.BringToFront();
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
        _grade.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Papel", HeaderText = "Seu papel", FillWeight = 90 });
        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Motorista", HeaderText = "Ofereceu (Motorista)", FillWeight = 120 });
        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Trajeto", HeaderText = "Trajeto", FillWeight = 140 });
        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Horario", HeaderText = "Horário", FillWeight = 70 });
        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Vagas", HeaderText = "Vagas restantes", FillWeight = 80 });
        _grade.Columns.Add(new DataGridViewTextBoxColumn
        { Name = "Passageiros", HeaderText = "Reservaram (Passageiros)", FillWeight = 180 });
    }

    private void Atualizar()
    {
        try
        {
            Mensagem resposta = _cliente.Enviar(Mensagem.Texto(Operacao.Reservas, _nomeUsuario));
            if (resposta.Operacao != Operacao.Resposta)
            {
                Aviso(resposta.Dados);
                return;
            }

            List<CaronaComReservas> painel =
                resposta.ObterDados<List<CaronaComReservas>>() ?? new List<CaronaComReservas>();

            _grade.Rows.Clear();
            foreach (CaronaComReservas item in painel)
            {
                Carona c = item.Carona;
                string passageiros = item.Passageiros.Count > 0
                    ? string.Join(", ", item.Passageiros)
                    : "— nenhuma reserva —";
                string papel = c.Motorista == _nomeUsuario ? "Motorista" : "Passageiro";

                _grade.Rows.Add(
                    papel,
                    c.Motorista,
                    $"{c.Origem} → {c.Destino}",
                    c.Horario,
                    c.Vagas,
                    passageiros);
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

    private void Aviso(string texto)
        => MessageBox.Show(this, texto, "UniMove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

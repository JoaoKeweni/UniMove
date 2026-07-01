using System.Drawing.Drawing2D;

namespace UniMove.Client;

/// <summary>Paleta e componentes visuais compartilhados pelas telas.</summary>
public static class Ui
{
    public static readonly Color Azul = Color.FromArgb(0, 108, 194);
    public static readonly Color AzulEscuro = Color.FromArgb(0, 84, 153);
    public static readonly Color Verde = Color.FromArgb(38, 166, 91);
    public static readonly Color Fundo = Color.White;
    public static readonly Color Texto = Color.FromArgb(45, 45, 45);
    public static readonly Color Cinza = Color.FromArgb(120, 120, 120);

    public static readonly Font FonteTitulo = new("Segoe UI", 22F, FontStyle.Bold);
    public static readonly Font FonteBotao = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font FonteCampo = new("Segoe UI", 11F);
    public static readonly Font FonteRotulo = new("Segoe UI", 10F, FontStyle.Regular);

    /// <summary>Cria um botão arredondado, com estilo achatado e moderno.</summary>
    public static Button BotaoArredondado(string texto, Color cor, int largura, int altura)
    {
        var botao = new Button
        {
            Text = texto,
            Size = new Size(largura, altura),
            BackColor = cor,
            ForeColor = Color.White,
            Font = FonteBotao,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        botao.FlatAppearance.BorderSize = 0;
        botao.FlatAppearance.MouseOverBackColor = Escurecer(cor, 0.08);
        botao.FlatAppearance.MouseDownBackColor = Escurecer(cor, 0.16);
        AplicarBordaArredondada(botao, 18);
        botao.Resize += (_, _) => AplicarBordaArredondada(botao, 18);
        return botao;
    }

    /// <summary>Aplica uma região de cantos arredondados ao controle.</summary>
    public static void AplicarBordaArredondada(Control controle, int raio)
    {
        using var caminho = new GraphicsPath();
        int d = raio * 2;
        var r = controle.ClientRectangle;
        caminho.AddArc(r.X, r.Y, d, d, 180, 90);
        caminho.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        caminho.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        caminho.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        caminho.CloseFigure();
        controle.Region = new Region(caminho);
    }

    private static Color Escurecer(Color cor, double fator)
    {
        int r = (int)(cor.R * (1 - fator));
        int g = (int)(cor.G * (1 - fator));
        int b = (int)(cor.B * (1 - fator));
        return Color.FromArgb(r, g, b);
    }
}

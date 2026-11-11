using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Nuestro propio MessageBox con el estilo de PITCHR (tarjeta blanca, ícono de color y botón píldora azul).
    /// Como esta clase está dentro del namespace PITCHR, cuando cualquier pantalla escribe
    /// MessageBox.Show(...) C# usa ESTA clase en vez de la de Windows. Por eso no hubo que cambiar
    /// ningún otro archivo: todos los avisos (contraseña incorrecta, bienvenido, errores...) ya salen así.
    /// </summary>
    public static class MessageBox
    {
        public static DialogResult Show(string texto)
        {
            return Show(texto, "PitchR", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static DialogResult Show(string texto, string titulo)
        {
            return Show(texto, titulo, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static DialogResult Show(string texto, string titulo, MessageBoxButtons botones)
        {
            return Show(texto, titulo, botones, MessageBoxIcon.None);
        }

        public static DialogResult Show(string texto, string titulo, MessageBoxButtons botones, MessageBoxIcon icono)
        {
            using (VentanaAviso ventana = new VentanaAviso(texto, titulo, botones, icono))
            {
                Form duena = Form.ActiveForm;
                if (duena != null && duena.Visible) return ventana.ShowDialog(duena);
                return ventana.ShowDialog();
            }
        }

        public static DialogResult Show(string texto, string titulo, MessageBoxButtons botones, MessageBoxIcon icono, MessageBoxDefaultButton botonPorDefecto)
        {
            return Show(texto, titulo, botones, icono);
        }
    }

    /// <summary>La ventanita del aviso. Se arma con código (no tiene diseñador).</summary>
    internal class VentanaAviso : Form
    {
        // Colores de PITCHR
        private static readonly Color AzulBoton = Color.FromArgb(8, 127, 223);    // #087FDF
        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);   // #0060D6
        private static readonly Color Naranja = Color.FromArgb(248, 110, 56);     // #F86E38
        private static readonly Color Rojo = Color.FromArgb(220, 53, 69);
        private static readonly Color GrisBoton = Color.FromArgb(240, 243, 244);  // #F0F3F4
        private static readonly Color GrisTexto = Color.FromArgb(80, 84, 96);

        private const int Ancho = 420;
        private const int Relleno = 32;

        private readonly Color _colorIcono;
        private readonly string _simbolo;

        public VentanaAviso(string texto, string titulo, MessageBoxButtons botones, MessageBoxIcon icono)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Color.White;
            KeyPreview = true;
            Text = titulo;
            DoubleBuffered = true;

            // Color y símbolo según el tipo de aviso
            switch (icono)
            {
                case MessageBoxIcon.Error:       _colorIcono = Rojo;        _simbolo = "✕"; break;   // también Hand y Stop
                case MessageBoxIcon.Warning:     _colorIcono = Naranja;     _simbolo = "!"; break;   // también Exclamation
                case MessageBoxIcon.Question:    _colorIcono = AzulPitcher; _simbolo = "?"; break;
                case MessageBoxIcon.Information: _colorIcono = AzulPitcher; _simbolo = "✓"; break;   // también Asterisk
                default:                         _colorIcono = AzulPitcher; _simbolo = "i"; break;
            }

            Font fuenteTitulo = Fuente(20f, FontStyle.Bold);
            Font fuenteTexto = Fuente(15f, FontStyle.Regular);
            int anchoTexto = Ancho - Relleno * 2;
            int y = Relleno + 64 + 16;   // debajo del ícono

            // Título
            Label tituloLabel = new Label
            {
                Text = string.IsNullOrWhiteSpace(titulo) ? "PitchR" : titulo,
                Font = fuenteTitulo,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Bounds = new Rectangle(Relleno, y, anchoTexto, 30)
            };
            Controls.Add(tituloLabel);
            y += 30 + 8;

            // Mensaje (la altura se calcula según lo largo del texto)
            Size medida = TextRenderer.MeasureText(texto ?? "", fuenteTexto, new Size(anchoTexto, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter);
            Label textoLabel = new Label
            {
                Text = texto,
                Font = fuenteTexto,
                ForeColor = GrisTexto,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false,
                Bounds = new Rectangle(Relleno, y, anchoTexto, medida.Height + 6)
            };
            Controls.Add(textoLabel);
            y += textoLabel.Height + 24;

            // Botones
            Button[] lista = CrearBotones(botones);
            int anchoBoton = lista.Length == 1 ? 160 : 140, separacion = 12;
            int total = lista.Length * anchoBoton + (lista.Length - 1) * separacion;
            int x = (Ancho - total) / 2;
            foreach (Button b in lista)
            {
                b.Bounds = new Rectangle(x, y, anchoBoton, 44);
                Controls.Add(b);
                x += anchoBoton + separacion;
            }
            AcceptButton = lista[0];
            CancelButton = lista[lista.Length - 1];

            ClientSize = new Size(Ancho, y + 44 + Relleno);

            // Esquinas redondeadas de la ventana
            using (GraphicsPath ruta = Redondeado(new RectangleF(0, 0, Width, Height), 20))
                Region = new Region(ruta);

            // Se puede arrastrar la ventana con el mouse
            MouseDown += Arrastrar;
            tituloLabel.MouseDown += Arrastrar;
            textoLabel.MouseDown += Arrastrar;
        }

        private Button[] CrearBotones(MessageBoxButtons botones)
        {
            switch (botones)
            {
                case MessageBoxButtons.OKCancel:
                    return new[] { Boton("Aceptar", DialogResult.OK, true), Boton("Cancelar", DialogResult.Cancel, false) };
                case MessageBoxButtons.YesNo:
                    return new[] { Boton("Sí", DialogResult.Yes, true), Boton("No", DialogResult.No, false) };
                case MessageBoxButtons.YesNoCancel:
                    return new[] { Boton("Sí", DialogResult.Yes, true), Boton("No", DialogResult.No, false), Boton("Cancelar", DialogResult.Cancel, false) };
                case MessageBoxButtons.RetryCancel:
                    return new[] { Boton("Reintentar", DialogResult.Retry, true), Boton("Cancelar", DialogResult.Cancel, false) };
                default:
                    return new[] { Boton("Aceptar", DialogResult.OK, true) };
            }
        }

        // Botón píldora: azul con letra blanca (principal) o gris con letra negra (secundario)
        private Button Boton(string texto, DialogResult resultado, bool principal)
        {
            Button b = new Button
            {
                Text = texto,
                DialogResult = resultado,
                FlatStyle = FlatStyle.Flat,
                Font = Fuente(15f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.White;
            b.FlatAppearance.MouseDownBackColor = Color.White;
            b.MouseEnter += (s, e) => b.Invalidate();
            b.MouseLeave += (s, e) => b.Invalidate();
            b.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                bool encima = b.ClientRectangle.Contains(b.PointToClient(Cursor.Position));
                Color fondo = principal ? AzulBoton : GrisBoton;
                if (encima) fondo = principal ? ControlPaint.Light(fondo, 0.2f) : ControlPaint.Dark(fondo, 0.02f);
                RectangleF area = new RectangleF(0, 0, b.Width - 1, b.Height - 1);
                using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
                using (SolidBrush brocha = new SolidBrush(fondo))
                    g.FillPath(brocha, ruta);
                TextRenderer.DrawText(g, b.Text, b.Font, b.ClientRectangle, principal ? Color.White : Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            };
            return b;
        }

        // Dibuja el borde y el círculo del ícono
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Borde gris suave
            using (GraphicsPath borde = Redondeado(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 20))
            using (Pen lapiz = new Pen(Color.FromArgb(225, 225, 225)))
                g.DrawPath(lapiz, borde);

            // Círculo de color clarito con el círculo fuerte adentro
            RectangleF afuera = new RectangleF((Width - 64) / 2f, Relleno, 64, 64);
            using (SolidBrush clarito = new SolidBrush(Color.FromArgb(35, _colorIcono)))
                g.FillEllipse(clarito, afuera);
            RectangleF adentro = RectangleF.Inflate(afuera, -10, -10);
            using (SolidBrush fuerte = new SolidBrush(_colorIcono))
                g.FillEllipse(fuerte, adentro);
            TextRenderer.DrawText(g, _simbolo, Fuente(22f, FontStyle.Bold), Rectangle.Round(adentro), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }

        // Sombra de Windows alrededor de la ventana
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;   // CS_DROPSHADOW
                return cp;
            }
        }

        // ---------- Ayudantes ----------
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void Arrastrar(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            try
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);   // mover la ventana como si se arrastrara la barra de título
            }
            catch (Exception) { }
        }

        // Plus Jakarta Sans si está instalada; si no, Segoe UI
        private static Font Fuente(float pixeles, FontStyle estilo)
        {
            Font f = new Font("Plus Jakarta Sans", pixeles, estilo, GraphicsUnit.Pixel);
            if (f.Name == "Plus Jakarta Sans") return f;
            f.Dispose();
            return new Font("Segoe UI", pixeles, estilo, GraphicsUnit.Pixel);
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            GraphicsPath ruta = new GraphicsPath();
            float d = Math.Max(1f, Math.Min(radio * 2, Math.Min(r.Width, r.Height)));
            ruta.AddArc(r.X, r.Y, d, d, 180, 90);
            ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }
    }
}

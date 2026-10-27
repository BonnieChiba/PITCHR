using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    public partial class CrearCuentaInversor : Form
    {
        public CrearCuentaInversor()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void LineaSuperiorPicturebox_Click(object sender, EventArgs e)
        {

        }

        private void RegistrarseBoton_Click(object sender, EventArgs e)
        {
            // CrearCuentaEmprendedor todavía no existe en este proyecto (está en IntegrativeProject).
            // Cuando la agreguen, quiten las // de estas 3 líneas:
            // CrearCuentaEmprendedor PantallaPrincipal = new CrearCuentaEmprendedor();
            // PantallaPrincipal.Show();
            // this.Hide();
        }

        private void ContraseñaTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void RegresarPictureBox_Click(object sender, EventArgs e)
        {
            // Aquí abren la pantalla anterior, igual que en RegistrarseBoton_Click.
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame en Figma: todo el diseño está hecho en 1280 × 832.
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 832f;

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private Image _ondaArriba;
        private Image _ondaAbajo;
        private Bitmap _fondoListo;
        private float _escala = 1f;   // cuánto creció el contenido
        private float _escalaX = 1f;  // cuánto se estiró el fondo a lo ancho
        private float _escalaY = 1f;  // cuánto se estiró el fondo a lo alto

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);
        private static readonly Color GrisBorde = Color.FromArgb(217, 217, 217);

        // Mensaje de Windows para poner texto guía gris dentro de un TextBox ("Usuario", "Contraseña"...).
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // Las ondas se dibujan a mano para que una pase por debajo y otra por encima del cuadro blanco.
            _ondaArriba = LineaSuperiorPicturebox.Image;
            _ondaAbajo = pictureBox1.Image;
            LineaSuperiorPicturebox.Visible = false;
            pictureBox1.Visible = false;

            // Las imágenes de fondo son solo la vista previa del diseñador; al ejecutar todo se dibuja con código.
            BackgroundInversor.BackgroundImage = null;
            CuadroBlancoTexto.BackgroundImage = null;
            RegistrarseBoton.BackgroundImage = null;

            // El cuadro blanco se dibuja redondeado y con sombra en CuadroBlancoTexto_Paint.
            CuadroBlancoTexto.BackColor = Color.Transparent;
            ActivarDobleBuffer(BackgroundInversor);
            ActivarDobleBuffer(CuadroBlancoTexto);

            // Botón píldora: el fondo lo pinta RegistrarseBoton_Paint.
            RegistrarseBoton.BackColor = Color.Transparent;
            RegistrarseBoton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            RegistrarseBoton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            RegistrarseBoton.MouseEnter += (s, e) => RegistrarseBoton.Invalidate();
            RegistrarseBoton.MouseLeave += (s, e) => RegistrarseBoton.Invalidate();

            // Campos: texto guía (viene de la propiedad Tag) y borde azul cuando están activos.
            foreach (Control control in CuadroBlancoTexto.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;
                caja.HandleCreated += (s, e) => PonerTextoGuia(caja);
                caja.GotFocus += (s, e) => CuadroBlancoTexto.Invalidate();
                caja.LostFocus += (s, e) => CuadroBlancoTexto.Invalidate();
            }

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            BackgroundInversor.Paint += BackgroundInversor_Paint;
            CuadroBlancoTexto.Paint += CuadroBlancoTexto_Paint;
            CuadroBlancoTexto.MouseDown += CuadroBlancoTexto_MouseDown;
            RegistrarseBoton.Paint += RegistrarseBoton_Paint;
            TerminosCheckbox.Paint += TerminosCheckbox_Paint;
            TerminosCheckbox.CheckedChanged += TerminosCheckbox_CheckedChanged;
            Resize += CrearCuentaInversor_Resize;

            // Se guardan las posiciones y fuentes del diseñador (1280 × 832) para escalarlas después.
            GuardarOriginales(BackgroundInversor);
            Load += (s, e) => AjustarATamano();
        }

        private void GuardarOriginales(Control padre)
        {
            foreach (Control hijo in padre.Controls)
            {
                _posicionesOriginales[hijo] = hijo.Bounds;
                _fuentesOriginales[hijo] = hijo.Font;
                GuardarOriginales(hijo);
            }
        }

        private static void ActivarDobleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           .SetValue(control, true, null);
        }

        private static void PonerTextoGuia(TextBoxBase caja)
        {
            string guia = caja.Tag as string;
            if (string.IsNullOrEmpty(guia)) return;
            try
            {
                SendMessage(caja.Handle, EM_SETCUEBANNER, (IntPtr)1, guia);
            }
            catch (Exception)
            {
                // Solo falla fuera de Windows.
            }
        }

        private void CrearCuentaInversor_Resize(object sender, EventArgs e)
        {
            AjustarATamano();
        }

        /// <summary>
        /// Hace crecer o encoger todo para llenar la ventana:
        /// el fondo se estira completo y el contenido crece parejo (sin deformarse) y centrado.
        /// </summary>
        private void AjustarATamano()
        {
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _escalaX = cliente.Width / AnchoFigma;
            _escalaY = cliente.Height / AltoFigma;
            _escala = Math.Min(_escalaX, _escalaY);
            float margenX = (cliente.Width - AnchoFigma * _escala) / 2f;
            float margenY = (cliente.Height - AltoFigma * _escala) / 2f;

            SuspendLayout();
            BackgroundInversor.Bounds = new Rectangle(0, 0, cliente.Width, cliente.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BackgroundInversor;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            ResumeLayout(true);

            PrepararFondo(cliente);
            BackgroundInversor.Invalidate(true);
        }

        private static readonly Dictionary<string, Font> CacheFuentes = new Dictionary<string, Font>();

        /// <summary>Misma fuente pero escalada. Si la fuente del Figma no está instalada, usa Segoe UI.</summary>
        private static Font EscalarFuente(Font original, float escala)
        {
            string nombre = original.OriginalFontName ?? original.Name;
            if (!FuenteInstalada(nombre)) nombre = "Segoe UI";
            float pixeles = (float)Math.Round(original.SizeInPoints * 96f / 72f * escala, 1);
            string clave = nombre + "|" + pixeles + "|" + (int)original.Style;

            Font fuente;
            if (!CacheFuentes.TryGetValue(clave, out fuente))
            {
                fuente = new Font(nombre, pixeles, original.Style, GraphicsUnit.Pixel);
                CacheFuentes[clave] = fuente;
            }
            return fuente;
        }

        private static readonly Dictionary<string, bool> Instaladas = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private static bool FuenteInstalada(string nombre)
        {
            bool hay;
            if (Instaladas.TryGetValue(nombre, out hay)) return hay;
            hay = false;
            using (InstalledFontCollection fuentes = new InstalledFontCollection())
                foreach (FontFamily familia in fuentes.Families)
                    if (string.Equals(familia.Name, nombre, StringComparison.OrdinalIgnoreCase)) { hay = true; break; }
            Instaladas[nombre] = hay;
            return hay;
        }

        /// <summary>Dibuja una sola vez (por tamaño) el fondo: margen blanco, rectángulo azul y ondas.</summary>
        private void PrepararFondo(Size tamano)
        {
            Bitmap nuevo = new Bitmap(tamano.Width, tamano.Height);
            using (Graphics g = Graphics.FromImage(nuevo))
            {
                Calidad(g);
                g.Clear(Color.White);
                RectangleF azul = new RectangleF(19 * _escalaX, 19 * _escalaY, 1241 * _escalaX, 794 * _escalaY);
                using (GraphicsPath ruta = Redondeado(azul, 20 * _escala))
                using (SolidBrush brocha = new SolidBrush(AzulPitcher))
                    g.FillPath(brocha, ruta);
                if (_ondaAbajo != null) g.DrawImage(_ondaAbajo, 0, 586 * _escalaY, AnchoFigma * _escalaX, 246 * _escalaY);
                if (_ondaArriba != null) g.DrawImage(_ondaArriba, 0, 0, AnchoFigma * _escalaX, 254 * _escalaY);
            }
            if (_fondoListo != null) _fondoListo.Dispose();
            _fondoListo = nuevo;
        }

        private void BackgroundInversor_Paint(object sender, PaintEventArgs e)
        {
            if (_fondoListo != null) e.Graphics.DrawImageUnscaled(_fondoListo, 0, 0);
        }

        /// <summary>Cuadro blanco redondeado con sombra, campos tipo píldora y la onda que pasa por encima.</summary>
        private void CuadroBlancoTexto_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            float k = _escala;
            RectangleF tarjeta = new RectangleF(4 * k, 0, CuadroBlancoTexto.Width - 8 * k - 1, CuadroBlancoTexto.Height - 10 * k);

            Sombra(g, tarjeta, 30 * k, k);
            using (GraphicsPath ruta = Redondeado(tarjeta, 30 * k))
            {
                g.FillPath(Brushes.White, ruta);

                // Campos píldora alrededor de cada TextBox (alto 37 y 16 de margen, como en Figma)
                foreach (Control control in CuadroBlancoTexto.Controls)
                {
                    TextBoxBase caja = control as TextBoxBase;
                    if (caja == null || !caja.Visible) continue;
                    using (GraphicsPath pildora = Redondeado(Pildora(caja), 18.5f * k))
                    using (Pen borde = new Pen(caja.Focused ? AzulPitcher : GrisBorde, Math.Max(1f, k)))
                        g.DrawPath(borde, pildora);
                }

                // La onda de arriba cruza por encima del cuadro blanco (como en el Figma)
                if (_ondaArriba != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    g.DrawImage(_ondaArriba, -CuadroBlancoTexto.Left, -CuadroBlancoTexto.Top, AnchoFigma * _escalaX, 254 * _escalaY);
                    g.Clip = anterior;
                }
            }
        }

        private RectangleF Pildora(Control caja)
        {
            float k = _escala;
            float centro = caja.Top + caja.Height / 2f;
            return new RectangleF(caja.Left - 16 * k, centro - 18.5f * k, caja.Width + 32 * k, 37 * k);
        }

        // Clic en la píldora (fuera del texto) también pone el cursor en el campo.
        private void CuadroBlancoTexto_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Control control in CuadroBlancoTexto.Controls)
                if (control is TextBoxBase && Pildora(control).Contains(e.Location)) { control.Focus(); return; }
        }

        /// <summary>Botón "Registrarme" como píldora azul con sombra.</summary>
        private void RegistrarseBoton_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);   // el botón está sobre el cuadro blanco
            float k = boton.Height / 56f;
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k);
            Sombra(g, area, area.Height / 2f, k);

            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            Color color = encima ? ControlPaint.Light(AzulPitcher, 0.15f) : AzulPitcher;
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            using (SolidBrush brocha = new SolidBrush(color))
                g.FillPath(brocha, ruta);

            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), boton.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void TerminosCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            TerminosCheckbox.Invalidate();
        }

        /// <summary>Casilla cuadrada con borde verde y el texto a la derecha, como en el Figma.</summary>
        private void TerminosCheckbox_Paint(object sender, PaintEventArgs e)
        {
            CheckBox casilla = (CheckBox)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF cuadro = new RectangleF(1 * k, 5 * k, 28 * k, 28 * k);

            using (GraphicsPath ruta = Redondeado(cuadro, 5 * k))
            {
                using (SolidBrush fondo = new SolidBrush(casilla.Checked ? Color.FromArgb(67, 200, 120) : Color.FromArgb(245, 245, 245)))
                    g.FillPath(fondo, ruta);
                using (Pen borde = new Pen(Color.FromArgb(161, 251, 203), 2 * k))
                    g.DrawPath(borde, ruta);
            }
            if (casilla.Checked)
            {
                using (Pen palomita = new Pen(Color.White, 2.5f * k))
                {
                    palomita.StartCap = LineCap.Round;
                    palomita.EndCap = LineCap.Round;
                    palomita.LineJoin = LineJoin.Round;
                    g.DrawLines(palomita, new[]
                    {
                        new PointF(cuadro.X + cuadro.Width * 0.25f, cuadro.Y + cuadro.Height * 0.52f),
                        new PointF(cuadro.X + cuadro.Width * 0.43f, cuadro.Y + cuadro.Height * 0.70f),
                        new PointF(cuadro.X + cuadro.Width * 0.76f, cuadro.Y + cuadro.Height * 0.32f)
                    });
                }
            }
            TextRenderer.DrawText(g, casilla.Text, casilla.Font, new Point((int)(44 * k), 0), Color.Black);
        }

        private static void Calidad(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
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

        /// <summary>Sombra como la del Figma (Y 4, desenfoque 4, negro al 25 %).</summary>
        private static void Sombra(Graphics g, RectangleF r, float radio, float k)
        {
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(r.X - d * 0.5f, r.Y + d, r.Width + d, r.Height + d * 0.5f);
                using (GraphicsPath ruta = Redondeado(capa, radio + d))
                using (SolidBrush brocha = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                    g.FillPath(brocha, ruta);
            }
        }

        #endregion

        private void TerminosCheckbox_CheckedChanged_1(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    public partial class IniciaAqui : Form
    {
        public IniciaAqui()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
        }

        // Tarjeta azul "Inversor"
        private void InversorPanel_Click(object sender, EventArgs e)
        {
            CrearCuentaInversor pantalla = new CrearCuentaInversor();
            pantalla.Show();
            this.Hide();
        }

        // Tarjeta naranja "Emprendedor"
        private void EmprendedorPanel_Click(object sender, EventArgs e)
        {
            // Cuando agreguen la pantalla CrearCuentaEmprendedor al proyecto, quiten las // de estas 3 líneas
            // y borren el MessageBox:
            // CrearCuentaEmprendedor pantalla = new CrearCuentaEmprendedor();
            // pantalla.Show();
            // this.Hide();
            MessageBox.Show("La pantalla para emprendedores estará disponible próximamente.", "Pitcher");
        }

        // Flecha de regresar
        private void RegresarPictureBox_Click(object sender, EventArgs e)
        {
            EmprendimientoGuest pantalla = new EmprendimientoGuest();
            pantalla.Show();
            this.Hide();
        }

        // "¿Ya tienes una cuenta? Iniciar sesión"
        private void IniciarSesionLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Cuando agreguen la pantalla de iniciar sesión, ábranla aquí con el mismo patrón:
            //     IniciarSesion pantalla = new IniciarSesion();
            //     pantalla.Show();
            //     this.Hide();
            MessageBox.Show("La pantalla para iniciar sesión estará disponible próximamente.", "Pitcher");
        }

        // Como las pantallas se ocultan con Hide(), si el usuario cierra esta ventana con la X
        // se cierra toda la aplicación (si no, se quedaría abierta en segundo plano).
        private void IniciaAqui_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame "Inicio" en Figma.
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 832f;

        private static readonly Color AzulInversor = Color.FromArgb(56, 189, 248);      // #38BDF8
        private static readonly Color NaranjaEmprendedor = Color.FromArgb(248, 110, 56); // #F86E38

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private float _escala = 1f;

        private void PrepararDiseno()
        {
            // Las imágenes de fondo de las tarjetas son solo la vista previa del diseñador;
            // al ejecutar, las tarjetas se dibujan con código (redondeadas, con sombra y efecto al pasar el mouse).
            InversorPanel.BackgroundImage = null;
            EmprendedorPanel.BackgroundImage = null;
            ActivarDobleBuffer(InversorPanel);
            ActivarDobleBuffer(EmprendedorPanel);

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            InversorPanel.Paint += Tarjeta_Paint;
            EmprendedorPanel.Paint += Tarjeta_Paint;
            ConectarTarjeta(InversorPanel, InversorPanel_Click);
            ConectarTarjeta(EmprendedorPanel, EmprendedorPanel_Click);
            Resize += (s, e) => AjustarATamano();
            Load += (s, e) => AjustarATamano();
            Shown += (s, e) => ActiveControl = null;   // que ningún control aparezca seleccionado al abrir

            // Se guardan las posiciones y fuentes del diseñador (1280 × 832) para escalarlas después.
            GuardarOriginales(this);
        }

        /// <summary>El clic en el ícono o el texto de una tarjeta cuenta como clic en la tarjeta.</summary>
        private static void ConectarTarjeta(Panel tarjeta, EventHandler alHacerClic)
        {
            foreach (Control hijo in tarjeta.Controls)
            {
                hijo.Cursor = Cursors.Hand;
                hijo.Click += alHacerClic;
                hijo.MouseEnter += (s, e) => tarjeta.Invalidate(true);
                hijo.MouseLeave += (s, e) => tarjeta.Invalidate(true);
            }
            tarjeta.MouseEnter += (s, e) => tarjeta.Invalidate(true);
            tarjeta.MouseLeave += (s, e) => tarjeta.Invalidate(true);
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

        /// <summary>
        /// Hace crecer o encoger todo para llenar la ventana: el contenido crece parejo (sin deformarse) y centrado.
        /// </summary>
        private void AjustarATamano()
        {
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _escala = Math.Min(cliente.Width / AnchoFigma, cliente.Height / AltoFigma);
            float margenX = (cliente.Width - AnchoFigma * _escala) / 2f;
            float margenY = (cliente.Height - AltoFigma * _escala) / 2f;

            SuspendLayout();
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == this;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            ResumeLayout(true);
            Invalidate(true);
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

        /// <summary>Tarjeta redondeada (radio 30) con sombra; se aclara un poco al pasar el mouse.</summary>
        private void Tarjeta_Paint(object sender, PaintEventArgs e)
        {
            Panel tarjeta = (Panel)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.White);   // las tarjetas están sobre fondo blanco

            float k = _escala;
            RectangleF area = new RectangleF(4 * k, 0, tarjeta.Width - 8 * k - 1, tarjeta.Height - 10 * k);

            // Sombra del Figma (Y 4, desenfoque 4, negro al 25 %)
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(area.X - d * 0.5f, area.Y + d, area.Width + d, area.Height + d * 0.5f);
                using (GraphicsPath sombra = Redondeado(capa, 30 * k + d))
                using (SolidBrush brocha = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                    g.FillPath(brocha, sombra);
            }

            Color color = tarjeta == InversorPanel ? AzulInversor : NaranjaEmprendedor;
            bool encima = tarjeta.ClientRectangle.Contains(tarjeta.PointToClient(Cursor.Position));
            if (encima) color = ControlPaint.Light(color, 0.12f);

            using (GraphicsPath ruta = Redondeado(area, 30 * k))
            using (SolidBrush relleno = new SolidBrush(color))
                g.FillPath(relleno, ruta);
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

        #endregion
    }
}

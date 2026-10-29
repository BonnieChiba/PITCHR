using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Datos de un emprendimiento. Por ahora se llenan con ejemplos en EmprendimientosGuest;
    /// cuando conecten la base de datos, cada fila del SELECT se convierte en uno de estos.
    /// </summary>
    public class Emprendimiento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public Image Imagen { get; set; }   // si es null, la tarjeta dibuja un fondo de color con las iniciales

        public Emprendimiento(int id, string nombre, string descripcion, string categoria, Image imagen)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Categoria = categoria;
            Imagen = imagen;
        }
    }

    /// <summary>
    /// Tarjeta de un emprendimiento, con el diseño del Figma (Home).
    /// Tiene dos formas: Vertical (carrusel de arriba, 291 × 351) y Horizontal (cuadrícula de abajo, 514 × 259).
    /// Todo se dibuja en OnPaint, así crece o se encoge sin perder calidad.
    /// </summary>
    [DesignerCategory("UserControl")]
    public class TarjetaEmprendimiento : UserControl
    {
        public enum Formas { Vertical, Horizontal }

        private string _titulo = "Nombre del emprendimiento";
        private string _descripcion = "Descripción corta del emprendimiento.";
        private string _categoria = "Categoría";
        private Image _imagen;
        private bool _favorito;
        private Formas _forma = Formas.Vertical;
        private bool _mouseEncima;

        /// <summary>Se dispara al hacer clic en la tarjeta (no en el corazón).</summary>
        public event EventHandler TarjetaClick;

        public TarjetaEmprendimiento()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Size = new Size(291, 351);
        }

        [Category("Pitcher")] public string Titulo { get { return _titulo; } set { _titulo = value; Invalidate(); } }
        [Category("Pitcher")] public string Descripcion { get { return _descripcion; } set { _descripcion = value; Invalidate(); } }
        [Category("Pitcher")] public string Categoria { get { return _categoria; } set { _categoria = value; Invalidate(); } }
        [Category("Pitcher")] public Image Imagen { get { return _imagen; } set { _imagen = value; Invalidate(); } }
        [Category("Pitcher")] public bool Favorito { get { return _favorito; } set { _favorito = value; Invalidate(); } }
        [Category("Pitcher")] public Formas Forma { get { return _forma; } set { _forma = value; Invalidate(); } }

        /// <summary>El emprendimiento que muestra esta tarjeta (para saber cuál se eligió).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Emprendimiento Datos { get; private set; }

        public void Mostrar(Emprendimiento datos)
        {
            Datos = datos;
            _titulo = datos.Nombre;
            _descripcion = datos.Descripcion;
            _categoria = datos.Categoria;
            _imagen = datos.Imagen;
            Invalidate();
        }

        // ---------- Medidas del Figma ----------
        private float K { get { return _forma == Formas.Vertical ? Width / 291f : Width / 514f; } }

        private RectangleF Tarjeta
        {
            get { float k = K; return new RectangleF(2 * k, 1 * k, Width - 4 * k - 1, Height - 7 * k); }
        }

        private RectangleF Corazon
        {
            get
            {
                float k = K; RectangleF t = Tarjeta;
                return _forma == Formas.Vertical
                    ? new RectangleF(t.X + 234 * k, t.Bottom - 50 * k, 37 * k, 37 * k)
                    : new RectangleF(t.X + 445 * k, t.Bottom - 46 * k, 37 * k, 37 * k);
            }
        }

        // ---------- Eventos del mouse ----------
        protected override void OnMouseEnter(EventArgs e) { _mouseEncima = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _mouseEncima = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;
            RectangleF zona = Corazon; zona.Inflate(6 * K, 6 * K);
            if (zona.Contains(e.Location))
            {
                Favorito = !Favorito;     // por ahora solo cambia el dibujo; después se guardará en la base
            }
            else if (TarjetaClick != null)
            {
                TarjetaClick(this, EventArgs.Empty);
            }
        }

        // ---------- Dibujo ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent != null ? Parent.BackColor : Color.White);

            float k = K;
            RectangleF t = Tarjeta;
            float radio = 20 * k;

            // Sombra (Y 4, desenfoque 4, negro 25 %) y fondo #F6F6F6
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(t.X - d * 0.4f, t.Y + d, t.Width + d * 0.8f, t.Height + d * 0.4f);
                using (GraphicsPath p = Redondeado(capa, radio + d))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(_mouseEncima ? 24 : 16, 0, 0, 0)))
                    g.FillPath(b, p);
            }
            using (GraphicsPath p = Redondeado(t, radio))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(246, 246, 246)))
                g.FillPath(b, p);

            RectangleF foto;
            RectangleF textos;
            float tamTitulo;
            if (_forma == Formas.Vertical)
            {
                foto = new RectangleF(t.X, t.Y, t.Width, 176 * k);
                textos = new RectangleF(t.X + 16 * k, t.Y + 186 * k, t.Width - 32 * k, 0);
                tamTitulo = 25;
            }
            else
            {
                foto = new RectangleF(t.X, t.Y, 173 * k, t.Height);
                textos = new RectangleF(t.X + 188 * k, t.Y + 36 * k, t.Width - 210 * k, 0);
                tamTitulo = 28;
            }

            DibujarFoto(g, foto, radio, k);

            using (Font fTitulo = Fuente("Plus Jakarta Sans", tamTitulo * k, FontStyle.Bold))
            using (Font fDesc = Fuente("Plus Jakarta Sans", 17 * k, FontStyle.Regular))
            {
                Rectangle rTitulo = Rectangle.Round(new RectangleF(textos.X, textos.Y, textos.Width, fTitulo.Height * 1.1f));
                TextRenderer.DrawText(g, _titulo, fTitulo, rTitulo, Color.Black,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                float yDesc = rTitulo.Bottom + (_forma == Formas.Vertical ? 8 : 14) * k;
                float altoDesc = (_forma == Formas.Vertical ? 46 : 70) * k;
                Rectangle rDesc = Rectangle.Round(new RectangleF(textos.X, yDesc, textos.Width, altoDesc));
                TextRenderer.DrawText(g, _descripcion, fDesc, rDesc, Color.FromArgb(30, 30, 30),
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }

            DibujarEtiqueta(g, k, t);
            DibujarCorazon(g, Corazon, _favorito);
        }

        private void DibujarFoto(Graphics g, RectangleF foto, float radio, float k)
        {
            using (GraphicsPath ruta = _forma == Formas.Vertical ? RedondeadoArriba(foto, radio) : RedondeadoIzquierda(foto, radio))
            {
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                if (_imagen != null)
                {
                    // Recorta la imagen para llenar el espacio sin deformarla (como "object-fit: cover")
                    float escala = Math.Max(foto.Width / _imagen.Width, foto.Height / _imagen.Height);
                    float w = _imagen.Width * escala, h = _imagen.Height * escala;
                    g.DrawImage(_imagen, foto.X + (foto.Width - w) / 2f, foto.Y + (foto.Height - h) / 2f, w, h);
                }
                else
                {
                    // Sin foto todavía: fondo con el color de la categoría y las iniciales del nombre
                    Color c1, c2;
                    ColoresCategoria(_categoria, out c1, out c2);
                    using (LinearGradientBrush b = new LinearGradientBrush(foto, c1, c2, 35f))
                        g.FillRectangle(b, foto);
                    using (SolidBrush brillo = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    {
                        float d = foto.Height * 1.1f;
                        g.FillEllipse(brillo, foto.Right - d * 0.55f, foto.Y - d * 0.35f, d, d);
                    }
                    using (Font fIni = Fuente("Plus Jakarta Sans", (_forma == Formas.Vertical ? 64 : 56) * k, FontStyle.Bold))
                        TextRenderer.DrawText(g, Iniciales(_titulo), fIni, Rectangle.Round(foto), Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                g.Clip = anterior;
            }
        }

        private void DibujarEtiqueta(Graphics g, float k, RectangleF t)
        {
            string texto = (_categoria ?? "").ToUpperInvariant();
            using (Font f = Fuente("Plus Jakarta Sans", 13 * k, FontStyle.Bold))
            {
                Size medida = TextRenderer.MeasureText(g, texto, f, Size.Empty, TextFormatFlags.NoPadding);
                float alto = 32 * k;
                float ancho = medida.Width + 30 * k;
                float x = _forma == Formas.Vertical ? t.X + 14 * k : t.X + 190 * k;
                float y = _forma == Formas.Vertical ? t.Bottom - 48 * k : t.Bottom - 46 * k;
                RectangleF r = new RectangleF(x, y, ancho, alto);
                using (GraphicsPath p = Redondeado(r, alto / 2f))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(125, 248, 110, 56)))   // rgba(248,110,56,.49)
                    g.FillPath(b, p);
                TextRenderer.DrawText(g, texto, f, Rectangle.Round(r), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }

        private static void DibujarCorazon(Graphics g, RectangleF r, bool lleno)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddBezier(x + w * 0.50f, y + h * 0.86f, x + w * 0.10f, y + h * 0.58f, x + w * 0.02f, y + h * 0.40f, x + w * 0.08f, y + h * 0.26f);
                p.AddBezier(x + w * 0.08f, y + h * 0.26f, x + w * 0.16f, y + h * 0.08f, x + w * 0.42f, y + h * 0.08f, x + w * 0.50f, y + h * 0.28f);
                p.AddBezier(x + w * 0.50f, y + h * 0.28f, x + w * 0.58f, y + h * 0.08f, x + w * 0.84f, y + h * 0.08f, x + w * 0.92f, y + h * 0.26f);
                p.AddBezier(x + w * 0.92f, y + h * 0.26f, x + w * 0.98f, y + h * 0.40f, x + w * 0.90f, y + h * 0.58f, x + w * 0.50f, y + h * 0.86f);
                p.CloseFigure();
                if (lleno)
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(232, 59, 59)))
                        g.FillPath(b, p);
                }
                else
                {
                    using (Pen pen = new Pen(Color.FromArgb(25, 25, 25), Math.Max(1.5f, w * 0.075f)))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawPath(pen, p);
                    }
                }
            }
        }

        // ---------- Utilidades ----------
        private static string Iniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "";
            string[] partes = nombre.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string ini = partes[0].Substring(0, 1);
            if (partes.Length > 1) ini += partes[1].Substring(0, 1);
            return ini.ToUpperInvariant();
        }

        /// <summary>Colores de fondo para las tarjetas sin foto, según la categoría (las del menú Filtrar).</summary>
        public static void ColoresCategoria(string categoria, out Color c1, out Color c2)
        {
            switch ((categoria ?? "").Trim().ToLowerInvariant())
            {
                case "comida": c1 = Color.FromArgb(248, 110, 56); c2 = Color.FromArgb(253, 168, 92); break;
                case "finanzas": c1 = Color.FromArgb(0, 96, 214); c2 = Color.FromArgb(64, 93, 255); break;
                case "hogar y limpieza": c1 = Color.FromArgb(56, 189, 248); c2 = Color.FromArgb(125, 211, 252); break;
                case "industria": c1 = Color.FromArgb(71, 85, 105); c2 = Color.FromArgb(148, 163, 184); break;
                case "naturaleza": c1 = Color.FromArgb(22, 163, 74); c2 = Color.FromArgb(132, 204, 22); break;
                case "salud": c1 = Color.FromArgb(225, 29, 72); c2 = Color.FromArgb(251, 113, 133); break;
                case "startup": c1 = Color.FromArgb(9, 9, 107); c2 = Color.FromArgb(124, 58, 237); break;
                default: c1 = Color.FromArgb(0, 96, 214); c2 = Color.FromArgb(30, 153, 247); break;
            }
        }

        private static Font Fuente(string nombre, float pixeles, FontStyle estilo)
        {
            if (!EsInstalada(nombre)) nombre = "Segoe UI";
            return new Font(nombre, Math.Max(1f, pixeles), estilo, GraphicsUnit.Pixel);
        }

        private static bool? _jakarta;
        private static bool EsInstalada(string nombre)
        {
            if (nombre != "Plus Jakarta Sans") return true;
            if (_jakarta == null)
            {
                _jakarta = false;
                using (InstalledFontCollection fuentes = new InstalledFontCollection())
                    foreach (FontFamily f in fuentes.Families)
                        if (f.Name == nombre) { _jakarta = true; break; }
            }
            return _jakarta.Value;
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Max(1f, Math.Min(radio * 2, Math.Min(r.Width, r.Height)));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath RedondeadoArriba(RectangleF r, float radio)
        {
            GraphicsPath p = new GraphicsPath();
            float d = radio * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath RedondeadoIzquierda(RectangleF r, float radio)
        {
            GraphicsPath p = new GraphicsPath();
            float d = radio * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddLine(r.Right, r.Y, r.Right, r.Bottom);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}

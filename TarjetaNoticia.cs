using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Datos de una noticia. Por ahora se llenan en Noticias.CargarNoticias();
    /// cuando conecten la base de datos, cada fila del SELECT se convierte en una de estas.
    /// </summary>
    public class Noticia
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Categoria { get; set; }
        public string Fecha { get; set; }       // texto tal como se muestra, ej. "12 JUN 2025"
        public Image Imagen { get; set; }
        public string Enlace { get; set; }      // página de donde se sacó la noticia (columna eventLink)

        public Noticia(int id, string titulo, string resumen, string categoria, string fecha, Image imagen)
        {
            Id = id;
            Titulo = titulo;
            Resumen = resumen;
            Categoria = categoria;
            Fecha = fecha;
            Imagen = imagen;
        }
    }

    /// <summary>
    /// Tarjeta de noticia con el diseño del Figma (News).
    /// Destacada: la grande de la izquierda (628 × 483). Lista: las tres de la derecha (476 × 143).
    /// Todo se dibuja en OnPaint, así crece o se encoge sin perder calidad.
    /// </summary>
    public class TarjetaNoticia : UserControl
    {
        public enum Formas { Destacada, Lista }

        private string _titulo = "Título de la noticia";
        private string _resumen = "Resumen corto de la noticia.";
        private string _categoria = "Categoría";
        private string _fecha = "01 ENE 2026";
        private Image _imagen;
        private Formas _forma = Formas.Lista;
        private bool _mouseEncima;

        private static readonly Color Naranja = Color.FromArgb(255, 105, 56);      // #FF6938
        private static readonly Color AzulTitulo = Color.FromArgb(8, 120, 232);    // #0878E8
        private static readonly Color GrisFecha = Color.FromArgb(144, 161, 185);   // #90A1B9
        private static readonly Color GrisVerMas = Color.FromArgb(98, 116, 142);   // #62748E

        /// <summary>Se dispara al hacer clic en la tarjeta.</summary>
        public event EventHandler TarjetaClick;

        public TarjetaNoticia()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Size = new Size(480, 150);
        }

        [Category("PitchR")] public string Titulo { get { return _titulo; } set { _titulo = value; Invalidate(); } }
        [Category("PitchR")] public string Resumen { get { return _resumen; } set { _resumen = value; Invalidate(); } }
        [Category("PitchR")] public string Categoria { get { return _categoria; } set { _categoria = value; Invalidate(); } }
        [Category("PitchR")] public string Fecha { get { return _fecha; } set { _fecha = value; Invalidate(); } }
        [Category("PitchR")] public Image Imagen { get { return _imagen; } set { _imagen = value; Invalidate(); } }
        [Category("PitchR"), DefaultValue("")] public string Enlace { get; set; } = "";   // página que se abre al hacer clic
        [Category("PitchR")] public Formas Forma { get { return _forma; } set { _forma = value; Invalidate(); } }

        /// <summary>La noticia que muestra esta tarjeta (para saber cuál se eligió).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Noticia Datos { get; private set; }

        public void Mostrar(Noticia datos)
        {
            Datos = datos;
            _titulo = datos.Titulo;
            _resumen = datos.Resumen;
            _categoria = datos.Categoria;
            _fecha = datos.Fecha;
            _imagen = datos.Imagen;   // si es null se dibuja un degradado azul
            Invalidate();
        }

        // ---------- Medidas del Figma ----------
        private float K { get { return _forma == Formas.Destacada ? Width / 632f : Width / 480f; } }
        private RectangleF Tarjeta { get { float k = K; return new RectangleF(2 * k, 1 * k, Width - 4 * k - 1, Height - 7 * k); } }

        protected override void OnMouseEnter(EventArgs e) { _mouseEncima = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _mouseEncima = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button == MouseButtons.Left && TarjetaClick != null) TarjetaClick(this, EventArgs.Empty);
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
            float radio = (_forma == Formas.Destacada ? 20 : 16) * k;

            // Sombra (Y 4, desenfoque 4, negro 25 %)
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(t.X - d * 0.4f, t.Y + d, t.Width + d * 0.8f, t.Height + d * 0.4f);
                using (GraphicsPath p = Redondeado(capa, radio + d))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(_mouseEncima ? 24 : 16, 0, 0, 0)))
                    g.FillPath(b, p);
            }

            if (_forma == Formas.Destacada) DibujarDestacada(g, t, radio, k);
            else DibujarLista(g, t, radio, k);
        }

        /// <summary>Noticia grande: foto completa, degradado oscuro abajo y textos blancos encima.</summary>
        private void DibujarDestacada(Graphics g, RectangleF t, float radio, float k)
        {
            using (GraphicsPath ruta = Redondeado(t, radio))
            {
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                using (SolidBrush fondo = new SolidBrush(Color.FromArgb(30, 41, 59))) g.FillRectangle(fondo, t);
                DibujarFoto(g, t);

                // Degradado del Figma: de transparente (arriba) a #020618 (abajo), pasando por 30 % a la mitad
                RectangleF sombra = new RectangleF(t.X, t.Y + 125 * k, t.Width, t.Height - 125 * k + 1);
                using (LinearGradientBrush b = new LinearGradientBrush(sombra, Color.Transparent, Color.FromArgb(2, 6, 24), 90f))
                {
                    ColorBlend mezcla = new ColorBlend();
                    mezcla.Colors = new[] { Color.FromArgb(0, 2, 6, 24), Color.FromArgb(77, 2, 6, 24), Color.FromArgb(255, 2, 6, 24) };
                    mezcla.Positions = new[] { 0f, 0.5f, 1f };
                    b.InterpolationColors = mezcla;
                    g.FillRectangle(b, sombra);
                }
                g.Clip = anterior;
            }

            float x = t.X + 32 * k;
            float ancho = 549 * k;
            TextFormatFlags varias = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;

            // Los textos se acomodan de abajo hacia arriba (como en el Figma): Ver más → resumen → título → etiqueta
            float yVerMas = t.Bottom - 46 * k;
            using (Font fTitulo = Fuente(30 * k, FontStyle.Bold))
            using (Font fResumen = Fuente(15 * k, FontStyle.Regular))
            using (Font fTag = Fuente(12 * k, FontStyle.Bold))
            {
                float altoResumen = string.IsNullOrWhiteSpace(_resumen) ? -18 * k : Math.Min(TextRenderer.MeasureText(g, _resumen, fResumen, new Size((int)ancho, int.MaxValue), varias | TextFormatFlags.NoPadding).Height, 48 * k);
                float yResumen = yVerMas - 18 * k - altoResumen;
                // El título puede ocupar hasta 3 renglones (el alto de un renglón cambia según la fuente de cada computadora)
                int altoRenglon = TextRenderer.MeasureText(g, "Ág", fTitulo, Size.Empty, TextFormatFlags.NoPadding).Height;
                float altoTitulo = Math.Min(TextRenderer.MeasureText(g, _titulo, fTitulo, new Size((int)ancho, int.MaxValue), varias | TextFormatFlags.NoPadding).Height, altoRenglon * 3);
                float yTitulo = yResumen - 12 * k - altoTitulo;
                float yTag = yTitulo - 14 * k - 28 * k;

                // Etiqueta naranja "DESTACADA" y fecha
                string tag = "DESTACADA";
                Size medida = TextRenderer.MeasureText(g, tag, fTag, Size.Empty, TextFormatFlags.NoPadding);
                RectangleF pildora = new RectangleF(x, yTag, medida.Width + 24 * k, 28 * k);
                using (GraphicsPath p = Redondeado(pildora, pildora.Height / 2f))
                using (SolidBrush b = new SolidBrush(Naranja))
                    g.FillPath(b, p);
                Texto(g, tag, fTag, pildora, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                Texto(g, (_fecha ?? "").ToUpperInvariant(), fTag,
                      new RectangleF(pildora.Right + 12 * k, pildora.Y, 300 * k, pildora.Height), Color.FromArgb(191, 255, 255, 255),
                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                Texto(g, _titulo, fTitulo, new RectangleF(x, yTitulo, ancho, altoTitulo), Color.White, varias);
                Texto(g, _resumen, fResumen, new RectangleF(x, yResumen, ancho, altoResumen), Color.FromArgb(204, 255, 255, 255), varias);
            }

            DibujarVerMas(g, "Ver más", Fuente(14 * k, FontStyle.Bold), new PointF(x, yVerMas), Color.White, k);
        }

        /// <summary>Noticia chica: foto a la izquierda; categoría, fecha, título y "Ver más" a la derecha.</summary>
        private void DibujarLista(Graphics g, RectangleF t, float radio, float k)
        {
            using (GraphicsPath ruta = Redondeado(t, radio))
            {
                g.FillPath(Brushes.White, ruta);

                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                RectangleF areaFoto = new RectangleF(t.X, t.Y, 120 * k, t.Height);
                g.SetClip(areaFoto, CombineMode.Intersect);   // la foto no se sale de su columna aunque sea muy ancha
                DibujarFoto(g, areaFoto);
                g.Clip = anterior;

                using (Pen borde = new Pen(Color.FromArgb(241, 245, 249), Math.Max(1f, 0.8f * k)))
                    g.DrawPath(borde, ruta);
            }

            float x = t.X + 137 * k;
            float derecha = t.Right - 16 * k;
            using (Font fChica = Fuente(10 * k, FontStyle.Bold))
            {
                Texto(g, (_categoria ?? "").ToUpperInvariant(), fChica, new RectangleF(x, t.Y + 26 * k, 200 * k, 16 * k), Naranja,
                      TextFormatFlags.Left | TextFormatFlags.Top);
                Texto(g, (_fecha ?? "").ToUpperInvariant(), fChica, new RectangleF(derecha - 120 * k, t.Y + 26 * k, 120 * k, 16 * k), GrisFecha,
                      TextFormatFlags.Right | TextFormatFlags.Top);
            }

            using (Font fTitulo = Fuente(15 * k, FontStyle.Bold))
                Texto(g, _titulo, fTitulo, new RectangleF(x, t.Y + 46 * k, 268 * k, 50 * k), AzulTitulo,
                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            DibujarVerMas(g, "Ver más", Fuente(12 * k, FontStyle.Bold), new PointF(x, t.Y + 100 * k), GrisVerMas, k);
        }

        private void DibujarFoto(Graphics g, RectangleF area)
        {
            if (_imagen == null)
            {
                using (LinearGradientBrush b = new LinearGradientBrush(area, Color.FromArgb(0, 96, 214), Color.FromArgb(56, 189, 248), 35f))
                    g.FillRectangle(b, area);
                return;
            }
            // Recorta la imagen para llenar el espacio sin deformarla (como "object-fit: cover")
            float escala = Math.Max(area.Width / _imagen.Width, area.Height / _imagen.Height);
            float w = _imagen.Width * escala, h = _imagen.Height * escala;
            g.DrawImage(_imagen, area.X + (area.Width - w) / 2f, area.Y + (area.Height - h) / 2f, w, h);
        }

        /// <summary>"Ver más" seguido de una flecha →.</summary>
        private static void DibujarVerMas(Graphics g, string texto, Font fuente, PointF punto, Color color, float k)
        {
            using (fuente)
            {
                Size medida = TextRenderer.MeasureText(g, texto, fuente, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, texto, fuente, Point.Round(punto), color, TextFormatFlags.NoPadding);
                float cy = punto.Y + medida.Height / 2f + 1 * k;
                float x0 = punto.X + medida.Width + 8 * k, x1 = x0 + 13 * k;
                using (Pen p = new Pen(color, Math.Max(1.2f, 1.7f * k)))
                {
                    p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
                    g.DrawLine(p, x0, cy, x1, cy);
                    g.DrawLines(p, new[] { new PointF(x1 - 5 * k, cy - 5 * k), new PointF(x1, cy), new PointF(x1 - 5 * k, cy + 5 * k) });
                }
            }
        }

        // ---------- Utilidades ----------
        private static void Texto(Graphics g, string texto, Font fuente, RectangleF area, Color color, TextFormatFlags formato)
        {
            TextRenderer.DrawText(g, texto, fuente, Rectangle.Round(area), color, formato | TextFormatFlags.NoPadding);
        }

        private static bool? _jakarta;
        private static Font Fuente(float pixeles, FontStyle estilo)
        {
            if (_jakarta == null)
            {
                _jakarta = false;
                using (InstalledFontCollection fuentes = new InstalledFontCollection())
                    foreach (FontFamily f in fuentes.Families)
                        if (f.Name == "Plus Jakarta Sans") { _jakarta = true; break; }
            }
            return new Font(_jakarta.Value ? "Plus Jakarta Sans" : "Segoe UI", Math.Max(1f, pixeles), estilo, GraphicsUnit.Pixel);
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
    }
}

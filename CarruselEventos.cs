using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Datos de un evento. Por ahora se llenan en SeccionEventos.CargarEventos();
    /// cuando exista la tabla de eventos en la base, cada fila del SELECT se convierte en uno de estos.
    /// </summary>
    public class Evento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Fecha { get; set; }        // texto tal como se muestra, ej. "13 de Octubre 2026"
        public string Horario { get; set; }      // ej. "11:00am - 8:00pm" (se muestra en el popup)
        public string DescripcionCompleta { get; set; }   // texto largo del popup; si está vacío se usa Descripcion
        public Image Imagen { get; set; }        // si es null se dibuja un degradado con ColorFondo
        public Color ColorFondo { get; set; }
        public bool Favorito { get; set; }

        public Evento(int id, string nombre, string descripcion, string fecha, Image imagen, Color colorFondo)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Fecha = fecha;
            Imagen = imagen;
            ColorFondo = colorFondo;
        }
    }

    /// <summary>
    /// Carrusel infinito de tarjetas de evento (diseño del Figma "Eventos").
    /// Avanza solo cada pocos segundos, se pausa con el mouse encima y se puede arrastrar
    /// o mover con la rueda del mouse. El corazón marca/desmarca el evento como favorito.
    /// Todo se dibuja en OnPaint, así crece o se encoge con la ventana sin perder calidad.
    /// </summary>
    public class CarruselEventos : Control
    {
        // ---------- Medidas del Figma (en píxeles del diseño de 1280 × 701) ----------
        private const float AltoDiseno = 360f;      // alto del carrusel (tarjeta + sombra)
        private const float AnchoTarjeta = 291f;
        private const float AltoTarjeta = 348f;
        private const float AltoImagen = 185f;
        private const float Separacion = 28f;
        private const float Paso = AnchoTarjeta + Separacion;
        private const float Radio = 20f;
        private const float ArribaTarjeta = 4f;

        private static readonly Color GrisTarjeta = Color.FromArgb(244, 244, 244);
        private static readonly Color Negro = Color.FromArgb(17, 17, 17);
        private static readonly Color RojoCorazon = Color.FromArgb(239, 68, 68);

        private readonly List<Evento> _eventos = new List<Evento>();
        private readonly Timer _animacion = new Timer { Interval = 15 };
        private readonly Timer _avanceAutomatico = new Timer { Interval = 3500 };

        // Desplazamiento en píxeles del diseño: 0 = el primer evento queda al centro.
        private float _desplazamiento;
        private float _destino;

        private bool _arrastrando;
        private bool _seMovio;
        private int _xInicioArrastre;
        private float _desplazamientoInicioArrastre;
        private bool _mouseEncima;
        private int _tarjetaEncima = int.MinValue;   // posición (j) de la tarjeta bajo el mouse

        /// <summary>Se dispara al hacer clic en una tarjeta (no en el corazón).</summary>
        public event EventHandler<Evento> EventoClick;

        /// <summary>Se dispara al marcar o desmarcar un evento como favorito.</summary>
        public event EventHandler<Evento> FavoritoCambiado;

        public CarruselEventos()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Size = new Size(1127, 360);

            _animacion.Tick += (s, e) => Animar();
            _avanceAutomatico.Tick += (s, e) => { if (!_mouseEncima && !_arrastrando) Mover(1); };
        }

        /// <summary>Avanza solo cada 3.5 segundos.</summary>
        [Category("PitchR"), DefaultValue(true)]
        public bool AvanceAutomatico { get; set; } = true;

        public void MostrarEventos(IEnumerable<Evento> eventos)
        {
            _eventos.Clear();
            _eventos.AddRange(eventos);
            _desplazamiento = _destino = 0;
            if (AvanceAutomatico && _eventos.Count > 1 && !DesignMode) _avanceAutomatico.Start();
            Invalidate();
        }

        /// <summary>Mueve el carrusel una o más tarjetas (positivo = a la derecha).</summary>
        public void Mover(int tarjetas)
        {
            if (_eventos.Count == 0) return;
            _destino = (float)Math.Round(_destino / Paso + tarjetas) * Paso;
            _animacion.Start();

            // Reinicia la cuenta del avance automático para que no brinque justo después de un clic
            if (_avanceAutomatico.Enabled) { _avanceAutomatico.Stop(); _avanceAutomatico.Start(); }
        }

        private float K { get { return Height / AltoDiseno; } }

        private void Animar()
        {
            float diferencia = _destino - _desplazamiento;
            if (Math.Abs(diferencia) < 0.5f)
            {
                _desplazamiento = _destino;
                _animacion.Stop();
            }
            else
            {
                _desplazamiento += diferencia * 0.14f;   // se frena al llegar (ease-out)
            }
            Invalidate();
        }

        // ---------- Posición de cada tarjeta ----------
        // La tarjeta j (puede ser negativa) muestra el evento j mod N y se dibuja en X = centro + j·paso − desplazamiento.
        private int Indice(int j)
        {
            int n = _eventos.Count;
            return ((j % n) + n) % n;
        }

        private RectangleF Tarjeta(int j)
        {
            float k = K;
            float centroX = Width / 2f - AnchoTarjeta * k / 2f;
            return new RectangleF(centroX + (j * Paso - _desplazamiento) * k, ArribaTarjeta * k, AnchoTarjeta * k, AltoTarjeta * k);
        }

        private void RangoVisible(out int primera, out int ultima)
        {
            float k = K;
            float centroX = Width / 2f - AnchoTarjeta * k / 2f;
            primera = (int)Math.Floor((-centroX / k + _desplazamiento - AnchoTarjeta) / Paso);
            ultima = (int)Math.Ceiling((Width - centroX) / k / Paso + _desplazamiento / Paso);
        }

        private int TarjetaEn(Point p)
        {
            if (_eventos.Count == 0) return int.MinValue;
            int primera, ultima;
            RangoVisible(out primera, out ultima);
            for (int j = primera; j <= ultima; j++)
                if (Tarjeta(j).Contains(p)) return j;
            return int.MinValue;
        }

        private RectangleF Corazon(RectangleF t)
        {
            float k = K;
            return new RectangleF(t.X + 230 * k, t.Y + 288 * k, 40 * k, 40 * k);
        }

        // ---------- Mouse ----------
        protected override void OnMouseEnter(EventArgs e) { _mouseEncima = true; base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e)
        {
            _mouseEncima = false;
            _tarjetaEncima = int.MinValue;
            Cursor = Cursors.Default;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || _eventos.Count == 0) return;
            _arrastrando = true;
            _seMovio = false;
            _xInicioArrastre = e.X;
            _desplazamientoInicioArrastre = _desplazamiento;
            _animacion.Stop();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_arrastrando)
            {
                int dx = e.X - _xInicioArrastre;
                if (Math.Abs(dx) > 5) _seMovio = true;
                if (_seMovio)
                {
                    _desplazamiento = _destino = _desplazamientoInicioArrastre - dx / K;
                    Cursor = Cursors.SizeWE;
                    Invalidate();
                }
                return;
            }

            int j = TarjetaEn(e.Location);
            if (j != _tarjetaEncima) { _tarjetaEncima = j; Invalidate(); }
            Cursor = j != int.MinValue ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_arrastrando) return;
            _arrastrando = false;

            if (_seMovio)
            {
                // Acomoda la tarjeta más cercana al centro
                _destino = (float)Math.Round(_desplazamiento / Paso) * Paso;
                _animacion.Start();
                Cursor = Cursors.Hand;
                return;
            }

            int j = TarjetaEn(e.Location);
            if (j == int.MinValue) return;
            Evento evento = _eventos[Indice(j)];
            if (Corazon(Tarjeta(j)).Contains(e.Location))
            {
                evento.Favorito = !evento.Favorito;
                Invalidate();
                FavoritoCambiado?.Invoke(this, evento);
            }
            else
            {
                EventoClick?.Invoke(this, evento);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Mover(e.Delta > 0 ? -1 : 1);
        }

        // ---------- Dibujo ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            if (_eventos.Count == 0) return;

            int primera, ultima;
            RangoVisible(out primera, out ultima);
            for (int j = primera; j <= ultima; j++)
                DibujarTarjeta(g, Tarjeta(j), _eventos[Indice(j)], j == _tarjetaEncima && !_arrastrando);
        }

        private void DibujarTarjeta(Graphics g, RectangleF t, Evento ev, bool encima)
        {
            float k = K;
            float radio = Radio * k;

            // Sombra (Y 4, desenfoque 4, negro 25 %); más marcada con el mouse encima
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(t.X - d * 0.4f, t.Y + d, t.Width + d * 0.8f, t.Height + d * 0.4f);
                using (GraphicsPath p = Redondeado(capa, radio + d))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(encima ? 34 : 18, 0, 0, 0)))
                    g.FillPath(b, p);
            }

            using (GraphicsPath ruta = Redondeado(t, radio))
            {
                using (SolidBrush fondo = new SolidBrush(GrisTarjeta)) g.FillPath(fondo, ruta);

                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                DibujarImagen(g, ev, new RectangleF(t.X, t.Y, t.Width, AltoImagen * k));
                g.Clip = anterior;
            }

            float x = t.X + 18 * k;
            float ancho = t.Width - 36 * k;
            using (Font fTitulo = Fuente(24 * k, FontStyle.Bold))
                Texto(g, ev.Nombre, fTitulo, new RectangleF(x, t.Y + 200 * k, ancho, 34 * k), Negro,
                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            using (Font fDescripcion = Fuente(16 * k, FontStyle.Regular))
                Texto(g, ev.Descripcion, fDescripcion, new RectangleF(x, t.Y + 238 * k, ancho, 52 * k), Negro,
                      TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            using (Font fFecha = Fuente(20 * k, FontStyle.Regular))
                Texto(g, ev.Fecha, fFecha, new RectangleF(x, t.Y + 293 * k, 210 * k, 30 * k), Negro,
                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            DibujarCorazon(g, Corazon(t), ev.Favorito, k);
        }

        internal static void DibujarImagen(Graphics g, Evento ev, RectangleF area)
        {
            if (ev.Imagen != null)
            {
                // Recorta la imagen para llenar el espacio sin deformarla (como "object-fit: cover")
                float escala = Math.Max(area.Width / ev.Imagen.Width, area.Height / ev.Imagen.Height);
                float w = ev.Imagen.Width * escala, h = ev.Imagen.Height * escala;
                g.DrawImage(ev.Imagen, area.X + (area.Width - w) / 2f, area.Y + (area.Height - h) / 2f, w, h);
                return;
            }

            // Sin imagen: degradado del color del evento con su nombre encima
            Color claro = ControlPaint.Light(ev.ColorFondo, 0.6f);
            using (LinearGradientBrush b = new LinearGradientBrush(area, ev.ColorFondo, claro, 35f))
                g.FillRectangle(b, area);
            using (Font f = Fuente(area.Height * 0.17f, FontStyle.Bold))
                Texto(g, ev.Nombre, f, RectangleF.Inflate(area, -area.Width * 0.08f, 0), Color.White,
                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        /// <summary>Corazón de contorno (o relleno rojo si es favorito).</summary>
        internal static void DibujarCorazon(Graphics g, RectangleF area, bool relleno, float k)
        {
            float w = area.Width * 0.82f, h = area.Height * 0.72f;
            float x = area.X + (area.Width - w) / 2f, y = area.Y + (area.Height - h) / 2f + h * 0.04f;

            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddBezier(x + w * 0.5f, y + h, x + w * 0.38f, y + h * 0.86f, x, y + h * 0.62f, x, y + h * 0.3f);
                p.AddBezier(x, y + h * 0.3f, x, y + h * 0.08f, x + w * 0.16f, y - h * 0.02f, x + w * 0.28f, y);
                p.AddBezier(x + w * 0.28f, y, x + w * 0.4f, y + h * 0.01f, x + w * 0.47f, y + h * 0.1f, x + w * 0.5f, y + h * 0.18f);
                p.AddBezier(x + w * 0.5f, y + h * 0.18f, x + w * 0.53f, y + h * 0.1f, x + w * 0.6f, y + h * 0.01f, x + w * 0.72f, y);
                p.AddBezier(x + w * 0.72f, y, x + w * 0.84f, y - h * 0.02f, x + w, y + h * 0.08f, x + w, y + h * 0.3f);
                p.AddBezier(x + w, y + h * 0.3f, x + w, y + h * 0.62f, x + w * 0.62f, y + h * 0.86f, x + w * 0.5f, y + h);
                p.CloseFigure();

                if (relleno)
                    using (SolidBrush b = new SolidBrush(RojoCorazon)) g.FillPath(b, p);
                using (Pen lapiz = new Pen(relleno ? RojoCorazon : Negro, Math.Max(1.5f, 2.6f * k)))
                {
                    lapiz.LineJoin = LineJoin.Round;
                    g.DrawPath(lapiz, p);
                }
            }
        }

        // ---------- Utilidades ----------
        internal static void Texto(Graphics g, string texto, Font fuente, RectangleF area, Color color, TextFormatFlags formato)
        {
            TextRenderer.DrawText(g, texto ?? "", fuente, Rectangle.Round(area), color, formato | TextFormatFlags.NoPadding);
        }

        private static bool? _jakarta;
        internal static Font Fuente(float pixeles, FontStyle estilo)
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

        internal static GraphicsPath Redondeado(RectangleF r, float radio)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animacion.Dispose();
                _avanceAutomatico.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Ventana emergente con el detalle de un evento (diseño del Figma).
    /// Se abre con PopupEvento.Mostrar(...): oscurece la pantalla de atrás y muestra la tarjeta al centro.
    /// El botón azul (o la tecla Esc) la cierra; el corazón marca/desmarca el evento como favorito.
    /// </summary>
    public class PopupEvento : Form
    {
        // Medidas del Figma
        private const float AnchoDiseno = 540f;
        private const float AltoDiseno = 650f;

        private static readonly Color Negro = Color.FromArgb(17, 17, 17);

        private readonly Evento _evento;
        private readonly float _k;
        private readonly BotonCircular _botonRegresar;
        private bool _corazonEncima;

        /// <summary>Se dispara al marcar o desmarcar el evento como favorito.</summary>
        public event EventHandler<Evento> FavoritoCambiado;

        private PopupEvento(Evento evento, float escala)
        {
            _evento = evento;
            _k = escala;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            KeyPreview = true;
            BackColor = Color.White;
            DoubleBuffered = true;
            ClientSize = new Size((int)(AnchoDiseno * _k), (int)(AltoDiseno * _k));

            using (GraphicsPath ruta = CarruselEventos.Redondeado(new RectangleF(0, 0, ClientSize.Width, ClientSize.Height), 22 * _k))
                Region = new Region(ruta);

            _botonRegresar = new BotonCircular
            {
                Icono = BotonCircular.Iconos.Regresar,
                Bounds = new Rectangle((int)(42 * _k), (int)(558 * _k), (int)(BotonCircular.TamanoRegresar * _k), (int)(BotonCircular.TamanoRegresar * _k))
            };
            _botonRegresar.Click += (s, e) => Close();
            Controls.Add(_botonRegresar);
        }

        /// <summary>Abre el popup centrado sobre la ventana que contiene a "origen".</summary>
        public static void Mostrar(Control origen, Evento evento, EventHandler<Evento> favoritoCambiado)
        {
            Form dueno = origen.FindForm();
            float escala = Math.Max(0.6f, Math.Min(origen.Width / 1280f, origen.Height / 701f));

            // Fondo oscuro semitransparente encima de la app
            using (Form oscuro = new Form())
            {
                oscuro.FormBorderStyle = FormBorderStyle.None;
                oscuro.StartPosition = FormStartPosition.Manual;
                oscuro.ShowInTaskbar = false;
                oscuro.BackColor = Color.Black;
                oscuro.Opacity = 0.45;
                Rectangle area = dueno != null ? dueno.RectangleToScreen(dueno.ClientRectangle) : Screen.FromControl(origen).WorkingArea;
                oscuro.Bounds = area;
                oscuro.Show(dueno);

                using (PopupEvento popup = new PopupEvento(evento, escala))
                {
                    if (favoritoCambiado != null) popup.FavoritoCambiado += favoritoCambiado;
                    popup.Location = new Point(area.X + (area.Width - popup.Width) / 2, area.Y + (area.Height - popup.Height) / 2);
                    popup.ShowDialog(oscuro);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) Close();
        }

        private RectangleF Corazon { get { return new RectangleF(442 * _k, 552 * _k, 52 * _k, 52 * _k); } }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool encima = Corazon.Contains(e.Location);
            if (encima != _corazonEncima)
            {
                _corazonEncima = encima;
                Cursor = encima ? Cursors.Hand : Cursors.Default;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left || !Corazon.Contains(e.Location)) return;
            _evento.Favorito = !_evento.Favorito;
            Invalidate();
            FavoritoCambiado?.Invoke(this, _evento);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);
            float k = _k;

            // Imagen con esquinas redondeadas
            RectangleF imagen = new RectangleF(46 * k, 37 * k, 449 * k, 272 * k);
            using (GraphicsPath ruta = CarruselEventos.Redondeado(imagen, 18 * k))
            {
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                CarruselEventos.DibujarImagen(g, _evento, imagen);
                g.Clip = anterior;
            }

            TextFormatFlags centrado = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

            using (Font fTitulo = CarruselEventos.Fuente(38 * k, FontStyle.Bold))
                CarruselEventos.Texto(g, _evento.Nombre, fTitulo, new RectangleF(30 * k, 328 * k, 480 * k, 52 * k), Negro, centrado);

            string descripcion = string.IsNullOrWhiteSpace(_evento.DescripcionCompleta) ? _evento.Descripcion : _evento.DescripcionCompleta;
            using (Font fDescripcion = CarruselEventos.Fuente(19 * k, FontStyle.Regular))
                CarruselEventos.Texto(g, descripcion, fDescripcion, new RectangleF(70 * k, 386 * k, 410 * k, 72 * k), Negro,
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            using (Font fFecha = CarruselEventos.Fuente(23 * k, FontStyle.Bold))
            {
                CarruselEventos.Texto(g, _evento.Fecha, fFecha, new RectangleF(30 * k, 466 * k, 480 * k, 38 * k), Negro, centrado);
                if (!string.IsNullOrWhiteSpace(_evento.Horario))
                    CarruselEventos.Texto(g, _evento.Horario, fFecha, new RectangleF(30 * k, 521 * k, 480 * k, 38 * k), Negro, centrado);
            }

            CarruselEventos.DibujarCorazon(g, Corazon, _evento.Favorito, k * 1.15f);

            // Borde gris muy suave alrededor de la tarjeta
            using (GraphicsPath borde = CarruselEventos.Redondeado(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 22 * k))
            using (Pen p = new Pen(Color.FromArgb(226, 230, 236), Math.Max(1f, k)))
                g.DrawPath(p, borde);
        }
    }
}

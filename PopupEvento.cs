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
        private readonly PictureBox _botonRegresar;
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

            // El mismo botón de regresar de Iniciar sesión y Crear cuenta (PictureBox transparente)
            _botonRegresar = new PictureBox
            {
                BackColor = Color.Transparent,
                Bounds = new Rectangle((int)(42 * _k), (int)(558 * _k), (int)(BotonCircular.TamanoRegresar * _k), (int)(BotonCircular.TamanoRegresar * _k))
            };
            // Usa la imagen del botón de regresar guardada en el .resx de Iniciar sesión
            BotonCircular.PonerImagenRegresar(_botonRegresar);   // la imagen del botón de Iniciar sesión
            BotonCircular.HacerBotonRegresar(_botonRegresar);
            _botonRegresar.Click += (s, e) => Close();
            Controls.Add(_botonRegresar);

            // Nombre, descripción, fecha y horario van en una zona desplazable:
            // si la descripción es larga aparece la barra (o se usa la rueda del mouse) en vez de cortarse.
            _desplazable = new PopupTerminos.PanelDesplazable
            {
                AutoScroll = true,
                BackColor = Color.White,
                Bounds = Rectangle.Round(new RectangleF(30 * _k, 318 * _k, 488 * _k, 228 * _k))
            };
            int ancho = _desplazable.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            _desplazable.Controls.Add(new TextoEvento(_evento, _k, ancho));
            _desplazable.MouseEnter += (s, e) => _desplazable.Focus();
            Controls.Add(_desplazable);
            Shown += (s, e) => _desplazable.Focus();
        }

        private readonly PopupTerminos.PanelDesplazable _desplazable;

        /// <summary>Dibuja nombre, descripción, fecha y horario del evento. Su alto es el de todo el texto.</summary>
        private class TextoEvento : Control
        {
            private readonly Evento _evento;
            private readonly float _k;
            private readonly Font _fTitulo, _fDescripcion, _fFecha;
            private readonly string _descripcion;
            private int _altoTitulo, _altoDescripcion, _altoFecha;

            private const TextFormatFlags Centrado = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            private const TextFormatFlags Parrafo = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;

            public TextoEvento(Evento evento, float k, int ancho)
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                BackColor = Color.White;
                _evento = evento;
                _k = k;
                _fTitulo = CarruselEventos.Fuente(38 * k, FontStyle.Bold);
                _fDescripcion = CarruselEventos.Fuente(19 * k, FontStyle.Regular);
                _fFecha = CarruselEventos.Fuente(23 * k, FontStyle.Bold);
                _descripcion = string.IsNullOrWhiteSpace(evento.DescripcionCompleta) ? evento.Descripcion : evento.DescripcionCompleta;

                _altoTitulo = TextRenderer.MeasureText(evento.Nombre ?? "", _fTitulo, new Size(ancho, 0), Centrado).Height;
                _altoDescripcion = TextRenderer.MeasureText(_descripcion ?? "", _fDescripcion, new Size(AnchoDescripcion(ancho), 0), Parrafo).Height;
                _altoFecha = TextRenderer.MeasureText("Bgjy", _fFecha, Size.Empty, Centrado).Height;
                Size = new Size(ancho, Posiciones(out _, out _, out _));
            }

            private int AnchoDescripcion(int ancho) { return ancho - (int)(60 * _k); }

            // Calcula dónde va cada texto y regresa el alto total
            private int Posiciones(out int yDescripcion, out int yFecha, out int yHorario)
            {
                yDescripcion = (int)(_altoTitulo + 8 * _k);
                yFecha = (int)(yDescripcion + _altoDescripcion + 12 * _k);
                yHorario = (int)(yFecha + _altoFecha + 14 * _k);
                bool conHorario = !string.IsNullOrWhiteSpace(_evento.Horario);
                return (int)((conHorario ? yHorario + _altoFecha : yFecha + _altoFecha) + 10 * _k);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                int yDescripcion, yFecha, yHorario;
                Posiciones(out yDescripcion, out yFecha, out yHorario);
                int colchon = (int)(10 * _k);   // para que no se corten la "g", "p", "y"

                TextRenderer.DrawText(g, _evento.Nombre ?? "", _fTitulo, new Rectangle(0, 0, Width, _altoTitulo + colchon), Negro, Centrado);
                TextRenderer.DrawText(g, _descripcion ?? "", _fDescripcion,
                    new Rectangle((int)(30 * _k), yDescripcion, AnchoDescripcion(Width), _altoDescripcion + colchon), Negro, Parrafo);
                TextRenderer.DrawText(g, _evento.Fecha ?? "", _fFecha, new Rectangle(0, yFecha, Width, _altoFecha + colchon), Negro, Centrado);
                if (!string.IsNullOrWhiteSpace(_evento.Horario))
                    TextRenderer.DrawText(g, _evento.Horario, _fFecha, new Rectangle(0, yHorario, Width, _altoFecha + colchon), Negro, Centrado);
            }

            protected override void OnMouseDown(MouseEventArgs e) { Parent?.Focus(); base.OnMouseDown(e); }

            protected override void Dispose(bool disposing)
            {
                if (disposing) { _fTitulo.Dispose(); _fDescripcion.Dispose(); _fFecha.Dispose(); }
                base.Dispose(disposing);
            }
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

            // Nombre, descripción, fecha y horario los dibuja TextoEvento dentro de la zona desplazable

            CarruselEventos.DibujarCorazon(g, Corazon, _evento.Favorito, k * 1.15f);

            // Borde gris muy suave alrededor de la tarjeta
            using (GraphicsPath borde = CarruselEventos.Redondeado(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 22 * k))
            using (Pen p = new Pen(Color.FromArgb(226, 230, 236), Math.Max(1f, k)))
                g.DrawPath(p, borde);
        }
    }
}

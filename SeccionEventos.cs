using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Sección "Eventos" de HomePage (es un panel, no una ventana).
    /// Título, subtítulo y un carrusel con las tarjetas de los eventos.
    /// </summary>
    public partial class SeccionEventos : UserControl
    {
        public SeccionEventos()
        {
            InitializeComponent();

            // Guarda posiciones y fuentes del diseñador (1280 × 701) para escalarlas con la ventana
            foreach (Control c in Controls)
            {
                _posicionesOriginales[c] = c.Bounds;
                _fuentesOriginales[c] = c.Font;
            }
            Resize += (s, e) => AjustarATamano();

            Carrusel.EventoClick += Evento_Click;
            AnteriorBoton.Click += (s, e) => Carrusel.Mover(-1);
            SiguienteBoton.Click += (s, e) => Carrusel.Mover(1);
            _eventos = CargarEventos();
            Carrusel.MostrarEventos(_eventos);
        }

        public const int IdPitchRFest = 3;
        private readonly List<Evento> _eventos;

        /// <summary>Abre el popup de un evento por su Id (lo usa el "Ver más" del banner de Emprendimientos).</summary>
        public void AbrirEvento(int idEvento)
        {
            Evento evento = _eventos.Find(ev => ev.Id == idEvento);
            if (evento == null) return;
            // Espera a que la sección ya esté en pantalla para que el popup tome bien el tamaño
            BeginInvoke((Action)(() => Evento_Click(this, evento)));
        }

        // ==================== EVENTOS ====================
        // Por ahora la lista es fija. Cuando exista la tabla de eventos en la base,
        // aquí va el SELECT y cada fila se convierte en un Evento (igual que en SeccionNoticias).
        // Las imágenes vienen de SeccionEventos.resx (van dentro del programa, no hace falta tener las fotos).
        // Si alguna no se encuentra, se dibuja un degradado con el color del evento.
        /// <summary>Lee una imagen guardada en SeccionEventos.resx (null si no existe).</summary>
        private static Image Imagen(string nombre)
        {
            try
            {
                var recursos = new System.ComponentModel.ComponentResourceManager(typeof(SeccionEventos));
                return recursos.GetObject(nombre) as Image;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<Evento> CargarEventos()
        {
            return new List<Evento>
            {
                new Evento(1, "Farmers Market", "Mercado agricultor en Distrito 1.", "13 de Octubre 2026",
                           Imagen("FarmersMarket"), Color.FromArgb(96, 119, 74))
                {
                    DescripcionCompleta = "Mercado agricultor en Distrito 1 con productos frescos de productores locales.",
                    Horario = "9:00am - 3:00pm"
                },
                new Evento(2, "Japan Market", "Mercado de emprendimientos locales.", "24 de Octubre 2026",
                           Imagen("JapanMarket"), Color.FromArgb(240, 74, 120))
                {
                    DescripcionCompleta = "Mercado de emprendimientos locales con temáticas asiáticas y japonesas.",
                    Horario = "11:00am - 8:00pm"
                },
                new Evento(IdPitchRFest, "PitchR Fest", "Evento de presentación de emprendimientos.", "3 de Enero de 2027",
                           Imagen("PitchRFest"), Color.FromArgb(56, 160, 230))
                {
                    DescripcionCompleta = "Conoce los proyectos en vivo: presentaciones, demostraciones y conecta cara a cara con los fundadores antes de invertir.",
                    Horario = "10:00am - 6:00pm"
                },
            };
        }

        // Clic en una tarjeta: abre el popup con el detalle. Si cambian el favorito ahí, se actualiza el carrusel.
        private void Evento_Click(object sender, Evento evento)
        {
            PopupEvento.Mostrar(this, evento, (s, e) => Carrusel.Invalidate());
            Carrusel.Invalidate();
        }

        #region Sección adaptable (no es necesario modificar esta parte)

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();

        private void AjustarATamano()
        {
            if (Width < 50 || Height < 50) return;
            float escala = Math.Min(Width / 1280f, Height / 701f);
            float margenX = (Width - 1280f * escala) / 2f;
            float margenY = (Height - 701f * escala) / 2f;
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Rectangle r = par.Value;
                par.Key.Bounds = new Rectangle((int)(r.X * escala + margenX), (int)(r.Y * escala + margenY),
                                               (int)(r.Width * escala), (int)(r.Height * escala));
                Font f = _fuentesOriginales[par.Key];
                par.Key.Font = new Font(f.FontFamily, Math.Max(1f, f.Size * escala), f.Style, GraphicsUnit.Pixel);
            }
        }

        #endregion
    }
}

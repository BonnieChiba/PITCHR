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

            // Cada vez que se abre Eventos (HomePage la vuelve a poner en pantalla), PitchR Fest queda al centro
            Carrusel.CentrarEn(IdPitchRFest);
            ParentChanged += (s, e) => { if (Parent != null) Carrusel.CentrarEn(IdPitchRFest); };
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
                new Evento(1, "Premio Emprende", "Premiación estatal a startups.", "8 Dic 2026",
                           Imagen("PremioEmprende"), Color.FromArgb(124, 58, 170))
                {
                    DescripcionCompleta = "Ceremonia de premiación del Premio Chihuahua Emprende 2026, organizado por la Secretaría de Innovación y Desarrollo Económico (SIDE). Premia a los mejores emprendimientos del estado en 5 categorías: Visionario, Tradicional, Mujer Emprendedora, Sostenible e Impacto Social, con $100,000 al primer lugar y $50,000 al segundo en cada una.",
                    Horario = "Horario por confirmar",
                    Lugar = "Chihuahua capital (sede por confirmar)"
                },
                new Evento(2, "Tertulia Navideña", "Bazar navideño local.", "5 - 21 Dic 2026",
                           Imagen("TertuliaNavidena"), Color.FromArgb(220, 38, 48))
                {
                    DescripcionCompleta = "Bazar navideño organizado por La Tertulia Bazar con emprendimientos y marcas locales. Ideal para comprar regalos y apoyar negocios de Chihuahua. Días: 5, 6, 12, 13, 19, 20 y 21 de diciembre.",
                    Horario = "1:00pm - 8:00pm",
                    Lugar = "Plaza Cantera Cinco, Av. de la Cantera 9301, La Haciendita, Chihuahua"
                },
                new Evento(IdPitchRFest, "PitchR Fest", "Evento de presentación de emprendimientos.", "3 de Enero de 2027",
                           Imagen("PitchRFest"), Color.FromArgb(56, 160, 230))
                {
                    DescripcionCompleta = "Conoce los proyectos en vivo: presentaciones, demostraciones y conecta cara a cara con los fundadores antes de dar el siguiente paso.",
                    Horario = "10:00am - 6:00pm",
                    Lugar = "UTCH BIS, Carretera Chihuahua-Aldama Km 3, Colinas del León, Chihuahua"
                },
                new Evento(4, "Curso Cobranza", "Taller de COPARMEX.", "3 - 4 Dic 2026",
                           Imagen("CursoCobranza"), Color.FromArgb(22, 128, 74))
                {
                    DescripcionCompleta = "Curso-taller \"Crédito y Cobranza\" de COPARMEX Chihuahua: estrategias para reducir la morosidad, mejorar la recuperación de cartera y planear el próximo año. Imparte Lic. Marco Ruvalcaba. Costo: $2,500 - $3,828.",
                    Horario = "9:00am - 2:00pm",
                    Lugar = "COPARMEX Chihuahua, C. Juan Aldama 1800, Zona Centro, Chihuahua"
                },
                new Evento(5, "Curso Finanzas", "Taller de COPARMEX.", "8 - 9 Dic 2026",
                           Imagen("FinanzasNoFinancieros"), Color.FromArgb(16, 96, 60))
                {
                    DescripcionCompleta = "Curso \"Finanzas para no Financieros\" de COPARMEX Chihuahua sobre presupuesto y flujo de efectivo para emprendedores y dueños de negocio. Instructora: CP Angelina Solórzano. Costo: $2,650 - $4,060.",
                    Horario = "9:00am - 2:00pm",
                    Lugar = "COPARMEX Chihuahua, C. Juan Aldama 1800, Zona Centro, Chihuahua"
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

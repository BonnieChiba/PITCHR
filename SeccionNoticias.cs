using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Sección "Noticias" de HomePage (es un panel, no una ventana). Versión SIN base de datos.
    /// Las 4 noticias (título, categoría, fecha, resumen, imagen y enlace) están puestas en el diseñador,
    /// así se ven igual en el diseñador y al correr la app.
    /// Para cambiar una: abre el diseñador, da clic en la tarjeta y cambia sus propiedades del grupo "PitchR".
    /// </summary>
    public partial class SeccionNoticias : UserControl
    {
        public SeccionNoticias()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
        }

        private void SeccionNoticias_Load(object sender, EventArgs e)
        {
            // No se carga nada: las noticias ya vienen del diseñador.
        }

        // ==================== VER MÁS ====================
        // Clic en una noticia (o en su "Ver más"): abre la página original en el navegador.
        private void Noticia_Click(object sender, EventArgs e)
        {
            string enlace = ((TarjetaNoticia)sender).Enlace;
            if (string.IsNullOrWhiteSpace(enlace)) return;

            try
            {
                Process.Start(enlace);   // abre el navegador predeterminado
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el enlace:\n" + ex.Message, "Noticias", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #region Diseño del Figma y sección adaptable (no es necesario modificar esta parte)

        // Medidas del frame "News" en Figma sin el encabezado (que ahora está en HomePage): 1280 × 701.
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 701f;

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private float _escala = 1f;

        private void PrepararDiseno()
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           .SetValue(BackgroundNoticias, true, null);
            Resize += (s, e) => AjustarATamano();

            // Se guardan las posiciones y fuentes del diseñador (1280 × 701) para escalarlas después.
            GuardarOriginales(BackgroundNoticias);
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

        /// <summary>Hace crecer o encoger todo para llenar la sección: crece parejo (sin deformarse) y centrado.</summary>
        private void AjustarATamano()
        {
            Size tamano = ClientSize;
            if (tamano.Width < 50 || tamano.Height < 50) return;

            _escala = Math.Min(tamano.Width / AnchoFigma, tamano.Height / AltoFigma);
            float margenX = (tamano.Width - AnchoFigma * _escala) / 2f;
            float margenY = (tamano.Height - AltoFigma * _escala) / 2f;

            SuspendLayout();
            BackgroundNoticias.Bounds = new Rectangle(0, 0, tamano.Width, tamano.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BackgroundNoticias;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                if (!(c is TarjetaNoticia)) c.Font = EscalarFuente(_fuentesOriginales[c], _escala);   // las tarjetas escalan solas
            }
            ResumeLayout(true);
            BackgroundNoticias.Invalidate(true);
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
                fuente = new Font(nombre, Math.Max(1f, pixeles), original.Style, GraphicsUnit.Pixel);
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

        #endregion
    }
}

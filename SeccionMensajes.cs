using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Sección "Mensajes" de HomePage (es un panel, no una ventana). Por ahora solo muestra un aviso;
    /// cuando la diseñen, su contenido se agrega aquí igual que en SeccionNoticias.
    /// </summary>
    public partial class SeccionMensajes : UserControl
    {
        public SeccionMensajes()
        {
            InitializeComponent();

            // Guarda posiciones y fuentes del diseñador (1280 × 701) para escalarlas con la ventana
            foreach (Control c in Controls)
            {
                _posicionesOriginales[c] = c.Bounds;
                _fuentesOriginales[c] = c.Font;
            }
            Resize += (s, e) => AjustarATamano();
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Ventana emergente con los Términos y Condiciones (se abre desde "ver Terminos y condiciones" al crear cuenta).
    /// Oscurece la pantalla de atrás y muestra la tarjeta al centro, igual que PopupEvento.
    ///  - "Aceptar": cierra y regresa true (la pantalla marca la casilla de términos).
    ///  - Botón de regresar o Esc: cierra sin aceptar.
    /// </summary>
    public class PopupTerminos : Form
    {
        // Medidas del diseño
        private const float AnchoDiseno = 504f;
        private const float AltoDiseno = 654f;

        private static readonly Color Negro = Color.FromArgb(17, 17, 17);
        private static readonly Color Azul = Color.FromArgb(0, 96, 214);
        private static readonly Color AzulEncima = Color.FromArgb(0, 80, 186);

        private static readonly string[,] Clausulas =
        {
            { "1. Privacidad y Tratamiento de la Información.",
              "Agradecemos la confianza depositada en la plataforma. Nos comprometemos a mantener la confidencialidad de tus datos personales y/o empresariales. La información proporcionada al registrarte (como nombre, correo o RFC) será utilizada únicamente para crear, administrar y autenticar tu cuenta, así como para facilitar la interacción entre usuarios dentro de la plataforma. Tu información no será vendida ni cedida a terceros." },
            { "2. Transacciones e Inversiones Fuera de la Aplicación.",
              "La plataforma opera únicamente como una vitrina digital y espacio de networking para conectar usuarios. En ningún caso la plataforma procesa, recibe, gestiona ni custodia dinero, capital ni fondos de inversión. Toda negociación, acuerdo, contrato o transferencia financiera se lleva a cabo 100% fuera de la aplicación y de forma totalmente independiente a nuestros sistemas." },
            { "3. Deslinde de Responsabilidad y Fraudes.",
              "La plataforma no se hace responsable por pérdidas financieras, incumplimientos, fraudes o estafas que resulten de acuerdos alcanzados entre usuarios fuera de la aplicación. Es responsabilidad exclusiva de cada usuario realizar la debida investigación (Due Diligence), validación legal y financiera antes de concretar cualquier acuerdo o entrega de dinero." },
            { "4. Verificación de RFC e Identidad.",
              "Como medida de seguridad, la plataforma puede solicitar la validación del Registro Federal de Contribuyentes (RFC) o documentos oficiales. Estas validaciones se realizan de buena fe; la plataforma no garantiza la autenticidad infalible de dicha documentación ni asume responsabilidad si un usuario proporciona información apócrifa, alterada o falsa." },
            { "5. Uso Aceptable.",
              "Queda prohibido proporcionar información falsa, suplantar identidades o realizar actividades ilícitas dentro de la plataforma. Nos reservamos el derecho de suspender o cancelar las cuentas que incumplan estas normas." },
        };

        private readonly float _k;
        private readonly PictureBox _botonRegresar;
        private bool _aceptarEncima;

        private PopupTerminos(float escala)
        {
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

            // El mismo botón de regresar de toda la app, arriba a la izquierda
            int lado = (int)(44 * _k);
            _botonRegresar = new PictureBox
            {
                BackColor = Color.Transparent,
                Bounds = new Rectangle((int)(16 * _k), (int)(16 * _k), lado, lado)
            };
            BotonCircular.PonerImagenRegresar(_botonRegresar);
            BotonCircular.HacerBotonRegresar(_botonRegresar);
            _botonRegresar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(_botonRegresar);

            // Zona del texto con barra de desplazamiento (la rueda del mouse también funciona)
            _desplazable = new PanelDesplazable
            {
                AutoScroll = true,
                BackColor = Color.White,
                Bounds = Rectangle.Round(new RectangleF(40 * _k, 100 * _k, 430 * _k, 440 * _k))
            };
            int anchoTexto = _desplazable.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (int)(10 * _k);
            _desplazable.Controls.Add(new TextoClausulas(TamanoLetra * _k, anchoTexto) { Location = new Point((int)(4 * _k), 0) });
            _desplazable.MouseEnter += (s, e) => _desplazable.Focus();
            Controls.Add(_desplazable);
            Shown += (s, e) => _desplazable.Focus();
        }

        private const float TamanoLetra = 17f;   // letra de las cláusulas (en píxeles del diseño)
        private readonly PanelDesplazable _desplazable;

        /// <summary>Panel con AutoScroll que puede tomar el foco, para que la rueda del mouse lo mueva.</summary>
        internal class PanelDesplazable : Panel   // también lo usa PopupEvento
        {
            public PanelDesplazable()
            {
                SetStyle(ControlStyles.Selectable, true);
                DoubleBuffered = true;
            }

            protected override void OnMouseDown(MouseEventArgs e) { Focus(); base.OnMouseDown(e); }
        }

        /// <summary>Dibuja todas las cláusulas (título en negritas + texto). Su alto es el de todo el texto.</summary>
        private class TextoClausulas : Control
        {
            private const TextFormatFlags Formato = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            private readonly Font _negrita;
            private readonly Font _normal;
            private readonly float _separacion;

            public TextoClausulas(float tamano, int ancho)
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                BackColor = Color.White;
                _negrita = CarruselEventos.Fuente(tamano, FontStyle.Bold);
                _normal = CarruselEventos.Fuente(tamano, FontStyle.Regular);
                _separacion = tamano * 1.1f;

                float alto = 0;
                foreach (int[] h in Altos(ancho)) alto += h[0] + 2 + h[1] + _separacion;
                Size = new Size(ancho, (int)Math.Ceiling(alto + tamano));   // un poco de aire al final
            }

            // Alto del título y del texto de cada cláusula
            private System.Collections.Generic.IEnumerable<int[]> Altos(int ancho)
            {
                for (int i = 0; i < Clausulas.GetLength(0); i++)
                    yield return new[]
                    {
                        TextRenderer.MeasureText(Clausulas[i, 0], _negrita, new Size(ancho, 0), Formato).Height,
                        TextRenderer.MeasureText(Clausulas[i, 1], _normal, new Size(ancho, 0), Formato).Height
                    };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
                float y = 0;
                int i = 0;
                foreach (int[] h in Altos(Width))
                {
                    // +colchón abajo para que no se corten la "g", "p", "y"
                    TextRenderer.DrawText(e.Graphics, Clausulas[i, 0], _negrita, new Rectangle(0, (int)y, Width, h[0] + 8), Negro, Formato);
                    y += h[0] + 2;
                    TextRenderer.DrawText(e.Graphics, Clausulas[i, 1], _normal, new Rectangle(0, (int)y, Width, h[1] + 8), Negro, Formato);
                    y += h[1] + _separacion;
                    i++;
                }
            }

            protected override void OnMouseDown(MouseEventArgs e) { Parent?.Focus(); base.OnMouseDown(e); }

            protected override void Dispose(bool disposing)
            {
                if (disposing) { _negrita.Dispose(); _normal.Dispose(); }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Abre los términos centrados sobre la ventana de "origen".
        /// Regresa true si la persona presionó "Aceptar".
        /// </summary>
        public static bool Mostrar(Control origen)
        {
            Form dueno = origen.FindForm();
            Rectangle area = dueno != null ? dueno.RectangleToScreen(dueno.ClientRectangle) : Screen.FromControl(origen).WorkingArea;
            // Ocupa como máximo el 90 % del alto de la ventana
            float escala = Math.Max(0.7f, Math.Min(1.4f, Math.Min(area.Height * 0.9f / AltoDiseno, area.Width * 0.9f / AnchoDiseno)));

            using (Form oscuro = new Form())
            {
                oscuro.FormBorderStyle = FormBorderStyle.None;
                oscuro.StartPosition = FormStartPosition.Manual;
                oscuro.ShowInTaskbar = false;
                oscuro.BackColor = Color.Black;
                oscuro.Opacity = 0.45;
                oscuro.Bounds = area;
                oscuro.Show(dueno);

                using (PopupTerminos popup = new PopupTerminos(escala))
                {
                    popup.Location = new Point(area.X + (area.Width - popup.Width) / 2, area.Y + (area.Height - popup.Height) / 2);
                    return popup.ShowDialog(oscuro) == DialogResult.OK;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
        }

        // ---------- Botón Aceptar (dibujado) ----------
        private RectangleF Aceptar { get { return new RectangleF(162 * _k, 560 * _k, 177 * _k, 48 * _k); } }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool encima = Aceptar.Contains(e.Location);
            if (encima != _aceptarEncima)
            {
                _aceptarEncima = encima;
                Cursor = encima ? Cursors.Hand : Cursors.Default;
                Invalidate(Rectangle.Ceiling(RectangleF.Inflate(Aceptar, 8 * _k, 8 * _k)));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_aceptarEncima) { _aceptarEncima = false; Cursor = Cursors.Default; Invalidate(); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left || !Aceptar.Contains(e.Location)) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        // ---------- Dibujo ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);
            float k = _k;

            using (Font fTitulo = CarruselEventos.Fuente(32 * k, FontStyle.Bold))
                CarruselEventos.Texto(g, "Términos y Condiciones", fTitulo, new RectangleF(60 * k, 38 * k, 384 * k, 48 * k), Negro,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);

            // Botón "Aceptar": píldora azul con sombra
            RectangleF b = Aceptar;
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                using (GraphicsPath p = CarruselEventos.Redondeado(new RectangleF(b.X - d * 0.4f, b.Y + d, b.Width + d * 0.8f, b.Height), b.Height / 2f))
                using (SolidBrush s = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                    g.FillPath(s, p);
            }
            using (GraphicsPath p = CarruselEventos.Redondeado(b, b.Height / 2f))
            using (SolidBrush s = new SolidBrush(_aceptarEncima ? AzulEncima : Azul))
                g.FillPath(s, p);
            using (Font fBoton = CarruselEventos.Fuente(24 * k, FontStyle.Bold))
                CarruselEventos.Texto(g, "Aceptar", fBoton, b, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);

            using (GraphicsPath borde = CarruselEventos.Redondeado(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 22 * k))
            using (Pen p = new Pen(Color.FromArgb(226, 230, 236), Math.Max(1f, k)))
                g.DrawPath(p, borde);
        }
    }
}

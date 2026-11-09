using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Botón redondo con sombra y un ícono azul: flecha izquierda/derecha (carrusel) o "regresar" (popup).
    /// Se dibuja en OnPaint, así crece o se encoge con la ventana sin perder calidad.
    /// </summary>
    public class BotonCircular : Control
    {
        public enum Iconos { Izquierda, Derecha, Regresar }

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);   // #0060D6
        private Iconos _icono = Iconos.Derecha;
        private Color _fondo = Color.White;
        private bool _mouseEncima;

        public BotonCircular()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
            Size = new Size(50, 50);
        }

        [Category("PitchR")] public Iconos Icono { get { return _icono; } set { _icono = value; Invalidate(); } }
        [Category("PitchR")] public Color ColorCirculo { get { return _fondo; } set { _fondo = value; Invalidate(); } }

        protected override void OnMouseEnter(EventArgs e) { _mouseEncima = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _mouseEncima = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Parent != null ? Parent.BackColor : Color.White);

            if (_icono == Iconos.Regresar)
            {
                DibujarRegresar(g, ClientRectangle, _mouseEncima);
                return;
            }

            float k = Width / 50f;
            RectangleF circulo = new RectangleF(3 * k, 1 * k, Width - 6 * k - 1, Height - 6 * k - 1);

            // Sombra suave (Y 2, desenfoque 4)
            for (int i = 3; i >= 1; i--)
            {
                float d = i * k;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(_mouseEncima ? 26 : 18, 0, 0, 0)))
                    g.FillEllipse(b, circulo.X - d * 0.4f, circulo.Y + d, circulo.Width + d * 0.8f, circulo.Height + d * 0.3f);
            }

            Color fondo = _mouseEncima ? Color.FromArgb(232, 240, 252) : _fondo;
            using (SolidBrush b = new SolidBrush(fondo)) g.FillEllipse(b, circulo);
            using (Pen borde = new Pen(Color.FromArgb(226, 230, 236), Math.Max(1f, 0.8f * k))) g.DrawEllipse(borde, circulo);

            float cx = circulo.X + circulo.Width / 2f, cy = circulo.Y + circulo.Height / 2f;
            float s = circulo.Width / 44f;   // tamaño del ícono relativo al círculo
            using (Pen p = new Pen(AzulPitcher, Math.Max(1.5f, 3.2f * s)))
            {
                p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
                switch (_icono)
                {
                    case Iconos.Izquierda:
                        g.DrawLines(p, new[] { new PointF(cx + 4 * s, cy - 9 * s), new PointF(cx - 5 * s, cy), new PointF(cx + 4 * s, cy + 9 * s) });
                        break;
                    case Iconos.Derecha:
                        g.DrawLines(p, new[] { new PointF(cx - 4 * s, cy - 9 * s), new PointF(cx + 5 * s, cy), new PointF(cx - 4 * s, cy + 9 * s) });
                        break;
                }
            }
        }

        // ==================== BOTÓN DE REGRESAR (el mismo en toda la app) ====================
        /// <summary>Tamaño de todos los botones de regresar en el diseño (1280 px de ancho).</summary>
        public const int TamanoRegresar = 52;

        /// <summary>
        /// Dibuja el botón de regresar de Pitcher: círculo gris claro con sombra suave y la flecha azul curva.
        /// Lo usan todas las pantallas (Inicia aquí, Iniciar sesión, Crear cuenta, perfiles, producto, marca y el popup de eventos)
        /// para que todos se vean iguales. No limpia el fondo: así funciona encima de imágenes o colores.
        /// </summary>
        public static void DibujarRegresar(Graphics g, Rectangle area, bool encima)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float lado = Math.Min(area.Width, area.Height);
            float k = lado / TamanoRegresar;
            float d = lado - 8 * k;   // deja espacio para la sombra
            RectangleF circulo = new RectangleF(area.X + (area.Width - d) / 2f, area.Y + (area.Height - lado) / 2f + 2 * k, d, d);

            // Sombra (Y 3, desenfoque 4)
            for (int i = 4; i >= 1; i--)
            {
                float s0 = i * k;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(encima ? 22 : 16, 0, 0, 0)))
                    g.FillEllipse(b, circulo.X - s0 * 0.35f, circulo.Y + s0 * 0.9f, circulo.Width + s0 * 0.7f, circulo.Height + s0 * 0.3f);
            }

            using (SolidBrush b = new SolidBrush(encima ? Color.FromArgb(226, 234, 246) : Color.FromArgb(241, 244, 247)))
                g.FillEllipse(b, circulo);
            using (Pen borde = new Pen(Color.FromArgb(222, 227, 233), Math.Max(1f, 0.8f * k)))
                g.DrawEllipse(borde, circulo);

            // Flecha de "responder": punta a la izquierda y una curva gruesa que baja a la derecha
            float cx = circulo.X + circulo.Width / 2f + 1 * k, cy = circulo.Y + circulo.Height / 2f;
            float s = circulo.Width / 44f;
            using (SolidBrush relleno = new SolidBrush(AzulPitcher))
                g.FillPolygon(relleno, new[] { new PointF(cx - 12 * s, cy - 4 * s), new PointF(cx - 2 * s, cy - 13 * s), new PointF(cx - 2 * s, cy + 5 * s) });
            using (Pen p = new Pen(AzulPitcher, Math.Max(2f, 5.5f * s)))
            {
                p.EndCap = LineCap.Round;
                g.DrawBezier(p, new PointF(cx - 3 * s, cy - 4 * s), new PointF(cx + 8 * s, cy - 4 * s),
                             new PointF(cx + 12 * s, cy + 2 * s), new PointF(cx + 10 * s, cy + 11 * s));
            }
        }

        /// <summary>
        /// Convierte cualquier control (PictureBox o Button) en el botón de regresar de Pitcher:
        /// quita su imagen/estilo propio, lo dibuja con DibujarRegresar y le pone efecto al pasar el mouse.
        /// </summary>
        public static void HacerBotonRegresar(Control control, Color fondo = default(Color))
        {
            PictureBox foto = control as PictureBox;
            if (foto != null) foto.Image = null;
            Button boton = control as Button;
            if (boton != null)
            {
                boton.FlatStyle = FlatStyle.Flat;
                boton.FlatAppearance.BorderSize = 0;
                boton.Text = "";
                // Solo la parte redonda es del botón: las esquinas dejan ver lo que hay atrás
                EventHandler recortar = (s, e) =>
                {
                    if (boton.Width < 2 || boton.Height < 2) return;
                    using (GraphicsPath ruta = new GraphicsPath())
                    {
                        ruta.AddEllipse(-1, -1, boton.Width + 2, boton.Height + 2);
                        boton.Region = new Region(ruta);
                    }
                };
                boton.Resize += recortar;
                recortar(boton, EventArgs.Empty);
            }
            control.Cursor = Cursors.Hand;

            bool encima = false;
            control.MouseEnter += (s, e) => { encima = true; control.Invalidate(); };
            control.MouseLeave += (s, e) => { encima = false; control.Invalidate(); };
            control.Paint += (s, e) =>
            {
                // Un botón pinta su propio fondo; se cubre con el color de atrás antes de dibujar el círculo
                if (boton != null)
                {
                    Color detras = !fondo.IsEmpty ? fondo : boton.Parent != null ? boton.Parent.BackColor : Color.White;
                    if (detras.A < 255) detras = Color.White;
                    e.Graphics.Clear(detras);
                }
                DibujarRegresar(e.Graphics, control.ClientRectangle, encima);
            };
        }
    }
}

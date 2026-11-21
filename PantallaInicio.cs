using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Pantalla de inicio animada (splash) que se muestra antes de HomePage.
    /// Se salta con un clic o cualquier tecla.
    /// </summary>
    public class PantallaInicio : Form
    {
        // ==================== CONFIGURACIÓN ====================
        private const string Lema = "Conectando futuros";
        private const int Ancho = 760;
        private const int Alto = 420;
        private const float AnchoLogo = 470f;     // ancho final del logo en pantalla
        private const float CentroLogoY = 146f;   // altura del centro del logo

        // Línea de tiempo (milisegundos)
        private const float T_EntradaCirculo = 0f, D_EntradaCirculo = 750f;
        private const float T_Onda = 350f, D_Onda = 900f;
        private const float T_Texto = 900f, D_Texto = 850f;
        private const float T_Brillo = 1750f, D_Brillo = 650f;
        private const float T_Lema = 1950f, D_Lema = 600f;
        private const float T_Barra = 2100f, D_Barra = 1500f;
        private const float T_Salida = 3750f, D_Salida = 400f;

        // Colores del logo
        private static readonly Color Azul = Color.FromArgb(30, 128, 255);
        private static readonly Color AzulClaro = Color.FromArgb(64, 166, 255);
        private static readonly Color Naranja = Color.FromArgb(248, 112, 51);
        private static readonly Color GrisTexto = Color.FromArgb(70, 78, 92);

        // Recortes del logo original (2000x600): "PITCH" y el círculo con la R
        private static readonly Rectangle RecorteTexto = new Rectangle(110, 150, 1306, 305);
        private static readonly Rectangle RecorteCirculo = new Rectangle(1415, 40, 520, 520);
        private const float CentroCirculoOrigenX = 1675f, CentroCirculoOrigenY = 300f;
        private const float LogoOrigenIzq = 110f, LogoOrigenDer = 1935f;

        // ==================== ESTADO ====================
        private readonly Timer _timer = new Timer();
        private readonly Stopwatch _reloj = new Stopwatch();
        private Bitmap _texto, _circulo, _siluetaBlanca;
        private RectangleF _destinoTexto, _destinoLogo;
        private PointF _centroFinalCirculo;
        private float _radioCirculo;
        private Font _fuenteLema;
        private bool _saltado;
        private float _offsetSalto;

        public PantallaInicio()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual; // se centra a mano en OnLoad
            ClientSize = new Size(Ancho, Alto);
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.White;
            Opacity = 0;
            KeyPreview = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            PrepararImagenes();
            _fuenteLema = new Font(FuenteDisponible("Segoe UI Semibold", "Segoe UI"), 24f, FontStyle.Regular, GraphicsUnit.Pixel);

            _timer.Interval = 15;
            _timer.Tick += (s, e) => Avanzar();
            MouseDown += (s, e) => Saltar();
            KeyDown += (s, e) => Saltar();
        }

        // ==================== PREPARACIÓN ====================
        private void PrepararImagenes()
        {
            float escala = AnchoLogo / (LogoOrigenDer - LogoOrigenIzq);
            float izq = (Ancho - AnchoLogo) / 2f;
            Func<float, float> X = x => izq + (x - LogoOrigenIzq) * escala;
            Func<float, float> Y = y => CentroLogoY + (y - CentroCirculoOrigenY) * escala;

            _destinoTexto = new RectangleF(X(RecorteTexto.Left), Y(RecorteTexto.Top),
                                           RecorteTexto.Width * escala, RecorteTexto.Height * escala);
            _centroFinalCirculo = new PointF(X(CentroCirculoOrigenX), Y(CentroCirculoOrigenY));
            _radioCirculo = RecorteCirculo.Width * escala / 2f;

            Bitmap logo = Properties.Resources.LogoPitchr;
            _texto = Recortar(logo, RecorteTexto, Size.Round(_destinoTexto.Size));
            int d = (int)Math.Round(_radioCirculo * 2);
            _circulo = Recortar(logo, RecorteCirculo, new Size(d, d));

            // Silueta blanca del logo completo (para el destello de luz)
            _destinoLogo = RectangleF.Union(_destinoTexto,
                new RectangleF(_centroFinalCirculo.X - _radioCirculo, _centroFinalCirculo.Y - _radioCirculo, d, d));
            Rectangle area = Rectangle.Ceiling(_destinoLogo);
            _siluetaBlanca = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(_siluetaBlanca))
            using (ImageAttributes blanco = new ImageAttributes())
            {
                blanco.SetColorMatrix(new ColorMatrix(new[]
                {
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 1, 1, 1, 0, 1 },
                }));
                g.TranslateTransform(-area.X, -area.Y);
                Rectangle rt = Rectangle.Round(_destinoTexto);
                g.DrawImage(_texto, rt, 0, 0, _texto.Width, _texto.Height, GraphicsUnit.Pixel, blanco);
                Rectangle rc = new Rectangle((int)Math.Round(_centroFinalCirculo.X - _radioCirculo),
                                             (int)Math.Round(_centroFinalCirculo.Y - _radioCirculo), d, d);
                g.DrawImage(_circulo, rc, 0, 0, d, d, GraphicsUnit.Pixel, blanco);
            }
        }

        private static Bitmap Recortar(Image origen, Rectangle recorte, Size tamano)
        {
            Bitmap bmp = new Bitmap(tamano.Width, tamano.Height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(origen, new Rectangle(Point.Empty, tamano), recorte, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        private static string FuenteDisponible(params string[] nombres)
        {
            using (InstalledFontCollection fuentes = new InstalledFontCollection())
            {
                foreach (string nombre in nombres)
                    foreach (FontFamily f in fuentes.Families)
                        if (string.Equals(f.Name, nombre, StringComparison.OrdinalIgnoreCase))
                            return nombre;
            }
            return SystemFonts.DefaultFont.FontFamily.Name;
        }

        // ==================== VENTANA (sombra y esquinas) ====================
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                int redondeado = 2; // DWMWCP_ROUND (solo Windows 11)
                DwmSetWindowAttribute(Handle, 33, ref redondeado, sizeof(int));
            }
            catch { /* Windows 10 o anterior: esquinas rectas */ }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Centrar con el tamaño real (ya escalado por Windows) en la pantalla actual
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Left + (area.Width - Width) / 2,
                                 area.Top + (area.Height - Height) / 2);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _reloj.Start();
            _timer.Start();
        }

        // ==================== ANIMACIÓN ====================
        private float Tiempo => _reloj.ElapsedMilliseconds + _offsetSalto;

        private void Saltar()
        {
            if (_saltado) return;
            _saltado = true;
            // Brinca al final de la animación y deja solo el desvanecido de salida
            _offsetSalto = Math.Max(0, T_Salida - _reloj.ElapsedMilliseconds);
        }

        private void Avanzar()
        {
            float t = Tiempo;
            if (t < 200f) Opacity = t / 200f;
            else if (t >= T_Salida) Opacity = 1f - Progreso(t, T_Salida, D_Salida);
            else Opacity = 1;

            if (t >= T_Salida + D_Salida)
            {
                _timer.Stop();
                Close();
                return;
            }
            Invalidate();
        }

        private static float Progreso(float t, float inicio, float duracion)
        {
            return Math.Max(0f, Math.Min(1f, (t - inicio) / duracion));
        }

        private static float EaseOutCubic(float x) { return 1f - (float)Math.Pow(1f - x, 3); }
        private static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - (float)Math.Pow(-2f * x + 2f, 3) / 2f;
        }
        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * (float)Math.Pow(x - 1f, 3) + c1 * (float)Math.Pow(x - 1f, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float t = Tiempo;
            DibujarFondo(g);

            // Posición del círculo: entra al centro y luego se recorre a su lugar final
            float pMov = EaseInOutCubic(Progreso(t, T_Texto, D_Texto));
            float centroX = Ancho / 2f + (_centroFinalCirculo.X - Ancho / 2f) * pMov;
            PointF centro = new PointF(centroX, _centroFinalCirculo.Y);

            DibujarOndas(g, t, centro);
            DibujarTexto(g, pMov, centroX);
            DibujarCirculo(g, t, centro);
            DibujarBrillo(g, t);
            DibujarLema(g, t);
            DibujarBarra(g, t);
        }

        private void DibujarFondo(Graphics g)
        {
            Rectangle r = ClientRectangle;
            using (LinearGradientBrush fondo = new LinearGradientBrush(r, Color.White, Color.FromArgb(242, 245, 249), 90f))
                g.FillRectangle(fondo, r);
            using (Pen borde = new Pen(Color.FromArgb(218, 223, 230)))
                g.DrawRectangle(borde, 0, 0, r.Width - 1, r.Height - 1);
        }

        private void DibujarOndas(Graphics g, float t, PointF centro)
        {
            Color[] colores = { AzulClaro, Naranja };
            // Radio máximo: la onda se desvanece antes de tocar cualquier borde,
            // así no se corta primero arriba.
            float margen = 18f;
            float radioMax = Math.Min(Math.Min(centro.Y, Alto - centro.Y),
                                      Math.Min(centro.X, Ancho - centro.X)) - margen;
            float crecimiento = Math.Max(0f, radioMax - _radioCirculo);
            for (int i = 0; i < 2; i++)
            {
                float p = Progreso(t, T_Onda + i * 180f, D_Onda);
                if (p <= 0f || p >= 1f) continue;
                float radio = _radioCirculo + crecimiento * EaseOutCubic(p);
                int alfa = (int)(170 * (1f - p));
                using (Pen pen = new Pen(Color.FromArgb(alfa, colores[i]), 3f * (1f - p) + 1f))
                    g.DrawEllipse(pen, centro.X - radio, centro.Y - radio, radio * 2, radio * 2);
            }
        }

        private void DibujarTexto(Graphics g, float pMov, float centroX)
        {
            if (pMov <= 0f) return;
            // "PITCH" sale de detrás del círculo: se desliza a la izquierda y
            // solo se ve la parte que ya pasó el borde izquierdo del círculo.
            float desplazamiento = (1f - pMov) * _destinoTexto.Width * 0.6f;
            RectangleF destino = _destinoTexto;
            destino.Offset(desplazamiento, 0);

            GraphicsState estado = g.Save();
            g.SetClip(new RectangleF(0, 0, centroX, Alto));
            using (ImageAttributes atributos = Alfa(Math.Min(1f, pMov * 1.6f)))
                g.DrawImage(_texto, Rectangle.Round(destino), 0, 0, _texto.Width, _texto.Height, GraphicsUnit.Pixel, atributos);
            g.Restore(estado);
        }

        private void DibujarCirculo(Graphics g, float t, PointF centro)
        {
            float p = Progreso(t, T_EntradaCirculo, D_EntradaCirculo);
            if (p <= 0f) return;
            float escala = Math.Max(0.01f, EaseOutBack(p));
            float angulo = -360f * (1f - EaseOutCubic(p));

            GraphicsState estado = g.Save();
            g.TranslateTransform(centro.X, centro.Y);
            g.RotateTransform(angulo);
            g.ScaleTransform(escala, escala);
            using (ImageAttributes atributos = Alfa(Math.Min(1f, p * 2.5f)))
                g.DrawImage(_circulo,
                    new Rectangle((int)-_radioCirculo, (int)-_radioCirculo, _circulo.Width, _circulo.Height),
                    0, 0, _circulo.Width, _circulo.Height, GraphicsUnit.Pixel, atributos);
            g.Restore(estado);
        }

        private void DibujarBrillo(Graphics g, float t)
        {
            float p = Progreso(t, T_Brillo, D_Brillo);
            if (p <= 0f || p >= 1f) return;

            float inclinacion = _destinoLogo.Height * 0.6f;
            float x = _destinoLogo.Left - 80f + (_destinoLogo.Width + 160f + inclinacion) * EaseInOutCubic(p);
            float[] anchos = { 70f, 40f, 16f };
            float[] alfas = { 0.18f, 0.25f, 0.35f };

            for (int i = 0; i < anchos.Length; i++)
            {
                using (GraphicsPath banda = new GraphicsPath())
                {
                    float w = anchos[i];
                    banda.AddPolygon(new[]
                    {
                        new PointF(x - w / 2f, _destinoLogo.Top),
                        new PointF(x + w / 2f, _destinoLogo.Top),
                        new PointF(x + w / 2f - inclinacion, _destinoLogo.Bottom),
                        new PointF(x - w / 2f - inclinacion, _destinoLogo.Bottom),
                    });
                    GraphicsState estado = g.Save();
                    g.SetClip(banda);
                    using (ImageAttributes atributos = Alfa(alfas[i]))
                        g.DrawImage(_siluetaBlanca,
                            new Rectangle((int)Math.Floor(_destinoLogo.X), (int)Math.Floor(_destinoLogo.Y), _siluetaBlanca.Width, _siluetaBlanca.Height),
                            0, 0, _siluetaBlanca.Width, _siluetaBlanca.Height, GraphicsUnit.Pixel, atributos);
                    g.Restore(estado);
                }
            }
        }

        private void DibujarLema(Graphics g, float t)
        {
            float p = Progreso(t, T_Lema, D_Lema);
            if (p <= 0f) return;
            float e = EaseOutCubic(p);
            RectangleF r = new RectangleF(0, _destinoLogo.Bottom + 24f + 12f * (1f - e), Ancho, 36f);
            using (StringFormat formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (SolidBrush pincel = new SolidBrush(Color.FromArgb((int)(255 * e), GrisTexto)))
                g.DrawString(Lema, _fuenteLema, pincel, r, formato);
        }

        private void DibujarBarra(Graphics g, float t)
        {
            float aparicion = Progreso(t, T_Barra - 250f, 250f);
            if (aparicion <= 0f) return;
            float p = EaseInOutCubic(Progreso(t, T_Barra, D_Barra));

            const float anchoBarra = 240f, altoBarra = 4f;
            RectangleF pista = new RectangleF((Ancho - anchoBarra) / 2f, Alto - 84f, anchoBarra, altoBarra);
            using (GraphicsPath forma = Redondeado(pista, altoBarra / 2f))
            using (SolidBrush fondo = new SolidBrush(Color.FromArgb((int)(255 * aparicion), 223, 228, 236)))
                g.FillPath(fondo, forma);

            if (p <= 0f) return;
            RectangleF lleno = new RectangleF(pista.X, pista.Y, Math.Max(altoBarra, anchoBarra * p), altoBarra);
            using (GraphicsPath forma = Redondeado(lleno, altoBarra / 2f))
            using (LinearGradientBrush pincel = new LinearGradientBrush(pista, Azul, AzulClaro, 0f))
                g.FillPath(pincel, forma);
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radio * 2f;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static ImageAttributes Alfa(float alfa)
        {
            ImageAttributes atributos = new ImageAttributes();
            atributos.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Max(0f, Math.Min(1f, alfa)) });
            return atributos;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                if (_texto != null) _texto.Dispose();
                if (_circulo != null) _circulo.Dispose();
                if (_siluetaBlanca != null) _siluetaBlanca.Dispose();
                if (_fuenteLema != null) _fuenteLema.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

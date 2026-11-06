using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Base de los paneles de perfil (EditarPerfil, PerfilInversor y Producto).
    /// Aquí está todo lo que comparten: que se adapten al tamaño de la ventana, las fuentes,
    /// las formas redondeadas, las sombras y cómo leer o guardar imágenes.
    /// Así cada panel solo tiene su diseño y su CRUD.
    /// </summary>
    public class SeccionBase : UserControl
    {
        // Tamaño del diseño en Figma sin la barra de arriba (que está en HomePage)
        protected float AnchoDiseno = 1280f;
        protected float AltoDiseno = 701f;

        protected float Escala = 1f;
        protected float MargenX, MargenY;

        // Carpeta (junto al .exe) donde se guardan las fotos. En la base solo se guarda el nombre del archivo.
        protected const string CarpetaImagenes = "Imagenes";

        protected static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);     // #0060D6
        protected static readonly Color AzulBoton = Color.FromArgb(8, 127, 223);      // #087FDF
        protected static readonly Color Naranja = Color.FromArgb(248, 110, 56);       // #F86E38

        private Control _lienzo;
        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();

        /// <summary>Guarda las posiciones del diseñador y hace que el panel crezca con la ventana.</summary>
        protected void PrepararEscalado(Control lienzo)
        {
            _lienzo = lienzo;
            ActivarDobleBuffer(lienzo);
            GuardarOriginales(lienzo);
            Resize += (s, e) => AjustarATamano();
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

        /// <summary>Todo crece parejo (sin deformarse) y centrado; el lienzo ocupa todo el panel.</summary>
        protected void AjustarATamano()
        {
            if (_lienzo == null || ClientSize.Width < 50 || ClientSize.Height < 50) return;

            Escala = Math.Min(ClientSize.Width / AnchoDiseno, ClientSize.Height / AltoDiseno);
            MargenX = (ClientSize.Width - AnchoDiseno * Escala) / 2f;
            MargenY = (ClientSize.Height - AltoDiseno * Escala) / 2f;

            SuspendLayout();
            _lienzo.Bounds = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == _lienzo;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * Escala + (directo ? MargenX : 0)),
                    (int)Math.Round(r.Y * Escala + (directo ? MargenY : 0)),
                    (int)Math.Round(r.Width * Escala),
                    (int)Math.Round(r.Height * Escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], Escala);
            }
            ResumeLayout(true);
            _lienzo.Invalidate(true);
        }

        // ---------- Fuentes ----------
        private static readonly Dictionary<string, Font> CacheFuentes = new Dictionary<string, Font>();
        private static readonly Dictionary<string, bool> Instaladas = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Misma fuente pero escalada. Si la fuente del Figma no está instalada, usa Segoe UI.</summary>
        protected static Font EscalarFuente(Font original, float escala)
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

        protected static bool FuenteInstalada(string nombre)
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

        /// <summary>Fuente Plus Jakarta Sans (o Segoe UI si no está instalada) del tamaño indicado ya escalado.</summary>
        protected Font Fuente(float pixeles, FontStyle estilo)
        {
            return EscalarFuente(new Font("Plus Jakarta Sans", pixeles, estilo, GraphicsUnit.Pixel), Escala);
        }

        // ---------- Texto guía de los TextBox ----------
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        /// <summary>Muestra en gris el texto de la propiedad Tag mientras el TextBox está vacío.</summary>
        protected static void PonerTextoGuia(TextBoxBase caja)
        {
            caja.HandleCreated += (s, e) =>
            {
                string guia = caja.Tag as string;
                if (string.IsNullOrEmpty(guia)) return;
                try { SendMessage(caja.Handle, EM_SETCUEBANNER, (IntPtr)1, guia); }
                catch (Exception) { /* solo falla fuera de Windows */ }
            };
        }

        // ---------- Imágenes ----------
        /// <summary>
        /// Lee una imagen guardada en la base: puede ser el nombre de un archivo de la carpeta Imagenes
        /// o un enlace de internet (http...). Si no existe o falla, regresa null.
        /// </summary>
        public static Image CargarImagen(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            valor = valor.Trim();
            try
            {
                if (valor.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (WebClient cliente = new WebClient())
                    using (MemoryStream datos = new MemoryStream(cliente.DownloadData(valor)))
                    using (Image original = Image.FromStream(datos))
                        return new Bitmap(original);
                }
                return LeerArchivo(Path.Combine(Application.StartupPath, CarpetaImagenes, valor));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Lee una imagen del disco sin dejar el archivo bloqueado.</summary>
        protected static Image LeerArchivo(string ruta)
        {
            if (!File.Exists(ruta)) return null;
            using (Image original = Image.FromFile(ruta))
                return new Bitmap(original);
        }

        /// <summary>Copia una foto elegida a la carpeta Imagenes y regresa el nombre con el que quedó (para guardarlo en la base).</summary>
        protected static string CopiarAImagenes(string rutaOrigen)
        {
            string carpeta = Path.Combine(Application.StartupPath, CarpetaImagenes);
            Directory.CreateDirectory(carpeta);
            string nombre = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Path.GetFileName(rutaOrigen);
            File.Copy(rutaOrigen, Path.Combine(carpeta, nombre), true);
            return nombre;
        }

        /// <summary>Abre el explorador para elegir una foto (PNG o JPG, máximo 5 MB). Regresa la ruta o null.</summary>
        protected static string ElegirFoto()
        {
            using (OpenFileDialog dialogo = new OpenFileDialog())
            {
                dialogo.Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (dialogo.ShowDialog() != DialogResult.OK) return null;
                if (new FileInfo(dialogo.FileName).Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("La imagen pesa más de 5 MB. Elige una más ligera.", "Imagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
                return dialogo.FileName;
            }
        }

        // ---------- Dibujo ----------
        protected static void ActivarDobleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           .SetValue(control, true, null);
        }

        protected static void Calidad(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        protected static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            GraphicsPath ruta = new GraphicsPath();
            float d = Math.Max(1f, Math.Min(radio * 2, Math.Min(r.Width, r.Height)));
            ruta.AddArc(r.X, r.Y, d, d, 180, 90);
            ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }

        /// <summary>Sombra como la del Figma (Y 4, desenfoque 4, negro al 25 %).</summary>
        protected static void Sombra(Graphics g, RectangleF r, float radio, float k)
        {
            for (int i = 4; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(r.X - d * 0.5f, r.Y + d, r.Width + d, r.Height + d * 0.5f);
                using (GraphicsPath ruta = Redondeado(capa, radio + d))
                using (SolidBrush brocha = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                    g.FillPath(brocha, ruta);
            }
        }

        /// <summary>Dibuja una imagen recortada para llenar el área sin deformarse (como "object-fit: cover").</summary>
        protected static void DibujarCubriendo(Graphics g, Image imagen, RectangleF area)
        {
            float escala = Math.Max(area.Width / imagen.Width, area.Height / imagen.Height);
            float w = imagen.Width * escala, h = imagen.Height * escala;
            g.DrawImage(imagen, area.X + (area.Width - w) / 2f, area.Y + (area.Height - h) / 2f, w, h);
        }

        /// <summary>Foto redonda (o un círculo gris con la inicial si no hay foto).</summary>
        protected static void DibujarFotoRedonda(Graphics g, Image foto, RectangleF circulo, string nombre, float k)
        {
            using (GraphicsPath ruta = new GraphicsPath())
            {
                ruta.AddEllipse(circulo);
                if (foto != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    DibujarCubriendo(g, foto, circulo);
                    g.Clip = anterior;
                }
                else
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(circulo, AzulPitcher, Color.FromArgb(56, 189, 248), 45f))
                        g.FillPath(b, ruta);
                    string inicial = string.IsNullOrWhiteSpace(nombre) ? "?" : nombre.Trim().Substring(0, 1).ToUpper();
                    using (Font f = new Font(FuenteInstalada("Plus Jakarta Sans") ? "Plus Jakarta Sans" : "Segoe UI", Math.Max(1f, circulo.Height * 0.42f), FontStyle.Bold, GraphicsUnit.Pixel))
                        TextRenderer.DrawText(g, inicial, f, Rectangle.Round(circulo), Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        /// <summary>Botón píldora con sombra (se conecta al Paint de cada botón). El color de fondo se toma de su Tag.</summary>
        protected void BotonPildora_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            Color detras = boton.Parent != null && boton.Parent.BackColor.A == 255 ? boton.Parent.BackColor : Color.White;
            g.Clear(detras);
            float k = Escala;
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k);
            Sombra(g, area, area.Height / 2f, k);

            Color fondo = boton.Tag is Color ? (Color)boton.Tag : AzulBoton;
            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            if (encima) fondo = fondo == Color.White ? Color.FromArgb(246, 246, 250) : ControlPaint.Light(fondo, 0.15f);
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            {
                using (SolidBrush b = new SolidBrush(fondo)) g.FillPath(b, ruta);
                if (fondo.R > 240 && fondo.G > 240)
                    using (Pen borde = new Pen(Color.FromArgb(213, 215, 221), Math.Max(1f, 0.8f * k))) g.DrawPath(borde, ruta);
            }
            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), boton.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        /// <summary>Le pone a un botón el estilo píldora: color de fondo y efecto al pasar el mouse.</summary>
        protected void HacerPildora(Button boton, Color fondo)
        {
            boton.Tag = fondo;
            boton.Paint += BotonPildora_Paint;
            boton.MouseEnter += (s, e) => boton.Invalidate();
            boton.MouseLeave += (s, e) => boton.Invalidate();
        }
    }
}

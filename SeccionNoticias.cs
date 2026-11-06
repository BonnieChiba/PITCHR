using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Sección "Noticias" de HomePage (es un panel, no una ventana). Lee integrative.news.
    /// </summary>
    public partial class SeccionNoticias : UserControl
    {
        private DBConn dbconn;                                      //Conectar
        private MySqlDataAdapter newsDA = new MySqlDataAdapter();   //Extraer
        private DataTable newstable = new DataTable();              //Almacenar

        // Carpeta (junto al .exe) donde van las fotos de las noticias.
        // En la columna newsImage se guarda el nombre del archivo (ej. "feria.jpg") o un enlace de internet (http...).
        private const string CarpetaImagenes = "Imagenes";

        public SeccionNoticias()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
        }

        private void SeccionNoticias_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== CRUD: LEER (SELECT) ====================
        // Trae las 4 noticias más recientes (la última que se guardó primero).
        private void LoadDBCommands()
        {
            newsDA.SelectCommand = new MySqlCommand("SELECT idNews, newsName, eventLink, evenDate, newsImage FROM integrative.news ORDER BY idNews DESC LIMIT 4");
        }

        private void LoadData()
        {
            dbconn = new DBConn();
            newsDA.SelectCommand.Connection = dbconn.dbConnection;
            try
            {
                newstable.Clear();
                newsDA.Fill(newstable);
                this.ShowData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las noticias:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // La noticia más nueva va a la tarjeta grande; las siguientes 3, a la columna derecha.
        private void ShowData()
        {
            TarjetaNoticia[] tarjetas = { NoticiaDestacada, Noticia1, Noticia2, Noticia3 };

            for (int i = 0; i < tarjetas.Length; i++)
            {
                if (i < newstable.Rows.Count)
                {
                    DataRow fila = newstable.Rows[i];
                    string enlace = Convert.ToString(fila["eventLink"]);

                    Noticia noticia = new Noticia(
                        Convert.ToInt32(fila["idNews"]),
                        Convert.ToString(fila["newsName"]),
                        "",                                   // la tabla no tiene resumen
                        this.Fuente(enlace),                  // en la etiqueta naranja se muestra de qué página es
                        this.FormatoFecha(fila["evenDate"]),
                        this.CargarImagen(Convert.ToString(fila["newsImage"])));
                    noticia.Enlace = enlace;

                    tarjetas[i].Mostrar(noticia);
                    tarjetas[i].Visible = true;
                }
                else
                {
                    tarjetas[i].Visible = false;   // no hay suficientes noticias para esta tarjeta
                }
            }

            if (newstable.Rows.Count == 0)
            {
                NoticiaDestacada.Mostrar(new Noticia(0, "Aún no hay noticias publicadas", "", "", "", null));
                NoticiaDestacada.Visible = true;
            }
        }

        // ==================== VER MÁS ====================
        // Clic en una noticia (o en su "Ver más"): abre en el navegador la página de donde se sacó (eventLink).
        private void Noticia_Click(object sender, EventArgs e)
        {
            Noticia elegida = ((TarjetaNoticia)sender).Datos;
            if (elegida == null || string.IsNullOrWhiteSpace(elegida.Enlace))
            {
                MessageBox.Show("Esta noticia no tiene enlace.", "Noticias", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string enlace = elegida.Enlace.Trim();
            if (!enlace.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                enlace = "https://" + enlace;
            }

            try
            {
                Process.Start(enlace);   // abre el navegador predeterminado
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el enlace:\n" + ex.Message, "Noticias", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== AYUDANTES ====================
        // Fecha como en el Figma: "12 JUN 2025". Si la columna es texto, se muestra tal cual.
        private string FormatoFecha(object valor)
        {
            if (valor == null || valor == DBNull.Value) return "";
            if (valor is DateTime)
            {
                string fecha = ((DateTime)valor).ToString("dd MMM yyyy", new CultureInfo("es-MX"));
                return fecha.Replace(".", "").ToUpper();
            }
            return valor.ToString();
        }

        // De "https://www.elheraldodechihuahua.com.mx/local/..." saca "elheraldodechihuahua.com.mx"
        private string Fuente(string enlace)
        {
            try
            {
                string texto = enlace.Trim();
                if (!texto.StartsWith("http", StringComparison.OrdinalIgnoreCase)) texto = "https://" + texto;
                string dominio = new Uri(texto).Host;
                return dominio.StartsWith("www.") ? dominio.Substring(4) : dominio;
            }
            catch (Exception)
            {
                return "Noticia";
            }
        }

        // La imagen puede ser un archivo de la carpeta Imagenes o un enlace de internet. Si falla, regresa null.
        private Image CargarImagen(string valor)
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
                    {
                        return new Bitmap(original);
                    }
                }

                string ruta = Path.Combine(Application.StartupPath, CarpetaImagenes, valor);
                if (!File.Exists(ruta)) return null;
                using (Image original = Image.FromFile(ruta))
                {
                    return new Bitmap(original);   // copia en memoria para no dejar el archivo bloqueado
                }
            }
            catch (Exception)
            {
                return null;   // si la imagen no se puede leer, la tarjeta muestra un degradado azul
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

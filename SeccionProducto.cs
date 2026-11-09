using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Panel "Ver perfil" del EMPRENDEDOR (frame "Product"): su marca, productos, descripción,
    /// galería de imágenes y la tarjeta del fundador.
    /// Lee integrative.entrepreneurship junto con integrative.entrepreneur (para el nombre y la foto del fundador).
    /// </summary>
    public partial class SeccionProducto : SeccionBase
    {
        private DBConn dbconn;                                                  //Conectar
        private MySqlDataAdapter entrepreneurshipDA = new MySqlDataAdapter();   //Extraer
        private DataTable entrepreneurshiptable = new DataTable();              //Almacenar

        // Mientras no inicien sesión (para probar el panel), se usa este idEntrepreneur. Debe existir en la tabla entrepreneur.
        private const int IdEmprendedorDePrueba = 1;

        // Imágenes que se van a dibujar
        private Image _logo, _fotoFundador;
        private readonly Image[] _productos = new Image[3];
        private readonly List<Image> _galeria = new List<Image>();   // imageHome, image2 e image3
        private int _imagenActual = 0;
        private bool _tieneMarca = false;

        /// <summary>Avisos para HomePage: la flecha de regresar y "todavía no tiene marca" (para ir a crearla).</summary>
        public event EventHandler Regresar;
        public event EventHandler CrearMarca;
        public event EventHandler<int> Contactar;   // manda el idEntrepreneur del fundador

        private int _idFundador = 0;

        public SeccionProducto()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma (ver región de abajo)
        }

        // Si se abrió desde una tarjeta, aquí va el idEntrepreneurship de esa tarjeta.
        // Si vale 0, se muestra la marca del emprendedor que inició sesión ("Ver perfil").
        private int _idEmprendimiento = 0;

        public SeccionProducto(int idEmprendimiento) : this()
        {
            _idEmprendimiento = idEmprendimiento;
        }

        private int IdEmprendedor()
        {
            if (Sesion.Activa && Sesion.Rol == 1) return Sesion.IdUsuario;
            return IdEmprendedorDePrueba;
        }

        private void SeccionProducto_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== LEER (SELECT con JOIN) ====================
        private void LoadDBCommands()
        {
            entrepreneurshipDA.SelectCommand = new MySqlCommand(
                "SELECT e.idEntrepreneur, e.brand, e.category, e.likes, e.description, e.imageHome, e.image2, e.image3, " +
                "e.productName1, e.productName2, e.productName3, e.productImage1, e.productImage2, e.productImage3, " +
                "en.name, en.lastName, en.imageProfile " +
                "FROM integrative.entrepreneurship e " +
                "INNER JOIN integrative.entrepreneur en ON en.idEntrepreneur = e.idEntrepreneur " +
                (_idEmprendimiento > 0
                    ? "WHERE e.idEntrepreneurship = @idEntrepreneurship LIMIT 1"
                    : "WHERE e.idEntrepreneur = @idEntrepreneur LIMIT 1"));
        }

        private void LoadData()
        {
            dbconn = new DBConn();
            entrepreneurshipDA.SelectCommand.Connection = dbconn.dbConnection;
            entrepreneurshipDA.SelectCommand.Parameters.Clear();
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@idEntrepreneur", this.IdEmprendedor());
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@idEntrepreneurship", _idEmprendimiento);
            try
            {
                entrepreneurshiptable.Clear();
                entrepreneurshipDA.Fill(entrepreneurshiptable);

                if (entrepreneurshiptable.Rows.Count > 0)
                {
                    this.ShowData(entrepreneurshiptable.Rows[0]);
                }
                else
                {
                    this.MostrarSinMarca();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowData(DataRow fila)
        {
            _tieneMarca = true;
            _idFundador = Convert.ToInt32(fila["idEntrepreneur"]);
            MarcaLabel.Text = Convert.ToString(fila["brand"]);
            CategoriaLabel.Text = Convert.ToString(fila["category"]).ToUpper();
            LikesLabel.Text = Convert.ToString(fila["likes"]);
            SobreTextoLabel.Text = Convert.ToString(fila["description"]);
            FundadorNombreLabel.Text = (Convert.ToString(fila["name"]) + " " + Convert.ToString(fila["lastName"])).Trim().ToUpper();

            _logo = CargarImagen(Convert.ToString(fila["imageHome"]));
            _fotoFundador = CargarImagen(Convert.ToString(fila["imageProfile"]));

            Label[] nombres = { Producto1Label, Producto2Label, Producto3Label };
            for (int i = 0; i < 3; i++)
            {
                nombres[i].Text = Convert.ToString(fila["productName" + (i + 1)]);
                _productos[i] = CargarImagen(Convert.ToString(fila["productImage" + (i + 1)]));
            }

            // Galería: las imágenes generales de la marca que sí existan
            _galeria.Clear();
            foreach (string columna in new[] { "image2", "image3", "imageHome" })
            {
                Image imagen = CargarImagen(Convert.ToString(fila[columna]));
                if (imagen != null) _galeria.Add(imagen);
            }
            _imagenActual = 0;
            FlechaIzquierdaBoton.Visible = _galeria.Count > 1;
            FlechaDerechaBoton.Visible = _galeria.Count > 1;

            FondoPanel.Invalidate(true);
        }

        // Si el emprendedor todavía no tiene marca, se le invita a crearla
        private void MostrarSinMarca()
        {
            _tieneMarca = false;
            MarcaLabel.Text = "Aún no tienes marca";
            CategoriaLabel.Text = "";
            LikesLabel.Text = "";
            SobreTextoLabel.Text = "Crea tu marca en \"Personalizar perfil\" para que los inversionistas la puedan ver.";
            FundadorNombreLabel.Text = Sesion.Activa ? Sesion.Usuario.ToUpper() : "";
            foreach (Label nombre in new[] { Producto1Label, Producto2Label, Producto3Label }) nombre.Text = "";
            FlechaIzquierdaBoton.Visible = false;
            FlechaDerechaBoton.Visible = false;
            PerfilBoton.Text = "CREAR MARCA";
            FondoPanel.Invalidate(true);
        }

        // ==================== GALERÍA ====================
        private void FlechaIzquierdaBoton_Click(object sender, EventArgs e)
        {
            if (_galeria.Count == 0) return;
            _imagenActual = (_imagenActual - 1 + _galeria.Count) % _galeria.Count;
            GaleriaPanel.Invalidate(true);
        }

        private void FlechaDerechaBoton_Click(object sender, EventArgs e)
        {
            if (_galeria.Count == 0) return;
            _imagenActual = (_imagenActual + 1) % _galeria.Count;
            GaleriaPanel.Invalidate(true);
        }

        // ==================== BOTONES ====================
        private void ContactarBoton_Click(object sender, EventArgs e)
        {
            // Solo un inversionista puede empezar una conversación con el fundador
            if (!Sesion.Activa || Sesion.Rol != 0)
            {
                MessageBox.Show("Inicia sesión como inversionista para enviarle un mensaje al fundador.", "Pitcher");
                return;
            }
            if (_idFundador == 0) return;
            if (Contactar != null) Contactar(this, _idFundador);
        }

        private void PerfilBoton_Click(object sender, EventArgs e)
        {
            if (!_tieneMarca)
            {
                if (CrearMarca != null) CrearMarca(this, EventArgs.Empty);
                return;
            }
            // Aquí se abrirá el perfil del fundador (frame "porfileFounder").
            MessageBox.Show("El perfil del fundador estará disponible próximamente.", "Pitcher");
        }

        private void RegresarBoton_Click(object sender, EventArgs e)
        {
            if (Regresar != null) Regresar(this, EventArgs.Empty);
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        private static readonly Color GrisFranja = Color.FromArgb(242, 242, 242);   // #F2F2F2
        private static readonly Color AzulMarino = Color.FromArgb(30, 41, 99);

        private void PrepararDiseno()
        {
            HacerPildora(ContactarBoton, AzulPitcher);
            HacerPildora(PerfilBoton, AzulPitcher);
            foreach (Control c in new Control[] { LogoPanel, Producto1Panel, Producto2Panel, Producto3Panel, SobrePanel, GaleriaPanel, FundadorPanel })
                ActivarDobleBuffer(c);

            FondoPanel.Paint += FondoPanel_Paint;
            LogoPanel.Paint += LogoPanel_Paint;
            Producto1Panel.Paint += Producto_Paint;
            Producto2Panel.Paint += Producto_Paint;
            Producto3Panel.Paint += Producto_Paint;
            SobrePanel.Paint += SobrePanel_Paint;
            GaleriaPanel.Paint += GaleriaPanel_Paint;
            FundadorPanel.Paint += FundadorPanel_Paint;
            RegresarBoton.Paint += (s, e) => SeccionPerfilInversor.DibujarRegresar(e.Graphics, RegresarBoton, Escala, GrisFranja);
            FlechaIzquierdaBoton.Paint += Flecha_Paint;
            FlechaDerechaBoton.Paint += Flecha_Paint;
            foreach (Button b in new[] { RegresarBoton, FlechaIzquierdaBoton, FlechaDerechaBoton })
            {
                Button boton = b;
                boton.MouseEnter += (s, e) => boton.Invalidate();
                boton.MouseLeave += (s, e) => boton.Invalidate();
            }
            PrepararEscalado(FondoPanel);
        }

        /// <summary>Franja gris de arriba (a todo lo ancho), corazón de likes y etiqueta de categoría.</summary>
        private void FondoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            using (SolidBrush b = new SolidBrush(GrisFranja))
                g.FillRectangle(b, 0, 0, FondoPanel.Width, MargenY + 200 * k);

            if (!_tieneMarca) return;

            // Corazón (a la izquierda del número de likes)
            RectangleF corazon = new RectangleF(LikesLabel.Left - 34 * k, LikesLabel.Top + 2 * k, 28 * k, 28 * k);
            DibujarCorazon(g, corazon, k);

            // Etiqueta naranja de la categoría (CategoriaLabel tiene AutoSize: mide lo que su texto)
            if (CategoriaLabel.Text.Length > 0)
            {
                RectangleF pildora = CategoriaLabel.Bounds;
                using (GraphicsPath p = Redondeado(pildora, pildora.Height / 2f))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(125, 248, 110, 56)))
                    g.FillPath(b, p);
            }
        }

        private static void DibujarCorazon(Graphics g, RectangleF r, float k)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddBezier(x + w * 0.50f, y + h * 0.86f, x + w * 0.10f, y + h * 0.58f, x + w * 0.02f, y + h * 0.40f, x + w * 0.08f, y + h * 0.26f);
                p.AddBezier(x + w * 0.08f, y + h * 0.26f, x + w * 0.16f, y + h * 0.08f, x + w * 0.42f, y + h * 0.08f, x + w * 0.50f, y + h * 0.28f);
                p.AddBezier(x + w * 0.50f, y + h * 0.28f, x + w * 0.58f, y + h * 0.08f, x + w * 0.84f, y + h * 0.08f, x + w * 0.92f, y + h * 0.26f);
                p.AddBezier(x + w * 0.92f, y + h * 0.26f, x + w * 0.98f, y + h * 0.40f, x + w * 0.90f, y + h * 0.58f, x + w * 0.50f, y + h * 0.86f);
                p.CloseFigure();
                using (Pen pen = new Pen(Color.FromArgb(90, 90, 90), Math.Max(1.5f, 2.2f * k))) g.DrawPath(pen, p);
            }
        }

        /// <summary>Logo redondo de la marca (imageHome) con sombra.</summary>
        private void LogoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisFranja);
            float k = Escala;
            RectangleF circulo = new RectangleF(2 * k, 0, LogoPanel.Width - 4 * k - 1, LogoPanel.Width - 4 * k - 1);
            for (int i = 4; i >= 1; i--)
                using (SolidBrush b = new SolidBrush(Color.FromArgb(14, 0, 0, 0)))
                    g.FillEllipse(b, circulo.X, circulo.Y + i * k, circulo.Width, circulo.Height);
            DibujarFotoRedonda(g, _logo, circulo, MarcaLabel.Text, k);
        }

        /// <summary>Tarjetitas blancas de los 3 productos.</summary>
        private void Producto_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            int indice = panel == Producto1Panel ? 0 : panel == Producto2Panel ? 1 : 2;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisFranja);
            float k = Escala;
            RectangleF r = new RectangleF(2 * k, 0, panel.Width - 4 * k - 1, panel.Height - 6 * k);
            Sombra(g, r, 8 * k, k);
            using (GraphicsPath ruta = Redondeado(r, 8 * k))
            {
                g.FillPath(Brushes.White, ruta);
                RectangleF foto = RectangleF.Inflate(r, -8 * k, -8 * k);
                if (_productos[indice] != null)
                {
                    Region anterior = g.Clip;
                    using (GraphicsPath p = Redondeado(foto, 6 * k))
                    {
                        g.SetClip(p, CombineMode.Intersect);
                        DibujarCubriendo(g, _productos[indice], foto);
                    }
                    g.Clip = anterior;
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 245, 248)))
                    using (GraphicsPath p = Redondeado(foto, 6 * k))
                        g.FillPath(b, p);
                }
            }
        }

        /// <summary>Recuadro gris "Sobre el emprendimiento".</summary>
        private void SobrePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            using (GraphicsPath ruta = Redondeado(new RectangleF(0, 0, SobrePanel.Width - 1, SobrePanel.Height - 1), 20 * k))
            using (SolidBrush b = new SolidBrush(GrisFranja))
                g.FillPath(b, ruta);
        }

        /// <summary>Galería: imagen actual (o fondo morado con líneas si no hay) con esquinas redondeadas.</summary>
        private void GaleriaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            RectangleF r = new RectangleF(0, 0, GaleriaPanel.Width - 1, GaleriaPanel.Height - 1);
            using (GraphicsPath ruta = Redondeado(r, 14 * k))
            {
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(76, 63, 170), Color.FromArgb(34, 26, 110), 60f))
                    g.FillRectangle(b, r);
                if (_galeria.Count > 0)
                {
                    DibujarCubriendo(g, _galeria[_imagenActual], r);
                }
                else
                {
                    using (Pen naranja = new Pen(Naranja, 6 * k))
                        g.DrawArc(naranja, r.Width * 0.62f, -r.Height * 0.25f, r.Width * 0.6f, r.Height * 0.55f, 90, 180);
                    using (Pen lila = new Pen(Color.FromArgb(150, 160, 140, 255), 6 * k))
                        g.DrawArc(lila, -r.Width * 0.3f, r.Height * 0.32f, r.Width * 1.0f, r.Height * 0.6f, 200, 120);
                }
                g.Clip = anterior;
            }
        }

        /// <summary>Tarjeta del fundador (blanca con la foto a la derecha), encima de la galería.</summary>
        private void FundadorPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            float k = Escala;
            RectangleF r = new RectangleF(0, 0, FundadorPanel.Width - 1, FundadorPanel.Height - 1);
            using (GraphicsPath ruta = Redondeado(r, 14 * k))
            {
                g.FillPath(Brushes.White, ruta);
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                RectangleF foto = new RectangleF(334 * k, 0, r.Width - 334 * k + 1, r.Height + 1);
                if (_fotoFundador != null)
                {
                    DibujarCubriendo(g, _fotoFundador, foto);
                }
                else
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(foto, AzulPitcher, Color.FromArgb(56, 189, 248), 60f))
                        g.FillRectangle(b, foto);
                    string inicial = FundadorNombreLabel.Text.Length > 0 ? FundadorNombreLabel.Text.Substring(0, 1) : "";
                    TextRenderer.DrawText(g, inicial, Fuente(56, FontStyle.Bold), Rectangle.Round(foto), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                g.Clip = anterior;
            }
        }

        /// <summary>Flechas de la galería: cuadrito blanco con triángulo azul marino.</summary>
        private void Flecha_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            RectangleF r = new RectangleF(2 * k, 2 * k, boton.Width - 4 * k - 1, boton.Height - 6 * k);
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            using (GraphicsPath s = Redondeado(new RectangleF(r.X, r.Y + 2 * k, r.Width, r.Height), 6 * k))
                g.FillPath(sombra, s);
            using (GraphicsPath p = Redondeado(r, 6 * k))
            using (SolidBrush b = new SolidBrush(encima ? Color.FromArgb(240, 243, 248) : Color.White))
                g.FillPath(b, p);
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f, t = 8 * k;
            PointF[] triangulo = boton == FlechaIzquierdaBoton
                ? new[] { new PointF(cx + t * 0.6f, cy - t), new PointF(cx + t * 0.6f, cy + t), new PointF(cx - t * 0.7f, cy) }
                : new[] { new PointF(cx - t * 0.6f, cy - t), new PointF(cx - t * 0.6f, cy + t), new PointF(cx + t * 0.7f, cy) };
            using (SolidBrush b = new SolidBrush(AzulMarino))
                g.FillPolygon(b, triangulo);
        }

        #endregion
    }
}

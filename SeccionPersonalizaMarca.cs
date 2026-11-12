using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Sección "Personaliza tu marca" de HomePage (es un panel, no una ventana).
    /// El emprendedor crea o edita su marca en integrative.entrepreneurship.
    /// </summary>
    public partial class SeccionPersonalizaMarca : UserControl
    {
        private DBConn dbconn;                                                  //Conectar
        private MySqlDataAdapter entrepreneurshipDA = new MySqlDataAdapter();   //Extraer
        private DataTable entrepreneurshiptable = new DataTable();              //Almacenar

        // idEntrepreneurship de la marca que se está editando. 0 = marca NUEVA (se hará INSERT).
        // Un emprendedor puede tener varias marcas: HomePage manda aquí el id de la marca que eligió en "Mis Marcas".
        private int idEntrepreneurship = 0;

        // Mientras no inicien sesión (para probar la pantalla), se usa este idEntrepreneur.
        // Debe existir en la tabla entrepreneur.
        private const int IdEmprendedorDePrueba = 1;

        // Carpeta (junto al .exe) donde se copian las fotos. En la base solo se guarda el nombre del archivo.
        private const string CarpetaImagenes = "Imagenes";

        // Las 6 imágenes de la pantalla. Cada recuadro sabe su título, el archivo guardado y el que se acaba de elegir.
        private Dictionary<Panel, CampoImagen> _imagenes;

        // Avisos para HomePage: la flecha de regresar y "se guardó la marca" (para refrescar Emprendimientos)
        public event EventHandler Regresar;
        public event EventHandler MarcaGuardada;

        public SeccionPersonalizaMarca() : this(0)
        {
        }

        public SeccionPersonalizaMarca(int idEmprendimiento)
        {
            InitializeComponent();
            idEntrepreneurship = idEmprendimiento;

            _imagenes = new Dictionary<Panel, CampoImagen>
            {
                { ImageHomePanel,      new CampoImagen("Imagen principal") },
                { Image2Panel,         new CampoImagen("Imagen 2") },
                { Image3Panel,         new CampoImagen("Imagen 3") },
                { ProductImage1Panel,  new CampoImagen("Producto Imagen 1") },
                { ProductImage2Panel,  new CampoImagen("Producto Imagen 2") },
                { ProductImage3Panel,  new CampoImagen("Producto Imagen 3") }
            };

            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
        }

        // El emprendedor dueño de la marca: el que inició sesión o, si nadie ha iniciado sesión, el de prueba.
        private int IdEmprendedor()
        {
            if (Sesion.Activa && Sesion.Rol == 1)
            {
                return Sesion.IdUsuario;
            }
            return IdEmprendedorDePrueba;
        }

        private void SeccionPersonalizaMarca_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== COMANDOS (SELECT, INSERT, UPDATE) ====================
        private void LoadDBCommands()
        {
            entrepreneurshipDA.SelectCommand = new MySqlCommand(
                "SELECT idEntrepreneurship, idEntrepreneur, brand, category, shortDescription, description, " +
                "imageHome, image2, image3, productName1, productName2, productName3, productImage1, productImage2, productImage3 " +
                "FROM integrative.entrepreneurship WHERE idEntrepreneurship = @idEntrepreneurship AND idEntrepreneur = @idEntrepreneur");

            entrepreneurshipDA.InsertCommand = new MySqlCommand(
                "INSERT INTO integrative.entrepreneurship (idEntrepreneur, brand, category, shortDescription, description, " +
                "imageHome, image2, image3, productName1, productName2, productName3, productImage1, productImage2, productImage3, likes) " +
                "VALUES (@idEntrepreneur, @brand, @category, @shortDescription, @description, " +
                "@imageHome, @image2, @image3, @productName1, @productName2, @productName3, @productImage1, @productImage2, @productImage3, 0)");

            entrepreneurshipDA.UpdateCommand = new MySqlCommand(
                "UPDATE integrative.entrepreneurship SET brand = @brand, category = @category, shortDescription = @shortDescription, " +
                "description = @description, imageHome = @imageHome, image2 = @image2, image3 = @image3, " +
                "productName1 = @productName1, productName2 = @productName2, productName3 = @productName3, " +
                "productImage1 = @productImage1, productImage2 = @productImage2, productImage3 = @productImage3 " +
                "WHERE idEntrepreneurship = @idEntrepreneurship");
        }

        // ==================== LEER (SELECT) ====================
        // Busca la marca elegida (solo si es de este emprendedor). Si es marca nueva, deja todo vacío.
        private void LoadData()
        {
            TituloLabel.Text = idEntrepreneurship == 0 ? "Nueva Marca" : "Editar Marca";
            if (idEntrepreneurship == 0)
            {
                this.clearTextBoxes();
                return;
            }

            dbconn = new DBConn();
            entrepreneurshipDA.SelectCommand.Connection = dbconn.dbConnection;
            entrepreneurshipDA.SelectCommand.Parameters.Clear();
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@idEntrepreneurship", idEntrepreneurship);
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@idEntrepreneur", this.IdEmprendedor());
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
                    idEntrepreneurship = 0;
                    this.clearTextBoxes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Pasa los datos de la fila a los controles de la pantalla
        private void ShowData(DataRow fila)
        {
            idEntrepreneurship = Convert.ToInt32(fila["idEntrepreneurship"]);

            BrandTextBox.Text = Convert.ToString(fila["brand"]);
            CategoryComboBox.SelectedItem = Convert.ToString(fila["category"]);
            ShortDescriptionTextBox.Text = Convert.ToString(fila["shortDescription"]);
            DescriptionTextBox.Text = Convert.ToString(fila["description"]);
            ProductName1TextBox.Text = Convert.ToString(fila["productName1"]);
            ProductName2TextBox.Text = Convert.ToString(fila["productName2"]);
            ProductName3TextBox.Text = Convert.ToString(fila["productName3"]);

            this.MostrarImagen(ImageHomePanel, Convert.ToString(fila["imageHome"]));
            this.MostrarImagen(Image2Panel, Convert.ToString(fila["image2"]));
            this.MostrarImagen(Image3Panel, Convert.ToString(fila["image3"]));
            this.MostrarImagen(ProductImage1Panel, Convert.ToString(fila["productImage1"]));
            this.MostrarImagen(ProductImage2Panel, Convert.ToString(fila["productImage2"]));
            this.MostrarImagen(ProductImage3Panel, Convert.ToString(fila["productImage3"]));
        }

        // ==================== GUARDAR (INSERT o UPDATE) ====================
        private void GuardarBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            try
            {
                // Primero se copian a la carpeta Imagenes las fotos que se acaban de elegir
                this.GuardarImagenesNuevas();

                using (DBConn dbTemp = new DBConn())
                {
                    // Si todavía no tiene marca se hace INSERT; si ya tiene, UPDATE
                    MySqlCommand comando = idEntrepreneurship == 0 ? entrepreneurshipDA.InsertCommand : entrepreneurshipDA.UpdateCommand;

                    comando.Connection = dbTemp.dbConnection;
                    comando.Connection.Open();
                    comando.Parameters.Clear();
                    comando.Parameters.AddRange(new[] {
                        new MySqlParameter("@idEntrepreneurship", idEntrepreneurship),
                        new MySqlParameter("@idEntrepreneur", this.IdEmprendedor()),
                        new MySqlParameter("@brand", BrandTextBox.Text.Trim()),
                        new MySqlParameter("@category", CategoryComboBox.SelectedItem.ToString()),
                        new MySqlParameter("@shortDescription", ShortDescriptionTextBox.Text.Trim()),
                        new MySqlParameter("@description", DescriptionTextBox.Text.Trim()),
                        new MySqlParameter("@imageHome", _imagenes[ImageHomePanel].Archivo),
                        new MySqlParameter("@image2", _imagenes[Image2Panel].Archivo),
                        new MySqlParameter("@image3", _imagenes[Image3Panel].Archivo),
                        new MySqlParameter("@productName1", ProductName1TextBox.Text.Trim()),
                        new MySqlParameter("@productName2", ProductName2TextBox.Text.Trim()),
                        new MySqlParameter("@productName3", ProductName3TextBox.Text.Trim()),
                        new MySqlParameter("@productImage1", _imagenes[ProductImage1Panel].Archivo),
                        new MySqlParameter("@productImage2", _imagenes[ProductImage2Panel].Archivo),
                        new MySqlParameter("@productImage3", _imagenes[ProductImage3Panel].Archivo)
                    });

                    comando.ExecuteNonQuery();

                    // Si fue marca nueva, se guarda su id para que el siguiente "Guardar" haga UPDATE (y no otra marca)
                    if (idEntrepreneurship == 0)
                    {
                        MySqlCommand ultimo = new MySqlCommand("SELECT LAST_INSERT_ID()", dbTemp.dbConnection);
                        idEntrepreneurship = Convert.ToInt32(ultimo.ExecuteScalar());
                    }
                }

                MessageBox.Show("Tu marca se guardó correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.LoadData();

                // Avisa a HomePage para que la sección Emprendimientos muestre la marca actualizada
                if (MarcaGuardada != null) MarcaGuardada(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar la marca", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== VALIDACIÓN ====================
        private bool Validar_campos()
        {
            if (string.IsNullOrWhiteSpace(BrandTextBox.Text))
            {
                MessageBox.Show("Escribe el nombre de tu emprendimiento.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BrandTextBox.Select();
                return false;
            }
            else if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una categoría.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.CategoryComboBox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ShortDescriptionTextBox.Text))
            {
                MessageBox.Show("Escribe la descripción corta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ShortDescriptionTextBox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Escribe la descripción de tu marca.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DescriptionTextBox.Select();
                return false;
            }
            else if (_imagenes[ImageHomePanel].Archivo == null && _imagenes[ImageHomePanel].RutaNueva == null)
            {
                MessageBox.Show("Elige la imagen principal (es la que se ve en la página de inicio).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                return true;
            }
        }

        // ==================== LIMPIAR CAMPOS ====================
        private void clearTextBoxes()
        {
            BrandTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
            ShortDescriptionTextBox.Text = "";
            DescriptionTextBox.Text = "";
            ProductName1TextBox.Text = "";
            ProductName2TextBox.Text = "";
            ProductName3TextBox.Text = "";
            foreach (Panel recuadro in _imagenes.Keys)
            {
                this.MostrarImagen(recuadro, null);
            }
        }

        // ==================== IMÁGENES ====================
        // Clic en cualquier recuadro de imagen: abre el explorador para elegir una foto
        private void ElegirImagen_Click(object sender, EventArgs e)
        {
            Panel recuadro = (Panel)sender;
            using (OpenFileDialog dialogo = new OpenFileDialog())
            {
                dialogo.Title = "Elegir " + _imagenes[recuadro].Titulo;
                dialogo.Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (dialogo.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (new FileInfo(dialogo.FileName).Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("La imagen pesa más de 5 MB. Elige una más ligera.", "Imagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Todavía no se copia: solo se guarda la ruta. Se copia al presionar "Guardar cambios".
                _imagenes[recuadro].RutaNueva = dialogo.FileName;
                _imagenes[recuadro].Vista = this.LeerImagen(dialogo.FileName);
                recuadro.Invalidate();
            }
        }

        // Muestra en el recuadro la imagen guardada en la base (solo el nombre del archivo)
        private void MostrarImagen(Panel recuadro, string archivo)
        {
            CampoImagen campo = _imagenes[recuadro];
            campo.Archivo = string.IsNullOrWhiteSpace(archivo) ? null : archivo.Trim();
            campo.RutaNueva = null;
            campo.Vista = campo.Archivo == null ? null : this.LeerImagen(Path.Combine(Application.StartupPath, CarpetaImagenes, campo.Archivo));
            recuadro.Invalidate();
        }

        // Copia a la carpeta Imagenes las fotos nuevas y cambia su "Archivo" por el nombre con el que quedó
        private void GuardarImagenesNuevas()
        {
            string carpeta = Path.Combine(Application.StartupPath, CarpetaImagenes);
            Directory.CreateDirectory(carpeta);

            foreach (CampoImagen campo in _imagenes.Values)
            {
                if (campo.RutaNueva == null) continue;

                // Se le agrega la fecha al nombre para que dos fotos con el mismo nombre no se encimen
                string nombre = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Path.GetFileName(campo.RutaNueva);
                File.Copy(campo.RutaNueva, Path.Combine(carpeta, nombre), true);
                campo.Archivo = nombre;
                campo.RutaNueva = null;
            }
        }

        // Lee una imagen sin dejar el archivo bloqueado. Si no existe, regresa null.
        private Image LeerImagen(string ruta)
        {
            if (!File.Exists(ruta)) return null;
            using (Image original = Image.FromFile(ruta))
            {
                return new Bitmap(original);
            }
        }

        // ==================== OTROS BOTONES ====================
        // Cancelar: descarta lo que no se guardó y vuelve a mostrar lo que hay en la base
        private void CancelarBoton_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }

        // Vista previa: muestra cómo se verá la tarjeta en la página de emprendimientos
        private void VistaPreviaBoton_Click(object sender, EventArgs e)
        {
            TarjetaEmprendimiento tarjeta = new TarjetaEmprendimiento();
            tarjeta.Forma = TarjetaEmprendimiento.Formas.Vertical;
            tarjeta.Titulo = BrandTextBox.Text.Trim().Length > 0 ? BrandTextBox.Text.Trim() : "Nombre de tu emprendimiento";
            tarjeta.Descripcion = ShortDescriptionTextBox.Text.Trim().Length > 0 ? ShortDescriptionTextBox.Text.Trim() : "Descripción corta";
            tarjeta.Categoria = CategoryComboBox.SelectedItem != null ? CategoryComboBox.SelectedItem.ToString() : "Categoría";
            tarjeta.Imagen = _imagenes[ImageHomePanel].Vista;
            tarjeta.Location = new Point(30, 30);
            tarjeta.Size = new Size(291, 358);

            Form vista = new Form();
            vista.Text = "Vista previa";
            vista.BackColor = Color.White;
            vista.FormBorderStyle = FormBorderStyle.FixedDialog;
            vista.MaximizeBox = false;
            vista.MinimizeBox = false;
            vista.StartPosition = FormStartPosition.CenterParent;
            vista.ClientSize = new Size(351, 418);
            vista.Controls.Add(tarjeta);
            vista.ShowDialog(this.FindForm());
        }

        // Flecha de regresar: HomePage regresa a "Mis Marcas"
        private void RegresarBoton_Click(object sender, EventArgs e)
        {
            if (Regresar != null) Regresar(this, EventArgs.Empty);
        }

        /// <summary>Datos de cada recuadro de imagen.</summary>
        private class CampoImagen
        {
            public string Titulo;       // "Imagen principal", "Producto Imagen 1"...
            public string Archivo;      // nombre del archivo guardado en la base (null = sin imagen)
            public string RutaNueva;    // foto recién elegida que todavía no se guarda (null = ninguna)
            public Image Vista;         // la imagen que se dibuja en el recuadro

            public CampoImagen(string titulo)
            {
                Titulo = titulo;
            }
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame "PersonalzaMarca" en Figma sin el encabezado (que ahora está en HomePage): 1280 × 720.
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 720f;

        private static readonly Color FondoPagina = Color.White;                      // fondo blanco como en el Figma "Editar Marca"
        private static readonly Color FondoFranja = Color.White;                      // fondo del botón de regresar
        private static readonly Color GrisMenu = Color.FromArgb(240, 243, 244);       // #F0F3F4
        private static readonly Color Naranja = Color.FromArgb(248, 110, 56);         // #F86E38
        private static readonly Color Celeste = Color.FromArgb(56, 189, 248);         // #38BDF8
        private static readonly Color AzulGuardar = Color.FromArgb(8, 127, 223);      // #087FDF
        private static readonly Color BordeCampo = Color.FromArgb(222, 221, 231);     // #DEDDE7
        private static readonly Color FondoDerecha = Color.FromArgb(250, 249, 253);   // #FAF9FD
        private static readonly Color LineaSuave = Color.FromArgb(236, 235, 241);     // #ECEBF1

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private float _escala = 1f;
        private float _margenX, _margenY;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // Las imágenes de fondo son solo la vista previa del diseñador; al ejecutar todo se dibuja con código.
            BackgroundMarca.BackgroundImage = null;
            TarjetaPanel.BackgroundImage = null;


            foreach (Control c in new Control[] { BackgroundMarca, TarjetaPanel, CuentaActivaPanel })
                ActivarDobleBuffer(c);
            foreach (Panel recuadro in _imagenes.Keys)
                ActivarDobleBuffer(recuadro);

            // Campos de texto: texto guía (viene de Tag) y borde azul cuando están activos
            foreach (Control control in TarjetaPanel.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;
                caja.HandleCreated += (s, e) => PonerTextoGuia(caja);
                caja.GotFocus += (s, e) => TarjetaPanel.Invalidate();
                caja.LostFocus += (s, e) => TarjetaPanel.Invalidate();
            }

            // Botones redondeados
            foreach (Button boton in new[] { VistaPreviaBoton, CancelarBoton, GuardarBoton })
            {
                Button b = boton;
                b.Paint += Boton_Paint;
                b.MouseEnter += (s, e) => b.Invalidate();
                b.MouseLeave += (s, e) => b.Invalidate();
            }
            BotonCircular.HacerBotonRegresar(RegresarBoton, FondoFranja);   // el mismo botón de regresar de toda la app

            // Lista de categorías: se dibuja a mano para mostrar "Elige una categoría" cuando está vacía
            CategoryComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            CategoryComboBox.DrawItem += CategoryComboBox_DrawItem;
            CategoryComboBox.GotFocus += (s, e) => TarjetaPanel.Invalidate();
            CategoryComboBox.LostFocus += (s, e) => TarjetaPanel.Invalidate();


            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            BackgroundMarca.Paint += BackgroundMarca_Paint;
            TarjetaPanel.Paint += TarjetaPanel_Paint;
            TarjetaPanel.MouseDown += TarjetaPanel_MouseDown;
            CuentaActivaPanel.Paint += CuentaActivaPanel_Paint;
            foreach (Panel recuadro in _imagenes.Keys)
            {
                Panel r = recuadro;
                r.Paint += RecuadroImagen_Paint;
                r.MouseEnter += (s, e) => r.Invalidate();
                r.MouseLeave += (s, e) => r.Invalidate();
            }
            Resize += (s, e) => AjustarATamano();

            // Se guardan las posiciones y fuentes del diseñador (1280 × 832) para escalarlas después.
            GuardarOriginales(BackgroundMarca);
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

        private static void ActivarDobleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           .SetValue(control, true, null);
        }

        private static void PonerTextoGuia(TextBoxBase caja)
        {
            string guia = caja.Tag as string;
            if (string.IsNullOrEmpty(guia)) return;
            try { SendMessage(caja.Handle, EM_SETCUEBANNER, (IntPtr)1, guia); }
            catch (Exception) { /* solo falla fuera de Windows */ }
        }

        /// <summary>
        /// Hace crecer o encoger todo para llenar la ventana: el contenido crece parejo (sin deformarse) y centrado;
        /// el fondo y la franja gris ocupan todo el ancho.
        /// </summary>
        private void AjustarATamano()
        {
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _escala = Math.Min(cliente.Width / AnchoFigma, cliente.Height / AltoFigma);
            _margenX = (cliente.Width - AnchoFigma * _escala) / 2f;
            _margenY = (cliente.Height - AltoFigma * _escala) / 2f;

            SuspendLayout();
            BackgroundMarca.Bounds = new Rectangle(0, 0, cliente.Width, cliente.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BackgroundMarca;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? _margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? _margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            // La lista de categorías se dibuja a mano: su alto sigue al tamaño de la letra
            CategoryComboBox.ItemHeight = Math.Max(10, (int)Math.Round(19 * _escala));
            ResumeLayout(true);
            BackgroundMarca.Invalidate(true);
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

        // ---------- Dibujo ----------
        /// <summary>Fondo blanco (diseño "Editar Marca" del Figma).</summary>
        private void BackgroundMarca_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(FondoPagina);
        }

        /// <summary>Tarjeta blanca: encabezado, columna derecha, números naranjas, cajas de texto y productos.</summary>
        private void TarjetaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(FondoPagina);
            float k = _escala;

            RectangleF tarjeta = new RectangleF(1, 0, TarjetaPanel.Width - 3, TarjetaPanel.Height - 6 * k);
            for (int i = 6; i >= 1; i--)
            {
                float d = i * k;
                RectangleF capa = new RectangleF(tarjeta.X - d * 0.5f, tarjeta.Y + d, tarjeta.Width + d, tarjeta.Height);
                using (GraphicsPath p = Redondeado(capa, 9 * k + d))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(5, 31, 34, 76)))
                    g.FillPath(b, p);
            }

            using (GraphicsPath ruta = Redondeado(tarjeta, 9 * k))
            {
                g.FillPath(Brushes.White, ruta);

                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                // Columna derecha (productos) con fondo lila muy claro
                using (SolidBrush b = new SolidBrush(FondoDerecha))
                    g.FillRectangle(b, 495 * k, 60 * k, tarjeta.Width, tarjeta.Height);
                using (Pen linea = new Pen(LineaSuave, Math.Max(1f, 0.8f * k)))
                {
                    g.DrawLine(linea, 0, 60 * k, tarjeta.Width, 60 * k);
                    g.DrawLine(linea, 495 * k, 60 * k, 495 * k, tarjeta.Height);
                }
                g.Clip = anterior;

                using (Pen borde = new Pen(Color.FromArgb(224, 223, 234), Math.Max(1f, 0.8f * k)))
                    g.DrawPath(borde, ruta);
            }

            // Números de sección
            Numero(g, "01", new RectangleF(24 * k, 75 * k, 24 * k, 24 * k), k);
            Numero(g, "02", new RectangleF(24 * k, 298 * k, 24 * k, 24 * k), k);
            Numero(g, "03", new RectangleF(519 * k, 75 * k, 24 * k, 24 * k), k);

            // Tarjetitas de los 3 productos
            float[] yProducto = { 110, 232, 354 };
            for (int i = 0; i < 3; i++)
            {
                RectangleF caja = new RectangleF(519 * k, yProducto[i] * k, 534 * k, 112 * k);
                using (GraphicsPath p = Redondeado(caja, 7 * k))
                using (Pen borde = new Pen(Color.FromArgb(228, 226, 236), Math.Max(1f, 0.8f * k)))
                {
                    g.FillPath(Brushes.White, p);
                    g.DrawPath(borde, p);
                }
                Numero(g, "0" + (i + 1), new RectangleF(530 * k, (yProducto[i] + 11) * k, 30 * k, 30 * k), k);
            }

            // Caja de Categoría: mismo alto y estilo que la de "Nombre Emprendimiento", a su lado
            {
                ComboBox lista = CategoryComboBox;
                RectangleF r = new RectangleF(lista.Left - 10 * k, BrandTextBox.Top - 7 * k, lista.Width + 20 * k, BrandTextBox.Height + 12 * k);
                using (GraphicsPath p = Redondeado(r, 5 * k))
                using (Pen borde = new Pen(lista.Focused ? Celeste : BordeCampo, Math.Max(1f, lista.Focused ? 1.5f * k : 0.8f * k)))
                {
                    g.FillPath(Brushes.White, p);
                    g.DrawPath(borde, p);
                }
            }

            // Cajas alrededor de cada TextBox (el TextBox no tiene borde)
            foreach (Control control in TarjetaPanel.Controls)
            {
                TextBoxBase campo = control as TextBoxBase;
                if (campo == null) continue;
                RectangleF r = new RectangleF(campo.Left - 10 * k, campo.Top - 7 * k, campo.Width + 20 * k, campo.Height + 12 * k);
                using (GraphicsPath p = Redondeado(r, 5 * k))
                using (Pen borde = new Pen(campo.Focused ? Celeste : BordeCampo, Math.Max(1f, campo.Focused ? 1.5f * k : 0.8f * k)))
                {
                    g.FillPath(Brushes.White, p);
                    g.DrawPath(borde, p);
                }
            }
        }

        /// <summary>Opciones de la lista de categorías; si no hay ninguna elegida muestra el texto guía en gris.</summary>
        private void CategoryComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool enCaja = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
            bool marcada = (e.State & DrawItemState.Selected) == DrawItemState.Selected && !enCaja;
            using (SolidBrush fondo = new SolidBrush(marcada ? Color.FromArgb(224, 242, 254) : Color.White))
                e.Graphics.FillRectangle(fondo, e.Bounds);

            string texto = e.Index >= 0 ? CategoryComboBox.Items[e.Index].ToString() : "Elige una categoría";
            Color color = e.Index >= 0 ? Color.FromArgb(40, 40, 52) : Color.FromArgb(170, 168, 180);
            Rectangle area = new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, texto, CategoryComboBox.Font, area, color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static void Numero(Graphics g, string texto, RectangleF r, float k)
        {
            using (GraphicsPath p = Redondeado(r, 6 * k))
            using (SolidBrush b = new SolidBrush(Naranja))
                g.FillPath(b, p);
            using (Font f = new Font(FuenteInstalada("Plus Jakarta Sans") ? "Plus Jakarta Sans" : "Segoe UI", Math.Max(1f, 10 * k), FontStyle.Bold, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, texto, f, Rectangle.Round(r), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        // Clic en la caja (fuera del texto) también pone el cursor en el campo
        private void TarjetaPanel_MouseDown(object sender, MouseEventArgs e)
        {
            float k = _escala;
            foreach (Control control in TarjetaPanel.Controls)
            {
                if (!(control is TextBoxBase)) continue;
                RectangleF r = new RectangleF(control.Left - 10 * k, control.Top - 7 * k, control.Width + 20 * k, control.Height + 12 * k);
                if (r.Contains(e.Location)) { control.Focus(); return; }
            }
        }

        /// <summary>Recuadro de imagen con borde punteado celeste: ícono o miniatura, título y tamaño.</summary>
        private void RecuadroImagen_Paint(object sender, PaintEventArgs e)
        {
            Panel recuadro = (Panel)sender;
            CampoImagen campo = _imagenes[recuadro];
            bool esProducto = recuadro == ProductImage1Panel || recuadro == ProductImage2Panel || recuadro == ProductImage3Panel;
            Graphics g = e.Graphics;
            Calidad(g);
            float k = _escala;
            g.Clear(Color.White);   // los recuadros están sobre fondo blanco

            bool lleno = campo.Vista != null;
            bool encima = recuadro.ClientRectangle.Contains(recuadro.PointToClient(Cursor.Position));
            RectangleF r = new RectangleF(1, 1, recuadro.Width - 3, recuadro.Height - 3);
            using (GraphicsPath p = Redondeado(r, 6 * k))
            {
                Color fondo = lleno || encima ? Color.FromArgb(232, 247, 254) : FondoDerecha;
                using (SolidBrush b = new SolidBrush(fondo)) g.FillPath(b, p);
                using (Pen borde = new Pen(Celeste, Math.Max(1f, 0.9f * k)))
                {
                    borde.DashStyle = DashStyle.Dash;
                    g.DrawPath(borde, p);
                }
            }

            // Miniatura de la foto elegida, o el ícono de imagen si todavía no hay
            float lado = lleno ? recuadro.Height - 14 * k : 24 * k;
            RectangleF cuadro = new RectangleF(8 * k, (recuadro.Height - lado) / 2f, lado, lado);
            using (GraphicsPath p = Redondeado(cuadro, 5 * k))
            {
                if (lleno)
                {
                    Region anterior = g.Clip;
                    g.SetClip(p, CombineMode.Intersect);
                    float escala = Math.Max(cuadro.Width / campo.Vista.Width, cuadro.Height / campo.Vista.Height);
                    float w = campo.Vista.Width * escala, h = campo.Vista.Height * escala;
                    g.DrawImage(campo.Vista, cuadro.X + (cuadro.Width - w) / 2f, cuadro.Y + (cuadro.Height - h) / 2f, w, h);
                    g.Clip = anterior;
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(36, 56, 189, 248))) g.FillPath(b, p);
                    IconoImagen(g, cuadro, k);
                }
            }

            // Textos
            float x = cuadro.Right + 8 * k;
            string fuente = FuenteInstalada("Plus Jakarta Sans") ? "Plus Jakarta Sans" : "Segoe UI";
            string detalle = campo.RutaNueva != null ? Path.GetFileName(campo.RutaNueva)
                           : campo.Archivo != null ? "Imagen guardada"
                           : "PNG o JPG · Máx. 5 MB";
            float anchoTexto = recuadro.Width - x - (esProducto ? 52 * k : 6 * k);
            using (Font fTitulo = new Font(fuente, Math.Max(1f, 9.5f * k), FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font fDetalle = new Font(fuente, Math.Max(1f, 8.5f * k), FontStyle.Regular, GraphicsUnit.Pixel))
            {
                // En los recuadros angostos (Imagen 2 y 3) el texto completo no cabe: se usa una versión más corta
                if (campo.RutaNueva == null && campo.Archivo == null)
                    foreach (string opcion in new[] { "PNG o JPG · Máx. 5 MB", "PNG o JPG · 5 MB", "Máx. 5 MB" })
                    {
                        detalle = opcion;
                        if (TextRenderer.MeasureText(g, opcion, fDetalle, Size.Empty, TextFormatFlags.NoPadding).Width <= anchoTexto) break;
                    }

                float cy = recuadro.Height / 2f;
                TextRenderer.DrawText(g, campo.Titulo, fTitulo, Rectangle.Round(new RectangleF(x, cy - 14 * k, anchoTexto, 14 * k)),
                    Color.FromArgb(77, 79, 98), TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, detalle, fDetalle, Rectangle.Round(new RectangleF(x, cy + 1 * k, anchoTexto, 13 * k)),
                    Color.FromArgb(170, 168, 180), TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                if (esProducto)
                {
                    // Etiqueta "Elegir" / "Cambiar" a la derecha
                    string texto = lleno ? "Cambiar" : "Elegir";
                    RectangleF pildora = new RectangleF(recuadro.Width - 50 * k, cy - 9 * k, 42 * k, 18 * k);
                    using (GraphicsPath p = Redondeado(pildora, 8 * k))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(36, 56, 189, 248)))
                        g.FillPath(b, p);
                    using (Font fElegir = new Font(fuente, Math.Max(1f, 8 * k), FontStyle.Bold, GraphicsUnit.Pixel))
                        TextRenderer.DrawText(g, texto, fElegir, Rectangle.Round(pildora), Celeste,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        /// <summary>Ícono de "imagen": un marco con una montaña y un sol.</summary>
        private static void IconoImagen(Graphics g, RectangleF caja, float k)
        {
            RectangleF r = new RectangleF(caja.X + caja.Width * 0.25f, caja.Y + caja.Height * 0.27f, caja.Width * 0.5f, caja.Height * 0.46f);
            using (Pen p = new Pen(Celeste, Math.Max(1f, 1.3f * k)))
            {
                p.LineJoin = LineJoin.Round;
                using (GraphicsPath marco = Redondeado(r, 1.5f * k)) g.DrawPath(p, marco);
                g.DrawLines(p, new[] { new PointF(r.X, r.Bottom - r.Height * 0.15f), new PointF(r.X + r.Width * 0.4f, r.Y + r.Height * 0.5f),
                                       new PointF(r.X + r.Width * 0.65f, r.Y + r.Height * 0.75f), new PointF(r.Right, r.Y + r.Height * 0.45f) });
                g.DrawEllipse(p, r.X + r.Width * 0.62f, r.Y + r.Height * 0.15f, r.Width * 0.18f, r.Width * 0.18f);
            }
        }

        /// <summary>Tarjeta "Cuenta activa": degradado naranja → rosa con anillos.</summary>
        private void CuentaActivaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF r = new RectangleF(0, 0, CuentaActivaPanel.Width - 1, CuentaActivaPanel.Height - 1);
            using (GraphicsPath ruta = Redondeado(r, 20 * k))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(r, Naranja, Color.FromArgb(229, 11, 182), 35f))
                    g.FillPath(b, ruta);
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                using (Pen anillo = new Pen(Color.FromArgb(64, 248, 245, 56), 9.6f * k))
                {
                    float d = 70 * k, m = 4.8f * k;
                    g.DrawEllipse(anillo, 376 * k + m, 4 * k + m, d - 2 * m, d - 2 * m);
                    g.DrawEllipse(anillo, 306 * k + m, 40 * k + m, d - 2 * m, d - 2 * m);
                    g.DrawEllipse(anillo, 376 * k + m, 84 * k + m, d - 2 * m, d - 2 * m);
                }
                g.Clip = anterior;
            }
        }


        /// <summary>Botones: vista previa / Cancelar (blancos) y Guardar cambios (azul).</summary>
        private void Boton_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            float k = _escala;
            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));

            g.Clear(FondoDerecha);   // los tres botones están sobre la columna derecha
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k);
            Sombra(g, area, area.Height / 2f, k);
            bool azul = boton == GuardarBoton;
            Color fondo = azul ? AzulGuardar : Color.White;
            if (encima) fondo = azul ? ControlPaint.Light(AzulGuardar, 0.15f) : Color.FromArgb(246, 246, 250);
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            {
                using (SolidBrush b = new SolidBrush(fondo)) g.FillPath(b, ruta);
                if (!azul)
                    using (Pen borde = new Pen(Color.FromArgb(213, 215, 221), Math.Max(1f, 0.8f * k))) g.DrawPath(borde, ruta);
            }
            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), boton.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private static void Calidad(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        private static GraphicsPath Redondeado(RectangleF r, float radio)
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
        private static void Sombra(Graphics g, RectangleF r, float radio, float k)
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

        #endregion
    }
}

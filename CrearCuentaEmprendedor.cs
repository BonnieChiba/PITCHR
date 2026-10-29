using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    public partial class CrearCuentaEmprendedor : Form
    {
        private DBConn dbconn;                                              //Conectar
        private MySqlDataAdapter entrepreneursDA = new MySqlDataAdapter();  //Extraer
        private DataTable entrepreneurtable = new DataTable();              //Almacenar
        private BindingSource entrepreneurDataBind = new BindingSource();   //Vincular
        private int maxID = 0;

        // Rol de la cuenta (columna "role"): 1 = emprendedor, 0 = inversor.
        private const int RolEmprendedor = 1;
        private int role;   // se asigna solo cuando todos los campos ya se verificaron

        public CrearCuentaEmprendedor()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)

            // Las pantallas se ocultan con Hide(); si cierran esta ventana con la X, se cierra toda la aplicación.
            this.FormClosed += (s, e) => Application.Exit();

            // El RFC solo acepta letras, números, Ñ y &, en mayúsculas y máximo 13 caracteres.
            this.RFCEmprendedorTextbox.KeyPress += RFCEmprendedorTextbox_KeyPress;
            this.RFCEmprendedorTextbox.TextChanged += RFCEmprendedorTextbox_TextChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
            this.BindControls();
            this.clearTextBoxes();
        }

        // Si su tabla o columnas se llaman distinto en MySQL, solo cambien los nombres en estas consultas
        // (y en la de ExistenDuplicados, más abajo).
        private void LoadDBCommands()
        {
            entrepreneursDA.SelectCommand = new MySqlCommand("SELECT idEntrepreneur, name, lastName, `user`, city, email, rfc, termsEntrepreneur FROM integrative.entrepreneur ORDER BY idEntrepreneur");

            entrepreneursDA.InsertCommand = new MySqlCommand("INSERT INTO integrative.entrepreneur (name, lastName, `user`, city, email, rfc, termsEntrepreneur, role, `password`) VALUES (@name, @lastName, @user, @city, @email, @rfc, @termsEntrepreneur, @role, @password)");
        }

        private void LoadData()
        {
            dbconn = new DBConn();
            entrepreneursDA.SelectCommand.Connection = dbconn.dbConnection;
            try
            {
                entrepreneursDA.Fill(entrepreneurtable);
                object computeResult = entrepreneurtable.Compute("MAX(idEntrepreneur)", "");
                if (computeResult != DBNull.Value)
                {
                    maxID = Convert.ToInt32(computeResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BindControls()
        {
            entrepreneurDataBind.DataSource = entrepreneurtable;

            // Igual que en CrearCuentaInversor: en una pantalla de REGISTRO no se vinculan las cajas de texto
            // a la tabla (mostrarían y editarían en memoria al primer emprendedor guardado).
            // El texto gris de ejemplo lo pone PrepararDiseno() con la propiedad Tag de cada caja.
        }

        // ==================== BOTÓN REGISTRARSE ====================
        private void RegistrarseBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            // Antes de guardar, revisa que el usuario, el email y el RFC no estén ya registrados.
            if (this.ExistenDuplicados() == true)
            {
                return;
            }

            // Todos los campos están completos y correctos: se le asigna el rol de emprendedor.
            this.role = RolEmprendedor;

            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    entrepreneursDA.InsertCommand.Connection = dbTemp.dbConnection;
                    entrepreneursDA.InsertCommand.Connection.Open();
                    entrepreneursDA.InsertCommand.Parameters.Clear();
                    entrepreneursDA.InsertCommand.Parameters.AddRange(new[] {
                        new MySqlParameter("@name", NombreTextbox.Text.Trim()),
                        new MySqlParameter("@lastName", ApellidoTextbox.Text.Trim()),
                        new MySqlParameter("@user", UsuarioTexto.Text.Trim()),
                        new MySqlParameter("@email", EmailTextbox.Text.Trim()),
                        new MySqlParameter("@city", CiudadEmprendedorTextBox.Text.Trim()),
                        new MySqlParameter("@rfc", RFCEmprendedorTextbox.Text.Trim().ToUpper()),
                        new MySqlParameter("@termsEntrepreneur", TerminosCheckbox.Checked),
                        new MySqlParameter("@role", this.role),
                        new MySqlParameter("@password", ContraseñaTextBox.Text)
                        // Recomendado: BCrypt.Net.BCrypt.HashPassword(ContraseñaTextBox.Text)
                    });

                    entrepreneursDA.InsertCommand.ExecuteNonQuery();

                    entrepreneurtable.Clear();
                    entrepreneursDA.Fill(entrepreneurtable);
                    maxID = Convert.ToInt32(entrepreneurtable.Compute("MAX(idEntrepreneur)", ""));

                    this.clearTextBoxes();
                    MessageBox.Show("Registro insertado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Solo se avanza si el INSERT salió bien.
                // Cuando agreguen la pantalla de iniciar sesión, quiten las // de estas 3 líneas:
                // IniciarSesion PantallaPrincipal = new IniciarSesion();
                // PantallaPrincipal.Show();
                // this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al insertar registro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== VALIDACIÓN ====================
        private bool Validar_campos()
        {
            if (string.IsNullOrWhiteSpace(UsuarioTexto.Text))
            {
                MessageBox.Show("El campo de usuario está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.UsuarioTexto.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ContraseñaTextBox.Text))
            {
                MessageBox.Show("El campo de contraseña está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ContraseñaTextBox.Select();
                return false;
            }
            else if (ContraseñaTextBox.Text != ConfirmarContraseñaTextbox.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ConfirmarContraseñaTextbox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(EmailTextbox.Text) || !EmailTextbox.Text.Contains("@"))
            {
                MessageBox.Show("Ingresa un email válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.EmailTextbox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(NombreTextbox.Text))
            {
                MessageBox.Show("El campo de nombre está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.NombreTextbox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ApellidoTextbox.Text))
            {
                MessageBox.Show("El campo de apellido está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ApellidoTextbox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(CiudadEmprendedorTextBox.Text))
            {
                MessageBox.Show("El campo de ciudad está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.CiudadEmprendedorTextBox.Select();
                return false;
            }
            // El RFC debe tener 12 caracteres (persona moral) o 13 (persona física).
            else if (RFCEmprendedorTextbox.Text.Trim().Length != 12 && RFCEmprendedorTextbox.Text.Trim().Length != 13)
            {
                MessageBox.Show("El RFC está incompleto (debe tener 12 o 13 caracteres).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.RFCEmprendedorTextbox.Select();
                return false;
            }
            else if (!TerminosCheckbox.Checked)
            {
                MessageBox.Show("Debes aceptar los términos y condiciones.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.TerminosCheckbox.Select();
                return false;
            }
            else
            {
                return true;
            }
        }

        // ==================== EVITAR DUPLICADOS ====================
        // Busca en la base de datos si ya existe un emprendedor con el mismo usuario, email o RFC.
        // Si encuentra alguno, muestra un mensaje que dice cuál(es) y regresa true (no se debe registrar).
        private bool ExistenDuplicados()
        {
            string usuario = UsuarioTexto.Text.Trim();
            string email = EmailTextbox.Text.Trim();
            string rfc = RFCEmprendedorTextbox.Text.Trim().ToUpper();

            bool usuarioRepetido = false;
            bool emailRepetido = false;
            bool rfcRepetido = false;

            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    MySqlCommand buscar = new MySqlCommand(
                        "SELECT `user`, email, rfc FROM integrative.entrepreneur " +
                        "WHERE `user` = @user OR email = @email OR rfc = @rfc", dbTemp.dbConnection);
                    buscar.Parameters.AddWithValue("@user", usuario);
                    buscar.Parameters.AddWithValue("@email", email);
                    buscar.Parameters.AddWithValue("@rfc", rfc);

                    dbTemp.dbConnection.Open();
                    using (MySqlDataReader lector = buscar.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            if (string.Equals(lector["user"].ToString().Trim(), usuario, StringComparison.OrdinalIgnoreCase)) usuarioRepetido = true;
                            if (string.Equals(lector["email"].ToString().Trim(), email, StringComparison.OrdinalIgnoreCase)) emailRepetido = true;
                            if (string.Equals(lector["rfc"].ToString().Trim(), rfc, StringComparison.OrdinalIgnoreCase)) rfcRepetido = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo revisar si los datos ya existen:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;   // si no se pudo revisar, mejor no registrar
            }

            if (usuarioRepetido == false && emailRepetido == false && rfcRepetido == false)
            {
                return false;
            }

            string mensaje = "Ya existe una cuenta registrada con:\n";
            if (usuarioRepetido) mensaje += "\n•  El usuario \"" + usuario + "\"";
            if (emailRepetido) mensaje += "\n•  El email \"" + email + "\"";
            if (rfcRepetido) mensaje += "\n•  El RFC \"" + rfc + "\"";
            mensaje += "\n\nPor favor usa datos diferentes.";
            MessageBox.Show(mensaje, "Datos ya registrados", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Pone el cursor en el primer campo repetido
            if (usuarioRepetido) this.UsuarioTexto.Select();
            else if (emailRepetido) this.EmailTextbox.Select();
            else this.RFCEmprendedorTextbox.Select();

            return true;
        }

        // ==================== RFC: SOLO 12 A 13 CARACTERES ====================
        private const int RfcMaximo = 13;

        // Bloquea al escribir: solo letras, números, Ñ y &, y no deja pasar de 13 caracteres.
        private void RFCEmprendedorTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;   // permite borrar, Ctrl+C, Ctrl+V, etc.

            bool caracterValido = char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '&';
            int seleccionados = RFCEmprendedorTextbox.SelectionLength;
            bool hayEspacio = RFCEmprendedorTextbox.Text.Length - seleccionados < RfcMaximo;

            if (!caracterValido || !hayEspacio)
            {
                e.Handled = true;   // no escribe la tecla
            }
        }

        // Por si pegan texto: lo pasa a mayúsculas, quita lo que no sea válido y lo corta a 13 caracteres.
        private void RFCEmprendedorTextbox_TextChanged(object sender, EventArgs e)
        {
            string limpio = "";
            foreach (char c in RFCEmprendedorTextbox.Text.ToUpper())
            {
                if ((char.IsLetterOrDigit(c) || c == '&') && limpio.Length < RfcMaximo)
                {
                    limpio += c;
                }
            }

            if (RFCEmprendedorTextbox.Text != limpio)
            {
                int cursor = Math.Min(RFCEmprendedorTextbox.SelectionStart, limpio.Length);
                RFCEmprendedorTextbox.Text = limpio;
                RFCEmprendedorTextbox.SelectionStart = cursor;
            }
        }

        // ==================== LIMPIAR CAMPOS ====================
        private void clearTextBoxes()
        {
            this.UsuarioTexto.Text = "";
            this.ContraseñaTextBox.Text = "";
            this.ConfirmarContraseñaTextbox.Text = "";
            this.EmailTextbox.Text = "";
            this.NombreTextbox.Text = "";
            this.ApellidoTextbox.Text = "";
            this.CiudadEmprendedorTextBox.Text = "";
            this.RFCEmprendedorTextbox.Clear();
            this.TerminosCheckbox.Checked = false;
        }

        // ==================== REGRESAR ====================
        private void RegresarPictureBox_Click(object sender, EventArgs e)
        {
            IniciaAqui pantalla = new IniciaAqui();
            pantalla.Show();
            this.Hide();
        }

        // Clic en "ver Terminos y condiciones"
        private void TerminosLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Aquí van los Términos y Condiciones de Pitcher.", "Términos y Condiciones");
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame "Login enterprising" en Figma: todo el diseño está hecho en 1280 × 832.
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 832f;

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private Image _ondaArriba;
        private Image _ondaAbajo;
        private Bitmap _fondoListo;
        private float _escala = 1f;   // cuánto creció el contenido
        private float _escalaX = 1f;  // cuánto se estiró el fondo a lo ancho
        private float _escalaY = 1f;  // cuánto se estiró el fondo a lo alto

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);
        private static readonly Color GrisBorde = Color.FromArgb(217, 217, 217);

        // Posición de las ondas naranjas en el Figma (y, alto)
        private const float OndaArribaAlto = 282f;
        private const float OndaAbajoY = 612f;
        private const float OndaAbajoAlto = 220f;
        private const float AltoPildora = 40f;   // un poco más bajos que en el Figma (43) para que quepan Nombre y Apellido

        // Mensaje de Windows para poner texto guía gris dentro de un TextBox ("Usuario", "Contraseña"...).
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // Las ondas se dibujan a mano para que una pase por debajo y otra por encima del cuadro blanco.
            _ondaArriba = LineaSuperiorPicturebox.Image;
            _ondaAbajo = pictureBox1.Image;
            LineaSuperiorPicturebox.Visible = false;
            pictureBox1.Visible = false;

            // Las imágenes de fondo son solo la vista previa del diseñador; al ejecutar todo se dibuja con código.
            BackgroundEmprendedor.BackgroundImage = null;
            CuadroBlancoTexto.BackgroundImage = null;
            RegistrarseBoton.BackgroundImage = null;

            // El cuadro blanco se dibuja redondeado y con sombra en CuadroBlancoTexto_Paint.
            CuadroBlancoTexto.BackColor = Color.Transparent;
            ActivarDobleBuffer(BackgroundEmprendedor);
            ActivarDobleBuffer(CuadroBlancoTexto);

            // Botón píldora: el fondo lo pinta RegistrarseBoton_Paint.
            RegistrarseBoton.BackColor = Color.Transparent;
            RegistrarseBoton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            RegistrarseBoton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            RegistrarseBoton.MouseEnter += (s, e) => RegistrarseBoton.Invalidate();
            RegistrarseBoton.MouseLeave += (s, e) => RegistrarseBoton.Invalidate();

            // Campos: texto guía (viene de la propiedad Tag) y borde azul cuando están activos.
            foreach (Control control in CuadroBlancoTexto.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;
                caja.AutoSize = false;   // el alto lo pone AcomodarCampos para que no se corten las letras
                caja.HandleCreated += (s, e) => PonerTextoGuia(caja);
                caja.GotFocus += (s, e) => CuadroBlancoTexto.Invalidate();
                caja.LostFocus += (s, e) => CuadroBlancoTexto.Invalidate();
            }

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            BackgroundEmprendedor.Paint += BackgroundEmprendedor_Paint;
            CuadroBlancoTexto.Paint += CuadroBlancoTexto_Paint;
            CuadroBlancoTexto.MouseDown += CuadroBlancoTexto_MouseDown;
            RegistrarseBoton.Paint += RegistrarseBoton_Paint;
            TerminosCheckbox.Paint += TerminosCheckbox_Paint;
            TerminosCheckbox.CheckedChanged += TerminosCheckbox_CheckedChanged;
            Resize += CrearCuentaEmprendedor_Resize;

            // Se guardan las posiciones y fuentes del diseñador (1280 × 832) para escalarlas después.
            GuardarOriginales(BackgroundEmprendedor);
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
            try
            {
                SendMessage(caja.Handle, EM_SETCUEBANNER, (IntPtr)1, guia);
            }
            catch (Exception)
            {
                // Solo falla fuera de Windows.
            }
        }

        private void CrearCuentaEmprendedor_Resize(object sender, EventArgs e)
        {
            AjustarATamano();
        }

        /// <summary>
        /// Hace crecer o encoger todo para llenar la ventana:
        /// el fondo se estira completo y el contenido crece parejo (sin deformarse) y centrado.
        /// </summary>
        private void AjustarATamano()
        {
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _escalaX = cliente.Width / AnchoFigma;
            _escalaY = cliente.Height / AltoFigma;
            _escala = Math.Min(_escalaX, _escalaY);
            float margenX = (cliente.Width - AnchoFigma * _escala) / 2f;
            float margenY = (cliente.Height - AltoFigma * _escala) / 2f;

            SuspendLayout();
            BackgroundEmprendedor.Bounds = new Rectangle(0, 0, cliente.Width, cliente.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BackgroundEmprendedor;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            AcomodarCampos();
            ResumeLayout(true);

            PrepararFondo(cliente);
            BackgroundEmprendedor.Invalidate(true);
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
                fuente = new Font(nombre, pixeles, original.Style, GraphicsUnit.Pixel);
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

        /// <summary>Dibuja una sola vez (por tamaño) el fondo: margen blanco, rectángulo azul y ondas naranjas.</summary>
        private void PrepararFondo(Size tamano)
        {
            Bitmap nuevo = new Bitmap(tamano.Width, tamano.Height);
            using (Graphics g = Graphics.FromImage(nuevo))
            {
                Calidad(g);
                g.Clear(Color.White);
                RectangleF azul = new RectangleF(19 * _escalaX, 19 * _escalaY, 1241 * _escalaX, 794 * _escalaY);
                using (GraphicsPath ruta = Redondeado(azul, 20 * _escala))
                using (SolidBrush brocha = new SolidBrush(AzulPitcher))
                    g.FillPath(brocha, ruta);
                if (_ondaAbajo != null) g.DrawImage(_ondaAbajo, 0, OndaAbajoY * _escalaY, AnchoFigma * _escalaX, OndaAbajoAlto * _escalaY);
                if (_ondaArriba != null) g.DrawImage(_ondaArriba, 0, 0, AnchoFigma * _escalaX, OndaArribaAlto * _escalaY);
            }
            if (_fondoListo != null) _fondoListo.Dispose();
            _fondoListo = nuevo;
        }

        private void BackgroundEmprendedor_Paint(object sender, PaintEventArgs e)
        {
            if (_fondoListo != null) e.Graphics.DrawImageUnscaled(_fondoListo, 0, 0);
        }

        /// <summary>Cuadro blanco redondeado con sombra, campos tipo píldora y la onda que pasa por encima.</summary>
        private void CuadroBlancoTexto_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            float k = _escala;
            RectangleF tarjeta = new RectangleF(4 * k, 0, CuadroBlancoTexto.Width - 8 * k - 1, CuadroBlancoTexto.Height - 10 * k);

            Sombra(g, tarjeta, 30 * k, k);
            using (GraphicsPath ruta = Redondeado(tarjeta, 30 * k))
            {
                g.FillPath(Brushes.White, ruta);

                // Campos píldora alrededor de cada TextBox (alto 40 y 16 de margen)
                foreach (Control control in CuadroBlancoTexto.Controls)
                {
                    TextBoxBase caja = control as TextBoxBase;
                    if (caja == null || !caja.Visible) continue;
                    using (GraphicsPath pildora = Redondeado(Pildora(caja), AltoPildora / 2f * k))
                    using (Pen borde = new Pen(caja.Focused ? AzulPitcher : GrisBorde, Math.Max(1f, k)))
                        g.DrawPath(borde, pildora);
                }

                // La onda de arriba cruza por encima del cuadro blanco (como en el Figma)
                if (_ondaArriba != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    g.DrawImage(_ondaArriba, -CuadroBlancoTexto.Left, -CuadroBlancoTexto.Top, AnchoFigma * _escalaX, OndaArribaAlto * _escalaY);
                    g.Clip = anterior;
                }
            }
        }

        private RectangleF Pildora(Control caja)
        {
            float k = _escala;
            float centro = CentroPildora(caja);
            return new RectangleF(caja.Left - 16 * k, centro - AltoPildora / 2f * k, caja.Width + 32 * k, AltoPildora * k);
        }

        // La píldora se centra donde estaba la caja en el diseñador, no en la caja ya acomodada.
        private float CentroPildora(Control caja)
        {
            Rectangle r = _posicionesOriginales[caja];
            return (r.Y + r.Height / 2f) * _escala;
        }

        /// <summary>
        /// Windows dibuja "Plus Jakarta Sans" mucho más abajo de lo que mide la fuente, así que con el alto
        /// automático se cortaban las letras (la "p" de Apellido). Aquí cada caja toma el alto que de verdad
        /// necesita y se mueve para que las letras queden centradas dentro de su píldora.
        /// </summary>
        private void AcomodarCampos()
        {
            float k = _escala;
            foreach (Control control in CuadroBlancoTexto.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;

                PointF tinta = MedirTinta(caja.Font);
                float centro = CentroPildora(caja);
                float limiteArriba = centro - AltoPildora / 2f * k + 2 * k;   // sin tapar el borde de la píldora
                float limiteAbajo = centro + AltoPildora / 2f * k - 2 * k;

                float arriba = Math.Max(centro - tinta.X, limiteArriba);
                float alto = Math.Min(tinta.Y + 2, limiteAbajo - arriba);
                caja.Bounds = new Rectangle(caja.Left, (int)Math.Round(arriba), caja.Width, (int)Math.Round(alto));
            }
        }

        private static readonly Dictionary<Font, PointF> CacheTinta = new Dictionary<Font, PointF>();

        /// <summary>
        /// Dónde pinta Windows las letras de una fuente, medido desde arriba de la caja:
        /// X = mitad de las mayúsculas, Y = parte más baja de letras como "p" o "g".
        /// </summary>
        private static PointF MedirTinta(Font fuente)
        {
            PointF medida;
            if (CacheTinta.TryGetValue(fuente, out medida)) return medida;

            Size tamano = TextRenderer.MeasureText("Hp", fuente);
            using (Bitmap bmp = new Bitmap(tamano.Width + 4, tamano.Height + 4))
            {
                int capArriba, capAbajo, pArriba, pAbajo;
                FilasConTinta(bmp, "H", fuente, out capArriba, out capAbajo);
                FilasConTinta(bmp, "p", fuente, out pArriba, out pAbajo);
                medida = new PointF((capArriba + capAbajo) / 2f, Math.Max(capAbajo, pAbajo) + 1);
            }
            CacheTinta[fuente] = medida;
            return medida;
        }

        private static void FilasConTinta(Bitmap bmp, string texto, Font fuente, out int primera, out int ultima)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                TextRenderer.DrawText(g, texto, fuente, Point.Empty, Color.Black, Color.White);
            }
            primera = 0;
            ultima = bmp.Height - 1;
            bool encontrada = false;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).R >= 128) continue;
                    if (!encontrada) primera = y;
                    encontrada = true;
                    ultima = y;
                    break;
                }
            }
        }

        // Clic en la píldora (fuera del texto) también pone el cursor en el campo.
        private void CuadroBlancoTexto_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Control control in CuadroBlancoTexto.Controls)
                if (control is TextBoxBase && Pildora(control).Contains(e.Location)) { control.Focus(); return; }
        }

        /// <summary>Botón "Registrarme" como píldora azul con sombra.</summary>
        private void RegistrarseBoton_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);   // el botón está sobre el cuadro blanco
            float k = boton.Height / 56f;
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k);
            Sombra(g, area, area.Height / 2f, k);

            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            Color color = encima ? ControlPaint.Light(AzulPitcher, 0.15f) : AzulPitcher;
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            using (SolidBrush brocha = new SolidBrush(color))
                g.FillPath(brocha, ruta);

            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), boton.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void TerminosCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            TerminosCheckbox.Invalidate();
        }

        /// <summary>Casilla cuadrada con borde verde y el texto a la derecha, como en el Figma.</summary>
        private void TerminosCheckbox_Paint(object sender, PaintEventArgs e)
        {
            CheckBox casilla = (CheckBox)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF cuadro = new RectangleF(1 * k, 5 * k, 28 * k, 28 * k);

            using (GraphicsPath ruta = Redondeado(cuadro, 5 * k))
            {
                using (SolidBrush fondo = new SolidBrush(casilla.Checked ? Color.FromArgb(67, 200, 120) : Color.FromArgb(245, 245, 245)))
                    g.FillPath(fondo, ruta);
                using (Pen borde = new Pen(Color.FromArgb(161, 251, 203), 2 * k))
                    g.DrawPath(borde, ruta);
            }
            if (casilla.Checked)
            {
                using (Pen palomita = new Pen(Color.White, 2.5f * k))
                {
                    palomita.StartCap = LineCap.Round;
                    palomita.EndCap = LineCap.Round;
                    palomita.LineJoin = LineJoin.Round;
                    g.DrawLines(palomita, new[]
                    {
                        new PointF(cuadro.X + cuadro.Width * 0.25f, cuadro.Y + cuadro.Height * 0.52f),
                        new PointF(cuadro.X + cuadro.Width * 0.43f, cuadro.Y + cuadro.Height * 0.70f),
                        new PointF(cuadro.X + cuadro.Width * 0.76f, cuadro.Y + cuadro.Height * 0.32f)
                    });
                }
            }
            TextRenderer.DrawText(g, casilla.Text, casilla.Font, new Point((int)(44 * k), 0), Color.Black);
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

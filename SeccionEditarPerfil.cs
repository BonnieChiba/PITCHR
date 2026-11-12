using MySqlConnector;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Panel "Actualización de Datos" (Editar perfil). Sirve para los DOS roles:
    /// si inició sesión un inversor usa la tabla investor; si es emprendedor, la tabla entrepreneur.
    /// Del emprendedor, el nombre, la descripción y la foto son los que salen como "Founder" en la página de producto.
    /// </summary>
    public partial class SeccionEditarPerfil : SeccionBase
    {
        private DBConn dbconn;                                          //Conectar
        private MySqlDataAdapter investorsDA = new MySqlDataAdapter();  //Extraer
        private DataTable investortable = new DataTable();              //Almacenar

        // Mientras no inicien sesión (para probar el panel), se usa este id. Debe existir en la tabla.
        private const int IdDePrueba = 1;

        private string _fotoGuardada;    // nombre del archivo en la columna imageProfile
        private string _fotoNueva;       // foto recién elegida que todavía no se guarda
        private Image _fotoVista;        // la foto que se dibuja en el recuadro

        /// <summary>Avisa a HomePage que se guardaron los cambios (para actualizar la foto de la barra).</summary>
        public event EventHandler PerfilActualizado;

        public SeccionEditarPerfil()
        {
            InitializeComponent();
            PrepararDiseño();   // estilo del Figma (ver región de abajo)
        }

        // ¿Quién inició sesión? Según su rol se usa una tabla u otra (las dos tienen las mismas columnas).
        private bool EsEmprendedor { get { return Sesion.Activa && Sesion.Rol == 1; } }
        private string Tabla { get { return EsEmprendedor ? "integrative.entrepreneur" : "integrative.investor"; } }
        private string ColumnaId { get { return EsEmprendedor ? "idEntrepreneur" : "idInvestor"; } }

        private int IdUsuario()
        {
            if (Sesion.Activa) return Sesion.IdUsuario;
            return IdDePrueba;
        }

        private void SeccionEditarPerfil_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            TipoCuentaLabel.Text = EsEmprendedor ? "Emprendedor" : "Inversionista";
            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== COMANDOS (SELECT y UPDATE) ====================
        private void LoadDBCommands()
        {
            investorsDA.SelectCommand = new MySqlCommand(
                "SELECT name, lastName, `user`, city, email, rfc, imageProfile, description " +
                "FROM " + Tabla + " WHERE " + ColumnaId + " = @id");

            // La contraseña solo se cambia si escribieron una nueva (si no, se deja la que ya tenía)
            investorsDA.UpdateCommand = new MySqlCommand(
                "UPDATE " + Tabla + " SET name = @name, lastName = @lastName, `user` = @user, city = @city, " +
                "email = @email, rfc = @rfc, imageProfile = @imageProfile, description = @description, " +
                "`password` = IF(@password = '', `password`, @password) " +
                "WHERE " + ColumnaId + " = @id");
        }

        // ==================== LEER (SELECT) ====================
        private void LoadData()
        {
            dbconn = new DBConn();
            investorsDA.SelectCommand.Connection = dbconn.dbConnection;
            investorsDA.SelectCommand.Parameters.Clear();
            investorsDA.SelectCommand.Parameters.AddWithValue("@id", this.IdUsuario());
            try
            {
                investortable.Clear();
                investorsDA.Fill(investortable);
                if (investortable.Rows.Count > 0)
                {
                    this.ShowData(investortable.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowData(DataRow fila)
        {
            NombreTextBox.Text = Convert.ToString(fila["name"]);
            ApellidoTextBox.Text = Convert.ToString(fila["lastName"]);
            UsuarioTextBox.Text = Convert.ToString(fila["user"]);
            CiudadTextBox.Text = Convert.ToString(fila["city"]);
            EmailTextBox.Text = Convert.ToString(fila["email"]);
            RFCTextBox.Text = Convert.ToString(fila["rfc"]);
            DescripcionTextBox.Text = Convert.ToString(fila["description"]);
            ContraseñaTextBox.Text = "";
            ConfirmarTextBox.Text = "";

            _fotoGuardada = Convert.ToString(fila["imageProfile"]);
            _fotoNueva = null;
            _fotoVista = CargarImagen(_fotoGuardada);
            FotoPanel.Invalidate();
        }

        // ==================== GUARDAR (UPDATE) ====================
        private void GuardarBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            // Antes de guardar, revisa que el usuario, el email y el RFC no los tenga OTRO inversor.
            if (this.ExistenDuplicados() == true)
            {
                return;
            }

            try
            {
                // Si eligieron una foto nueva, se copia a la carpeta Imagenes
                if (_fotoNueva != null)
                {
                    _fotoGuardada = CopiarAImagenes(_fotoNueva);
                    _fotoNueva = null;
                }

                using (DBConn dbTemp = new DBConn())
                {
                    investorsDA.UpdateCommand.Connection = dbTemp.dbConnection;
                    investorsDA.UpdateCommand.Connection.Open();
                    investorsDA.UpdateCommand.Parameters.Clear();
                    investorsDA.UpdateCommand.Parameters.AddRange(new[] {
                        new MySqlParameter("@id", this.IdUsuario()),
                        new MySqlParameter("@name", NombreTextBox.Text.Trim()),
                        new MySqlParameter("@lastName", ApellidoTextBox.Text.Trim()),
                        new MySqlParameter("@user", UsuarioTextBox.Text.Trim()),
                        new MySqlParameter("@city", CiudadTextBox.Text.Trim()),
                        new MySqlParameter("@email", EmailTextBox.Text.Trim()),
                        new MySqlParameter("@rfc", RFCTextBox.Text.Trim().ToUpper()),
                        new MySqlParameter("@imageProfile", string.IsNullOrWhiteSpace(_fotoGuardada) ? null : _fotoGuardada),
                        new MySqlParameter("@description", DescripcionTextBox.Text.Trim()),
                        new MySqlParameter("@password", ContraseñaTextBox.Text)
                    });
                    investorsDA.UpdateCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Tus datos se guardaron correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.LoadData();
                if (PerfilActualizado != null) PerfilActualizado(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== VALIDACIÓN ====================
        private bool Validar_campos()
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
            {
                return Aviso("Ingresa un email válido.", EmailTextBox);
            }
            else if (RFCTextBox.Text.Trim().Length != 12 && RFCTextBox.Text.Trim().Length != 13)
            {
                return Aviso("El RFC debe tener 12 o 13 caracteres.", RFCTextBox);
            }
            else if (string.IsNullOrWhiteSpace(CiudadTextBox.Text))
            {
                return Aviso("El campo de ciudad está vacío.", CiudadTextBox);
            }
            else if (string.IsNullOrWhiteSpace(UsuarioTextBox.Text))
            {
                return Aviso("El campo de usuario está vacío.", UsuarioTextBox);
            }
            else if (string.IsNullOrWhiteSpace(NombreTextBox.Text))
            {
                return Aviso("El campo de nombre está vacío.", NombreTextBox);
            }
            else if (string.IsNullOrWhiteSpace(ApellidoTextBox.Text))
            {
                return Aviso("El campo de apellido está vacío.", ApellidoTextBox);
            }
            else if (ContraseñaTextBox.Text != ConfirmarTextBox.Text)
            {
                return Aviso("Las contraseñas no coinciden.", ConfirmarTextBox);
            }
            else
            {
                return true;
            }
        }

        private bool Aviso(string mensaje, Control campo)
        {
            MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            campo.Select();
            return false;
        }

        // ==================== EVITAR DUPLICADOS ====================
        // Revisa inversores y emprendedores, sin contarse a sí mismo.
        private bool ExistenDuplicados()
        {
            string usuario = UsuarioTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string rfc = RFCTextBox.Text.Trim().ToUpper();
            string repetidos = "";

            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    MySqlCommand buscar = new MySqlCommand(
                        "SELECT `user`, email, rfc FROM integrative.investor " +
                        "WHERE (`user` = @user OR email = @email OR rfc = @rfc) AND NOT (@soyEmprendedor = 0 AND idInvestor = @id) " +
                        "UNION ALL " +
                        "SELECT `user`, email, rfc FROM integrative.entrepreneur " +
                        "WHERE (`user` = @user OR email = @email OR rfc = @rfc) AND NOT (@soyEmprendedor = 1 AND idEntrepreneur = @id)", dbTemp.dbConnection);
                    buscar.Parameters.AddWithValue("@user", usuario);
                    buscar.Parameters.AddWithValue("@email", email);
                    buscar.Parameters.AddWithValue("@rfc", rfc);
                    buscar.Parameters.AddWithValue("@id", this.IdUsuario());
                    buscar.Parameters.AddWithValue("@soyEmprendedor", EsEmprendedor ? 1 : 0);
                    dbTemp.dbConnection.Open();
                    using (MySqlDataReader lector = buscar.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            if (string.Equals(lector["user"].ToString(), usuario, StringComparison.OrdinalIgnoreCase)) repetidos += "\n•  El usuario \"" + usuario + "\"";
                            if (string.Equals(lector["email"].ToString(), email, StringComparison.OrdinalIgnoreCase)) repetidos += "\n•  El email \"" + email + "\"";
                            if (string.Equals(lector["rfc"].ToString(), rfc, StringComparison.OrdinalIgnoreCase)) repetidos += "\n•  El RFC \"" + rfc + "\"";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo revisar si los datos ya existen:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            if (repetidos.Length == 0) return false;
            MessageBox.Show("Ya existe otra cuenta con:\n" + repetidos + "\n\nPor favor usa datos diferentes.", "Datos ya registrados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return true;
        }

        // ==================== OTROS BOTONES ====================
        private void FotoPanel_Click(object sender, EventArgs e)
        {
            string ruta = ElegirFoto();
            if (ruta == null) return;
            _fotoNueva = ruta;                 // se copia al guardar
            _fotoVista = LeerArchivo(ruta);
            FotoPanel.Invalidate();
        }

        // Cancelar: descarta lo que no se guardó y vuelve a mostrar lo que hay en la base
        private void CancelarBoton_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        private void PrepararDiseño()
        {
            HacerPildora(CancelarBoton, Color.White);
            HacerPildora(GuardarBoton, AzulBoton);

            foreach (Control control in TarjetaPanel.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;
                PonerTextoGuia(caja);
                caja.GotFocus += (s, e) => TarjetaPanel.Invalidate();
                caja.LostFocus += (s, e) => TarjetaPanel.Invalidate();
            }

            ActivarDobleBuffer(TarjetaPanel);
            ActivarDobleBuffer(CuentaActivaPanel);
            ActivarDobleBuffer(FotoPanel);
            TarjetaPanel.Paint += TarjetaPanel_Paint;
            TarjetaPanel.MouseDown += (s, e) =>
            {
                foreach (Control c in TarjetaPanel.Controls)
                    if (c is TextBoxBase && Caja(c).Contains(e.Location)) { c.Focus(); return; }
            };
            CuentaActivaPanel.Paint += CuentaActivaPanel_Paint;
            FotoPanel.Paint += FotoPanel_Paint;
            FotoPanel.MouseEnter += (s, e) => FotoPanel.Invalidate();
            FotoPanel.MouseLeave += (s, e) => FotoPanel.Invalidate();

            PrepararEscalado(FondoPanel);
        }

        // Recuadro redondeado alrededor de cada TextBox (el TextBox no tiene borde)
        private RectangleF Caja(Control campo)
        {
            float k = Escala;
            return new RectangleF(campo.Left - 14 * k, campo.Top - 10 * k, campo.Width + 28 * k, campo.Height + 20 * k);
        }

        /// <summary>Tarjeta blanca con encabezado gris y los recuadros de los campos.</summary>
        private void TarjetaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            RectangleF tarjeta = new RectangleF(4 * k, 0, TarjetaPanel.Width - 8 * k - 1, TarjetaPanel.Height - 10 * k);
            Sombra(g, tarjeta, 20 * k, k);
            using (GraphicsPath ruta = Redondeado(tarjeta, 20 * k))
            {
                g.FillPath(Brushes.White, ruta);
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                using (SolidBrush gris = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    g.FillRectangle(gris, tarjeta.X, tarjeta.Y, tarjeta.Width, 100 * k);
                g.Clip = anterior;
            }

            foreach (Control control in TarjetaPanel.Controls)
            {
                TextBoxBase campo = control as TextBoxBase;
                if (campo == null) continue;
                using (GraphicsPath p = Redondeado(Caja(campo), 10 * k))
                using (Pen borde = new Pen(campo.Focused ? Color.FromArgb(30, 153, 247) : Color.FromArgb(222, 221, 231), Math.Max(1f, campo.Focused ? 1.5f * k : 1f * k)))
                {
                    g.FillPath(Brushes.White, p);
                    g.DrawPath(borde, p);
                }
            }
        }

        /// <summary>Recuadro de la foto de perfil: foto redonda y "Elegir foto".</summary>
        private void FotoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            bool encima = FotoPanel.ClientRectangle.Contains(FotoPanel.PointToClient(Cursor.Position));
            RectangleF r = new RectangleF(1, 1, FotoPanel.Width - 3, FotoPanel.Height - 3);
            using (GraphicsPath p = Redondeado(r, 12 * k))
            {
                using (SolidBrush b = new SolidBrush(encima ? Color.FromArgb(232, 247, 254) : Color.FromArgb(250, 249, 253))) g.FillPath(b, p);
                using (Pen borde = new Pen(Color.FromArgb(56, 189, 248), Math.Max(1f, k)) { DashStyle = DashStyle.Dash }) g.DrawPath(borde, p);
            }
            RectangleF circulo = new RectangleF(14 * k, (FotoPanel.Height - 64 * k) / 2f, 64 * k, 64 * k);
            DibujarFotoRedonda(g, _fotoVista, circulo, NombreTextBox.Text, k);
            Font titulo = Fuente(15, FontStyle.Bold);      // las fuentes de Fuente() se reutilizan: no se les hace Dispose
            Font detalle = Fuente(12, FontStyle.Regular);
            {
                float x = circulo.Right + 16 * k, cy = FotoPanel.Height / 2f;
                TextRenderer.DrawText(g, _fotoVista == null ? "Elegir foto" : "Cambiar foto", titulo, new Point((int)x, (int)(cy - 22 * k)), Color.FromArgb(77, 79, 98));
                TextRenderer.DrawText(g, "PNG o JPG · Máx. 5 MB", detalle, new Point((int)x, (int)(cy + 2 * k)), Color.FromArgb(170, 168, 180));
            }
        }

        /// <summary>Tarjeta "Cuenta activa": azul para inversionista, naranja-rosa para emprendedor.</summary>
        private void CuentaActivaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            RectangleF r = new RectangleF(0, 0, CuentaActivaPanel.Width - 1, CuentaActivaPanel.Height - 1);
            using (GraphicsPath ruta = Redondeado(r, 20 * k))
            {
                Color inicio = EsEmprendedor ? Color.FromArgb(248, 110, 56) : Color.FromArgb(8, 120, 232);
                Color fin = EsEmprendedor ? Color.FromArgb(229, 11, 182) : Color.FromArgb(14, 196, 233);
                using (LinearGradientBrush b = new LinearGradientBrush(r, inicio, fin, 20f))
                    g.FillPath(b, ruta);
                Region anterior = g.Clip;
                g.SetClip(ruta, CombineMode.Intersect);
                using (Pen anillo = new Pen(Color.FromArgb(70, 255, 255, 255), 9 * k))
                {
                    float d = 62 * k, m = 4.5f * k;
                    g.DrawEllipse(anillo, 318 * k + m, 8 * k + m, d - 2 * m, d - 2 * m);
                    g.DrawEllipse(anillo, 258 * k + m, 40 * k + m, d - 2 * m, d - 2 * m);
                    g.DrawEllipse(anillo, 318 * k + m, 76 * k + m, d - 2 * m, d - 2 * m);
                }
                g.Clip = anterior;
            }
        }

        #endregion
    }
}

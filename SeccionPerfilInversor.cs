using MySqlConnector;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Panel "Ver perfil" (frames "myProfile" y "porfileFounder"): foto, nombre, @usuario y descripción.
    ///  • Mi perfil (sin id): lee la fila de quien inició sesión (investor o entrepreneur) y muestra "Editar Perfil".
    ///  • Perfil del fundador (con idEntrepreneur, desde la página de producto): lee ese emprendedor y muestra "Enviar Mensaje".
    /// </summary>
    public partial class SeccionPerfilInversor : SeccionBase
    {
        private DBConn dbconn;                                          //Conectar
        private MySqlDataAdapter investorsDA = new MySqlDataAdapter();  //Extraer
        private DataTable investortable = new DataTable();              //Almacenar

        // Mientras no inicien sesión (para probar el panel), se usa este id. Debe existir en la tabla.
        private const int IdDePrueba = 1;

        private Image _foto;

        // idEntrepreneur del fundador que se está viendo. 0 = es mi propio perfil.
        private readonly int _idFundador;

        /// <summary>Avisos para HomePage: la flecha de regresar, "Editar Perfil" y "Enviar Mensaje".</summary>
        public event EventHandler Regresar;
        public event EventHandler EditarPerfil;
        public event EventHandler<int> EnviarMensaje;   // manda el idEntrepreneur del fundador

        public SeccionPerfilInversor() : this(0)
        {
        }

        public SeccionPerfilInversor(int idFundador)
        {
            InitializeComponent();
            _idFundador = idFundador;
            PrepararDiseno();   // estilo del Figma (ver región de abajo)
        }

        // ¿Estoy viendo el perfil de un fundador? (si el fundador soy yo, es mi propio perfil)
        private bool VerFundador
        {
            get
            {
                if (_idFundador == 0) return false;
                bool soyYo = Sesion.Activa && Sesion.Rol == 1 && Sesion.IdUsuario == _idFundador;
                return !soyYo;
            }
        }

        // Según el rol se usa una tabla u otra (las dos tienen las mismas columnas)
        private bool EsEmprendedor { get { return Sesion.Activa && Sesion.Rol == 1; } }

        private int IdUsuario()
        {
            if (Sesion.Activa) return Sesion.IdUsuario;
            return IdDePrueba;
        }

        private void SeccionPerfilInversor_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== LEER (SELECT) ====================
        private void LoadDBCommands()
        {
            investorsDA.SelectCommand = new MySqlCommand(
                (EsEmprendedor || VerFundador)
                    ? "SELECT name, lastName, `user`, description, imageProfile FROM integrative.entrepreneur WHERE idEntrepreneur = @id"
                    : "SELECT name, lastName, `user`, description, imageProfile FROM integrative.investor WHERE idInvestor = @id");
        }

        private void LoadData()
        {
            dbconn = new DBConn();
            investorsDA.SelectCommand.Connection = dbconn.dbConnection;
            investorsDA.SelectCommand.Parameters.Clear();
            investorsDA.SelectCommand.Parameters.AddWithValue("@id", VerFundador ? _idFundador : this.IdUsuario());
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
            NombreLabel.Text = (Convert.ToString(fila["name"]) + " " + Convert.ToString(fila["lastName"])).Trim().ToUpper();
            UsuarioLabel.Text = "@" + Convert.ToString(fila["user"]);

            string descripcion = Convert.ToString(fila["description"]);
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                descripcion = VerFundador
                    ? "Este emprendedor aún no tiene descripción."
                    : "Aún no tienes descripción. Agrégala en \"Editar Perfil\".";
            }
            DescripcionLabel.Text = descripcion;

            _foto = CargarImagen(Convert.ToString(fila["imageProfile"]));
            FotoPanel.Invalidate();
        }

        // ==================== BOTONES ====================
        // En mi perfil es "Editar Perfil"; en el de un fundador es "Enviar Mensaje"
        private void EditarBoton_Click(object sender, EventArgs e)
        {
            if (!VerFundador)
            {
                if (EditarPerfil != null) EditarPerfil(this, EventArgs.Empty);
                return;
            }

            // Igual que CONTACTAR en la página de producto: solo un inversionista puede escribirle al fundador
            if (!Sesion.Activa || Sesion.Rol != 0)
            {
                MessageBox.Show("Inicia sesión como inversionista para enviarle un mensaje al fundador.", "PitchR");
                return;
            }
            if (EnviarMensaje != null) EnviarMensaje(this, _idFundador);
        }

        private void RegresarBoton_Click(object sender, EventArgs e)
        {
            if (Regresar != null) Regresar(this, EventArgs.Empty);
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        private void PrepararDiseno()
        {
            if (VerFundador) EditarBoton.Text = "Enviar Mensaje";
            HacerPildora(EditarBoton, VerFundador ? Naranja : AzulBoton);   // naranja para "Enviar Mensaje", como en el Figma
            ActivarDobleBuffer(TarjetaPanel);
            ActivarDobleBuffer(FotoPanel);
            TarjetaPanel.Paint += TarjetaPanel_Paint;
            FotoPanel.Paint += FotoPanel_Paint;
            BotonCircular.HacerBotonRegresar(RegresarBoton, Color.White);
            PrepararEscalado(FondoPanel);
        }

        /// <summary>Tarjeta blanca redondeada con sombra, sobre el fondo lila claro.</summary>
        private void TarjetaPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(FondoPanel.BackColor);
            float k = Escala;
            RectangleF tarjeta = new RectangleF(4 * k, 0, TarjetaPanel.Width - 8 * k - 1, TarjetaPanel.Height - 10 * k);
            Sombra(g, tarjeta, 30 * k, k);
            using (GraphicsPath ruta = Redondeado(tarjeta, 30 * k))
                g.FillPath(Brushes.White, ruta);
        }

        /// <summary>Foto redonda grande con sombra.</summary>
        private void FotoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            RectangleF circulo = new RectangleF(3 * k, 0, FotoPanel.Width - 6 * k - 1, FotoPanel.Width - 6 * k - 1);
            for (int i = 6; i >= 1; i--)
                using (SolidBrush b = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
                    g.FillEllipse(b, circulo.X - i * 0.3f * k, circulo.Y + i * k, circulo.Width + i * 0.6f * k, circulo.Height);
            DibujarFotoRedonda(g, _foto, circulo, NombreLabel.Text, k);
        }

        // El botón de regresar se dibuja con BotonCircular.DibujarRegresar (el mismo en toda la app).

        #endregion
    }
}

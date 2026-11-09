using MySqlConnector;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Panel "Ver perfil" del INVERSOR (frame "myProfile"): foto, nombre, @usuario y descripción.
    /// Lee su fila en integrative.investor.
    /// </summary>
    public partial class SeccionPerfilInversor : SeccionBase
    {
        private DBConn dbconn;                                          //Conectar
        private MySqlDataAdapter investorsDA = new MySqlDataAdapter();  //Extraer
        private DataTable investortable = new DataTable();              //Almacenar

        // Mientras no inicien sesión (para probar el panel), se usa este idInvestor. Debe existir en la tabla investor.
        private const int IdInversorDePrueba = 1;

        private Image _foto;

        /// <summary>Avisos para HomePage: la flecha de regresar y el botón "Editar Perfil".</summary>
        public event EventHandler Regresar;
        public event EventHandler EditarPerfil;

        public SeccionPerfilInversor()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma (ver región de abajo)
        }

        private int IdInversor()
        {
            if (Sesion.Activa && Sesion.Rol == 0) return Sesion.IdUsuario;
            return IdInversorDePrueba;
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
                "SELECT name, lastName, `user`, description, imageProfile FROM integrative.investor WHERE idInvestor = @idInvestor");
        }

        private void LoadData()
        {
            dbconn = new DBConn();
            investorsDA.SelectCommand.Connection = dbconn.dbConnection;
            investorsDA.SelectCommand.Parameters.Clear();
            investorsDA.SelectCommand.Parameters.AddWithValue("@idInvestor", this.IdInversor());
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
            DescripcionLabel.Text = string.IsNullOrWhiteSpace(descripcion)
                ? "Aún no tienes descripción. Agrégala en \"Editar Perfil\"."
                : descripcion;

            _foto = CargarImagen(Convert.ToString(fila["imageProfile"]));
            FotoPanel.Invalidate();
        }

        // ==================== BOTONES ====================
        private void EditarBoton_Click(object sender, EventArgs e)
        {
            if (EditarPerfil != null) EditarPerfil(this, EventArgs.Empty);
        }

        private void RegresarBoton_Click(object sender, EventArgs e)
        {
            if (Regresar != null) Regresar(this, EventArgs.Empty);
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        private void PrepararDiseno()
        {
            HacerPildora(EditarBoton, AzulBoton);
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

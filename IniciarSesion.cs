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
    public partial class IniciarSesion : Form
    {
        // Rol de la cuenta (columna "role"): 1 = emprendedor, 0 = inversor. Igual que en CrearCuenta.
        private const int RolInversor = 0;
        private const int RolEmprendedor = 1;

        public IniciarSesion()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)

            // Las pantallas se ocultan con Hide(); si cierran esta ventana con la X, se cierra toda la aplicación.
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.clearTextBoxes();
        }

        // ==================== BOTÓN INICIAR SESIÓN ====================
        private void IniciarSesionBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            string usuario = UsuarioTextBox.Text.Trim();
            string contraseña = ContraseñaTextBox.Text;

            try
            {
                // Primero se busca entre los inversores y, si no está, entre los emprendedores.
                // Se puede entrar con el usuario o con el email.
                if (this.BuscarCuenta("integrative.investor", "idInvestor", usuario, contraseña, RolInversor) == false &&
                    this.BuscarCuenta("integrative.entrepreneur", "idEntrepreneur", usuario, contraseña, RolEmprendedor) == false)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.ContraseñaTextBox.Clear();
                    this.ContraseñaTextBox.Select();
                    return;
                }

                string tipo = Sesion.Rol == RolEmprendedor ? "emprendedor" : "inversor";
                MessageBox.Show("¡Bienvenido, " + Sesion.Usuario + "!\nIniciaste sesión como " + tipo + ".", "Iniciar sesión",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.clearTextBoxes();

                EmprendimientoGuest pantalla = new EmprendimientoGuest();
                pantalla.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Busca la cuenta en una tabla (investor o entrepreneur) por usuario o email.
        /// Si la encuentra y la contraseña coincide, guarda los datos en Sesion y regresa true.
        /// </summary>
        private bool BuscarCuenta(string tabla, string columnaId, string usuario, string contraseña, int rol)
        {
            using (DBConn dbTemp = new DBConn())
            {
                MySqlCommand buscar = new MySqlCommand(
                    "SELECT " + columnaId + ", `user`, `password` FROM " + tabla +
                    " WHERE `user` = @usuario OR email = @usuario LIMIT 1", dbTemp.dbConnection);
                buscar.Parameters.AddWithValue("@usuario", usuario);

                dbTemp.dbConnection.Open();
                using (MySqlDataReader lector = buscar.ExecuteReader())
                {
                    if (lector.Read() == false)
                    {
                        return false;   // no existe en esta tabla
                    }

                    // La contraseña se compara aquí (en C#) para que distinga mayúsculas y minúsculas.
                    string guardada = lector["password"].ToString();
                    if (guardada != contraseña)
                    {
                        return false;
                    }

                    Sesion.Iniciar(Convert.ToInt32(lector[columnaId]), lector["user"].ToString(), rol);
                    return true;
                }
            }
        }

        // ==================== VALIDACIÓN ====================
        private bool Validar_campos()
        {
            if (string.IsNullOrWhiteSpace(UsuarioTextBox.Text))
            {
                MessageBox.Show("Escribe tu usuario o email.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.UsuarioTextBox.Select();
                return false;
            }
            else if (string.IsNullOrWhiteSpace(ContraseñaTextBox.Text))
            {
                MessageBox.Show("El campo de contraseña está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.ContraseñaTextBox.Select();
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
            this.UsuarioTextBox.Text = "";
            this.ContraseñaTextBox.Text = "";
        }

        // ==================== OTROS BOTONES ====================
        private void RegresarPictureBox_Click(object sender, EventArgs e)
          {
            IniciaAqui pantalla = new IniciaAqui();
            pantalla.Show();
            this.Hide();
        }
        // "¿Aún no tienes cuenta? Registrarme" → elegir si es inversor o emprendedor
        private void RegistrarmeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IniciaAqui pantalla = new IniciaAqui();
            pantalla.Show();
            this.Hide();
        }

        private void OlvidasteLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Cuando tengan la pantalla para recuperar la contraseña, ábranla aquí.
            MessageBox.Show("La recuperación de contraseña estará disponible próximamente.", "Pitcher");
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame "LogIn" en Figma: todo el diseño está hecho en 1280 × 832.
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

        // Posición de las ondas grises en el Figma (y, alto)
        private const float OndaArribaAlto = 250f;
        private const float OndaAbajoY = 600f;
        private const float OndaAbajoAlto = 232f;
        private const float AltoPildora = 43f;   // los campos de esta pantalla miden 43 de alto en el Figma

        // Mensaje de Windows para poner texto guía gris dentro de un TextBox ("Usuario", "Contraseña"...).
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // Las ondas se dibujan a mano: la de arriba pasa por debajo del cuadro blanco y la de abajo, por encima.
            _ondaArriba = LineaSuperiorPicturebox.Image;
            _ondaAbajo = pictureBox1.Image;
            LineaSuperiorPicturebox.Visible = false;
            pictureBox1.Visible = false;

            // Las imágenes de fondo son solo la vista previa del diseñador; al ejecutar todo se dibuja con código.
            BackgroundInicio.BackgroundImage = null;
            CuadroBlancoTexto.BackgroundImage = null;
            IniciarSesionBoton.BackgroundImage = null;

            // El cuadro blanco se dibuja redondeado y con sombra en CuadroBlancoTexto_Paint.
            CuadroBlancoTexto.BackColor = Color.Transparent;
            ActivarDobleBuffer(BackgroundInicio);
            ActivarDobleBuffer(CuadroBlancoTexto);

            // Botón píldora: el fondo lo pinta IniciarSesionBoton_Paint.
            IniciarSesionBoton.BackColor = Color.Transparent;
            IniciarSesionBoton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            IniciarSesionBoton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            IniciarSesionBoton.MouseEnter += (s, e) => IniciarSesionBoton.Invalidate();
            IniciarSesionBoton.MouseLeave += (s, e) => IniciarSesionBoton.Invalidate();

            // Campos: texto guía (viene de la propiedad Tag) y borde azul cuando están activos.
            foreach (Control control in CuadroBlancoTexto.Controls)
            {
                TextBoxBase caja = control as TextBoxBase;
                if (caja == null) continue;
                caja.HandleCreated += (s, e) => PonerTextoGuia(caja);
                caja.GotFocus += (s, e) => CuadroBlancoTexto.Invalidate();
                caja.LostFocus += (s, e) => CuadroBlancoTexto.Invalidate();
            }

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            BackgroundInicio.Paint += BackgroundInicio_Paint;
            CuadroBlancoTexto.Paint += CuadroBlancoTexto_Paint;
            CuadroBlancoTexto.MouseDown += CuadroBlancoTexto_MouseDown;
            IniciarSesionBoton.Paint += IniciarSesionBoton_Paint;
            Resize += IniciarSesion_Resize;

            // Se guardan las posiciones y fuentes del diseñador (1280 × 832) para escalarlas después.
            GuardarOriginales(BackgroundInicio);
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

        private void IniciarSesion_Resize(object sender, EventArgs e)
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
            BackgroundInicio.Bounds = new Rectangle(0, 0, cliente.Width, cliente.Height);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BackgroundInicio;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala + (directo ? margenY : 0)),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            ResumeLayout(true);

            PrepararFondo(cliente);
            BackgroundInicio.Invalidate(true);
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

        /// <summary>Dibuja una sola vez (por tamaño) el fondo: margen blanco, rectángulo azul y ondas grises.</summary>
        private void PrepararFondo(Size tamano)
        {
            Bitmap nuevo = new Bitmap(tamano.Width, tamano.Height);
            using (Graphics g = Graphics.FromImage(nuevo))
            {
                Calidad(g);
                g.Clear(Color.White);
                RectangleF azul = new RectangleF(16 * _escalaX, 21 * _escalaY, 1241 * _escalaX, 794 * _escalaY);
                using (GraphicsPath ruta = Redondeado(azul, 20 * _escala))
                using (SolidBrush brocha = new SolidBrush(AzulPitcher))
                    g.FillPath(brocha, ruta);
                if (_ondaAbajo != null) g.DrawImage(_ondaAbajo, 0, OndaAbajoY * _escalaY, AnchoFigma * _escalaX, OndaAbajoAlto * _escalaY);
                if (_ondaArriba != null) g.DrawImage(_ondaArriba, 0, 0, AnchoFigma * _escalaX, OndaArribaAlto * _escalaY);
            }
            if (_fondoListo != null) _fondoListo.Dispose();
            _fondoListo = nuevo;
        }

        private void BackgroundInicio_Paint(object sender, PaintEventArgs e)
        {
            if (_fondoListo != null) e.Graphics.DrawImageUnscaled(_fondoListo, 0, 0);
        }

        /// <summary>Cuadro blanco redondeado con sombra, campos tipo píldora y la onda de abajo que pasa por encima.</summary>
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

                // La onda de abajo cruza por encima del cuadro blanco (como en el Figma)
                if (_ondaAbajo != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    g.DrawImage(_ondaAbajo, -CuadroBlancoTexto.Left, OndaAbajoY * _escalaY - CuadroBlancoTexto.Top, AnchoFigma * _escalaX, OndaAbajoAlto * _escalaY);
                    g.Clip = anterior;
                }
            }
        }

        private RectangleF Pildora(Control caja)
        {
            float k = _escala;
            float centro = caja.Top + caja.Height / 2f;
            return new RectangleF(caja.Left - 16 * k, centro - AltoPildora / 2f * k, caja.Width + 32 * k, AltoPildora * k);
        }

        // Clic en la píldora (fuera del texto) también pone el cursor en el campo.
        private void CuadroBlancoTexto_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Control control in CuadroBlancoTexto.Controls)
                if (control is TextBoxBase && Pildora(control).Contains(e.Location)) { control.Focus(); return; }
        }

        /// <summary>Botón "Iniciar Sesión" como píldora azul con sombra.</summary>
        private void IniciarSesionBoton_Paint(object sender, PaintEventArgs e)
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

    /// <summary>
    /// Datos de la persona que inició sesión. Se llenan en IniciarSesion y cualquier pantalla puede leerlos
    /// (por ejemplo, para mostrar la foto de perfil en lugar del botón "Iniciar Sesión").
    /// </summary>
    public static class Sesion
    {
        public static int IdUsuario { get; private set; }
        public static string Usuario { get; private set; }
        public static int Rol { get; private set; }          // 1 = emprendedor, 0 = inversor
        public static bool Activa { get; private set; }

        public static void Iniciar(int idUsuario, string usuario, int rol)
        {
            IdUsuario = idUsuario;
            Usuario = usuario;
            Rol = rol;
            Activa = true;
        }

        public static void Cerrar()
        {
            IdUsuario = 0;
            Usuario = null;
            Rol = 0;
            Activa = false;
        }
    }
}

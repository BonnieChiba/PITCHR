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
    /// <summary>
    /// Recuperar contraseña (frames "EnterEmail" y "Change password" del Figma), en una sola ventana con 2 pasos:
    ///   1. Correo   → busca la cuenta con ese email (en investor o en entrepreneur).
    ///   2. Cambiar  → actualiza la contraseña de esa cuenta.
    /// Sin código de verificación.
    /// </summary>
    public partial class RecuperarContrasena : Form
    {
        // Rol de la cuenta (columna "role"): 1 = emprendedor, 0 = inversor. Igual que en CrearCuenta.
        private const int RolInversor = 0;
        private const int RolEmprendedor = 1;

        private enum Paso { Correo, Cambiar }
        private Paso _paso = Paso.Correo;

        // Datos de la cuenta encontrada en el paso 1
        private string _email = "";
        private int _idUsuario = 0;
        private int _rol = 0;

        public RecuperarContrasena()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)

            // Las pantallas se ocultan con Hide(); si cierran esta ventana con la X, se cierra toda la aplicación.
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void RecuperarContrasena_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.MostrarPaso(Paso.Correo);
        }

        // ==================== BOTÓN (cambia según el paso) ====================
        private void AccionBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            if (_paso == Paso.Correo) this.BuscarCorreo();
            else this.CambiarContrasena();
        }

        // Enter = presionar el botón
        private void CajaDeTexto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.AccionBoton_Click(sender, e);
            }
        }

        // ==================== PASO 1: BUSCAR LA CUENTA (SELECT) ====================
        private void BuscarCorreo()
        {
            _email = DatoTextBox.Text.Trim();

            try
            {
                // Primero se busca entre los inversores y, si no está, entre los emprendedores.
                if (this.BuscarCuenta("integrative.investor", "idInvestor", RolInversor) == false &&
                    this.BuscarCuenta("integrative.entrepreneur", "idEntrepreneur", RolEmprendedor) == false)
                {
                    MessageBox.Show("No existe ninguna cuenta con ese correo.", "Recuperar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DatoTextBox.Select();
                    return;
                }

                this.MostrarPaso(Paso.Cambiar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al buscar la cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Busca la cuenta por email en una tabla. Si la encuentra, guarda su id y su rol.</summary>
        private bool BuscarCuenta(string tabla, string columnaId, int rol)
        {
            using (DBConn dbTemp = new DBConn())
            {
                MySqlCommand buscar = new MySqlCommand(
                    "SELECT " + columnaId + " FROM " + tabla + " WHERE email = @email LIMIT 1", dbTemp.dbConnection);
                buscar.Parameters.AddWithValue("@email", _email);
                dbTemp.dbConnection.Open();
                object id = buscar.ExecuteScalar();
                if (id == null || id == DBNull.Value) return false;

                _idUsuario = Convert.ToInt32(id);
                _rol = rol;
                return true;
            }
        }

        // ==================== PASO 2: NUEVA CONTRASEÑA (UPDATE) ====================
        private void CambiarContrasena()
        {
            string tabla = _rol == RolEmprendedor ? "integrative.entrepreneur" : "integrative.investor";
            string columnaId = _rol == RolEmprendedor ? "idEntrepreneur" : "idInvestor";

            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    MySqlCommand cambiar = new MySqlCommand(
                        "UPDATE " + tabla + " SET `password` = @password WHERE " + columnaId + " = @id", dbTemp.dbConnection);
                    cambiar.Parameters.AddWithValue("@password", NuevaTextBox.Text);
                    // Recomendado: BCrypt.Net.BCrypt.HashPassword(NuevaTextBox.Text)
                    cambiar.Parameters.AddWithValue("@id", _idUsuario);
                    dbTemp.dbConnection.Open();
                    cambiar.ExecuteNonQuery();
                }

                this.clearTextBoxes();
                MessageBox.Show("Tu contraseña se actualizó. Ya puedes iniciar sesión con la nueva.", "Recuperar contraseña",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                IniciarSesion pantalla = new IniciarSesion();
                pantalla.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al cambiar la contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== VALIDACIÓN ====================
        private bool Validar_campos()
        {
            if (_paso == Paso.Correo)
            {
                if (string.IsNullOrWhiteSpace(DatoTextBox.Text) || !DatoTextBox.Text.Contains("@"))
                {
                    MessageBox.Show("Ingresa un email válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DatoTextBox.Select();
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(NuevaTextBox.Text))
                {
                    MessageBox.Show("El campo de contraseña está vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    NuevaTextBox.Select();
                    return false;
                }
                else if (NuevaTextBox.Text != RepetirTextBox.Text)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RepetirTextBox.Select();
                    return false;
                }
            }
            return true;
        }

        // ==================== LIMPIAR CAMPOS ====================
        private void clearTextBoxes()
        {
            this.DatoTextBox.Text = "";
            this.NuevaTextBox.Text = "";
            this.RepetirTextBox.Text = "";
        }

        // ==================== CAMBIAR DE PASO ====================
        private void MostrarPaso(Paso paso)
        {
            _paso = paso;
            this.clearTextBoxes();

            bool cambiar = paso == Paso.Cambiar;
            if (cambiar)
            {
                TituloLabel.Text = "Cambiar contraseña";
                AccionBoton.Text = "Actualizar contraseña";
            }
            else
            {
                TituloLabel.Text = "Olvidaste tu contraseña";
                DescripcionLabel.Text = "Ingresa el correo electrónico asociado a tu cuenta para cambiar tu contraseña.";
                DatoTextBox.Text = _email;
                AccionBoton.Text = "Continuar";
            }

            DescripcionLabel.Visible = !cambiar;
            DatoTextBox.Visible = !cambiar;
            NuevaTextBox.Visible = cambiar;
            RepetirTextBox.Visible = cambiar;

            this.AjustarATamano();
            (cambiar ? NuevaTextBox : DatoTextBox).Select();
        }

        // ==================== REGRESAR Y MENÚ ====================
        private void RegresarPictureBox_Click(object sender, EventArgs e)
        {
            // Desde "Cambiar contraseña" regresa al correo; desde el correo, a Iniciar sesión
            if (_paso == Paso.Cambiar) { this.MostrarPaso(Paso.Correo); return; }

            IniciarSesion pantalla = new IniciarSesion();
            pantalla.Show();
            this.Hide();
        }

        private void AbrirHome(HomePage.Seccion seccion)
        {
            HomePage pantalla = new HomePage(seccion);
            pantalla.Show();
            this.Hide();
        }

        private void EmprendimientosLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { this.AbrirHome(HomePage.Seccion.Emprendimientos); }
        private void EventosLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { this.AbrirHome(HomePage.Seccion.Eventos); }
        private void NoticiasLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { this.AbrirHome(HomePage.Seccion.Noticias); }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // Todo el diseño está hecho en 1280 × 832. La tarjeta gris mide 562 × 397 y está en (335, 262).
        private const float AnchoFigma = 1280f, AltoFigma = 832f;
        private static readonly RectangleF TarjetaFigma = new RectangleF(335, 262, 562, 397);

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);
        private static readonly Color GrisTarjeta = Color.FromArgb(246, 246, 246);   // #F6F6F6
        private static readonly Color GrisMenu = Color.FromArgb(240, 243, 244);      // #F0F3F4
        private static readonly Color GrisBorde = Color.FromArgb(217, 217, 217);     // #D9D9D9

        private Image _fondoEmail, _fondoCambio;
        private Bitmap _fondoListo;
        private Paso _pasoDelFondo;
        private float _k = 1f, _sx = 1f, _sy = 1f;   // escala pareja, a lo ancho y a lo alto
        private PointF _origen;                       // esquina del contenido de la tarjeta

        // Medidas del Figma dentro de la tarjeta (x, y, ancho, alto) para cada paso
        private RectangleF _titulo, _descripcion, _caja1, _caja2, _boton;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // Los 2 fondos del Figma (azul con las ondas grises y la tarjeta). Las PictureBox solo los guardan.
            _fondoEmail = FondoEmailPictureBox.Image;
            _fondoCambio = FondoCambioPictureBox.Image;
            FondoPanel.BackgroundImage = null;   // la vista previa del diseñador; al ejecutar se dibuja con código

            ActivarDobleBuffer(FondoPanel);
            FondoPanel.Paint += FondoPanel_Paint;
            FondoPanel.MouseDown += FondoPanel_MouseDown;
            AccionBoton.Paint += AccionBoton_Paint;
            AccionBoton.MouseEnter += (s, e) => AccionBoton.Invalidate();
            AccionBoton.MouseLeave += (s, e) => AccionBoton.Invalidate();

            foreach (TextBox caja in new[] { DatoTextBox, NuevaTextBox, RepetirTextBox })
            {
                TextBox c = caja;
                c.AutoSize = false;
                c.HandleCreated += (s, e) => PonerTextoGuia(c);
                c.GotFocus += (s, e) => FondoPanel.Invalidate();
                c.LostFocus += (s, e) => FondoPanel.Invalidate();
            }

            Resize += (s, e) => AjustarATamano();
            Load += (s, e) => AjustarATamano();
        }

        private static void ActivarDobleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           .SetValue(control, true, null);
        }

        private static void PonerTextoGuia(TextBox caja)
        {
            string guia = caja.Tag as string;
            if (string.IsNullOrEmpty(guia) || !caja.IsHandleCreated) return;
            try
            {
                SendMessage(caja.Handle, EM_SETCUEBANNER, (IntPtr)1, guia);
            }
            catch (Exception)
            {
                // Solo falla fuera de Windows.
            }
        }

        /// <summary>
        /// El fondo se estira a toda la ventana (como en IniciarSesion). Lo de adentro de la tarjeta
        /// crece parejo (sin deformarse) y se centra dentro de la tarjeta.
        /// </summary>
        private void AjustarATamano()
        {
            Size cliente = FondoPanel.ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _sx = cliente.Width / AnchoFigma;
            _sy = cliente.Height / AltoFigma;
            _k = Math.Min(_sx, _sy);
            RectangleF tarjeta = new RectangleF(TarjetaFigma.X * _sx, TarjetaFigma.Y * _sy, TarjetaFigma.Width * _sx, TarjetaFigma.Height * _sy);
            _origen = new PointF(tarjeta.X + (tarjeta.Width - TarjetaFigma.Width * _k) / 2f, tarjeta.Y + (tarjeta.Height - TarjetaFigma.Height * _k) / 2f);

            // Posiciones del Figma dentro de la tarjeta
            if (_paso == Paso.Correo)
            {
                _titulo = new RectangleF(0, 46, 562, 70);
                _descripcion = new RectangleF(40, 130, 482, 84);
                _caja1 = new RectangleF(30, 219, 498, 48);
                _boton = new RectangleF(176, 294, 210, 61);
            }
            else
            {
                _titulo = new RectangleF(0, 40, 562, 80);
                _caja1 = new RectangleF(32, 144, 498, 48);
                _caja2 = new RectangleF(32, 213, 498, 48);
                _boton = new RectangleF(131, 284, 299, 55);
            }

            SuspendLayout();
            TituloLabel.Bounds = Escalar(_titulo);
            TituloLabel.Font = Fuente(_paso == Paso.Cambiar ? 48 : 43, FontStyle.Regular);
            DescripcionLabel.Bounds = Escalar(_descripcion);
            DescripcionLabel.Font = Fuente(19, FontStyle.Regular);
            AccionBoton.Bounds = Escalar(_boton);
            AccionBoton.Font = Fuente(20, FontStyle.Bold);
            AcomodarCaja(DatoTextBox, _caja1);
            AcomodarCaja(NuevaTextBox, _caja1);
            AcomodarCaja(RepetirTextBox, _caja2);

            // Botón de regresar y menú de arriba (en sus lugares del Figma)
            RegresarPictureBox.Bounds = new Rectangle((int)(52 * _sx), (int)(59 * _sy), (int)(60 * _k), (int)(60 * _k));
            float derechaMenu = 1214 * _sx;
            float xMenu = derechaMenu - 343 * _k, yMenu = 59 * _sy;
            UbicarEnlace(NoticiasLink, xMenu, yMenu, 257, 76);
            UbicarEnlace(EventosLink, xMenu, yMenu, 178, 70);
            UbicarEnlace(EmprendimientosLink, xMenu, yMenu, 23, 140);
            ResumeLayout(true);

            PrepararFondo(cliente);
            FondoPanel.Invalidate(true);
        }

        private Rectangle Escalar(RectangleF r)
        {
            return Rectangle.Round(new RectangleF(_origen.X + r.X * _k, _origen.Y + r.Y * _k, r.Width * _k, r.Height * _k));
        }

        // La caja de texto va dentro de su píldora: 20 de margen y centrada a lo alto
        private void AcomodarCaja(TextBox caja, RectangleF pildora)
        {
            caja.Font = Fuente(18, FontStyle.Regular);
            Rectangle p = Escalar(pildora);
            int alto = TextRenderer.MeasureText("Ág", caja.Font).Height + 2;
            caja.Bounds = new Rectangle(p.X + (int)(20 * _k), p.Y + (p.Height - alto) / 2, p.Width - (int)(40 * _k), alto);
        }

        private void UbicarEnlace(LinkLabel enlace, float xMenu, float yMenu, float x, float ancho)
        {
            enlace.Font = Fuente(15, FontStyle.Regular);
            int alto = TextRenderer.MeasureText(enlace.Text, enlace.Font).Height;
            enlace.Bounds = new Rectangle((int)(xMenu + x * _k), (int)(yMenu + (52 * _k - alto) / 2f), (int)(ancho * _k), alto);
        }

        private Image FondoDelPaso()
        {
            if (_paso == Paso.Cambiar) return _fondoCambio;
            return _fondoEmail;
        }

        /// <summary>Dibuja una sola vez (por tamaño y paso) el fondo estirado a toda la ventana.</summary>
        private void PrepararFondo(Size tamano)
        {
            if (_fondoListo != null && _fondoListo.Size == tamano && _pasoDelFondo == _paso) return;
            Bitmap nuevo = new Bitmap(tamano.Width, tamano.Height);
            using (Graphics g = Graphics.FromImage(nuevo))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.White);
                Image fondo = FondoDelPaso();
                if (fondo != null) g.DrawImage(fondo, new Rectangle(0, 0, tamano.Width, tamano.Height));
            }
            if (_fondoListo != null) _fondoListo.Dispose();
            _fondoListo = nuevo;
            _pasoDelFondo = _paso;
        }

        /// <summary>Fondo, menú de arriba y las cajas de texto tipo píldora.</summary>
        private void FondoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_fondoListo != null) g.DrawImageUnscaled(_fondoListo, 0, 0);
            Calidad(g);

            // Menú de arriba (píldora gris con sombra)
            RectangleF menu = new RectangleF(1214 * _sx - 343 * _k, 59 * _sy, 343 * _k, 52 * _k);
            Sombra(g, menu, menu.Height / 2f, _k);
            using (GraphicsPath ruta = Redondeado(menu, menu.Height / 2f))
            using (SolidBrush b = new SolidBrush(GrisMenu))
                g.FillPath(b, ruta);

            // Cajas de texto
            foreach (TextBox caja in new[] { DatoTextBox, NuevaTextBox, RepetirTextBox })
            {
                if (!caja.Visible) continue;
                RectangleF p = Escalar(caja == RepetirTextBox ? _caja2 : _caja1);
                using (GraphicsPath ruta = Redondeado(p, p.Height / 2f))
                {
                    g.FillPath(Brushes.White, ruta);
                    using (Pen borde = new Pen(caja.Focused ? AzulPitcher : GrisBorde, Math.Max(1f, _k)))
                        g.DrawPath(borde, ruta);
                }
            }
        }

        // Clic en la píldora (fuera del texto) también pone el cursor en la caja
        private void FondoPanel_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (TextBox caja in new[] { DatoTextBox, NuevaTextBox, RepetirTextBox })
                if (caja.Visible && Escalar(caja == RepetirTextBox ? _caja2 : _caja1).Contains(e.Location)) { caja.Focus(); return; }
        }

        /// <summary>Botón azul tipo píldora con sombra, como en el Figma.</summary>
        private void AccionBoton_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisTarjeta);   // el botón está sobre la tarjeta gris
            float k = _k;
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 6 * k);
            Sombra(g, area, area.Height / 2f, k);

            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            using (SolidBrush brocha = new SolidBrush(encima ? ControlPaint.Light(AzulPitcher, 0.15f) : AzulPitcher))
                g.FillPath(brocha, ruta);

            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private static readonly Dictionary<string, Font> CacheFuentes = new Dictionary<string, Font>();
        private static readonly Dictionary<string, bool> Instaladas = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Plus Jakarta Sans escalada (si no está instalada, Segoe UI). No se les hace Dispose: se reutilizan.</summary>
        private Font Fuente(float pixeles, FontStyle estilo)
        {
            string nombre = FuenteInstalada("Plus Jakarta Sans") ? "Plus Jakarta Sans" : "Segoe UI";
            float tam = (float)Math.Round(Math.Max(6f, pixeles * _k), 1);
            string clave = nombre + "|" + tam + "|" + (int)estilo;
            Font f;
            if (!CacheFuentes.TryGetValue(clave, out f))
            {
                f = new Font(nombre, tam, estilo, GraphicsUnit.Pixel);
                CacheFuentes[clave] = f;
            }
            return f;
        }

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

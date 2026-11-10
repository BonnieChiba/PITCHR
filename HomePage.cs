using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
 
    public partial class HomePage : Form
    {
        public enum Seccion { Emprendimientos, Eventos, Noticias, Mensajes, PersonalizarPerfil, VerPerfil }

        // Crear y guardar secciones 
        private readonly Dictionary<Seccion, UserControl> _secciones = new Dictionary<Seccion, UserControl>();
        private Seccion _seccionActual;
        private readonly Seccion _seccionInicial;

        // Dropdown menu perfil
        private ContextMenuStrip _menuPerfil;

        public HomePage() : this(Seccion.Emprendimientos)
        {
        }

        public HomePage(Seccion seccionInicial)
        {
            InitializeComponent();
            PrepararDiseno();   
            _seccionInicial = seccionInicial;

            //para cerrar el form
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void HomePage_Load(object sender, EventArgs e)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.ActualizarZona();
            this.MostrarSeccion(_seccionInicial);
        }

        // ==================== ZONAS DE NAVEGACIÓN ====================
        private void ActualizarZona()
        {
            bool conSesion = Sesion.Activa;
            ZonaGuestPanel.Visible = !conSesion;
            ZonaUserPanel.Visible = conSesion;
            if (conSesion) this.CargarFotoPerfil();

            if (!conSesion && _seccionActual >= Seccion.Mensajes)
            {
                this.MostrarSeccion(Seccion.Emprendimientos);
            }
        }

        private void CargarFotoPerfil()
        {
            string consulta = Sesion.Rol == 1
                ? "SELECT imageProfile FROM integrative.entrepreneur WHERE idEntrepreneur = @id"
                : "SELECT imageProfile FROM integrative.investor WHERE idInvestor = @id";
            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    MySqlCommand buscar = new MySqlCommand(consulta, dbTemp.dbConnection);
                    buscar.Parameters.AddWithValue("@id", Sesion.IdUsuario);
                    dbTemp.dbConnection.Open();
                    object valor = buscar.ExecuteScalar();
                    Image foto = SeccionBase.CargarImagen(valor == null || valor == DBNull.Value ? null : valor.ToString());
                    _fotoPerfil = foto;   // si no tiene foto queda en null y se dibuja el ícono azul predeterminado
                }
            }
            catch (Exception)
            {
                _fotoPerfil = null;   // si falla la consulta, se muestra el ícono azul predeterminado
            }
            PerfilPictureBox.Invalidate();
        }

        // ==================== CAMBIAR DE SECCIÓN ====================
        private void Menu_Click(object sender, EventArgs e)
        {
            Label opcion = (Label)sender;
            Seccion destino = (Seccion)Enum.Parse(typeof(Seccion), opcion.Tag.ToString());
            this.MostrarSeccion(destino);
        }

        private void MostrarSeccion(Seccion destino)
        {
            if (destino >= Seccion.Mensajes && !Sesion.Activa)
            {
                destino = Seccion.Emprendimientos;
            }

            UserControl panel;
            bool esDePerfil = destino == Seccion.PersonalizarPerfil || destino == Seccion.VerPerfil;
            if (esDePerfil)
            {
                // Los paneles de perfil se crean nuevos cada vez, para que siempre muestren los datos actuales
                panel = this.CrearPanelDePerfil(destino);
                panel.Dock = DockStyle.Fill;
            }
            else if (!_secciones.TryGetValue(destino, out panel))
            {
                panel = this.CrearSeccion(destino);
                panel.Dock = DockStyle.Fill;
                _secciones[destino] = panel;
            }

            ContenidoPanel.SuspendLayout();
            foreach (Control anterior in ContenidoPanel.Controls)
            {
                // Los paneles de perfil anteriores ya no se usan: se liberan. Las secciones del menú se guardan.
                if (!_secciones.ContainsValue(anterior as UserControl) && anterior != panel) anterior.Dispose();
            }
            ContenidoPanel.Controls.Clear();       // quita el panel anterior
            ContenidoPanel.Controls.Add(panel);    // pone el nuevo
            ContenidoPanel.ResumeLayout(true);

            _seccionActual = destino;
            this.MarcarOpcion(destino);
        }

        private UserControl CrearSeccion(Seccion seccion)
        {
            switch (seccion)
            {
                case Seccion.Eventos: return new SeccionEventos();
                case Seccion.Noticias: return new SeccionNoticias();
                case Seccion.Mensajes: return new SeccionMensajes();
                default:
                    SeccionEmprendimientos emprendimientos = new SeccionEmprendimientos();
                    emprendimientos.AbrirEmprendimiento += (s, id) => this.AbrirProducto(id);
                    return emprendimientos;
            }
        }

        // ==================== PÁGINA DE PRODUCTO (clic en tarjeta) ====================
        private void AbrirProducto(int idEmprendimiento)
        {
            SeccionProducto producto = new SeccionProducto(idEmprendimiento);
            producto.Regresar += (s, e) => MostrarSeccion(Seccion.Emprendimientos);
            producto.Contactar += (s, idEntrepreneur) => AbrirMensajesCon(idEntrepreneur);
            producto.Dock = DockStyle.Fill;

            ContenidoPanel.SuspendLayout();
            foreach (Control anterior in ContenidoPanel.Controls)
            {
                if (!_secciones.ContainsValue(anterior as UserControl)) anterior.Dispose();
            }
            ContenidoPanel.Controls.Clear();       // quita Emprendimientos (se queda guardada)
            ContenidoPanel.Controls.Add(producto); // pone la página de producto
            ContenidoPanel.ResumeLayout(true);
        }

        // ==================== MENSAJES (botón CONTACTAR) ====================
        private void AbrirMensajesCon(int idEntrepreneur)
        {
            this.MostrarSeccion(Seccion.Mensajes);
            SeccionMensajes mensajes = _secciones[Seccion.Mensajes] as SeccionMensajes;
            if (mensajes != null) mensajes.AbrirConversacion(idEntrepreneur);
        }

        // ==================== PANELES DE PERFIL  ====================
        private UserControl CrearPanelDePerfil(Seccion seccion)
        {
            bool emprendedor = Sesion.Rol == 1;

            if (seccion == Seccion.PersonalizarPerfil)
            {
                if (emprendedor)
                {
                    SeccionPersonalizaMarca marca = new SeccionPersonalizaMarca();
                    marca.Regresar += (s, e) => MostrarSeccion(Seccion.VerPerfil);
                    marca.MarcaGuardada += (s, e) => OlvidarSeccion(Seccion.Emprendimientos);   // para que se vea la marca nueva
                    return marca;
                }
                SeccionEditarPerfil editar = new SeccionEditarPerfil();
                editar.PerfilActualizado += (s, e) => CargarFotoPerfil();
                return editar;
            }

            // Ver perfil
            if (emprendedor)
            {
                SeccionProducto producto = new SeccionProducto();
                producto.Regresar += (s, e) => MostrarSeccion(Seccion.Emprendimientos);
                producto.CrearMarca += (s, e) => MostrarSeccion(Seccion.PersonalizarPerfil);
                return producto;
            }
            SeccionPerfilInversor perfil = new SeccionPerfilInversor();
            perfil.Regresar += (s, e) => MostrarSeccion(Seccion.Emprendimientos);
            perfil.EditarPerfil += (s, e) => MostrarSeccion(Seccion.PersonalizarPerfil);
            return perfil;
        }

        // Borra una sección guardada para que la próxima vez se vuelva a crear (y a leer de la base)
        private void OlvidarSeccion(Seccion seccion)
        {
            UserControl panel;
            if (_secciones.TryGetValue(seccion, out panel))
            {
                _secciones.Remove(seccion);
                if (panel.Parent == null) panel.Dispose();
            }
        }

        // Pinta de azul la opción de la sección actual (en las dos zonas)
        private void MarcarOpcion(Seccion actual)
        {
            foreach (Label opcion in this.OpcionesMenu())
            {
                bool esActual = opcion.Tag.ToString() == actual.ToString();
                opcion.ForeColor = esActual ? AzulPitcher : Color.Black;
            }
        }

        private IEnumerable<Label> OpcionesMenu()
        {
            foreach (Control c in MenuGuestPanel.Controls) yield return (Label)c;
            foreach (Control c in MenuUserPanel.Controls) yield return (Label)c;
        }

        // ==================== BOTONES DE LA DERECHA ====================
        private void IniciarSesionBoton_Click(object sender, EventArgs e)
        {
            IniciarSesion pantalla = new IniciarSesion();
            pantalla.Show();
            this.Hide();
        }

        // Dropdown Perfil
        private void PerfilPictureBox_Click(object sender, EventArgs e)
        {
            _menuPerfil = new ContextMenuStrip();
            _menuPerfil.Font = new Font(FuenteInstalada("Plus Jakarta Sans") ? "Plus Jakarta Sans" : "Segoe UI", 14f, FontStyle.Regular, GraphicsUnit.Pixel);
            _menuPerfil.ShowImageMargin = false;
            _menuPerfil.Padding = new Padding(4, 6, 4, 6);

            // Encabezado: nombre de usuario y tipo de cuenta
            ToolStripLabel encabezado = new ToolStripLabel("@" + Sesion.Usuario + (Sesion.Rol == 1 ? "  ·  Emprendedor" : "  ·  Inversionista"));
            encabezado.ForeColor = Color.Gray;
            _menuPerfil.Items.Add(encabezado);
            _menuPerfil.Items.Add(new ToolStripSeparator());

            _menuPerfil.Items.Add("Personalizar perfil", null, (s, a) => MostrarSeccion(Seccion.PersonalizarPerfil));
            _menuPerfil.Items.Add("Ver perfil", null, (s, a) => MostrarSeccion(Seccion.VerPerfil));
            _menuPerfil.Items.Add(new ToolStripSeparator());
            ToolStripItem cerrar = _menuPerfil.Items.Add("Cerrar sesión", null, (s, a) => CerrarSesion());
            cerrar.ForeColor = Color.FromArgb(220, 38, 38);
            foreach (ToolStripItem item in _menuPerfil.Items) item.Padding = new Padding(8, 6, 8, 6);

            _menuPerfil.Show(PerfilPictureBox, new Point(PerfilPictureBox.Width - _menuPerfil.PreferredSize.Width, PerfilPictureBox.Height + 4));
        }

        private void CerrarSesion()
        {
            Sesion.Cerrar();
            this.ActualizarZona();              // la barra regresa a la zona de invitado
            this.MostrarSeccion(Seccion.Emprendimientos);
        }


        #region Diseño y pantalla adaptable 

        
        private const float AnchoFigma = 1280f;
        private const float AltoFigma = 832f;
        private const float AltoBarra = 131f;

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);
        private static readonly Color AzulClaro = Color.FromArgb(30, 153, 247);
        private static readonly Color GrisBarra = Color.FromArgb(230, 230, 230);   // #E6E6E6
        private static readonly Color GrisMenu = Color.FromArgb(240, 243, 244);    // #F0F3F4

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private Image _fotoPerfil;
        private float _escala = 1f;

        private void PrepararDiseno()
        {
            // Imágenes de vista previa del diseñador; al ejecutar se dibuja 
            MenuGuestPanel.BackgroundImage = null;
            MenuUserPanel.BackgroundImage = null;
            IniciarSesionBoton.BackgroundImage = null;

            // La foto de perfil se dibuja redonda. La imagen del diseñador es solo la vista previa:
            // al ejecutar se usa la foto de la cuenta o, si no tiene, el ícono azul predeterminado.
            _fotoPerfil = null;
            PerfilPictureBox.Image = null;

            foreach (Control c in new Control[] { BarraPanel, MenuGuestPanel, MenuUserPanel, ZonaGuestPanel, ZonaUserPanel })
                ActivarDobleBuffer(c);

            // Eventos del diseño: 
            MenuGuestPanel.Paint += MenuPanel_Paint;
            MenuUserPanel.Paint += MenuPanel_Paint;
            IniciarSesionBoton.Paint += IniciarSesionBoton_Paint;
            IniciarSesionBoton.MouseEnter += (s, e) => IniciarSesionBoton.Invalidate();
            IniciarSesionBoton.MouseLeave += (s, e) => IniciarSesionBoton.Invalidate();
            PerfilPictureBox.Paint += PerfilPictureBox_Paint;
            foreach (Label opcion in OpcionesMenu())
            {
                Label o = opcion;
                o.MouseEnter += (s, e) => o.ForeColor = AzulPitcher;
                o.MouseLeave += (s, e) => MarcarOpcion(_seccionActual);
            }
            Resize += (s, e) => AjustarATamano();

            // Se guardan las posiciones y fuentes del diseñador para escalarlas 
            GuardarOriginales(BarraPanel);
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

       
        private void AjustarATamano()
        {
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada

            _escala = Math.Min(cliente.Width / AnchoFigma, cliente.Height / AltoFigma);
            float margenX = (cliente.Width - AnchoFigma * _escala) / 2f;
            int altoBarra = (int)Math.Round(AltoBarra * _escala);

            SuspendLayout();
            BarraPanel.Bounds = new Rectangle(0, 0, cliente.Width, altoBarra);
            ContenidoPanel.Bounds = new Rectangle(0, altoBarra, cliente.Width, cliente.Height - altoBarra);
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                bool directo = c.Parent == BarraPanel;
                c.Bounds = new Rectangle(
                    (int)Math.Round(r.X * _escala + (directo ? margenX : 0)),
                    (int)Math.Round(r.Y * _escala),
                    (int)Math.Round(r.Width * _escala),
                    (int)Math.Round(r.Height * _escala));
                c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            ResumeLayout(true);
            BarraPanel.Invalidate(true);
        }

        private static readonly Dictionary<string, Font> CacheFuentes = new Dictionary<string, Font>();

        /// <summary>Misma fuente</summary>
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
        /// <summary>Menú de opciones</summary>
        private void MenuPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel menu = (Panel)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisBarra);
            float k = _escala;
            RectangleF r = new RectangleF(0, 0, menu.Width - 1, 52 * k);
            Sombra(g, r, r.Height / 2f, k);
            using (GraphicsPath ruta = Redondeado(r, r.Height / 2f))
            using (SolidBrush b = new SolidBrush(GrisMenu))
                g.FillPath(b, ruta);
        }

        /// <summary>Botón de Iniciar Sesión.</summary>
        private void IniciarSesionBoton_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisBarra);
            float k = _escala;
            RectangleF area = new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k);
            Sombra(g, area, area.Height / 2f, k);
            bool encima = boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            using (SolidBrush b = new SolidBrush(encima ? ControlPaint.Light(AzulClaro, 0.15f) : AzulClaro))
                g.FillPath(b, ruta);
            TextRenderer.DrawText(g, boton.Text, boton.Font, Rectangle.Round(area), boton.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        /// <summary>Foto de perfil .</summary>
        private void PerfilPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisBarra);
            float k = _escala;
            // Círculo perfecto (mismo ancho y alto) para que la foto o el ícono no se vean ovalados
            float lado = Math.Min(PerfilPictureBox.Width - 3, 52 * k);
            RectangleF circulo = new RectangleF(1, 0, lado, lado);
            Sombra(g, circulo, circulo.Height / 2f, k);
            using (GraphicsPath ruta = new GraphicsPath())
            {
                ruta.AddEllipse(circulo);
                using (SolidBrush b = new SolidBrush(GrisMenu)) g.FillPath(b, ruta);
                if (_fotoPerfil != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    // La foto se recorta al centro para llenar el círculo sin estirarse
                    float escala = Math.Max(circulo.Width / _fotoPerfil.Width, circulo.Height / _fotoPerfil.Height);
                    float w = _fotoPerfil.Width * escala, h = _fotoPerfil.Height * escala;
                    g.DrawImage(_fotoPerfil, circulo.X + (circulo.Width - w) / 2f, circulo.Y + (circulo.Height - h) / 2f, w, h);
                    g.Clip = anterior;
                }
                else
                {
                    DibujarIconoPerfil(g, circulo);   // cuenta sin foto: ícono azul predeterminado
                }
            }
        }

        /// <summary>
        /// Ícono de perfil predeterminado: círculo azul (#2457C5) con una persona blanca.
        /// Se dibuja con figuras (no con una imagen) para que se vea nítido en cualquier tamaño.
        /// </summary>
        private static void DibujarIconoPerfil(Graphics g, RectangleF circulo)
        {
            float d = Math.Min(circulo.Width, circulo.Height);
            float x = circulo.X + (circulo.Width - d) / 2f, y = circulo.Y + (circulo.Height - d) / 2f;

            using (SolidBrush azul = new SolidBrush(Color.FromArgb(36, 87, 197)))
                g.FillEllipse(azul, x, y, d, d);

            using (SolidBrush blanco = new SolidBrush(Color.White))
            {
                // Cabeza
                float r = 0.165f * d;
                g.FillEllipse(blanco, x + 0.5f * d - r, y + 0.31f * d - r, 2 * r, 2 * r);

                // Cuerpo (arco de arriba más plano que el de abajo)
                using (GraphicsPath cuerpo = new GraphicsPath())
                {
                    cuerpo.AddArc(x + 0.185f * d, y + 0.60f * d, 0.64f * d, 0.28f * d, 180, 180);
                    cuerpo.AddArc(x + 0.185f * d, y + 0.575f * d, 0.64f * d, 0.33f * d, 0, 180);
                    cuerpo.CloseFigure();
                    g.FillPath(blanco, cuerpo);
                }
            }
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

        /// <summary>Sombra </summary>
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

        private void LogoPictureBox_Click(object sender, EventArgs e)
        {
            this.MostrarSeccion(Seccion.Emprendimientos);

        }
    }
}

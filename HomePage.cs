using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Pantalla principal. Arriba está la barra de navegación (siempre igual) y abajo un panel
    /// donde se cambia la sección: Emprendimientos, Eventos, Noticias o Mensajes.
    /// La barra tiene dos zonas: la de invitado (Guest) y la de usuario con sesión (User).
    /// </summary>
    public partial class HomePage : Form
    {
        public enum Seccion { Emprendimientos, Eventos, Noticias, Mensajes }

        // Cada sección se crea una sola vez y se guarda aquí; al regresar a ella se muestra la misma
        // (así no se vuelve a consultar la base de datos cada vez que cambias de sección).
        private readonly Dictionary<Seccion, UserControl> _secciones = new Dictionary<Seccion, UserControl>();
        private Seccion _seccionActual;
        private readonly Seccion _seccionInicial;

        // Menú que aparece al dar clic en la foto de perfil
        private ContextMenuStrip _menuPerfil;

        public HomePage() : this(Seccion.Emprendimientos)
        {
        }

        /// <summary>Abre la pantalla principal directo en una sección, ej. new HomePage(HomePage.Seccion.Noticias)</summary>
        public HomePage(Seccion seccionInicial)
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)
            _seccionInicial = seccionInicial;

            // Si cierran esta ventana con la X, se cierra toda la aplicación.
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void HomePage_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio ejecute esto
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.ActualizarZona();
            this.MostrarSeccion(_seccionInicial);
        }

        // ==================== ZONAS DE NAVEGACIÓN ====================
        // Invitado: Emprendimientos · Eventos · Noticias + botón "Iniciar Sesión"
        // Usuario:  Emprendimientos · Eventos · Noticias · Mensajes + foto de perfil
        private void ActualizarZona()
        {
            bool conSesion = Sesion.Activa;
            ZonaGuestPanel.Visible = !conSesion;
            ZonaUserPanel.Visible = conSesion;

            // Mensajes solo existe para usuarios con sesión
            if (!conSesion && _seccionActual == Seccion.Mensajes)
            {
                this.MostrarSeccion(Seccion.Emprendimientos);
            }
        }

        // ==================== CAMBIAR DE SECCIÓN ====================
        // Clic en cualquier opción del menú (de las dos zonas). La propiedad Tag dice a qué sección va.
        private void Menu_Click(object sender, EventArgs e)
        {
            Label opcion = (Label)sender;
            Seccion destino = (Seccion)Enum.Parse(typeof(Seccion), opcion.Tag.ToString());
            this.MostrarSeccion(destino);
        }

        private void MostrarSeccion(Seccion destino)
        {
            if (destino == Seccion.Mensajes && !Sesion.Activa)
            {
                destino = Seccion.Emprendimientos;
            }

            UserControl panel;
            if (!_secciones.TryGetValue(destino, out panel))
            {
                panel = this.CrearSeccion(destino);
                panel.Dock = DockStyle.Fill;
                _secciones[destino] = panel;
            }

            ContenidoPanel.SuspendLayout();
            ContenidoPanel.Controls.Clear();       // quita la sección anterior (no se borra, queda guardada)
            ContenidoPanel.Controls.Add(panel);    // pone la nueva
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
                default: return new SeccionEmprendimientos();
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
        // Zona invitado: "Iniciar Sesión"
        private void IniciarSesionBoton_Click(object sender, EventArgs e)
        {
            IniciarSesion pantalla = new IniciarSesion();
            pantalla.Show();
            this.Hide();
        }

        // Zona usuario: clic en la foto de perfil → menú con opciones
        private void PerfilPictureBox_Click(object sender, EventArgs e)
        {
            _menuPerfil = new ContextMenuStrip();
            _menuPerfil.Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            _menuPerfil.Items.Add(new ToolStripLabel(Sesion.Usuario ?? "Mi cuenta") { Font = new Font("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel) });
            _menuPerfil.Items.Add(new ToolStripSeparator());
            if (Sesion.Rol == 1)   // solo los emprendedores tienen marca
            {
                _menuPerfil.Items.Add("Personaliza tu marca", null, (s, a) =>
                {
                    PersonalizaMarca pantalla = new PersonalizaMarca();
                    pantalla.Show();
                    this.Hide();
                });
            }
            _menuPerfil.Items.Add("Cerrar sesión", null, (s, a) =>
            {
                Sesion.Cerrar();
                this.ActualizarZona();
            });
            _menuPerfil.Show(PerfilPictureBox, new Point(PerfilPictureBox.Width - _menuPerfil.PreferredSize.Width, PerfilPictureBox.Height));
        }


        #region Diseño del Figma y pantalla adaptable (no es necesario modificar esta parte)

        // La barra mide 1280 × 131 en el Figma (frame "News"); el resto de la ventana es para la sección.
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
            // Imágenes de vista previa del diseñador; al ejecutar se dibuja con código.
            MenuGuestPanel.BackgroundImage = null;
            MenuUserPanel.BackgroundImage = null;
            IniciarSesionBoton.BackgroundImage = null;

            // La foto de perfil se dibuja redonda con código
            _fotoPerfil = PerfilPictureBox.Image;
            PerfilPictureBox.Image = null;

            foreach (Control c in new Control[] { BarraPanel, MenuGuestPanel, MenuUserPanel, ZonaGuestPanel, ZonaUserPanel })
                ActivarDobleBuffer(c);

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
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

            // Se guardan las posiciones y fuentes del diseñador para escalarlas después.
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

        /// <summary>
        /// La barra ocupa todo el ancho y su contenido crece parejo y centrado; debajo, el panel de la sección
        /// ocupa todo lo que sobra (cada sección se acomoda sola a ese tamaño).
        /// </summary>
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
        /// <summary>Menú de opciones: píldora gris clara con sombra (se usa en las dos zonas).</summary>
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

        /// <summary>Botón "Iniciar Sesión": píldora azul con sombra.</summary>
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

        /// <summary>Foto de perfil redonda con sombra.</summary>
        private void PerfilPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisBarra);
            float k = _escala;
            RectangleF circulo = new RectangleF(1, 0, PerfilPictureBox.Width - 3, 52 * k - 1);
            Sombra(g, circulo, circulo.Height / 2f, k);
            using (GraphicsPath ruta = new GraphicsPath())
            {
                ruta.AddEllipse(circulo);
                using (SolidBrush b = new SolidBrush(GrisMenu)) g.FillPath(b, ruta);
                if (_fotoPerfil != null)
                {
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    g.DrawImage(_fotoPerfil, circulo);
                    g.Clip = anterior;
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

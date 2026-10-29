using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PITCHR
{
    public partial class EmprendimientosGuest : Form
    {
        // Lista con TODOS los emprendimientos. Por ahora se llena con ejemplos (CargarEjemplos);
        // cuando conecten la base de datos, aquí se guardan los que regrese el SELECT.
        private List<Emprendimiento> _todos = new List<Emprendimiento>();

        // Lo que se está mostrando después de aplicar el buscador y el filtro.
        private List<Emprendimiento> _visibles = new List<Emprendimiento>();

        private string _categoriaElegida = null;   // null = sin filtro
        private int _paginaCarrusel = 0;            // cuál grupo de 3 se ve arriba

        private const int TarjetasPorPagina = 3;    // tarjetas que caben en el carrusel
        private const int MaximoCarrusel = 6;       // los primeros 6 van al carrusel; el resto, a la cuadrícula

        public EmprendimientosGuest()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma + adaptarse a cualquier pantalla (ver región de abajo)

            // Es la primera pantalla: si la cierran con la X, se cierra toda la aplicación.
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void EmprendimientosGuest_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio ejecute esto
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.CargarEjemplos();
            this.AplicarBusquedaYFiltro();
        }

        // ==================== DATOS DE EJEMPLO (TEMPORAL) ====================
        // Cuando hagan el CRUD, este método se cambia por un SELECT a la tabla de emprendimientos:
        // por cada fila se hace  _todos.Add(new Emprendimiento(id, nombre, descripcion, categoria, imagen));
        // Como todavía no hay fotos, Imagen va en null y la tarjeta dibuja un fondo con las iniciales.
        private void CargarEjemplos()
        {
            _todos.Clear();
            _todos.Add(new Emprendimiento(1, "Burritos La Norteña", "Burritos de guisos caseros con tortilla de harina hecha del día.", "Comida", null));
            _todos.Add(new Emprendimiento(2, "Huerto Vivo", "Kits para cultivar hortalizas en casa, aunque tengas poco espacio.", "Naturaleza", null));
            _todos.Add(new Emprendimiento(3, "Limpia Fácil", "Productos de limpieza biodegradables y rellenables.", "Hogar y Limpieza", null));
            _todos.Add(new Emprendimiento(4, "FinanzApp", "App para que los jóvenes organicen sus gastos y empiecen a ahorrar.", "Finanzas", null));
            _todos.Add(new Emprendimiento(5, "Café Sierra Alta", "Café de altura tostado en Chihuahua, en grano y molido.", "Comida", null));
            _todos.Add(new Emprendimiento(6, "Vital Fisio", "Terapia física a domicilio con fisioterapeutas certificados.", "Salud", null));
            _todos.Add(new Emprendimiento(7, "CodeCamp CUU", "Cursos intensivos de programación para estudiantes de prepa y universidad.", "Startup", null));
            _todos.Add(new Emprendimiento(8, "Taller Robles", "Herrería y estructuras metálicas a la medida para casas y negocios.", "Industria", null));
            _todos.Add(new Emprendimiento(9, "Nogal Dulce", "Dulces artesanales de nuez de Delicias, empacados a mano.", "Comida", null));
            _todos.Add(new Emprendimiento(10, "EcoMascotas", "Accesorios para mascotas hechos con materiales reciclados.", "Naturaleza", null));
            _todos.Add(new Emprendimiento(11, "Casa Brillante", "Servicio de limpieza profunda para casas y oficinas.", "Hogar y Limpieza", null));
            _todos.Add(new Emprendimiento(12, "Mente Sana", "Sesiones de psicología en línea a un precio accesible.", "Salud", null));
        }

        // ==================== BUSCADOR Y FILTRO ====================
        private void AplicarBusquedaYFiltro()
        {
            string texto = BuscarTextBox.Text.Trim();

            _visibles.Clear();
            foreach (Emprendimiento emp in _todos)
            {
                bool coincideTexto = texto.Length == 0
                    || emp.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                    || emp.Descripcion.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
                bool coincideCategoria = _categoriaElegida == null
                    || string.Equals(emp.Categoria, _categoriaElegida, StringComparison.OrdinalIgnoreCase);

                if (coincideTexto && coincideCategoria)
                {
                    _visibles.Add(emp);
                }
            }

            _paginaCarrusel = 0;
            this.MostrarTarjetas();
        }

        private void BuscarTextBox_TextChanged(object sender, EventArgs e)
        {
            this.AplicarBusquedaYFiltro();
        }

        private void BuscarBoton_Click(object sender, EventArgs e)
        {
            this.AplicarBusquedaYFiltro();
        }

        private void FiltrarBoton_Click(object sender, EventArgs e)
        {
            FiltrosPanel.Visible = !FiltrosPanel.Visible;
            if (FiltrosPanel.Visible) FiltrosPanel.BringToFront();
        }

        // Clic en una categoría del menú de filtros. Si ya estaba elegida, se quita el filtro.
        private void Filtro_Click(object sender, EventArgs e)
        {
            string categoria = ((Label)sender).Text;
            if (string.Equals(_categoriaElegida, categoria, StringComparison.OrdinalIgnoreCase))
            {
                _categoriaElegida = null;
            }
            else
            {
                _categoriaElegida = categoria;
            }

            FiltrosPanel.Visible = false;
            this.MarcarFiltroElegido();
            this.AplicarBusquedaYFiltro();
        }

        private void MarcarFiltroElegido()
        {
            foreach (Control control in FiltrosPanel.Controls)
            {
                Label opcion = control as Label;
                if (opcion == null) continue;
                bool elegida = string.Equals(opcion.Text, _categoriaElegida, StringComparison.OrdinalIgnoreCase);
                opcion.ForeColor = elegida ? AzulPitcher : Color.Black;
                opcion.Font = EscalarFuente(new Font(_fuentesOriginales[opcion].FontFamily, _fuentesOriginales[opcion].Size,
                    elegida ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel), _escala);
            }
            FiltrarBoton.Text = _categoriaElegida == null ? "Filtrar" : "Filtrar •";
        }

        // ==================== CARRUSEL ====================
        private void FlechaIzquierdaPictureBox_Click(object sender, EventArgs e)
        {
            int paginas = PaginasCarrusel();
            if (paginas <= 1) return;
            _paginaCarrusel = (_paginaCarrusel - 1 + paginas) % paginas;
            this.MostrarCarrusel();
        }

        private void FlechaDerechaPictureBox_Click(object sender, EventArgs e)
        {
            int paginas = PaginasCarrusel();
            if (paginas <= 1) return;
            _paginaCarrusel = (_paginaCarrusel + 1) % paginas;
            this.MostrarCarrusel();
        }

        private int PaginasCarrusel()
        {
            int enCarrusel = Math.Min(_visibles.Count, MaximoCarrusel);
            return (enCarrusel + TarjetasPorPagina - 1) / TarjetasPorPagina;
        }

        // ==================== PENDIENTES (todavía sin conexión a otras pantallas) ====================
        private void IniciarSesionBoton_Click(object sender, EventArgs e)
        {
            // Aquí se abrirá la pantalla de iniciar sesión.
        }

        private void VerMasBoton_Click(object sender, EventArgs e)
        {
            // Aquí se abrirá la pantalla de eventos (el anuncio es fijo).
        }

        private void Tarjeta_Click(object sender, EventArgs e)
        {
            // Aquí se abrirá el detalle del emprendimiento elegido:
            //     Emprendimiento elegido = ((TarjetaEmprendimiento)sender).Datos;
        }


        #region Diseño del Figma, tarjetas y pantalla adaptable (no es necesario modificar esta parte)

        // Medidas del frame "Home" en Figma: 1280 de ancho. La pantalla crece a lo ancho y se recorre hacia abajo.
        private const float AnchoFigma = 1280f;

        private static readonly Color AzulPitcher = Color.FromArgb(0, 96, 214);
        private static readonly Color AzulClaro = Color.FromArgb(30, 153, 247);
        private static readonly Color Naranja = Color.FromArgb(248, 110, 56);
        private static readonly Color GrisFondo = Color.FromArgb(240, 243, 244);

        // Posición de las tarjetas en el Figma (relativas a CarruselPanel y CuadriculaPanel)
        private static readonly int[] XCarrusel = { 20, 346, 672 };
        private static readonly Size TamVertical = new Size(291, 358);
        private static readonly int[] XCuadricula = { 0, 557 };
        private static readonly Size TamHorizontal = new Size(514, 266);
        private const int AltoFilaCuadricula = 324;

        private readonly Dictionary<Control, Rectangle> _posicionesOriginales = new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, Font> _fuentesOriginales = new Dictionary<Control, Font>();
        private readonly List<TarjetaEmprendimiento> _tarjetasCarrusel = new List<TarjetaEmprendimiento>();
        private readonly List<TarjetaEmprendimiento> _tarjetasCuadricula = new List<TarjetaEmprendimiento>();
        private Dictionary<Button, Color> _coloresPildora;
        private float _escala = 1f;
        private bool _ajustando;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private void PrepararDiseno()
        {
            // La imagen del anuncio es solo la vista previa del diseñador; al ejecutar el degradado se dibuja con código.
            BannerPanel.BackgroundImage = null;

            // Las tarjetas "Muestra" solo sirven para ver el diseño en el diseñador; al ejecutar se crean desde los datos.
            foreach (Control muestra in new Control[] { MuestraVertical1, MuestraVertical2, MuestraVertical3,
                                                         MuestraHorizontal1, MuestraHorizontal2, MuestraHorizontal3, MuestraHorizontal4 })
            {
                muestra.Parent.Controls.Remove(muestra);
                muestra.Dispose();
            }

            foreach (Control c in new Control[] { Contenido, BannerPanel, MenuPanel, BuscadorPanel, FiltrosPanel, CarruselPanel, CuadriculaPanel })
                ActivarDobleBuffer(c);

            // Botones tipo píldora (se dibujan en BotonPildora_Paint)
            _coloresPildora = new Dictionary<Button, Color>
            {
                { IniciarSesionBoton, AzulClaro },
                { EventoTagBoton, AzulClaro },
                { VerMasBoton, Naranja },
                { FiltrarBoton, AzulPitcher },
                { BuscarBoton, AzulPitcher }
            };
            foreach (Button boton in _coloresPildora.Keys)
            {
                Button b = boton;
                b.Paint += BotonPildora_Paint;
                b.MouseEnter += (s, e) => b.Invalidate();
                b.MouseLeave += (s, e) => b.Invalidate();
            }
            EventoTagBoton.Cursor = Cursors.Default;
            EventoTagBoton.TabStop = false;

            // Texto guía del buscador (viene de la propiedad Tag)
            BuscarTextBox.HandleCreated += (s, e) => PonerTextoGuia(BuscarTextBox);
            BuscarTextBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; AplicarBusquedaYFiltro(); } };

            // Eventos del diseño: se conectan aquí (no en el diseñador) para que Visual Studio no los borre.
            BannerPanel.Paint += BannerPanel_Paint;
            MenuPanel.Paint += MenuPanel_Paint;
            BuscadorPanel.Paint += BuscadorPanel_Paint;
            FiltrosPanel.Paint += FiltrosPanel_Paint;
            foreach (Control opcion in FiltrosPanel.Controls)
            {
                Control o = opcion;
                o.MouseEnter += (s, e) => o.BackColor = Color.FromArgb(240, 246, 255);
                o.MouseLeave += (s, e) => o.BackColor = Color.Transparent;
            }
            // Clic fuera del menú de filtros: se cierra
            Contenido.MouseDown += (s, e) => FiltrosPanel.Visible = false;
            FiltrosPanel.VisibleChanged += (s, e) => FiltrarBoton.Invalidate();   // la flecha de Filtrar gira
            Resize += (s, e) => AjustarATamano();

            // Se guardan las posiciones y fuentes del diseñador (diseño de 1280 de ancho) para escalarlas después.
            _posicionesOriginales[Contenido] = Contenido.Bounds;
            GuardarOriginales(Contenido);
            Load += (s, e) => AjustarATamano();
        }

        // ---------- Crear las tarjetas a partir de _visibles ----------
        private void MostrarTarjetas()
        {
            SinResultadosLabel.Visible = _visibles.Count == 0;
            this.MostrarCarrusel();

            // Cuadrícula: los que no caben en el carrusel, en filas de 2
            foreach (TarjetaEmprendimiento t in _tarjetasCuadricula) QuitarTarjeta(t);
            _tarjetasCuadricula.Clear();

            int indice = 0;
            for (int i = MaximoCarrusel; i < _visibles.Count; i++, indice++)
            {
                Rectangle r = new Rectangle(XCuadricula[indice % 2], (indice / 2) * AltoFilaCuadricula, TamHorizontal.Width, TamHorizontal.Height);
                TarjetaEmprendimiento t = CrearTarjeta(_visibles[i], TarjetaEmprendimiento.Formas.Horizontal, CuadriculaPanel, r);
                _tarjetasCuadricula.Add(t);
            }

            // El alto de la cuadrícula (y de toda la pantalla) depende de cuántas filas hay
            int filas = (_tarjetasCuadricula.Count + 1) / 2;
            Rectangle cuadricula = _posicionesOriginales[CuadriculaPanel];
            cuadricula.Height = Math.Max(0, filas * AltoFilaCuadricula - (AltoFilaCuadricula - TamHorizontal.Height));
            _posicionesOriginales[CuadriculaPanel] = cuadricula;
            CuadriculaPanel.Visible = filas > 0;

            Rectangle contenido = _posicionesOriginales[Contenido];
            contenido.Height = (filas > 0 ? cuadricula.Bottom : _posicionesOriginales[CarruselPanel].Bottom) + 70;
            _posicionesOriginales[Contenido] = contenido;

            AjustarATamano();
        }

        private void MostrarCarrusel()
        {
            foreach (TarjetaEmprendimiento t in _tarjetasCarrusel) QuitarTarjeta(t);
            _tarjetasCarrusel.Clear();

            int enCarrusel = Math.Min(_visibles.Count, MaximoCarrusel);
            int inicio = _paginaCarrusel * TarjetasPorPagina;
            for (int i = 0; i < TarjetasPorPagina && inicio + i < enCarrusel; i++)
            {
                Rectangle r = new Rectangle(XCarrusel[i], 0, TamVertical.Width, TamVertical.Height);
                _tarjetasCarrusel.Add(CrearTarjeta(_visibles[inicio + i], TarjetaEmprendimiento.Formas.Vertical, CarruselPanel, r));
            }

            bool hayMasPaginas = PaginasCarrusel() > 1;
            FlechaIzquierdaPictureBox.Visible = hayMasPaginas;
            FlechaDerechaPictureBox.Visible = hayMasPaginas;
            AjustarATamano();
        }

        private TarjetaEmprendimiento CrearTarjeta(Emprendimiento datos, TarjetaEmprendimiento.Formas forma, Panel contenedor, Rectangle posicion)
        {
            TarjetaEmprendimiento t = new TarjetaEmprendimiento();
            t.Forma = forma;
            t.Mostrar(datos);
            t.TarjetaClick += Tarjeta_Click;
            _posicionesOriginales[t] = posicion;
            _fuentesOriginales[t] = t.Font;
            contenedor.Controls.Add(t);
            return t;
        }

        private void QuitarTarjeta(TarjetaEmprendimiento t)
        {
            _posicionesOriginales.Remove(t);
            _fuentesOriginales.Remove(t);
            t.Parent.Controls.Remove(t);
            t.Dispose();
        }

        // ---------- Escalado ----------
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
        /// El diseño crece parejo para llenar el ANCHO de la ventana; lo que no cabe a lo alto se recorre con la barra.
        /// </summary>
        private void AjustarATamano()
        {
            if (_ajustando) return;
            Size cliente = ClientSize;
            if (cliente.Width < 50 || cliente.Height < 50) return;   // ventana minimizada
            _ajustando = true;

            float altoDiseno = _posicionesOriginales[Contenido].Height;
            float escala = cliente.Width / AnchoFigma;
            if (altoDiseno * escala > cliente.Height)   // habrá barra de desplazamiento: se le deja su espacio
                escala = (cliente.Width - SystemInformation.VerticalScrollBarWidth) / AnchoFigma;
            _escala = escala;

            SuspendLayout();
            Contenido.SuspendLayout();
            foreach (KeyValuePair<Control, Rectangle> par in _posicionesOriginales)
            {
                Control c = par.Key;
                Rectangle r = par.Value;
                if (c == Contenido) continue;
                c.Bounds = new Rectangle((int)Math.Round(r.X * _escala), (int)Math.Round(r.Y * _escala),
                                         (int)Math.Round(r.Width * _escala), (int)Math.Round(r.Height * _escala));
                if (!(c is TarjetaEmprendimiento)) c.Font = EscalarFuente(_fuentesOriginales[c], _escala);
            }
            Contenido.Bounds = new Rectangle(AutoScrollPosition.X, AutoScrollPosition.Y,
                                             (int)Math.Round(AnchoFigma * _escala), (int)Math.Round(altoDiseno * _escala));
            Contenido.ResumeLayout(true);
            ResumeLayout(true);
            Contenido.Invalidate(true);
            _ajustando = false;
        }

        private static readonly Dictionary<string, Font> CacheFuentes = new Dictionary<string, Font>();

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
        /// <summary>Anuncio azul: rectángulo redondeado (radio 40) con degradado #405DFF → #09096B.</summary>
        private void BannerPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            RectangleF r = new RectangleF(0, 0, BannerPanel.Width - 1, BannerPanel.Height - 1);
            using (GraphicsPath ruta = Redondeado(r, 40 * _escala))
            using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(0, 0, BannerPanel.Width, 1),
                       Color.FromArgb(64, 93, 255), Color.FromArgb(9, 9, 107), 0f))
                g.FillPath(b, ruta);
        }

        /// <summary>Menú gris (Emprendimientos · Eventos · Noticias) con sombra.</summary>
        private void MenuPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF r = new RectangleF(0, 0, MenuPanel.Width - 1, 52 * k);
            Sombra(g, r, r.Height / 2f, k);
            using (GraphicsPath ruta = Redondeado(r, r.Height / 2f))
            using (SolidBrush b = new SolidBrush(GrisFondo))
                g.FillPath(b, ruta);
        }

        /// <summary>Barra de búsqueda: píldora gris con borde azul y sombra.</summary>
        private void BuscadorPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF r = new RectangleF(2 * k, 2 * k, 887 * k, 53 * k);
            Sombra(g, r, r.Height / 2f, k);
            using (GraphicsPath ruta = Redondeado(r, r.Height / 2f))
            {
                using (SolidBrush b = new SolidBrush(GrisFondo)) g.FillPath(b, ruta);
                using (Pen p = new Pen(BuscarTextBox.Focused ? AzulClaro : AzulPitcher, Math.Max(1f, k))) g.DrawPath(p, ruta);
            }
        }

        /// <summary>Menú de categorías: tarjeta blanca con líneas entre opciones.</summary>
        private void FiltrosPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = _escala;
            RectangleF r = new RectangleF(1, 1, FiltrosPanel.Width - 3, FiltrosPanel.Height - 3);
            using (GraphicsPath ruta = Redondeado(r, 12 * k))
            using (Pen borde = new Pen(Color.FromArgb(217, 217, 217), Math.Max(1f, k)))
                g.DrawPath(borde, ruta);
            using (Pen linea = new Pen(Color.FromArgb(225, 225, 225), Math.Max(1f, k)))
                for (int i = 1; i < 7; i++)
                {
                    float y = (4 + i * 40) * k;
                    g.DrawLine(linea, 0, y, FiltrosPanel.Width, y);
                }
        }

        /// <summary>Botones redondeados: Iniciar Sesión, Evento presencial, Ver más, Filtrar y la lupa.</summary>
        private void BotonPildora_Paint(object sender, PaintEventArgs e)
        {
            Button boton = (Button)sender;
            Graphics g = e.Graphics;
            Calidad(g);

            // Sobre el anuncio y la barra de búsqueda el fondo es transparente (se ve lo que hay detrás)
            bool fondoTransparente = boton.Parent == BannerPanel || boton.Parent == BuscadorPanel;
            bool conSombra = boton != EventoTagBoton;
            if (!fondoTransparente) g.Clear(Color.White);

            float k = _escala;
            RectangleF area = conSombra
                ? new RectangleF(3 * k, 0, boton.Width - 6 * k - 1, boton.Height - 8 * k)
                : new RectangleF(0, 0, boton.Width - 1, boton.Height - 1);
            if (boton.Parent == BuscadorPanel)
            {
                // La lupa y "Filtrar" van pegados a la barra; su sombra es la de la barra
                area = new RectangleF(1, 1, boton.Width - 2, 53 * k - 1);
                if (boton == FiltrarBoton) area.Width = 141 * k;
            }
            else if (conSombra)
            {
                Sombra(g, area, area.Height / 2f, k);
            }

            bool encima = boton.Enabled && boton.Cursor == Cursors.Hand && boton.ClientRectangle.Contains(boton.PointToClient(Cursor.Position));
            Color color = _coloresPildora[boton];
            if (encima) color = ControlPaint.Light(color, 0.15f);
            using (GraphicsPath ruta = Redondeado(area, area.Height / 2f))
            using (SolidBrush b = new SolidBrush(color))
                g.FillPath(b, ruta);

            if (boton == BuscarBoton && boton.Image != null)
            {
                float lado = 30 * k;
                g.DrawImage(boton.Image, area.X + (area.Width - lado) / 2f, area.Y + (area.Height - lado) / 2f, lado, lado);
                return;
            }
            if (boton == FiltrarBoton)
            {
                // "Filtrar" + flecha hacia abajo
                Rectangle rt = Rectangle.Round(new RectangleF(area.X + 18 * k, area.Y, 80 * k, area.Height));
                TextRenderer.DrawText(g, boton.Text, boton.Font, rt, Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                float cx = area.X + 108 * k, cy = area.Y + area.Height / 2f;
                using (Pen p = new Pen(Color.White, 2.6f * k))
                {
                    p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round;
                    bool abierto = FiltrosPanel.Visible;
                    float dy = abierto ? -4 * k : 4 * k;
                    g.DrawLines(p, new[] { new PointF(cx - 9 * k, cy - dy), new PointF(cx, cy + dy), new PointF(cx + 9 * k, cy - dy) });
                }
                return;
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

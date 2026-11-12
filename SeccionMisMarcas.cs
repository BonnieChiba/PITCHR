using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PITCHR
{
    /// <summary>
    /// Panel con una lista de emprendimientos en tarjetas (frames "MisMarcas" del Figma). Sirve para dos pantallas:
    ///  • Mis Marcas (solo emprendedor): sus marcas, cada una con botón EDITAR, y el botón "+ Nueva marca".
    ///  • Favoritos (los dos roles): los emprendimientos a los que les dio corazón, con el corazón en rojo.
    /// No tiene diseñador: las tarjetas se crean con código según lo que haya en la base.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    public class SeccionMisMarcas : SeccionBase
    {
        public enum Modos { MisMarcas, Favoritos }

        private DBConn dbconn;                                                  //Conectar
        private MySqlDataAdapter entrepreneurshipDA = new MySqlDataAdapter();   //Extraer
        private DataTable entrepreneurshiptable = new DataTable();              //Almacenar

        private readonly Modos _modo;

        private readonly Label TituloLabel = new Label();
        private readonly Label VacioLabel = new Label();
        private readonly Button NuevaMarcaBoton = new Button();
        private readonly List<TarjetaMarca> _tarjetas = new List<TarjetaMarca>();

        /// <summary>Avisos para HomePage.</summary>
        public event EventHandler<int> AbrirEmprendimiento;   // clic en la tarjeta: abre la página de producto
        public event EventHandler<int> EditarMarca;           // botón EDITAR (0 = marca nueva)
        public event EventHandler FavoritoQuitado;            // se quitó un corazón (Emprendimientos se actualiza)

        public SeccionMisMarcas() : this(Modos.MisMarcas)
        {
        }

        public SeccionMisMarcas(Modos modo)
        {
            _modo = modo;
            BackColor = Color.White;
            AutoScroll = true;
            DoubleBuffered = true;

            TituloLabel.Text = modo == Modos.MisMarcas ? "Mis Marcas" : "Favoritos";
            TituloLabel.ForeColor = AzulPitcher;
            TituloLabel.BackColor = Color.Transparent;
            TituloLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(TituloLabel);

            VacioLabel.Text = modo == Modos.MisMarcas
                ? "Aún no tienes marcas. Crea la primera con \"+ Nueva marca\"."
                : "Aún no tienes favoritos. Dale corazón a un emprendimiento para guardarlo aquí.";
            VacioLabel.ForeColor = Color.FromArgb(140, 140, 150);
            VacioLabel.BackColor = Color.Transparent;
            VacioLabel.Visible = false;
            Controls.Add(VacioLabel);

            NuevaMarcaBoton.Text = "+ Nueva marca";
            NuevaMarcaBoton.ForeColor = Color.White;
            NuevaMarcaBoton.Cursor = Cursors.Hand;
            NuevaMarcaBoton.Visible = modo == Modos.MisMarcas;
            NuevaMarcaBoton.Click += (s, e) => { if (EditarMarca != null) EditarMarca(this, 0); };
            HacerPildora(NuevaMarcaBoton, AzulBoton);
            Controls.Add(NuevaMarcaBoton);

            Resize += (s, e) => Acomodar();
            Load += SeccionMisMarcas_Load;
        }

        private void SeccionMisMarcas_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadDBCommands();
            this.LoadData();
        }

        // ==================== COMANDOS (SELECT) ====================
        private void LoadDBCommands()
        {
            if (_modo == Modos.MisMarcas)
            {
                // Todas las marcas del emprendedor que inició sesión
                entrepreneurshipDA.SelectCommand = new MySqlCommand(
                    "SELECT idEntrepreneurship, brand, shortDescription, category, imageHome " +
                    "FROM integrative.entrepreneurship WHERE idEntrepreneur = @yo ORDER BY idEntrepreneurship");
            }
            else
            {
                // Los emprendimientos a los que les dio corazón (tabla entrepreneurshipLike), el más reciente primero
                entrepreneurshipDA.SelectCommand = new MySqlCommand(
                    "SELECT e.idEntrepreneurship, e.brand, e.shortDescription, e.category, e.imageHome " +
                    "FROM integrative.entrepreneurshipLike l " +
                    "INNER JOIN integrative.entrepreneurship e ON e.idEntrepreneurship = l.idEntrepreneurship " +
                    "WHERE l.idUser = @yo AND l.role = @rol ORDER BY l.date DESC");
            }
        }

        // ==================== LEER (SELECT) ====================
        private void LoadData()
        {
            dbconn = new DBConn();
            entrepreneurshipDA.SelectCommand.Connection = dbconn.dbConnection;
            entrepreneurshipDA.SelectCommand.Parameters.Clear();
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
            entrepreneurshipDA.SelectCommand.Parameters.AddWithValue("@rol", Sesion.Rol);
            try
            {
                entrepreneurshiptable.Clear();
                entrepreneurshipDA.Fill(entrepreneurshiptable);
                this.ShowData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los emprendimientos:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Crea una tarjeta por cada fila
        private void ShowData()
        {
            foreach (TarjetaMarca vieja in _tarjetas)
            {
                Controls.Remove(vieja);
                vieja.Dispose();
            }
            _tarjetas.Clear();

            foreach (DataRow fila in entrepreneurshiptable.Rows)
            {
                TarjetaMarca tarjeta = new TarjetaMarca(_modo);
                tarjeta.Id = Convert.ToInt32(fila["idEntrepreneurship"]);
                tarjeta.Titulo = Convert.ToString(fila["brand"]);
                tarjeta.Descripcion = Convert.ToString(fila["shortDescription"]);
                tarjeta.Categoria = Convert.ToString(fila["category"]);
                tarjeta.Imagen = CargarImagen(Convert.ToString(fila["imageHome"]));
                tarjeta.Abrir += (s, e) => { if (AbrirEmprendimiento != null) AbrirEmprendimiento(this, ((TarjetaMarca)s).Id); };
                tarjeta.Editar += (s, e) => { if (EditarMarca != null) EditarMarca(this, ((TarjetaMarca)s).Id); };
                tarjeta.QuitarCorazon += Tarjeta_QuitarCorazon;
                _tarjetas.Add(tarjeta);
                Controls.Add(tarjeta);
            }

            VacioLabel.Visible = _tarjetas.Count == 0;
            this.Acomodar();
        }

        // ==================== QUITAR DE FAVORITOS (DELETE) ====================
        // Clic en el corazón rojo: borra el like y actualiza el total de likes del emprendimiento.
        private void Tarjeta_QuitarCorazon(object sender, EventArgs e)
        {
            TarjetaMarca tarjeta = (TarjetaMarca)sender;
            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    dbTemp.dbConnection.Open();
                    MySqlCommand borrar = new MySqlCommand(
                        "DELETE FROM integrative.entrepreneurshipLike WHERE idEntrepreneurship = @id AND idUser = @yo AND role = @rol", dbTemp.dbConnection);
                    borrar.Parameters.AddWithValue("@id", tarjeta.Id);
                    borrar.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
                    borrar.Parameters.AddWithValue("@rol", Sesion.Rol);
                    borrar.ExecuteNonQuery();

                    MySqlCommand total = new MySqlCommand(
                        "UPDATE integrative.entrepreneurship SET likes = " +
                        "(SELECT COUNT(*) FROM integrative.entrepreneurshipLike WHERE idEntrepreneurship = @id) " +
                        "WHERE idEntrepreneurship = @id", dbTemp.dbConnection);
                    total.Parameters.AddWithValue("@id", tarjeta.Id);
                    total.ExecuteNonQuery();
                }

                this.LoadData();   // vuelve a leer: la tarjeta ya no aparece
                if (FavoritoQuitado != null) FavoritoQuitado(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo quitar de favoritos:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        // Medidas del Figma (sin la barra de arriba de HomePage): tarjetas de 509 × 259 en dos columnas
        private const float AnchoFigma = 1280f, AltoFigma = 665f;
        private const float ColumnaIzq = 81f, ColumnaDer = 666f, PrimeraFila = 119f, SeparacionFilas = 311f;
        private const float AnchoTarjeta = 509f, AltoTarjeta = 259f, Borde = 8f;   // Borde = espacio para la sombra

        private void Acomodar()
        {
            if (ClientSize.Width < 50 || ClientSize.Height < 50) return;
            float k = Math.Min(ClientSize.Width / AnchoFigma, ClientSize.Height / AltoFigma);
            float mx = Math.Max(0, (ClientSize.Width - AnchoFigma * k) / 2f);
            int dy = AutoScrollPosition.Y;   // si ya bajó con la rueda del mouse

            SuspendLayout();
            TituloLabel.Font = EscalarFuente(FuenteBase(50f, FontStyle.Bold), k);
            TituloLabel.Bounds = R(mx + ColumnaIzq * k, 22 * k + dy, 600 * k, 72 * k);

            NuevaMarcaBoton.Font = EscalarFuente(FuenteBase(17f, FontStyle.Bold), k);
            NuevaMarcaBoton.Bounds = R(mx + (ColumnaDer + AnchoTarjeta - 190) * k, 34 * k + dy, 190 * k, 50 * k);

            VacioLabel.Font = EscalarFuente(FuenteBase(18f, FontStyle.Regular), k);
            VacioLabel.Bounds = R(mx + ColumnaIzq * k, PrimeraFila * k + dy, 1100 * k, 40 * k);

            for (int i = 0; i < _tarjetas.Count; i++)
            {
                float x = i % 2 == 0 ? ColumnaIzq : ColumnaDer;
                float y = PrimeraFila + (i / 2) * SeparacionFilas;
                _tarjetas[i].K = k;
                _tarjetas[i].Bounds = R(mx + (x - Borde) * k, (y - Borde) * k + dy, (AnchoTarjeta + Borde * 2) * k, (AltoTarjeta + Borde * 2) * k);
            }

            // Espacio de abajo para que la última fila no quede pegada al borde al bajar
            int filas = (_tarjetas.Count + 1) / 2;
            AutoScrollMinSize = new Size(0, (int)((PrimeraFila + filas * SeparacionFilas + 20) * k));
            ResumeLayout(true);
        }

        // Fuentes al tamaño del Figma (se crean una sola vez); EscalarFuente las ajusta a la ventana
        private static readonly Dictionary<string, Font> FuentesBase = new Dictionary<string, Font>();
        private static Font FuenteBase(float pixeles, FontStyle estilo)
        {
            string clave = pixeles + "|" + (int)estilo;
            Font f;
            if (!FuentesBase.TryGetValue(clave, out f))
            {
                f = new Font("Plus Jakarta Sans", pixeles, estilo, GraphicsUnit.Pixel);
                FuentesBase[clave] = f;
            }
            return f;
        }

        private static Rectangle R(float x, float y, float w, float h)
        {
            return new Rectangle((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(w), (int)Math.Round(h));
        }

        /// <summary>
        /// Una tarjeta del Figma: foto a la izquierda, nombre y descripción a la derecha, y abajo
        /// el botón EDITAR (Mis Marcas) o la categoría y el corazón rojo (Favoritos).
        /// </summary>
        private class TarjetaMarca : Control
        {
            public int Id;
            public string Titulo = "", Descripcion = "", Categoria = "";
            public Image Imagen;
            private readonly Modos _modo;
            private float _k = 1f;

            public event EventHandler Abrir, Editar, QuitarCorazon;

            public TarjetaMarca(Modos modo)
            {
                _modo = modo;
                Cursor = Cursors.Hand;
                BackColor = Color.White;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            }

            public float K { get { return _k; } set { _k = value; Invalidate(); } }

            // Zonas en medidas del Figma (dentro de la tarjeta de 509 × 259)
            private RectangleF Tarjeta { get { return new RectangleF(Borde * _k, Borde * _k, AnchoTarjeta * _k, AltoTarjeta * _k); } }
            private RectangleF Zona(float x, float y, float w, float h)
            {
                RectangleF t = Tarjeta;
                return new RectangleF(t.X + x * _k, t.Y + y * _k, w * _k, h * _k);
            }
            private RectangleF BotonEditar { get { return Zona(344, 196, 133, 44); } }
            private RectangleF Corazon { get { return Zona(445, 200, 37, 37); } }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                if (e.Button != MouseButtons.Left) return;
                if (_modo == Modos.MisMarcas && BotonEditar.Contains(e.Location))
                {
                    if (Editar != null) Editar(this, EventArgs.Empty);
                }
                else if (_modo == Modos.Favoritos && RectangleF.Inflate(Corazon, 6 * _k, 6 * _k).Contains(e.Location))
                {
                    if (QuitarCorazon != null) QuitarCorazon(this, EventArgs.Empty);
                }
                else if (Tarjeta.Contains(e.Location))
                {
                    if (Abrir != null) Abrir(this, EventArgs.Empty);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                Invalidate();   // para el efecto al pasar el mouse sobre EDITAR
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                Calidad(g);
                g.Clear(Parent != null ? Parent.BackColor : Color.White);
                float k = _k;
                RectangleF t = Tarjeta;
                float radio = 20 * k;

                // Tarjeta gris clarito con sombra
                Sombra(g, t, radio, k);
                using (GraphicsPath ruta = Redondeado(t, radio))
                {
                    using (SolidBrush fondo = new SolidBrush(Color.FromArgb(246, 246, 246)))
                        g.FillPath(fondo, ruta);

                    // Foto de la izquierda (173 de ancho), recortada a las esquinas redondeadas de la tarjeta
                    Region anterior = g.Clip;
                    g.SetClip(ruta, CombineMode.Intersect);
                    RectangleF foto = Zona(0, 0, 173, 259);
                    g.SetClip(foto, CombineMode.Intersect);
                    if (Imagen != null)
                    {
                        DibujarCubriendo(g, Imagen, foto);
                    }
                    else
                    {
                        using (LinearGradientBrush azul = new LinearGradientBrush(foto, Color.FromArgb(0, 96, 214), Color.FromArgb(56, 189, 248), 60f))
                            g.FillRectangle(azul, foto);
                    }
                    g.Clip = anterior;
                }

                // Nombre y descripción
                TextFormatFlags una = TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
                TextFormatFlags varias = TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                TextRenderer.DrawText(g, Titulo, F(32, FontStyle.Bold), Rectangle.Round(Zona(188, 36, 300, 46)), Color.Black, una | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(g, Descripcion, F(16, FontStyle.Regular), Rectangle.Round(Zona(188, 92, 296, 92)), Color.FromArgb(40, 40, 40), varias);

                if (_modo == Modos.MisMarcas)
                {
                    // Botón EDITAR azul
                    RectangleF b = BotonEditar;
                    bool encima = b.Contains(PointToClient(Cursor.Position));
                    using (GraphicsPath pildora = Redondeado(b, b.Height / 2f))
                    using (SolidBrush azul = new SolidBrush(encima ? Color.FromArgb(38, 147, 235) : Color.FromArgb(8, 127, 223)))
                        g.FillPath(azul, pildora);
                    TextRenderer.DrawText(g, "EDITAR", F(19, FontStyle.Bold), Rectangle.Round(b), Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                }
                else
                {
                    // Etiqueta naranja con la categoría
                    if (!string.IsNullOrWhiteSpace(Categoria))
                    {
                        Font f = F(14, FontStyle.Bold);
                        string texto = Categoria.ToUpper();
                        float ancho = TextRenderer.MeasureText(g, texto, f, Size.Empty, TextFormatFlags.NoPadding).Width + 30 * k;
                        RectangleF etiqueta = Zona(191, 203, 0, 34);
                        etiqueta.Width = Math.Min(ancho, 230 * k);
                        using (GraphicsPath pildora = Redondeado(etiqueta, etiqueta.Height / 2f))
                        using (SolidBrush naranja = new SolidBrush(Color.FromArgb(125, 248, 110, 56)))
                            g.FillPath(naranja, pildora);
                        TextRenderer.DrawText(g, texto, f, Rectangle.Round(etiqueta), Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                    }

                    // Corazón rojo (clic para quitarlo de favoritos)
                    DibujarCorazon(g, Corazon);
                }
            }

            private Font F(float pixeles, FontStyle estilo)
            {
                return EscalarFuente(FuenteBase(pixeles, estilo), _k);
            }

            // El mismo corazón de las tarjetas de Emprendimientos, lleno de rojo
            private static void DibujarCorazon(Graphics g, RectangleF r)
            {
                float x = r.X, y = r.Y, w = r.Width, h = r.Height;
                using (GraphicsPath p = new GraphicsPath())
                {
                    p.AddBezier(x + w * 0.50f, y + h * 0.86f, x + w * 0.10f, y + h * 0.58f, x + w * 0.02f, y + h * 0.40f, x + w * 0.08f, y + h * 0.26f);
                    p.AddBezier(x + w * 0.08f, y + h * 0.26f, x + w * 0.16f, y + h * 0.08f, x + w * 0.42f, y + h * 0.08f, x + w * 0.50f, y + h * 0.28f);
                    p.AddBezier(x + w * 0.50f, y + h * 0.28f, x + w * 0.58f, y + h * 0.08f, x + w * 0.84f, y + h * 0.08f, x + w * 0.92f, y + h * 0.26f);
                    p.AddBezier(x + w * 0.92f, y + h * 0.26f, x + w * 0.98f, y + h * 0.40f, x + w * 0.90f, y + h * 0.58f, x + w * 0.50f, y + h * 0.86f);
                    p.CloseFigure();
                    using (SolidBrush rojo = new SolidBrush(Color.FromArgb(232, 59, 59)))
                        g.FillPath(rojo, p);
                }
            }
        }

        #endregion
    }
}

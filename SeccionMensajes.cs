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
    /// Sección "Mensajes" (frame "Messages" del Figma).
    /// Izquierda: las conversaciones. Derecha: los mensajes de la conversación elegida y la caja para escribir.
    ///
    /// Tabla integrative.message:
    ///   idMessage, idInvestor, idEntrepreneur, senderRole (0 = lo escribió el inversor, 1 = el emprendedor), content, date
    /// Una conversación es siempre UN inversor con UN emprendedor.
    ///
    /// Crear = Enviar  ·  Leer = abrir una conversación  ·  Actualizar = clic derecho en un mensaje tuyo → Editar.
    /// Los mensajes NO se pueden borrar (se guardan como evidencia). Si se editan, se guarda cuándo (editedDate).
    /// </summary>
    public partial class SeccionMensajes : SeccionBase
    {
        private DBConn dbconn;                                                //Conectar
        private MySqlDataAdapter conversacionesDA = new MySqlDataAdapter();   //Extraer la lista de conversaciones
        private MySqlDataAdapter messageDA = new MySqlDataAdapter();          //Extraer, insertar, editar y borrar mensajes
        private DataTable conversacionestable = new DataTable();             //Almacenar
        private DataTable messagetable = new DataTable();                     //Almacenar

        // Datos ya listos para dibujar
        private class Conversacion
        {
            public int IdOtro;          // idEntrepreneur o idInvestor de la otra persona
            public string Nombre;
            public Image Foto;
            public string Ultimo;       // último mensaje
            public DateTime? Fecha;     // cuándo se mandó el último mensaje
        }

        private class Mensaje
        {
            public int Id;
            public bool EsMio;
            public string Texto;
            public DateTime? Fecha;
            public DateTime? Editado;   // null = nunca se ha editado
            public RectangleF Burbuja;  // dónde quedó dibujado (para el clic derecho)
        }

        private readonly List<Conversacion> _conversaciones = new List<Conversacion>();
        private readonly List<Mensaje> _mensajes = new List<Mensaje>();
        private int _idOtro = 0;           // conversación abierta (0 = ninguna)
        private int _idEditando = 0;       // mensaje que se está editando (0 = ninguno)
        private string _usuarioCargado = "";
        private RectangleF _caja;              // dónde se dibuja la caja redondeada para escribir   // para saber si cambió la sesión
        private Image _miFoto;
        private Image _fotoOtro;
        private ContextMenuStrip _menuMensaje;

        public SeccionMensajes()
        {
            InitializeComponent();
            PrepararDiseno();   // estilo del Figma (ver región de abajo)
        }

        // Rol 0 = inversor, 1 = emprendedor (igual que en CrearCuenta)
        private bool SoyInversor()
        {
            return Sesion.Rol == 0;
        }

        private int IdInvestor()
        {
            return SoyInversor() ? Sesion.IdUsuario : _idOtro;
        }

        private int IdEntrepreneur()
        {
            return SoyInversor() ? _idOtro : Sesion.IdUsuario;
        }

        private void SeccionMensajes_Load(object sender, EventArgs e)
        {
            // Evita que el diseñador de Visual Studio intente conectarse a MySQL
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadData();

            // HomePage guarda esta sección: cada vez que se vuelve a mostrar, se leen los mensajes nuevos
            this.ParentChanged += (s, a) => { if (this.Parent != null) this.LoadData(); };
        }

        // ==================== CONSULTAS ====================
        private void LoadDBCommands()
        {
            // Las columnas cambian según quién inició sesión: si soy inversor, "la otra persona" es un emprendedor.
            string mia = SoyInversor() ? "idInvestor" : "idEntrepreneur";
            string otra = SoyInversor() ? "idEntrepreneur" : "idInvestor";
            string tablaOtra = SoyInversor() ? "integrative.entrepreneur" : "integrative.investor";

            // LEER: con quién he hablado y cuál fue el último mensaje de cada conversación
            conversacionesDA.SelectCommand = new MySqlCommand(
                "SELECT o." + otra + " AS idOtro, o.name, o.lastName, o.imageProfile, m.content, m.`date` " +
                "FROM integrative.message m " +
                "INNER JOIN " + tablaOtra + " o ON o." + otra + " = m." + otra + " " +
                "WHERE m." + mia + " = @yo " +
                "AND m.idMessage = (SELECT MAX(m2.idMessage) FROM integrative.message m2 " +
                "                   WHERE m2.idInvestor = m.idInvestor AND m2.idEntrepreneur = m.idEntrepreneur) " +
                "ORDER BY m.idMessage DESC");

            // LEER: los mensajes de una conversación
            messageDA.SelectCommand = new MySqlCommand(
                "SELECT idMessage, senderRole, content, `date`, editedDate FROM integrative.message " +
                "WHERE idInvestor = @idInvestor AND idEntrepreneur = @idEntrepreneur ORDER BY `date`, idMessage");

            // CREAR: enviar un mensaje
            messageDA.InsertCommand = new MySqlCommand(
                "INSERT INTO integrative.message (idInvestor, idEntrepreneur, senderRole, content, `date`) " +
                "VALUES (@idInvestor, @idEntrepreneur, @senderRole, @content, NOW())");

            // ACTUALIZAR: editar un mensaje propio. La fecha original (`date`) no cambia; se guarda cuándo se editó.
            messageDA.UpdateCommand = new MySqlCommand(
                "UPDATE integrative.message SET content = @content, editedDate = NOW() WHERE idMessage = @idMessage");

            // (No hay DELETE: los mensajes no se pueden borrar para mantener evidencia de las conversaciones.)
        }

        private void LoadData()
        {
            if (!Sesion.Activa) return;

            // Si entró otra persona, se olvida la conversación que tenía abierta la anterior
            string usuario = Sesion.Rol + "-" + Sesion.IdUsuario;
            if (usuario != _usuarioCargado)
            {
                _usuarioCargado = usuario;
                _idOtro = 0;
            }

            this.LoadDBCommands();   // las consultas dependen de si es inversor o emprendedor
            dbconn = new DBConn();
            conversacionesDA.SelectCommand.Connection = dbconn.dbConnection;
            conversacionesDA.SelectCommand.Parameters.Clear();
            conversacionesDA.SelectCommand.Parameters.AddWithValue("@yo", Sesion.IdUsuario);
            try
            {
                _miFoto = this.LeerMiFoto();
                conversacionestable.Clear();
                conversacionesDA.Fill(conversacionestable);
                this.ShowConversaciones(conversacionestable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Si ya había una conversación abierta, se vuelve a leer; si no, se abre la más reciente
            if (_idOtro == 0 && _conversaciones.Count > 0) _idOtro = _conversaciones[0].IdOtro;
            this.AbrirConversacion(_idOtro);
        }

        private Image LeerMiFoto()
        {
            string tabla = SoyInversor() ? "integrative.investor" : "integrative.entrepreneur";
            string columnaId = SoyInversor() ? "idInvestor" : "idEntrepreneur";
            using (DBConn dbTemp = new DBConn())
            {
                MySqlCommand buscar = new MySqlCommand("SELECT imageProfile FROM " + tabla + " WHERE " + columnaId + " = @id", dbTemp.dbConnection);
                buscar.Parameters.AddWithValue("@id", Sesion.IdUsuario);
                dbTemp.dbConnection.Open();
                return CargarImagen(Convert.ToString(buscar.ExecuteScalar()));
            }
        }

        private void ShowConversaciones(DataTable tabla)
        {
            _conversaciones.Clear();
            foreach (DataRow fila in tabla.Rows)
            {
                Conversacion c = new Conversacion();
                c.IdOtro = Convert.ToInt32(fila["idOtro"]);
                c.Nombre = (Convert.ToString(fila["name"]) + " " + Convert.ToString(fila["lastName"])).Trim();
                c.Foto = CargarImagen(Convert.ToString(fila["imageProfile"]));
                c.Ultimo = Convert.ToString(fila["content"]);
                c.Fecha = LeerFecha(fila["date"]);
                _conversaciones.Add(c);
            }
            SinConversacionesLabel.Visible = _conversaciones.Count == 0;
            SinConversacionesLabel.Text = SoyInversor()
                ? "Aún no tienes conversaciones.\nEntra a un emprendimiento y presiona CONTACTAR."
                : "Aún no tienes conversaciones.\nAquí aparecerán los mensajes de los inversionistas.";
            this.AcomodarListas();
        }

        /// <summary>
        /// Abre la conversación con esa persona (idEntrepreneur si soy inversor, idInvestor si soy emprendedor).
        /// HomePage la usa para el botón CONTACTAR de la página de producto.
        /// </summary>
        public void AbrirConversacion(int idOtro)
        {
            _idOtro = idOtro;
            this.CancelarEdicion();

            bool hayChat = _idOtro > 0;
            EncabezadoPanel.Visible = hayChat;
            ChatPanel.Visible = hayChat;
            MensajeTextBox.Enabled = hayChat;
            EnviarBoton.Enabled = hayChat;
            SinChatLabel.Visible = !hayChat;
            if (!hayChat)
            {
                _mensajes.Clear();
                this.AcomodarListas();
                return;
            }

            // Si es alguien nuevo (vino de CONTACTAR), se agrega arriba de la lista mientras se manda el primer mensaje
            Conversacion actual = _conversaciones.Find(c => c.IdOtro == _idOtro);
            if (actual == null)
            {
                actual = this.BuscarPersona(_idOtro);
                if (actual == null) return;
                _conversaciones.Insert(0, actual);
                SinConversacionesLabel.Visible = false;
            }
            ContactoNombreLabel.Text = actual.Nombre;
            _fotoOtro = actual.Foto;
            ContactoFotoPanel.Invalidate();

            if (dbconn == null) dbconn = new DBConn();
            messageDA.SelectCommand.Connection = dbconn.dbConnection;
            messageDA.SelectCommand.Parameters.Clear();
            messageDA.SelectCommand.Parameters.AddWithValue("@idInvestor", this.IdInvestor());
            messageDA.SelectCommand.Parameters.AddWithValue("@idEntrepreneur", this.IdEntrepreneur());
            try
            {
                messagetable.Clear();
                messageDA.Fill(messagetable);
                this.ShowMensajes(messagetable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MensajeTextBox.Select();
        }

        // Nombre y foto de alguien con quien todavía no hay mensajes
        private Conversacion BuscarPersona(int idOtro)
        {
            string tabla = SoyInversor() ? "integrative.entrepreneur" : "integrative.investor";
            string columnaId = SoyInversor() ? "idEntrepreneur" : "idInvestor";
            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    MySqlCommand buscar = new MySqlCommand(
                        "SELECT name, lastName, imageProfile FROM " + tabla + " WHERE " + columnaId + " = @id", dbTemp.dbConnection);
                    buscar.Parameters.AddWithValue("@id", idOtro);
                    dbTemp.dbConnection.Open();
                    using (MySqlDataReader lector = buscar.ExecuteReader())
                    {
                        if (lector.Read() == false) return null;
                        Conversacion c = new Conversacion();
                        c.IdOtro = idOtro;
                        c.Nombre = (lector["name"].ToString() + " " + lector["lastName"].ToString()).Trim();
                        c.Foto = CargarImagen(lector["imageProfile"].ToString());
                        c.Ultimo = "Nueva conversación";
                        return c;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void ShowMensajes(DataTable tabla)
        {
            int miRol = SoyInversor() ? 0 : 1;
            _mensajes.Clear();
            foreach (DataRow fila in tabla.Rows)
            {
                Mensaje m = new Mensaje();
                m.Id = Convert.ToInt32(fila["idMessage"]);
                m.EsMio = Convert.ToInt32(fila["senderRole"]) == miRol;
                m.Texto = Convert.ToString(fila["content"]);
                m.Fecha = LeerFecha(fila["date"]);
                m.Editado = LeerFecha(fila["editedDate"]);
                _mensajes.Add(m);
            }
            this.AcomodarListas();
            ChatPanel.AutoScrollPosition = new Point(0, MensajesLista.Height);   // baja hasta el último mensaje
        }

        // ==================== FECHA Y HORA ====================
        private static DateTime? LeerFecha(object valor)
        {
            if (valor == null || valor == DBNull.Value) return null;
            DateTime fecha;
            if (valor is DateTime) return (DateTime)valor;
            if (DateTime.TryParse(Convert.ToString(valor), out fecha)) return fecha;
            return null;
        }

        // Debajo de cada mensaje: "Hoy, 13:05" · "Ayer, 09:10" · "05/11/2026, 18:30"
        private static string FechaCompleta(DateTime? fecha)
        {
            if (fecha == null) return "";
            DateTime f = fecha.Value;
            if (f.Date == DateTime.Today) return "Hoy, " + f.ToString("HH:mm");
            if (f.Date == DateTime.Today.AddDays(-1)) return "Ayer, " + f.ToString("HH:mm");
            return f.ToString("dd/MM/yyyy") + ", " + f.ToString("HH:mm");
        }

        // "Hoy, 13:05  ·  Editado hoy, 14:40" (si nunca se editó, solo la primera parte)
        private static string FechaDelMensaje(Mensaje m)
        {
            string texto = FechaCompleta(m.Fecha);
            if (m.Editado != null)
            {
                string editado = FechaCompleta(m.Editado);
                if (editado.StartsWith("Hoy") || editado.StartsWith("Ayer")) editado = editado.ToLower();
                texto += "  ·  Editado " + editado;
            }
            return texto;
        }

        // En la lista de conversaciones: "13:05" · "Ayer" · "05/11/26"
        private static string FechaCorta(DateTime? fecha)
        {
            if (fecha == null) return "";
            DateTime f = fecha.Value;
            if (f.Date == DateTime.Today) return f.ToString("HH:mm");
            if (f.Date == DateTime.Today.AddDays(-1)) return "Ayer";
            return f.ToString("dd/MM/yy");
        }

        // ==================== ENVIAR (INSERT) O GUARDAR UNA EDICIÓN (UPDATE) ====================
        private void EnviarBoton_Click(object sender, EventArgs e)
        {
            if (this.Validar_campos() == false)
            {
                return;
            }

            try
            {
                using (DBConn dbTemp = new DBConn())
                {
                    if (_idEditando == 0)
                    {
                        messageDA.InsertCommand.Connection = dbTemp.dbConnection;
                        messageDA.InsertCommand.Parameters.Clear();
                        messageDA.InsertCommand.Parameters.AddRange(new[] {
                            new MySqlParameter("@idInvestor", this.IdInvestor()),
                            new MySqlParameter("@idEntrepreneur", this.IdEntrepreneur()),
                            new MySqlParameter("@senderRole", SoyInversor() ? 0 : 1),
                            new MySqlParameter("@content", MensajeTextBox.Text.Trim())
                        });
                        dbTemp.dbConnection.Open();
                        messageDA.InsertCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        messageDA.UpdateCommand.Connection = dbTemp.dbConnection;
                        messageDA.UpdateCommand.Parameters.Clear();
                        messageDA.UpdateCommand.Parameters.AddRange(new[] {
                            new MySqlParameter("@content", MensajeTextBox.Text.Trim()),
                            new MySqlParameter("@idMessage", _idEditando)
                        });
                        dbTemp.dbConnection.Open();
                        messageDA.UpdateCommand.ExecuteNonQuery();
                    }
                }
                this.clearTextBoxes();
                this.LoadData();   // vuelve a leer la lista y los mensajes
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al enviar el mensaje", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== VALIDACIÓN Y LIMPIAR ====================
        private bool Validar_campos()
        {
            if (_idOtro == 0)
            {
                MessageBox.Show("Primero elige una conversación.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(MensajeTextBox.Text))
            {
                MensajeTextBox.Select();
                return false;   // no se manda un mensaje vacío
            }
            else
            {
                return true;
            }
        }

        private void clearTextBoxes()
        {
            this.MensajeTextBox.Text = "";
            this.CancelarEdicion();
        }

        // ==================== EDITAR UN MENSAJE PROPIO ====================
        private void EmpezarEdicion(Mensaje m)
        {
            _idEditando = m.Id;
            MensajeTextBox.Text = m.Texto;
            MensajeTextBox.SelectionStart = MensajeTextBox.Text.Length;
            EditandoLabel.Visible = true;
            MensajeTextBox.Select();
            MensajesLista.Invalidate();
        }

        private void CancelarEdicion()
        {
            if (_idEditando == 0) return;
            _idEditando = 0;
            MensajeTextBox.Text = "";
            EditandoLabel.Visible = false;
            MensajesLista.Invalidate();
        }

        // Enter = enviar, Esc = cancelar la edición
        private void MensajeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.EnviarBoton_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                this.CancelarEdicion();
            }
        }

        // Clic en una conversación de la lista
        private void ConversacionesLista_MouseClick(object sender, MouseEventArgs e)
        {
            int indice = (int)(e.Y / (AltoFila * Escala));
            if (indice < 0 || indice >= _conversaciones.Count) return;
            this.AbrirConversacion(_conversaciones[indice].IdOtro);
            ConversacionesLista.Invalidate();
        }

        // Clic derecho en un mensaje propio: Editar (no hay opción de eliminar)
        private void MensajesLista_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            Mensaje elegido = _mensajes.Find(m => m.EsMio && m.Burbuja.Contains(e.Location));
            if (elegido == null) return;

            _menuMensaje.Items.Clear();
            _menuMensaje.Items.Add("Editar mensaje", null, (s, a) => this.EmpezarEdicion(elegido));
            _menuMensaje.Show(MensajesLista, e.Location);
        }


        #region Diseño del Figma (no es necesario modificar esta parte)

        // Medidas del frame "Messages" (sin la barra de arriba, que está en HomePage)
        private const float AltoFila = 92f;          // cada conversación de la lista
        private const float AnchoBurbuja = 309f;
        private const float RellenoX = 25f, RellenoY = 20f;
        private const float AltoMinimoBurbuja = 64f;
        private const float EspacioEntreMensajes = 16f;
        private const float AltoHora = 24f;           // la fecha y hora debajo de cada burbuja
        private const float TamFotoChat = 52f;

        private static readonly Color GrisTitulo = Color.FromArgb(230, 230, 230);   // #E6E6E6
        private static readonly Color GrisBurbuja = Color.FromArgb(217, 217, 217);  // #D9D9D9
        private static readonly Color GrisCaja = Color.FromArgb(240, 243, 244);     // #F0F3F4
        private static readonly Color GrisLinea = Color.FromArgb(227, 226, 226);    // #E3E2E2
        private static readonly Color GrisHora = Color.FromArgb(130, 132, 142);

        private void PrepararDiseno()
        {
            AltoDiseno = 717f;
            _menuMensaje = new ContextMenuStrip();
            _menuMensaje.Font = new Font("Segoe UI", 10f);

            foreach (Control c in new Control[] { ConversacionesLista, MensajesLista, ContactoFotoPanel, EncabezadoPanel })
                ActivarDobleBuffer(c);

            FondoPanel.Paint += FondoPanel_Paint;
            ConversacionesLista.Paint += ConversacionesLista_Paint;
            MensajesLista.Paint += MensajesLista_Paint;
            ContactoFotoPanel.Paint += ContactoFotoPanel_Paint;
            EnviarBoton.Paint += EnviarBoton_Paint;
            EnviarBoton.MouseEnter += (s, e) => EnviarBoton.Invalidate();
            EnviarBoton.MouseLeave += (s, e) => EnviarBoton.Invalidate();
            MensajeTextBox.HandleCreated += (s, e) => PonerTextoGuia(MensajeTextBox);
            MensajeTextBox.GotFocus += (s, e) => FondoPanel.Invalidate();
            MensajeTextBox.LostFocus += (s, e) => FondoPanel.Invalidate();

            PrepararEscalado(FondoPanel);

            // Después de que la base escala todo, se acomodan la lista de conversaciones y las burbujas
            Resize += (s, e) => AcomodarListas();
            Load += (s, e) => AcomodarListas();
        }

        /// <summary>Calcula el alto de la lista y la posición de cada burbuja (según el tamaño de la ventana).</summary>
        private void AcomodarListas()
        {
            float k = Escala;

            // Si la ventana es más ancha o más alta que el diseño, la franja naranja llega hasta el borde derecho y hasta arriba
            int arriba = (int)Math.Round(MargenY);
            EncabezadoPanel.Bounds = new Rectangle(EncabezadoPanel.Left, 0, FondoPanel.ClientSize.Width - EncabezadoPanel.Left, arriba + (int)Math.Round(115 * k));
            ContactoFotoPanel.Top = arriba + (int)Math.Round(21 * k);     // la foto y el nombre se quedan en su lugar del Figma
            ContactoNombreLabel.Top = arriba + (int)Math.Round(24 * k);

            // La zona del chat ocupa todo lo que queda a la derecha y hacia abajo; la caja para escribir va pegada abajo
            int derecha = FondoPanel.ClientSize.Width, abajo = FondoPanel.ClientSize.Height;
            _caja = new RectangleF(ChatPanel.Left + 47 * k, abajo - 84 * k, derecha - ChatPanel.Left - 47 * k - 71 * k, 52 * k);
            ChatPanel.Bounds = new Rectangle(ChatPanel.Left, ChatPanel.Top, derecha - ChatPanel.Left, (int)(_caja.Y - 38 * k) - ChatPanel.Top);
            SinChatLabel.Bounds = ChatPanel.Bounds;
            EnviarBoton.Location = new Point((int)Math.Round(_caja.Right - 52 * k), (int)Math.Round(_caja.Y + 4 * k));
            MensajeTextBox.Bounds = new Rectangle((int)Math.Round(_caja.X + 30 * k), (int)Math.Round(_caja.Y + (_caja.Height - MensajeTextBox.Height) / 2f),
                (int)Math.Round(_caja.Width - 30 * k - 62 * k), MensajeTextBox.Height);
            EditandoLabel.Location = new Point((int)Math.Round(_caja.X + 23 * k), (int)Math.Round(_caja.Y - 32 * k));

            // "Mensajes" en 70 px; si en esta computadora la fuente es más ancha, se achica hasta que quepa
            float tam = 70;
            while (tam > 30 && TextRenderer.MeasureText(TituloLabel.Text, Fuente(tam, FontStyle.Bold)).Width > TituloLabel.Width - 16 * k) tam -= 2;
            TituloLabel.Font = Fuente(tam, FontStyle.Bold);

            // La columna de chats llega desde el borde izquierdo de la ventana hasta abajo (igual que el gris del título)
            ConversacionesPanel.Bounds = new Rectangle(0, ConversacionesPanel.Top,
                (int)Math.Round(MargenX + 398 * k), FondoPanel.ClientSize.Height - ConversacionesPanel.Top);

            // Conversaciones: una fila de 92 por cada una
            int altoLista = (int)Math.Ceiling(_conversaciones.Count * AltoFila * k);
            ConversacionesLista.Bounds = new Rectangle(0, ConversacionesPanel.AutoScrollPosition.Y, AnchoSinBarra(ConversacionesPanel, altoLista), altoLista);
            ConversacionesLista.Invalidate();

            // Mensajes: se mide cada burbuja; se acomodan de abajo hacia arriba como en un chat
            Font fuente = Fuente(15, FontStyle.Regular);
            float anchoTexto = (AnchoBurbuja - 2 * RellenoX) * k;
            List<float> altos = new List<float>();
            float total = EspacioEntreMensajes * k;
            foreach (Mensaje m in _mensajes)
            {
                Size medida = TextRenderer.MeasureText(m.Texto, fuente, new Size((int)anchoTexto, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                float alto = Math.Max(AltoMinimoBurbuja * k, medida.Height + 2 * RellenoY * k);
                altos.Add(alto);
                total += alto + (AltoHora + EspacioEntreMensajes) * k;
            }

            int altoChat = (int)Math.Max(total, ChatPanel.Height);
            float y = altoChat - total + EspacioEntreMensajes * k;   // si son pocos mensajes, quedan abajo
            int anchoChat = AnchoSinBarra(ChatPanel, altoChat);
            for (int i = 0; i < _mensajes.Count; i++)
            {
                Mensaje m = _mensajes[i];
                // Los míos pegados a la derecha (dejando lugar para mi foto); los del otro a la izquierda
                float x = m.EsMio ? anchoChat - 87 * k - AnchoBurbuja * k : 110 * k;
                m.Burbuja = new RectangleF(x, y, AnchoBurbuja * k, altos[i]);
                y += altos[i] + (AltoHora + EspacioEntreMensajes) * k;
            }
            MensajesLista.Bounds = new Rectangle(0, ChatPanel.AutoScrollPosition.Y, AnchoSinBarra(ChatPanel, altoChat), altoChat);
            MensajesLista.Invalidate();
        }

        // Ancho para la lista: si no cabe a lo alto, se deja lugar para la barra de desplazamiento (así no sale la horizontal)
        private static int AnchoSinBarra(Panel contenedor, int altoContenido)
        {
            bool conBarra = altoContenido > contenedor.Height;
            return contenedor.Width - (conBarra ? SystemInformation.VerticalScrollBarWidth : 0);
        }

        /// <summary>Fondo gris del título, línea divisoria y la caja de texto redondeada con borde azul.</summary>
        private void FondoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            float k = Escala, x0 = MargenX, y0 = MargenY;

            using (SolidBrush gris = new SolidBrush(GrisTitulo))
                g.FillRectangle(gris, 0, 0, x0 + 401 * k, y0 + 115 * k);   // desde el borde izquierdo (y de arriba) de la ventana

            using (GraphicsPath linea = Redondeado(new RectangleF(x0 + 398 * k, y0 + 100 * k, 3 * k, 617 * k), 1.5f * k))
            using (SolidBrush b = new SolidBrush(GrisLinea))
                g.FillPath(b, linea);

            RectangleF caja = _caja;
            Sombra(g, caja, caja.Height / 2f, k);
            using (GraphicsPath ruta = Redondeado(caja, caja.Height / 2f))
            {
                using (SolidBrush fondo = new SolidBrush(GrisCaja)) g.FillPath(fondo, ruta);
                using (Pen borde = new Pen(AzulPitcher, Math.Max(1f, (MensajeTextBox.Focused ? 2f : 1f) * k))) g.DrawPath(borde, ruta);
            }
        }

        /// <summary>Cada conversación: foto redonda, nombre en negritas y el último mensaje.</summary>
        private void ConversacionesLista_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            Font fNombre = Fuente(20, FontStyle.Bold);
            Font fTexto = Fuente(15, FontStyle.Regular);

            for (int i = 0; i < _conversaciones.Count; i++)
            {
                Conversacion c = _conversaciones[i];
                float y = i * AltoFila * k;
                if (c.IdOtro == _idOtro)
                    using (SolidBrush sel = new SolidBrush(GrisCaja)) g.FillRectangle(sel, 0, y, ConversacionesLista.Width, AltoFila * k);
                using (Pen linea = new Pen(Color.FromArgb(217, 217, 217), Math.Max(1f, k)))
                    g.DrawLine(linea, 0, y + AltoFila * k - 1, ConversacionesLista.Width, y + AltoFila * k - 1);

                // El contenido se recorre lo mismo que el margen, para quedar alineado con el título
                float ox = MargenX;
                float ancho = ConversacionesLista.Width;
                int altoNombre = TextRenderer.MeasureText("Ág", fNombre).Height;
                int altoTexto = TextRenderer.MeasureText("Ág", fTexto).Height * 2;   // dos renglones
                float yNombre = y + 8 * k;
                float yTexto = yNombre + altoNombre + 1 * k;

                DibujarFotoRedonda(g, c.Foto, new RectangleF(ox + 17 * k, y + 12 * k, 69 * k, 69 * k), c.Nombre, k);
                TextRenderer.DrawText(g, c.Nombre, fNombre, Rectangle.Round(new RectangleF(ox + 101 * k, yNombre, ancho - ox - 101 * k - 70 * k, altoNombre)), Color.Black,
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, FechaCorta(c.Fecha), Fuente(12, FontStyle.Regular), Rectangle.Round(new RectangleF(ancho - 72 * k, yNombre + 4 * k, 62 * k, altoNombre)), GrisHora,
                    TextFormatFlags.Right | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, c.Ultimo, fTexto, Rectangle.Round(new RectangleF(ox + 101 * k, yTexto, ancho - ox - 101 * k - 12 * k, Math.Min(altoTexto, y + AltoFila * k - 4 * k - yTexto))), Color.Black,
                    TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>Burbujas grises: las mías a la derecha con mi foto, las del otro a la izquierda con la suya.</summary>
        private void MensajesLista_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Color.White);
            float k = Escala;
            Font fuente = Fuente(15, FontStyle.Regular);

            foreach (Mensaje m in _mensajes)
            {
                RectangleF b = m.Burbuja;
                RectangleF conHora = new RectangleF(0, b.Y, MensajesLista.Width, b.Height + AltoHora * k);
                if (!e.ClipRectangle.IntersectsWith(Rectangle.Round(conHora))) continue;

                using (GraphicsPath ruta = Redondeado(b, 20 * k))
                {
                    using (SolidBrush fondo = new SolidBrush(GrisBurbuja)) g.FillPath(fondo, ruta);
                    if (m.Id == _idEditando)
                        using (Pen borde = new Pen(AzulPitcher, 2 * k)) g.DrawPath(borde, ruta);
                }
                RectangleF texto = new RectangleF(b.X + RellenoX * k, b.Y + RellenoY * k, b.Width - 2 * RellenoX * k, b.Height - 2 * RellenoY * k);
                TextRenderer.DrawText(g, m.Texto, fuente, Rectangle.Round(texto), Color.Black,
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter);

                float xFoto = m.EsMio ? MensajesLista.Width - 71 * k : 25 * k;
                RectangleF foto = new RectangleF(xFoto, b.Y + (b.Height - TamFotoChat * k) / 2f, TamFotoChat * k, TamFotoChat * k);
                DibujarFotoRedonda(g, m.EsMio ? _miFoto : _fotoOtro, foto, m.EsMio ? Sesion.Usuario : ContactoNombreLabel.Text, k);

                // Fecha y hora debajo de la burbuja (a la derecha en los míos, a la izquierda en los del otro)
                float anchoHora = 460 * k;   // más ancho que la burbuja por si dice "Editado ..."
                RectangleF hora = m.EsMio
                    ? new RectangleF(b.Right - 8 * k - anchoHora, b.Bottom + 4 * k, anchoHora, (AltoHora - 4) * k)
                    : new RectangleF(b.X + 8 * k, b.Bottom + 4 * k, anchoHora, (AltoHora - 4) * k);
                TextRenderer.DrawText(g, FechaDelMensaje(m), Fuente(12, FontStyle.Regular), Rectangle.Round(hora), GrisHora,
                    (m.EsMio ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        private void ContactoFotoPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(Naranja);
            DibujarFotoRedonda(g, _fotoOtro, new RectangleF(0, 0, ContactoFotoPanel.Width - 1, ContactoFotoPanel.Height - 1), ContactoNombreLabel.Text, Escala);
        }

        /// <summary>Botón circular azul con el ícono de enviar del Figma.</summary>
        private void EnviarBoton_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Calidad(g);
            g.Clear(GrisCaja);
            float k = Escala;
            bool encima = EnviarBoton.ClientRectangle.Contains(EnviarBoton.PointToClient(Cursor.Position));
            Color color = !EnviarBoton.Enabled ? Color.FromArgb(160, 170, 185) : encima ? ControlPaint.Light(AzulPitcher, 0.15f) : AzulPitcher;
            using (SolidBrush b = new SolidBrush(color))
                g.FillEllipse(b, 0, 0, EnviarBoton.Width - 1, EnviarBoton.Height - 1);
            if (EnviarBoton.Image != null)
                g.DrawImage(EnviarBoton.Image, 11 * k, 8 * k, 30 * k, 30 * k);
        }

        #endregion
    }
}

namespace PITCHR
{
    partial class HomePage
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomePage));
            this.EmprendimientosGuestLabel = new System.Windows.Forms.Label();
            this.EventosGuestLabel = new System.Windows.Forms.Label();
            this.NoticiasGuestLabel = new System.Windows.Forms.Label();
            this.MenuGuestPanel = new System.Windows.Forms.Panel();
            this.IniciarSesionBoton = new System.Windows.Forms.Button();
            this.ZonaGuestPanel = new System.Windows.Forms.Panel();
            this.EmprendimientosUserLabel = new System.Windows.Forms.Label();
            this.EventosUserLabel = new System.Windows.Forms.Label();
            this.NoticiasUserLabel = new System.Windows.Forms.Label();
            this.MensajesUserLabel = new System.Windows.Forms.Label();
            this.MenuUserPanel = new System.Windows.Forms.Panel();
            this.PerfilPictureBox = new System.Windows.Forms.PictureBox();
            this.ZonaUserPanel = new System.Windows.Forms.Panel();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.BarraPanel = new System.Windows.Forms.Panel();
            this.ContenidoPanel = new System.Windows.Forms.Panel();
            this.MenuGuestPanel.SuspendLayout();
            this.ZonaGuestPanel.SuspendLayout();
            this.MenuUserPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PerfilPictureBox)).BeginInit();
            this.ZonaUserPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            this.BarraPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // EmprendimientosGuestLabel
            // 
            this.EmprendimientosGuestLabel.BackColor = System.Drawing.Color.Transparent;
            this.EmprendimientosGuestLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EmprendimientosGuestLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.EmprendimientosGuestLabel.ForeColor = System.Drawing.Color.Black;
            this.EmprendimientosGuestLabel.Location = new System.Drawing.Point(8, 0);
            this.EmprendimientosGuestLabel.Name = "EmprendimientosGuestLabel";
            this.EmprendimientosGuestLabel.Size = new System.Drawing.Size(146, 52);
            this.EmprendimientosGuestLabel.TabIndex = 0;
            this.EmprendimientosGuestLabel.Tag = "Emprendimientos";
            this.EmprendimientosGuestLabel.Text = "Emprendimientos";
            this.EmprendimientosGuestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EmprendimientosGuestLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // EventosGuestLabel
            // 
            this.EventosGuestLabel.BackColor = System.Drawing.Color.Transparent;
            this.EventosGuestLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EventosGuestLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.EventosGuestLabel.ForeColor = System.Drawing.Color.Black;
            this.EventosGuestLabel.Location = new System.Drawing.Point(164, 0);
            this.EventosGuestLabel.Name = "EventosGuestLabel";
            this.EventosGuestLabel.Size = new System.Drawing.Size(84, 52);
            this.EventosGuestLabel.TabIndex = 1;
            this.EventosGuestLabel.Tag = "Eventos";
            this.EventosGuestLabel.Text = "Eventos";
            this.EventosGuestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EventosGuestLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // NoticiasGuestLabel
            // 
            this.NoticiasGuestLabel.BackColor = System.Drawing.Color.Transparent;
            this.NoticiasGuestLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NoticiasGuestLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.NoticiasGuestLabel.ForeColor = System.Drawing.Color.Black;
            this.NoticiasGuestLabel.Location = new System.Drawing.Point(254, 0);
            this.NoticiasGuestLabel.Name = "NoticiasGuestLabel";
            this.NoticiasGuestLabel.Size = new System.Drawing.Size(86, 52);
            this.NoticiasGuestLabel.TabIndex = 2;
            this.NoticiasGuestLabel.Tag = "Noticias";
            this.NoticiasGuestLabel.Text = "Noticias";
            this.NoticiasGuestLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NoticiasGuestLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuGuestPanel
            // 
            this.MenuGuestPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.MenuGuestPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MenuGuestPanel.BackgroundImage")));
            this.MenuGuestPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MenuGuestPanel.Controls.Add(this.NoticiasGuestLabel);
            this.MenuGuestPanel.Controls.Add(this.EventosGuestLabel);
            this.MenuGuestPanel.Controls.Add(this.EmprendimientosGuestLabel);
            this.MenuGuestPanel.Location = new System.Drawing.Point(24, 9);
            this.MenuGuestPanel.Name = "MenuGuestPanel";
            this.MenuGuestPanel.Size = new System.Drawing.Size(354, 60);
            this.MenuGuestPanel.TabIndex = 0;
            // 
            // IniciarSesionBoton
            // 
            this.IniciarSesionBoton.BackColor = System.Drawing.Color.Transparent;
            this.IniciarSesionBoton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("IniciarSesionBoton.BackgroundImage")));
            this.IniciarSesionBoton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.IniciarSesionBoton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.IniciarSesionBoton.FlatAppearance.BorderSize = 0;
            this.IniciarSesionBoton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.IniciarSesionBoton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.IniciarSesionBoton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IniciarSesionBoton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.IniciarSesionBoton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.IniciarSesionBoton.Location = new System.Drawing.Point(400, 9);
            this.IniciarSesionBoton.Name = "IniciarSesionBoton";
            this.IniciarSesionBoton.Size = new System.Drawing.Size(120, 60);
            this.IniciarSesionBoton.TabIndex = 1;
            this.IniciarSesionBoton.Text = "Iniciar Sesión";
            this.IniciarSesionBoton.UseVisualStyleBackColor = false;
            this.IniciarSesionBoton.Click += new System.EventHandler(this.IniciarSesionBoton_Click);
            // 
            // ZonaGuestPanel
            // 
            this.ZonaGuestPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.ZonaGuestPanel.Controls.Add(this.IniciarSesionBoton);
            this.ZonaGuestPanel.Controls.Add(this.MenuGuestPanel);
            this.ZonaGuestPanel.Location = new System.Drawing.Point(700, 30);
            this.ZonaGuestPanel.Name = "ZonaGuestPanel";
            this.ZonaGuestPanel.Size = new System.Drawing.Size(540, 80);
            this.ZonaGuestPanel.TabIndex = 1;
            // 
            // EmprendimientosUserLabel
            // 
            this.EmprendimientosUserLabel.BackColor = System.Drawing.Color.Transparent;
            this.EmprendimientosUserLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EmprendimientosUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.EmprendimientosUserLabel.ForeColor = System.Drawing.Color.Black;
            this.EmprendimientosUserLabel.Location = new System.Drawing.Point(16, 0);
            this.EmprendimientosUserLabel.Name = "EmprendimientosUserLabel";
            this.EmprendimientosUserLabel.Size = new System.Drawing.Size(158, 52);
            this.EmprendimientosUserLabel.TabIndex = 0;
            this.EmprendimientosUserLabel.Tag = "Emprendimientos";
            this.EmprendimientosUserLabel.Text = "Emprendimientos";
            this.EmprendimientosUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EmprendimientosUserLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // EventosUserLabel
            // 
            this.EventosUserLabel.BackColor = System.Drawing.Color.Transparent;
            this.EventosUserLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EventosUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.EventosUserLabel.ForeColor = System.Drawing.Color.Black;
            this.EventosUserLabel.Location = new System.Drawing.Point(177, 0);
            this.EventosUserLabel.Name = "EventosUserLabel";
            this.EventosUserLabel.Size = new System.Drawing.Size(80, 52);
            this.EventosUserLabel.TabIndex = 1;
            this.EventosUserLabel.Tag = "Eventos";
            this.EventosUserLabel.Text = "Eventos";
            this.EventosUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.EventosUserLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // NoticiasUserLabel
            // 
            this.NoticiasUserLabel.BackColor = System.Drawing.Color.Transparent;
            this.NoticiasUserLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NoticiasUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.NoticiasUserLabel.ForeColor = System.Drawing.Color.Black;
            this.NoticiasUserLabel.Location = new System.Drawing.Point(256, 0);
            this.NoticiasUserLabel.Name = "NoticiasUserLabel";
            this.NoticiasUserLabel.Size = new System.Drawing.Size(82, 52);
            this.NoticiasUserLabel.TabIndex = 2;
            this.NoticiasUserLabel.Tag = "Noticias";
            this.NoticiasUserLabel.Text = "Noticias";
            this.NoticiasUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NoticiasUserLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MensajesUserLabel
            // 
            this.MensajesUserLabel.BackColor = System.Drawing.Color.Transparent;
            this.MensajesUserLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MensajesUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.MensajesUserLabel.ForeColor = System.Drawing.Color.Black;
            this.MensajesUserLabel.Location = new System.Drawing.Point(340, 0);
            this.MensajesUserLabel.Name = "MensajesUserLabel";
            this.MensajesUserLabel.Size = new System.Drawing.Size(86, 52);
            this.MensajesUserLabel.TabIndex = 3;
            this.MensajesUserLabel.Tag = "Mensajes";
            this.MensajesUserLabel.Text = "Mensajes";
            this.MensajesUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.MensajesUserLabel.Click += new System.EventHandler(this.Menu_Click);
            // 
            // MenuUserPanel
            // 
            this.MenuUserPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.MenuUserPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MenuUserPanel.BackgroundImage")));
            this.MenuUserPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.MenuUserPanel.Controls.Add(this.MensajesUserLabel);
            this.MenuUserPanel.Controls.Add(this.NoticiasUserLabel);
            this.MenuUserPanel.Controls.Add(this.EventosUserLabel);
            this.MenuUserPanel.Controls.Add(this.EmprendimientosUserLabel);
            this.MenuUserPanel.Location = new System.Drawing.Point(25, 9);
            this.MenuUserPanel.Name = "MenuUserPanel";
            this.MenuUserPanel.Size = new System.Drawing.Size(461, 60);
            this.MenuUserPanel.TabIndex = 0;
            // 
            // PerfilPictureBox
            // 
            this.PerfilPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.PerfilPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PerfilPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("PerfilPictureBox.Image")));
            this.PerfilPictureBox.Location = new System.Drawing.Point(505, 9);
            this.PerfilPictureBox.Name = "PerfilPictureBox";
            this.PerfilPictureBox.Size = new System.Drawing.Size(52, 56);
            this.PerfilPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PerfilPictureBox.TabIndex = 1;
            this.PerfilPictureBox.TabStop = false;
            this.PerfilPictureBox.Click += new System.EventHandler(this.PerfilPictureBox_Click);
            // 
            // ZonaUserPanel
            // 
            this.ZonaUserPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.ZonaUserPanel.Controls.Add(this.PerfilPictureBox);
            this.ZonaUserPanel.Controls.Add(this.MenuUserPanel);
            this.ZonaUserPanel.Location = new System.Drawing.Point(640, 30);
            this.ZonaUserPanel.Name = "ZonaUserPanel";
            this.ZonaUserPanel.Size = new System.Drawing.Size(600, 80);
            this.ZonaUserPanel.TabIndex = 2;
            this.ZonaUserPanel.Visible = false;
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.LogoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("LogoPictureBox.Image")));
            this.LogoPictureBox.Location = new System.Drawing.Point(70, 47);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(200, 60);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.LogoPictureBox.TabIndex = 0;
            this.LogoPictureBox.TabStop = false;
            this.LogoPictureBox.Click += new System.EventHandler(this.LogoPictureBox_Click);
            // 
            // BarraPanel
            // 
            this.BarraPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.BarraPanel.Controls.Add(this.ZonaUserPanel);
            this.BarraPanel.Controls.Add(this.ZonaGuestPanel);
            this.BarraPanel.Controls.Add(this.LogoPictureBox);
            this.BarraPanel.Location = new System.Drawing.Point(0, 0);
            this.BarraPanel.Name = "BarraPanel";
            this.BarraPanel.Size = new System.Drawing.Size(1280, 131);
            this.BarraPanel.TabIndex = 0;
            // 
            // ContenidoPanel
            // 
            this.ContenidoPanel.BackColor = System.Drawing.Color.White;
            this.ContenidoPanel.Location = new System.Drawing.Point(0, 131);
            this.ContenidoPanel.Name = "ContenidoPanel";
            this.ContenidoPanel.Size = new System.Drawing.Size(1280, 701);
            this.ContenidoPanel.TabIndex = 1;
            // 
            // HomePage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1280, 832);
            this.Controls.Add(this.ContenidoPanel);
            this.Controls.Add(this.BarraPanel);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "HomePage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PitchR";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.HomePage_Load);
            this.MenuGuestPanel.ResumeLayout(false);
            this.ZonaGuestPanel.ResumeLayout(false);
            this.MenuUserPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PerfilPictureBox)).EndInit();
            this.ZonaUserPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            this.BarraPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label EmprendimientosGuestLabel;
        private System.Windows.Forms.Label EventosGuestLabel;
        private System.Windows.Forms.Label NoticiasGuestLabel;
        private System.Windows.Forms.Panel MenuGuestPanel;
        private System.Windows.Forms.Button IniciarSesionBoton;
        private System.Windows.Forms.Panel ZonaGuestPanel;
        private System.Windows.Forms.Label EmprendimientosUserLabel;
        private System.Windows.Forms.Label EventosUserLabel;
        private System.Windows.Forms.Label NoticiasUserLabel;
        private System.Windows.Forms.Label MensajesUserLabel;
        private System.Windows.Forms.Panel MenuUserPanel;
        private System.Windows.Forms.PictureBox PerfilPictureBox;
        private System.Windows.Forms.Panel ZonaUserPanel;
        private System.Windows.Forms.PictureBox LogoPictureBox;
        private System.Windows.Forms.Panel BarraPanel;
        private System.Windows.Forms.Panel ContenidoPanel;
    }
}

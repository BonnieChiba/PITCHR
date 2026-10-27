using System.Drawing;
using System.Windows.Forms;

namespace PITCHR
{
    partial class CrearCuentaInversor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrearCuentaInversor));
            this.CuadroBlancoTexto = new System.Windows.Forms.Panel();
            this.TerminosLink = new System.Windows.Forms.LinkLabel();
            this.RFCInversorTextbox = new System.Windows.Forms.MaskedTextBox();
            this.RegistrarseBoton = new System.Windows.Forms.Button();
            this.TerminosCheckbox = new System.Windows.Forms.CheckBox();
            this.NombreTextbox = new System.Windows.Forms.TextBox();
            this.ApellidoTextbox = new System.Windows.Forms.TextBox();
            this.EmailTextbox = new System.Windows.Forms.TextBox();
            this.ConfirmarContraseñaTextbox = new System.Windows.Forms.TextBox();
            this.ContraseñaTextBox = new System.Windows.Forms.TextBox();
            this.UsuarioTexto = new System.Windows.Forms.TextBox();
            this.InversorTexto = new System.Windows.Forms.Label();
            this.CrearCuentaLabel = new System.Windows.Forms.Label();
            this.InvierteText = new System.Windows.Forms.Label();
            this.ChihuahuaText = new System.Windows.Forms.Label();
            this.BackgroundInversor = new System.Windows.Forms.Panel();
            this.RegresarPictureBox = new System.Windows.Forms.PictureBox();
            this.LineaSuperiorPicturebox = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.CuadroBlancoTexto.SuspendLayout();
            this.BackgroundInversor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RegresarPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LineaSuperiorPicturebox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // CuadroBlancoTexto
            // 
            this.CuadroBlancoTexto.BackColor = System.Drawing.Color.Transparent;
            this.CuadroBlancoTexto.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CuadroBlancoTexto.BackgroundImage")));
            this.CuadroBlancoTexto.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CuadroBlancoTexto.Controls.Add(this.TerminosLink);
            this.CuadroBlancoTexto.Controls.Add(this.RFCInversorTextbox);
            this.CuadroBlancoTexto.Controls.Add(this.RegistrarseBoton);
            this.CuadroBlancoTexto.Controls.Add(this.TerminosCheckbox);
            this.CuadroBlancoTexto.Controls.Add(this.NombreTextbox);
            this.CuadroBlancoTexto.Controls.Add(this.ApellidoTextbox);
            this.CuadroBlancoTexto.Controls.Add(this.EmailTextbox);
            this.CuadroBlancoTexto.Controls.Add(this.ConfirmarContraseñaTextbox);
            this.CuadroBlancoTexto.Controls.Add(this.ContraseñaTextBox);
            this.CuadroBlancoTexto.Controls.Add(this.UsuarioTexto);
            this.CuadroBlancoTexto.Controls.Add(this.InversorTexto);
            this.CuadroBlancoTexto.Controls.Add(this.CrearCuentaLabel);
            this.CuadroBlancoTexto.Location = new System.Drawing.Point(80, 84);
            this.CuadroBlancoTexto.Name = "CuadroBlancoTexto";
            this.CuadroBlancoTexto.Size = new System.Drawing.Size(578, 674);
            this.CuadroBlancoTexto.TabIndex = 0;
            // 
            // TerminosLink
            // 
            this.TerminosLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.TerminosLink.AutoSize = true;
            this.TerminosLink.BackColor = System.Drawing.Color.Transparent;
            this.TerminosLink.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TerminosLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.TerminosLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(153)))), ((int)(((byte)(247)))));
            this.TerminosLink.Location = new System.Drawing.Point(197, 526);
            this.TerminosLink.Name = "TerminosLink";
            this.TerminosLink.Size = new System.Drawing.Size(197, 21);
            this.TerminosLink.TabIndex = 10;
            this.TerminosLink.TabStop = true;
            this.TerminosLink.Text = "ver Terminos y condiciones";
            // 
            // RFCInversorTextbox
            // 
            this.RFCInversorTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RFCInversorTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.RFCInversorTextbox.Location = new System.Drawing.Point(99, 463);
            this.RFCInversorTextbox.Name = "RFCInversorTextbox";
            this.RFCInversorTextbox.Size = new System.Drawing.Size(363, 23);
            this.RFCInversorTextbox.TabIndex = 8;
            this.RFCInversorTextbox.Tag = "RFC";
            // 
            // RegistrarseBoton
            // 
            this.RegistrarseBoton.BackColor = System.Drawing.Color.Transparent;
            this.RegistrarseBoton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RegistrarseBoton.BackgroundImage")));
            this.RegistrarseBoton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.RegistrarseBoton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RegistrarseBoton.FlatAppearance.BorderSize = 0;
            this.RegistrarseBoton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.RegistrarseBoton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.RegistrarseBoton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RegistrarseBoton.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.RegistrarseBoton.ForeColor = System.Drawing.Color.White;
            this.RegistrarseBoton.Location = new System.Drawing.Point(146, 563);
            this.RegistrarseBoton.Name = "RegistrarseBoton";
            this.RegistrarseBoton.Size = new System.Drawing.Size(272, 56);
            this.RegistrarseBoton.TabIndex = 11;
            this.RegistrarseBoton.Text = "Registrarme";
            this.RegistrarseBoton.UseVisualStyleBackColor = false;
            this.RegistrarseBoton.Click += new System.EventHandler(this.RegistrarseBoton_Click);
            // 
            // TerminosCheckbox
            // 
            this.TerminosCheckbox.BackColor = System.Drawing.Color.White;
            this.TerminosCheckbox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TerminosCheckbox.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TerminosCheckbox.Location = new System.Drawing.Point(174, 502);
            this.TerminosCheckbox.Name = "TerminosCheckbox";
            this.TerminosCheckbox.Size = new System.Drawing.Size(230, 36);
            this.TerminosCheckbox.TabIndex = 9;
            this.TerminosCheckbox.Text = "Terminos y Condiciones,";
            this.TerminosCheckbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.TerminosCheckbox.UseVisualStyleBackColor = false;
            this.TerminosCheckbox.CheckedChanged += new System.EventHandler(this.TerminosCheckbox_CheckedChanged_1);
            // 
            // NombreTextbox
            // 
            this.NombreTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.NombreTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.NombreTextbox.Location = new System.Drawing.Point(99, 373);
            this.NombreTextbox.Name = "NombreTextbox";
            this.NombreTextbox.Size = new System.Drawing.Size(363, 23);
            this.NombreTextbox.TabIndex = 6;
            this.NombreTextbox.Tag = "Nombre";
            // 
            // ApellidoTextbox
            // 
            this.ApellidoTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ApellidoTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ApellidoTextbox.Location = new System.Drawing.Point(99, 418);
            this.ApellidoTextbox.Name = "ApellidoTextbox";
            this.ApellidoTextbox.Size = new System.Drawing.Size(363, 23);
            this.ApellidoTextbox.TabIndex = 7;
            this.ApellidoTextbox.Tag = "Apellido";
            // 
            // EmailTextbox
            // 
            this.EmailTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.EmailTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.EmailTextbox.Location = new System.Drawing.Point(99, 327);
            this.EmailTextbox.Name = "EmailTextbox";
            this.EmailTextbox.Size = new System.Drawing.Size(363, 23);
            this.EmailTextbox.TabIndex = 5;
            this.EmailTextbox.Tag = "Email";
            // 
            // ConfirmarContraseñaTextbox
            // 
            this.ConfirmarContraseñaTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ConfirmarContraseñaTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ConfirmarContraseñaTextbox.Location = new System.Drawing.Point(99, 282);
            this.ConfirmarContraseñaTextbox.Name = "ConfirmarContraseñaTextbox";
            this.ConfirmarContraseñaTextbox.Size = new System.Drawing.Size(363, 23);
            this.ConfirmarContraseñaTextbox.TabIndex = 4;
            this.ConfirmarContraseñaTextbox.Tag = "Confirmar contraseña";
            this.ConfirmarContraseñaTextbox.UseSystemPasswordChar = true;
            // 
            // ContraseñaTextBox
            // 
            this.ContraseñaTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ContraseñaTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ContraseñaTextBox.Location = new System.Drawing.Point(99, 237);
            this.ContraseñaTextBox.Name = "ContraseñaTextBox";
            this.ContraseñaTextBox.Size = new System.Drawing.Size(363, 23);
            this.ContraseñaTextBox.TabIndex = 3;
            this.ContraseñaTextBox.Tag = "Contraseña";
            this.ContraseñaTextBox.UseSystemPasswordChar = true;
            this.ContraseñaTextBox.TextChanged += new System.EventHandler(this.ContraseñaTextBox_TextChanged);
            // 
            // UsuarioTexto
            // 
            this.UsuarioTexto.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UsuarioTexto.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.UsuarioTexto.Location = new System.Drawing.Point(99, 192);
            this.UsuarioTexto.Name = "UsuarioTexto";
            this.UsuarioTexto.Size = new System.Drawing.Size(363, 23);
            this.UsuarioTexto.TabIndex = 2;
            this.UsuarioTexto.Tag = "Usuario";
            // 
            // InversorTexto
            // 
            this.InversorTexto.BackColor = System.Drawing.Color.Transparent;
            this.InversorTexto.Font = new System.Drawing.Font("Microsoft Sans Serif", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.InversorTexto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(189)))), ((int)(((byte)(248)))));
            this.InversorTexto.Location = new System.Drawing.Point(118, 117);
            this.InversorTexto.Name = "InversorTexto";
            this.InversorTexto.Size = new System.Drawing.Size(300, 53);
            this.InversorTexto.TabIndex = 1;
            this.InversorTexto.Text = "Inversor";
            this.InversorTexto.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CrearCuentaLabel
            // 
            this.CrearCuentaLabel.BackColor = System.Drawing.Color.Transparent;
            this.CrearCuentaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.CrearCuentaLabel.ForeColor = System.Drawing.Color.Black;
            this.CrearCuentaLabel.Location = new System.Drawing.Point(38, 51);
            this.CrearCuentaLabel.Name = "CrearCuentaLabel";
            this.CrearCuentaLabel.Size = new System.Drawing.Size(500, 66);
            this.CrearCuentaLabel.TabIndex = 0;
            this.CrearCuentaLabel.Text = "Crear Cuenta";
            this.CrearCuentaLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // InvierteText
            // 
            this.InvierteText.BackColor = System.Drawing.Color.Transparent;
            this.InvierteText.Font = new System.Drawing.Font("Microsoft Sans Serif", 75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.InvierteText.ForeColor = System.Drawing.Color.White;
            this.InvierteText.Location = new System.Drawing.Point(718, 251);
            this.InvierteText.Name = "InvierteText";
            this.InvierteText.Size = new System.Drawing.Size(520, 290);
            this.InvierteText.TabIndex = 1;
            this.InvierteText.Text = "Invierte\r\nComparte\r\nExpande";
            // 
            // ChihuahuaText
            // 
            this.ChihuahuaText.BackColor = System.Drawing.Color.Transparent;
            this.ChihuahuaText.Font = new System.Drawing.Font("Ink Free", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.ChihuahuaText.ForeColor = System.Drawing.Color.White;
            this.ChihuahuaText.Location = new System.Drawing.Point(723, 538);
            this.ChihuahuaText.Name = "ChihuahuaText";
            this.ChihuahuaText.Size = new System.Drawing.Size(420, 48);
            this.ChihuahuaText.TabIndex = 2;
            this.ChihuahuaText.Text = "Chihuahua, México";
            // 
            // BackgroundInversor
            // 
            this.BackgroundInversor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.BackgroundInversor.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BackgroundInversor.BackgroundImage")));
            this.BackgroundInversor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.BackgroundInversor.Controls.Add(this.RegresarPictureBox);
            this.BackgroundInversor.Controls.Add(this.ChihuahuaText);
            this.BackgroundInversor.Controls.Add(this.InvierteText);
            this.BackgroundInversor.Controls.Add(this.CuadroBlancoTexto);
            this.BackgroundInversor.Controls.Add(this.LineaSuperiorPicturebox);
            this.BackgroundInversor.Controls.Add(this.pictureBox1);
            this.BackgroundInversor.Location = new System.Drawing.Point(0, 0);
            this.BackgroundInversor.Name = "BackgroundInversor";
            this.BackgroundInversor.Size = new System.Drawing.Size(1280, 832);
            this.BackgroundInversor.TabIndex = 0;
            // 
            // RegresarPictureBox
            // 
            this.RegresarPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.RegresarPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RegresarPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("RegresarPictureBox.Image")));
            this.RegresarPictureBox.Location = new System.Drawing.Point(37, 36);
            this.RegresarPictureBox.Name = "RegresarPictureBox";
            this.RegresarPictureBox.Size = new System.Drawing.Size(52, 52);
            this.RegresarPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.RegresarPictureBox.TabIndex = 4;
            this.RegresarPictureBox.TabStop = false;
            this.RegresarPictureBox.Click += new System.EventHandler(this.RegresarPictureBox_Click);
            // 
            // LineaSuperiorPicturebox
            // 
            this.LineaSuperiorPicturebox.BackColor = System.Drawing.Color.Transparent;
            this.LineaSuperiorPicturebox.Image = ((System.Drawing.Image)(resources.GetObject("LineaSuperiorPicturebox.Image")));
            this.LineaSuperiorPicturebox.Location = new System.Drawing.Point(0, 0);
            this.LineaSuperiorPicturebox.Name = "LineaSuperiorPicturebox";
            this.LineaSuperiorPicturebox.Size = new System.Drawing.Size(1280, 254);
            this.LineaSuperiorPicturebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LineaSuperiorPicturebox.TabIndex = 1;
            this.LineaSuperiorPicturebox.TabStop = false;
            this.LineaSuperiorPicturebox.Click += new System.EventHandler(this.LineaSuperiorPicturebox_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 586);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1280, 246);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // CrearCuentaInversor
            // 
            this.AcceptButton = this.RegistrarseBoton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1280, 832);
            this.Controls.Add(this.BackgroundInversor);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "CrearCuentaInversor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pitcher - Crear cuenta";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.CuadroBlancoTexto.ResumeLayout(false);
            this.CuadroBlancoTexto.PerformLayout();
            this.BackgroundInversor.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RegresarPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LineaSuperiorPicturebox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel CuadroBlancoTexto;
        private PictureBox LineaSuperiorPicturebox;
        private Button RegistrarseBoton;
        private LinkLabel TerminosLink;
        private CheckBox TerminosCheckbox;
        private TextBox NombreTextbox;
        private TextBox ApellidoTextbox;
        private TextBox EmailTextbox;
        private TextBox ConfirmarContraseñaTextbox;
        private TextBox ContraseñaTextBox;
        private TextBox UsuarioTexto;
        private Label InversorTexto;
        private Label CrearCuentaLabel;
        private Label InvierteText;
        private Label ChihuahuaText;
        private Panel BackgroundInversor;
        private PictureBox pictureBox1;
        private MaskedTextBox RFCInversorTextbox;
        private ImageList imageList1;
        private PictureBox RegresarPictureBox;
    }
}

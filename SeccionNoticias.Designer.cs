namespace PITCHR
{
    partial class SeccionNoticias
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeccionNoticias));
            this.TituloLabel = new System.Windows.Forms.Label();
            this.NoticiaDestacada = new PITCHR.TarjetaNoticia();
            this.Noticia1 = new PITCHR.TarjetaNoticia();
            this.Noticia2 = new PITCHR.TarjetaNoticia();
            this.Noticia3 = new PITCHR.TarjetaNoticia();
            this.BackgroundNoticias = new System.Windows.Forms.Panel();
            this.BackgroundNoticias.SuspendLayout();

            this.SuspendLayout();
            // 
            // TituloLabel
            // 
            this.TituloLabel.BackColor = System.Drawing.Color.Transparent;
            this.TituloLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 70F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.TituloLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.TituloLabel.Location = new System.Drawing.Point(62, 4);
            this.TituloLabel.Name = "TituloLabel";
            this.TituloLabel.Size = new System.Drawing.Size(520, 100);
            this.TituloLabel.TabIndex = 3;
            this.TituloLabel.Text = "Noticias";
            this.TituloLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NoticiaDestacada
            // 
            this.NoticiaDestacada.BackColor = System.Drawing.Color.White;
            this.NoticiaDestacada.Categoria = "Destacada";
            this.NoticiaDestacada.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NoticiaDestacada.Fecha = "15 JUN 2025 · 6 min";
            this.NoticiaDestacada.Forma = PITCHR.TarjetaNoticia.Formas.Destacada;
            this.NoticiaDestacada.Imagen = ((System.Drawing.Image)(resources.GetObject("NoticiaDestacada.Imagen")));
            this.NoticiaDestacada.Location = new System.Drawing.Point(67, 126);
            this.NoticiaDestacada.Name = "NoticiaDestacada";
            this.NoticiaDestacada.Size = new System.Drawing.Size(632, 490);
            this.NoticiaDestacada.TabIndex = 4;
            this.NoticiaDestacada.Resumen = "Emprendedores de distintas industrias comparten cómo una red puede cambiar el rumbo de un proyecto.";
            this.NoticiaDestacada.Titulo = "El poder de encontrarse: la comunidad que impulsa nuevas ideas";
            this.NoticiaDestacada.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia1
            // 
            this.Noticia1.BackColor = System.Drawing.Color.White;
            this.Noticia1.Categoria = "Emprendimientos";
            this.Noticia1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia1.Fecha = "12 JUN 2025";
            this.Noticia1.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia1.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia1.Imagen")));
            this.Noticia1.Location = new System.Drawing.Point(719, 125);
            this.Noticia1.Name = "Noticia1";
            this.Noticia1.Size = new System.Drawing.Size(480, 150);
            this.Noticia1.TabIndex = 5;
            this.Noticia1.Resumen = "";
            this.Noticia1.Titulo = "El diseño local encuentra nuevas formas de crecer";
            this.Noticia1.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia2
            // 
            this.Noticia2.BackColor = System.Drawing.Color.White;
            this.Noticia2.Categoria = "Comunidad";
            this.Noticia2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia2.Fecha = "08 JUN 2025";
            this.Noticia2.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia2.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia2.Imagen")));
            this.Noticia2.Location = new System.Drawing.Point(719, 295);
            this.Noticia2.Name = "Noticia2";
            this.Noticia2.Size = new System.Drawing.Size(480, 150);
            this.Noticia2.TabIndex = 6;
            this.Noticia2.Resumen = "";
            this.Noticia2.Titulo = "Ferias que conectan talento, historias y comunidad";
            this.Noticia2.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia3
            // 
            this.Noticia3.BackColor = System.Drawing.Color.White;
            this.Noticia3.Categoria = "Innovación";
            this.Noticia3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia3.Fecha = "03 JUN 2025";
            this.Noticia3.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia3.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia3.Imagen")));
            this.Noticia3.Location = new System.Drawing.Point(719, 470);
            this.Noticia3.Name = "Noticia3";
            this.Noticia3.Size = new System.Drawing.Size(480, 150);
            this.Noticia3.TabIndex = 7;
            this.Noticia3.Resumen = "";
            this.Noticia3.Titulo = "Objetos únicos, procesos conscientes";
            this.Noticia3.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // BackgroundNoticias
            // 
            this.BackgroundNoticias.BackColor = System.Drawing.Color.White;
            this.BackgroundNoticias.Controls.Add(this.Noticia3);
            this.BackgroundNoticias.Controls.Add(this.Noticia2);
            this.BackgroundNoticias.Controls.Add(this.Noticia1);
            this.BackgroundNoticias.Controls.Add(this.NoticiaDestacada);
            this.BackgroundNoticias.Controls.Add(this.TituloLabel);
            this.BackgroundNoticias.Location = new System.Drawing.Point(0, 0);
            this.BackgroundNoticias.Name = "BackgroundNoticias";
            this.BackgroundNoticias.Size = new System.Drawing.Size(1280, 701);
            this.BackgroundNoticias.TabIndex = 0;
            // 
            // SeccionNoticias
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.BackgroundNoticias);
            this.Name = "SeccionNoticias";
            this.Size = new System.Drawing.Size(1280, 701);
            this.Load += new System.EventHandler(this.SeccionNoticias_Load);
            this.BackgroundNoticias.ResumeLayout(false);

            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label TituloLabel;
        private PITCHR.TarjetaNoticia NoticiaDestacada;
        private PITCHR.TarjetaNoticia Noticia1;
        private PITCHR.TarjetaNoticia Noticia2;
        private PITCHR.TarjetaNoticia Noticia3;
        private System.Windows.Forms.Panel BackgroundNoticias;
    }
}

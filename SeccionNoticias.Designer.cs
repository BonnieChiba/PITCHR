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
            this.NoticiaDestacada.Fecha = "25 SEP 2026";
            this.NoticiaDestacada.Enlace = "https://www.tiempo.com.mx/local/mayra-hurtado-startup-silicon-valley-chihuahua-2026/";
            this.NoticiaDestacada.Forma = PITCHR.TarjetaNoticia.Formas.Destacada;
            this.NoticiaDestacada.Imagen = ((System.Drawing.Image)(resources.GetObject("NoticiaDestacada.Imagen")));
            this.NoticiaDestacada.Location = new System.Drawing.Point(67, 126);
            this.NoticiaDestacada.Name = "NoticiaDestacada";
            this.NoticiaDestacada.Size = new System.Drawing.Size(632, 490);
            this.NoticiaDestacada.TabIndex = 4;
            this.NoticiaDestacada.Resumen = "Mayra Hurtado ganó con su startup Hormony el pase para representar a México en la final de Startup World Cup 2026 en Silicon Valley.";
            this.NoticiaDestacada.Titulo = "Competirá chihuahuense Mayra en mundial de Startup ¡en Silicon Valley!";
            this.NoticiaDestacada.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia1
            // 
            this.Noticia1.BackColor = System.Drawing.Color.White;
            this.Noticia1.Categoria = "Eventos";
            this.Noticia1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia1.Fecha = "22 JUN 2026";
            this.Noticia1.Enlace = "https://www.municipiochihuahua.gob.mx/CCS/Prensa/Invita_Municipio_a_Jornada_Emprende_2026_para_impulsar_negocios_y_proyectos_innovadores";
            this.Noticia1.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia1.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia1.Imagen")));
            this.Noticia1.Location = new System.Drawing.Point(719, 125);
            this.Noticia1.Name = "Noticia1";
            this.Noticia1.Size = new System.Drawing.Size(480, 150);
            this.Noticia1.TabIndex = 5;
            this.Noticia1.Resumen = "";
            this.Noticia1.Titulo = "Invita Municipio a Jornada Emprende 2026";
            this.Noticia1.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia2
            // 
            this.Noticia2.BackColor = System.Drawing.Color.White;
            this.Noticia2.Categoria = "Convocatorias";
            this.Noticia2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia2.Fecha = "10 MAR 2026";
            this.Noticia2.Enlace = "https://chihuahua.gob.mx/prensa/invita-gobierno-del-estado-participar-en-la-convocatoria-premio-chihuahua-emprende-2026";
            this.Noticia2.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia2.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia2.Imagen")));
            this.Noticia2.Location = new System.Drawing.Point(719, 295);
            this.Noticia2.Name = "Noticia2";
            this.Noticia2.Size = new System.Drawing.Size(480, 150);
            this.Noticia2.TabIndex = 6;
            this.Noticia2.Resumen = "";
            this.Noticia2.Titulo = "Convocatoria Premio Chihuahua Emprende 2026";
            this.Noticia2.TarjetaClick += new System.EventHandler(this.Noticia_Click);
            // 
            // Noticia3
            // 
            this.Noticia3.BackColor = System.Drawing.Color.White;
            this.Noticia3.Categoria = "Emprendimientos";
            this.Noticia3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Noticia3.Fecha = "16 FEB 2026";
            this.Noticia3.Enlace = "https://soychihuahua.mx/abre-municipio-convocatoria-a-emprendedores-para-participar-en-vende-tu-proyecto-2026/";
            this.Noticia3.Forma = PITCHR.TarjetaNoticia.Formas.Lista;
            this.Noticia3.Imagen = ((System.Drawing.Image)(resources.GetObject("Noticia3.Imagen")));
            this.Noticia3.Location = new System.Drawing.Point(719, 470);
            this.Noticia3.Name = "Noticia3";
            this.Noticia3.Size = new System.Drawing.Size(480, 150);
            this.Noticia3.TabIndex = 7;
            this.Noticia3.Resumen = "";
            this.Noticia3.Titulo = "Abre Municipio convocatoria \"Vende tu Proyecto 2026\"";
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

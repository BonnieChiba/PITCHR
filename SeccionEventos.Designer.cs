namespace PITCHR
{
    partial class SeccionEventos
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

        #region Component Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TituloLabel = new System.Windows.Forms.Label();
            this.SubtituloLabel = new System.Windows.Forms.Label();
            this.Carrusel = new PITCHR.CarruselEventos();
            this.AnteriorBoton = new PITCHR.BotonCircular();
            this.SiguienteBoton = new PITCHR.BotonCircular();
            this.SuspendLayout();
            //
            // AnteriorBoton
            //
            this.AnteriorBoton.Icono = PITCHR.BotonCircular.Iconos.Izquierda;
            this.AnteriorBoton.Location = new System.Drawing.Point(16, 389);
            this.AnteriorBoton.Name = "AnteriorBoton";
            this.AnteriorBoton.Size = new System.Drawing.Size(52, 52);
            this.AnteriorBoton.TabIndex = 3;
            //
            // SiguienteBoton
            //
            this.SiguienteBoton.Icono = PITCHR.BotonCircular.Iconos.Derecha;
            this.SiguienteBoton.Location = new System.Drawing.Point(1212, 389);
            this.SiguienteBoton.Name = "SiguienteBoton";
            this.SiguienteBoton.Size = new System.Drawing.Size(52, 52);
            this.SiguienteBoton.TabIndex = 4;
            //
            // TituloLabel
            //
            this.TituloLabel.BackColor = System.Drawing.Color.Transparent;
            this.TituloLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 80F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.TituloLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.TituloLabel.Location = new System.Drawing.Point(240, 40);
            this.TituloLabel.Name = "TituloLabel";
            this.TituloLabel.Size = new System.Drawing.Size(800, 110);
            this.TituloLabel.TabIndex = 0;
            this.TituloLabel.Text = "Eventos";
            this.TituloLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // SubtituloLabel
            //
            this.SubtituloLabel.BackColor = System.Drawing.Color.Transparent;
            this.SubtituloLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.SubtituloLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.SubtituloLabel.Location = new System.Drawing.Point(240, 152);
            this.SubtituloLabel.Name = "SubtituloLabel";
            this.SubtituloLabel.Size = new System.Drawing.Size(800, 40);
            this.SubtituloLabel.TabIndex = 1;
            this.SubtituloLabel.Text = "Vive la experiencia, comparte el futuro";
            this.SubtituloLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // Carrusel
            //
            this.Carrusel.BackColor = System.Drawing.Color.White;
            this.Carrusel.Location = new System.Drawing.Point(77, 236);
            this.Carrusel.Name = "Carrusel";
            this.Carrusel.Size = new System.Drawing.Size(1127, 360);
            this.Carrusel.TabIndex = 2;
            //
            // SeccionEventos
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.SiguienteBoton);
            this.Controls.Add(this.AnteriorBoton);
            this.Controls.Add(this.Carrusel);
            this.Controls.Add(this.SubtituloLabel);
            this.Controls.Add(this.TituloLabel);
            this.Name = "SeccionEventos";
            this.Size = new System.Drawing.Size(1280, 701);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label TituloLabel;
        private System.Windows.Forms.Label SubtituloLabel;
        private CarruselEventos Carrusel;
        private BotonCircular AnteriorBoton;
        private BotonCircular SiguienteBoton;
    }
}

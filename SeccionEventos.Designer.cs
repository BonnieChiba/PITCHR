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
            this.AvisoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TituloLabel
            // 
            this.TituloLabel.BackColor = System.Drawing.Color.Transparent;
            this.TituloLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 70F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.TituloLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.TituloLabel.Location = new System.Drawing.Point(62, 4);
            this.TituloLabel.Name = "TituloLabel";
            this.TituloLabel.Size = new System.Drawing.Size(700, 100);
            this.TituloLabel.TabIndex = 0;
            this.TituloLabel.Text = "Eventos";
            this.TituloLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AvisoLabel
            // 
            this.AvisoLabel.BackColor = System.Drawing.Color.Transparent;
            this.AvisoLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.AvisoLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(112)))), ((int)(((byte)(125)))));
            this.AvisoLabel.Location = new System.Drawing.Point(69, 120);
            this.AvisoLabel.Name = "AvisoLabel";
            this.AvisoLabel.Size = new System.Drawing.Size(1100, 40);
            this.AvisoLabel.TabIndex = 1;
            this.AvisoLabel.Text = "Muy pronto podrás ver aquí los eventos de la comunidad Pitcher.";
            // 
            // SeccionEventos
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.AvisoLabel);
            this.Controls.Add(this.TituloLabel);
            this.Name = "SeccionEventos";
            this.Size = new System.Drawing.Size(1280, 701);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label TituloLabel;
        private System.Windows.Forms.Label AvisoLabel;
    }
}

namespace PITCHR
{
    partial class SeccionMensajes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SeccionMensajes));
            this.FondoPanel = new System.Windows.Forms.Panel();
            this.TituloLabel = new System.Windows.Forms.Label();
            this.ConversacionesPanel = new System.Windows.Forms.Panel();
            this.ConversacionesLista = new System.Windows.Forms.Panel();
            this.SinConversacionesLabel = new System.Windows.Forms.Label();
            this.EncabezadoPanel = new System.Windows.Forms.Panel();
            this.ContactoFotoPanel = new System.Windows.Forms.Panel();
            this.ContactoNombreLabel = new System.Windows.Forms.Label();
            this.ChatPanel = new System.Windows.Forms.Panel();
            this.MensajesLista = new System.Windows.Forms.Panel();
            this.SinChatLabel = new System.Windows.Forms.Label();
            this.EditandoLabel = new System.Windows.Forms.Label();
            this.MensajeTextBox = new System.Windows.Forms.TextBox();
            this.EnviarBoton = new System.Windows.Forms.Button();
            this.FondoPanel.SuspendLayout();
            this.ConversacionesPanel.SuspendLayout();
            this.EncabezadoPanel.SuspendLayout();
            this.ChatPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // FondoPanel
            //
            this.FondoPanel.BackColor = System.Drawing.Color.White;
            this.FondoPanel.Controls.Add(this.TituloLabel);
            this.FondoPanel.Controls.Add(this.ConversacionesPanel);
            this.FondoPanel.Controls.Add(this.EncabezadoPanel);
            this.FondoPanel.Controls.Add(this.SinChatLabel);
            this.FondoPanel.Controls.Add(this.ChatPanel);
            this.FondoPanel.Controls.Add(this.EditandoLabel);
            this.FondoPanel.Controls.Add(this.MensajeTextBox);
            this.FondoPanel.Controls.Add(this.EnviarBoton);
            this.FondoPanel.Location = new System.Drawing.Point(0, 0);
            this.FondoPanel.Name = "FondoPanel";
            this.FondoPanel.Size = new System.Drawing.Size(1280, 717);
            this.FondoPanel.TabIndex = 0;
            //
            // TituloLabel
            //
            this.TituloLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.TituloLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 70F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.TituloLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.TituloLabel.Location = new System.Drawing.Point(0, 4);
            this.TituloLabel.Name = "TituloLabel";
            this.TituloLabel.Size = new System.Drawing.Size(398, 104);
            this.TituloLabel.TabIndex = 0;
            this.TituloLabel.Text = "Mensajes";
            this.TituloLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // ConversacionesPanel
            //
            this.ConversacionesPanel.AutoScroll = true;
            this.ConversacionesPanel.BackColor = System.Drawing.Color.White;
            this.ConversacionesPanel.Controls.Add(this.SinConversacionesLabel);
            this.ConversacionesPanel.Controls.Add(this.ConversacionesLista);
            this.ConversacionesPanel.Location = new System.Drawing.Point(0, 115);
            this.ConversacionesPanel.Name = "ConversacionesPanel";
            this.ConversacionesPanel.Size = new System.Drawing.Size(398, 602);
            this.ConversacionesPanel.TabIndex = 1;
            //
            // ConversacionesLista
            //
            this.ConversacionesLista.BackColor = System.Drawing.Color.White;
            this.ConversacionesLista.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ConversacionesLista.Location = new System.Drawing.Point(0, 0);
            this.ConversacionesLista.Name = "ConversacionesLista";
            this.ConversacionesLista.Size = new System.Drawing.Size(398, 92);
            this.ConversacionesLista.TabIndex = 0;
            this.ConversacionesLista.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ConversacionesLista_MouseClick);
            //
            // SinConversacionesLabel
            //
            this.SinConversacionesLabel.BackColor = System.Drawing.Color.White;
            this.SinConversacionesLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.SinConversacionesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(112)))), ((int)(((byte)(125)))));
            this.SinConversacionesLabel.Location = new System.Drawing.Point(24, 24);
            this.SinConversacionesLabel.Name = "SinConversacionesLabel";
            this.SinConversacionesLabel.Size = new System.Drawing.Size(350, 90);
            this.SinConversacionesLabel.TabIndex = 1;
            this.SinConversacionesLabel.Text = "Aún no tienes conversaciones.";
            this.SinConversacionesLabel.Visible = false;
            //
            // EncabezadoPanel
            //
            this.EncabezadoPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(110)))), ((int)(((byte)(56)))));
            this.EncabezadoPanel.Controls.Add(this.ContactoFotoPanel);
            this.EncabezadoPanel.Controls.Add(this.ContactoNombreLabel);
            this.EncabezadoPanel.Location = new System.Drawing.Point(401, 0);
            this.EncabezadoPanel.Name = "EncabezadoPanel";
            this.EncabezadoPanel.Size = new System.Drawing.Size(879, 115);
            this.EncabezadoPanel.TabIndex = 2;
            //
            // ContactoFotoPanel
            //
            this.ContactoFotoPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(110)))), ((int)(((byte)(56)))));
            this.ContactoFotoPanel.Location = new System.Drawing.Point(23, 21);
            this.ContactoFotoPanel.Name = "ContactoFotoPanel";
            this.ContactoFotoPanel.Size = new System.Drawing.Size(74, 74);
            this.ContactoFotoPanel.TabIndex = 0;
            //
            // ContactoNombreLabel
            //
            this.ContactoNombreLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(110)))), ((int)(((byte)(56)))));
            this.ContactoNombreLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.ContactoNombreLabel.ForeColor = System.Drawing.Color.White;
            this.ContactoNombreLabel.Location = new System.Drawing.Point(119, 24);
            this.ContactoNombreLabel.Name = "ContactoNombreLabel";
            this.ContactoNombreLabel.Size = new System.Drawing.Size(740, 66);
            this.ContactoNombreLabel.TabIndex = 1;
            this.ContactoNombreLabel.Text = "Nombre del contacto";
            this.ContactoNombreLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // ChatPanel
            //
            this.ChatPanel.AutoScroll = true;
            this.ChatPanel.BackColor = System.Drawing.Color.White;
            this.ChatPanel.Controls.Add(this.MensajesLista);
            this.ChatPanel.Location = new System.Drawing.Point(410, 115);
            this.ChatPanel.Name = "ChatPanel";
            this.ChatPanel.Size = new System.Drawing.Size(870, 480);
            this.ChatPanel.TabIndex = 3;
            //
            // MensajesLista
            //
            this.MensajesLista.BackColor = System.Drawing.Color.White;
            this.MensajesLista.Location = new System.Drawing.Point(0, 0);
            this.MensajesLista.Name = "MensajesLista";
            this.MensajesLista.Size = new System.Drawing.Size(870, 480);
            this.MensajesLista.TabIndex = 0;
            this.MensajesLista.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MensajesLista_MouseUp);
            //
            // SinChatLabel
            //
            this.SinChatLabel.BackColor = System.Drawing.Color.White;
            this.SinChatLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.SinChatLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(112)))), ((int)(((byte)(125)))));
            this.SinChatLabel.Location = new System.Drawing.Point(457, 300);
            this.SinChatLabel.Name = "SinChatLabel";
            this.SinChatLabel.Size = new System.Drawing.Size(752, 120);
            this.SinChatLabel.TabIndex = 4;
            this.SinChatLabel.Text = "Elige una conversación de la lista para ver sus mensajes.";
            this.SinChatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.SinChatLabel.Visible = false;
            //
            // EditandoLabel
            //
            this.EditandoLabel.BackColor = System.Drawing.Color.White;
            this.EditandoLabel.Font = new System.Drawing.Font("Plus Jakarta Sans", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.EditandoLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(96)))), ((int)(((byte)(214)))));
            this.EditandoLabel.Location = new System.Drawing.Point(480, 600);
            this.EditandoLabel.Name = "EditandoLabel";
            this.EditandoLabel.Size = new System.Drawing.Size(700, 26);
            this.EditandoLabel.TabIndex = 5;
            this.EditandoLabel.Text = "Editando mensaje  ·  Esc para cancelar";
            this.EditandoLabel.Visible = false;
            //
            // MensajeTextBox
            //
            this.MensajeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.MensajeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MensajeTextBox.Font = new System.Drawing.Font("Plus Jakarta Sans", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.MensajeTextBox.Location = new System.Drawing.Point(487, 645);
            this.MensajeTextBox.MaxLength = 1000;
            this.MensajeTextBox.Name = "MensajeTextBox";
            this.MensajeTextBox.Size = new System.Drawing.Size(655, 28);
            this.MensajeTextBox.TabIndex = 6;
            this.MensajeTextBox.Tag = "Escribir un mensaje...";
            this.MensajeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MensajeTextBox_KeyDown);
            //
            // EnviarBoton
            //
            this.EnviarBoton.BackColor = System.Drawing.Color.Transparent;
            this.EnviarBoton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EnviarBoton.FlatAppearance.BorderSize = 0;
            this.EnviarBoton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.EnviarBoton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.EnviarBoton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EnviarBoton.Image = ((System.Drawing.Image)(resources.GetObject("EnviarBoton.Image")));
            this.EnviarBoton.Location = new System.Drawing.Point(1157, 637);
            this.EnviarBoton.Name = "EnviarBoton";
            this.EnviarBoton.Size = new System.Drawing.Size(45, 45);
            this.EnviarBoton.TabIndex = 7;
            this.EnviarBoton.UseVisualStyleBackColor = false;
            this.EnviarBoton.Click += new System.EventHandler(this.EnviarBoton_Click);
            //
            // SeccionMensajes
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.FondoPanel);
            this.Name = "SeccionMensajes";
            this.Size = new System.Drawing.Size(1280, 717);
            this.Load += new System.EventHandler(this.SeccionMensajes_Load);
            this.FondoPanel.ResumeLayout(false);
            this.FondoPanel.PerformLayout();
            this.ConversacionesPanel.ResumeLayout(false);
            this.EncabezadoPanel.ResumeLayout(false);
            this.ChatPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel FondoPanel;
        private System.Windows.Forms.Label TituloLabel;
        private System.Windows.Forms.Panel ConversacionesPanel;
        private System.Windows.Forms.Panel ConversacionesLista;
        private System.Windows.Forms.Label SinConversacionesLabel;
        private System.Windows.Forms.Panel EncabezadoPanel;
        private System.Windows.Forms.Panel ContactoFotoPanel;
        private System.Windows.Forms.Label ContactoNombreLabel;
        private System.Windows.Forms.Panel ChatPanel;
        private System.Windows.Forms.Panel MensajesLista;
        private System.Windows.Forms.Label SinChatLabel;
        private System.Windows.Forms.Label EditandoLabel;
        private System.Windows.Forms.TextBox MensajeTextBox;
        private System.Windows.Forms.Button EnviarBoton;
    }
}

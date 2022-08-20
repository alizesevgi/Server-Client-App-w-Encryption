namespace server_432project
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.port_to_be_connected = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.IP_text = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.port_to_listen = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.listen = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.port_server1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server 2";
            // 
            // port_to_be_connected
            // 
            this.port_to_be_connected.Location = new System.Drawing.Point(88, 65);
            this.port_to_be_connected.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.port_to_be_connected.Name = "port_to_be_connected";
            this.port_to_be_connected.Size = new System.Drawing.Size(87, 22);
            this.port_to_be_connected.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port ";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "IP";
            // 
            // IP_text
            // 
            this.IP_text.Location = new System.Drawing.Point(88, 103);
            this.IP_text.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.IP_text.Name = "IP_text";
            this.IP_text.Size = new System.Drawing.Size(85, 22);
            this.IP_text.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(34, 147);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(141, 46);
            this.button1.TabIndex = 5;
            this.button1.Text = "connect to the master server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // port_to_listen
            // 
            this.port_to_listen.Location = new System.Drawing.Point(310, 65);
            this.port_to_listen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.port_to_listen.Name = "port_to_listen";
            this.port_to_listen.Size = new System.Drawing.Size(84, 22);
            this.port_to_listen.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Port";
            // 
            // listen
            // 
            this.listen.Location = new System.Drawing.Point(244, 110);
            this.listen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listen.Name = "listen";
            this.listen.Size = new System.Drawing.Size(152, 33);
            this.listen.TabIndex = 8;
            this.listen.Text = "listen";
            this.listen.UseVisualStyleBackColor = true;
            this.listen.Click += new System.EventHandler(this.listen_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(35, 256);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(391, 164);
            this.richTextBox1.TabIndex = 9;
            this.richTextBox1.Text = "";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(244, 195);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(153, 34);
            this.button2.TabIndex = 11;
            this.button2.Text = "connect to server 1";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(241, 162);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Port ";
            // 
            // port_server1
            // 
            this.port_server1.Location = new System.Drawing.Point(309, 157);
            this.port_server1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.port_server1.Name = "port_server1";
            this.port_server1.Size = new System.Drawing.Size(87, 22);
            this.port_server1.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 436);
            this.Controls.Add(this.port_server1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.listen);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.port_to_listen);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.IP_text);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.port_to_be_connected);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox port_to_be_connected;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox IP_text;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox port_to_listen;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button listen;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox port_server1;
    }
}


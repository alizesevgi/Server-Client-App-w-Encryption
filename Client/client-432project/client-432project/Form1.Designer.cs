namespace client_432project
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
            this.IP_address_text = new System.Windows.Forms.TextBox();
            this.port_num_text = new System.Windows.Forms.TextBox();
            this.IP = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.connect_button = new System.Windows.Forms.Button();
            this.disconnect_button = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox1_filepath = new System.Windows.Forms.TextBox();
            this.button1_upload = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.clientName_box = new System.Windows.Forms.TextBox();
            this.download_file_name = new System.Windows.Forms.TextBox();
            this.download_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // IP_address_text
            // 
            this.IP_address_text.Location = new System.Drawing.Point(66, 37);
            this.IP_address_text.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.IP_address_text.Name = "IP_address_text";
            this.IP_address_text.Size = new System.Drawing.Size(91, 20);
            this.IP_address_text.TabIndex = 0;
            // 
            // port_num_text
            // 
            this.port_num_text.Location = new System.Drawing.Point(67, 73);
            this.port_num_text.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.port_num_text.Name = "port_num_text";
            this.port_num_text.Size = new System.Drawing.Size(90, 20);
            this.port_num_text.TabIndex = 1;
            // 
            // IP
            // 
            this.IP.AutoSize = true;
            this.IP.Location = new System.Drawing.Point(13, 41);
            this.IP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(17, 13);
            this.IP.TabIndex = 2;
            this.IP.Text = "IP";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 75);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "PORT";
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(219, 35);
            this.connect_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(134, 22);
            this.connect_button.TabIndex = 4;
            this.connect_button.Text = "connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.connect_button_Click);
            // 
            // disconnect_button
            // 
            this.disconnect_button.Enabled = false;
            this.disconnect_button.Location = new System.Drawing.Point(219, 72);
            this.disconnect_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.disconnect_button.Name = "disconnect_button";
            this.disconnect_button.Size = new System.Drawing.Size(134, 20);
            this.disconnect_button.TabIndex = 5;
            this.disconnect_button.Text = "disconnect";
            this.disconnect_button.UseVisualStyleBackColor = true;
            this.disconnect_button.Click += new System.EventHandler(this.disconnect_button_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(15, 115);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(491, 181);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            // 
            // textBox1_filepath
            // 
            this.textBox1_filepath.Location = new System.Drawing.Point(40, 321);
            this.textBox1_filepath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox1_filepath.Name = "textBox1_filepath";
            this.textBox1_filepath.Size = new System.Drawing.Size(165, 20);
            this.textBox1_filepath.TabIndex = 8;
            // 
            // button1_upload
            // 
            this.button1_upload.Enabled = false;
            this.button1_upload.Location = new System.Drawing.Point(219, 321);
            this.button1_upload.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button1_upload.Name = "button1_upload";
            this.button1_upload.Size = new System.Drawing.Size(134, 22);
            this.button1_upload.TabIndex = 9;
            this.button1_upload.Text = "upload";
            this.button1_upload.UseVisualStyleBackColor = true;
            this.button1_upload.Click += new System.EventHandler(this.button1_upload_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "client name";
            // 
            // clientName_box
            // 
            this.clientName_box.Location = new System.Drawing.Point(76, 10);
            this.clientName_box.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.clientName_box.Name = "clientName_box";
            this.clientName_box.Size = new System.Drawing.Size(80, 20);
            this.clientName_box.TabIndex = 11;
            // 
            // download_file_name
            // 
            this.download_file_name.Location = new System.Drawing.Point(40, 353);
            this.download_file_name.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.download_file_name.Name = "download_file_name";
            this.download_file_name.Size = new System.Drawing.Size(164, 20);
            this.download_file_name.TabIndex = 12;
            // 
            // download_button
            // 
            this.download_button.Enabled = false;
            this.download_button.Location = new System.Drawing.Point(219, 353);
            this.download_button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.download_button.Name = "download_button";
            this.download_button.Size = new System.Drawing.Size(134, 24);
            this.download_button.TabIndex = 13;
            this.download_button.Text = "download";
            this.download_button.UseVisualStyleBackColor = true;
            this.download_button.Click += new System.EventHandler(this.download_button_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 436);
            this.Controls.Add(this.download_button);
            this.Controls.Add(this.download_file_name);
            this.Controls.Add(this.clientName_box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1_upload);
            this.Controls.Add(this.textBox1_filepath);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.disconnect_button);
            this.Controls.Add(this.connect_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IP);
            this.Controls.Add(this.port_num_text);
            this.Controls.Add(this.IP_address_text);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox IP_address_text;
        private System.Windows.Forms.TextBox port_num_text;
        private System.Windows.Forms.Label IP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.Button disconnect_button;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox1_filepath;
        private System.Windows.Forms.Button button1_upload;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox clientName_box;
        private System.Windows.Forms.TextBox download_file_name;
        private System.Windows.Forms.Button download_button;
    }
}


namespace LoginApp
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.ButtonLogin = new System.Windows.Forms.Button();
            this.ButtonRegister = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxSaltedHash = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSalt = new System.Windows.Forms.TextBox();
            this.buttonGenerateSaltedHash = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxSizePass = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxSizeSalt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(28, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(32, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Username";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(32, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Password";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(35, 103);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(195, 20);
            this.textBoxUsername.TabIndex = 3;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(35, 169);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(195, 20);
            this.textBoxPassword.TabIndex = 4;
            // 
            // ButtonLogin
            // 
            this.ButtonLogin.Location = new System.Drawing.Point(157, 218);
            this.ButtonLogin.Name = "ButtonLogin";
            this.ButtonLogin.Size = new System.Drawing.Size(73, 40);
            this.ButtonLogin.TabIndex = 5;
            this.ButtonLogin.Text = "button1";
            this.ButtonLogin.UseVisualStyleBackColor = true;
            this.ButtonLogin.Click += new System.EventHandler(this.ButtonLogin_Click);
            // 
            // ButtonRegister
            // 
            this.ButtonRegister.Location = new System.Drawing.Point(51, 218);
            this.ButtonRegister.Name = "ButtonRegister";
            this.ButtonRegister.Size = new System.Drawing.Size(73, 40);
            this.ButtonRegister.TabIndex = 6;
            this.ButtonRegister.Text = "Register";
            this.ButtonRegister.UseVisualStyleBackColor = true;
            this.ButtonRegister.Click += new System.EventHandler(this.ButtonRegister_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(371, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Salted Password Hash";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // textBoxSaltedHash
            // 
            this.textBoxSaltedHash.Location = new System.Drawing.Point(364, 65);
            this.textBoxSaltedHash.Multiline = true;
            this.textBoxSaltedHash.Name = "textBoxSaltedHash";
            this.textBoxSaltedHash.ReadOnly = true;
            this.textBoxSaltedHash.Size = new System.Drawing.Size(301, 58);
            this.textBoxSaltedHash.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(371, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Salt";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // textBoxSalt
            // 
            this.textBoxSalt.Location = new System.Drawing.Point(364, 169);
            this.textBoxSalt.Multiline = true;
            this.textBoxSalt.Name = "textBoxSalt";
            this.textBoxSalt.ReadOnly = true;
            this.textBoxSalt.Size = new System.Drawing.Size(301, 37);
            this.textBoxSalt.TabIndex = 26;
            // 
            // buttonGenerateSaltedHash
            // 
            this.buttonGenerateSaltedHash.Location = new System.Drawing.Point(583, 218);
            this.buttonGenerateSaltedHash.Name = "buttonGenerateSaltedHash";
            this.buttonGenerateSaltedHash.Size = new System.Drawing.Size(82, 23);
            this.buttonGenerateSaltedHash.TabIndex = 31;
            this.buttonGenerateSaltedHash.Text = "Generate";
            this.buttonGenerateSaltedHash.UseVisualStyleBackColor = true;
            this.buttonGenerateSaltedHash.Click += new System.EventHandler(this.buttonGenerateSaltedHash_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(700, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Size (Bits)";
            // 
            // textBoxSizePass
            // 
            this.textBoxSizePass.Location = new System.Drawing.Point(703, 65);
            this.textBoxSizePass.Name = "textBoxSizePass";
            this.textBoxSizePass.ReadOnly = true;
            this.textBoxSizePass.Size = new System.Drawing.Size(48, 20);
            this.textBoxSizePass.TabIndex = 33;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(700, 153);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Size (Bits)";
            // 
            // textBoxSizeSalt
            // 
            this.textBoxSizeSalt.Location = new System.Drawing.Point(703, 169);
            this.textBoxSizeSalt.Name = "textBoxSizeSalt";
            this.textBoxSizeSalt.ReadOnly = true;
            this.textBoxSizeSalt.Size = new System.Drawing.Size(48, 20);
            this.textBoxSizeSalt.TabIndex = 35;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 313);
            this.Controls.Add(this.textBoxSizeSalt);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxSizePass);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonGenerateSaltedHash);
            this.Controls.Add(this.textBoxSalt);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxSaltedHash);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ButtonRegister);
            this.Controls.Add(this.ButtonLogin);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button ButtonLogin;
        private System.Windows.Forms.Button ButtonRegister;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxSaltedHash;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSalt;
        private System.Windows.Forms.Button buttonGenerateSaltedHash;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxSizePass;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxSizeSalt;
    }
}


namespace WHA_avac
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.loginB = new System.Windows.Forms.Button();
            this.autoB = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.rate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.logT = new System.Windows.Forms.RichTextBox();
            this.addB = new System.Windows.Forms.Button();
            this.deleteB = new System.Windows.Forms.Button();
            this.inputT = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.urlList = new System.Windows.Forms.CheckedListBox();
            this.testLog = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loginB
            // 
            this.loginB.Location = new System.Drawing.Point(276, 66);
            this.loginB.Name = "loginB";
            this.loginB.Size = new System.Drawing.Size(88, 36);
            this.loginB.TabIndex = 1;
            this.loginB.Text = "Login";
            this.loginB.UseVisualStyleBackColor = true;
            this.loginB.Click += new System.EventHandler(this.loginB_Click);
            // 
            // autoB
            // 
            this.autoB.Location = new System.Drawing.Point(276, 12);
            this.autoB.Name = "autoB";
            this.autoB.Size = new System.Drawing.Size(218, 48);
            this.autoB.TabIndex = 0;
            this.autoB.Text = "start";
            this.autoB.UseVisualStyleBackColor = true;
            this.autoB.Click += new System.EventHandler(this.autoB_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "rate:";
            // 
            // rate
            // 
            this.rate.Location = new System.Drawing.Point(75, 23);
            this.rate.Name = "rate";
            this.rate.Size = new System.Drawing.Size(68, 21);
            this.rate.TabIndex = 4;
            this.rate.Text = "500";
            this.rate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.rate_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(149, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "ms";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F);
            this.label3.Location = new System.Drawing.Point(441, 362);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Copyright  Thomas";
            // 
            // logT
            // 
            this.logT.Location = new System.Drawing.Point(513, 12);
            this.logT.Name = "logT";
            this.logT.ReadOnly = true;
            this.logT.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.logT.Size = new System.Drawing.Size(406, 338);
            this.logT.TabIndex = 9;
            this.logT.Text = "";
            this.logT.TextChanged += new System.EventHandler(this.logT_TextChanged);
            // 
            // addB
            // 
            this.addB.Location = new System.Drawing.Point(398, 66);
            this.addB.Name = "addB";
            this.addB.Size = new System.Drawing.Size(96, 36);
            this.addB.TabIndex = 11;
            this.addB.Text = "import";
            this.addB.UseVisualStyleBackColor = true;
            this.addB.Click += new System.EventHandler(this.addB_Click);
            // 
            // deleteB
            // 
            this.deleteB.Location = new System.Drawing.Point(192, 12);
            this.deleteB.Name = "deleteB";
            this.deleteB.Size = new System.Drawing.Size(63, 23);
            this.deleteB.TabIndex = 12;
            this.deleteB.Text = "delete";
            this.deleteB.UseVisualStyleBackColor = true;
            this.deleteB.Visible = false;
            this.deleteB.Click += new System.EventHandler(this.deleteB_Click);
            // 
            // inputT
            // 
            this.inputT.Location = new System.Drawing.Point(75, 50);
            this.inputT.Name = "inputT";
            this.inputT.Size = new System.Drawing.Size(180, 21);
            this.inputT.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "username:";
            // 
            // urlList
            // 
            this.urlList.Enabled = false;
            this.urlList.FormattingEnabled = true;
            this.urlList.Location = new System.Drawing.Point(14, 106);
            this.urlList.Name = "urlList";
            this.urlList.Size = new System.Drawing.Size(480, 244);
            this.urlList.TabIndex = 15;
            // 
            // testLog
            // 
            this.testLog.Location = new System.Drawing.Point(14, 392);
            this.testLog.Name = "testLog";
            this.testLog.ReadOnly = true;
            this.testLog.Size = new System.Drawing.Size(905, 316);
            this.testLog.TabIndex = 16;
            this.testLog.Text = "";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(75, 79);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(180, 21);
            this.textBox1.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "password:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(289, 357);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "test regx";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 730);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.testLog);
            this.Controls.Add(this.urlList);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.inputT);
            this.Controls.Add(this.deleteB);
            this.Controls.Add(this.addB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logT);
            this.Controls.Add(this.autoB);
            this.Controls.Add(this.loginB);
            this.Name = "Form1";
            this.Text = "FacebookFriendsIdWorm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loginB;
        private System.Windows.Forms.Button autoB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox logT;
        private System.Windows.Forms.Button addB;
        private System.Windows.Forms.Button deleteB;
        private System.Windows.Forms.TextBox inputT;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckedListBox urlList;
        private System.Windows.Forms.RichTextBox testLog;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
    }
}


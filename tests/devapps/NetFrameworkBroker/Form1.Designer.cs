namespace NetDesktopWinForms
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
            this.resultTbx = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.authorityCbx = new System.Windows.Forms.ComboBox();
            this.clientIdCbx = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.loginHintTxt = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.promptCbx = new System.Windows.Forms.ComboBox();
            this.atsBtn = new System.Windows.Forms.Button();
            this.atiBtn = new System.Windows.Forms.Button();
            this.atsAtiBtn = new System.Windows.Forms.Button();
            this.useBrokerChk = new System.Windows.Forms.CheckBox();
            this.accBtn = new System.Windows.Forms.Button();
            this.clearBtn = new System.Windows.Forms.Button();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.cbxScopes = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // resultTbx
            // 
            this.resultTbx.Location = new System.Drawing.Point(12, 191);
            this.resultTbx.Multiline = true;
            this.resultTbx.Name = "resultTbx";
            this.resultTbx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultTbx.Size = new System.Drawing.Size(615, 286);
            this.resultTbx.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Authority";
            // 
            // authorityCbx
            // 
            this.authorityCbx.FormattingEnabled = true;
            this.authorityCbx.Items.AddRange(new object[] {
            "https://login.microsoftonline.com/common",
            "https://login.microsoftonline.com/organizations",
            "https://login.microsoftonline.com/consumers",
            "https://login.microsoftonline.com/49f548d0-12b7-4169-a390-bb5304d24462",
            "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47",
            "https://login.windows-ppe.net/organizations"});
            this.authorityCbx.Location = new System.Drawing.Point(63, 13);
            this.authorityCbx.Name = "authorityCbx";
            this.authorityCbx.Size = new System.Drawing.Size(334, 21);
            this.authorityCbx.TabIndex = 3;
            this.authorityCbx.Text = "https://login.microsoftonline.com/common";
            // 
            // clientIdCbx
            // 
            this.clientIdCbx.FormattingEnabled = true;
            this.clientIdCbx.Items.AddRange(new object[] {
            "1d18b3b0-251b-4714-a02a-9956cec86c2d",
            "872cd9fa-d31f-45e0-9eab-6e460a02d1f1"});
            this.clientIdCbx.Location = new System.Drawing.Point(63, 40);
            this.clientIdCbx.Name = "clientIdCbx";
            this.clientIdCbx.Size = new System.Drawing.Size(283, 21);
            this.clientIdCbx.TabIndex = 4;
            this.clientIdCbx.Text = "1d18b3b0-251b-4714-a02a-9956cec86c2d";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "ClientId";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Login Hint";
            // 
            // loginHintTxt
            // 
            this.loginHintTxt.Location = new System.Drawing.Point(71, 68);
            this.loginHintTxt.Name = "loginHintTxt";
            this.loginHintTxt.Size = new System.Drawing.Size(100, 20);
            this.loginHintTxt.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(177, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Prompt";
            // 
            // promptCbx
            // 
            this.promptCbx.FormattingEnabled = true;
            this.promptCbx.Items.AddRange(new object[] {
            "select_account",
            "force_login",
            "no_prompt",
            "consent",
            "never"});
            this.promptCbx.Location = new System.Drawing.Point(224, 66);
            this.promptCbx.Name = "promptCbx";
            this.promptCbx.Size = new System.Drawing.Size(121, 21);
            this.promptCbx.TabIndex = 10;
            // 
            // atsBtn
            // 
            this.atsBtn.Location = new System.Drawing.Point(12, 118);
            this.atsBtn.Name = "atsBtn";
            this.atsBtn.Size = new System.Drawing.Size(75, 23);
            this.atsBtn.TabIndex = 11;
            this.atsBtn.Text = "ATS";
            this.atsBtn.UseVisualStyleBackColor = true;
            this.atsBtn.Click += new System.EventHandler(this.atsBtn_Click);
            // 
            // atiBtn
            // 
            this.atiBtn.Location = new System.Drawing.Point(93, 118);
            this.atiBtn.Name = "atiBtn";
            this.atiBtn.Size = new System.Drawing.Size(75, 23);
            this.atiBtn.TabIndex = 12;
            this.atiBtn.Text = "ATI";
            this.atiBtn.UseVisualStyleBackColor = true;
            this.atiBtn.Click += new System.EventHandler(this.atiBtn_Click);
            // 
            // atsAtiBtn
            // 
            this.atsAtiBtn.Location = new System.Drawing.Point(174, 118);
            this.atsAtiBtn.Name = "atsAtiBtn";
            this.atsAtiBtn.Size = new System.Drawing.Size(75, 23);
            this.atsAtiBtn.TabIndex = 13;
            this.atsAtiBtn.Text = "ATS + ATI";
            this.atsAtiBtn.UseVisualStyleBackColor = true;
            this.atsAtiBtn.Click += new System.EventHandler(this.atsAtiBtn_Click);
            // 
            // useBrokerChk
            // 
            this.useBrokerChk.AutoSize = true;
            this.useBrokerChk.Checked = true;
            this.useBrokerChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useBrokerChk.Location = new System.Drawing.Point(12, 95);
            this.useBrokerChk.Name = "useBrokerChk";
            this.useBrokerChk.Size = new System.Drawing.Size(79, 17);
            this.useBrokerChk.TabIndex = 14;
            this.useBrokerChk.Text = "Use Broker";
            this.useBrokerChk.UseVisualStyleBackColor = true;
            // 
            // accBtn
            // 
            this.accBtn.Location = new System.Drawing.Point(282, 118);
            this.accBtn.Name = "accBtn";
            this.accBtn.Size = new System.Drawing.Size(115, 23);
            this.accBtn.TabIndex = 15;
            this.accBtn.Text = "Show Accounts";
            this.accBtn.UseVisualStyleBackColor = true;
            this.accBtn.Click += new System.EventHandler(this.accBtn_Click);
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(552, 483);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(75, 23);
            this.clearBtn.TabIndex = 16;
            this.clearBtn.Text = "Clear Log";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // btnClearCache
            // 
            this.btnClearCache.Location = new System.Drawing.Point(282, 147);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(115, 23);
            this.btnClearCache.TabIndex = 17;
            this.btnClearCache.Text = "Clear MSAL Cache";
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // cbxScopes
            // 
            this.cbxScopes.FormattingEnabled = true;
            this.cbxScopes.Items.AddRange(new object[] {
            "User.Read",
            "User.Read User.Read.All"});
            this.cbxScopes.Location = new System.Drawing.Point(401, 40);
            this.cbxScopes.Name = "cbxScopes";
            this.cbxScopes.Size = new System.Drawing.Size(226, 21);
            this.cbxScopes.TabIndex = 18;
            this.cbxScopes.Text = "User.Read";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(352, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Scopes";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 510);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbxScopes);
            this.Controls.Add(this.btnClearCache);
            this.Controls.Add(this.clearBtn);
            this.Controls.Add(this.accBtn);
            this.Controls.Add(this.useBrokerChk);
            this.Controls.Add(this.atsAtiBtn);
            this.Controls.Add(this.atiBtn);
            this.Controls.Add(this.atsBtn);
            this.Controls.Add(this.promptCbx);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.loginHintTxt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.clientIdCbx);
            this.Controls.Add(this.authorityCbx);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.resultTbx);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox resultTbx;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox authorityCbx;
        private System.Windows.Forms.ComboBox clientIdCbx;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox loginHintTxt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox promptCbx;
        private System.Windows.Forms.Button atsBtn;
        private System.Windows.Forms.Button atiBtn;
        private System.Windows.Forms.Button atsAtiBtn;
        private System.Windows.Forms.CheckBox useBrokerChk;
        private System.Windows.Forms.Button accBtn;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.ComboBox cbxScopes;
        private System.Windows.Forms.Label label5;
    }
}


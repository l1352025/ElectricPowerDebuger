namespace ElectricPowerDebuger.Dialog
{
    partial class InputTwoParamDlg
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
            this.lbParam1 = new System.Windows.Forms.Label();
            this.lbParam2 = new System.Windows.Forms.Label();
            this.txtParam1 = new System.Windows.Forms.TextBox();
            this.txtParam2 = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbParam1
            // 
            this.lbParam1.Location = new System.Drawing.Point(13, 22);
            this.lbParam1.Name = "lbParam1";
            this.lbParam1.Size = new System.Drawing.Size(131, 12);
            this.lbParam1.TabIndex = 0;
            this.lbParam1.Text = "参数 1（地址/数字）：";
            // 
            // lbParam2
            // 
            this.lbParam2.Location = new System.Drawing.Point(13, 55);
            this.lbParam2.Name = "lbParam2";
            this.lbParam2.Size = new System.Drawing.Size(131, 12);
            this.lbParam2.TabIndex = 0;
            this.lbParam2.Text = "参数 2（报文/数字）：";
            // 
            // txtParam1
            // 
            this.txtParam1.Location = new System.Drawing.Point(150, 19);
            this.txtParam1.Name = "txtParam1";
            this.txtParam1.Size = new System.Drawing.Size(157, 21);
            this.txtParam1.TabIndex = 1;
            this.txtParam1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtParam1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtParam1_KeyPress);
            // 
            // txtParam2
            // 
            this.txtParam2.Location = new System.Drawing.Point(150, 52);
            this.txtParam2.Name = "txtParam2";
            this.txtParam2.Size = new System.Drawing.Size(157, 21);
            this.txtParam2.TabIndex = 1;
            this.txtParam2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtParam2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtParam2_KeyPress);
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(150, 88);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(57, 32);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "确定";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(251, 88);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(56, 32);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // InputTwoParamDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 140);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.txtParam2);
            this.Controls.Add(this.txtParam1);
            this.Controls.Add(this.lbParam2);
            this.Controls.Add(this.lbParam1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputTwoParamDlg";
            this.ShowInTaskbar = false;
            this.Text = "请输入";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InputTwoParamDlg_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbParam1;
        private System.Windows.Forms.Label lbParam2;
        private System.Windows.Forms.TextBox txtParam1;
        private System.Windows.Forms.TextBox txtParam2;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
    }
}
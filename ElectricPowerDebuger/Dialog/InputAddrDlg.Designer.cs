namespace ElectricPowerDebuger.Dialog
{
    partial class InputAddrDlg
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
            this.tbAddress = new System.Windows.Forms.TextBox();
            this.lbNewAddress = new System.Windows.Forms.Label();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbAddress
            // 
            this.tbAddress.Location = new System.Drawing.Point(102, 33);
            this.tbAddress.Name = "tbAddress";
            this.tbAddress.Size = new System.Drawing.Size(117, 21);
            this.tbAddress.TabIndex = 0;
            this.tbAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbAddress_KeyPress);
            // 
            // lbNewAddress
            // 
            this.lbNewAddress.AutoSize = true;
            this.lbNewAddress.Location = new System.Drawing.Point(35, 36);
            this.lbNewAddress.Name = "lbNewAddress";
            this.lbNewAddress.Size = new System.Drawing.Size(65, 12);
            this.lbNewAddress.TabIndex = 1;
            this.lbNewAddress.Text = "节点地址：";
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(46, 83);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(72, 24);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "确认";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(147, 83);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(72, 24);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // InputAddrDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 126);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.lbNewAddress);
            this.Controls.Add(this.tbAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputAddrDlg";
            this.ShowInTaskbar = false;
            this.Text = "请输入节点地址";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InputAddrDlg_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbAddress;
        private System.Windows.Forms.Label lbNewAddress;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
    }
}
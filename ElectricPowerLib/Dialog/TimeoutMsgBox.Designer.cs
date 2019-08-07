namespace ElectricPowerDebuger.Dialog
{
    partial class TimeoutMsgBox
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
            this.lbMsg = new System.Windows.Forms.Label();
            this.lbTimeout = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMsg.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbMsg.ForeColor = System.Drawing.Color.Linen;
            this.lbMsg.Location = new System.Drawing.Point(157, 105);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(91, 40);
            this.lbMsg.TabIndex = 0;
            this.lbMsg.Text = "PASS";
            // 
            // lbTimeout
            // 
            this.lbTimeout.AutoSize = true;
            this.lbTimeout.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbTimeout.ForeColor = System.Drawing.Color.Linen;
            this.lbTimeout.Location = new System.Drawing.Point(372, 221);
            this.lbTimeout.Name = "lbTimeout";
            this.lbTimeout.Size = new System.Drawing.Size(31, 21);
            this.lbTimeout.TabIndex = 1;
            this.lbTimeout.Text = "3 s";
            // 
            // TimeoutMsgBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.ClientSize = new System.Drawing.Size(415, 251);
            this.Controls.Add(this.lbTimeout);
            this.Controls.Add(this.lbMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimeoutMsgBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TimeoutMsgBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Label lbTimeout;
    }
}
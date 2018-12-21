namespace ElectricPowerDebuger.Dialog
{
    partial class SelectTaskIdDlg
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
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.lbSelectTaskId = new System.Windows.Forms.Label();
            this.cmbTaskIdList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(50, 78);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 0;
            this.btOk.Text = "确认";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(159, 78);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // lbSelectTaskId
            // 
            this.lbSelectTaskId.AutoSize = true;
            this.lbSelectTaskId.Location = new System.Drawing.Point(12, 33);
            this.lbSelectTaskId.Name = "lbSelectTaskId";
            this.lbSelectTaskId.Size = new System.Drawing.Size(113, 12);
            this.lbSelectTaskId.TabIndex = 3;
            this.lbSelectTaskId.Text = "请选择或输入任务ID";
            // 
            // cmbTaskIdList
            // 
            this.cmbTaskIdList.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbTaskIdList.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cmbTaskIdList.FormattingEnabled = true;
            this.cmbTaskIdList.Location = new System.Drawing.Point(140, 29);
            this.cmbTaskIdList.Name = "cmbTaskIdList";
            this.cmbTaskIdList.Size = new System.Drawing.Size(94, 20);
            this.cmbTaskIdList.TabIndex = 4;
            this.cmbTaskIdList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbTaskIdList_KeyPress);
            // 
            // SelectTaskIdDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 124);
            this.Controls.Add(this.cmbTaskIdList);
            this.Controls.Add(this.lbSelectTaskId);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectTaskIdDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "任务ID选择";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectTaskIdDlg_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbSelectTaskId;
        private System.Windows.Forms.ComboBox cmbTaskIdList;
    }
}
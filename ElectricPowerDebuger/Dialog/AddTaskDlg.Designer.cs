namespace ElectricPowerDebuger.Dialog
{
    partial class AddTaskDlg
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
            this.gbPriority = new System.Windows.Forms.GroupBox();
            this.rbLevel3 = new System.Windows.Forms.RadioButton();
            this.rbLevel2 = new System.Windows.Forms.RadioButton();
            this.rbLevel1 = new System.Windows.Forms.RadioButton();
            this.rbLevel0 = new System.Windows.Forms.RadioButton();
            this.gbAck = new System.Windows.Forms.GroupBox();
            this.rbNoAck = new System.Windows.Forms.RadioButton();
            this.rbNeedAck = new System.Windows.Forms.RadioButton();
            this.gbOutTime = new System.Windows.Forms.GroupBox();
            this.lbUnit = new System.Windows.Forms.Label();
            this.nudOutTime = new System.Windows.Forms.NumericUpDown();
            this.gbTask = new System.Windows.Forms.GroupBox();
            this.lbNotice = new System.Windows.Forms.Label();
            this.rtbTaskContent = new System.Windows.Forms.RichTextBox();
            this.cbTaskType = new System.Windows.Forms.ComboBox();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.rbMultiTaskOne = new System.Windows.Forms.RadioButton();
            this.rbMultiTaskSelected = new System.Windows.Forms.RadioButton();
            this.gbMultiTask = new System.Windows.Forms.GroupBox();
            this.rbMultiTaskAll = new System.Windows.Forms.RadioButton();
            this.gbPriority.SuspendLayout();
            this.gbAck.SuspendLayout();
            this.gbOutTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOutTime)).BeginInit();
            this.gbTask.SuspendLayout();
            this.gbMultiTask.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPriority
            // 
            this.gbPriority.Controls.Add(this.rbLevel3);
            this.gbPriority.Controls.Add(this.rbLevel2);
            this.gbPriority.Controls.Add(this.rbLevel1);
            this.gbPriority.Controls.Add(this.rbLevel0);
            this.gbPriority.Location = new System.Drawing.Point(12, 12);
            this.gbPriority.Name = "gbPriority";
            this.gbPriority.Size = new System.Drawing.Size(329, 47);
            this.gbPriority.TabIndex = 0;
            this.gbPriority.TabStop = false;
            this.gbPriority.Text = "优先级";
            // 
            // rbLevel3
            // 
            this.rbLevel3.AutoSize = true;
            this.rbLevel3.Location = new System.Drawing.Point(252, 20);
            this.rbLevel3.Name = "rbLevel3";
            this.rbLevel3.Size = new System.Drawing.Size(41, 16);
            this.rbLevel3.TabIndex = 3;
            this.rbLevel3.Text = "3级";
            this.rbLevel3.UseVisualStyleBackColor = true;
            // 
            // rbLevel2
            // 
            this.rbLevel2.AutoSize = true;
            this.rbLevel2.Checked = true;
            this.rbLevel2.Location = new System.Drawing.Point(176, 20);
            this.rbLevel2.Name = "rbLevel2";
            this.rbLevel2.Size = new System.Drawing.Size(41, 16);
            this.rbLevel2.TabIndex = 2;
            this.rbLevel2.TabStop = true;
            this.rbLevel2.Text = "2级";
            this.rbLevel2.UseVisualStyleBackColor = true;
            // 
            // rbLevel1
            // 
            this.rbLevel1.AutoSize = true;
            this.rbLevel1.Location = new System.Drawing.Point(100, 20);
            this.rbLevel1.Name = "rbLevel1";
            this.rbLevel1.Size = new System.Drawing.Size(41, 16);
            this.rbLevel1.TabIndex = 1;
            this.rbLevel1.Text = "1级";
            this.rbLevel1.UseVisualStyleBackColor = true;
            // 
            // rbLevel0
            // 
            this.rbLevel0.AutoSize = true;
            this.rbLevel0.Location = new System.Drawing.Point(24, 20);
            this.rbLevel0.Name = "rbLevel0";
            this.rbLevel0.Size = new System.Drawing.Size(41, 16);
            this.rbLevel0.TabIndex = 0;
            this.rbLevel0.Text = "0级";
            this.rbLevel0.UseVisualStyleBackColor = true;
            // 
            // gbAck
            // 
            this.gbAck.Controls.Add(this.rbNoAck);
            this.gbAck.Controls.Add(this.rbNeedAck);
            this.gbAck.Location = new System.Drawing.Point(12, 65);
            this.gbAck.Name = "gbAck";
            this.gbAck.Size = new System.Drawing.Size(452, 47);
            this.gbAck.TabIndex = 1;
            this.gbAck.TabStop = false;
            this.gbAck.Text = "应答设置";
            // 
            // rbNoAck
            // 
            this.rbNoAck.AutoSize = true;
            this.rbNoAck.Location = new System.Drawing.Point(22, 20);
            this.rbNoAck.Name = "rbNoAck";
            this.rbNoAck.Size = new System.Drawing.Size(179, 16);
            this.rbNoAck.TabIndex = 2;
            this.rbNoAck.Text = "不需要应答，如广播校时任务";
            this.rbNoAck.UseVisualStyleBackColor = true;
            // 
            // rbNeedAck
            // 
            this.rbNeedAck.AutoSize = true;
            this.rbNeedAck.Checked = true;
            this.rbNeedAck.Location = new System.Drawing.Point(249, 20);
            this.rbNeedAck.Name = "rbNeedAck";
            this.rbNeedAck.Size = new System.Drawing.Size(143, 16);
            this.rbNeedAck.TabIndex = 1;
            this.rbNeedAck.TabStop = true;
            this.rbNeedAck.Text = "需要应答，如抄表任务";
            this.rbNeedAck.UseVisualStyleBackColor = true;
            // 
            // gbOutTime
            // 
            this.gbOutTime.Controls.Add(this.lbUnit);
            this.gbOutTime.Controls.Add(this.nudOutTime);
            this.gbOutTime.Location = new System.Drawing.Point(12, 121);
            this.gbOutTime.Name = "gbOutTime";
            this.gbOutTime.Size = new System.Drawing.Size(452, 52);
            this.gbOutTime.TabIndex = 2;
            this.gbOutTime.TabStop = false;
            this.gbOutTime.Text = "超时时间";
            // 
            // lbUnit
            // 
            this.lbUnit.AutoSize = true;
            this.lbUnit.Location = new System.Drawing.Point(184, 24);
            this.lbUnit.Name = "lbUnit";
            this.lbUnit.Size = new System.Drawing.Size(17, 12);
            this.lbUnit.TabIndex = 1;
            this.lbUnit.Text = "秒";
            // 
            // nudOutTime
            // 
            this.nudOutTime.Location = new System.Drawing.Point(25, 20);
            this.nudOutTime.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudOutTime.Minimum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudOutTime.Name = "nudOutTime";
            this.nudOutTime.Size = new System.Drawing.Size(150, 21);
            this.nudOutTime.TabIndex = 0;
            this.nudOutTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudOutTime.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.nudOutTime.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // gbTask
            // 
            this.gbTask.Controls.Add(this.lbNotice);
            this.gbTask.Controls.Add(this.rtbTaskContent);
            this.gbTask.Controls.Add(this.cbTaskType);
            this.gbTask.Location = new System.Drawing.Point(12, 179);
            this.gbTask.Name = "gbTask";
            this.gbTask.Size = new System.Drawing.Size(452, 103);
            this.gbTask.TabIndex = 3;
            this.gbTask.TabStop = false;
            this.gbTask.Text = "任务内容";
            // 
            // lbNotice
            // 
            this.lbNotice.Font = new System.Drawing.Font("宋体", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbNotice.ForeColor = System.Drawing.Color.Red;
            this.lbNotice.Location = new System.Drawing.Point(193, 24);
            this.lbNotice.Name = "lbNotice";
            this.lbNotice.Size = new System.Drawing.Size(259, 27);
            this.lbNotice.TabIndex = 2;
            this.lbNotice.Text = "请在下面中输入以空格隔开的645命令：[ 控制字、数据单元长度、数据单元 ]";
            // 
            // rtbTaskContent
            // 
            this.rtbTaskContent.Enabled = false;
            this.rtbTaskContent.Location = new System.Drawing.Point(24, 54);
            this.rtbTaskContent.Name = "rtbTaskContent";
            this.rtbTaskContent.Size = new System.Drawing.Size(416, 39);
            this.rtbTaskContent.TabIndex = 1;
            this.rtbTaskContent.Text = "";
            this.rtbTaskContent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.rtbTaskContent_KeyPress);
            // 
            // cbTaskType
            // 
            this.cbTaskType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTaskType.FormattingEnabled = true;
            this.cbTaskType.Items.AddRange(new object[] {
            "抄读正向有功",
            "自定义抄读数据"});
            this.cbTaskType.Location = new System.Drawing.Point(24, 24);
            this.cbTaskType.Name = "cbTaskType";
            this.cbTaskType.Size = new System.Drawing.Size(151, 20);
            this.cbTaskType.TabIndex = 0;
            this.cbTaskType.SelectedIndexChanged += new System.EventHandler(this.cbTaskType_SelectedIndexChanged);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(37, 352);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 23);
            this.btOK.TabIndex = 4;
            this.btOK.Text = "确定";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(389, 352);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // rbMultiTaskOne
            // 
            this.rbMultiTaskOne.AutoSize = true;
            this.rbMultiTaskOne.Checked = true;
            this.rbMultiTaskOne.Location = new System.Drawing.Point(22, 23);
            this.rbMultiTaskOne.Name = "rbMultiTaskOne";
            this.rbMultiTaskOne.Size = new System.Drawing.Size(47, 16);
            this.rbMultiTaskOne.TabIndex = 1;
            this.rbMultiTaskOne.TabStop = true;
            this.rbMultiTaskOne.Text = "单播";
            this.rbMultiTaskOne.UseVisualStyleBackColor = true;
            // 
            // rbMultiTaskSelected
            // 
            this.rbMultiTaskSelected.AutoSize = true;
            this.rbMultiTaskSelected.Location = new System.Drawing.Point(118, 23);
            this.rbMultiTaskSelected.Name = "rbMultiTaskSelected";
            this.rbMultiTaskSelected.Size = new System.Drawing.Size(83, 16);
            this.rbMultiTaskSelected.TabIndex = 2;
            this.rbMultiTaskSelected.Text = "多播已选择";
            this.rbMultiTaskSelected.UseVisualStyleBackColor = true;
            // 
            // gbMultiTask
            // 
            this.gbMultiTask.Controls.Add(this.rbMultiTaskOne);
            this.gbMultiTask.Controls.Add(this.rbMultiTaskAll);
            this.gbMultiTask.Controls.Add(this.rbMultiTaskSelected);
            this.gbMultiTask.Location = new System.Drawing.Point(12, 288);
            this.gbMultiTask.Name = "gbMultiTask";
            this.gbMultiTask.Size = new System.Drawing.Size(452, 47);
            this.gbMultiTask.TabIndex = 6;
            this.gbMultiTask.TabStop = false;
            this.gbMultiTask.Text = "单播/多播任务";
            // 
            // rbMultiTaskAll
            // 
            this.rbMultiTaskAll.AutoSize = true;
            this.rbMultiTaskAll.Location = new System.Drawing.Point(249, 23);
            this.rbMultiTaskAll.Name = "rbMultiTaskAll";
            this.rbMultiTaskAll.Size = new System.Drawing.Size(71, 16);
            this.rbMultiTaskAll.TabIndex = 3;
            this.rbMultiTaskAll.Text = "多播所有";
            this.rbMultiTaskAll.UseVisualStyleBackColor = true;
            // 
            // AddTaskDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 397);
            this.Controls.Add(this.gbMultiTask);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.gbTask);
            this.Controls.Add(this.gbOutTime);
            this.Controls.Add(this.gbAck);
            this.Controls.Add(this.gbPriority);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddTaskDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "任务参数设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AddTaskDlg_FormClosed);
            this.Load += new System.EventHandler(this.AddTaskDlg_Load);
            this.gbPriority.ResumeLayout(false);
            this.gbPriority.PerformLayout();
            this.gbAck.ResumeLayout(false);
            this.gbAck.PerformLayout();
            this.gbOutTime.ResumeLayout(false);
            this.gbOutTime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOutTime)).EndInit();
            this.gbTask.ResumeLayout(false);
            this.gbMultiTask.ResumeLayout(false);
            this.gbMultiTask.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbPriority;
        private System.Windows.Forms.RadioButton rbLevel3;
        private System.Windows.Forms.RadioButton rbLevel2;
        private System.Windows.Forms.RadioButton rbLevel1;
        private System.Windows.Forms.RadioButton rbLevel0;
        private System.Windows.Forms.GroupBox gbAck;
        private System.Windows.Forms.RadioButton rbNoAck;
        private System.Windows.Forms.RadioButton rbNeedAck;
        private System.Windows.Forms.GroupBox gbOutTime;
        private System.Windows.Forms.Label lbUnit;
        private System.Windows.Forms.NumericUpDown nudOutTime;
        private System.Windows.Forms.GroupBox gbTask;
        private System.Windows.Forms.RichTextBox rtbTaskContent;
        private System.Windows.Forms.ComboBox cbTaskType;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbNotice;
        private System.Windows.Forms.RadioButton rbMultiTaskOne;
        private System.Windows.Forms.RadioButton rbMultiTaskSelected;
        private System.Windows.Forms.GroupBox gbMultiTask;
        private System.Windows.Forms.RadioButton rbMultiTaskAll;

    }
}
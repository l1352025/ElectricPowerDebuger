namespace ElectricPowerDebuger.Function
{
    partial class LogManager
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

            if(this.scom.IsOpen)
            {
                this.scom.Close();
            }

            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpNetCnt = new System.Windows.Forms.GroupBox();
            this.treeNetCnt = new System.Windows.Forms.TreeView();
            this.grpLogMgr = new System.Windows.Forms.GroupBox();
            this.btLogLoad = new System.Windows.Forms.Button();
            this.btLogSave = new System.Windows.Forms.Button();
            this.btLogRead = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rbReadByHour = new System.Windows.Forms.RadioButton();
            this.rbReadByDay = new System.Windows.Forms.RadioButton();
            this.rbReadByMth = new System.Windows.Forms.RadioButton();
            this.chkListHour = new System.Windows.Forms.CheckedListBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.grpPortSet = new System.Windows.Forms.GroupBox();
            this.btPortCtrl = new System.Windows.Forms.Button();
            this.combPortChk = new System.Windows.Forms.ComboBox();
            this.combPortBaud = new System.Windows.Forms.ComboBox();
            this.combPortNum = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.listView1 = new System.Windows.Forms.ListView();
            this.序号 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.日期 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.时间 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.日志类型 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.日志数据 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.rtbLogText = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.treeProtocol = new System.Windows.Forms.TreeView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpNetCnt.SuspendLayout();
            this.grpLogMgr.SuspendLayout();
            this.grpPortSet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpNetCnt);
            this.splitContainer1.Panel1.Controls.Add(this.grpLogMgr);
            this.splitContainer1.Panel1.Controls.Add(this.grpPortSet);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1105, 563);
            this.splitContainer1.SplitterDistance = 308;
            this.splitContainer1.TabIndex = 0;
            // 
            // grpNetCnt
            // 
            this.grpNetCnt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpNetCnt.Controls.Add(this.treeNetCnt);
            this.grpNetCnt.Location = new System.Drawing.Point(-1, 277);
            this.grpNetCnt.Name = "grpNetCnt";
            this.grpNetCnt.Size = new System.Drawing.Size(307, 281);
            this.grpNetCnt.TabIndex = 0;
            this.grpNetCnt.TabStop = false;
            this.grpNetCnt.Text = "组网统计";
            // 
            // treeNetCnt
            // 
            this.treeNetCnt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeNetCnt.Location = new System.Drawing.Point(1, 17);
            this.treeNetCnt.Margin = new System.Windows.Forms.Padding(0);
            this.treeNetCnt.Name = "treeNetCnt";
            this.treeNetCnt.Size = new System.Drawing.Size(306, 267);
            this.treeNetCnt.TabIndex = 0;
            // 
            // grpLogMgr
            // 
            this.grpLogMgr.Controls.Add(this.btLogLoad);
            this.grpLogMgr.Controls.Add(this.btLogSave);
            this.grpLogMgr.Controls.Add(this.btLogRead);
            this.grpLogMgr.Controls.Add(this.label2);
            this.grpLogMgr.Controls.Add(this.rbReadByHour);
            this.grpLogMgr.Controls.Add(this.rbReadByDay);
            this.grpLogMgr.Controls.Add(this.rbReadByMth);
            this.grpLogMgr.Controls.Add(this.chkListHour);
            this.grpLogMgr.Controls.Add(this.dateTimePicker1);
            this.grpLogMgr.Location = new System.Drawing.Point(3, 63);
            this.grpLogMgr.Name = "grpLogMgr";
            this.grpLogMgr.Size = new System.Drawing.Size(300, 196);
            this.grpLogMgr.TabIndex = 0;
            this.grpLogMgr.TabStop = false;
            this.grpLogMgr.Text = "日志管理";
            // 
            // btLogLoad
            // 
            this.btLogLoad.Location = new System.Drawing.Point(229, 173);
            this.btLogLoad.Name = "btLogLoad";
            this.btLogLoad.Size = new System.Drawing.Size(65, 23);
            this.btLogLoad.TabIndex = 2;
            this.btLogLoad.Text = "导入日志";
            this.btLogLoad.UseVisualStyleBackColor = true;
            this.btLogLoad.Click += new System.EventHandler(this.btLogLoad_Click);
            // 
            // btLogSave
            // 
            this.btLogSave.Location = new System.Drawing.Point(160, 173);
            this.btLogSave.Name = "btLogSave";
            this.btLogSave.Size = new System.Drawing.Size(63, 23);
            this.btLogSave.TabIndex = 2;
            this.btLogSave.Text = "保存日志";
            this.btLogSave.UseVisualStyleBackColor = true;
            this.btLogSave.Click += new System.EventHandler(this.btLogSave_Click);
            // 
            // btLogRead
            // 
            this.btLogRead.Location = new System.Drawing.Point(79, 173);
            this.btLogRead.Name = "btLogRead";
            this.btLogRead.Size = new System.Drawing.Size(62, 23);
            this.btLogRead.TabIndex = 2;
            this.btLogRead.Text = "读取日志";
            this.btLogRead.UseVisualStyleBackColor = true;
            this.btLogRead.Click += new System.EventHandler(this.btLogRead_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "读取规则";
            // 
            // rbReadByHour
            // 
            this.rbReadByHour.AutoSize = true;
            this.rbReadByHour.Checked = true;
            this.rbReadByHour.Location = new System.Drawing.Point(79, 67);
            this.rbReadByHour.Name = "rbReadByHour";
            this.rbReadByHour.Size = new System.Drawing.Size(47, 16);
            this.rbReadByHour.TabIndex = 2;
            this.rbReadByHour.TabStop = true;
            this.rbReadByHour.Text = "按时";
            this.rbReadByHour.UseVisualStyleBackColor = true;
            this.rbReadByHour.CheckedChanged += new System.EventHandler(this.rbReadByHour_CheckedChanged);
            // 
            // rbReadByDay
            // 
            this.rbReadByDay.AutoSize = true;
            this.rbReadByDay.Location = new System.Drawing.Point(79, 45);
            this.rbReadByDay.Name = "rbReadByDay";
            this.rbReadByDay.Size = new System.Drawing.Size(47, 16);
            this.rbReadByDay.TabIndex = 2;
            this.rbReadByDay.Text = "按日";
            this.rbReadByDay.UseVisualStyleBackColor = true;
            // 
            // rbReadByMth
            // 
            this.rbReadByMth.AutoSize = true;
            this.rbReadByMth.Location = new System.Drawing.Point(79, 23);
            this.rbReadByMth.Name = "rbReadByMth";
            this.rbReadByMth.Size = new System.Drawing.Size(47, 16);
            this.rbReadByMth.TabIndex = 2;
            this.rbReadByMth.Text = "按月";
            this.rbReadByMth.UseVisualStyleBackColor = true;
            // 
            // chkListHour
            // 
            this.chkListHour.CheckOnClick = true;
            this.chkListHour.FormattingEnabled = true;
            this.chkListHour.Items.AddRange(new object[] {
            "00 时",
            "01 时",
            "02 时",
            "03 时",
            "04 时",
            "05 时",
            "06 时",
            "07 时",
            "08 时",
            "09 时",
            "10 时",
            "11 时",
            "12 时",
            "13 时",
            "14 时",
            "15 时",
            "16 时",
            "17 时",
            "18 时",
            "19 时",
            "20 时",
            "21 时",
            "22 时",
            "23 时"});
            this.chkListHour.Location = new System.Drawing.Point(132, 47);
            this.chkListHour.Name = "chkListHour";
            this.chkListHour.Size = new System.Drawing.Size(106, 100);
            this.chkListHour.TabIndex = 1;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(132, 20);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(106, 21);
            this.dateTimePicker1.TabIndex = 0;
            // 
            // grpPortSet
            // 
            this.grpPortSet.Controls.Add(this.btPortCtrl);
            this.grpPortSet.Controls.Add(this.combPortChk);
            this.grpPortSet.Controls.Add(this.combPortBaud);
            this.grpPortSet.Controls.Add(this.combPortNum);
            this.grpPortSet.Controls.Add(this.label1);
            this.grpPortSet.Location = new System.Drawing.Point(3, 7);
            this.grpPortSet.Name = "grpPortSet";
            this.grpPortSet.Size = new System.Drawing.Size(300, 50);
            this.grpPortSet.TabIndex = 0;
            this.grpPortSet.TabStop = false;
            this.grpPortSet.Text = "端口设置";
            // 
            // btPortCtrl
            // 
            this.btPortCtrl.BackColor = System.Drawing.Color.Silver;
            this.btPortCtrl.Location = new System.Drawing.Point(236, 18);
            this.btPortCtrl.Name = "btPortCtrl";
            this.btPortCtrl.Size = new System.Drawing.Size(58, 23);
            this.btPortCtrl.TabIndex = 2;
            this.btPortCtrl.Text = "打开";
            this.btPortCtrl.UseVisualStyleBackColor = false;
            this.btPortCtrl.Click += new System.EventHandler(this.btPortCtrl_Click);
            // 
            // combPortChk
            // 
            this.combPortChk.FormattingEnabled = true;
            this.combPortChk.Items.AddRange(new object[] {
            "8N1",
            "8E1",
            "8O1"});
            this.combPortChk.Location = new System.Drawing.Point(185, 20);
            this.combPortChk.Name = "combPortChk";
            this.combPortChk.Size = new System.Drawing.Size(45, 20);
            this.combPortChk.TabIndex = 1;
            // 
            // combPortBaud
            // 
            this.combPortBaud.FormattingEnabled = true;
            this.combPortBaud.Items.AddRange(new object[] {
            "9600",
            "19200",
            "115200"});
            this.combPortBaud.Location = new System.Drawing.Point(116, 20);
            this.combPortBaud.Name = "combPortBaud";
            this.combPortBaud.Size = new System.Drawing.Size(63, 20);
            this.combPortBaud.TabIndex = 1;
            // 
            // combPortNum
            // 
            this.combPortNum.FormattingEnabled = true;
            this.combPortNum.Items.AddRange(new object[] {
            "COM15"});
            this.combPortNum.Location = new System.Drawing.Point(57, 20);
            this.combPortNum.Name = "combPortNum";
            this.combPortNum.Size = new System.Drawing.Size(53, 20);
            this.combPortNum.TabIndex = 1;
            this.combPortNum.Click += new System.EventHandler(this.combPortNum_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "端口";
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Panel2.Controls.Add(this.treeProtocol);
            this.splitContainer2.Size = new System.Drawing.Size(793, 563);
            this.splitContainer2.SplitterDistance = 569;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.listView1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.rtbLogText);
            this.splitContainer3.Size = new System.Drawing.Size(569, 563);
            this.splitContainer3.SplitterDistance = 444;
            this.splitContainer3.TabIndex = 0;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.BackColor = System.Drawing.SystemColors.Control;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.序号,
            this.日期,
            this.时间,
            this.日志类型,
            this.日志数据});
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(567, 442);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // 序号
            // 
            this.序号.Text = "序号";
            // 
            // 日期
            // 
            this.日期.Text = "日期";
            // 
            // 时间
            // 
            this.时间.Text = "时间";
            // 
            // 日志类型
            // 
            this.日志类型.Text = "日志类型";
            // 
            // 日志数据
            // 
            this.日志数据.Text = "日志数据";
            // 
            // rtbLogText
            // 
            this.rtbLogText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbLogText.Location = new System.Drawing.Point(-3, 0);
            this.rtbLogText.Name = "rtbLogText";
            this.rtbLogText.Size = new System.Drawing.Size(571, 111);
            this.rtbLogText.TabIndex = 0;
            this.rtbLogText.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "协议解析";
            // 
            // treeProtocol
            // 
            this.treeProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeProtocol.Location = new System.Drawing.Point(0, 25);
            this.treeProtocol.Name = "treeProtocol";
            this.treeProtocol.Size = new System.Drawing.Size(218, 537);
            this.treeProtocol.TabIndex = 1;
            // 
            // LogManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.splitContainer1);
            this.Name = "LogManager";
            this.Size = new System.Drawing.Size(1105, 563);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpNetCnt.ResumeLayout(false);
            this.grpLogMgr.ResumeLayout(false);
            this.grpLogMgr.PerformLayout();
            this.grpPortSet.ResumeLayout(false);
            this.grpPortSet.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox grpLogMgr;
        private System.Windows.Forms.GroupBox grpPortSet;
        private System.Windows.Forms.GroupBox grpNetCnt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbReadByHour;
        private System.Windows.Forms.RadioButton rbReadByDay;
        private System.Windows.Forms.RadioButton rbReadByMth;
        private System.Windows.Forms.CheckedListBox chkListHour;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button btPortCtrl;
        private System.Windows.Forms.ComboBox combPortChk;
        private System.Windows.Forms.ComboBox combPortBaud;
        private System.Windows.Forms.ComboBox combPortNum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btLogLoad;
        private System.Windows.Forms.Button btLogSave;
        private System.Windows.Forms.Button btLogRead;
        private System.Windows.Forms.TreeView treeNetCnt;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader 序号;
        private System.Windows.Forms.ColumnHeader 日期;
        private System.Windows.Forms.ColumnHeader 时间;
        private System.Windows.Forms.ColumnHeader 日志类型;
        private System.Windows.Forms.ColumnHeader 日志数据;
        private System.Windows.Forms.RichTextBox rtbLogText;
        private System.Windows.Forms.TreeView treeProtocol;
        private System.Windows.Forms.Label label3;
    }
}

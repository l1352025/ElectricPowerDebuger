namespace ElectricPowerDebuger.Function
{
    partial class ConcSimulator
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btStopRegister = new System.Windows.Forms.Button();
            this.btStartRegister = new System.Windows.Forms.Button();
            this.btDisableEventReport = new System.Windows.Forms.Button();
            this.btEnableEventReport = new System.Windows.Forms.Button();
            this.btSetConcAddr = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btReadFatherNode = new System.Windows.Forms.Button();
            this.btReadSubnodeRegProgress = new System.Windows.Forms.Button();
            this.btReadSubnodeInfo = new System.Windows.Forms.Button();
            this.btReadSubnodeCount = new System.Windows.Forms.Button();
            this.btReadComDelayTime = new System.Windows.Forms.Button();
            this.btReadAddress = new System.Windows.Forms.Button();
            this.btModuleWorkMode = new System.Windows.Forms.Button();
            this.btVersionInfo = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button10 = new System.Windows.Forms.Button();
            this.gpbTask = new System.Windows.Forms.GroupBox();
            this.btReadUndoTaskInfo = new System.Windows.Forms.Button();
            this.btPauseTask = new System.Windows.Forms.Button();
            this.btStartTask = new System.Windows.Forms.Button();
            this.btReadRemainTask = new System.Windows.Forms.Button();
            this.btReadUndoTaskTab = new System.Windows.Forms.Button();
            this.btReadUndoTaskNum = new System.Windows.Forms.Button();
            this.gpbInitial = new System.Windows.Forms.GroupBox();
            this.btInitialTask = new System.Windows.Forms.Button();
            this.btInitialDocument = new System.Windows.Forms.Button();
            this.btResetDevice = new System.Windows.Forms.Button();
            this.gpbComSelect = new System.Windows.Forms.GroupBox();
            this.btOpenPort = new System.Windows.Forms.Button();
            this.cmbBaudrate = new System.Windows.Forms.ComboBox();
            this.cmbPort = new System.Windows.Forms.ComboBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lbConcAddr = new System.Windows.Forms.Label();
            this.dgvDocument = new System.Windows.Forms.DataGridView();
            this.SN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.表具地址 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.任务ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.优先级 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.状态 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.结果 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sNDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.表具地址DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.任务IDDataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.优先级DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.状态DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.结果DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cnMenuDocument = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAddTask = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelTask = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiClearDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiImportFromDevice = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExportToDevice = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLoadDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSaveDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiAddSubNode = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelSubNode = new System.Windows.Forms.ToolStripMenuItem();
            this.dsDocument = new System.Data.DataSet();
            this.tbDocument = new System.Data.DataTable();
            this.SerialNo = new System.Data.DataColumn();
            this.Address = new System.Data.DataColumn();
            this.TaskId = new System.Data.DataColumn();
            this.TaskPriority = new System.Data.DataColumn();
            this.TaskStatus = new System.Data.DataColumn();
            this.TaskResult = new System.Data.DataColumn();
            this.lbDocument = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pgbCapactity = new System.Windows.Forms.ProgressBar();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rtbCommMsg = new System.Windows.Forms.RichTextBox();
            this.cnMenuComm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAutoScrollCommMsg = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiClearAllCommMsg = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSaveCommMsgToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.sNDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.表具地址DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.任务IDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.优先级DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.状态DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.结果DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.timerConcSim = new System.Windows.Forms.Timer(this.components);
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDlg = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gpbTask.SuspendLayout();
            this.gpbInitial.SuspendLayout();
            this.gpbComSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocument)).BeginInit();
            this.cnMenuDocument.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dsDocument)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbDocument)).BeginInit();
            this.cnMenuComm.SuspendLayout();
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
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.gpbTask);
            this.splitContainer1.Panel1.Controls.Add(this.gpbInitial);
            this.splitContainer1.Panel1.Controls.Add(this.gpbComSelect);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1342, 682);
            this.splitContainer1.SplitterDistance = 373;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btStopRegister);
            this.groupBox3.Controls.Add(this.btStartRegister);
            this.groupBox3.Controls.Add(this.btDisableEventReport);
            this.groupBox3.Controls.Add(this.btEnableEventReport);
            this.groupBox3.Controls.Add(this.btSetConcAddr);
            this.groupBox3.Location = new System.Drawing.Point(9, 400);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(350, 117);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "写参数";
            // 
            // btStopRegister
            // 
            this.btStopRegister.Location = new System.Drawing.Point(182, 80);
            this.btStopRegister.Name = "btStopRegister";
            this.btStopRegister.Size = new System.Drawing.Size(150, 23);
            this.btStopRegister.TabIndex = 11;
            this.btStopRegister.Text = "终止从节点主动注册";
            this.btStopRegister.UseVisualStyleBackColor = true;
            this.btStopRegister.Click += new System.EventHandler(this.btStopRegister_Click);
            // 
            // btStartRegister
            // 
            this.btStartRegister.Location = new System.Drawing.Point(18, 80);
            this.btStartRegister.Name = "btStartRegister";
            this.btStartRegister.Size = new System.Drawing.Size(150, 23);
            this.btStartRegister.TabIndex = 12;
            this.btStartRegister.Text = "激活从节点主动注册";
            this.btStartRegister.UseVisualStyleBackColor = true;
            this.btStartRegister.Click += new System.EventHandler(this.btStartRegister_Click);
            // 
            // btDisableEventReport
            // 
            this.btDisableEventReport.Location = new System.Drawing.Point(182, 49);
            this.btDisableEventReport.Name = "btDisableEventReport";
            this.btDisableEventReport.Size = new System.Drawing.Size(150, 23);
            this.btDisableEventReport.TabIndex = 9;
            this.btDisableEventReport.Text = "禁止事件上报";
            this.btDisableEventReport.UseVisualStyleBackColor = true;
            this.btDisableEventReport.Click += new System.EventHandler(this.btDisableEventReport_Click);
            // 
            // btEnableEventReport
            // 
            this.btEnableEventReport.Location = new System.Drawing.Point(18, 49);
            this.btEnableEventReport.Name = "btEnableEventReport";
            this.btEnableEventReport.Size = new System.Drawing.Size(150, 23);
            this.btEnableEventReport.TabIndex = 8;
            this.btEnableEventReport.Text = "允许事件上报";
            this.btEnableEventReport.UseVisualStyleBackColor = true;
            this.btEnableEventReport.Click += new System.EventHandler(this.btEnableEventReport_Click);
            // 
            // btSetConcAddr
            // 
            this.btSetConcAddr.Location = new System.Drawing.Point(18, 21);
            this.btSetConcAddr.Name = "btSetConcAddr";
            this.btSetConcAddr.Size = new System.Drawing.Size(150, 23);
            this.btSetConcAddr.TabIndex = 8;
            this.btSetConcAddr.Text = "设置主节点地址";
            this.btSetConcAddr.UseVisualStyleBackColor = true;
            this.btSetConcAddr.Click += new System.EventHandler(this.btSetConcAddr_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btReadFatherNode);
            this.groupBox2.Controls.Add(this.btReadSubnodeRegProgress);
            this.groupBox2.Controls.Add(this.btReadSubnodeInfo);
            this.groupBox2.Controls.Add(this.btReadSubnodeCount);
            this.groupBox2.Controls.Add(this.btReadComDelayTime);
            this.groupBox2.Controls.Add(this.btReadAddress);
            this.groupBox2.Controls.Add(this.btModuleWorkMode);
            this.groupBox2.Controls.Add(this.btVersionInfo);
            this.groupBox2.Location = new System.Drawing.Point(9, 247);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(350, 150);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "读参数";
            // 
            // btReadFatherNode
            // 
            this.btReadFatherNode.Location = new System.Drawing.Point(182, 114);
            this.btReadFatherNode.Name = "btReadFatherNode";
            this.btReadFatherNode.Size = new System.Drawing.Size(150, 23);
            this.btReadFatherNode.TabIndex = 13;
            this.btReadFatherNode.Text = "查询从节点的父节点";
            this.btReadFatherNode.UseVisualStyleBackColor = true;
            this.btReadFatherNode.Click += new System.EventHandler(this.btReadFatherNode_Click);
            // 
            // btReadSubnodeRegProgress
            // 
            this.btReadSubnodeRegProgress.Location = new System.Drawing.Point(18, 114);
            this.btReadSubnodeRegProgress.Name = "btReadSubnodeRegProgress";
            this.btReadSubnodeRegProgress.Size = new System.Drawing.Size(150, 23);
            this.btReadSubnodeRegProgress.TabIndex = 10;
            this.btReadSubnodeRegProgress.Text = "查询从节点主动注册进度";
            this.btReadSubnodeRegProgress.UseVisualStyleBackColor = true;
            this.btReadSubnodeRegProgress.Click += new System.EventHandler(this.btReadSubnodeRegProgress_Click);
            // 
            // btReadSubnodeInfo
            // 
            this.btReadSubnodeInfo.Location = new System.Drawing.Point(182, 83);
            this.btReadSubnodeInfo.Name = "btReadSubnodeInfo";
            this.btReadSubnodeInfo.Size = new System.Drawing.Size(150, 23);
            this.btReadSubnodeInfo.TabIndex = 12;
            this.btReadSubnodeInfo.Text = "查询从节点信息";
            this.btReadSubnodeInfo.UseVisualStyleBackColor = true;
            this.btReadSubnodeInfo.Click += new System.EventHandler(this.btReadSubnodeInfo_Click);
            // 
            // btReadSubnodeCount
            // 
            this.btReadSubnodeCount.Location = new System.Drawing.Point(18, 83);
            this.btReadSubnodeCount.Name = "btReadSubnodeCount";
            this.btReadSubnodeCount.Size = new System.Drawing.Size(150, 23);
            this.btReadSubnodeCount.TabIndex = 12;
            this.btReadSubnodeCount.Text = "查询从节点数量";
            this.btReadSubnodeCount.UseVisualStyleBackColor = true;
            this.btReadSubnodeCount.Click += new System.EventHandler(this.btReadSubnodeCount_Click);
            // 
            // btReadComDelayTime
            // 
            this.btReadComDelayTime.Location = new System.Drawing.Point(182, 52);
            this.btReadComDelayTime.Name = "btReadComDelayTime";
            this.btReadComDelayTime.Size = new System.Drawing.Size(150, 23);
            this.btReadComDelayTime.TabIndex = 9;
            this.btReadComDelayTime.Text = "查询通信延时时长";
            this.btReadComDelayTime.UseVisualStyleBackColor = true;
            this.btReadComDelayTime.Click += new System.EventHandler(this.btReadComDelayTime_Click);
            // 
            // btReadAddress
            // 
            this.btReadAddress.Location = new System.Drawing.Point(18, 52);
            this.btReadAddress.Name = "btReadAddress";
            this.btReadAddress.Size = new System.Drawing.Size(150, 23);
            this.btReadAddress.TabIndex = 8;
            this.btReadAddress.Text = "查询主节点地址";
            this.btReadAddress.UseVisualStyleBackColor = true;
            this.btReadAddress.Click += new System.EventHandler(this.btReadAddress_Click);
            // 
            // btModuleWorkMode
            // 
            this.btModuleWorkMode.Location = new System.Drawing.Point(182, 21);
            this.btModuleWorkMode.Name = "btModuleWorkMode";
            this.btModuleWorkMode.Size = new System.Drawing.Size(150, 23);
            this.btModuleWorkMode.TabIndex = 8;
            this.btModuleWorkMode.Text = "本地通信模块运行模式";
            this.btModuleWorkMode.UseVisualStyleBackColor = true;
            this.btModuleWorkMode.Click += new System.EventHandler(this.btModuleWorkMode_Click);
            // 
            // btVersionInfo
            // 
            this.btVersionInfo.Location = new System.Drawing.Point(18, 21);
            this.btVersionInfo.Name = "btVersionInfo";
            this.btVersionInfo.Size = new System.Drawing.Size(150, 23);
            this.btVersionInfo.TabIndex = 8;
            this.btVersionInfo.Text = "厂商代码和版本信息";
            this.btVersionInfo.UseVisualStyleBackColor = true;
            this.btVersionInfo.Click += new System.EventHandler(this.btVersionInfo_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.button10);
            this.groupBox1.Location = new System.Drawing.Point(9, 518);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(350, 114);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "流程管理";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "模块识别流程",
            "档案同步流程",
            "任务执行流程",
            "容错机制流程",
            "主动注册流程",
            "文件传输流程"});
            this.comboBox1.Location = new System.Drawing.Point(18, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(149, 20);
            this.comboBox1.TabIndex = 12;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(182, 21);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(150, 23);
            this.button10.TabIndex = 8;
            this.button10.Text = "执行流程";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // gpbTask
            // 
            this.gpbTask.Controls.Add(this.btReadUndoTaskInfo);
            this.gpbTask.Controls.Add(this.btPauseTask);
            this.gpbTask.Controls.Add(this.btStartTask);
            this.gpbTask.Controls.Add(this.btReadRemainTask);
            this.gpbTask.Controls.Add(this.btReadUndoTaskTab);
            this.gpbTask.Controls.Add(this.btReadUndoTaskNum);
            this.gpbTask.Location = new System.Drawing.Point(9, 125);
            this.gpbTask.Name = "gpbTask";
            this.gpbTask.Size = new System.Drawing.Size(350, 119);
            this.gpbTask.TabIndex = 3;
            this.gpbTask.TabStop = false;
            this.gpbTask.Text = "管理任务";
            // 
            // btReadUndoTaskInfo
            // 
            this.btReadUndoTaskInfo.Location = new System.Drawing.Point(182, 52);
            this.btReadUndoTaskInfo.Name = "btReadUndoTaskInfo";
            this.btReadUndoTaskInfo.Size = new System.Drawing.Size(150, 23);
            this.btReadUndoTaskInfo.TabIndex = 11;
            this.btReadUndoTaskInfo.Text = "查询未完成任务信息";
            this.btReadUndoTaskInfo.UseVisualStyleBackColor = true;
            this.btReadUndoTaskInfo.Click += new System.EventHandler(this.btReadUndoTaskInfo_Click);
            // 
            // btPauseTask
            // 
            this.btPauseTask.Location = new System.Drawing.Point(182, 83);
            this.btPauseTask.Name = "btPauseTask";
            this.btPauseTask.Size = new System.Drawing.Size(150, 23);
            this.btPauseTask.TabIndex = 10;
            this.btPauseTask.Text = "暂停任务";
            this.btPauseTask.UseVisualStyleBackColor = true;
            this.btPauseTask.Click += new System.EventHandler(this.btPauseTask_Click);
            // 
            // btStartTask
            // 
            this.btStartTask.Location = new System.Drawing.Point(18, 83);
            this.btStartTask.Name = "btStartTask";
            this.btStartTask.Size = new System.Drawing.Size(150, 23);
            this.btStartTask.TabIndex = 9;
            this.btStartTask.Text = "启动任务";
            this.btStartTask.UseVisualStyleBackColor = true;
            this.btStartTask.Click += new System.EventHandler(this.btStartTask_Click);
            // 
            // btReadRemainTask
            // 
            this.btReadRemainTask.Location = new System.Drawing.Point(182, 21);
            this.btReadRemainTask.Name = "btReadRemainTask";
            this.btReadRemainTask.Size = new System.Drawing.Size(150, 23);
            this.btReadRemainTask.TabIndex = 8;
            this.btReadRemainTask.Text = "查询可分配任务数";
            this.btReadRemainTask.UseVisualStyleBackColor = true;
            this.btReadRemainTask.Click += new System.EventHandler(this.btReadRemainTask_Click);
            // 
            // btReadUndoTaskTab
            // 
            this.btReadUndoTaskTab.Location = new System.Drawing.Point(18, 52);
            this.btReadUndoTaskTab.Name = "btReadUndoTaskTab";
            this.btReadUndoTaskTab.Size = new System.Drawing.Size(150, 23);
            this.btReadUndoTaskTab.TabIndex = 8;
            this.btReadUndoTaskTab.Text = "查询未完成任务列表";
            this.btReadUndoTaskTab.UseVisualStyleBackColor = true;
            this.btReadUndoTaskTab.Click += new System.EventHandler(this.btReadUndoTaskTab_Click);
            // 
            // btReadUndoTaskNum
            // 
            this.btReadUndoTaskNum.Location = new System.Drawing.Point(18, 21);
            this.btReadUndoTaskNum.Name = "btReadUndoTaskNum";
            this.btReadUndoTaskNum.Size = new System.Drawing.Size(150, 23);
            this.btReadUndoTaskNum.TabIndex = 8;
            this.btReadUndoTaskNum.Text = "查询未完成任务数";
            this.btReadUndoTaskNum.UseVisualStyleBackColor = true;
            this.btReadUndoTaskNum.Click += new System.EventHandler(this.btReadUndoTaskNum_Click);
            // 
            // gpbInitial
            // 
            this.gpbInitial.Controls.Add(this.btInitialTask);
            this.gpbInitial.Controls.Add(this.btInitialDocument);
            this.gpbInitial.Controls.Add(this.btResetDevice);
            this.gpbInitial.Location = new System.Drawing.Point(9, 65);
            this.gpbInitial.Name = "gpbInitial";
            this.gpbInitial.Size = new System.Drawing.Size(350, 57);
            this.gpbInitial.TabIndex = 2;
            this.gpbInitial.TabStop = false;
            this.gpbInitial.Text = "初始化模块";
            // 
            // btInitialTask
            // 
            this.btInitialTask.Location = new System.Drawing.Point(235, 21);
            this.btInitialTask.Name = "btInitialTask";
            this.btInitialTask.Size = new System.Drawing.Size(97, 23);
            this.btInitialTask.TabIndex = 8;
            this.btInitialTask.Text = "初始化任务";
            this.btInitialTask.UseVisualStyleBackColor = true;
            this.btInitialTask.Click += new System.EventHandler(this.btInitialTask_Click);
            // 
            // btInitialDocument
            // 
            this.btInitialDocument.Location = new System.Drawing.Point(126, 21);
            this.btInitialDocument.Name = "btInitialDocument";
            this.btInitialDocument.Size = new System.Drawing.Size(97, 23);
            this.btInitialDocument.TabIndex = 8;
            this.btInitialDocument.Text = "初始化档案";
            this.btInitialDocument.UseVisualStyleBackColor = true;
            this.btInitialDocument.Click += new System.EventHandler(this.btInitialDocument_Click);
            // 
            // btResetDevice
            // 
            this.btResetDevice.Location = new System.Drawing.Point(18, 21);
            this.btResetDevice.Name = "btResetDevice";
            this.btResetDevice.Size = new System.Drawing.Size(97, 23);
            this.btResetDevice.TabIndex = 8;
            this.btResetDevice.Text = "复位硬件";
            this.btResetDevice.UseVisualStyleBackColor = true;
            this.btResetDevice.Click += new System.EventHandler(this.btResetDevice_Click);
            // 
            // gpbComSelect
            // 
            this.gpbComSelect.Controls.Add(this.btOpenPort);
            this.gpbComSelect.Controls.Add(this.cmbBaudrate);
            this.gpbComSelect.Controls.Add(this.cmbPort);
            this.gpbComSelect.Location = new System.Drawing.Point(9, 6);
            this.gpbComSelect.Name = "gpbComSelect";
            this.gpbComSelect.Size = new System.Drawing.Size(350, 56);
            this.gpbComSelect.TabIndex = 1;
            this.gpbComSelect.TabStop = false;
            this.gpbComSelect.Text = "端口选择";
            // 
            // btOpenPort
            // 
            this.btOpenPort.BackColor = System.Drawing.Color.Silver;
            this.btOpenPort.Location = new System.Drawing.Point(235, 21);
            this.btOpenPort.Name = "btOpenPort";
            this.btOpenPort.Size = new System.Drawing.Size(97, 23);
            this.btOpenPort.TabIndex = 7;
            this.btOpenPort.Text = "打开端口";
            this.btOpenPort.UseVisualStyleBackColor = false;
            this.btOpenPort.Click += new System.EventHandler(this.btOpenPort_Click);
            // 
            // cmbBaudrate
            // 
            this.cmbBaudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBaudrate.FormattingEnabled = true;
            this.cmbBaudrate.Items.AddRange(new object[] {
            "9600",
            "115200"});
            this.cmbBaudrate.Location = new System.Drawing.Point(126, 22);
            this.cmbBaudrate.Name = "cmbBaudrate";
            this.cmbBaudrate.Size = new System.Drawing.Size(97, 20);
            this.cmbBaudrate.TabIndex = 6;
            // 
            // cmbPort
            // 
            this.cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPort.FormattingEnabled = true;
            this.cmbPort.Location = new System.Drawing.Point(18, 22);
            this.cmbPort.Name = "cmbPort";
            this.cmbPort.Size = new System.Drawing.Size(97, 20);
            this.cmbPort.TabIndex = 5;
            this.cmbPort.Click += new System.EventHandler(this.cmbPort_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lbConcAddr);
            this.splitContainer2.Panel1.Controls.Add(this.dgvDocument);
            this.splitContainer2.Panel1.Controls.Add(this.lbDocument);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label9);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.label8);
            this.splitContainer2.Panel2.Controls.Add(this.label5);
            this.splitContainer2.Panel2.Controls.Add(this.pgbCapactity);
            this.splitContainer2.Panel2.Controls.Add(this.label7);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Panel2.Controls.Add(this.rtbCommMsg);
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Size = new System.Drawing.Size(965, 682);
            this.splitContainer2.SplitterDistance = 376;
            this.splitContainer2.TabIndex = 0;
            // 
            // lbConcAddr
            // 
            this.lbConcAddr.BackColor = System.Drawing.SystemColors.Control;
            this.lbConcAddr.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbConcAddr.Location = new System.Drawing.Point(212, 0);
            this.lbConcAddr.Name = "lbConcAddr";
            this.lbConcAddr.Size = new System.Drawing.Size(154, 20);
            this.lbConcAddr.TabIndex = 1;
            this.lbConcAddr.Text = "主节点地址：未知";
            this.lbConcAddr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvDocument
            // 
            this.dgvDocument.AllowUserToAddRows = false;
            this.dgvDocument.AllowUserToDeleteRows = false;
            this.dgvDocument.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.dgvDocument.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDocument.AutoGenerateColumns = false;
            this.dgvDocument.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvDocument.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDocument.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDocument.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDocument.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SN,
            this.表具地址,
            this.任务ID,
            this.优先级,
            this.状态,
            this.结果,
            this.sNDataGridViewTextBoxColumn1,
            this.表具地址DataGridViewTextBoxColumn1,
            this.任务IDDataGridViewTextBoxColumn1,
            this.优先级DataGridViewTextBoxColumn1,
            this.状态DataGridViewTextBoxColumn1,
            this.结果DataGridViewTextBoxColumn1});
            this.dgvDocument.ContextMenuStrip = this.cnMenuDocument;
            this.dgvDocument.Cursor = System.Windows.Forms.Cursors.Default;
            this.dgvDocument.DataMember = "tbDocument";
            this.dgvDocument.DataSource = this.dsDocument;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDocument.DefaultCellStyle = dataGridViewCellStyle9;
            this.dgvDocument.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDocument.GridColor = System.Drawing.Color.DarkGreen;
            this.dgvDocument.Location = new System.Drawing.Point(0, 20);
            this.dgvDocument.Name = "dgvDocument";
            this.dgvDocument.ReadOnly = true;
            this.dgvDocument.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvDocument.RowHeadersVisible = false;
            this.dgvDocument.RowTemplate.Height = 23;
            this.dgvDocument.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvDocument.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDocument.Size = new System.Drawing.Size(374, 660);
            this.dgvDocument.StandardTab = true;
            this.dgvDocument.TabIndex = 0;
            // 
            // SN
            // 
            this.SN.DataPropertyName = "SN";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.SN.DefaultCellStyle = dataGridViewCellStyle3;
            this.SN.HeaderText = "SN";
            this.SN.Name = "SN";
            this.SN.ReadOnly = true;
            this.SN.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.SN.Width = 35;
            // 
            // 表具地址
            // 
            this.表具地址.DataPropertyName = "表具地址";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Navy;
            this.表具地址.DefaultCellStyle = dataGridViewCellStyle4;
            this.表具地址.HeaderText = "表具地址";
            this.表具地址.Name = "表具地址";
            this.表具地址.ReadOnly = true;
            this.表具地址.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.表具地址.Width = 95;
            // 
            // 任务ID
            // 
            this.任务ID.DataPropertyName = "任务ID";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.任务ID.DefaultCellStyle = dataGridViewCellStyle5;
            this.任务ID.HeaderText = "任务ID";
            this.任务ID.Name = "任务ID";
            this.任务ID.ReadOnly = true;
            this.任务ID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.任务ID.Width = 50;
            // 
            // 优先级
            // 
            this.优先级.DataPropertyName = "优先级";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.优先级.DefaultCellStyle = dataGridViewCellStyle6;
            this.优先级.HeaderText = "优先级";
            this.优先级.Name = "优先级";
            this.优先级.ReadOnly = true;
            this.优先级.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.优先级.Width = 50;
            // 
            // 状态
            // 
            this.状态.DataPropertyName = "状态";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.状态.DefaultCellStyle = dataGridViewCellStyle7;
            this.状态.HeaderText = "状态";
            this.状态.Name = "状态";
            this.状态.ReadOnly = true;
            this.状态.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.状态.Width = 60;
            // 
            // 结果
            // 
            this.结果.DataPropertyName = "结果";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.结果.DefaultCellStyle = dataGridViewCellStyle8;
            this.结果.HeaderText = "结果";
            this.结果.Name = "结果";
            this.结果.ReadOnly = true;
            this.结果.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.结果.Width = 80;
            // 
            // sNDataGridViewTextBoxColumn1
            // 
            this.sNDataGridViewTextBoxColumn1.DataPropertyName = "SN";
            this.sNDataGridViewTextBoxColumn1.HeaderText = "SN";
            this.sNDataGridViewTextBoxColumn1.Name = "sNDataGridViewTextBoxColumn1";
            this.sNDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // 表具地址DataGridViewTextBoxColumn1
            // 
            this.表具地址DataGridViewTextBoxColumn1.DataPropertyName = "表具地址";
            this.表具地址DataGridViewTextBoxColumn1.HeaderText = "表具地址";
            this.表具地址DataGridViewTextBoxColumn1.Name = "表具地址DataGridViewTextBoxColumn1";
            this.表具地址DataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // 任务IDDataGridViewTextBoxColumn1
            // 
            this.任务IDDataGridViewTextBoxColumn1.DataPropertyName = "任务ID";
            this.任务IDDataGridViewTextBoxColumn1.HeaderText = "任务ID";
            this.任务IDDataGridViewTextBoxColumn1.Name = "任务IDDataGridViewTextBoxColumn1";
            this.任务IDDataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // 优先级DataGridViewTextBoxColumn1
            // 
            this.优先级DataGridViewTextBoxColumn1.DataPropertyName = "优先级";
            this.优先级DataGridViewTextBoxColumn1.HeaderText = "优先级";
            this.优先级DataGridViewTextBoxColumn1.Name = "优先级DataGridViewTextBoxColumn1";
            this.优先级DataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // 状态DataGridViewTextBoxColumn1
            // 
            this.状态DataGridViewTextBoxColumn1.DataPropertyName = "状态";
            this.状态DataGridViewTextBoxColumn1.HeaderText = "状态";
            this.状态DataGridViewTextBoxColumn1.Name = "状态DataGridViewTextBoxColumn1";
            this.状态DataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // 结果DataGridViewTextBoxColumn1
            // 
            this.结果DataGridViewTextBoxColumn1.DataPropertyName = "结果";
            this.结果DataGridViewTextBoxColumn1.HeaderText = "结果";
            this.结果DataGridViewTextBoxColumn1.Name = "结果DataGridViewTextBoxColumn1";
            this.结果DataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // cnMenuDocument
            // 
            this.cnMenuDocument.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddTask,
            this.tsmiDelTask,
            this.tsSeparator1,
            this.tsmiClearDocument,
            this.tsmiSelectAll,
            this.tsSeparator2,
            this.tsmiImportFromDevice,
            this.tsmiExportToDevice,
            this.tsSeparator3,
            this.tsmiLoadDocument,
            this.tsmiSaveDocument,
            this.tsSeparator4,
            this.tsmiAddSubNode,
            this.tsmiDelSubNode});
            this.cnMenuDocument.Name = "contextMenuStrip1";
            this.cnMenuDocument.Size = new System.Drawing.Size(161, 248);
            this.cnMenuDocument.Opening += new System.ComponentModel.CancelEventHandler(this.cnMenuDocument_Opening);
            // 
            // tsmiAddTask
            // 
            this.tsmiAddTask.Name = "tsmiAddTask";
            this.tsmiAddTask.Size = new System.Drawing.Size(160, 22);
            this.tsmiAddTask.Text = "添加任务";
            this.tsmiAddTask.Click += new System.EventHandler(this.tsmiAddTask_Click);
            // 
            // tsmiDelTask
            // 
            this.tsmiDelTask.Name = "tsmiDelTask";
            this.tsmiDelTask.Size = new System.Drawing.Size(160, 22);
            this.tsmiDelTask.Text = "删除任务";
            this.tsmiDelTask.Click += new System.EventHandler(this.tsmiDelTask_Click);
            // 
            // tsSeparator1
            // 
            this.tsSeparator1.Name = "tsSeparator1";
            this.tsSeparator1.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmiClearDocument
            // 
            this.tsmiClearDocument.Name = "tsmiClearDocument";
            this.tsmiClearDocument.Size = new System.Drawing.Size(160, 22);
            this.tsmiClearDocument.Text = "清空档案";
            this.tsmiClearDocument.Click += new System.EventHandler(this.tsmiClearDocument_Click);
            // 
            // tsmiSelectAll
            // 
            this.tsmiSelectAll.Name = "tsmiSelectAll";
            this.tsmiSelectAll.Size = new System.Drawing.Size(160, 22);
            this.tsmiSelectAll.Text = "全部选择";
            this.tsmiSelectAll.Click += new System.EventHandler(this.tsmiSelectAll_Click);
            // 
            // tsSeparator2
            // 
            this.tsSeparator2.Name = "tsSeparator2";
            this.tsSeparator2.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmiImportFromDevice
            // 
            this.tsmiImportFromDevice.Name = "tsmiImportFromDevice";
            this.tsmiImportFromDevice.Size = new System.Drawing.Size(160, 22);
            this.tsmiImportFromDevice.Text = "从设备导入档案";
            this.tsmiImportFromDevice.Click += new System.EventHandler(this.tsmiImportFromDevice_Click);
            // 
            // tsmiExportToDevice
            // 
            this.tsmiExportToDevice.Name = "tsmiExportToDevice";
            this.tsmiExportToDevice.Size = new System.Drawing.Size(160, 22);
            this.tsmiExportToDevice.Text = "导出档案到设备";
            this.tsmiExportToDevice.Click += new System.EventHandler(this.tsmiExportToDevice_Click);
            // 
            // tsSeparator3
            // 
            this.tsSeparator3.Name = "tsSeparator3";
            this.tsSeparator3.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmiLoadDocument
            // 
            this.tsmiLoadDocument.Name = "tsmiLoadDocument";
            this.tsmiLoadDocument.Size = new System.Drawing.Size(160, 22);
            this.tsmiLoadDocument.Text = "从文件读取档案";
            this.tsmiLoadDocument.Click += new System.EventHandler(this.tsmiLoadDocument_Click);
            // 
            // tsmiSaveDocument
            // 
            this.tsmiSaveDocument.Name = "tsmiSaveDocument";
            this.tsmiSaveDocument.Size = new System.Drawing.Size(160, 22);
            this.tsmiSaveDocument.Text = "保存档案到文件";
            this.tsmiSaveDocument.Click += new System.EventHandler(this.tsmiSaveDocument_Click);
            // 
            // tsSeparator4
            // 
            this.tsSeparator4.Name = "tsSeparator4";
            this.tsSeparator4.Size = new System.Drawing.Size(157, 6);
            // 
            // tsmiAddSubNode
            // 
            this.tsmiAddSubNode.Name = "tsmiAddSubNode";
            this.tsmiAddSubNode.Size = new System.Drawing.Size(160, 22);
            this.tsmiAddSubNode.Text = "添加从节点";
            this.tsmiAddSubNode.Click += new System.EventHandler(this.tsmiAddSubNode_Click);
            // 
            // tsmiDelSubNode
            // 
            this.tsmiDelSubNode.Name = "tsmiDelSubNode";
            this.tsmiDelSubNode.Size = new System.Drawing.Size(160, 22);
            this.tsmiDelSubNode.Text = "删除从节点";
            this.tsmiDelSubNode.Click += new System.EventHandler(this.tsmiDelSubNode_Click);
            // 
            // dsDocument
            // 
            this.dsDocument.DataSetName = "dsNodeDoc";
            this.dsDocument.Tables.AddRange(new System.Data.DataTable[] {
            this.tbDocument});
            // 
            // tbDocument
            // 
            this.tbDocument.Columns.AddRange(new System.Data.DataColumn[] {
            this.SerialNo,
            this.Address,
            this.TaskId,
            this.TaskPriority,
            this.TaskStatus,
            this.TaskResult});
            this.tbDocument.TableName = "tbDocument";
            // 
            // SerialNo
            // 
            this.SerialNo.Caption = "SerialNo";
            this.SerialNo.ColumnName = "SN";
            // 
            // Address
            // 
            this.Address.Caption = "Address";
            this.Address.ColumnName = "表具地址";
            // 
            // TaskId
            // 
            this.TaskId.ColumnName = "任务ID";
            // 
            // TaskPriority
            // 
            this.TaskPriority.Caption = "Priority";
            this.TaskPriority.ColumnName = "优先级";
            // 
            // TaskStatus
            // 
            this.TaskStatus.ColumnName = "状态";
            // 
            // TaskResult
            // 
            this.TaskResult.ColumnName = "结果";
            // 
            // lbDocument
            // 
            this.lbDocument.BackColor = System.Drawing.SystemColors.Control;
            this.lbDocument.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbDocument.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbDocument.Location = new System.Drawing.Point(0, 0);
            this.lbDocument.Name = "lbDocument";
            this.lbDocument.Size = new System.Drawing.Size(374, 20);
            this.lbDocument.TabIndex = 0;
            this.lbDocument.Text = "档案列表〖0〗";
            this.lbDocument.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(414, 4);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 9;
            this.label9.Text = "解析";
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Green;
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(402, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 10);
            this.label6.TabIndex = 6;
            this.label6.Text = " ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(340, 4);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 8;
            this.label8.Text = "接收";
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Indigo;
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Location = new System.Drawing.Point(386, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 10);
            this.label5.TabIndex = 5;
            this.label5.Text = " ";
            // 
            // pgbCapactity
            // 
            this.pgbCapactity.Dock = System.Windows.Forms.DockStyle.Top;
            this.pgbCapactity.Location = new System.Drawing.Point(0, 20);
            this.pgbCapactity.Name = "pgbCapactity";
            this.pgbCapactity.Size = new System.Drawing.Size(583, 1);
            this.pgbCapactity.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(278, 4);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "发送";
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Red;
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Location = new System.Drawing.Point(328, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 10);
            this.label4.TabIndex = 4;
            this.label4.Text = " ";
            // 
            // rtbCommMsg
            // 
            this.rtbCommMsg.BackColor = System.Drawing.SystemColors.Control;
            this.rtbCommMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbCommMsg.ContextMenuStrip = this.cnMenuComm;
            this.rtbCommMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCommMsg.Location = new System.Drawing.Point(0, 20);
            this.rtbCommMsg.Name = "rtbCommMsg";
            this.rtbCommMsg.ReadOnly = true;
            this.rtbCommMsg.Size = new System.Drawing.Size(583, 660);
            this.rtbCommMsg.TabIndex = 1;
            this.rtbCommMsg.Text = "";
            // 
            // cnMenuComm
            // 
            this.cnMenuComm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAutoScrollCommMsg,
            this.tsmiClearAllCommMsg,
            this.tsmiSaveCommMsgToFile});
            this.cnMenuComm.Name = "contextMenuStrip1";
            this.cnMenuComm.Size = new System.Drawing.Size(125, 70);
            this.cnMenuComm.Opening += new System.ComponentModel.CancelEventHandler(this.cnMenuComm_Opening);
            // 
            // tsmiAutoScrollCommMsg
            // 
            this.tsmiAutoScrollCommMsg.CheckOnClick = true;
            this.tsmiAutoScrollCommMsg.Name = "tsmiAutoScrollCommMsg";
            this.tsmiAutoScrollCommMsg.Size = new System.Drawing.Size(124, 22);
            this.tsmiAutoScrollCommMsg.Text = "自动滚动";
            this.tsmiAutoScrollCommMsg.Click += new System.EventHandler(this.tsmiAutoScrollCommMsg_Click);
            // 
            // tsmiClearAllCommMsg
            // 
            this.tsmiClearAllCommMsg.Name = "tsmiClearAllCommMsg";
            this.tsmiClearAllCommMsg.Size = new System.Drawing.Size(124, 22);
            this.tsmiClearAllCommMsg.Text = "清空记录";
            this.tsmiClearAllCommMsg.Click += new System.EventHandler(this.tsmiClearAllCommMsg_Click);
            // 
            // tsmiSaveCommMsgToFile
            // 
            this.tsmiSaveCommMsgToFile.Name = "tsmiSaveCommMsgToFile";
            this.tsmiSaveCommMsgToFile.Size = new System.Drawing.Size(124, 22);
            this.tsmiSaveCommMsgToFile.Text = "保存记录";
            this.tsmiSaveCommMsgToFile.Click += new System.EventHandler(this.tsmiSaveCommMsgToFile_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Blue;
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(266, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 10);
            this.label3.TabIndex = 3;
            this.label3.Text = " ";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(583, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "通讯记录及解析：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sNDataGridViewTextBoxColumn
            // 
            this.sNDataGridViewTextBoxColumn.DataPropertyName = "SN";
            this.sNDataGridViewTextBoxColumn.HeaderText = "SN";
            this.sNDataGridViewTextBoxColumn.Name = "sNDataGridViewTextBoxColumn";
            this.sNDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // 表具地址DataGridViewTextBoxColumn
            // 
            this.表具地址DataGridViewTextBoxColumn.DataPropertyName = "表具地址";
            this.表具地址DataGridViewTextBoxColumn.HeaderText = "表具地址";
            this.表具地址DataGridViewTextBoxColumn.Name = "表具地址DataGridViewTextBoxColumn";
            this.表具地址DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // 任务IDDataGridViewTextBoxColumn
            // 
            this.任务IDDataGridViewTextBoxColumn.DataPropertyName = "任务ID";
            this.任务IDDataGridViewTextBoxColumn.HeaderText = "任务ID";
            this.任务IDDataGridViewTextBoxColumn.Name = "任务IDDataGridViewTextBoxColumn";
            this.任务IDDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // 优先级DataGridViewTextBoxColumn
            // 
            this.优先级DataGridViewTextBoxColumn.DataPropertyName = "优先级";
            this.优先级DataGridViewTextBoxColumn.HeaderText = "优先级";
            this.优先级DataGridViewTextBoxColumn.Name = "优先级DataGridViewTextBoxColumn";
            this.优先级DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // 状态DataGridViewTextBoxColumn
            // 
            this.状态DataGridViewTextBoxColumn.DataPropertyName = "状态";
            this.状态DataGridViewTextBoxColumn.HeaderText = "状态";
            this.状态DataGridViewTextBoxColumn.Name = "状态DataGridViewTextBoxColumn";
            this.状态DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // 结果DataGridViewTextBoxColumn
            // 
            this.结果DataGridViewTextBoxColumn.DataPropertyName = "结果";
            this.结果DataGridViewTextBoxColumn.HeaderText = "结果";
            this.结果DataGridViewTextBoxColumn.Name = "结果DataGridViewTextBoxColumn";
            this.结果DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // serialPort
            // 
            this.serialPort.Parity = System.IO.Ports.Parity.Even;
            this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);
            // 
            // timerConcSim
            // 
            this.timerConcSim.Enabled = true;
            this.timerConcSim.Interval = 10;
            this.timerConcSim.Tick += new System.EventHandler(this.timerConcSim_Tick);
            // 
            // openFileDlg
            // 
            this.openFileDlg.FileName = "openFileDialog1";
            // 
            // ConcSimulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ConcSimulator";
            this.Size = new System.Drawing.Size(1342, 682);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.gpbTask.ResumeLayout(false);
            this.gpbInitial.ResumeLayout(false);
            this.gpbComSelect.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDocument)).EndInit();
            this.cnMenuDocument.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dsDocument)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbDocument)).EndInit();
            this.cnMenuComm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox gpbInitial;
        private System.Windows.Forms.GroupBox gpbComSelect;
        private System.Windows.Forms.Button btOpenPort;
        private System.Windows.Forms.ComboBox cmbBaudrate;
        private System.Windows.Forms.ComboBox cmbPort;
        private System.Windows.Forms.Button btInitialTask;
        private System.Windows.Forms.Button btInitialDocument;
        private System.Windows.Forms.Button btResetDevice;
        private System.Windows.Forms.GroupBox gpbTask;
        private System.Windows.Forms.Button btStartTask;
        private System.Windows.Forms.Button btReadRemainTask;
        private System.Windows.Forms.Button btReadUndoTaskTab;
        private System.Windows.Forms.Button btReadUndoTaskNum;
        private System.Windows.Forms.ContextMenuStrip cnMenuDocument;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddTask;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelTask;
        private System.Windows.Forms.DataGridView dgvDocument;
        private System.Windows.Forms.Label lbDocument;
        private System.Data.DataSet dsDocument;
        private System.Data.DataTable tbDocument;
        private System.Data.DataColumn SerialNo;
        private System.Data.DataColumn Address;
        private System.Data.DataColumn TaskId;
        private System.Data.DataColumn TaskStatus;
        private System.Data.DataColumn TaskResult;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbCommMsg;
        private System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.Timer timerConcSim;
        private System.Windows.Forms.ProgressBar pgbCapactity;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btReadComDelayTime;
        private System.Windows.Forms.Button btReadAddress;
        private System.Windows.Forms.Button btModuleWorkMode;
        private System.Windows.Forms.Button btVersionInfo;
        private System.Windows.Forms.Button btReadFatherNode;
        private System.Windows.Forms.Button btReadSubnodeRegProgress;
        private System.Windows.Forms.Button btReadSubnodeCount;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btStopRegister;
        private System.Windows.Forms.Button btStartRegister;
        private System.Windows.Forms.Button btDisableEventReport;
        private System.Windows.Forms.Button btEnableEventReport;
        private System.Windows.Forms.Button btSetConcAddr;
        private System.Windows.Forms.Button btPauseTask;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolStripMenuItem tsmiLoadDocument;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.ToolStripMenuItem tsmiSaveDocument;
        private System.Windows.Forms.ToolStripMenuItem tsmiImportFromDevice;
        private System.Windows.Forms.ToolStripMenuItem tsmiExportToDevice;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearDocument;
        private System.Windows.Forms.ContextMenuStrip cnMenuComm;
        private System.Windows.Forms.ToolStripMenuItem tsmiAutoScrollCommMsg;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearAllCommMsg;
        private System.Windows.Forms.ToolStripMenuItem tsmiSaveCommMsgToFile;
        private System.Windows.Forms.SaveFileDialog saveFileDlg;
        private System.Windows.Forms.ToolStripSeparator tsSeparator1;
        private System.Windows.Forms.ToolStripSeparator tsSeparator2;
        private System.Windows.Forms.ToolStripSeparator tsSeparator3;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddSubNode;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelSubNode;
        private System.Windows.Forms.Label lbConcAddr;
        private System.Data.DataColumn TaskPriority;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelectAll;
        private System.Windows.Forms.ToolStripSeparator tsSeparator4;
        private System.Windows.Forms.Button btReadUndoTaskInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn SN;
        private System.Windows.Forms.DataGridViewTextBoxColumn 表具地址;
        private System.Windows.Forms.DataGridViewTextBoxColumn 任务ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn 优先级;
        private System.Windows.Forms.DataGridViewTextBoxColumn 状态;
        private System.Windows.Forms.DataGridViewTextBoxColumn 结果;
        private System.Windows.Forms.DataGridViewTextBoxColumn sNDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn 表具地址DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn 任务IDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn 优先级DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn 状态DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn 结果DataGridViewTextBoxColumn;
        private System.Windows.Forms.Button btReadSubnodeInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn sNDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 表具地址DataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 任务IDDataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 优先级DataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 状态DataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn 结果DataGridViewTextBoxColumn1;
    }
}

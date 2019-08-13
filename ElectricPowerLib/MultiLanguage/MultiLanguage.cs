using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ElectricPowerLib.MultiLanguage
{
    public class LanguageDict
    {
        /// <summary>
        /// 中文->英文 字典定义示例: zh-CN to en-US
        /// </summary>
        public static Dictionary<string, string> zh_CN_To_en_US = new Dictionary<string, string>()
        {
            // 界面
            {"NB模组调试软件",                "NB-Module_Debuger"},
            {"上海桑锐电子科技股份有限公司",  "Shanghai Sunray Electronics Technology Co., Ltd."},

            {"界面语言",            "UI Language"},
            {"简体中文(zh-CN)",     "简体中文(zh-CN)"},
            {"English (en-US)",     "English (en-US)"},
            {"模组选择",            "Module Select"},
            {"型号",                "Model"},

            {"连接设备",            "Connect Device"},
            {"端口号",              "Port"},
            {"打开串口",            "Open"},
            {"关闭串口",            "Close"},

            {"信息查询",            "Information Query"},
            {"版本号",              "Version"},
            {"IMEI号码",            "IMEI"},
            {"BAND值",              "Band"},
            {"SIM卡ID",             "IMSI"},
            {"温度、电池电压",      "Temp、Vbat"},

            {"参数设置",            "Parameter Setting"},
            {"BAND设置",            "Set Band "},

            {"网络连接",            "Network Connection"},
            {"入网",                "Join Network "},
            {"查询网络状态",        "Query UEStatus"},
            {"云平台",              "Server"},
            {"CDP服务器",           "CDP Server"},
            {"UDP服务器",           "UDP Server"},
            {"OneNet平台",          "CMCC OneNet"},
            {"建立连接",            "Connect "},
            {"断开连接",            "Disconnect "},
            {"数据\r\n上传",        "Data\r\nUpload"},
            {"查看接收数据",        "Check Receive"},
            {"入网状态：离线",      "NetState：Offline"},
            {"入网状态：成功",      "NetState：Online"},

            {"通信记录",            "Message Log"},
            {"清空记录",            "Clear Log"},
            {"保存记录",            "Save Log"},
            {"显示时间",            "Show Time"},
            {"发送Ctrl-Z",          "Send Ctrl-Z"},
            {"发送ESC",             "Send ESC"},
            {"  AT 指 令 ：",       "AT Command:"},
            {"发送",                "Send"},
            {"命令执行中...",       "Command Executing ..."},

            // 提示信息
            {"入网启动中...",                    "Join Network Starting ..."},
            {"请入网后再连接平台",               "Please Join Network Before Connect Server"},
            {"输入的Ip或Port无效：",             "The Input Ip Or Port Is Invalid : "},
            {"请连接平台后再上传",               "Please Connect Server Before Upload Data"},
            {"请输入数据后再上传",               "Please Input Data Before Upload"},
            {"请连接平台后再接收",               "Please Connect Server Before Check Receive"},

            {"[ 串口连接已断开 ]",               "[ SerialPort Disconnected ]"},
            {"打开串口失败",                     "Open SerialPort Failed"},
            {"关闭串口失败",                     "Close SerialPort Failed"},
            {"模组检测中...",                    "Module Check Stating ..."},
            {"连接的可能不是",                   "Current Connect Device Maybe Not"},
            {"模组",                             "Module"},
            {"请至少选择一个Band值进行设置",     "Please choose at least one Band to set"},
            {"成功",                             "Success"},
            {"失败",                             "Failed"},
            {"保存成功",                         "Saved Success" },

            // 接收解析
            {"模组型号",                "Module Model"},
            {"收到数据",                "Received Data"},
            {"远程地址",                "Remote Address"},
            {"长度",                    "Length"},
            {"数据",                    "Data"},
            {"剩余缓存数据",            "Remain Data Length"},
            {"网络状态",                "UEStatus"},
            {"软件版本",                "Software Version"},
            {"硬件版本",                "Hardware Version"},
            {"温度",                    "Temperature"},
            {"电池电压",                "Battery Voltage"},

            // AT指令名
            {"模组检测",                "Module Check "},
            {"连接平台",                "Connect Server "},
            {"数据上传",                "Upload Data "},
            {"查看接收缓存",            "Check Receive Buffer "},

            {"查询版本号",            "Query Version "},
            {"查询软件版本",          "Query Software Version "},
            {"查询硬件版本",          "Query Hardware Version "},
            {"查询BAND",              "Query Band "},
            {"查询IMEI",              "Query IMEI "},
            {"查询SIM卡ID",           "Query IMSI "},
            {"查询温度和电压",        "Query Temperature and Battery Voltage "},
            {"查询入网状态",          "Query NetState "},
            {"查询网络状态信息",      "Query UEStatus "},

            {"关闭SIM卡",             "Close Sim Function "},
            {"设置BAND",              "Set Band "},
            {"打开SIM卡",             "Open Sim Function "},
            {"模组复位及入网",        "Module Reset And AutoRegister "},
            {"激活PDP上下文",         "Active PDP Context "},
            {"模组复位",              "Module Reset "},
            {"模组配置",              "Module Configure-0 "},
            {"模组配置1",             "Module Configure-1 "},
            {"模组配置2",             "Module Configure-2 "},
            {"入网激活",              "Attach to Packet Domain Service "},

            {"设置COAP协议IP",        "Configure CDP Server "},
            {"打开接收上报",          "Enable New Message Indication "},
            {"创建通信Socket",        "Create Socket "},
            {"创建通信套件",          "Create Communication Suite "},
            {"添加设备对象",          "Add Device Object "},
            {"注册设备",              "Open OneNet Connection "},
            {"观察对象应答",          "Response To Observe Object "},
            {"发现对象资源应答",      "Response To Discovery Resource "},

            {"发送数据",              "Send Data "},
            {"发送UDP数据",           "Send UDP Data "},
            {"上报对象资源",          "Report Object Resource "},
            {"接收Socket数据",        "Receive Socket Data "},

            {"关闭CDP连接",           "Close CDP Connection "},
            {"关闭通信Socket",        "Close Socket "},
            {"注销设备",              "Close OneNet Connection "},
            {"删除通信套件",          "Delete Communication Suite "},

            {"自定义AT指令",          "Send Custom AT Command "},
        };
    }

    public class MultiLanguage
    {
        public static string CurrentLanguage;
        public static Dictionary<string, string> CurrentResource;
        private static Dictionary<Object, string> DefaultResource = new Dictionary<Object, string>();

        public MultiLanguage(Dictionary<string, string> dict_Zh_to_En)
        {
            if (dict_Zh_to_En == null)
            {
                throw new Exception("Dictionary<string, string> dict_Zh_to_En Can't be null !");
            }

            CurrentResource = dict_Zh_to_En;
        }
        /// <summary>
        /// 初始化语言
        /// </summary>
        public static void InitLanguage(Control control)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            
            if (CurrentResource == null)
            {
                CurrentResource = LanguageDict.zh_CN_To_en_US;
            }

            //使用递归的方式对控件及其子控件进行处理
            SetControlLanguageText(control);

            //工具栏或者菜单动态构建窗体或者控件的时候，重新对子控件进行处理
            //control.ControlAdded += (sender, e) =>
            //{
            //    SetControlLanguageText(e.Control);
            //};
        }
        /// <summary>
        /// 内容的语言转化
        /// </summary>
        /// <param name="parent"></param>
        public static void SetControlLanguageText(System.Windows.Forms.Control parent)
        {
            CurrentLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

            if (parent.HasChildren)
            {
                parent.Text = GetObjectText(parent);
                foreach (System.Windows.Forms.Control ctrl in parent.Controls)
                {
                    SetContainerLanguage(ctrl);
                }
            }
            else
            {
                SetLanguage(parent);
            }
        }

        /// <summary>
        /// 根据语言标识符得到转换后的值
        /// </summary>
        /// <param name="languageFlag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetCurrentText(string key)
        {
            string strRet = "";

            string language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (string.IsNullOrWhiteSpace(key) || language.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
            {
                strRet = key;
            }
            else
            {
                if (CurrentResource == null)
                {
                    switch (language)
                    {
                        case "en":
                        case "en-US":
                        default:
                            CurrentResource = LanguageDict.zh_CN_To_en_US;
                            break;
                    }
                }

                strRet = CurrentResource.FirstOrDefault(q => q.Key == key).Value;
                if(strRet == null)
                {
                    strRet = key;
                }
            }

            return strRet;
        }

        public static string GetDefaultText(string value)
        {
            string strRet = "";

            string language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (string.IsNullOrWhiteSpace(value) || language.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
            {
                strRet = value;
            }
            else
            {
                if (CurrentResource == null)
                {
                    switch (language)
                    {
                        case "en":
                        case "en-US":
                        default:
                            CurrentResource = LanguageDict.zh_CN_To_en_US;
                            break;
                    }
                }

                strRet = CurrentResource.FirstOrDefault(q => q.Value == value).Key;
                if (strRet == null)
                {
                    strRet = value;
                }
            }

            return strRet;
        }
        /// <summary>
        /// 获取控件默认语言
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static string GetControlDefaultText(Control ctrl)
        {
            string strRet = "";
            Object obj = ctrl as Object;

            if (DefaultResource.ContainsKey(obj))
            {
                strRet = DefaultResource[obj];
            }
            else
            {
                string objText = "";

                if (obj is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)obj;
                    for (int n = 0; n < comboBox.Items.Count; n++)
                    {
                        objText += comboBox.Items[n].ToString() + "/";
                    }
                    objText = objText.Remove(objText.Length - 1);
                }
                else if (obj is CheckedListBox)
                {
                    CheckedListBox chklistBox = (CheckedListBox)obj;
                    for (int n = 0; n < chklistBox.Items.Count; n++)
                    {
                        objText += chklistBox.Items[n].ToString() + "/";
                    }
                    objText = objText.Remove(objText.Length - 1);
                }
                else if (obj is DataGridViewColumn)
                {
                    objText = ((DataGridViewColumn)obj).HeaderText;
                }
                else if (obj is Control)
                {
                    objText = ((Control)obj).Text;
                }

                string language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                if (language.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
                {
                    strRet = objText;
                }
                else
                {
                    if (CurrentResource == null)
                    {
                        switch (language)
                        {
                            case "en":
                            case "en-US":
                            default:
                                CurrentResource = LanguageDict.zh_CN_To_en_US;
                                break;
                        }
                    }

                    strRet = CurrentResource.FirstOrDefault(q => q.Key == objText).Value;
                    if (strRet == null)
                    {
                        strRet = objText;
                    }
                }
            }

            return strRet;
        }

        /// <summary>
        /// 更新控件默认语言
        /// </summary>
        /// <param name="obj"></param>
        public static void UpdateControlDefaultText(Control ctrl)
        {
            string objText = "";
            Object obj = ctrl as Object;

            if (obj is ComboBox)
            {
                ComboBox comboBox = (ComboBox)obj;
                for (int n = 0; n < comboBox.Items.Count; n++)
                {
                    objText += comboBox.Items[n].ToString() + "/";
                }
                objText = objText.Remove(objText.Length - 1);
            }
            else if (obj is CheckedListBox)
            {
                CheckedListBox chklistBox = (CheckedListBox)obj;
                for (int n = 0; n < chklistBox.Items.Count; n++)
                {
                    objText += chklistBox.Items[n].ToString() + "/";
                }
                objText = objText.Remove(objText.Length - 1);
            }
            else if (obj is DataGridViewColumn)
            {
                objText = ((DataGridViewColumn)obj).HeaderText;
            }
            else if (obj is Control)
            {
                objText = ((Control)obj).Text;
            }

            /*
            if (obj is ToolStripItem)
            {
                objText = ((ToolStripItem)obj).Text;
            }
            else if (obj is ToolStripMenuItem)
            {
                objText = ((ToolStripMenuItem)obj).Text;
            }
            else if (obj is TreeNode)
            {
                objText = ((TreeNode)obj).Text;
            }
            else if (obj is TabPage)
            {
                objText = ((TabPage)obj).Text;
            }
            */


            if (DefaultResource.ContainsKey(obj))
            {
                DefaultResource[obj] = objText;
            }
            else
            {
                DefaultResource.Add(obj, objText);
            }

            SetControlLanguageText(ctrl);
        }

        #region 设置控件语言
        /// <summary>
        /// 设置容器类控件的语言
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private static void SetContainerLanguage(System.Windows.Forms.Control ctrl)
        {
            if (ctrl is DataGridView)
            {
                try
                {
                    DataGridView dataGridView = (DataGridView)ctrl;
                    foreach (DataGridViewColumn dgvc in dataGridView.Columns)
                    {
                        try
                        {
                            if (dgvc.HeaderText.ToString() != "" && dgvc.Visible)
                            {
                                dgvc.HeaderText = GetObjectText(dgvc);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception)
                { }
            }
            if (ctrl is MenuStrip)
            {
                MenuStrip menuStrip = (MenuStrip)ctrl;
                foreach (ToolStripMenuItem toolItem in menuStrip.Items)
                {
                    try
                    {
                        toolItem.Text = GetObjectText(toolItem);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (toolItem.DropDownItems.Count > 0)
                        {
                            GetItemText(toolItem);
                        }
                    }
                }
            }
            else if (ctrl is TreeView)
            {
                TreeView treeView = (TreeView)ctrl;
                foreach (TreeNode node in treeView.Nodes)
                {
                    try
                    {
                        node.Text = GetObjectText(node);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (node.Nodes.Count > 0)
                        {
                            GetNodeText(node);
                        }
                    }
                }
            }
            else if (ctrl is TabControl)
            {
                TabControl tabCtrl = (TabControl)ctrl;
                try
                {
                    foreach (TabPage tabPage in tabCtrl.TabPages)
                    {
                        tabPage.Text = GetObjectText(tabPage);
                    }
                }
                catch (Exception)
                {
                }
            }
            else if (ctrl is StatusStrip)
            {
                StatusStrip statusStrip = (StatusStrip)ctrl;
                foreach (ToolStripItem toolItem in statusStrip.Items)
                {
                    try
                    {
                        toolItem.Text = GetObjectText(toolItem);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        ToolStripDropDownButton tsDDBtn = toolItem as ToolStripDropDownButton;
                        if (tsDDBtn != null && tsDDBtn.DropDownItems.Count > 0)
                        {
                            GetItemText(tsDDBtn);
                        }
                    }
                }
            }
            else if (ctrl is ToolStrip)
            {
                ToolStrip statusStrip = (ToolStrip)ctrl;
                foreach (ToolStripItem toolItem in statusStrip.Items)
                {
                    try
                    {
                        toolItem.Text = GetObjectText(toolItem);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (ctrl is CheckedListBox)
            {
                CheckedListBox chkListBox = (CheckedListBox)ctrl;
                try
                {
                    if(chkListBox.Items.Count > 0)
                    {
                        GetObjectText(chkListBox);
                    }
                }
                catch (Exception)
                { }
            }
            else if (ctrl is ComboBox)
            {
                ComboBox comboBox = (ComboBox)ctrl;
                try
                {
                    if (comboBox.Items.Count > 0)
                    {
                        GetObjectText(comboBox);
                    }
                }
                catch (Exception)
                { }
            }
            else if (ctrl is GroupBox)
            {
                GroupBox grpBox = (GroupBox)ctrl;
                try
                {
                    grpBox.Text = GetObjectText(grpBox);
                }
                catch (Exception)
                {
                }
            }


            if (ctrl.HasChildren)
            {
                foreach (System.Windows.Forms.Control c in ctrl.Controls)
                {
                    SetContainerLanguage(c);
                }
            }
            else
            {
                SetLanguage(ctrl);
            }

        }
        /// <summary>
        /// 设置普通控件的语言
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private static void SetLanguage(System.Windows.Forms.Control ctrl)
        {
            if (true)
            {
                if (ctrl is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)ctrl;
                    try
                    {
                        checkBox.Text = GetObjectText(checkBox);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (ctrl is Label)
                {
                    Label label = (Label)ctrl;
                    try
                    {
                        label.Text = GetObjectText(label);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (ctrl is Button)
                {
                    Button button = (Button)ctrl;
                    try
                    {
                        button.Text = GetObjectText(button);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (ctrl is GroupBox)
                {
                    GroupBox groupBox = (GroupBox)ctrl;
                    try
                    {
                        groupBox.Text = GetObjectText(groupBox);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (ctrl is RadioButton)
                {
                    RadioButton radioButton = (RadioButton)ctrl;
                    try
                    {
                        radioButton.Text = GetObjectText(radioButton);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (ctrl is CheckedListBox)
                {
                    CheckedListBox chkListBox = (CheckedListBox)ctrl;
                    try
                    {
                        if (chkListBox.Items.Count > 0)
                        {
                            GetObjectText(chkListBox);
                        }
                    }
                    catch (Exception)
                    { }
                }
                else if (ctrl is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)ctrl;
                    try
                    {
                        if (comboBox.Items.Count > 0)
                        {
                            GetObjectText(comboBox);
                        }
                    }
                    catch (Exception)
                    { }
                }
            }

        }
        /// <summary>
        /// 递归转化菜单
        /// </summary>
        /// <param name="menuItem"></param>
        private static void GetItemText(ToolStripDropDownItem menuItem)
        {
            foreach (ToolStripItem toolItem in menuItem.DropDownItems)
            {
                try
                {
                    toolItem.Text = GetObjectText(toolItem);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (toolItem is ToolStripDropDownItem)
                    {
                        ToolStripDropDownItem subMenuStrip = (ToolStripDropDownItem)toolItem;
                        if (subMenuStrip.DropDownItems.Count > 0)
                        {
                            GetItemText(subMenuStrip);
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 递归转化树
        /// </summary>
        /// <param name="menuItem"></param>
        private static void GetNodeText(TreeNode node)
        {

            foreach (TreeNode treeNode in node.Nodes)
            {
                try
                {
                    treeNode.Text = GetObjectText(treeNode);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (treeNode.Nodes.Count > 0)
                    {
                        GetNodeText(treeNode);
                    }
                }
            }
        }

        private static string GetObjectText(Object obj)
        {
            string objText = "";

            string language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (language.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
            {
                // default language
                if (DefaultResource.ContainsKey(obj))
                {
                    objText = DefaultResource[obj];

                    if (obj is ComboBox)
                    {
                        ComboBox comboBox = (ComboBox)obj;
                        string[] items = objText.Split('/');
                        for (int n = 0; n < comboBox.Items.Count && items.Length == comboBox.Items.Count; n++)
                        {
                            comboBox.Items[n] = items[n];
                        }
                    }
                    else if (obj is CheckedListBox)
                    {
                        CheckedListBox chklistBox = (CheckedListBox)obj;
                        string[] items = objText.Split('/');
                        for (int n = 0; n < chklistBox.Items.Count && items.Length == chklistBox.Items.Count; n++)
                        {
                            chklistBox.Items[n] = items[n];
                        }
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    /*
                    if (obj is ToolStripItem)
                    {
                        objText = ((ToolStripItem)obj).Text;
                    }
                    else if (obj is ToolStripMenuItem)
                    {
                        objText = ((ToolStripMenuItem)obj).Text;
                    }
                    else if (obj is TreeNode)
                    {
                        objText = ((TreeNode)obj).Text;
                    }
                    else if (obj is TabPage)
                    {
                        objText = ((TabPage)obj).Text;
                    }
                    //else 
                     */
                    if (obj is ComboBox)
                    {
                        ComboBox comboBox = (ComboBox)obj;
                        for (int n = 0; n < comboBox.Items.Count; n++)
                        {
                            objText += comboBox.Items[n].ToString() + "/";
                        }
                        objText = objText.Remove(objText.Length - 1);
                    }
                    else if (obj is CheckedListBox)
                    {
                        CheckedListBox chklistBox = (CheckedListBox)obj;
                        for (int n = 0; n < chklistBox.Items.Count; n++)
                        {
                            objText += chklistBox.Items[n].ToString() + "/";
                        }
                        objText = objText.Remove(objText.Length - 1);
                    }
                    else if (obj is DataGridViewColumn)
                    {
                        objText = ((DataGridViewColumn)obj).HeaderText;
                    }
                    else if (obj is Control)
                    {
                        objText = ((Control)obj).Text;
                    }
                    else if (obj is object)
                    {
                        objText = ((object)obj).ToString();
                    }

                    DefaultResource.Add(obj, objText);
                }
            }
            else
            {
                // other language
                if (DefaultResource.ContainsKey(obj) == true)
                {
                    objText = DefaultResource[obj];

                    if (obj is ComboBox)
                    {
                        ComboBox comboBox = (ComboBox)obj;
                        //int selectIndex = comboBox.SelectedIndex;
                        string[] items = objText.Split('/');
                        for (int n = 0; n < comboBox.Items.Count && items.Length == comboBox.Items.Count; n++)
                        {
                            comboBox.Items[n] = GetCurrentText(items[n]);
                        }
                        //comboBox.SelectedIndex = selectIndex;
                    }
                    else if (obj is CheckedListBox)
                    {
                        CheckedListBox chklistBox = (CheckedListBox)obj;
                        //int selectIndex = chklistBox.SelectedIndex;
                        string[] items = objText.Split('/');
                        for (int n = 0; n < chklistBox.Items.Count && items.Length == chklistBox.Items.Count; n++)
                        {
                            chklistBox.Items[n] = GetCurrentText(items[n]);
                        }
                        //chklistBox.SelectedIndex = selectIndex;
                    }
                    else
                    {
                        objText = GetCurrentText(objText);
                    }

                }
            }

            return objText;
        }
        #endregion
    }
}

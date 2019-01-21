using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ElectricPowerDebuger
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //在InitializeComponent()之前调用
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new FrmMain());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                Application.Exit();
            }
        }

        // 方法1：在"Properties/Resources.resx"里添加dll文件
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources")) return null;

            string rootNamespace = Assembly.GetExecutingAssembly().FullName.Split(',')[0];

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(rootNamespace + ".Properties.Resources", Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return Assembly.Load(bytes);
        }

        // 方法2：在工程和引用里添加dll文件，都设置为不复制到本地，工程的dll生成操作选择“嵌入的资源”
        private static Assembly CurrentDomain_AssemblyResolve2(object sender, ResolveEventArgs args)
        {
            string rootNamespace = Assembly.GetExecutingAssembly().FullName.Split(',')[0];
            string resourceName = rootNamespace + new AssemblyName(args.Name).Name + ".dll";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {

                byte[] assemblyData = new byte[stream.Length];

                stream.Read(assemblyData, 0, assemblyData.Length);

                return Assembly.Load(assemblyData);

            }

        }
    }
}

using System;
using System.Reflection;

namespace ElectricPowerLib.Common
{
    /// <summary>
    /// 设置双缓冲属性扩展类
    /// </summary>
    public static class DoubleBufferedHelper
    {
        /// <summary>
        /// 设置双缓冲属性，解决闪烁问题
        /// </summary>
        /// <param name="obj">需设置双缓冲属性的控件</param>
        /// <param name="flag">是否启用双缓冲属性</param>
        public static void DoubleBuffered(this  object obj, bool flag)
        {
            Type tp = obj.GetType();
            PropertyInfo pi = tp.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(obj, flag, null);
        }
    }
}

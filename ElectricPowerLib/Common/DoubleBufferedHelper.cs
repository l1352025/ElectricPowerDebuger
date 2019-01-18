using System;
using System.Reflection;

namespace ElectricPowerLib.Common
{
    public static class DoubleBufferedHelper
    {
        /// <summary>
        /// 双缓冲，解决闪烁问题
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        public static void DoubleBuffered(this  object obj, bool flag)
        {
            Type tp = obj.GetType();
            PropertyInfo pi = tp.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(obj, flag, null);
        }
    }
}

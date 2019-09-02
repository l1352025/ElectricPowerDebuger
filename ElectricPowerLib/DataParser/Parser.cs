using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricPowerLib.DataParser
{
    /// <summary>
    /// 解析常见数据字段
    /// </summary>
    public class Parser
    {
        private static UInt32 u32Tmp;
        private static UInt16 u16Tmp;
        private static double doubleTmp;
        private static string[] details;

        /// <summary>
        /// 解析 整数10进制数
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细："[unsigned,signed],[*,/],[倍数]"</param>
        /// <param name="objValue">解析后的值 Int32/UInt32</param>
        /// <returns>解析后的字符串</returns>
        public static string GetIntDec(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;

            u32Tmp = 0;
            details = detail.Split(',');
            for (i = 0; i < length; i++)
            {
                u32Tmp += ((UInt32)buffer[index++] << i * 8);
            }

            if (details.Length == 3)
            {
                if (details[1] == "*")
                {
                    u32Tmp = (UInt32)(u32Tmp * Convert.ToUInt32(details[2]));
                }
                else if (details[1] == "/")
                {
                    u32Tmp = (UInt32)(u32Tmp / Convert.ToUInt32(details[2]));
                }
            }

            if (details.Length == 3 && details[0] == "signed"
                && (u32Tmp & (1 << (i * 8 - 1))) > 0)
            {
                u32Tmp &= ~(1u << (i * 8 - 1));
                value = "-" + u32Tmp;
                objValue = Convert.ToInt32(value);
            }
            else
            {
                value = u32Tmp.ToString();
                objValue = u32Tmp;
            }

            return value;
        }

        /// <summary>
        /// 解析 整数16进制数
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细：""</param>
        /// <param name="objValue">解析后的值 UInt32</param>
        /// <returns>解析后的字符串</returns>
        public static string GetIntHex(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;

            u32Tmp = 0;
            for (i = 0; i < length; i++)
            {
                u32Tmp += ((UInt32)buffer[index++] << i * 8);
            }

            if (i == 4)
            {
                value = u32Tmp.ToString("X8");
            }
            else if (i == 2)
            {
                value = u32Tmp.ToString("X4");
            }
            else
            {
                value = u32Tmp.ToString("X2");
            }
            objValue = u32Tmp;

            return value;
        }

        /// <summary>
        /// 解析double数
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细：小数位数及倍率 "[小数字节数],[*,/],[倍数]" </param>
        /// <param name="objValue">解析后的值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetDouble(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;
            details = detail.Split(',');
            int len = Convert.ToByte(details[0]);
            doubleTmp = 0;
            u32Tmp = 0;
            index = index;
            for (i = 0; i < length - len; i++)
            {
                u32Tmp += (UInt32)(buffer[index++] << i * 8);
            }

            u16Tmp = 0;
            for (i = 0; i < len; i++)
            {
                u16Tmp += (UInt16)(buffer[index++] << i * 8);
            }
            if (details[1] == "*")
            {
                doubleTmp = (double)(u16Tmp * Convert.ToDouble(details[2]));
            }
            else if (details[1] == "/")
            {
                doubleTmp = (double)(u16Tmp / Convert.ToDouble(details[2]));
            }

            doubleTmp = (double)u32Tmp + doubleTmp;
            value = doubleTmp.ToString();
            objValue = doubleTmp;

            return value;
        }

        /// <summary>
        /// 解析 十六进制字符串（bytes to hex）
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细：分隔符 "[Normal,Reverse],[ '', ',', ' ']" </param>
        /// <param name="objValue">解析后的值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetHexStr(byte[] buffer, int index, int length, string detail)
        {
            string value = "";
            int i;

            u32Tmp = 0;
            details = detail.Split(',');
            for (i = 0; i < length; i++)
            {
                u32Tmp += ((UInt32)buffer[index++] << i * 8);
            }

            return value;
        }

       

        /// <summary>
        /// 解析 Ascii字符串
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细："[Normal,Reverse]"</param>
        /// <param name="objValue">解析后的值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetAscii(byte[] buffer, int index, int length, string detail)
        {
            string value = "";
            int i;

            u32Tmp = 0;
            details = detail.Split(',');
            for (i = 0; i < length; i++)
            {
                u32Tmp += ((UInt32)buffer[index++] << i * 8);
            }

            return value;
        }

        /// <summary>
        /// 解析整数十进制数
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细："[Normal,Reverse],[BIN,BCD],[yy[yy]-MM-dd ][HH:mm[:ss]]" </param>
        /// <param name="objValue">解析后的值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetTime(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;

            details = detail.Split(',');

            byte[] timeBuf = new byte[length];
            byte[] time = new byte[7] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            int timeStart = 0;
            Array.Copy(buffer, index, timeBuf, 0, timeBuf.Length);
            if (details[0] == "Reverse")
            {
                Array.Reverse(timeBuf, 0, timeBuf.Length);
            }
            if (details[1] == "BIN")
            {
                for (i = 0; i < timeBuf.Length; i++)
                {
                    timeBuf[i] = (byte)(timeBuf[i] + (timeBuf[i] / 10) * 6);    // dec to bcd
                }
            }
            if (detail == "yyyy-MM-dd"
                || detail == "yyyy-MM-dd HH:mm:ss")
            {
                timeStart = 0;
            }
            else if (detail == "yy-MM-dd"
                || detail == "yy-MM-dd HH:mm:ss")
            {
                time[0] = 0x20;
                timeStart = 1;
            }
            else if (detail == "HH:mm"
                || detail == "HH:mm:ss")
            {
                timeStart = 4;
            }

            for (i = timeStart; i < timeStart + timeBuf.Length; i++)
            {
                time[i] = timeBuf[i - timeStart];
            }

            if (time[4] == 0xFF)
            {
                // yyyy-MM-dd
                value = time[0].ToString("X2")
                    + time[1].ToString("X2") + "-"
                    + time[2].ToString("X2") + "-"
                    + time[3].ToString("X2");
            }
            else if (time[1] == 0xFF)
            {
                // HH:mm:ss
                value = time[4].ToString("X2") + ":"
                    + time[5].ToString("X2") + ":"
                    + (time[6] == 0xFF ? "" : time[6].ToString("X2"));
            }
            else
            {
                // yyyy-MM-dd HH:mm:ss
                value = time[0].ToString("X2")
                    + time[1].ToString("X2") + "-"
                    + time[2].ToString("X2") + "-"
                    + time[3].ToString("X2") + " "
                    + time[4].ToString("X2") + ":"
                    + time[5].ToString("X2") + ":"
                    + time[6].ToString("X2");
            }
            objValue = DateTime.Parse(value);

            return value;
        }
    }
}

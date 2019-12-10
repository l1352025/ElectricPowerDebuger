using ElectricPowerLib.Common;
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
        /// <summary>
        /// 解析 整数10进制数
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细："[unsigned,signed],[*,/],[倍数],[unsigned,signed,-unsigned]"</param>
        /// <param name="objValue">解析后的值 Int32/UInt32</param>
        /// <returns>解析后的字符串</returns>
        public static string GetIntDec(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;
            UInt32 u32Tmp;
            Int32 i32Tmp;
            string[] details;
            byte u8Tmp;
            bool isNegative = false;
            int signIdx = 0xFF;

            u32Tmp = 0;
            details = detail.Split(',');
            signIdx = length - 1;
            if (details.Length == 3 && details[2] == "-unsigned"
                && (buffer[index + signIdx] & 0x80) > 0)
            {
                isNegative = true;
            }
            for (i = 0; i < length; i++)
            {
                u8Tmp = (i == signIdx && isNegative ? (byte)(buffer[index] & 0x7F) : buffer[index]);
                u32Tmp += ((UInt32)u8Tmp << i * 8);
                index++;
            }

            i32Tmp = 0;
            if (details.Length == 3 && details[3] == "signed")
            {
                if (details[0] == "*")
                {
                    i32Tmp = (Int32)((Int32)u32Tmp * Convert.ToUInt32(details[1]));
                }
                else if (details[0] == "/")
                {
                    i32Tmp = (Int32)((Int32)u32Tmp / Convert.ToUInt32(details[1]));
                }
                value = i32Tmp.ToString();
                objValue = i32Tmp;

                return value;
            }

            if (details.Length >= 2)
            {
                if (details[0] == "*")
                {
                    u32Tmp = (UInt32)(u32Tmp * Convert.ToUInt32(details[1]));
                }
                else if (details[0] == "/")
                {
                    u32Tmp = (UInt32)(u32Tmp / Convert.ToUInt32(details[1]));
                }
            }

            if (isNegative)
            {
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
            UInt32 u32Tmp;

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
        /// <param name="detail">可选详细：小数位数及倍率 "[小数字节数],[*,/],[倍数],[unsigned,signed,-unsigned]" </param>
        /// <param name="objValue">解析后的值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetDouble(byte[] buffer, int index, int length, string detail, out object objValue)
        {
            string value = "";
            int i;
            UInt32 u32Tmp;
            Int32 i32Tmp;
            double doubleTmp;
            string[] details;
            byte u8Tmp;
            bool isNegative = false;
            int signIdx = 0xFF;

            details = detail.Split(',');
            int len = Convert.ToByte(details[0]);
            signIdx = (length > len ? length - len - 1 : length - 1);
            doubleTmp = 0;
            u32Tmp = 0;

            if (details.Length == 4 && details[3] == "-unsigned"
                && (buffer[index + signIdx] & 0x80) > 0)
            {
                isNegative = true;
            }
            for (i = 0; i < length - len; i++)
            {
                u8Tmp = (i == signIdx && isNegative ? (byte)(buffer[index] & 0x7F) : buffer[index]);
                u32Tmp += ((UInt32)u8Tmp << i * 8);
                index++;
            }
            if (length > len) signIdx = 0xFF;

            UInt32 u32Tmp2 = 0;
            for (i = 0; i < len; i++)
            {
                u8Tmp = (i == signIdx && isNegative ? (byte)(buffer[index] & 0x7F) : buffer[index]);
                u32Tmp2 += (UInt32)(u8Tmp << i * 8);
                index++;
            }

            if (details.Length == 4 && details[3] == "signed")
            {
                i32Tmp = (len == 2 ? (Int16)u32Tmp2 : (Int32)u32Tmp2);

                if (details[1] == "*")
                {
                    doubleTmp = (double)(i32Tmp * Convert.ToDouble(details[2]));
                }
                else if (details[1] == "/")
                {
                    doubleTmp = (double)(i32Tmp / Convert.ToDouble(details[2]));
                }
            }
            else if (details.Length >= 3)
            {
                if (details[1] == "*")
                {
                    doubleTmp = (double)(u32Tmp2 * Convert.ToDouble(details[2]));
                }
                else if (details[1] == "/")
                {
                    doubleTmp = (double)(u32Tmp2 / Convert.ToDouble(details[2]));
                }
            }

            doubleTmp = (double)u32Tmp + doubleTmp;
            if (isNegative)
            {
                value = "-" + doubleTmp;
                objValue = Convert.ToDouble(value);
            }
            else
            {
                value = doubleTmp.ToString();
                objValue = doubleTmp;
            }

            return value;
        }

        /// <summary>
        /// 解析 十六进制字符串（bytes to hex）
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="index">起始索引</param>
        /// <param name="length">长度</param>
        /// <param name="detail">可选详细：分隔符 "[Normal,Reverse],[ '', ',', ' ']" </param>
        /// <returns>解析后的字符串</returns>
        public static string GetHexStr(byte[] buffer, int index, int length, string detail)
        {
            string value = "";
            int i;
            UInt32 u32Tmp;
            string[] details;

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
        /// <returns>解析后的字符串</returns>
        public static string GetAscii(byte[] buffer, int index, int length, string detail)
        {
            string value = "";
            int i;
            UInt32 u32Tmp;
            string[] details;

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
        /// <param name="isReverse">是否反序</param>
        /// <param name="isBcd">是否BCD格式</param>
        /// <param name="detail">可选详细："[yy[yy]-MM-dd ][HH:mm[:ss]]" </param>
        /// <param name="objValue">解析后的time值</param>
        /// <returns>解析后的字符串</returns>
        public static string GetTime(byte[] buffer, int index, int length, bool isReverse, bool isBcd, string detail, out object objValue)
        {
            string value = "";
            int i;

            byte[] timeBuf = new byte[length];
            Array.Copy(buffer, index, timeBuf, 0, timeBuf.Length);
            if (isReverse) //  not bigEndian
            {
                Array.Reverse(timeBuf, 0, timeBuf.Length);
            }
            if (!isBcd)     // DataType.BIN
            {
                for (i = 0; i < timeBuf.Length; i++)
                {
                    timeBuf[i] = Util.DecToBcd(timeBuf[i]);
                }
            }

            try
            {
                string timeStr = detail;
                string strTmp;
                index = 0;
                if (timeStr.Contains("yyyy"))
                {
                    strTmp = timeBuf[index].ToString("X2") + timeBuf[index + 1].ToString("X2");
                    timeStr = timeStr.Replace("yyyy", strTmp);
                    index += 2;
                }
                if (timeStr.Contains("yy"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("yy", strTmp);
                }
                if (timeStr.Contains("MM"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("MM", strTmp);
                }
                if (timeStr.Contains("dd"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("dd", strTmp);
                }
                if (timeStr.Contains("HH"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("HH", strTmp);
                }
                if (timeStr.Contains("mm"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("mm", strTmp);
                }
                if (timeStr.Contains("ss"))
                {
                    strTmp = timeBuf[index++].ToString("X2");
                    timeStr = timeStr.Replace("ss", strTmp);
                }

                value = timeStr;
            }
            catch (Exception)
            {
                value = "解析错误";
            }
            DateTime.TryParse(value, out DateTime time);
            objValue = time;

            return value;
        }
    }
}

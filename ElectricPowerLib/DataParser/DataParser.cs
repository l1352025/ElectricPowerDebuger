﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using ElectricPowerLib.Common;

namespace ElectricPowerLib.DataParser
{

#pragma warning disable

    public enum DataType
    {
        BIN,
        BCD,
    }
    public enum ShowType
    {
        INT_DEC,        // 整数10进制显示, 需指定[乘以|除以]，[倍数]，[unsigned|signed|-unsigned]：*, 100，signed  
        INT_HEX,        // 整数16进制显示
        DOUBLE,         // 浮点数显示，需指定小数部分字节数，乘以|除以，倍数，[unsigned|signed|-unsigned]：2,/,1000，-unsigned(原码表示) 
        STR_ASCII,      // ASCII字符串显示
        STR_HEX,        // HEX字符串显示，详细需指定分隔符：""  / " "  /  ","
        STR_TIME,       // 时间显示，详细需指定时间格式：yyyy-MM-dd HH:mm:ss (年月日时分秒的部分或全部)
        STR_CASE,       // Case显示，详细需指定每个case含义：0-读取，1-设置，其他-未知 / 0xAA-成功，0xAB-失败，... ，其他-未知
        
        STR_VAR,        // 可变字符串显示，需调用回调函数填充字段内容
        DATA_ITEM       // 数据项显示，即包含0-N个字段，需调用回调函数填充字段内容
    }

    public delegate void SetFieldFunc(string funcName, DataField field);
    public class DataField
    {
        public string name;           // 字段名
        public byte[] buffer;          // 字段缓存：数据项数据缓存区
        public int offset;             // 字段索引：在数据缓存区的偏移量 = 数据项偏移量 + 前面所有字段长度
        public int length;             // 字段长度
        public bool bigEndian;         // 字段是否是大端模式：false-小端，true-大端
        public DataType dataType;     // 数据类型：参考 DataType
        public ShowType showTpye;     // 显示类型：参考 ShowType
        public string showDetail;     // 显示详细：参考 ShowType 注释
        public string showSuffix;     // 显示后缀：一般为单位（如 mA / L / m3）

        public DataItem dataItem;     // 数据项指针：该字段显示类型为数据项，即包含0-N个字段，需调用回调函数填充字段内容
        public string callbackName;   // 设置字段回调函数名称

        public string value;            // 显示值
        public object objValue;         // 数字值对象：如 int ，byte ，double

        public string HexStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = this.offset; i < (this.offset + this.length) && i < this.buffer.Length; i++)
                {
                    sb.Append(this.buffer[i].ToString("X2"));
                    sb.Append(' ');
                }
                return sb.ToString().TrimEnd(' ');
            }
        }

        public void Parse(byte[] fieldBuffer, int fieldOffset)
        {
            int i, index;
            UInt32 u32Tmp;
            UInt16 u16Tmp;
            Int32 i32Tmp;
            byte u8Tmp;
            bool isNegative = false;
            int signIdx = 0xFF;
            double doubleTmp;
            string[] details;

            this.buffer = fieldBuffer;
            this.offset = fieldOffset;

            if(this.buffer == null)
            {
                this.value = "缓冲区空";
                this.objValue = null;
                return;
            }

            if (this.offset + this.length > this.buffer.Length)
            {
                this.value = "";
                this.length = 0;
                return;
            }
            
            // 1.数据项: 回调函数解析
            if (this.showTpye == ShowType.DATA_ITEM)
            {
                if (this.dataItem == null)
                {
                    this.dataItem = new DataItem();
                }
                
                if (DataItem.callbackFunc != null && !string.IsNullOrEmpty(this.callbackName))
                {
                    DataItem.callbackFunc(this.callbackName, this);
                }
                return;
            }

            // 2.可变字段: 回调函数解析 
            if (this.showTpye == ShowType.STR_VAR)
            {
                if (DataItem.callbackFunc != null && !string.IsNullOrEmpty(this.callbackName))
                {
                    DataItem.callbackFunc(this.callbackName, this);
                }
                return;
            }

            // 3.具体字段：根据显示类型和显示详细解析
            switch (this.showTpye)
            {
                case ShowType.INT_DEC:
                    u32Tmp = 0;
                    index = this.offset;
                    details = this.showDetail.Split(',');
                    signIdx = this.length - 1;
                    if (details.Length == 3 && details[2] == "-unsigned"
                        && (this.buffer[index + signIdx] & 0x80) > 0)
                    {
                        isNegative = true;
                    }
                    for (i = 0; i < this.length; i++)
                    {
                        u8Tmp = (i == signIdx && isNegative ? (byte)(this.buffer[index] & 0x7F) : this.buffer[index]);
                        u32Tmp += ((UInt32)u8Tmp << i * 8);
                        index++;
                    }

                    i32Tmp = 0;
                    if (details.Length == 3 && details[2] == "signed")
                    {
                        if (details[0] == "*")
                        {
                            i32Tmp = (Int32)((Int32)u32Tmp * Convert.ToUInt32(details[1]));
                        }
                        else if (details[0] == "/")
                        {
                            i32Tmp = (Int32)((Int32)u32Tmp / Convert.ToUInt32(details[1]));
                        }
                        this.value = i32Tmp.ToString();
                        this.objValue = i32Tmp;

                        break;
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
                        this.value = "-" + u32Tmp;
                        this.objValue = Convert.ToInt32(this.value);
                    }
                    else
                    {
                        this.value = u32Tmp.ToString();
                        this.objValue = u32Tmp;
                    }
                    break;

                case ShowType.INT_HEX:
                    u32Tmp = 0;
                    index = this.offset;
                    for (i = 0; i < this.length; i++)
                    {
                        u32Tmp += ((UInt32)this.buffer[index++] << i * 8);
                    }

                    if (i == 4)
                    {
                        this.value = u32Tmp.ToString("X8");
                    }
                    else if (i == 2)
                    {
                        this.value = u32Tmp.ToString("X4");
                    }
                    else
                    {
                        this.value = u32Tmp.ToString("X2");
                    }
                    this.objValue = u32Tmp;
                    break;

                case ShowType.DOUBLE:
                    details = this.showDetail.Split(',');
                    int len = Convert.ToByte(details[0]);
                    signIdx = (this.length > len ? this.length - len - 1 : this.length - 1);
                    doubleTmp = 0;
                    u32Tmp = 0;
                    index = this.offset;
                    if (details.Length == 4 && details[3] == "-unsigned"
                        && (this.buffer[index + signIdx] & 0x80) > 0)
                    {
                        isNegative = true;
                    }
                    for (i = 0; i < this.length - len; i++)
                    {
                        u8Tmp = (i == signIdx && isNegative ? (byte)(this.buffer[index] & 0x7F) : this.buffer[index]);
                        u32Tmp += ((UInt32)u8Tmp << i * 8);
                        index++;
                    }
                    if(this.length > len) signIdx = 0xFF;

                    UInt32 u32Tmp2 = 0;
                    for (i = 0; i < len; i++)
                    {
                        u8Tmp = (i == signIdx && isNegative ? (byte)(this.buffer[index] & 0x7F) : this.buffer[index]);
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
                    else if(details.Length >= 3)
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
                    if (isNegative)     // -unsigned
                    {
                        this.value = "-" + doubleTmp;
                        this.objValue = Convert.ToDouble(this.value);
                    }
                    else                // unsigned / signed
                    {
						this.value = doubleTmp.ToString();
                        this.objValue = doubleTmp;
                    }
                    break;

                case ShowType.STR_ASCII:
                    this.value = Encoding.ASCII.GetString(this.buffer, this.offset, this.length);
                    break;

                case ShowType.STR_HEX:
                    this.value = Util.GetStringHexFromBytes(this.buffer, this.offset, this.length, this.showDetail, !this.bigEndian);
                    break;

                case ShowType.STR_TIME:
                    byte[] timeBuf = new byte[this.length];
                    Array.Copy(this.buffer, this.offset, timeBuf, 0, timeBuf.Length);
                    if (this.bigEndian == false)
                    {
                        Array.Reverse(timeBuf, 0, timeBuf.Length);
                    }
                    if (this.dataType == DataType.BIN)
                    {
                        for (i = 0; i < timeBuf.Length; i++)
                        {
                            timeBuf[i] = Util.DecToBcd(timeBuf[i]);
                        }
                    }

                    try
                    {
                        string timeStr = this.showDetail;
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

                        this.value = timeStr;
                    }
                    catch (Exception)
                    {
                        this.value = "解析错误";
                    }
                    DateTime.TryParse(value, out DateTime time);
                    objValue = time;
                    break;

                case ShowType.STR_CASE:
                    u8Tmp = this.buffer[this.offset];
                    details = this.showDetail.Trim().Split(',');
                    string[] item;
                    byte key;
                    foreach (string str in details)
                    {
                        item = str.Trim().Split('-');
                        if (item[0].Length == 1)
                        {
                            key = (byte)item[0][0];             // 单个字符：a-z,A-Z
                        }
                        else
                        {
                            key = Convert.ToByte(item[0], 16);  // 单字节HEX: 0x00-0xFF
                        }
                        if (key == u8Tmp)
                        {
                            this.value = item[1];
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(this.value))
                    {
                        this.value = "未知";
                    }
                    this.objValue = u8Tmp;
                    break;


                default:
                    break;
            }
        }

        // 显示值：字段名 + 字段值 + 单位
        public override string ToString() 
        {
            int strLength, padLength;
            string str = null;
            StringBuilder sb = new StringBuilder();

            if (this.showTpye == ShowType.DATA_ITEM && this.dataItem != null)
            {
                str = this.dataItem.ToString();
            }
            else
            {
                sb.Append(this.name);
                sb.Append("：");
                sb.Append(this.value);
                sb.Append(" ");
                sb.Append(this.showSuffix);
                str = sb.ToString();

                if (DataItem.isShowFieldHexStr)
                {
                    strLength = Encoding.Default.GetByteCount(str);

                    if (strLength < 30)
                    {
                        padLength = 30;
                    }
                    else if (strLength < 40)
                    {
                        padLength = 40;
                    }
                    else if (strLength < 50)
                    {
                        padLength = 50;
                    }
                    else
                    {
                        padLength = 60;
                    }

                    for (int i = 0; i < (padLength - strLength); i++)
                    {
                        sb.Append(' ');
                    }

                    sb.Append("【" + this.HexStr + "】");
                    str = sb.ToString();
                }
            }

            return str;
        }

    }

    public class DataItem
    {
        public int id;            // 数据项ID
        public string name;       // 数据项名
        public byte[] buffer;     // 数据项数据缓存区
        public int offset;        // 数据项在数据缓存区起始索引
        public int length;        // 数据项数据缓存区长度
        public List<DataField> fields;    // 字段列表

        public static SetFieldFunc callbackFunc; // 设置字段回调函数
        public static bool isShowFieldHexStr;    // 是否显示字段的Hex字符串

        public string ErrorMsg { get; set; }
        public string HexStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = this.offset; i < (this.offset + this.length) && i < this.buffer.Length; i++)
                {
                    sb.Append(this.buffer[i].ToString("X2"));
                    sb.Append(' ');
                }
                return sb.ToString();
            }
        }

        public DataField this[int index]
        {
            get
            {
                return this.fields[index];
            }
        }

        public DataField this[string fieldName]
        {
            get
            {
                return this.fields.Find(q => q.name.Contains(fieldName)); // 模糊查找，返回第一个
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataField field in this.fields)
            {
                sb.AppendLine(field.ToString());
            }

            return sb.ToString();
        }

        public void InitFromJson(string strJson)
        {
            DataItem item = JsonConvert.DeserializeObject<DataItem>(strJson);
            this.id = item.id;
            this.name = item.name;
            this.offset = item.offset;
            this.length = item.length;
            this.fields = item.fields;
        }

        public void Parse(byte[] buf, int index, int length)
        {
            int currOffset;

            this.buffer = buf;
            this.offset = index;
            this.length = length;   // 不确定，解析完后得到实际长度：currOffset - this.offset
            currOffset = this.offset;

            foreach (DataField field in this.fields)
            {
                try
                {
                    if (length > 1 && currOffset + field.length > (index + length))
                    {
                        field.buffer = buf;
                        field.offset = currOffset;
                        field.value = "";
                        field.length = 0;
                    }
                    else
                    {
                        field.Parse(buf, currOffset);
                    }
                    currOffset += field.length;
                }
                catch(Exception ex)
                {
                    field.value = "解析异常：" + ex.Message;
                    continue;
                }

            }// foreach

            this.length = currOffset - this.offset;     // 实际长度

        }

    }

#pragma warning restore
}

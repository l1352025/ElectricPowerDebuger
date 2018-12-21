using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricPowerDebuger.Common
{
    class Util
    {
        public static int GetByteAddrFromString(string strSource, byte[] DataByte, int iStart, bool bReverse = false)
        {
            byte tmp;
            int iLoop, iPos = iStart;
            for (iLoop = 0; iLoop < strSource.Length; )
            {
                DataByte[iPos++] = Convert.ToByte(strSource.Substring(iLoop, 2), 16);
                iLoop += 2;
            }
            if (true == bReverse)
            {
                for (iLoop = 0; iLoop < (iPos - iStart) / 2; iLoop++)
                {
                    tmp = DataByte[iLoop + iStart];
                    DataByte[iLoop + iStart] = DataByte[iPos - 1 - iLoop];
                    DataByte[iPos - 1 - iLoop] = tmp;
                }
            }
            return (iPos - iStart);
        }
        public static string GetStringHexFromByte(byte[] DataByte, int iStart, int iLength, string strSeparate = "", bool Reverse = false)
        {
            string strResult = "";
            for (int iLoop = 0; iLoop < iLength; iLoop++)
            {
                if (Reverse == true)
                {
                    strResult += DataByte[iStart + iLength - 1 - iLoop].ToString("X2") + strSeparate;
                }
                else
                {
                    strResult += DataByte[iStart + iLoop].ToString("X2") + strSeparate;
                }
            }
            strResult.Trim();
            return strResult;
        }
        public static byte[] GetDlt645Frame(byte[] addr, byte[] data)
        {
            byte[] frame = new byte[10 + data.Length];
            byte index = 0, crc = 0, iLoop;

            frame[index++] = 0x68;
            addr.CopyTo(frame, index);
            index += (byte)addr.Length;
            frame[index++] = 0x68;

            data.CopyTo(frame, index);
            index += (byte)data.Length;

            for (iLoop = 0; iLoop < index; iLoop++)
            {
                crc += frame[iLoop];
            }
            frame[index++] = crc;
            frame[index++] = 0x16;

            return frame;
        }
    }
}

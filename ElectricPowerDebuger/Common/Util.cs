using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace ElectricPowerDebuger.Common
{
    class Util
    {
		public static int GetBytesFromStringHex(string strSource, byte[] DataByte, int iStart, bool bReverse = false)
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

        public static byte[] GetBytesFromStringHex(string strSource, string strSeparate = "", bool bReverse = false)
        {
            byte tmp;
            int iLoop;
            byte[] bytes;

            if (strSeparate == "")
            {
                strSource = strSource.Trim();
            }
            else
            {
                strSource = strSource.Trim().Replace(strSeparate, "");
            }
            bytes = new byte[strSource.Length / 2];

            for (iLoop = 0; iLoop < bytes.Length; iLoop++)
            {
                bytes[iLoop] = Convert.ToByte(strSource.Substring(iLoop * 2, 2), 16);
            }
            if (true == bReverse)
            {
                for (iLoop = 0; iLoop < bytes.Length; iLoop++)
                {
                    tmp = bytes[iLoop];
                    bytes[iLoop] = bytes[bytes.Length - 1 - iLoop];
                    bytes[bytes.Length - 1 - iLoop] = tmp;
                }
            }

            return bytes;
        }

        public static string GetStringHexFromBytes(byte[] DataByte, int iStart, int iLength, string strSeparate = "", bool Reverse = false)
        {
            string strResult = "";
            
            if(DataByte == null)
            {
                return strResult;
            }

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

        public static byte BcdToDec(byte bcd)
        {
            return (byte)(bcd - (bcd >> 4) * 6);
        }

        public static byte DecToBcd(byte dec)
        {
            return (byte)(dec + (dec / 10) * 6);
        }

        public static int IndexOf(byte[] srcArray, byte[] bytes)
        {
            return IndexOf(srcArray, bytes, 0);
        }
        public static int IndexOf(byte[] srcArray, byte b)
        {
            return IndexOf(srcArray, new byte[] { b }, 0);
        }
        public static int IndexOf(byte[] srcArray, byte b, int startIndex)
        {
            return IndexOf(srcArray, new byte[] { b }, startIndex);
        }

        public static int IndexOf(byte[] srcArray, byte[] bytes, int startIndex)
        {
            int index = -1, i, j;

            if (bytes == null || bytes.Length == 0) return index;

            for (i = startIndex; i <= (srcArray.Length - bytes.Length); i++)
            {
                if (srcArray[i] == bytes[0])
                {
                    for (j = 0; j < bytes.Length; j++)
                    {
                        if (srcArray[i + j] != bytes[j])
                        {
                            break;
                        }
                    }

                    if (j == bytes.Length)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
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

#if true
        //将一个字节数组序列化为结构
        private IFormatter formatter = new BinaryFormatter();
        private ValueType deserializeByteArrayToInfoObj(byte[] bytes)
        {
            ValueType vt;
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                MemoryStream stream = new MemoryStream(bytes);
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
                vt = (ValueType)formatter.Deserialize(stream);
                stream.Close();
                return vt;
            }
            catch (Exception)
            {
                return null;
            }
        }
        //将一个结构序列化为字节数组
        private byte[] serializeInfoObjToByteArray(ValueType infoStruct)
        {
            if (infoStruct == null)
            {
                return null;
            }

            try
            {
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, infoStruct);

                byte[] bytes = new byte[(int)stream.Length];
                stream.Position = 0;
                int count = stream.Read(bytes, 0, (int)stream.Length);
                stream.Close();
                return bytes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// 将字节数组转换为结构体
        public object BytesToStruct(byte[] bytes, Type type)
        {
            //得到结构体大小
            int size = Marshal.SizeOf(type);
            Math.Log(size, 1);

            if (size > bytes.Length)
                return null;
            //分配结构大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将BYTE数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内容空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }
        /// 将结构转换为字节数组
        public byte[] StructTOBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            //创建byte数组
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷贝到分配好的内存空间
            Marshal.StructureToPtr(obj, structPtr, false);
            //从内存空间拷贝到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }
#endif

        /// <summary>
        /// 执行dos命令
        /// </summary>
        /// <param name="executablePath">cmd.exe路径</param>
        /// <param name="args">命令字符串/.bat文件/.cmd文件</param>
        /// <param name="workingFolder">命令执行的位置</param>
        /// <param name="ignoreErrorCode"></param>
        /// <returns></returns>
        public static string ExecuteWindowsCmd(string executablePath, string args, string workingFolder, bool ignoreErrorCode = true, bool ignoreOutput = true)
        {
            if (!Path.IsPathRooted(executablePath))
            {
                string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                executablePath = Path.Combine(executingDirectory, executablePath);
            }

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(executablePath, args);
                process.StartInfo.WorkingDirectory = (workingFolder != null ? workingFolder : "");
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                StringBuilder stdOutput = new StringBuilder();
                StringBuilder stdError = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null && !ignoreOutput)
                        {
                            stdOutput.AppendLine(e.Data);
                        }
                        else
                        {
                            outputWaitHandle.Set();
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null && !ignoreOutput)
                        {
                            stdError.AppendLine(e.Data);
                        }
                        else
                        {
                            errorWaitHandle.Set();
                        }
                    };

                    string processOutput = string.Empty;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(10000)
                        && outputWaitHandle.WaitOne(10000)
                        && errorWaitHandle.WaitOne(10000))
                    {
                        // Process is completed. 
                        processOutput = stdOutput.ToString() + stdError.ToString();
                        if (!ignoreErrorCode && process.ExitCode != 0)
                        {
                            throw new Exception(string.Format("{0} {1}, ExitCode {2}, Args {3}.", executablePath, args, process.ExitCode, processOutput));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Process running is time out in {0} s ", 10));
                    }

                    return processOutput;

                }
            }
        }

    }
}

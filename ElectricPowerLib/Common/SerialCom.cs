using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;

namespace ElectricPowerLib.Common
{
    public class SerialCom
    {
        public delegate void EventHandle(byte[] readBuffer); //接收数据处理函数委托
        public event EventHandle DataReceivedEvent;          //接收到数据引发事件
        public event EventHandler UnexpectedClosedEvent;     //端口异常关闭引发事件

        public SerialPort serialPort;   //串行端口
        Thread thread;                  //接收线程
        volatile bool _keepReading;     //接收线程控制标志
        volatile ArrayList readBuf = new ArrayList();

        /// <summary>
        /// 串口构造函数
        /// </summary>
        public SerialCom()
        {
            serialPort = new SerialPort();
            thread = null;
            _keepReading = false;
        }

        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 串口配置
        /// </summary>
        /// <param name="name">串口名</param>
        /// <param name="baudrate">波特率</param>
        /// <param name="bitsAndParity">数据位、停止位、校验
        /// “8N1” -- 8个数据位、1个停止位、无校验
        /// “8O1” -- 8个数据位、1个停止位、奇校验
        /// “8E1” -- 8个数据位、1个停止位、偶校验
        /// </param>
        public void Config(string name, int baudrate, string bitsAndParity = null)
        {
            int databits;
            StopBits stopbits;
            Parity parity;
            switch (bitsAndParity)
            {
                case "8N1":
                    parity = Parity.None;
                    databits = 8;
                    stopbits = StopBits.One;
                    break;

                case "8O1":
                    parity = Parity.Odd;
                    databits = 8;
                    stopbits = StopBits.One;
                    break;

                case "8E1":
                default:
                    parity = Parity.Even;
                    databits = 8;
                    stopbits = StopBits.One;
                    break;
            }

            Config(name, baudrate, databits, stopbits, parity);
        }
        public void Config(string name, int baudrate, int databits, StopBits stopbits, Parity parity)
        {
            serialPort.PortName = name;
            serialPort.BaudRate = baudrate;
            serialPort.DataBits = databits;
            serialPort.StopBits = stopbits;
            serialPort.Parity = parity;
        }

        /// <summary>
        /// 返回串口是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns>打开返回0 错误返回-1</returns>
        public int Open()
        {
            Close();
            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    serialPort.DiscardOutBuffer();  //清空发送缓冲区数据
                    serialPort.DiscardInBuffer();   //清空接收缓存区数据  
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            if (serialPort.IsOpen)
            {
                StartReading();
            }

            return 0;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>关闭返回0 错误返回-1</returns>
        public int Close()
        {
            try
            {
                StopReading();
                if(serialPort.IsOpen)
                {
                    serialPort.DiscardOutBuffer();  //清空发送缓冲区数据
                    serialPort.DiscardInBuffer();   //清空接收缓存区数据  
                }
                serialPort.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return 0;
        }

        /// <summary>
        /// 串口发送数据
        /// </summary>
        /// <param name="send">待发送的字节数组</param>
        /// <param name="offSet">数组偏移量</param>
        /// <param name="count">待发送的字节数</param>
        public void WritePort(byte[] send, int offSet, int count)
        {
            try
            {
                if (IsOpen)
                {
                    serialPort.Write(send, offSet, count);
                }               
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 开始接收串口数据
        /// </summary>
        private void StartReading()
        {
            if (!_keepReading)
            {
                readBuf.Clear();

                _keepReading = true;
                thread = new Thread(new ThreadStart(ReadPort));
                thread.Start();
            }
        }

        /// <summary>
        /// 停止接收串口数据
        /// </summary>
        private void StopReading()
        {
            if (_keepReading)
            {
                _keepReading = false;
                //thread.Join(); //等待thread线程终止
                thread.Abort();
                thread = null;
            }
        }

        /// <summary>
        /// 读串口数据线程函数
        /// </summary>
        private void ReadPort()
        {
            byte bRead = 0;

            DateTime dt = DateTime.Now;
            TimeSpan ts;

            int timeValue = 0;
            switch (serialPort.BaudRate)
            {
                case 1200: timeValue = 80; break;
                case 2400: timeValue = 60; break;
                case 4800: timeValue = 40; break;
                case 9600: timeValue = 40; break;
                case 19200: timeValue = 30; break;
                case 38400: timeValue = 30; break;
                case 56000: timeValue = 30; break;
                case 57600: timeValue = 30; break;
                case 115200: timeValue = 20; break;              
            }

            //timeValue = (int)((float)(10 * 11) / serialPort.BaudRate * 1000 + 0.5);

            while (_keepReading)
            {
                if (false == serialPort.IsOpen)
                {
                    _keepReading = false;
                    if(UnexpectedClosedEvent != null)
                    {
                        UnexpectedClosedEvent(null, null);
                    }
                    readBuf.Clear();
                    return;
                }

                try
                {
                    do
                    {
                        if (serialPort.BytesToRead > 0)
                        {
                            bRead = (byte)serialPort.ReadByte();
                            readBuf.Add(bRead);
                            dt = DateTime.Now;
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                        ts = DateTime.Now - dt;
                    } while (ts.TotalMilliseconds < timeValue);  //判断一帧是否已接收完
                }
                catch (Exception ex)
                {
                    string msg = "SerialCom.ReadPort() Error: " + ex.Message;
                    LogHelper.WriteLine("error.log", msg);
                }

                if (serialPort.IsOpen && DataReceivedEvent != null && readBuf.Count != 0)
                {
                    byte[] rxBuf = new byte[readBuf.Count];
                    readBuf.CopyTo(rxBuf);
                    DataReceivedEvent(rxBuf);
                    readBuf.Clear();
                }
                Thread.Sleep(10);
            }
        }
    }
}

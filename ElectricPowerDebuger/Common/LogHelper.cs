using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ElectricPowerDebuger.Common
{
    class LogHelper:IDisposable
    {
        private static ConcurrentQueue<string> _logQueue;
        private static Thread _logOutputTask;
        private static Semaphore _logSem;
        private static string _logFileName;

        public LogHelper(string logName)
        {
            _logFileName = logName;
            _logQueue = new ConcurrentQueue<string>();
            _logSem = new Semaphore(0, 100);

            _logOutputTask = new Thread(new ThreadStart(delegate 
            {
                string strLog;
                StreamWriter sw;
                while(_logOutputTask.IsAlive)
                {
                    _logSem.WaitOne();
                    sw = new StreamWriter(_logFileName, true, Encoding.UTF8);
                    while(_logQueue.Count > 0 && _logQueue.TryDequeue(out strLog))
                    {
                        sw.Write(strLog);
                    }
                    sw.Close();
                }
            }));

            _logOutputTask.IsBackground = true;
            _logOutputTask.Start();
        }

        public void Dispose()
        {
            if(_logOutputTask != null)
            {
                _logOutputTask.Abort();
                _logOutputTask = null;
            }

            if(_logQueue != null)
            {
                _logQueue = null;
            }

            if(_logSem != null)
            {
                _logSem.Close();
                _logSem = null;
            }
        }

        public void Write(string str)
        {
            Thread t = new Thread(new ThreadStart(delegate 
            {
                str = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]  ") + str;
                _logQueue.Enqueue(str);
                _logSem.Release(1);
            }));

            t.Start();
        }

        public void WriteLine(string str)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                str = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]  ") + str;
                _logQueue.Enqueue(str + "\r\n");
                _logSem.Release(1);
            }));

            t.Start();
        }

        // 静态函数
        public static void Write(string path, string str)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                str = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]  ") + str;
                using(StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
                {
                    sw.Write(str);
                }
            }));

            t.Start();
        }

        public static void WriteLine(string path, string str)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                str = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]  ") + str;
                using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
                {
                    sw.WriteLine(str);
                }
            }));

            t.Start();
        }
    }
}

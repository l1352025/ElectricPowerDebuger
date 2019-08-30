using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricPowerLib.Common
{
    /// <summary>
    /// 命令收发时使用的类
    /// </summary>
    public class Command
    {
        /// <summary>
        /// 命令处理的方法
        /// </summary>
        /// <param name="cmd"></param>
        public delegate void CommandHandler(Command cmd);

#pragma warning disable

        public string Name;   
        public string Comment;
        public byte[] TxBuf;
        public byte[] RxBuf;
        public List<object> Params;
        public int TimeWaitMS;
        public int RetryTimes;
        public int TxCnt;
        public string GrpName;
        public int GrpCmdCnt;
        public int GrpCmdTxCnt;
        public bool IsEnable;
        public bool IsNoResponse;
        public CommandHandler SendFunc;
        public CommandHandler RecvFunc;
        public CommandHandler TimeoutFunc;

        public Command()
            : this(null, null, null, null, 0, 1)
        {
        }
        public Command(string cmdName)
            : this(cmdName, null, null, null, 0, 1)
        {
        }
        public Command(string cmdName, CommandHandler sendFunc, CommandHandler recvFunc, CommandHandler timeoutFunc, int timeOut, int retryTimes)
        {
            Name = cmdName;
            TimeWaitMS = timeOut;
            RetryTimes = retryTimes;
            SendFunc = sendFunc;
            RecvFunc = recvFunc;
            TimeoutFunc = timeoutFunc;
            Params = new List<object>();
            TxCnt = 0;
            Comment = "";
            GrpName = cmdName;
            GrpCmdCnt = 1;
            GrpCmdTxCnt = 0;
        }

#pragma warning restore

    };
}

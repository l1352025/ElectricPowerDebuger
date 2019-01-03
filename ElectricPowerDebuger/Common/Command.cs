using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricPowerDebuger.Common
{

    delegate void CommandHandler(Command cmd);
    class Command
    {
        public string Name;
        public byte[] TxBuf;
        public byte[] RxBuf;
        public List<object> Params;
        public string GrpName;
        public int TimeWaitMS;
        public int RetryTimes;
        public bool IsEnable;
        public CommandHandler SendFunc;
        public CommandHandler RecvFunc;

        public Command()
            : this(null, null, null, 0, 1)
        {
        }
        public Command(string cmdName)
            : this(cmdName, null, null, 0, 1)
        {
        }
        public Command(string cmdName, CommandHandler sendFunc, CommandHandler recvFunc, int timeOut, int retryTimes)
        {
            Name = cmdName;
            TimeWaitMS = timeOut;
            RetryTimes = retryTimes;
            SendFunc = sendFunc;
            RecvFunc = recvFunc;
            Params = new List<object>();
            GrpName = "";
        }
    };
}

using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBase
{
    public static class Log
    {
        private static object _logQueueLock = new object();
        private static Queue<string> _logQueue = new Queue<string>();
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().Name);

        public static void AddLog(string format, params object[] list)
        {
            string log = string.Format(format, list);
            AddLog(log);
        }

        public static void AddLog(string log)
        {
            Monitor.Enter(_logQueueLock);
            _logQueue.Enqueue(log);
            Monitor.Exit(_logQueueLock);
        }

        public static string GetLog()
        {
            Monitor.Enter(_logQueueLock);
            string log = string.Empty;

            if (0 < _logQueue.Count)
                log = _logQueue.Dequeue(
                    );
            Monitor.Exit(_logQueueLock);

            return log;
        }

        public static int Count
        {
            get
            {
                Monitor.Enter(_logQueueLock);
                int count = _logQueue.Count;
                Monitor.Exit(_logQueueLock);

                return count;
            }
        }
    }
}

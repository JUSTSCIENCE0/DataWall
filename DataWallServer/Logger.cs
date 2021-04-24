using System;
using System.Threading;

namespace DataWallServer
{
    class Logger
    {
        private string console;
        private Mutex mtx;

        public Logger()
        {
            console = "";
            mtx = new Mutex();
        }

        public void msg(string Msg)
        {
            mtx.WaitOne();
            DateTime now = DateTime.Now;
            string time = now.Day.ToString("00") + "." +
                          now.Month.ToString("00") + "." +
                          now.Year.ToString() + " " +
                          now.Hour.ToString("00") + ":" +
                          now.Minute.ToString("00") + ":" +
                          now.Second.ToString("00") + ":" +
                          now.Millisecond.ToString("000") + " - ";
            console += time + Msg + Environment.NewLine;
            mtx.ReleaseMutex();
        }

        public string print()
        {
            mtx.WaitOne();
            string res = console;
            console = "";
            mtx.ReleaseMutex();

            return res;
        }
    }
}

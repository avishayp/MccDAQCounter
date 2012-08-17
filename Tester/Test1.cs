using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Runtime;
using CounterSender;
using Globals;

namespace Tester
{
    class Test1
    {
        public Test1()
        {
            Init();
        }

        public void RunTest(int interval, int timeout)
        {
            _interval = interval;
            _timeout = timeout;
            RunTest();
        }

        public void RunTest()
        {
            Console.WriteLine(String.Format("IP: {0}, port: {1}, runtime: {2} seconds, sampling interval = {3} milli", _ip.ToString(), _port.ToString(), _timeout, _interval));
            Console.WriteLine("{0}: Started, entering real-time mode...", DateTime.Now.ToString());

            SetAffinity(2);     // just because I've tested it on a duo-core...
            SetExitPoint();

            _reset = new ManualResetEvent(false);
            _csw = new CounterSenderWrapper();
            _csw.Init(_ip, _port);

            _csw.__CounterStopped += new NoParams(_csw___CounterStopped);

            _csw.StartPolling(_interval, _timeout);

            if (IsInfinite)
            {
                _reset.WaitOne();
            }
            else
            {
                if (!_reset.WaitOne(_timeout * 1000 * 2))
                { // exit without signaling
                    TryStop();
                }
            }

            Console.WriteLine("{0}: Ended, exiting real-time mode...", DateTime.Now.ToString());
            Console.WriteLine("Press any key to close this window...");
            Console.ReadKey();
        }

        private void Init()
        {
            _timeout = 60;  // sec
            _interval = 1;  // msec
            _ip = IPAddress.Parse("192.168.1.12");  //  data is sent to this address
            _port = 3650;

            IsInfinite = _timeout < 0;
        }

        private void SetAffinity(int proc)
        {
            using (Process p = Process.GetCurrentProcess())
            {
                p.ProcessorAffinity = new IntPtr(proc);
            }
        }

        private void SetExitPoint()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                Console.Write(ex.Message);
                Console.Write(ex.InnerException);
                Console.Write(ex.StackTrace);
            }
            TryStop();
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            TryStop();
        }

        private void TryStop()
        {
            if (_csw != null)
            {
                _csw.StopPolling();
            }
        }

        private void _csw___CounterStopped()
        {
            _reset.Set();
        }

        private IPAddress _ip;
        private int _port;
        private int _timeout;
        private int _interval;
        private ManualResetEvent _reset;
        private bool IsInfinite;

        private CounterSenderWrapper _csw;
    }

}

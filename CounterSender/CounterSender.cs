using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using HWInterface;
using MccCounter;
using System.IO;
using Globals;
using System.Diagnostics;

namespace CounterSender
{

    public class CounterSenderWrapper
    {
        // for counter
        private PollingSender _ps;

        public event NoParams __CounterStopped;
        private void InvokeCounterStopped()
        {
            if (__CounterStopped != null)
            {
                __CounterStopped();
            }
        }

        public bool IsInit { get; private set; }

        public bool Init(IPAddress IP, int port)
        {
            IsInit = false;
            try
            {
                IPAddress hostIP = IP;
                IPEndPoint ep = new IPEndPoint(hostIP, port);
                ISender sender = new UDPSender();
                sender.Connect(ep);
                ICounter counter = new CounterWrapper();
                counter.Init(0);
                _ps = new PollingSender(sender, counter);
                _ps.__Stopped += new NoParams(_ps___Stopped);
            }
            catch (Exception ex)
            {
                return false;
            }

            IsInit = true;
            return true;
        }

        void _ps___Stopped()
        {
            InvokeCounterStopped();
        }

        public void ResetCounter()
        {
            _ps.Reset();
        }

        public void StartPolling(int interval, int timeout)
        {
            _ps.StartPoll(interval, timeout);
        }

        public void StopPolling()
        {
            if (_ps != null)
            {
                _ps.StopPoll();
            }
        }

        class PollingSender
        {
            public event NoParams __Stopped;

            private void InvokeStopped()
            {
                if (__Stopped != null)
                {
                    __Stopped();
                }
            }

            public PollingSender(ISender sender, ICounter counter)
            {
                Init(sender, counter);
            }

            private void Init(ISender sender, ICounter counter)
            {
                _log = String.Empty;
                _buffer = new byte[8];
                _sender = sender;
                _counter = counter;
                _TW = new TimerWrapper();
                _TW.__Tick += new EventHandler(_TT___Tick);
                _TW.__Stopped += new NoParams(_TT___Stopped);
                if (_sender == null)
                {
                    throw new Exception("Attempt to init a pollimg-sender thread with null sender device.");
                }

                if (_counter == null)
                {
                    throw new Exception("Attempt to init a pollimg-sender thread with null polling device.");
                }
            }

            void _TT___Stopped()
            {
                WriteLog();
                InvokeStopped();
            }

            private String GetFileName()
            {
                return "Counter.log";
            }

            private void WriteLog()
            {
                File.WriteAllText(GetFileName(), _log); 
            }

            void _TT___Tick(object sender, EventArgs e)
            {
                SendData(GetCounts(), (float)((TimerEventArgs)e).Tick);
            }

            private byte[] _buffer;
            private ISender _sender;
            private ICounter _counter;
            private TimerWrapper _TW;
            private String _log;

            private void SendData(float count, float time)
            {
                _samples++;
                UpdateMsg(ref _buffer, count, time);
                _sender.Send(_buffer);
                Log(count, time);
            }

            private void Stop()
            {
                _TW.Stop();
            }

            public void Reset()
            {
                _counter.Reset();
            }

            public void StartPoll(int interval, int timeout)
            {
                _log += String.Format("{0}: Started polling - interval = {1} [ms], timeout = {2} [sec]{3}", DateTime.Now.ToString(),
                    interval, timeout, Environment.NewLine);

                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                _TW.Start(interval, timeout);
            }

            private float GetCounts()
            {
                return _counter.Read();
            }

            private void UpdateMsg(ref byte[] data, float count, float timestamp)
            {
                Array.Copy(BitConverter.GetBytes(count), 0, data, 4, 4);
                Array.Copy(BitConverter.GetBytes(timestamp), 0, data, 8, 4);
            }

            private ulong _samples = 0;

            private void Log(float x, float y)
            {
                _log += String.Format("{0}, {1}{2}", x, y, Environment.NewLine);
            }

            public void StopPoll()
            {
                this.Stop();
            }
        }

    }
}
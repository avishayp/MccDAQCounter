using System;
using System.Collections.Generic;
using Globals;

namespace HWInterface
{
    public class TimerEventArgs : EventArgs
    {
        public int Tick { get; set; } 
    }

    public interface ITimer
    {
        event EventHandler __Tick;
        event NoParams __Stopped;

        void Start(int interval, int timeout);
        void Stop();
    }

    public class ITimer_moq
    {
        public event EventHandler __Tick;
        public event NoParams __Stopped;

        public void Start(int interval, int timeout) { }
        public void Stop() { }
    }
}

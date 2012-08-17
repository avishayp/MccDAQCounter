using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HWInterface;
using System.Runtime.InteropServices;
using Globals;

namespace CounterSender
{

    public class TimerWrapper : ITimer
    {
        #region DLL IMPORT
        [DllImport("WinMM.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution,
                    TimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("WinMM.dll", SetLastError = true)]
        static extern uint timeKillEvent(uint timerEventId);

        [DllImport("Winmm.dll")]
        private static extern int timeGetTime();
        #endregion

        public event EventHandler __Tick;
        public event NoParams __Stopped;

        public delegate void TimerEventHandler(uint id, uint msg, ref int userCtx,
            int rsv1, int rsv2);

        public void Stop()
        {
            timeKillEvent(m_fastTimer);
            InvokeStopped();
        }

        public void Start(int interval, int timeout)
        {
            m_res = 0;
            m_count = 0;
            _startCount = timeGetTime();
            m_maxCount = timeout * 1000 / interval;
            m_interval = interval;
            int myData = 0;	// dummy data
            _thandler = new TimerEventHandler(tickHandler);
            m_fastTimer = timeSetEvent(interval, interval, _thandler,
                ref myData, 1);	// type=periodic
        }

        private void InvokeTick(int t)
        {
            if (__Tick != null)
            {
                __Tick(null, new TimerEventArgs() { Tick = t });
            }
        }

        private void InvokeStopped()
        {
            if (__Stopped != null)
            {
                __Stopped();
            }
        }

        private long m_maxCount;
        private int m_interval;
        private uint m_fastTimer;
        private long m_count;
        private int _startCount;
        private int _maxTimerTTL = 1000;    // ms
        private int m_res;
        private TimerEventHandler _thandler;

        private bool IsInfinite { get { return m_maxCount < 0; } }
        private bool IsRestarted { get { return m_interval <= 100 && m_interval >= 5; } }

        private void tickHandler(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            int span = timeGetTime() - _startCount;
            InvokeTick(span + m_res);

            if (m_count++ >= m_maxCount && !IsInfinite)
            {
                Stop();
            }
            else if (IsRestarted && span > _maxTimerTTL)
            {
                m_res += span;
                RestartTimer();
                _startCount = timeGetTime();
            }
        }

        private void RestartTimer()
        {
            int myData = 0;	// dummy data
            timeKillEvent(m_fastTimer);
            m_fastTimer = timeSetEvent(m_interval, m_interval, new TimerEventHandler(tickHandler), ref myData, 1);	// type=periodic
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using HWInterface;
using Globals;

namespace MccCounter
{
    public class CounterWrapper : ICounter
    {
        private MccDAQCounter _counter;
        private int _count, _prev;
        private bool _isOK;

        public bool IsOK
        {
            get { return _isOK; }
            set
            {
                if (_isOK != value)
                {
                    _isOK = value;
                    if (!_isOK)
                    {
                        InvokeError();
                    }
                }
            }
        }

        public event NoParams __CounterOverflow;

        private bool IsInit { get; set; }
        public bool Init(object cfg)
        {
            InternalInit();
            IsInit = _counter.Init((int)cfg);
            if (!IsInit)
            {
                InvokeError();
            }
            else
            {
                InvokeError();
            }
            return IsInit;
        }

        public int Counts { get { return _count; } }

        public void Reset() 
        {
            _prev = _count = 0;
            if (IsInit)
                _counter.Reset(); 
        }

        private void InvokeError()
        {
            throw new Exception(_counter.ULStat.Message);
        }

        private void InvokeOverflow()
        {
            if (__CounterOverflow != null)
            {
                __CounterOverflow();
            }
        }

        private void InternalInit()
        {
            _isOK = true;
            IsInit = false;
            _counter = new MccDAQCounter();
            _count = _prev = 0;
        }

        public int Read()
        {
            int temp = (int)_counter.Read();
            if (IsOK = _counter.IsOK)
            {
                _prev = _count;
                _count = temp;
                CheckOverflow();
            }
            return _count;
        }

        private void CheckOverflow()
        {
            if (_prev > _count)
            {
                InvokeOverflow();
            }
        }

    }
}

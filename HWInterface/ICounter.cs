using System;
using System.Collections.Generic;
using Globals;

namespace HWInterface
{    

    public interface ICounter
    {
        event NoParams __CounterOverflow;

        bool Init(object cfg);
        int Read();
        void Reset();
    }

    public class ICounter_moq : ICounter
    {
        public event NoParams __CounterOverflow;

        public bool Init(object cfg) { return true; }
        public int Read() { return 0; }
        public void Reset() { }
    }
}

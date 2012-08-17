using System;
using System.Collections.Generic;
using System.Text;
using MccDaq;

namespace MccCounter
{
    public class MccDAQCounter
    {
        private uint _count;
        private MccBoard _daqBoard;
        public ErrorInfo ULStat;

        private CounterRegister _RegName;
        private const int _LoadValue = 0;       // MccDAQ support only loading 0 to counter (used for reset)
        private const short _counterNum = 1;    // MccDAQ has one counter

        public bool IsOK { get { return ULStat.Value == ErrorInfo.ErrorCode.NoErrors; } }

        public bool Init(int board)
        {
            _count = 0;
            try
            {
                ULStat = MccService.ErrHandling(ErrorReporting.DontPrint, ErrorHandling.DontStop);
                _daqBoard = new MccBoard(board);
                if (_daqBoard == null)
                {
                    return false;
                }

                _RegName = CounterRegister.LoadReg1; //  register name of counter 1
                Reset();
                Read();
            }
            catch (Exception ex)
            {
                return false;            	
            }
            return IsOK;
        }

        public void Reset()
        {
            ULStat = _daqBoard.CLoad32(_RegName, _LoadValue);
        }

        public uint Read()
        {
            ULStat = _daqBoard.CIn32(_counterNum, out _count);
            return IsOK ? _count : 0;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Net;

namespace HWInterface
{
    public interface ISender
    {
        int Send(byte[] b);
        void Connect(EndPoint ipe);
    }


}

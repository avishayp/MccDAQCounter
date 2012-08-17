using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using HWInterface;

namespace CounterSender
{

    public class UDPSender : ISender
    {
        public UDPSender()
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private Socket _sock;
        private bool _isConnected;

        public int Send(Byte[] data)
        {
            return _isConnected ? _sock.Send(data) : -1;
        }

        public void Connect(EndPoint ipe)
        {
            try
            {
                _sock.Connect(ipe);
                _isConnected = true;
            }
            catch
            {
                _isConnected = false;
            }
        }
    }

}

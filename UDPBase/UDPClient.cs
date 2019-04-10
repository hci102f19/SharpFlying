using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase
{
    public class UDPClient
    {
        protected readonly UdpClient Client = new UdpClient();
        protected IPEndPoint EndPoint;

        protected const string HELOMessage = "HELO";
        protected const string BYEMessage = "K-BYE";

        protected const int Timeout = 3000;

        protected bool IsConnected = false;
        protected int PacketsDropped;

        public UDPClient(string host, int port)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        public void Connect()
        {
            if (IsConnected)
                return;
            Client.Client.ReceiveTimeout = Timeout;

            Client.Connect(EndPoint);
            SendHELO();
        }

        protected void SendHELO()
        {
            var sendBuffer = Encoding.UTF8.GetBytes(HELOMessage);
            Client.Send(sendBuffer, sendBuffer.Length);
        }

        public string ReceiveData()
        {
            try
            {
                var receivedData = Encoding.UTF8.GetString(Client.Receive(ref EndPoint));
                PacketsDropped = 0;

                if (!IsConnected)
                {
                    if (receivedData == HELOMessage)
                        IsConnected = true;
                    else
                        throw new Exception("Server did not acknowledge client"); // TODO: Exception which makes sense
                }
                else if (receivedData == BYEMessage)
                    throw new Exception("Server is stopping."); // TODO: Exception which makes sense

                return receivedData;
            }
            catch (SocketException e)
            {
                if (PacketsDropped++ >= 3)
                    throw new Exception("Server stopped responding."); // TODO: Exception which makes sense
            }

            return null;
        }
    }
}
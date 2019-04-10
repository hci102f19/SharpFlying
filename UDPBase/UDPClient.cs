using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPBase.exceptions;

namespace UDPBase
{
    public class UDPClient
    {
        protected const string HELOMessage = "HELO";
        protected const string BYEMessage = "K-BYE";

        protected const int Timeout = 1000;
        protected readonly UdpClient Client = new UdpClient();
        protected IPEndPoint EndPoint;

        protected bool IsConnected;
        protected int ReconnectionCount;
        protected int PacketsDropped;

        public UDPClient(string host, int port)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            Client.Client.ReceiveTimeout = Timeout;
        }

        public bool ReConnect()
        {
            if (++ReconnectionCount >= 3)
            {
                Console.WriteLine("The program was unable to reconnect to server.");
                return false;
            }

            PacketsDropped = 0;
            IsConnected = false;

            Connect();
            return true;
        }

        public void Connect()
        {
            if (IsConnected)
                return;
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
                ResetChecks();

                if (!IsConnected)
                {
                    if (receivedData == HELOMessage)
                    {
                        IsConnected = true;
                        return null;
                    }
                    else
                        throw new NoAcknowledgement("Server did not acknowledge client");
                }
                else if (receivedData == BYEMessage)
                {
                    throw new ServerStopping("Server is stopping.");
                }

                return receivedData;
            }
            catch (SocketException e)
            {
                if (PacketsDropped++ >= 3)
                    throw new ServerStoppedResponding("Server stopped responding.");
            }

            return null;
        }

        protected void ResetChecks()
        {
            PacketsDropped = 0;
            ReconnectionCount = 0;
        }
    }
}
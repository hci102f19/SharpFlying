using System;
using ServiceLib;
using UDPBase;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected readonly UDPClient Client = new UDPClient("192.168.1.102", 20002);

        public UltraSonicService()
        {
            IgnoreInput = true;
        }


        protected override void Run()
        {
            Client.Connect();

            while (IsRunning)
            {
                var data = Client.ReceiveData();
                if (data == null)
                    continue;

                Deserialize(data);
            }
        }

        protected void Deserialize(string data)
        {
            Console.WriteLine("Ultra Sanic: " + data);
            // Network = JsonConvert.DeserializeObject<Network>(data);
            //CalculatePosition();
        }

        private void CalculatePosition()
        {
            Console.WriteLine("TEST");
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading;
using BebopFlying.Bebop_Classes;
using BebopFlying.Bebop_Classes.Structs;
using Flight.Enums;
using FlightLib;

namespace BebopFlying
{
    //todo: Check hvis tråde er i live
    public class Bebop : IFly
    {
        //Logger
        private static NLog.Logger _logger;

        //Log to ensure 
        private static readonly object ThisLock = new object();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly int[] _seq = new int[256];

        private UdpClient _arstreamClient;
        private CancellationToken _cancelToken;

        //Command struct used for sending commands to the drone
        private Command _cmd;

        //UDP client to send data to the drone
        private UdpClient _droneUdpClient;

        //Bebop vector set by the move command to fly
        private Vector _flyVector;
        private IPEndPoint _remoteIpEndPoint;
        private byte[] _receivedData;

        /// <summary>
        ///     Initializes the bebop object at a specific updateRate
        /// </summary>
        /// <param name="updateRate">Numer of updates per second</param>
        public Bebop(int updateRate)
        {
            if (updateRate <= 0) throw new ArgumentOutOfRangeException(nameof(updateRate));
            _logger = NLog.LogManager.GetCurrentClassLogger();
            Updaterate = 1000 / updateRate;
        }

        protected int Updaterate { get; }


        public void TakeOff()
        {
            _logger.Debug("Performing takeoff...");
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING;
            _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_TAKEOFF;
            _cmd.cmd[3] = 0;

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void Landing()
        {
            _logger.Debug("Landing...");
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING;
            _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_LANDING;
            _cmd.cmd[3] = 0;

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void Move(Vector flightVector)
        {
            _flyVector = flightVector;
        }

        public bool IsAlive()
        {
            throw new NotImplementedException();
        }

        public ConnectionStatus Connect()
        {
            _logger.Debug("Attempting to connect to drone...");

            //Initialize the drone udp client
            _droneUdpClient = new UdpClient(CommandSet.IP, 54321);

            //make handshake with TCP_client, and the port is set to be 4444
            var tcpClient = new TcpClient(CommandSet.IP, CommandSet.DISCOVERY_PORT);
            //Initialize the network stream for the handshake
            var stream = new NetworkStream(tcpClient.Client);

            //initialize reader and writer
            var streamWriter = new StreamWriter(stream);
            var streamReader = new StreamReader(stream);

            //when the drone receive the message bellow, it will return the confirmation
            streamWriter.WriteLine(CommandSet.HandshakeMessage);
            streamWriter.Flush();

            var receiveMessage = streamReader.ReadLine();

            if (receiveMessage == null)
            {
                _logger.Fatal("Connection failed");
                return ConnectionStatus.Failed;
            }

            _logger.Debug("The message from the drone shows: " + receiveMessage);

            //initialize
            _cmd = default(Command);
            _flyVector = default(Vector);

            //All State setting
            GenerateAllStates();
            GenerateAllSettings();

            //enable video streaming
            VideoEnable();

            //init ARStream
            InitArStream();
            //initARStream();

            //init CancellationToken
            _cancelToken = _cts.Token;

            //todo: TRÅD OG SMART TRÅD HANDLING HER
            //PcmdThreadActive();


            //arStreamThreadActive();
            return ConnectionStatus.Success;
        }

        private void SendCommand(ref Command cmd, int type = CommandSet.ARNETWORKAL_FRAME_TYPE_DATA,
            int id = CommandSet.BD_NET_CD_NONACK_ID)
        {
            var bufSize = cmd.size + 7;
            var buf = new byte[bufSize];

            _seq[id]++;
            if (_seq[id] > 255) _seq[id] = 0;

            buf[0] = (byte) type;
            buf[1] = (byte) id;
            buf[2] = (byte) _seq[id];
            buf[3] = (byte) (bufSize & 0xff);
            buf[4] = (byte) ((bufSize & 0xff00) >> 8);
            buf[5] = (byte) ((bufSize & 0xff0000) >> 16);
            buf[6] = (byte) ((bufSize & 0xff000000) >> 24);

            cmd.cmd.CopyTo(buf, 7);


            _droneUdpClient.Send(buf, buf.Length);
            lock (ThisLock)
            {
                _flyVector.Flag = 0;
                _flyVector.Pitch = 0;
                _flyVector.Roll = 0;
                _flyVector.Yaw = 0;
            }
        }

        private void GenerateAllStates()
        {
            _logger.Debug("Generated all states");
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMON;
            _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff;
            _cmd.cmd[3] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & (0xff00 >> 8);

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        private void GenerateAllSettings()
        {
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_SETTINGS;
            _cmd.cmd[2] = 0 & 0xff; // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_ALLSETTINGS = 0
            _cmd.cmd[3] = 0 & (0xff00 >> 8);

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void VideoEnable()
        {
            _cmd = default(Command);
            _cmd.size = 5;
            _cmd.cmd = new byte[5];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_MEDIASTREAMING;
            _cmd.cmd[2] = 0 & 0xff; // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_VIDEOENABLE = 0
            _cmd.cmd[3] = 0 & (0xff00 >> 8);
            _cmd.cmd[4] = 1; //arg: Enable

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void InitArStream()
        {
            _arstreamClient = new UdpClient(55004);
            _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        }

        public byte[] GetImageData()
        {
            _receivedData = _arstreamClient.Receive(ref _remoteIpEndPoint);
            return _receivedData;
        }

        public void CancelAllTasks()
        {
            _cts.Cancel();
        }
    }
}
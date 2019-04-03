using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BebopFlying.Bebop_Classes;
using BebopFlying.Bebop_Classes.Structs;
using Flight.Enums;
using FlightLib;
using NLog;

namespace BebopFlying
{
    public class Bebop : IFly
    {
        //Logger
        private static NLog.Logger _logger;

        //Log to ensure that access to flyvector is fine during multithreading
        private static readonly object ThisLock = new object();

        private readonly int[] _seq = new int[256];

        //Command struct used for sending commands to the drone
        private Command _cmd;

        //UDP client to send data to the drone
        private UdpClient _droneUdpClient;
        private UdpClient _droneDataClient;
        private UdpClient _arstreamClient;
        private IPEndPoint _remoteIpEndPoint;

        private IPEndPoint _droneData = new IPEndPoint(IPAddress.Any, 43210);

        //Bebop vector set by the move command to fly
        private Vector _flyVector = new Vector();

        private Thread _commandGeneratorThread;
        private Thread _threadWatcher;
        private Thread _StreamReader;

        /// <summary>
        ///     Initializes the bebop object at a specific updaterate
        /// </summary>
        /// <param name="updaterate">Numer of updates per second</param>
        public Bebop(int updaterate)
        {
            if (updaterate <= 0) throw new ArgumentOutOfRangeException(nameof(updaterate));
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") {FileName = "BebopFileLog.txt"};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            NLog.LogManager.Configuration = config;
            _logger = NLog.LogManager.GetCurrentClassLogger();
            Updaterate = 1000 / updaterate;
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
            return _threadWatcher.IsAlive;
        }

        private StreamReader streamReader;

        /// <summary>
        /// Connects to the drone
        /// </summary>
        /// <returns>The connection status of the connect attempt</returns>
        public ConnectionStatus Connect()
        {
            try
            {
                _logger.Debug("Attempting to connect to drone...");
                //Initialize the drone udp client
                _droneUdpClient = new UdpClient(CommandSet.IP, 54321);
                _droneDataClient = new UdpClient(CommandSet.IP, 43210);

                //make handshake with TCP_client, and the port is set to be 4444
                var tcpClient = new TcpClient(CommandSet.IP, CommandSet.DISCOVERY_PORT);

                //Initialize the network stream for the handshake
                var stream = new NetworkStream(tcpClient.Client);

                //initialize reader and writer
                var streamWriter = new StreamWriter(stream);
                streamReader = new StreamReader(stream);
                //when the drone receive the message below, it will return the confirmation
                streamWriter.WriteLine(CommandSet.HandshakeMessage);
                streamWriter.Flush();
                _StreamReader = new Thread(ReadDroneOutput);

                var droneHandshakeResponse = streamReader.ReadLine();

                if (droneHandshakeResponse == null)
                {
                    _logger.Fatal("Connection failed");
                    return ConnectionStatus.Failed;
                }

                _logger.Debug("The message from the drone shows: " + droneHandshakeResponse);

                //initialize command struct and movement vector
                _cmd = default(Command);
                _flyVector = new Vector();


                //All State setting
                AskForStateUpdate();
                GenerateAllSettings();

                //enable video streaming
                VideoEnable();
                //InitArStream();
            }
            catch (SocketException ex)
            {
                _logger.Fatal(ex.Message);
                throw;
            }

            _commandGeneratorThread = new Thread(PcmdThreadActive);
            _commandGeneratorThread.Start();
            _threadWatcher = new Thread(ThreadManager);
            _threadWatcher.Start();
            _StreamReader.Start();
            _logger.Debug("Successfully connected to the drone");
            return ConnectionStatus.Success;
        }

        private void CreateSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.ReceiveTimeout = 5000;

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            socket.Bind(_droneData);

            Console.WriteLine(socket.Connected == false);
            socket.Connect(CommandSet.IP, CommandSet.D2C_PORT);
            Console.WriteLine(socket.Connected == true);

            _droneDataClient.Client = socket;
        }

        private void ReadDroneOutput()
        {
            CreateSocket();
            byte[] data = new byte[0];
            string message = "";
            while (true)
            {
                Console.WriteLine("READING?");
                try
                {
                    AskForStateUpdate();
                    data = _droneDataClient.Receive(ref _droneData);
                    message = Encoding.ASCII.GetString(data);
                    Console.WriteLine(message);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10060)
                    {
                        // Handle the error. 10060 is a timeout error, which is expected.
                    }
                }
                finally
                {
                    SmartSleep(100);
                }
            }
        }

        /// <summary>
        /// Sends a command to the drone
        /// </summary>
        /// <param name="cmd">The command to send</param>
        /// <param name="type">The type of command to send, defaults to a fly command</param>
        /// <param name="id">The id of the command, defaults to not receiving an acknowledge</param>
        private void SendCommand(ref Command cmd, int type = CommandSet.ARNETWORKAL_FRAME_TYPE_DATA, int id = CommandSet.BD_NET_CD_NONACK_ID)
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

            //Send buffer to drone
            _droneUdpClient.Send(buf, buf.Length);

            //Reset flyvector
            lock (ThisLock)
            {
                _flyVector.Flag = 0;
                _flyVector.Pitch = 0;
                _flyVector.Roll = 0;
                _flyVector.Yaw = 0;
            }
        }

        private void AskForStateUpdate()
        {
            //_logger.Debug("Generated all states");
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMON;
            _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff;
            _cmd.cmd[3] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & (0xff00 >> 8);

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        private void PcmdThreadActive()
        {
            _logger.Debug("Started command generator thread");
            while (true)
            {
                GenerateDroneCommand();
                SmartSleep(Updaterate);
            }
        }


        private void ThreadManager()
        {
            _logger.Debug("Started Threadwatcher");
            while (true)
            {
                if (_commandGeneratorThread.IsAlive)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    _logger.Fatal("Bebop command thread is not alive, initializing emergency procedure!");
                    Landing();
                }
            }
        }

        /// <summary>
        /// Generates the command for the drone
        /// </summary>
        private void GenerateDroneCommand()
        {
            lock (ThisLock)
            {
                //if(_flyVector.IsNull())return;
                _cmd = default(Command);
                _cmd.size = 13;
                _cmd.cmd = new byte[13];

                _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3;
                _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING;
                _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_PCMD;
                _cmd.cmd[3] = 0;
                _cmd.cmd[4] = (byte) _flyVector.Flag; // flag
                _cmd.cmd[5] = _flyVector.Roll >= 0 ? (byte) _flyVector.Roll : (byte) (256 + _flyVector.Roll); // roll: fly left or right [-100 ~ 100]
                _cmd.cmd[6] = _flyVector.Pitch >= 0 ? (byte) _flyVector.Pitch : (byte) (256 + _flyVector.Pitch); // pitch: backward or forward [-100 ~ 100]
                _cmd.cmd[7] = _flyVector.Yaw >= 0 ? (byte) _flyVector.Yaw : (byte) (256 + _flyVector.Yaw); // yaw: rotate left or right [-100 ~ 100]
                _cmd.cmd[8] = _flyVector.Gaz >= 0 ? (byte) _flyVector.Gaz : (byte) (256 + _flyVector.Gaz); // gaze: down or up [-100 ~ 100]

                // for Debug Mode
                _cmd.cmd[9] = 0;
                _cmd.cmd[10] = 0;
                _cmd.cmd[11] = 0;
                _cmd.cmd[12] = 0;

                SendCommand(ref _cmd);
            }
        }

        /// <summary>
        /// Busy sleeps for the specified amount of time.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to sleep</param>
        private void SmartSleep(int milliseconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < milliseconds)
            {
                Thread.Yield();
            }

            sw.Stop();
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

        public void InitArStream()
        {
            _arstreamClient = new UdpClient(55004);
            _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        private void VideoEnable()
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
    }
}
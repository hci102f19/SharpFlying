using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Threading;
using BebopFlying.BebopClasses;
using Flight.Enums;
using FlightLib;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BebopFlying
{
    public class Bebop : IFly
    {
        #region Properties

        //Logger
        protected static Logger _logger;

        //Log to ensure that access to flyvector is fine during multithreading
        protected static readonly object ThisLock = new object();

        protected int MaxPacketRetries = 1;

        //Dictionary for storing secquence counter
        protected readonly Dictionary<string, int> sequenceCounter = new Dictionary<string, int>
        {
            {"PONG", 0},
            {"SEND_NO_ACK", 0},
            {"SEND_WITH_ACK", 0},
            {"SEND_HIGH_PRIORITY", 0},
            {"VIDEO_ACK", 0},
            {"ACK_DRONE_DATA", 0},
            {"NO_ACK_DRONE_DATA", 0},
            {"VIDEO_DATA", 0}
        };

        protected readonly Dictionary<string, int> BufferIds = new Dictionary<string, int>()
        {
            {"PING", 0},
            {"PONG", 1},
            {"SEND_NO_ACK", 10},
            {"SEND_WITH_ACK", 11},
            {"SEND_HIGH_PRIORITY", 12},
            {"VIDEO_ACK", 13},
            {"ACK_DRONE_DATA", 127},
            {"NO_ACK_DRONE_DATA", 126},
            {"VIDEO_DATA", 125},
            {"ACK_FROM_SEND_WITH_ACK", 139}
        };

        protected readonly Dictionary<string, int> DataTypesByName = new Dictionary<string, int>()
        {
            {"ACK", 1},
            {"DATA_NO_ACK", 2},
            {"LOW_LATENCY_DATA", 3},
            {"DATA_WITH_ACK", 4}
        };

        //Command struct used for sending commands to the drone
        protected Thread CommandGeneratorThread;
        protected Thread StreamReaderThread;
        protected Thread ThreadWatcher;

        protected IPEndPoint DroneData = new IPEndPoint(IPAddress.Any, 43210);
        protected UdpClient DroneDataClient = new UdpClient(CommandSet.IP, 43210);

        //UDP client to send data to the drone
        protected UdpClient DroneUdpClient = new UdpClient(CommandSet.IP, 54321);

        protected Vector FlyVector = new Vector();

        public bool IsRunning { get; protected set; } = true;

        #endregion

        public Bebop()
        {
            LoggingConfiguration config = new LoggingConfiguration();
            FileTarget logfile = new FileTarget("logfile") {FileName = "BebopFileLog.txt"};

//            ConsoleTarget logconsole = new ConsoleTarget("logconsole");
//            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        #region Connection

        public ConnectionStatus Connect()
        {
            try
            {
                _logger.Debug("Attempting to connect to drone...");

                if (!DoHandshake())
                    return ConnectionStatus.Failed;
            }
            catch (SocketException ex)
            {
                _logger.Fatal(ex.Message);
                throw;
            }

            CommandGeneratorThread = new Thread(PcmdThreadActive);
            ThreadWatcher = new Thread(ThreadManager);
            StreamReaderThread = new Thread(ReadDroneOutput);

            CommandGeneratorThread.Start();
            ThreadWatcher.Start();
            StreamReaderThread.Start();

            _logger.Debug("Successfully connected to the drone");
            return ConnectionStatus.Success;
        }

        protected bool DoHandshake()
        {
            //make handshake with TCP_client, and the port is set to be 4444
            TcpClient tcpClient = new TcpClient(CommandSet.IP, CommandSet.DISCOVERY_PORT);

            //Initialize the network stream for the handshake
            NetworkStream stream = new NetworkStream(tcpClient.Client);

            //initialize reader and writer
            StreamWriter streamWriter = new StreamWriter(stream);
            StreamReader streamReader = new StreamReader(stream);

            //when the drone receive the message below, it will return the confirmation
            streamWriter.WriteLine(CommandSet.HandshakeMessage);
            streamWriter.Flush();

            string droneHandshakeResponse = streamReader.ReadLine();

            if (droneHandshakeResponse == null)
            {
                _logger.Fatal("Connection failed");
                return false;
            }

            streamWriter.Close();
            streamReader.Close();
            stream.Close();
            tcpClient.Close();

            return true;
        }

        public void Disconnect()
        {
            IsRunning = false;

            SmartSleep(500);

            DroneUdpClient.Close();
            DroneDataClient.Close();
        }

        #endregion

        #region Intercation

        public void TakeOff()
        {
            _logger.Debug("Performing takeoff...");
            // TODO: Look into Command, and sending
            Command _cmd = new Command(4, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);

            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_TAKEOFF);
            _cmd.InsertData(0);

            SendCommand(_cmd);
        }

        public void Land()
        {
            _logger.Debug("Landing...");

            // TODO: Look into Command, and sending
            Command _cmd = new Command(4, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);

            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_LANDING);
            _cmd.InsertData(0);

            SendCommand(_cmd);
        }

        public void Move(Vector flightVector)
        {
            FlyVector = flightVector;
        }

        public void StartVideo()
        {
            Command _cmd = new Command(5, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);

            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_MEDIASTREAMING);
            _cmd.InsertData(0 & 0xff); // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_VIDEOENABLE = 0
            _cmd.InsertData(0 & (0xff00 >> 8));
            _cmd.InsertData(1); //arg: Enable

            SendCommand(_cmd);
        }

        public void StopVideo()
        {
        }

        #endregion

        #region Public Info

        public bool IsAlive()
        {
            return ThreadWatcher.IsAlive;
        }

        #endregion

        #region Dronedata Handling

        protected void CreateSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = 5000, ReceiveBufferSize = 66000
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            socket.Bind(DroneData);

            DroneDataClient.Client = socket;
        }

        protected void ReadDroneOutput()
        {
            CreateSocket();
            while (IsRunning)
            {
                try
                {
                    var data = DroneDataClient.Receive(ref DroneData);
                    HandleData(data);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10060)
                    {
                        _logger.Fatal("Socket exception " + ex.Message);
                    }
                    else
                    {
                        _logger.Debug("Timed out - Trying again");
                    }
                }
            }
        }

        protected void HandleData(byte[] data)
        {
            const int size = 7;
            while (data.Length > 0)
            {
                int dataType = data[0],
                    bufferId = data[1],
                    packetSeqId = data[2],
                    packetSize = BitConverter.ToInt32(data, 3);

                //Extract the non-header data from the drone
                byte[] currentFrame = data.Skip(size).Take(packetSize - size).ToArray();

                //Handle frame data
                HandleFrame(dataType, bufferId, packetSeqId, currentFrame);

                //Skip extracted data to handle remaining packet size
                data = data.Skip(packetSize).ToArray();
            }
        }

        protected void HandleFrame(int dataType, int bufferId, int packetSeqId, byte[] data)
        {
            //If the drone is pinging us -> Send back pong
            if (bufferId == CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PING)
            {
                SendPong(data);
                _logger.Debug("Pong");
            }

            switch (dataType)
            {
                //Drone is asking for us to acknowledge the receival of the packet
                case CommandSet.ARNETWORKAL_FRAME_TYPE_ACK:
                    int ackSeqNumber = data[0];

                    CommandReceiver.SetCommandReceived("SEND_WITH_ACK", ackSeqNumber, true);

                    AckPacket(bufferId, ackSeqNumber);
                    _logger.Debug("Send Ack");
                    break;

                // Drone just sent us sensor data
                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA:
                    if (bufferId == CommandSet.BD_NET_DC_NAVDATA_ID || bufferId == CommandSet.BD_NET_DC_EVENT_ID)
                        UpdateSensorData(dataType, bufferId, packetSeqId, data, false);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY:
                    _logger.Debug("Handle low latency data?");
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK:
                    if (bufferId == CommandSet.BD_NET_DC_NAVDATA_ID || bufferId == CommandSet.BD_NET_DC_EVENT_ID)
                        UpdateSensorData(dataType, bufferId, packetSeqId, data, true);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_MAX:
                    _logger.Debug("Received a maxframe(Technically unknown?)");
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_UNINITIALIZED:
                    _logger.Debug("Received an uninitialized frame");
                    break;

                default:
                    _logger.Fatal("Unknown data type received from drone!");
                    break;
            }
        }

        #endregion

        #region Update Data

        // TODO: Test data
        protected List<string> states = new List<string>()
        {
            "landed", "takingoff", "hovering", "flying", "landing", "emergency", "usertakeoff", "motor_ramping",
            "emergency_landing"
        };

        protected void UpdateSensorData(int dataType, int bufferId, int packetSeqId, byte[] data, bool ack)
        {
            _logger.Debug("Sensor update");

            int projectId = (byte) data[0], classId = (byte) data[1], cmdId = BitConverter.ToInt16(data, 2);

            const int offset = 4;
            byte[] sensorData = data.Skip(offset).ToArray();

            if (projectId == 1 && classId == 4 && cmdId == 1)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("Length: {0}", sensorData.Length);
                Console.WriteLine("ALL: {0}", String.Join(", ", data));
                Console.WriteLine("FLYING: {0}", (byte) sensorData[0]);
                Console.WriteLine("FLYING State: {0}", states[(byte) sensorData[0]]);
            }
            else if (projectId == 0 && classId == 5 && cmdId == 1)
            {
                Console.WriteLine("BATTERY: {0}%", (byte) sensorData[0]);
            }

            if (ack)
                AckPacket(bufferId, packetSeqId);
        }

        #endregion

        #region Drone Responses

        protected void AckPacket(int bufferId, int packetId)
        {
            int newBufferId = (bufferId + 128) % 256;

            Command packet = new Command(5, id: newBufferId, padding: false);

            packet.InsertData(CommandSet.ARNETWORKAL_FRAME_TYPE_ACK);
            packet.InsertData((byte) newBufferId);
            packet.InsertData((byte) (packet.SequenceID() + 1) % 256);
            packet.InsertData(8);
            packet.InsertData((byte) packetId);

            SafeSend(packet.ExportCommand());
        }

        protected void SendPong(byte[] data)
        {
            int size = data.Length;

            int seq = sequenceCounter["PONG"];

            sequenceCounter["PONG"] = seq + 1 % 256;

            Command packet = new Command(size + 7);

            packet.InsertData(CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PONG);
            packet.InsertData(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA);
            packet.InsertData((byte) sequenceCounter["PONG"]);
            packet.InsertData((byte) (size + 7));
            packet.CopyData(data, 4);

            SendCommand(packet);
        }

        #endregion

        #region Send Data To Drone

        protected bool SendCommand(Command cmd)
        {
            int tryNum = 0;
            CommandReceiver.SetCommandReceived("SEND_WITH_ACK", cmd.SequenceID(), false);

            while (tryNum < MaxPacketRetries && !CommandReceiver.IsCommandReceived("SEND_WITH_ACK", cmd.SequenceID()))
            {
                _logger.Debug("Trying to send package to drone.");
                SafeSend(cmd.ExportCommand());

                tryNum++;

                SmartSleep(500);
            }

            //Reset flyvector
            lock (ThisLock)
            {
                FlyVector.ResetVector();
            }

            return CommandReceiver.IsCommandReceived("SEND_WITH_ACK", cmd.SequenceID());
        }

        protected void SafeSend(byte[] buffer)
        {
            bool packetSent = false;
            int tryNum = 0;

            while (!packetSent && tryNum < MaxPacketRetries)
            {
                try
                {
                    DroneUdpClient.Send(buffer, buffer.Length);
                    packetSent = true;
                }
                catch (Exception)
                {
                    // TODO: Reconnect.
                    tryNum++;
                }
            }
        }

        #endregion

        #region Threades

        /// <summary>
        /// Drone command thread
        /// Generates and sends movement commands received
        /// </summary>
        protected void PcmdThreadActive()
        {
            _logger.Debug("Started command generator thread");
            while (IsRunning)
            {
                GenerateDroneCommand();
                Thread.Sleep(1000 / 30);
            }
        }


        /// <summary>
        /// Threadwatcher to make sure the command generation thread is alive and well
        /// </summary>
        protected void ThreadManager()
        {
            _logger.Debug("Started Threadwatcher");
            while (IsRunning)
            {
                if (CommandGeneratorThread.IsAlive)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    _logger.Fatal("Bebop command thread is not alive, initializing emergency procedure!");
                    Land();
                }
            }
        }

        #endregion

        #region Movement Generation

        /// <summary>
        ///     Generates the command for the drone
        /// </summary>
        protected void GenerateDroneCommand()
        {
            lock (ThisLock)
            {
                //if(_flyVector.IsNull())return;
                Command _cmd = new Command(13);

                _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
                _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
                _cmd.InsertData(CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_PCMD);
                _cmd.InsertData(0);
                _cmd.InsertData((byte) FlyVector.Flag); // flag
                _cmd.InsertData(FlyVector.Roll >= 0
                    ? (byte) FlyVector.Roll
                    : (byte) (256 + FlyVector.Roll)); // roll: fly left or right [-100 ~ 100]
                _cmd.InsertData(FlyVector.Pitch >= 0
                    ? (byte) FlyVector.Pitch
                    : (byte) (256 + FlyVector.Pitch)); // pitch: backward or forward [-100 ~ 100]
                _cmd.InsertData(FlyVector.Yaw >= 0
                    ? (byte) FlyVector.Yaw
                    : (byte) (256 + FlyVector.Yaw)); // yaw: rotate left or right [-100 ~ 100]
                _cmd.InsertData(FlyVector.Gaz >= 0
                    ? (byte) FlyVector.Gaz
                    : (byte) (256 + FlyVector.Gaz)); // gaze: down or up [-100 ~ 100]

                // for Debug Mode
                _cmd.InsertData(0);
                _cmd.InsertData(0);
                _cmd.InsertData(0);
                _cmd.InsertData(0);

                SendCommand(_cmd);
            }
        }

        #endregion

        #region Sleep & Updates

        public void SmartSleep(int milliseconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < milliseconds)
                Thread.Sleep(10);

            sw.Stop();
        }

        public void AskForStateUpdate()
        {
            Command _cmd = new Command(3, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);

            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_COMMON);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMON);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff);

            throw new NotImplementedException();
            // SendNoParamCommandPacketAck(_cmd);
        }

        protected void GenerateAllSettings()
        {
            Command _cmd = new Command(4, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);

            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_PROJECT_COMMON);
            _cmd.InsertData(CommandSet.ARCOMMANDS_ID_COMMON_CLASS_SETTINGS);
            _cmd.InsertData(0 & 0xff); // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_ALLSETTINGS = 0
            _cmd.InsertData(0 & (0xff00 >> 8));

            SendCommand(_cmd);
        }

        #endregion
    }
}
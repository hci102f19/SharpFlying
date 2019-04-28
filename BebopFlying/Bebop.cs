using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BebopFlying.BebopClasses;
using BebopFlying.Sensors;
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
        protected static Logger Logger;

        //Log to ensure that access to fly vector is fine during multi threading
        protected static readonly object ThisLock = new object();

        protected const int MaxPacketRetries = 1;
        protected const int HandleSize = 7;
        protected const int HandleOffset = 4;
        protected const int VectorMin = -100, VectorMax = 100;



        //Dictionary for storing sequence counter
        protected readonly Dictionary<string, int> SequenceCounter = new Dictionary<string, int>
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
        protected UdpClient DroneDataClient = new UdpClient(CommandSet.Ip, 43210);

        //UDP client to send data to the drone
        protected UdpClient DroneUdpClient = new UdpClient(CommandSet.Ip, 54321);

        protected Vector FlyVector = new Vector();

        public bool IsRunning { get; protected set; } = true;

        public Battery Battery { get; protected set; } = new Battery(0, 5, 1);
        public FlyingState FlyingState { get; protected set; } = new FlyingState(1, 4, 1);
        public Altitude Altitude { get; protected set; } = new Altitude(1, 4, 8);

        protected List<Sensor> Sensors;

        #endregion

        public Bebop()
        {
            LoggingConfiguration config = new LoggingConfiguration();
            FileTarget logfile = new FileTarget("logfile") {FileName = "BebopFileLog.txt"};

            ConsoleTarget consoleLog = new ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleLog);

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            Logger = LogManager.GetCurrentClassLogger();

            // Set Bebop sensors
            Sensors = new List<Sensor>()
            {
                Battery,
                FlyingState,
                Altitude
            };
        }

        #region Connection

        public ConnectionStatus Connect()
        {
            try
            {
                Logger.Debug("Attempting to connect to drone...");

                if (!DoHandshake())
                    return ConnectionStatus.Failed;
            }
            catch (SocketException ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }

            CommandGeneratorThread = new Thread(PcmdThreadActive);
            ThreadWatcher = new Thread(ThreadManager);
            StreamReaderThread = new Thread(ReadDroneOutput);

            CommandGeneratorThread.Start();
            ThreadWatcher.Start();
            StreamReaderThread.Start();

            AskForStateUpdate();

            Logger.Debug("Successfully connected to the drone");
            return ConnectionStatus.Success;
        }

        protected bool DoHandshake()
        {
            //make handshake with TCP_client, and the port is set to be 4444
            TcpClient tcpClient = new TcpClient(CommandSet.Ip, CommandSet.DISCOVERY_PORT);

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
                Logger.Fatal("Connection failed");
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
            TakeOff(5000);
        }

        public void TakeOff(int timeout)
        {
            Logger.Debug("Performing takeoff...");
            CommandTuple cmdTuple = new CommandTuple(1, 0, 1);

            if (FlyingState.GetState() == FlyingState.State.Landed || FlyingState.GetState() == FlyingState.State.UnKn0wn)
                SendNoParam(cmdTuple);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (FlyingState.GetState() != FlyingState.State.Hovering && FlyingState.GetState() != FlyingState.State.Flying && sw.ElapsedMilliseconds < timeout)
            {
                if (FlyingState.GetState() == FlyingState.State.Emergency)
                    break;
                SmartSleep(100);
            }

            sw.Stop();
        }

        public void Land()
        {
            Logger.Debug("Landing...");
            CommandTuple cmdTuple = new CommandTuple(1, 0, 3);

            SendNoParam(cmdTuple);
        }

        public void Move(Vector flightVector)
        {
            lock (ThisLock)
            {
                FlyVector = flightVector;
            }
        }

        public void StartVideo()
        {
            CommandTuple cmdTuple = new CommandTuple(1, 21, 0);
            CommandParam cmdParam = new CommandParam();
            cmdParam.AddData((byte) 1); // Enable Video

            SendParam(cmdTuple, cmdParam);
        }

        public void StopVideo()
        {
            CommandTuple cmdTuple = new CommandTuple(1, 21, 0);
            CommandParam cmdParam = new CommandParam();
            cmdParam.AddData((byte) 0); // Disable Video

            SendParam(cmdTuple, cmdParam);
        }

        #endregion

        #region Public Info

        public bool IsAlive()
        {
            return ThreadWatcher.IsAlive;
        }

        public bool IsLanded()
        {
            return FlyingState.GetState() == FlyingState.State.Emergency || FlyingState.GetState() == FlyingState.State.Landed;
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
                    byte[] data = DroneDataClient.Receive(ref DroneData);
                    HandleData(data);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10060)
                    {
                        Logger.Fatal("Socket exception " + ex.Message);
                    }
                    else
                    {
                        Logger.Debug("Timed out - Trying again");
                    }
                }
            }
        }

        protected void HandleData(byte[] data)
        {
            while (data.Length > 0)
            {
                int dataType = data[0],
                    bufferId = data[1],
                    packetSeqId = data[2],
                    packetSize = BitConverter.ToInt32(data, 3);

                //Extract the non-header data from the drone
                byte[] currentFrame = data.Skip(HandleSize).Take(packetSize - HandleSize).ToArray();

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
                Logger.Debug("Pong");
            }

            switch (dataType)
            {
                //Drone is asking for us to acknowledge the retrieval of the packet
                case CommandSet.ARNETWORKAL_FRAME_TYPE_ACK:
                    int ackSeqNumber = data[0];

                    CommandReceiver.SetCommandReceived("SEND_WITH_ACK", ackSeqNumber, true);
                    AckPacket(bufferId, ackSeqNumber);
                    Logger.Debug("Send Ack");
                    break;

                // Drone just sent us sensor data
                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA:
                    if (bufferId == CommandSet.BD_NET_DC_NAVDATA_ID || bufferId == CommandSet.BD_NET_DC_EVENT_ID)
                        UpdateSensorData(dataType, bufferId, packetSeqId, data, false);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY:
                    Logger.Debug("Handle low latency data?");
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK:
                    if (bufferId == CommandSet.BD_NET_DC_NAVDATA_ID || bufferId == CommandSet.BD_NET_DC_EVENT_ID)
                        UpdateSensorData(dataType, bufferId, packetSeqId, data, true);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_MAX:
                    Logger.Debug("Received a maxframe(Technically unknown?)");
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_UNINITIALIZED:
                    Logger.Debug("Received an uninitialized frame");
                    break;

                default:
                    Logger.Fatal("Unknown data type received from drone!");
                    break;
            }
        }

        #endregion

        #region Update Data

        protected void UpdateSensorData(int dataType, int bufferId, int packetSeqId, byte[] data, bool ack)
        {
            Logger.Debug("Sensor update");
            if (data.Length == 0)
            {
                Logger.Error("DATA IS NULL YOU FACKERS!");
                return;
            }

            int projectId = data[0], classId = data[1], cmdId = BitConverter.ToInt16(data, 2);

            foreach (Sensor sensor in Sensors)
                if (sensor.Apply(projectId, classId, cmdId))
                    sensor.Parse(data.Skip(HandleOffset).ToArray());

            if (ack)
                AckPacket(bufferId, packetSeqId);
        }

        #endregion

        #region Drone Responses

        protected void AckPacket(int bufferId, int packetId)
        {
            // ReSharper disable once StringLiteralTypo
            const string fmt = "<BBBIB";
            int newBufferId = (bufferId + 128) % 256;

            Command cmd = new Command {SequenceId = newBufferId};

            cmd.InsertData((byte) DataTypesByName["ACK"]);
            cmd.InsertData((byte) newBufferId);
            cmd.InsertData((byte) cmd.SequenceId);
            cmd.InsertData((uint) StructConverter.PacketSize(fmt));
            cmd.InsertData((byte) packetId);

            SafeSend(cmd.Export(fmt));
        }

        protected void SendPong(byte[] data)
        {
            int byteSize = 4, totalByteSize = byteSize + data.Length;
            // ReSharper disable once StringLiteralTypo
            const string fmt = "<BBBI";

            SequenceCounter["PONG"] = (SequenceCounter["PONG"] + 1) % 256;

            Command cmd = new Command();
            cmd.InsertData((byte) DataTypesByName["DATA_NO_ACK"]);
            cmd.InsertData((byte) BufferIds["PONG"]);
            cmd.InsertData((byte) SequenceCounter["PONG"]);
            cmd.InsertData((uint) (StructConverter.PacketSize(fmt) + data.Length));

            byte[] initCommand = cmd.Export(fmt);
            Array.Resize(ref initCommand, totalByteSize);

            data.CopyTo(initCommand, byteSize);

            SafeSend(initCommand);
        }

        #endregion

        #region Send Data To Drone

        protected bool SendNoParam(CommandTuple cmdTuple)
        {
            SequenceCounter["SEND_WITH_ACK"] = (SequenceCounter["SEND_WITH_ACK"] + 1) % 256;
            // ReSharper disable once StringLiteralTypo
            const string fmt = "<BBBIBBH";

            Command cmd = new Command();

            cmd.InsertData((byte) DataTypesByName["DATA_WITH_ACK"]);
            cmd.InsertData((byte) BufferIds["SEND_WITH_ACK"]);
            cmd.InsertData((byte) SequenceCounter["SEND_WITH_ACK"]);
            cmd.InsertData((uint) StructConverter.PacketSize(fmt));
            cmd.InsertTuple(cmdTuple);

            return SendCommandAck(cmd.Export(fmt), SequenceCounter["SEND_WITH_ACK"]);
        }

        protected bool? SendParam(CommandTuple cmdTuple, CommandParam cmdParam, bool ack = true)
        {
            // ReSharper disable once InconsistentNaming
            string ACK = (ack) ? "SEND_WITH_ACK" : "SEND_NO_ACK";
            // ReSharper disable once InconsistentNaming
            string DataACK = (ack) ? "DATA_WITH_ACK" : "DATA_NO_ACK";
            // ReSharper disable once StringLiteralTypo
            string fmt = "<BBBIBBH" + cmdParam.Format();

            SequenceCounter[ACK] = (SequenceCounter[ACK] + 1) % 256;

            Command cmd = new Command();
            cmd.InsertData((byte) DataTypesByName[DataACK]);
            cmd.InsertData((byte) BufferIds[ACK]);
            cmd.InsertData((byte) SequenceCounter[ACK]);
            cmd.InsertData((uint) StructConverter.PacketSize(fmt));

            cmd.InsertTuple(cmdTuple);
            cmd.InsertParam(cmdParam);

            if (ack)
                return SendCommandAck(cmd.Export(fmt), SequenceCounter["SEND_WITH_ACK"]);
            SendCommandNoAck(cmd.Export(fmt));
            return null;
        }

        // ReSharper disable once IdentifierTypo
        protected bool SendSinglePcmd(CommandTuple cmdTuple, CommandParam cmdParam)
        {
            SequenceCounter["SEND_NO_ACK"] = (SequenceCounter["SEND_NO_ACK"] + 1) % 256;
            // ReSharper disable once StringLiteralTypo
            string fmt = "<BBBIBBH" + cmdParam.Format();

            Command cmd = new Command();

            cmd.InsertData((byte) DataTypesByName["DATA_NO_ACK"]);
            cmd.InsertData((byte) BufferIds["SEND_NO_ACK"]);
            cmd.InsertData((byte) SequenceCounter["SEND_NO_ACK"]);
            cmd.InsertData((uint) StructConverter.PacketSize(fmt));

            cmd.InsertTuple(cmdTuple);
            cmd.InsertParam(cmdParam);

            return SafeSend(cmd.Export(fmt));
        }

        protected bool SendCommandAck(byte[] packet, int sequenceId)
        {
            int tryNum = 0;
            CommandReceiver.SetCommandReceived("SEND_WITH_ACK", sequenceId, false);

            while (tryNum < MaxPacketRetries && !CommandReceiver.IsCommandReceived("SEND_WITH_ACK", sequenceId))
            {
                SafeSend(packet);
                tryNum++;
                SmartSleep(500);
            }

            return CommandReceiver.IsCommandReceived("SEND_WITH_ACK", sequenceId);
        }

        private void SendCommandNoAck(byte[] packet)
        {
            SafeSend(packet);
        }


        protected bool SafeSend(byte[] packet)
        {
            bool packetSent = false;
            int tryNum = 0;

            while (!packetSent && tryNum < MaxPacketRetries)
            {
                try
                {
                    DroneUdpClient.Send(packet, packet.Length);
                    packetSent = true;
                }
                catch (Exception)
                {
                    // TODO: Reconnect.
                    tryNum++;
                }
            }

            return packetSent;
        }

        #endregion

        #region Threades

        // ReSharper disable once IdentifierTypo
        protected void PcmdThreadActive()
        {
            Logger.Debug("Started command generator thread");
            while (IsRunning)
            {
                GenerateDroneCommand();
                Thread.Sleep(1000 / 30);
            }
        }


        protected void ThreadManager()
        {
            Logger.Debug("Started Threadwatcher");
            while (IsRunning)
            {
                if (CommandGeneratorThread.IsAlive)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    Logger.Fatal("Bebop command thread is not alive, initializing emergency procedure!");
                    Land();
                }
            }
        }

        #endregion

        #region Movement Generation

        protected static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        protected void GenerateDroneCommand()
        {
            lock (ThisLock)
            {
                if (FlyVector.IsNull())
                    return;

                CommandTuple cmdTuple = new CommandTuple(1, 0, 2);

                int roll = Clamp(FlyVector.Roll, VectorMin, VectorMax);
                int pitch = Clamp(FlyVector.Pitch, VectorMin, VectorMax);
                int yaw = Clamp(FlyVector.Yaw, VectorMin, VectorMax);
                int gaz = Clamp(FlyVector.Gaz, VectorMin, VectorMax);

                CommandParam cmdParam = new CommandParam();

                cmdParam.AddData((byte) 1);
                cmdParam.AddData((sbyte) roll);
                cmdParam.AddData((sbyte) pitch);
                cmdParam.AddData((sbyte) yaw);
                cmdParam.AddData((sbyte) gaz);
                cmdParam.AddData((uint) 0);

                SendSinglePcmd(cmdTuple, cmdParam);
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
            CommandTuple cmdTuple = new CommandTuple(0, 4, 0);

            SendNoParam(cmdTuple);
        }

        #endregion
    }
}
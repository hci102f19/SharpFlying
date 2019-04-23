using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BebopFlying.BebopClasses;
using BebopFlying.BebopClasses.Structs;
using Flight.Enums;
using FlightLib;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BebopFlying
{
    public class Bebop : IFly
    {
        //Logger
        private static Logger _logger;

        //Log to ensure that access to flyvector is fine during multithreading
        private static readonly object ThisLock = new object();

        private readonly Dictionary<Tuple<string, int>, bool> _commandReceived = new Dictionary<Tuple<string, int>, bool>();

        // TODO: Look at using int instead.
        private readonly int[] _seq = new int[256];

        protected int MaxPacketRetries = 1;

        //Dictionary for storing secquence counter
        private readonly Dictionary<string, int> _sequenceDictionary = new Dictionary<string, int>
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

        private UdpClient _arstreamClient;

        //Command struct used for sending commands to the drone
        private Command _cmd;

        private Thread _commandGeneratorThread;

        private IPEndPoint _droneData = new IPEndPoint(IPAddress.Any, 43210);
        private UdpClient _droneDataClient;

        //UDP client to send data to the drone
        private UdpClient _droneUdpClient;

        //Bebop vector set by the move command to fly
        private Vector _flyVector = new Vector();
        private IPEndPoint _remoteIpEndPoint;

        private Thread _StreamReaderThread;
        private Thread _threadWatcher;

        public bool IsRunning { get; protected set; } = true;

        private StreamReader _streamReader;


        /// <summary>
        ///     Initializes the bebop object at a specific updaterate
        /// </summary>
        /// <param name="updaterate">Number of updates per second</param>
        public Bebop(int updaterate)
        {
            if (updaterate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(updaterate));
            }

            LoggingConfiguration config = new LoggingConfiguration();
            FileTarget logfile = new FileTarget("logfile") {FileName = "BebopFileLog.txt"};

            //ConsoleTarget logconsole = new ConsoleTarget("logconsole");
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();

            Updaterate = 1000 / updaterate;
        }

        protected int Updaterate { get; }

        #region Public methods

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

        public void Land()
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

        /// <summary>
        ///     Connects to the drone
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
                TcpClient tcpClient = new TcpClient(CommandSet.IP, CommandSet.DISCOVERY_PORT);

                //Initialize the network stream for the handshake
                NetworkStream stream = new NetworkStream(tcpClient.Client);

                //initialize reader and writer
                StreamWriter streamWriter = new StreamWriter(stream);
                _streamReader = new StreamReader(stream);
                //when the drone receive the message below, it will return the confirmation
                streamWriter.WriteLine(CommandSet.HandshakeMessage);
                streamWriter.Flush();
                _StreamReaderThread = new Thread(ReadDroneOutput);

                string droneHandshakeResponse = _streamReader.ReadLine();

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
            _StreamReaderThread.Start();
            _logger.Debug("Successfully connected to the drone");
            return ConnectionStatus.Success;
        }

        public void Disconnect()
        {
            IsRunning = false;

            SmartSleep(500);

            _droneUdpClient.Close();
            _droneDataClient.Close();
        }

        #endregion


        #region Dronedata Handling

        private void CreateSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.ReceiveTimeout = 5000;

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            socket.Bind(_droneData);

            _droneDataClient.Client = socket;
        }

        /// <summary>
        /// Method that reads the output from the drone and handles the data accordingly
        /// </summary>
        private void ReadDroneOutput()
        {
            CreateSocket();
            byte[] data = new byte[0];
            while (IsRunning)
            {
                try
                {
                    data = _droneDataClient.Receive(ref _droneData);

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
                finally
                {
                    SmartSleep(100);
                }
            }
        }

        /// <summary>
        /// Based on the data, extracts the framedata 
        /// </summary>
        /// <param name="data">full data</param>
        private void HandleData(byte[] data)
        {
            const int size = 7;
            while (data.Length > 7)
            {
                //Unpact struct from array
                var datalist = StructConverter.Unpack("<BBBI", data.Take(7).ToArray());

                //Extract datatypes
                byte datatype = (byte) datalist[0];
                byte bufferID = (byte) datalist[1];
                byte packetSeqID = (byte) datalist[2];

                //Int behaves weirdly, so have to convert explicitly
                object packetSize = datalist[3];
                uint intermediateConv = (uint) packetSize;
                int actualSize = (int) intermediateConv;
                //todo:Test against drone
                if (actualSize > 1000)
                {
                    Console.WriteLine("Conversion error");
                }

                //Extract the non-header data from the drone
                byte[] recvData = new byte[actualSize];
                recvData = data.Skip(size).Take(actualSize).ToArray();

                //Handle frame data
                HandleFrameData(datatype, bufferID, packetSeqID, recvData);

                //Skip extracted data to handle remaining packet size
                data = data.Skip((actualSize + size)).ToArray();
            }
        }

        /// <summary>
        /// Handles the dataframe
        /// </summary>
        /// <param name="datatype">Datatype of packet</param>
        /// <param name="bufferID">The bufferID of the packet</param>
        /// <param name="packetSeqID">The sequence ID of the packet</param>
        /// <param name="data">The actual non-header-data of the packet</param>
        private void HandleFrameData(byte datatype, byte bufferID, byte packetSeqID, byte[] data)
        {
            //If the drone is pinging us -> Send back pong
            if (bufferID == CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PING && data.Length > 0)
            {
                SendPong(data);
                _logger.Debug("Pong");
            }

            //Drone is asking for us to acknowledge the receival of the packet
            if (datatype == CommandSet.ARNETWORKAL_FRAME_TYPE_ACK && data.Length > 0)
            {
                int ackSeqNumber = data[0];

                SetCommandReceived("SEND_WITH_ACK", ackSeqNumber, true);

                AckPacket(bufferID, ackSeqNumber);
                _logger.Debug("Send Ack");
            }
            //Drone just sent us sensor data -> No acknowledge required
            else if (datatype == CommandSet.ARNETWORKAL_FRAME_TYPE_DATA)
            {
                if (bufferID == CommandSet.BD_NET_DC_NAVDATA_ID || bufferID == CommandSet.BD_NET_DC_EVENT_ID)
                {
                    UpdateSensorData(data, bufferID, packetSeqID, false);
                    _logger.Debug("Sensor update");
                }
            }
            else if (datatype == CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY)
            {
                _logger.Debug("Handle low latency data?");
            }
            else if (datatype == CommandSet.ARNETWORKAL_FRAME_TYPE_MAX)
            {
                _logger.Debug("Received a maxframe(Technically unknown?)");
            }
            else if (datatype == CommandSet.ARNETWORKAL_FRAME_TYPE_UNINITIALIZED)
            {
                _logger.Debug("Received an uninitialized frame");
            }
            else
            {
                _logger.Fatal("Unknown data type received from drone!");
            }
        }

        private void UpdateSensorData(byte[] rawDataPacket, byte bufferId, int seqNumber, bool ack)
        {
            //todo not doing?
        }

        /// <summary>
        /// Method used to send back an acknowledge packet to the drone when it requests it
        /// </summary>
        /// <param name="bufferID">The bufferID of the packet to acknowledge</param>
        /// <param name="packetID">The packetID of the packet to acknowledge</param>
        private void AckPacket(byte bufferID, int packetID)
        {
            int newbufferId = (bufferID + 128) % 256;

            Tuple<string, int> tupledata = new Tuple<string, int>("ACK", newbufferId);
            Command packet = new Command();
            if (_commandReceived.ContainsKey(tupledata))
            {
                _commandReceived[new Tuple<string, int>("ACK", 0)] = false;
            }
            else
            {
                _commandReceived[new Tuple<string, int>("ACK", (newbufferId + 1) % 256)] = true;
                packet.cmd = new byte[5];
                packet.cmd[0] = CommandSet.ARNETWORKAL_FRAME_TYPE_ACK;
                packet.cmd[1] = (byte) newbufferId;
                packet.cmd[2] = (byte) ((newbufferId + 1) % 256);
                packet.cmd[3] = 8;
                packet.cmd[4] = (byte) packetID;
                packet.size = 5;
            }

            SafeSendDroneCMD(packet);
        }

        /// <summary>
        /// Sends a "Pong" back to the drone
        /// used by the drone as a IsAlive request
        /// </summary>
        /// <param name="data">The pingdata to pong back</param>
        private void SendPong(byte[] data)
        {
            int size = data.Length;

            int seq = _sequenceDictionary["PONG"];

            _sequenceDictionary["PONG"] = seq + 1 % 256;
            Command packet = new Command();

            byte[] cmd = new byte[size + 7];
            //Construct packet
            cmd[0] = CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PONG; //BufferID
            cmd[1] = CommandSet.ARNETWORKAL_FRAME_TYPE_DATA; //DataType
            cmd[2] = (byte) _sequenceDictionary["PONG"]; //Sequence ID
            cmd[3] = (byte) (size + 7); //Total packet size
            data.CopyTo(cmd, 4); //Pong data
            packet.cmd = cmd;
            packet.size = size + 7;
            SafeSendDroneCMD(packet);
        }

        /// <summary>
        /// Sends a DroneCMD to the drone - Retries if the attempt is unsuccessful
        /// </summary>
        /// <param name="droneCmd"The command to send></param>
        private void SafeSendDroneCMD(Command droneCmd)
        {
            bool packetSent = false;
            int attemptNo = 0;

            while (!packetSent && attemptNo < 2)
            {
                try
                {
                    _droneUdpClient.Send(droneCmd.cmd, droneCmd.size);
                    packetSent = true;
                }
                catch (Exception e)
                {
                    packetSent = false;
                    attemptNo += 1;
                }
            }
        }

        /// <summary>
        ///     Sends a command to the drone
        /// </summary>
        /// <param name="cmd">The command to send</param>
        /// <param name="type">The type of command to send, defaults to a fly command</param>
        /// <param name="id">The id of the command, defaults to not receiving an acknowledge</param>
        private bool SendCommand(ref Command cmd, int type = CommandSet.ARNETWORKAL_FRAME_TYPE_DATA, int id = CommandSet.BD_NET_CD_NONACK_ID)
        {
            int bufSize = cmd.size + 7;
            byte[] buf = new byte[bufSize];

            _seq[id] = (_seq[id] + 1) % 256;

            buf[0] = (byte) type;
            buf[1] = (byte) id;
            buf[2] = (byte) _seq[id];
            buf[3] = (byte) (bufSize & 0xff);
            buf[4] = (byte) ((bufSize & 0xff00) >> 8);
            buf[5] = (byte) ((bufSize & 0xff0000) >> 16);
            buf[6] = (byte) ((bufSize & 0xff000000) >> 24);

            cmd.cmd.CopyTo(buf, 7);

            int tryNum = 0;
            SetCommandReceived("SEND_WITH_ACK", _seq[id], false);

            while (tryNum < MaxPacketRetries && !IsCommandReceived("SEND_WITH_ACK", _seq[id]))
            {
                _logger.Debug("Trying to send package to drone.");
                SafeSend(buf);

                tryNum++;

                SmartSleep(500);
            }

            //Reset flyvector
            lock (ThisLock)
            {
                _flyVector.ResetVector();
            }

            return IsCommandReceived("SEND_WITH_ACK", _seq[id]);
        }


        protected void SafeSend(byte[] buffer)
        {
            bool packetSent = false;
            int tryNum = 0;

            while (!packetSent && tryNum < MaxPacketRetries)
            {
                try
                {
                    _droneUdpClient.Send(buffer, buffer.Length);
                    packetSent = true;
                }
                catch (Exception e)
                {
                    // TODO: Reconnect.
                    tryNum++;
                }
            }
        }

        private void AskForStateUpdate()
        {
            _cmd = default(Command);
            _cmd.size = 4;
            _cmd.cmd = new byte[4];

            _cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            _cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMON;
            _cmd.cmd[2] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff;
            _cmd.cmd[3] = CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & (0xff00 >> 8);

            SendCommand(ref _cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        #endregion


        /// <summary>
        /// Drone command thread
        /// Generates and sends movement commands received
        /// </summary>
        private void PcmdThreadActive()
        {
            _logger.Debug("Started command generator thread");
            while (IsRunning)
            {
                GenerateDroneCommand();
                SmartSleep(Updaterate);
            }
        }


        /// <summary>
        /// Threadwatcher to make sure the command generation thread is alive and well
        /// </summary>
        private void ThreadManager()
        {
            _logger.Debug("Started Threadwatcher");
            while (IsRunning)
            {
                if (_commandGeneratorThread.IsAlive)
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

        #region CommandGeneration

        /// <summary>
        ///     Generates the command for the drone
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
        ///     Busy sleeps for the specified amount of time.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to sleep</param>
        public void SmartSleep(int milliseconds)
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

        #endregion

        #region Command Handler

        protected void SetCommandReceived(string channel, int seqId, bool val)
        {
            var key = new Tuple<string, int>(channel, seqId);
            _commandReceived[key] = val;
        }

        protected bool IsCommandReceived(string channel, int seqId)
        {
            var key = new Tuple<string, int>(channel, seqId);
            return _commandReceived[key];
        }

        #endregion
    }
}
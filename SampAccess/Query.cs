using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace SampAccess
{
    /// <summary>
    /// Class intended to taking query to SAMP server
    /// <para>It is supporting only IPv4 addresses.</para>
    /// </summary>
    public class Query : IDisposable
    {
        /// <summary>
        /// Address of server, that gets queries 
        /// </summary>
        public IPEndPoint Address { get; }

        /// <summary>
        /// Represents connection with server
        /// </summary>
        private Socket Connection { get; }

        /// <summary>
        /// Enumerating of query types
        /// </summary>
        public enum QueryType : byte
        {
            /// <summary>
            /// Gather common information about server
            /// </summary>
            CommonInformation = 0,

            /// <summary>
            /// Gather list of server rules
            /// </summary>
            ListOfRules = 1,

            /// <summary>
            /// Gather common information about players
            /// </summary>
            CommonPlayersInformation = 2,

            /// <summary>
            /// Gather detailed information about players
            /// </summary>
            DetailedPlayersInformation = 3
        }

        /// <summary>
        /// Query (packet) type
        /// </summary>
        private readonly char[] PacketType = 
        {
            'i', 'r', 'c', 'd'
        };

        /// <summary>
        /// Struct, representing information about server
        /// </summary>
        public readonly struct ServerInformation
        {
            /// <summary>
            /// Number of online players
            /// </summary>
            public ushort PlayersOnline { get; init; }

            /// <summary>
            /// Maximum number of online players
            /// </summary>
            public ushort LimitOfPlayers { get; init; }

            /// <summary>
            /// Server has password (true) or not (false)
            /// </summary>
            public bool HasPassword { get; init; }

            /// <summary>
            /// Name of server (host)
            /// </summary>
            public string HostName { get; init; }

            /// <summary>
            /// GameMode of server (host)
            /// </summary>
            public string GameMode { get; init; }

            /// <summary>
            /// Language of server (host)
            /// </summary>
            public string Language { get; init; }
        }

        /// <summary>
        /// Structure representing a player on server
        /// </summary>
        public readonly struct Player
        {
            public uint? Ping { get; init; }
            public string Nick { get; init; }
            public uint Score { get; init; }
            public byte? PlayerID { get; init; }
        }

        /// <summary>
        /// Private field, that is indicating, that <see cref="Query"/> is disposed
        /// </summary>
        private bool _isdisposed = false;

        /// <summary>
        /// Common information about server
        /// </summary>
        public ServerInformation CommonInformation { get; private set; }

        /// <summary>
        /// Represents ping in milliseconds
        /// </summary>
        public double? PingInMilliseconds { get; private set; }

        /// <summary>
        /// Represents list of server rules
        /// </summary>
        public Dictionary<string, string> ServerRules { get; private set; }

        /// <summary>
        /// Maximum number of players that can play on server
        /// </summary>
        protected const ushort SampMaxPlayers = 1000;

        /// <summary>
        /// Length of message, sending to server
        /// </summary>
        protected const byte SampMessageLength = 11;

        /// <summary>
        /// List, representing information about players
        /// </summary>
        public List<Player> PlayersInformation { get; } = new(byte.MaxValue+1);

        /// <summary>
        /// Length of SAMP server answer
        /// </summary>
        protected const ushort SampAnswerLength = 2048; // 2 KB

        /// <summary>
        /// What encoding the text is encoded in 
        /// </summary>
        public ushort TextCodePage { get; init; } = 1251;

        /// <summary>
        /// Creates new instance of <see cref="Query"/>
        /// <para>If you set send (or receive) time-out to 0, it will indicate infinite time-out period</para>
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <param name="SendTimeout"></param>
        /// <param name="ReceiveTimeout"></param>
        public Query(string IP, ushort Port, ushort SendTimeout = 5000, ushort ReceiveTimeout = 5000)
        {
            Address = new(IPAddress.Parse(IP), Port);

            Connection = new(Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                SendTimeout = SendTimeout,
                ReceiveTimeout = ReceiveTimeout
            };

            Connection.Connect(Address);
        }

        /// <summary>
        /// Static constructor of class <see cref="Query"/>
        /// </summary>
        static Query()
        {
            CodePages.AddCodePages();
            //We're registring more encodings from System.Text.Encoding.CodePages package
        }

        /// <summary>
        /// Send query to the server and gather all information about it
        /// </summary>
        /// <param name="Type"></param>
        /// <exception cref="SocketException"/>
        public void Initialize()
        {
            Initialize(QueryType.CommonInformation);
            Initialize(QueryType.ListOfRules);
            Initialize((CommonInformation.PlayersOnline < byte.MaxValue) ? QueryType.DetailedPlayersInformation : QueryType.CommonPlayersInformation);
            // If online players greater than 255 (because PlayerID is 8-bit encoded), then server doesn't respond :/
        }

        /// <summary>
        /// Send query to the server and gather some information about it
        /// </summary>
        /// <param name="Type"></param>
        /// <exception cref="SocketException"></exception>
        public void Initialize(QueryType Type)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Sending a query
            {
                using MemoryStream stream = new(SampMessageLength);
                using BinaryWriter writer = new(stream);

                writer.Write("SAMP".ToCharArray());
                writer.Write(Address.Address.GetAddressBytes());
                writer.Write((ushort)Address.Port);
                writer.Write(PacketType[(byte)Type]);

                Connection.Send(stream.ToArray());
            }

            // Receiving server answer
            {
                byte[] receive = new byte[SampAnswerLength];

                Connection.Receive(receive);

                stopwatch.Stop();
                PingInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                using MemoryStream stream = new(receive);
                using BinaryReader read = new(stream);

                read.ReadBytes(SampMessageLength); // When you recieve a packet, there are 11 bytes (this is what we sent) of a packet you can remove right away.

                switch (Type)
                {
                    case QueryType.CommonInformation:
                        {
                            CommonInformation = new()
                            {
                                HasPassword = read.ReadBoolean(),
                                PlayersOnline = read.ReadUInt16(),
                                LimitOfPlayers = read.ReadUInt16(),
                                HostName = Encoding.GetEncoding(TextCodePage).GetString(read.ReadBytes(read.ReadInt32())),
                                GameMode = Encoding.GetEncoding(TextCodePage).GetString(read.ReadBytes(read.ReadInt32())),
                                Language = Encoding.GetEncoding(TextCodePage).GetString(read.ReadBytes(read.ReadInt32()))
                            };
                            break;
                        }
                    case QueryType.ListOfRules:
                        {
                            ServerRules = new();

                            ushort rules = read.ReadUInt16();
                            for (int i = 0; i < rules; i++)
                            {
                                ServerRules.Add(new(read.ReadChars(read.ReadByte())), new(read.ReadChars(read.ReadByte())));
                            }
                            break;
                        }
                    case QueryType.CommonPlayersInformation:
                        {
                            PlayersInformation.Clear();

                            ushort players = read.ReadUInt16();
                            for(int i = 0; i < players; i++)
                            {
                                PlayersInformation.Add(new()
                                {
                                    Nick = new(read.ReadChars(read.ReadByte())),
                                    Score = read.ReadUInt32()
                                });
                            }
                            break;
                        }
                    case QueryType.DetailedPlayersInformation:
                        {
                            PlayersInformation.Clear();

                            ushort players = read.ReadUInt16();
                            for (int i = 0; i < players; i++)
                            {
                                PlayersInformation.Add(new()
                                {
                                    PlayerID = read.ReadByte(),
                                    Nick = new string(read.ReadChars(read.ReadByte())),
                                    Score = read.ReadUInt32(),
                                    Ping = read.ReadUInt32()
                                });
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Close connection with the server
        /// </summary>
        public void Dispose()
        {
            if (_isdisposed) return;
            _isdisposed = true;
            Connection.Close();
            GC.SuppressFinalize(this);
        }

        ~Query() => Connection.Close();
    }
}

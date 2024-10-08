﻿using System;
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
    /// This class can get and handle information from given SAMP server.
    /// <para>It can work with IPv4 addresses only.</para>
    /// </summary>
    public class Query : IDisposable
    {
        /// <summary>
        /// Address of server that gets our queries.
        /// </summary>
        public IPEndPoint Address { get; }

        /// <summary>
        /// This is the connection with server
        /// </summary>
        private Socket Connection { get; }

        /// <summary>
        /// Enumerating of query types
        /// </summary>
        public enum QueryType : byte
        {
            /// <summary>
            /// Get common information about server
            /// </summary>
            CommonInformation = 0,

            /// <summary>
            /// Get list of server rules
            /// </summary>
            ListOfRules = 1,

            /// <summary>
            /// Get common information about players
            /// </summary>
            CommonPlayersInformation = 2,

            /// <summary>
            /// Get detailed information about players
            /// </summary>
            DetailedPlayersInformation = 3
        }

        /// <summary>
        /// Query type characters.
        /// Server gets query type as a character. 
        /// </summary>
        private readonly char[] PacketType = 
        {
            'i', 'r', 'c', 'd'
        };

        /// <summary>
        /// This structure contains information about a server
        /// </summary>
        public readonly struct ServerInformation
        {
            /// <summary>
            /// Number of online players.
            /// </summary>
            public ushort PlayersOnline { get; init; }

            /// <summary>
            /// Maximum number of online players.
            /// </summary>
            public ushort LimitOfPlayers { get; init; }

            /// <summary>
            /// Indicates whether a server has password (true) or not (false).
            /// </summary>
            public bool HasPassword { get; init; }

            /// <summary>
            /// Server host name.
            /// </summary>
            public string HostName { get; init; }

            /// <summary>
            /// Server game mode.
            /// </summary>
            public string GameMode { get; init; }

            /// <summary>
            /// Language of a server.
            /// </summary>
            public string Language { get; init; }
        }

        /// <summary>
        /// This structure contains information about a player on a server
        /// </summary>
        public readonly struct Player
        {
            public uint? Ping { get; init; }
            public string Nick { get; init; }
            public uint Score { get; init; }
            public byte? PlayerID { get; init; }
        }

        /// <summary>
        /// Private field, that indicates that <see cref="Query"/> is disposed
        /// </summary>
        private bool _isdisposed = false;

        /// <summary>
        /// Common information about server.
        /// </summary>
        public ServerInformation CommonInformation { get; private set; }

        /// <summary>
        /// Ping (measured in milliseconds).
        /// </summary>
        public double? PingInMilliseconds { get; private set; }

        /// <summary>
        /// This is the list of server rules.
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
        /// List that has information about players.
        /// </summary>
        public List<Player> PlayersInformation { get; } = new(byte.MaxValue+1);

        /// <summary>
        /// Length of SAMP server answer.
        /// </summary>
        protected const ushort SampAnswerLength = 2048; // 2 KB

        /// <summary>
        /// This code page will be used while interpreting data from a server.
        /// </summary>
        public ushort TextCodePage { get; init; } = 1251;

        /// <summary>
        /// Creates new instance of <see cref="Query"/>
        /// <para>If you set send (or receive) time-out to 0, it will indicate infinite time-out period</para>
        /// </summary>
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
        /// Send query to the server and get all information about it
        /// </summary>
        /// <exception cref="SocketException"/>
        public void Initialize()
        {
            Initialize(QueryType.CommonInformation);
            Initialize(QueryType.ListOfRules);
            Initialize((CommonInformation.PlayersOnline < byte.MaxValue) ? QueryType.DetailedPlayersInformation : QueryType.CommonPlayersInformation);
            // If online players greater than 255 (because PlayerID is 8-bit encoded), then server doesn't respond :/
        }

        /// <summary>
        /// Send query to the server and get some information about it.
        /// </summary>
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

                read.ReadBytes(SampMessageLength); // When you recieve a packet, there are 11 bytes (this is what we sent) that you can remove right away.

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
        /// Close server connection.
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

using CF_Server.Network.Sockets;
using System;
using System.Linq;
using System.Net;
using MYSQLCONNECTION = MySql.Data.MySqlClient.MySqlConnection;

namespace CF_Server
{
    public static class Program
    {
        public static GameServer[] Servers;
        public const string
            Database = "cf",
            DBPassword = "424570";
        public const int AuthPort = 13008;
        public const string AuthIP = "192.168.1.2";
        public const string GameIP = "192.168.1.2";
        private static Network.Sockets.ServerSocket AuthServer;
        public static Thread Thread;
        static int MaxOnline = 0;
        public static bool CapturePackets = false, CaptureUnknownPackets = false;
        private static void Main(string[] args)
        {
            #region Initiate Servers
            Program.Servers = new GameServer[3];
            Program.Servers[0] = new GameServer
            {
                IPBytes = IPAddress.Parse(GameIP).GetAddressBytes(),
                IP = "192.168.1.2",
                serverType = Enums.ServerType.Normal,
                Port = 13009,
                MaxPlayers = 800u,
                NoLimit = 1,
                Name = "Alchemist",
                MinRank = 0,
                MaxRank = 100
            };
            Program.Servers[1] = new GameServer
            {
                IPBytes = IPAddress.Parse(GameIP).GetAddressBytes(),
                IP = GameIP,
                serverType = Enums.ServerType.Normal,
                Port = 13010,
                MaxPlayers = 800u,
                NoLimit = 1,
                Name = "LordsRoad",
                MinRank = 0,
                MaxRank = 100
            };
            Program.Servers[2] = new GameServer
            {
                IPBytes = IPAddress.Parse(GameIP).GetAddressBytes(),
                IP = GameIP,
                serverType = Enums.ServerType.Normal,
                Port = 13011,
                MaxPlayers = 800u,
                NoLimit = 1,
                Name = "Satanic",
                MinRank = 0,
                MaxRank = 100
            };
            #endregion
            Thread = new Thread();
            Thread.Init();
            AuthServer = new Network.Sockets.ServerSocket();
            AuthServer.OnClientConnect += AuthServer_OnClientConnect;
            AuthServer.OnClientReceive += AuthServer_OnClientReceive;
            AuthServer.OnClientDisconnect += AuthServer_OnClientDisconnect;
            AuthServer.Enable(AuthPort, AuthIP);
            Console.Title = "[CF_Server] - Online Players: 0 - MaxOnline: 0";
            Console.WriteLine(string.Concat(new object[]
				{
					"[",
					DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss"),
					"] Auth Server started on "+AuthIP+":",
					AuthPort
				}));
            for (int i = 0; i < Servers.Length; i++)
            {
                Program.Servers[i].Open();
                Console.WriteLine(string.Concat(new object[]
				{
					"[",
					DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss"),
					"] "+Program.Servers[i].Name+" Server started on "+Program.Servers[i].IP+":",
					Program.Servers[i].Port
				}));
            }
            Kernel.Rooms.Add(new Room()
            {
                Channel = 1,
                Server = Program.Servers[0],
                VIP = true,
                Status = Enums.RoomStatus.inGame,
                gameMode = Enums.GameMode.Team_Death_Match,
                MapID = 20,//Egypt
                maxNumberOfPlayers = 10,
                objectiveType = Enums.RoomObjType.Kills,
                NoFlash_Smoke = true,
                Weapons = Enums.RoomWeapons.Sniper,
                Name = "my Testing Room1",
                Password = "",
                Number = 1,
                maxObjectiveCount = 100,
                Players = new System.Collections.Generic.Dictionary<uint, Client.GameClient>() { { 0, new Client.GameClient(null) { Entity = new Entity() { Name = "2ndTest", Clan = "Yuzumaki" } } } },
                objectiveCount = 70,
                Host = new Client.GameClient(null) { Entity = new Entity() { Name = "2ndTest", Clan = "Yuzumaki" } }
            });
            #region ReadCommands
            while (true)
            {
                string Command = Console.ReadLine();
                HandleCommand(Command);
            }
            #endregion
        }
        public static void UpdateConsoleTitle()
        {
            if (Kernel.GamePool.Count > MaxOnline)
                MaxOnline = Kernel.GamePool.Count;
            Console.Title = "[CF_Server] - Online Players: " + Kernel.GamePool.Count + " - MaxOnline: " + MaxOnline;
        }
        static void AuthServer_OnClientConnect(ClientWrapper wrapper)
        {
            wrapper.Connector = new Client.AuthClient(wrapper);
        }
        static void AuthServer_OnClientDisconnect(ClientWrapper obj)
        {
            obj.Disconnect();
        }
        static void AuthServer_OnClientReceive(byte[] buffer2, int length, ClientWrapper wrapper)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(buffer2, 0, buffer, 0, length);
            Client.AuthClient authClient = wrapper.Connector as Client.AuthClient;
            var packet = new AuthPackets.Packet(buffer);
            packet.Deserialize(buffer);
            if (CapturePackets)
                Console.WriteLine("Captured a packet from Client (AUTH Server) Length: " + buffer.Length + " Type: (" + buffer[3] + "-" + buffer[4] + "-" + buffer[5] + ")");
            Auth.PacketHandler.Process(authClient, packet);
        }
        public static void GameServer_OnClientConnect(ClientWrapper obj)
        {
            Client.GameClient client = new Client.GameClient(obj);
            obj.Connector = client;
        }
        public static void GameServer_OnClientDisconnect(ClientWrapper obj)
        {
            if (obj.Connector != null)
                (obj.Connector as Client.GameClient).Disconnect();
            else
                obj.Disconnect();
        }
        public static void GameServer_OnClientReceive(byte[] buffer2, int length, ClientWrapper wrapper)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(buffer2, 0, buffer, 0, length);
            if (wrapper.Connector == null)
            {
                wrapper.Disconnect();
                return;
            }
            Client.GameClient gameClient = wrapper.Connector as Client.GameClient;
            #region Split
            {
                var str = Game.PacketHandler.BytesToString(buffer);
                if (str.Contains("F2 F1"))
                {
                    var split = str.Split(new string[] { " F2 F1 " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < split.Length; i++)
                    {
                        byte[] buf = new byte[split[i].Length + 1];

                        buf[buf.Length - 1] = 0xF2;
                        if (i == 0)
                        {
                            Buffer.BlockCopy(Game.PacketHandler.StringToBytes(split[i]), 0, buf, 0, Game.PacketHandler.StringToBytes(split[i]).Length);

                        }
                        else
                        {
                            Buffer.BlockCopy(Game.PacketHandler.StringToBytes(split[i]), 0, buf, 1, Game.PacketHandler.StringToBytes(split[i]).Length);
                            byte[] buf2 = new byte[buf.Length + 1];
                            Buffer.BlockCopy(buf, 0, buf2, 1, buf.Length);
                            buf[0] = 0xF1;
                        }
                        var packet2 = new GamePackets.Packet(buffer);
                        packet2.Deserialize(buffer);
                        if (CapturePackets)
                            Console.WriteLine("Captured a packet from Client (GAME Server) Length: " + buffer.Length + " Type: (" + buffer[3] + "-" + buffer[4] + "-" + buffer[5] + ")");
                        Game.PacketHandler.Process(gameClient, packet2);
                    }
                    return;
                }
            }
            #endregion
            var packet = new GamePackets.Packet(buffer);
            packet.Deserialize(buffer);
            if (CapturePackets)
                Console.WriteLine("Captured a packet from Client (GAME Server) Length: " + buffer.Length + " Type: (" + buffer[3] + "-" + buffer[4] + "-" + buffer[5] + ")");
            Game.PacketHandler.Process(gameClient, packet);
        }
        private static void HandleCommand(string Command)
        {
            switch (Command)
            {
                case "@exit":
                    {
                        foreach (var player in Kernel.GamePool.Values) player.Disconnect();
                        Environment.Exit(0);
                        break;
                    }
                case "@clear":
                    {
                        Console.Clear();
                        Console.WriteLine(string.Concat(new object[]
			         	{
			         		"[",
			         		DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss"),
			         		"] Auth Server started on "+AuthIP+":",
			         		AuthPort
			         	}));
                        break;
                    }
                case "@capturepackets":
                    {
                        CapturePackets = CapturePackets ? false : true;
                        if (CapturePackets) Console.WriteLine("Now the console will capture incoming-outgoing packets");
                        else Console.WriteLine("Now the console won't capture incoming-outgoing packets");
                        break;
                    }
                case "@captureunknownpackets":
                    {
                        CaptureUnknownPackets = CaptureUnknownPackets ? false : true;
                        if (CaptureUnknownPackets) Console.WriteLine("Now the console will capture incoming unknown packets");
                        else Console.WriteLine("Now the console won't capture incoming unknown packets");
                        break;
                    }
                case "test":
                    {
                        break;
                    }
            }
        }
        public static MYSQLCONNECTION MySqlConnection
        {
            get
            {
                MYSQLCONNECTION conn = new MYSQLCONNECTION();
                conn.ConnectionString = "Server=localhost;Port=3306;Database=" + Database + ";Uid=root;Password=" + DBPassword + ";Persist Security Info=True;Pooling=true; Min Pool Size = 32;  Max Pool Size = 300;";
                return conn;
            }
        }
    }
}

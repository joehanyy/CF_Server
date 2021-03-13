using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace CF_Server.Game
{
    public class PacketHandler
    {
        public static void Process(Client.GameClient client, GamePackets.Packet packet)
        {
            if (packet.CorrectPacket)
            {
                switch (packet.packetType)
                {
                    case GamePackets.Packet.PacketType.C2S_AuthToChannelServer:
                         Database.AccountTable Account = null;
                        string Identifer = Encoding.ASCII.GetString(packet.buffer, 54, 31).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        if (Kernel.AwaitingPool.TryGetValue(Identifer, out Account))
                        {
                            client.Account = Account;
                            Kernel.AwaitingPool.Remove(Identifer);
                            Client.GameClient aClient = null;
                            if (Kernel.GamePool.TryGetValue(Account.EntityID, out aClient))
                                aClient.Disconnect();
                            Kernel.GamePool.Remove(Account.EntityID);
                            LoadEntity(client);
                            Kernel.GamePool.Add(Account.EntityID, client);
                            Console.WriteLine(client.Entity.Name + " has been logged on! (" + client.Account.Server.Name + ")");
                            Program.UpdateConsoleTitle();
                            SendCompleteLogin(client);
                            SendMyPlayerData(client);
                        }
                        break;
                    case GamePackets.Packet.PacketType.C2S_GetChannels:
                        SendChannels(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_ExitGameInfoInto:
                        SendExitInfoInto(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_ConfirmBack:
                        var back = new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ConfirmBack, new byte[4] { 1, 0, 0, 0 });
                        client.Send(back);
                        break;

                    case GamePackets.Packet.PacketType.C2S_ChannelJoin:
                        SendChannelJoin(client, packet.buffer[6]);
                        break;

                    case GamePackets.Packet.PacketType.C2S_HeartBeat:
                        SendHeartBeat(client, BitConverter.ToUInt32(packet.buffer, 6));
                        break;

                    case GamePackets.Packet.PacketType.C2S_ServerTime:
                        SendNewGUID(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_GetPlayerStats:
                        SendPlayerStats(client, client);
                        break;

                    case GamePackets.Packet.PacketType.S2C_GetAnotherPlayerStats:
                        SendPlayerStats(Kernel.GamePool.Values.Where(i => i.Entity != null && i.Entity.Name == Encoding.ASCII.GetString(packet.buffer, 6, 12)).FirstOrDefault(), client);
                        break;
                    case GamePackets.Packet.PacketType.C2S_ChannelData:
                        client.Entity.Lobby = true;
                        SendChannelRooms(client);
                        SendPlayersOnChannel(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_GetPlayersOnChannel:
                        SendPlayersOnChannel(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_JoinToRoom:
                        client.Entity.Lobby = false;
                        JoinToRoom(client, packet);
                        break;

                    case GamePackets.Packet.PacketType.C2S_CreateRoom:
                        CreateRoom(client, packet);
                        break;

                    case GamePackets.Packet.PacketType.C2S_BackFromRoom:
                        BackFromRoom(client);
                        break;


                    case GamePackets.Packet.PacketType.C2S_RequestExit:
                        SendExitInfo(client);
                        break;

                    case GamePackets.Packet.PacketType.С2S_EnterToShootingRoom:
                        break;

                    case GamePackets.Packet.PacketType.C2S_ChannelsUpdate:
                        SendChannels(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_ExitFromChannelToChannelsList:
                        ExitFromChannel(client);
                        break;
                        
                    case GamePackets.Packet.PacketType.C2S_AutoJoin:
                        AutoJoinRoom(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_AutoJoinOptions:
                        AutoJoinOptions(client, packet);
                        break;

                    case GamePackets.Packet.PacketType.C2S_GetZP:
                        SendZP(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_Mileage:
                        SendMileage(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_StorageItems:
                        SendStorageItems(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_FeverUpdate:
                        SendFeverStatus(client);
                        break;

                    case GamePackets.Packet.PacketType.C2S_ChangeSettings:
                        ChangeSettings(client, packet);
                        break;

                    case GamePackets.Packet.PacketType.C2S_FeverInfoUpdate:
                        SendFeverInfo(client);
                        break;
                    case GamePackets.Packet.PacketType.Unknown:
                        if (Program.CaptureUnknownPackets)
                            Console.WriteLine("Unknown Packet! =>  (" + packet.buffer[3] + "-" + packet.buffer[4] + "-" + packet.buffer[5] + ")");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Incorrent Packet Structure! =>  (" + packet.buffer[3] + "-" + packet.buffer[4] + "-" + packet.buffer[5] + ")");
            }
        }
        private static void LoadEntity(Client.GameClient client)
        {
            Database.EntityTable.LoadEntity(client);
            Program.Thread.Register(client);
        }
        private static void SendExitInfoInto(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(1000);//XPGained
            writer.Write(1000);//GPGained
            writer.Write(client.Entity.TotalKills);
            writer.Write(client.Entity.TotalDeaths);
            writer.Write(client.Entity.HeadshotKills);
            writer.Write(client.Entity.Wins);
            writer.Write(client.Entity.Loses);
            writer.Write(client.Entity.Experience);
            writer.Write(client.Entity.GP);
            writer.Write(0);
            writer.Write(client.Entity.Rank + 1);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ExitGameInfoInto, answer));
        }
        private static void SendCompleteLogin(Client.GameClient client)
        {
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GameServer, new byte[4] { 1, 0, 0, 0 }));
            client.Entity.FullyLoaded = true;
        }
        private static void SendExitInfo(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(1000);//XPGained
            writer.Write(1000);//GPGained
            writer.Write(client.Entity.TotalKills);
            writer.Write(client.Entity.TotalDeaths);
            writer.Write(client.Entity.TotalBattles);
            writer.Write(client.Entity.Wins);
            writer.Write(client.Entity.Loses);
            writer.Write(client.Entity.Experience);
            writer.Write(client.Entity.GP);
            writer.Write(1);//RoundesNeedToPlayToPromote
            writer.Write(client.Entity.Rank + 1);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ExitGameInfo, answer));
        }
        private static void SendChannels(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);

            for (ushort i = 1; i <= Constants.ChannelsCount; i++)
            {
                writer.Write((ushort)(i - 1));
                writer.Write(Constants.ChannelsCapacity);
                writer.Write((ushort)Kernel.GamePool.Values.Where(e => e.Entity.Channel == i).Count());//Current Number of Players on the Channel
                memory.Position += 14;
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
           client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GetChannels, answer));
        }
        private static void ExitFromChannel(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(0u);
            for (ushort i = 1; i <= Constants.ChannelsCount; i++)
            {
                writer.Write((ushort)(i - 1));
                writer.Write(Constants.ChannelsCapacity);
                writer.Write((ushort)Kernel.GamePool.Values.Where(e => e.Entity.Channel == i).Count());//Current Number of Players on the Channel
                memory.Position += 14;
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Entity.Channel = 0;
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ExitFromChannel, answer));
        }
        private static void SendMyPlayerData(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write((byte)0);
            writer.Write((uint)0);
            writer.Write(Encoding.Default.GetBytes(client.Entity.UID.ToString()));
            writer.Write(new byte[21 - Encoding.Default.GetBytes(client.Entity.UID.ToString()).Length]);
            writer.Write((ushort)1);
            writer.Write((uint)0);
            writer.Write((uint)client.Entity.Honor);
            writer.Write(client.Entity.GP);
            writer.Write(uint.MaxValue);
            writer.Write(client.Entity.Experience);
            writer.Write((ulong)client.Entity.Rank);
            writer.Write((uint)client.Entity.EXPPercent);
            writer.Write(client.Entity.NeedEXP);
            writer.Write(client.Entity.Wins);
            writer.Write(client.Entity.Loses);
            writer.Write(client.Entity.TotalKills);
            writer.Write(client.Entity.TotalDeaths);
            writer.Write(client.Entity.HeadshotKills);
            writer.Write(client.Entity.TeamKills);
            writer.Write(client.Entity.Deserion);
            writer.Write(0u);
            writer.Write((uint)15);
            writer.Write((uint)15);
            memory.Position += 28;
            writer.Write(client.Entity.UID);
            writer.Write(Encoding.Default.GetBytes(client.Entity.Name));
            writer.Write(new byte[14 - Encoding.Default.GetBytes(client.Entity.Name).Length]);
            writer.Write((ushort)2);
            writer.Write(Encoding.Default.GetBytes(client.Account.Identifer));
            memory.Position += 17;
            writer.Write((ushort)16);
            writer.Write((ushort)6);
            writer.Write(0u);
            writer.Write(Encoding.Default.GetBytes(client.Entity.Name));
            writer.Write(new byte[20 - Encoding.Default.GetBytes(client.Entity.Name).Length]);
            writer.Write(client.Entity.Settings);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_PlayerData, answer));
        }
        private static void SendHeartBeat(Client.GameClient client, UInt32 id)
        {
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_HeartBeat, BitConverter.GetBytes(id)));
        }
        private static void SendPlayerStats(Client.GameClient Observed, Client.GameClient Observer)
        {
            byte[] buffer = new byte[1048];
            Writer.Write(Observed.Entity.Name, 6 - 6, buffer);
            Writer.Write(Observed.Entity.Clan, 19 - 6, buffer);
            Writer.Write(Observed.Entity.Rank, 54 - 6, buffer);
            Writer.Write(Observed.Entity.Wins, 70 - 6, buffer);
            Writer.Write(Observed.Entity.Loses, 74 - 6, buffer);
            Writer.Write(Observed.Entity.TotalKills, 78 - 6, buffer);
            Writer.Write(Observed.Entity.TotalDeaths, 82 - 6, buffer);
            Writer.Write(Observed.Entity.HeadshotKills, 86 - 6, buffer);
            Writer.Write(Observed.Entity.GeneradeKills, 90 - 6, buffer);
            Writer.Write(Observed.Entity.KnifeKills, 94 - 6, buffer);
            Writer.Write(Observed.Entity.Honor, 102 - 6, buffer);
            Writer.Write(Observed.Entity.Deserion, 106 - 6, buffer);
            Writer.Write(Observed.Entity.TeamKills, 110 - 6, buffer);
            int Offset = 114 - 6;
            byte[] GameModes = new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            for (byte i = 0; i < GameModes.Length; i++)
            {
                var battles = Observed.Entity.Battles.Where(x => x.GameMode == (Enums.GameMode)i);
                if (battles.Count() < 1) { Offset += 16; continue; }
                Writer.Write(battles.Where(q => q.Won).Count(), Offset, buffer); Offset += 4;
                Writer.Write(battles.Where(q => !q.Won).Count(), Offset, buffer); Offset += 4;
                Writer.Write(battles.Sum(w => w.Kills), Offset, buffer); Offset += 4;
                Writer.Write(battles.Sum(w => w.Deaths), Offset, buffer); Offset += 4;
            }
            Observer.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GetPlayerStat, buffer));
        }
        private static void SendChannelJoin(Client.GameClient client, byte channelnum)
        {
            bool Full = Kernel.GamePool.Values.Where(i => i.Entity != null && i.Entity.Channel == (ushort)(channelnum + 1)).Count() >= 300;
            byte[] buffer = new byte[8];
            buffer[0] = (byte)(Full ? 2 : 0);
            Writer.Write(channelnum, 4, buffer);
            if (!Full) client.Entity.Channel = (ushort)(channelnum + 1);
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ChannelJoin, buffer));
        }
        private static void UpdateWaitingList(Client.GameClient client, bool Remove = false)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(Remove ? 1 : 0);
            writer.Write(client.Entity.Rank);
            memory.Position += 12;
            writer.Write(Encoding.ASCII.GetBytes(client.Entity.Clan));
            writer.Write(new byte[34 - client.Entity.Clan.Length]);
            writer.Write(Encoding.ASCII.GetBytes(client.Entity.Name));
            writer.Write(new byte[13 - client.Entity.Name.Length]);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            foreach (var player in Kernel.GamePool.Values.Where(i => i.Entity != null && i.Account.Server == client.Account.Server && i.Entity.Channel == client.Entity.Channel && i.Entity.Lobby))
                client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_UpdateWaitingList, answer, 0x74));
        }
        private static void SendPlayersOnChannel(Client.GameClient client)
        {
            client.Send(StringToBytes("F1 00 00 01 22 00 F2"));
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            var players = Kernel.GamePool.Values.Where(i => i.Entity != null && i.Entity.Channel == client.Entity.Channel).ToArray();
            writer.Write((ushort)players.Length);
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i].Entity;
                writer.Write(player.Rank);
                memory.Position += 4;
                writer.Write(2835);
                writer.Write(player.UID);
                writer.Write(Encoding.ASCII.GetBytes(client.Entity.Clan));
                writer.Write(new byte[34 - client.Entity.Clan.Length]);
                writer.Write(Encoding.ASCII.GetBytes(client.Entity.Name));
                writer.Write(new byte[13 - client.Entity.Name.Length]);
                writer.Write(ulong.MaxValue);
                memory.Position += 40;
                writer.Write((byte)Constants.packetEndsWith);
                writer.Write((byte)Constants.packetStartsWith);
                memory.Position += 2;
                writer.Write((byte)1);
                writer.Write((ushort)39);
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GetPlayersOnChannel, answer, 0x6F));
        }
        private static void SendChannelRooms(Client.GameClient client)
        {
            var Rooms = Kernel.Rooms.Where(i => i.Channel == client.Entity.Channel && i.Server == client.Account.Server).ToArray();
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            memory.Position += 4;
            writer.Write((ushort)(Constants.RoomsPerChannel - Rooms.Length));
            writer.Write((ushort)Rooms.Length);
            writer.Write((byte)0);
            for (int i = 0; i < Rooms.Length; i++)
            {
                var Room = Rooms[i];
                writer.Write((ushort)(Room.Number - 1));
                writer.Write((byte)(Room.Password == "" ? 1 : 0));
                writer.Write((byte)(1));
                writer.Write((ushort)Room.gameMode);
                writer.Write((byte)0);
                writer.Write((ushort)0);
                writer.Write(Room.NoFlash_Smoke);
                writer.Write((byte)1);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((uint)Room.objectiveType);
                writer.Write((uint)Room.maxObjectiveCount);
                writer.Write((ushort)0);//May be Elite Mode
                writer.Write((ushort)Room.MapID);
                writer.Write((ushort)Room.numberOfPlayers);
                writer.Write((ushort)Room.maxNumberOfPlayers);
                writer.Write((ushort)Room.observersCount);
                writer.Write((byte)0);
                writer.Write((uint)Room.Status);
                writer.Write(Encoding.ASCII.GetBytes(Room.Host.Entity.Clan));
                writer.Write(new byte[34 - Room.Host.Entity.Clan.Length]);
                writer.Write(Encoding.ASCII.GetBytes(Room.Host.Entity.Name));
                writer.Write(new byte[13 - Room.Host.Entity.Name.Length]);
                writer.Write(3u);
                writer.Write((ushort)0x1E);
                memory.Position += 4;
                writer.Write(Encoding.ASCII.GetBytes(Room.Name));
                writer.Write(new byte[80 - Room.Name.Length]);
                writer.Write((byte)1);//if you put 0 will show X icon instead of lock with password icon and if put 1 the icon will hide
                writer.Write((ushort)(Room.botCount));
                writer.Write((byte)(0));
                memory.Position += 4;
                writer.Write((byte)0);
                writer.Write((ushort)(0));
                writer.Write((ushort)(Room.VIP ? 1 : 0));
                memory.Position += 6;
                writer.Write(byte.MaxValue);
                writer.Write((ushort)1);
                writer.Write((uint)ushort.MaxValue);
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GetRooms, answer));
        }
        private static void CreateRoom(Client.GameClient client, GamePackets.Packet packet)
        {
            if (client.Account.Server == null || client.Entity.Channel == 0) return;
            Room room = new Room()
            {
                Host = client,
                Players = new Dictionary<uint, Client.GameClient>() { { client.Entity.UID, client} },
                Channel = client.Entity.Channel,
                Status = Enums.RoomStatus.Waiting,
                maxNumberOfPlayers = packet.buffer[28],
                observersCount = packet.buffer[30],
                Server = client.Account.Server,
                VIP = client.Entity.VIP,
                objectiveType = (Enums.RoomObjType)packet.buffer[42],
                maxObjectiveCount = packet.buffer[46],
                NoFlash_Smoke = packet.buffer[39] == 0 ? false : true,
                Name = Encoding.ASCII.GetString(packet.buffer, 53, 80).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), ""),
                Password = Encoding.ASCII.GetString(packet.buffer, 6, 10).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), ""),
                MapID = BitConverter.ToUInt16(packet.buffer, 32),
                gameMode = (Enums.GameMode)packet.buffer[34]
            };
            room.Number = GetRoomNumber(room);
            Kernel.Rooms.Add(room);
            byte[] buffer = new byte[268];
            buffer[0] = 0x9C;
            buffer[1] = 0xFF;
            buffer[2] = 0xFF;
            buffer[3] = 0xFF;
            buffer[266] = 0x90;
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_CreateRoom, buffer));
            AddRoomInLobby(room);
        }
        private static void AddRoomInLobby(Room Room, bool Update = false)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(!Update ? 0u : 2u);//Type in offset 6 (0 = New, 1 = Delete, 2 = Update)
            writer.Write((ushort)(Room.Number - 1));
            writer.Write((byte)(Room.Password == "" ? 1 : 0));
            writer.Write((byte)(1));
            writer.Write((ushort)Room.gameMode);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(Room.NoFlash_Smoke);
            writer.Write((byte)1);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((uint)Room.objectiveType);
            writer.Write((uint)Room.maxObjectiveCount);
            writer.Write((ushort)0);//May be Elite Mode
            writer.Write((ushort)Room.MapID);
            writer.Write((ushort)Room.numberOfPlayers);
            writer.Write((ushort)Room.maxNumberOfPlayers);
            writer.Write((ushort)Room.observersCount);
            writer.Write((byte)0);
            writer.Write((uint)Room.Status);
            writer.Write(Encoding.ASCII.GetBytes(Room.Host.Entity.Clan));
            writer.Write(new byte[34 - Room.Host.Entity.Clan.Length]);
            writer.Write(Encoding.ASCII.GetBytes(Room.Host.Entity.Name));
            writer.Write(new byte[13 - Room.Host.Entity.Name.Length]);
            writer.Write(3u);
            writer.Write((ushort)0x1E);
            memory.Position += 4;
            writer.Write(Encoding.ASCII.GetBytes(Room.Name));
            writer.Write(new byte[80 - Room.Name.Length]);
            writer.Write((byte)1);//if you put 0 will show X icon instead of lock with password icon and if put 1 the icon will hide
            writer.Write((ushort)(Room.botCount));
            writer.Write((byte)(0));
            memory.Position += 4;
            writer.Write((byte)0);
            writer.Write((ushort)(0));
            writer.Write((ushort)(Room.VIP ? 1 : 0));
            memory.Position += 6;
            writer.Write(byte.MaxValue);
            writer.Write((ushort)1);
            writer.Write((uint)ushort.MaxValue);
            writer.Write((byte)0);
            writer.Write(33u);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            foreach (var client in Kernel.GamePool.Values.Where(i => i.Account.Server == Room.Server && i.Entity != null && i.Entity.Channel == Room.Channel && i.Entity.Lobby))
                client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_AddRoomToLobby, answer));
        }
        private static void RemoveRoomFromLobby(Room Room)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write(1u);//Type in offset 6 (May be remove room)
            writer.Write((ushort)(Room.Number - 1));
            memory.Position += 194;
            writer.Write(Constants.RoomsPerChannel - Kernel.Rooms.Where(i => i != Room && i.Channel == Room.Channel && i.Server == Room.Server).ToArray().Length);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            foreach (var client in Kernel.GamePool.Values.Where(i => i.Account.Server == Room.Server && i.Entity != null && i.Entity.Channel == Room.Channel && i.Entity.Lobby))
                client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_AddRoomToLobby, answer));
        }
        private static ushort GetRoomNumber(Room room)
        {
            ushort Number = 1;
            var array = Kernel.Rooms.Where(i => i.Channel == room.Channel && i.Server == room.Server).OrderBy(i => i.Number).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                var Room = array[i];
                if (Number + 1 <= Room.Number) break;
                Number++;
            }
            return Number;
        }
        private static void JoinToRoom(Client.GameClient client, GamePackets.Packet packet)
        {
            ushort Number = BitConverter.ToUInt16(packet.buffer, 6);
            string Password = Encoding.ASCII.GetString(packet.buffer, 8, 10).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
            var Room = Kernel.Rooms.Where(i => i.Server == client.Account.Server && i.Channel == client.Entity.Channel && i.Number == (Number - 1)).FirstOrDefault();
            if (Room == null || client.Account.Server == null || client.Entity.Channel == 0) return;
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);

            if (Room.numberOfPlayers < Room.maxNumberOfPlayers)
            {
                if (Room.Password == Password)
                {
                    Room.Players.Add(client.Entity.UID, client);
                    writer.Write((uint)Enums.RoomAnswer.Success);
                    writer.Write(Number);
                    writer.Write(Room.maxNumberOfPlayers);
                    writer.Write(Room.MapID);
                    writer.Write(1u);
                    writer.Write((uint)Room.gameMode);
                    writer.Write((byte)0);
                    writer.Write((byte)1);
                    writer.Write((ushort)0);
                    writer.Write((uint)Room.objectiveType);
                    writer.Write((uint)Room.maxObjectiveCount);
                    writer.Write(0u);
                    writer.Write((uint)((uint)(Room.Status) - 1));
                }
                else
                {
                    writer.Write((uint)Enums.RoomAnswer.WrongPassword);
                    writer.Write(Number);
                }
            }
            else
            {
                writer.Write((uint)Enums.RoomAnswer.Full);
                writer.Write(Number);
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            var answerPacket = new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_JoinRoom, answer);
            client.Send(answerPacket);
        }
        private static void BackFromRoom(Client.GameClient client)
        {
            bool Remove = false;
            var roomToRemove = new Room();
            foreach (var Room in Kernel.Rooms)
            {
                if (Room.Players.ContainsKey(client.Entity.UID))
                {
                    Room.Players.Remove(client.Entity.UID);
                    if (Room.Players.Count == 0)
                    {
                        Remove = true;
                        roomToRemove = Room;
                        RemoveRoomFromLobby(Room);
                    }
                    else
                    {
                        //Move The Host to another player and send to other players in the room that the host changed and the old host leaved
                        Room.Host = Room.Players.Values.FirstOrDefault();
                    }
                }
            }
            if (Remove) Kernel.Rooms.Remove(roomToRemove);
            client.Entity.Lobby = true;
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_BackFromRoom, new byte[4]));
        }
        private static void AutoJoinRoom(Client.GameClient client)
        {
            for (int s = 0; s < Program.Servers.ToArray().Length; s++)
            {
                for (int c = 0; c < 10; c++)
                {
                    var Rooms = Kernel.Rooms.Where(i => i.Server == Program.Servers.ToArray()[s] && i.Channel == (ushort)(c + 1) && !i.hasPassword && i.numberOfPlayers < i.Players.Count).FirstOrDefault();
                    if (Rooms != null)
                    {
                        byte[] buffer = new byte[24];
                        Writer.Write(0xE0, 11 - 6, buffer);
                        Writer.Write(s, 14 - 6, buffer);
                        Writer.Write(c, 16 - 6, buffer);
                        Writer.Write(Rooms.Number - 1, 18 - 6, buffer);
                        Writer.Write(2, 22 - 6, buffer);
                        Writer.Write(0x91, 26 - 6, buffer);
                        Writer.Write(0x54, 29 - 6, buffer);
                        client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_AutoJoin, buffer));
                    }
                }
            }
        }
        private static void SendZP(Client.GameClient client)
        {
            byte[] buffer = new byte[4];
            Writer.Write(client.Entity.ZP, 0, buffer);
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_GetZP, buffer));
        }
        private static void SendMileage(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write((byte)0);
            writer.Write(client.Entity.MP);
            writer.Write((byte)7);
            TimeSpan span = new DateTime(DateTime.Now.Month == 12 ? DateTime.Now.Year + 1 : DateTime.Now.Year, DateTime.Now.Month == 12 ? 1 : DateTime.Now.Month + 1, 1) - DateTime.Now;
            writer.Write((int)span.TotalSeconds);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_Mileage, answer));
        }
        private static void AutoJoinOptions(Client.GameClient client, GamePackets.Packet packet)
        {
            if (packet.buffer[22] != 1)//If not searching on the current channel
            {
                for (int s = 0; s < Program.Servers.ToArray().Length; s++)
                {
                    for (int c = 0; c < 10; c++)
                    {
                        var Rooms = Kernel.Rooms.Where(i => i.Server == Program.Servers.ToArray()[s] && (i.Channel == (ushort)(c + 1)) && !i.hasPassword && i.numberOfPlayers < i.Players.Count).ToArray();
                        Rooms = Rooms.Where(i => i.gameMode == (Enums.GameMode)BitConverter.ToUInt32(packet.buffer, 6)).ToArray();
                        if (BitConverter.ToUInt16(packet.buffer, 10) != ushort.MaxValue)
                            Rooms = Rooms.Where(i => i.MapID == BitConverter.ToUInt16(packet.buffer, 10)).ToArray();
                        Rooms = Rooms.Where(i => i.maxNumberOfPlayers >= packet.buffer[12]).ToArray();
                        if (packet.buffer[13] == 1) Rooms = Rooms.Where(i => i.Status == Enums.RoomStatus.inGame).ToArray();
                        if (packet.buffer[13] == 2) Rooms = Rooms.Where(i => i.Status == Enums.RoomStatus.Waiting).ToArray();
                        if (Rooms.FirstOrDefault() != null)
                        {
                            byte[] buffer = new byte[24];
                            Writer.Write(0xE0, 11 - 6, buffer);
                            Writer.Write(s, 14 - 6, buffer);
                            Writer.Write(c, 16 - 6, buffer);
                            Writer.Write(Rooms.FirstOrDefault().Number - 1, 18 - 6, buffer);
                            Writer.Write(2, 22 - 6, buffer);
                            Writer.Write(0x91, 26 - 6, buffer);
                            Writer.Write(0x54, 29 - 6, buffer);
                            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_AutoJoin, buffer));
                        }
                    }
                }
            }
            else
            {
                var Rooms = Kernel.Rooms.Where(i => i.Server == client.Account.Server && i.Channel == client.Entity.Channel && !i.hasPassword && i.numberOfPlayers < i.Players.Count).ToArray();
                Rooms = Rooms.Where(i => i.gameMode == (Enums.GameMode)BitConverter.ToUInt32(packet.buffer, 6)).ToArray();
                if (BitConverter.ToUInt16(packet.buffer, 10) != ushort.MaxValue)
                    Rooms = Rooms.Where(i => i.MapID == BitConverter.ToUInt16(packet.buffer, 10)).ToArray();
                Rooms = Rooms.Where(i => i.maxNumberOfPlayers >= packet.buffer[12]).ToArray();
                if (packet.buffer[13] == 1) Rooms = Rooms.Where(i => i.Status == Enums.RoomStatus.inGame).ToArray();
                if (packet.buffer[13] == 2) Rooms = Rooms.Where(i => i.Status == Enums.RoomStatus.Waiting).ToArray();
                if (Rooms.FirstOrDefault() != null)
                {
                    byte[] buffer = new byte[24];
                    Writer.Write(0xE0, 11 - 6, buffer);
                    Writer.Write(Array.IndexOf(Program.Servers.ToArray(), client.Account.Server), 14 - 6, buffer);
                    Writer.Write(client.Entity.Channel - 1, 16 - 6, buffer);
                    Writer.Write(Rooms.FirstOrDefault().Number - 1, 18 - 6, buffer);
                    Writer.Write(2, 22 - 6, buffer);
                    Writer.Write(0x91, 26 - 6, buffer);
                    Writer.Write(0x54, 29 - 6, buffer);
                    client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_AutoJoin, buffer));
                }
            }
        }
        private static void AddToWaitingList(Client.GameClient client)
        {
                 
        }
        private static void SendStorageItems(Client.GameClient client)
        {
            foreach (var type in Enum.GetValues(typeof(Enums.ItemsModel)).Cast<Enums.ItemsModel>())
            {
                System.IO.MemoryStream memory = new System.IO.MemoryStream();
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
                var items = client.Entity.Storage.Where(i => i.Type == type);
                writer.Write((int)type);
                writer.Write(items.Count());
                foreach (var item in items)
                {
                    if (item.ShopID != "000000000")
                        writer.Write(item.ShopID);
                    else writer.Write(new byte[9]);
                    writer.BaseStream.Position += 1;
                    writer.Write(item.StorageID);
                    writer.BaseStream.Position += 8;
                    writer.Write(Kernel.Random.Next(300000000, 800000000));//May be the UID
                    writer.BaseStream.Position += 4;
                    writer.Write(789432);
                    writer.Write((ushort)(0x1F/*item.ExpireDays*/));
                    writer.Write((ushort)(0x17/*item.ExpireHours*/));
                    writer.Write((uint)0x3B);
                    writer.Write(item.Health);
                    writer.Write(item.RepairPrice);
                    writer.Write(item.Health);
                    writer.BaseStream.Position += 18;
                }
                memory.Position = 0;
                byte[] buffer = new byte[memory.Length];
                memory.Read(buffer, 0, buffer.Length);
                writer.Close();
                memory.Close();
                client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_StorageItems, buffer));
            }
        }
        private static void SendFeverStatus(Client.GameClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            if (!client.Entity.FeverTimeActivated)
            {
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((uint)client.Entity.FPercent);
                writer.Write((ushort)client.Entity.FeverProgress);//May be the number of progresses passed
                writer.Write((ushort)100);//Max percent
            }
            else
            {
                writer.Write((ushort)3);
                writer.Write((ushort)0);
                writer.Write((uint)((client.Entity.FeverActivated.AddMinutes(client.Entity.FeverDuration) - Time32.Now).AllMinutes()));//Remaining Minutes
                writer.Write((ushort)client.Entity.FeverProgress);//May be the number of progresses passed
                writer.Write((ushort)100);//Max percent
            }
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_FeverUpdate, answer));
      //      if (!client.Entity.FeverInfoSended) SendFeverInfo(client);
        }
        public static void SendFeverReward(Client.GameClient client, uint Amount, ushort Section)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write((ulong)1);
            writer.Write((uint)4);
            writer.Write((ulong)0);
            writer.Write(Amount);
            memory.Position += 14;
            writer.Write(Section);
            writer.Write((ulong)client.Entity.FPercent);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_FeverReward, answer));
         //   if (!client.Entity.FeverInfoSended) SendFeverInfo(client);
        }
        public static void SendFeverInfo(Client.GameClient client)
        {
            client.Send(StringToBytes("F1 DC 00 01 A9 07 00 00 00 00 9B 00 00 00 28 00 00 00 34 00 00 00 68 00 00 00 1E 00 00 00 1E 00 00 00 0A 00 00 00 1E 00 00 00 01 00 00 00 9B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F2"));

            client.Entity.FeverInfoSended = true;
        }
        private static void ChangeSettings(Client.GameClient client, GamePackets.Packet packet)
        {
            if (client.Entity != null)
            {
                client.Entity.Settings = new byte[packet.buffer.Length - 7];
                Buffer.BlockCopy(packet.buffer, 6, client.Entity.Settings, 0, packet.buffer.Length - 7);
            }
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ChangeSettings, new byte[4]));
        }
        private static void SendNewGUID(Client.GameClient client)
        {
            byte[] buffer = new byte[208];
            Writer.Write(45138, 14 - 6, buffer);
            Writer.Write("pY20", 16 - 6, buffer);
            Writer.Write(44431, 22 - 6, buffer);
            Writer.Write("lY", 24 - 6, buffer);
            Writer.Write(DateTime.Now.ToString("yyyymmddhhmmss"), 26 - 6, buffer);
            Writer.Write(client.Account.Rank, 94 - 6, buffer);
            Writer.Write(client.Account.TotalKills, 98 - 6, buffer);
            Writer.Write(client.Account.TotalDeaths, 102 - 6, buffer);
            //client.Account.Identifer = Kernel.RandomString(32);
            Writer.Write(client.Account.Identifer, 106 - 6, buffer);
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_ServerTime, buffer));
        }
        public static void Announce(Client.GameClient client, string text)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            writer.Write((ushort)2);
            writer.Write(ushort.MaxValue);
            writer.Write((ushort)1);
            writer.Write((ushort)text.Length);
            writer.Write(text);
            writer.Write((byte)0);
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new GamePackets.Packet(GamePackets.Packet.PacketType.S2C_Announce, answer));
        }
        public static byte[] StringToBytes(string str, int BufferLength = 0, bool endsWithF2 = false)
        {
            if (BufferLength != 0)
            {
                string[] result = str.Split(new char[] { ' ' });
                List<byte> temp = new List<byte>();
                for (int i = 0; i < result.Length; i++)
                {
                    temp.Add(byte.Parse((result[i]), System.Globalization.NumberStyles.HexNumber));
                }
                int oldCount = temp.Count;
                for (int i = 0; i < (BufferLength - oldCount); i++)
                {
                    temp.Add((byte)(0));
                }
                if (endsWithF2)
                    temp.ToArray()[temp.ToArray().Length - 1] = 0xF2;
                return temp.ToArray();
            }
            else
            {
                string[] result = str.Split(new char[] { ' ' });
                List<byte> temp = new List<byte>(1024);
                for (int i = 0; i < result.Length; i++)
                {
                    temp.Add(byte.Parse((result[i]), System.Globalization.NumberStyles.HexNumber));
                }
                return temp.ToArray();
            }
        }
        public static string BytesToString(byte[] buffer)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var b in buffer)
            {
                builder.Append(b.ToString("X2"));
                builder.Append(" ");
            }
            return builder.Remove(builder.Length - 1, 1).ToString();
        }
        public static string DumpPacket(byte[] packet)
        {
            string DataStr = "";
            ushort PacketLength = (ushort)packet.Length;
            for (int i = 0; i < Math.Ceiling((double)PacketLength / 16); i++)
            {
                int t = 16;
                if (((i + 1) * 16) > PacketLength)
                    t = PacketLength - (i * 16);
                for (int a = 0; a < t; a++)
                {
                    DataStr += packet[i * 16 + a].ToString("X2") + " ";
                }
                if (t < 16)
                    for (int a = t; a < 16; a++)
                        DataStr += "   ";
                DataStr += "     ;";

                for (int a = 0; a < t; a++)
                {
                    DataStr += Convert.ToChar(packet[i * 16 + a]);
                }
                DataStr += Environment.NewLine;
            }
            DataStr.Replace(Convert.ToChar(0), '.');

            return DataStr.ToUpper();

        }
    }
}
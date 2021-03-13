using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace CF_Server.Auth
{
    public class PacketHandler
    {
        public static void Process(Client.AuthClient client, AuthPackets.Packet packet)
        {
            if (packet.CorrectPacket)
            {
                switch (packet.packetType)
                {
                    case AuthPackets.Packet.PacketType.C2S_Login:
                        var Username = Encoding.ASCII.GetString(packet.buffer, 16, 20).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        var Password = Encoding.ASCII.GetString(packet.buffer, 37, 20).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        var CFArugments = Encoding.ASCII.GetString(packet.buffer, 145 , 12).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        var MacAddress = Encoding.ASCII.GetString(packet.buffer, 407, 12).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        client.Account = new Database.AccountTable(Username)
                        {
                            IP = client.Socket.IP
                        };
                        client.Account.Identifer = Encoding.ASCII.GetString(packet.buffer, 77, 32).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
                        if (client.Account.exists && client.Account.Password == Password && !Kernel.GamePool.ContainsKey(client.Account.EntityID))
                        {
                            Kernel.AwaitingPool[client.Account.Identifer] = client.Account;
                            Login(Enums.LoginTypes.NoError, client);
                        }
                        else if (client.Account.exists && client.Account.Password == Password && Kernel.GamePool.ContainsKey(client.Account.EntityID))
                        {
                            Login(Enums.LoginTypes.Player_Already_Logged_In, client);
                        }
                        else
                        {
                            Login(Enums.LoginTypes.Unknown_Username_Or_Password, client);
                        }
                        break;
                    case AuthPackets.Packet.PacketType.C2S_AccountAlreadyLoggedOn:
                        if (client.Account != null)
                        {
                            var player = Kernel.GamePool.Values.Where(i => i.Account.EntityID == client.Account.EntityID).FirstOrDefault();
                            player.Disconnect();
                            packet = new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_PlayerHasBeenLoggedOut, new byte[0]);
                            client.Send(packet);
                        }
                        break;
                    case AuthPackets.Packet.PacketType.C2S_LoginToGameServer_Step1:
                         byte ServerNumber = packet.buffer[6];
                        client.Account.Server = Program.Servers[ServerNumber - 1];
                        SendNewGUID(client);
                        break;
                    case AuthPackets.Packet.PacketType.C2S_LoginToGameServer_Step2:
                        while (true)
                        {
                            if (Kernel.GamePool.ContainsKey(client.Account.EntityID) && Kernel.GamePool[client.Account.EntityID].Entity.FullyLoaded)
                            {
                                var ans = new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_LoginToGameServer_Step2, new byte[4] { 1, 0, 0, 0 });
                                client.Send(ans);
                                break;
                            }
                            else System.Threading.Thread.Sleep(10);
                        }
                        break;
                    case AuthPackets.Packet.PacketType.C2S_GoBackForServers:
                        ReturnToServers(client, packet);
                        break;
                    case AuthPackets.Packet.PacketType.C2S_CheckNameExsitance:
                        CheckNameExistance(client, packet);
                        break;
                    case AuthPackets.Packet.PacketType.C2S_CreateAccount:
                        CreateAccount(client, packet);
                        break;
                    case AuthPackets.Packet.PacketType.C2S_Exit:
                        //Send a Packet To Confirm Exit
                        packet.buffer[4] = 12;
                        client.Send(packet);
                        client.Disconnect();
                        break;
                    case AuthPackets.Packet.PacketType.Unknown:
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
        private static void Login(Enums.LoginTypes type, Client.AuthClient client)
        {
            if (type == Enums.LoginTypes.NoError)
            {
                byte[] Buffer = new byte[800];
                var packet = new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_ValidAccount, Buffer);
                packet.buffer[8] = 2; packet.buffer[406] = 1;
                client.Send(packet);
                SendServers(client);
            }
            else
            {
                byte[] buff = new byte[1452];
                Writer.Write((byte)type, 2, buff);
                if (type == Enums.LoginTypes.Player_Already_Logged_In)
                    Writer.Write(client.Account.EntityID, 26, buff);
                client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_DisplayError, buff));
            }
        }
        private static void SendNewGUID(Client.AuthClient client)
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
           // client.Account.Identifer = Kernel.RandomString(32);
            Writer.Write(client.Account.Identifer, 106 - 6, buffer);
            client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_LoginToGameServer_Step1, buffer));
        }
        private static void CreateAccount(Client.AuthClient client, AuthPackets.Packet packet)
        {
            string Name = Encoding.ASCII.GetString(packet.buffer, 6, 12).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
            if (!Database.EntityTable.NameExists(Name) && Database.EntityTable.CreateEntity(client, Name))
                client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_CreateAccount, new byte[4]));
        }
        private static void CheckNameExistance(Client.AuthClient client, AuthPackets.Packet packet)
        {
            string Name = Encoding.ASCII.GetString(packet.buffer, 6, 12).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
            client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_CheckNameExsitance, new byte[4] { (byte)(Database.EntityTable.NameExists(Name) ? 0x2 : 0x0), 0x0, 0x0, 0x0 }));
        }
        private static void TryEnterServer(Client.AuthClient client)
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep(Constants.timeToAutomaticallyEnterServer * 1000);
                client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_TryEnter, new byte[0]));
            });
            thread.Start();
        }
        private static void SendServers(Client.AuthClient client)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            #region Character Info
            writer.Write(client.Account.EntityID > 0 ? 0 : 6);
            if (client.Account.EntityID > 0)
            {
                writer.Write(Encoding.ASCII.GetBytes(client.Account.Name));
                writer.Write(new byte[16 - client.Account.Name.Length]);
            }
            else
            {
                memory.Position += 16;
            }
            writer.Write(client.Account.EntityID);
            writer.Write((ushort)0);
            writer.Write(client.Account.Rank);
            writer.Write(client.Account.TotalKills);
            writer.Write(client.Account.TotalDeaths);
            writer.Write(0u);
            writer.Write(StringToBytes("2D 3E F7 D5 CF 18 F7 3F"));
            writer.Write((ulong)Program.Servers.Length);
            #endregion
            #region Servers
            for (int i = 0; i < Program.Servers.Length; i++)
            {
                var Server = Program.Servers[i];
                writer.Write((ushort)(Server.serverType));
                writer.Write(Server.NoLimit);
                writer.Write(Server.MinRank);
                writer.Write(Server.MaxRank);
                memory.Position += 16;
                writer.Write((ushort)(i + 1));
                writer.Write(Encoding.ASCII.GetBytes(Server.Name));
                writer.Write(new byte[34 - Server.Name.Length]);
                writer.Write(Server.Port);
                writer.Write(Server.IPBytes);
                writer.Write(100u);
                writer.Write((ulong)((double)Kernel.GamePool.Values.Where(x => x.Account.Server == Server).Count() / (double)Server.MaxPlayers * 100d));//FF FF FF FF Should be Maintanance
            }
            #endregion
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_GetServers, answer));
            TryEnterServer(client);
        }
        private static void ReturnToServers(Client.AuthClient client, AuthPackets.Packet packet)
        {
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memory);
            string Username = Encoding.ASCII.GetString(packet.buffer, 10, 20).Replace(Encoding.ASCII.GetString(new byte[] { 0x0 }), "");
            uint UID = BitConverter.ToUInt32(packet.buffer, 6);
            client.Account = Kernel.GamePool.Values.Where(i => i.Entity.UID == UID && i.Account.Username == Username).FirstOrDefault().Account;
            Kernel.AwaitingPool[client.Account.Identifer] = client.Account;
            #region Character Info
            memory.Position += 20;
            writer.Write(client.Account.EntityID);
            writer.Write((ushort)0);
            writer.Write(client.Account.Rank);
            writer.Write(client.Account.TotalKills);
            writer.Write(client.Account.TotalDeaths);
            writer.Write(0u);
            writer.Write(StringToBytes("2D 3E F7 D5 CF 18 F7 3F"));
            writer.Write((ulong)Program.Servers.Length);
            #endregion
            #region Servers
            for (int i = 0; i < Program.Servers.Length; i++)
            {
                var Server = Program.Servers[i];
                writer.Write((ushort)(Server.serverType));
                writer.Write(Server.NoLimit);
                writer.Write(Server.MinRank);
                writer.Write(Server.MaxRank);
                memory.Position += 16;
                writer.Write((ushort)(i + 1));
                writer.Write(Encoding.ASCII.GetBytes(Server.Name));
                writer.Write(new byte[34 - Server.Name.Length]);
                writer.Write(Server.Port);
                writer.Write(Server.IPBytes);
                writer.Write(100u);
                writer.Write((ulong)((double)Kernel.GamePool.Values.Where(x => x.Account.Server == Server).Count() / (double)Server.MaxPlayers * 100d));//FF FF FF FF Should be Maintanance
            }
            #endregion
            memory.Position = 0;
            byte[] answer = new byte[memory.Length];
            memory.Read(answer, 0, answer.Length);
            writer.Close();
            memory.Close();
            client.Send(new AuthPackets.Packet(AuthPackets.Packet.PacketType.S2C_GoBackForServers, answer));
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
using System;

namespace CF_Server
{
	public class GameServer
	{
		public string Name;

		public int Port;

		public uint MaxPlayers;

        public byte[] IPBytes;

        public string IP;

		public ushort NoLimit;

		public ushort MinRank;

		public ushort MaxRank;

		public Enums.ServerType serverType;

        public CF_Server.Network.Sockets.ServerSocket Server;

        public void Open()
        {
            Server = new Network.Sockets.ServerSocket();
            Server.OnClientConnect += Program.GameServer_OnClientConnect;
            Server.OnClientReceive += Program.GameServer_OnClientReceive;
            Server.OnClientDisconnect += Program.GameServer_OnClientDisconnect;
            Server.Enable((ushort)Port);
        }
	}
}

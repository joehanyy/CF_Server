using System;

namespace CF_Server.Client
{
    public class GameClient
    {
        public GameClient(Network.Sockets.ClientWrapper socket)
        {
            Socket = socket;
            Entity = new Entity();
        }
        public Database.AccountTable Account;
        public IDisposable[] TimerSubscriptions;
        public object TimerSyncRoot;
        public Network.Sockets.ClientWrapper Socket;
        public Entity Entity;
        public void Save()
        {
            if (Entity != null) Database.EntityTable.SaveEntity(this);
        }
        public void Disconnect(bool Save = true)
        {
            if (Socket == null || Socket.Connector == null || !Kernel.GamePool.ContainsKey(Entity.UID)) return;
            if (Save) this.Save();
            Socket.Disconnect();
            Program.Thread.Unregister(this);
            Kernel.GamePool.Remove(Entity.UID);
            Console.WriteLine(Entity.Name + " has been logged off!");
            Program.UpdateConsoleTitle();
        }
        public void Send(byte[] buffer)
        {
            buffer[0] = Constants.packetStartsWith;
            buffer[buffer.Length - 1] = Constants.packetEndsWith;
            Socket.Send(buffer);
            if (Program.CapturePackets)
            {
                Console.WriteLine("Captured a packet from Server (Game Server) Length: " + buffer.Length + " Type: (" + buffer[3] + "-" + buffer[4] + "-" + buffer[5] + ")");
            }
        }
        public void Send(GamePackets.Packet packet)
        {
            packet.buffer[0] = Constants.packetStartsWith;
            packet.buffer[packet.buffer.Length - 1] = Constants.packetEndsWith;
            Socket.Send(packet.buffer);
            if (Program.CapturePackets)
            {
                Console.WriteLine("Captured a packet from Server (Game Server) Length: " + packet.buffer.Length + " Type: (" + packet.buffer[3] + "-" + packet.buffer[4] + "-" + packet.buffer[5] + ")");
            }
        }
    }
}

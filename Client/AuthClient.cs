using System;

namespace CF_Server.Client
{
    public class AuthClient
    {
        public AuthClient(Network.Sockets.ClientWrapper socket)
        {
            Socket = socket;
        }
        public Network.Sockets.ClientWrapper Socket;
        public Database.AccountTable Account;
        public void Disconnect()
        {
            Socket.Disconnect();
        }
        public void Send(byte[] buffer)
        {
            buffer[0] = Constants.packetStartsWith;
            buffer[buffer.Length - 1] = Constants.packetEndsWith;
            Socket.Send(buffer);
            if (Program.CapturePackets)
            {
                Console.WriteLine("Captured a packet from Server (Auth Server) Length: " + buffer.Length + " Type: (" + buffer[3] + "-" + buffer[4] + "-" + buffer[5] + ")");
            }
        }
        public void Send(AuthPackets.Packet packet)
        {
            packet.buffer[0] = Constants.packetStartsWith;
            packet.buffer[packet.buffer.Length - 1] = Constants.packetEndsWith;
            Socket.Send(packet.buffer);
            if (Program.CapturePackets)
            {
                Console.WriteLine("Captured a packet from Server (Auth Server) Length: " + packet.buffer.Length + " Type: (" + packet.buffer[3] + "-" + packet.buffer[4] + "-" + packet.buffer[5] + ")");
            }
        }
    }
}

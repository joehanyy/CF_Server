using System;
using System.Text;

namespace CF_Server.AuthPackets
{
    public class Packet
    {
        public Packet(byte[] buffer)
        {
            this.buffer = buffer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type of the packet</param>
        /// <param name="data">buffer of the packet that adds to the raw packet in offset 6</param>
        /// <param name="optionalLength">Leave it 0 if you want the length of the packet to be the buffer length or type a value on it to type it in offset 1 in raw packet</param>
        public Packet(PacketType type, byte[] data, int optionalLength = 0)
        {
            buffer = new byte[data.Length + 7];
            buffer[0] = Constants.packetStartsWith; buffer[buffer.Length - 1] = Constants.packetEndsWith;
            for (int i = 6; i < (data.Length + 6); i++)
            {
                buffer[i] = data[i - 6];
            }
            if (optionalLength == 0)
                Writer.Write(data.Length, 1, buffer);
            else Writer.Write(optionalLength, 1, buffer);
            packetType = type;
            switch (type)
            {
                case PacketType.S2C_DisplayError:            buffer[3] = 0; buffer[4] = 1; buffer[5] = 0; break;
                case PacketType.S2C_GetServers:              buffer[3] = 0; buffer[4] = 1; buffer[5] = 0; break;
                case PacketType.S2C_GoBackForServers:        buffer[3] = 0; buffer[4] = 3; buffer[5] = 0; break;
                case PacketType.S2C_TryEnter:                buffer[3] = 0; buffer[4] = 7; buffer[5] = 2; break;
                case PacketType.S2C_CreateAccount:           buffer[3] = 0; buffer[4] = 9; buffer[5] = 0; break;
                case PacketType.S2C_CheckNameExsitance:      buffer[3] = 0; buffer[4] = 11; buffer[5] = 0; break;
                case PacketType.S2C_PlayerHasBeenLoggedOut:  buffer[3] = 0; buffer[4] = 13; buffer[5] = 0; break;
                case PacketType.S2C_LoginToGameServer_Step1: buffer[3] = 0; buffer[4] = 16; buffer[5] = 0; break;
                case PacketType.S2C_LoginToGameServer_Step2: buffer[3] = 0; buffer[4] = 18; buffer[5] = 0; break;
                case PacketType.S2C_ExitGameInfo:            buffer[3] = 0; buffer[4] = 22; buffer[5] = 0; break;
                case PacketType.S2C_ValidAccount:            buffer[3] = 0; buffer[4] = 25; buffer[5] = 0; break;
            }
        }
        public void Deserialize(byte[] buffer)
        {
            byte Offset3 = buffer[3];
            byte Offset4 = buffer[4];
            byte Offset5 = buffer[5];
            byte Offset6 = buffer[6];
            switch (buffer[3])
            {
                case 0:
                    {
                        switch (buffer[4])
                        {
                            case 0:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_Login; break;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_GoBackForServers; break;
                                    }
                                    break;
                                }
                            case 8:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_CreateAccount; break;
                                    }
                                    break;
                                }
                            case 10:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_CheckNameExsitance; break;
                                    }
                                    break;
                                }
                            case 12:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_AccountAlreadyLoggedOn; break;
                                    }
                                    break;
                                }
                            case 15:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_LoginToGameServer_Step1; break;
                                    }
                                    break;
                                }
                            case 17:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0:
                                            {
                                                switch (buffer[6])
                                                {
                                                    case 0: packetType = PacketType.C2S_Exit; break;
                                                    case 1: packetType = PacketType.C2S_LoginToGameServer_Step2; break;
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case 21:
                                {
                                    switch (buffer[5])
                                    {
                                        case 0: packetType = PacketType.C2S_RequestExit; break;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }

        }
        public enum PacketType
        {
            Unknown,
            #region Client To Server
            C2S_Login,
            C2S_GoBackForServers,
            C2S_CreateAccount,
            C2S_CheckNameExsitance,
            C2S_AccountAlreadyLoggedOn,
            C2S_LoginToGameServer_Step1,
            C2S_Exit,
            C2S_LoginToGameServer_Step2,
            C2S_RequestExit,
            #endregion

            #region Server To Client
            S2C_DisplayError,
            S2C_GetServers,
            S2C_GoBackForServers,
            S2C_TryEnter,
            S2C_CreateAccount,
            S2C_CheckNameExsitance,
            S2C_PlayerHasBeenLoggedOut,
            S2C_LoginToGameServer_Step1,
            S2C_LoginToGameServer_Step2,
            S2C_ExitGameInfo,
            S2C_ValidAccount,
            #endregion
        }
        public PacketType packetType = PacketType.Unknown;
        public byte[] buffer;
        public bool CorrectPacket
        {
            get
            {
                return buffer[0] == Constants.packetStartsWith && buffer[buffer.Length - 1] == Constants.packetEndsWith;
            }
        }
    }
}

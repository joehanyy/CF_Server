using System;
using System.Text;

namespace CF_Server.GamePackets
{
    public class Packet
    {
        public Packet(byte[] buffer)
        {
            this.buffer = buffer;
            if (buffer[0] == Constants.packetStartsWith && buffer[buffer.Length - 1] == Constants.packetEndsWith)
                CorrectPacket = true;
            else { CorrectPacket = false; return; }
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
            CorrectPacket = true;
            switch (type)
            {
                case PacketType.S2C_ExitGameInfo: buffer[3] = 0; buffer[4] = 22; buffer[5] = 0; break;
                case PacketType.S2C_PlayerData: buffer[3] = 1; buffer[4] = 1; buffer[5] = 0; break;
                case PacketType.S2C_ServerTime: buffer[3] = 1; buffer[4] = 6; buffer[5] = 0; break;
                case PacketType.S2C_GameServer: buffer[3] = 1; buffer[4] = 9; buffer[5] = 0; break;
                case PacketType.S2C_ConfirmBack: buffer[3] = 1; buffer[4] = 11; buffer[5] = 0; break;
                case PacketType.S2C_AutoJoin: buffer[3] = 1; buffer[4] = 19; buffer[5] = 4; break;
                case PacketType.S2C_GetPlayerStat: buffer[3] = 1; buffer[4] = 94; buffer[5] = 0; break;
                case PacketType.S2C_ChangeSettings: buffer[3] = 1; buffer[4] = 24; buffer[5] = 0; break;
                case PacketType.S2C_ExitGameInfoInto: buffer[3] = 1; buffer[4] = 27; buffer[5] = 0; break;
                case PacketType.S2C_ChannelJoin: buffer[3] = 1; buffer[4] = 32; buffer[5] = 0; break;
                case PacketType.S2C_ExitFromChannel: buffer[3] = 1; buffer[4] = 36; buffer[5] = 0; break;
                case PacketType.S2C_GetChannels: buffer[3] = 1; buffer[4] = 37; buffer[5] = 0; break;
                case PacketType.S2C_GetPlayersOnChannel: buffer[3] = 1; buffer[4] = 38; buffer[5] = 0; break;
                case PacketType.S2C_GetRooms: buffer[3] = 1; buffer[4] = 51; buffer[5] = 2; break;
                case PacketType.S2C_CreateRoom: buffer[3] = 1; buffer[4] = 53; buffer[5] = 0; break;
                case PacketType.S2C_JoinRoom: buffer[3] = 1; buffer[4] = 55; buffer[5] = 0; break;
                case PacketType.S2C_BackFromRoom: buffer[3] = 1; buffer[4] = 65; buffer[5] = 0; break;
                case PacketType.S2C_UpdateWaitingList: buffer[3] = 1; buffer[4] = 81; buffer[5] = 0; break;
                case PacketType.S2C_AddRoomToLobby: buffer[3] = 1; buffer[4] = 82; buffer[5] = 0; break;
                case PacketType.S2C_GetAnotherPlayerStats: buffer[3] = 1; buffer[4] = 94; buffer[5] = 0; break;
                case PacketType.S2C_GetZP: buffer[3] = 1; buffer[4] = 129; buffer[5] = 0; break;
                case PacketType.S2C_FeverUpdate: buffer[3] = 1; buffer[4] = 169; buffer[5] = 2; break;
                case PacketType.S2C_FeverReward: buffer[3] = 1; buffer[4] = 169; buffer[5] = 5; break;
                case PacketType.S2C_FeverInfoUpdate: buffer[3] = 1; buffer[4] = 169; buffer[5] = 7; break;
                case PacketType.S2C_HeartBeat: buffer[3] = 1; buffer[4] = 172; buffer[5] = 0; break;
                case PacketType.S2C_StorageItems: buffer[3] = 1; buffer[4] = 201; buffer[5] = 0; break;

                case PacketType.S2C_Announce: buffer[3] = 4; buffer[4] = 8; buffer[5] = 0; break;

                case PacketType.S2C_Mileage: buffer[3] = 10; buffer[4] = 35; buffer[5] = 1; break;
            }
        }
        public void Deserialize(byte[] buffer)
        {
            byte Offset3 = buffer[3];
            byte Offset4 = buffer[4];
            byte Offset5 = buffer[5];
            byte Offset6 = buffer[6];
            if (Offset3 == 0 && Offset4 == 21 && Offset5 == 0) { packetType = PacketType.C2S_RequestExit; }
            if (Offset3 == 1 && Offset4 == 0 && Offset5 == 0) { packetType = PacketType.C2S_AuthToChannelServer; }
            if (Offset3 == 1 && Offset4 == 5 && Offset5 == 0) { packetType = PacketType.C2S_ServerTime; }
            if (Offset3 == 1 && Offset4 == 10 && Offset5 == 0) { packetType = PacketType.C2S_ConfirmBack; }
            if (Offset3 == 1 && Offset4 == 19 && Offset5 == 1) { packetType = PacketType.C2S_AutoJoin; }
            if (Offset3 == 1 && Offset4 == 19 && Offset5 == 6) { packetType = PacketType.C2S_AutoJoinOptions; }
            if (Offset3 == 1 && Offset4 == 23 && Offset5 == 0) { packetType = PacketType.C2S_ChangeSettings; }
            if (Offset3 == 1 && Offset4 == 26 && Offset5 == 0) { packetType = PacketType.C2S_ExitGameInfoInto; }
            if (Offset3 == 1 && Offset4 == 30 && Offset5 == 0) { packetType = PacketType.C2S_GetChannels; }
            if (Offset3 == 1 && Offset4 == 31 && Offset5 == 0) { packetType = PacketType.C2S_ChannelJoin; }
            if (Offset3 == 1 && Offset4 == 33 && Offset5 == 0) { packetType = PacketType.C2S_GetPlayersOnChannel; }
            if (Offset3 == 1 && Offset4 == 35 && Offset5 == 0) { packetType = PacketType.C2S_ExitFromChannelToChannelsList; }
            if (Offset3 == 1 && Offset4 == 50 && Offset5 == 0) { packetType = PacketType.C2S_ChannelData; }
            if (Offset3 == 1 && Offset4 == 52 && Offset5 == 0) { packetType = PacketType.C2S_CreateRoom; }
            if (Offset3 == 1 && Offset4 == 54 && Offset5 == 0) { packetType = PacketType.C2S_JoinToRoom; }
            if (Offset3 == 1 && Offset4 == 64 && Offset5 == 0) { packetType = PacketType.C2S_BackFromRoom; }
            if (Offset3 == 1 && Offset4 == 92 && Offset5 == 0) { packetType = PacketType.C2S_GetAnotherPlayerStats; }
            if (Offset3 == 1 && Offset4 == 93 && Offset5 == 0) { packetType = PacketType.C2S_GetPlayerStats; }
            if (Offset3 == 1 && Offset4 == 128 && Offset5 == 0) { packetType = PacketType.C2S_GetZP; }
            if (Offset3 == 1 && Offset4 == 132 && Offset5 == 0) { packetType = PacketType.С2S_EnterToShootingRoom; }
            if (Offset3 == 1 && Offset4 == 169 && Offset5 == 1) { packetType = PacketType.C2S_FeverUpdate; }
            if (Offset3 == 1 && Offset4 == 169 && Offset5 == 4) { packetType = PacketType.C2S_ChannelsUpdate; }
            if (Offset3 == 1 && Offset4 == 169 && Offset5 == 6) { packetType = PacketType.C2S_FeverInfoUpdate; }
            if (Offset3 == 1 && Offset4 == 171 && Offset5 == 0) { packetType = PacketType.C2S_HeartBeat; }
            if (Offset3 == 1 && Offset4 == 200 && Offset5 == 0) { packetType = PacketType.C2S_StorageItems; }

            if (Offset3 == 10 && Offset4 == 35 && Offset5 == 0) { packetType = PacketType.C2S_Mileage; }
        }
        public enum PacketType
        {
            Unknown,
            #region Client To Server
            C2S_RequestExit,
            C2S_AuthToChannelServer,
            C2S_ServerTime,
            C2S_ConfirmBack,
            C2S_AutoJoin,
            C2S_AutoJoinOptions,
            C2S_ChangeSettings,
            C2S_ExitGameInfoInto,
            C2S_GetChannels,
            C2S_ChannelJoin,
            C2S_GetPlayersOnChannel,
            C2S_ExitFromChannelToChannelsList,
            C2S_ChannelData,
            C2S_CreateRoom,
            C2S_JoinToRoom,
            C2S_BackFromRoom,
            C2S_GetAnotherPlayerStats,
            C2S_GetPlayerStats,
            C2S_GetZP,
            С2S_EnterToShootingRoom,
            C2S_FeverUpdate,
            C2S_ChannelsUpdate,
            C2S_FeverInfoUpdate,
            C2S_HeartBeat,
            C2S_StorageItems,
            C2S_Mileage,
            #endregion

            #region Server To Client
            S2C_ExitGameInfo,
            S2C_ConfirmBack,
            S2C_AutoJoin,
            S2C_PlayerData,
            S2C_ServerTime,
            S2C_GameServer,
            S2C_GetPlayerStat,
            S2C_ChangeSettings,
            S2C_ExitGameInfoInto,
            S2C_ChannelJoin,
            S2C_ExitFromChannel,
            S2C_GetPlayersOnChannel,
            S2C_GetChannels,
            S2C_GetRooms,
            S2C_CreateRoom,
            S2C_JoinRoom,
            S2C_BackFromRoom,
            S2C_GetAnotherPlayerStats,
            S2C_GetZP,
            S2C_FeverUpdate,
            S2C_FeverReward,
            S2C_FeverInfoUpdate,
            S2C_UpdateWaitingList,
            S2C_AddRoomToLobby,
            S2C_HeartBeat,
            S2C_StorageItems,
            S2C_Announce,
            S2C_Mileage,
            #endregion
        }
        public PacketType packetType = PacketType.Unknown;
        public byte[] buffer;
        public bool CorrectPacket = false;
    }
}

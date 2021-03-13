using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CF_Server
{
    public class Room
    {
        public Enums.GameMode gameMode;
        public ushort MapID, Number, Channel;
        public string Name, Password;
        public byte maxNumberOfPlayers, observersCount, botCount, maxObjectiveCount, objectiveCount;
        public Dictionary<uint, Client.GameClient> Players;
        public byte numberOfPlayers
        {
            get { return (byte)Players.Count; }
        }
        public Client.GameClient Host;
        public bool VIP, NoFlash_Smoke;
        public bool hasPassword { get { return Password != null && Password != ""; } }
        public Enums.RoomStatus Status;
        public GameServer Server;
        public Enums.RoomObjType objectiveType;
        public Enums.RoomWeapons Weapons;
    }
    public class Battle
    {
        public Enums.GameMode GameMode;
        public bool Won;
        public ushort Kills, Deaths;
    }
}

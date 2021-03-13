using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CF_Server.Database
{
    public class AccountTable
    {
        public string Username;
        public string Password;
        public string IP;
        public string Identifer;
        public GameServer Server;
        public ushort Rank;
        public uint TotalKills, TotalDeaths;
        public string Name;
        public Enums.AccountState State;
        public uint EntityID;
        public bool exists = false;

        public AccountTable(string username)
        {
            if (username == null) return;
            this.Username = username;
            this.Password = "";
            this.IP = "";
            this.State = Enums.AccountState.Default;
            this.EntityID = 0;
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("accounts").Where("Username", username))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    exists = true;
                    this.Password = reader.ReadString("Password");
                    this.IP = reader.ReadString("Ip");
                    this.EntityID = reader.ReadUInt32("EntityID");
                    this.State = (Enums.AccountState)reader.ReadInt32("State");
                    using (var cmd2 = new MySqlCommand(MySqlCommandType.SELECT).Select("characters").Where("UID", EntityID))
                    using (var reader2 = new MySqlReader(cmd2))
                    {
                        if (reader2.Read())
                        {
                            Name = reader2.ReadString("Name");
                            Rank = (ushort)Entity.RankExperiences.Where(i => i.Value >= reader2.ReadUInt32("Experience")).FirstOrDefault().Key;
                            byte[] Battles = reader2.ReadBlob("Battles");
                            var battles = new List<Battle>();
                            if (Battles.Length > 0)
                            {
                                using (var stream = new System.IO.MemoryStream(Battles))
                                using (var rdr = new System.IO.BinaryReader(stream))
                                {
                                    int count = rdr.ReadInt32();
                                    for (int i = 0; i < count; i++)
                                    {
                                        Battle battle = new Battle
                                        {
                                            GameMode = (Enums.GameMode)rdr.ReadByte(),
                                            Won = rdr.ReadBoolean(),
                                            Kills = rdr.ReadUInt16(),
                                            Deaths = rdr.ReadUInt16()
                                        };
                                        battles.Add(battle);
                                    }
                                }
                            }
                            TotalDeaths = (uint)battles.Sum(i => i.Deaths);
                            TotalKills = (uint)battles.Sum(i => i.Kills);
                        }
                    }
                }
            }
        }

        public void Save()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("accounts").Set("Password", Password).Set("Ip", IP).Set("EntityID", EntityID)
                    .Where("Username", Username).Execute();
        }
    }
}

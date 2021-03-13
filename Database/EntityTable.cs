using System;

namespace CF_Server.Database
{
    public static class EntityTable
    {
        public static bool LoadEntity(Client.GameClient client)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("characters").Where("UID", client.Account.EntityID))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    try
                    {
                        client.Entity = new Entity();
                        client.Entity.Owner = client;
                        client.Entity.UID = reader.ReadUInt32("UID");
                        client.Entity.Experience = reader.ReadUInt32("Experience");
                        client.Entity.Clan = reader.ReadString("Clan");
                        client.Entity.Name = reader.ReadString("Name");
                        client.Entity.Deserion = reader.ReadUInt32("Deserion");
                        client.Entity.GeneradeKills = reader.ReadUInt32("GeneradeKills");
                        client.Entity.GP = reader.ReadUInt32("GP");
                        client.Entity.ZP = reader.ReadUInt32("ZP");
                        client.Entity.MP = reader.ReadUInt32("MP");
                        client.Entity.VIP = reader.ReadBoolean("VIP");
                        client.Entity.HeadshotKills = reader.ReadUInt32("HeadshotKills");
                        client.Entity.KnifeKills = reader.ReadUInt32("KnifeKills");
                        client.Entity.TeamKills = reader.ReadUInt32("TeamKills");
                        client.Entity.Settings = reader.ReadBlob("Settings");
                        client.Entity.Battles = new System.Collections.Generic.List<Battle>();
                        byte[] Battles = reader.ReadBlob("Battles");
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
                                    client.Entity.Battles.Add(battle);
                                }
                            }
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return false;
                    }
                }
                else
                    return false;
            }
        }
        public static bool SaveEntity(Client.GameClient c, MySql.Data.MySqlClient.MySqlConnection conn)
        {
            try
            {
                if (c == null) return false;
                var e = c.Entity;
                if (e == null) return false;
                #region Battles
                if (e.Battles != null)
                {
                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream);
                    writer.Write(e.Battles.Count);
                    foreach (var battle in e.Battles)
                    {
                        writer.Write((byte)battle.GameMode);
                        writer.Write(battle.Won);
                        writer.Write(battle.Kills);
                        writer.Write(battle.Deaths);
                    }
                    string SQL = "UPDATE `characters` SET Battles=@battles where UID = " + e.UID + " ;";
                    byte[] rawData = stream.ToArray();
                    using (var con = Program.MySqlConnection)
                    {
                        con.Open();
                        using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = SQL;
                            cmd.Parameters.AddWithValue("@battles", rawData);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                #endregion
                #region Settings
                {
                    string SQL = "UPDATE `characters` SET Settings=@settings where UID = " + e.UID + " ;";
                    using (var con = Program.MySqlConnection)
                    {
                        con.Open();
                        using (var cmd = new MySql.Data.MySqlClient.MySqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandText = SQL;
                            cmd.Parameters.AddWithValue("@settings", e.Settings);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                #endregion
                using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE).Update("characters"))
                {
                    cmd.Set("Experience", e.Experience);
                    cmd.Set("Name", e.Name);
                    cmd.Set("Clan", e.Clan);
                    cmd.Set("HeadshotKills", e.HeadshotKills);
                    cmd.Set("GeneradeKills", e.GeneradeKills);
                    cmd.Set("KnifeKills", e.KnifeKills);
                    cmd.Set("TeamKills", e.TeamKills);
                    cmd.Set("VIP", e.VIP);
                    cmd.Set("Deserion", e.Deserion);
                    cmd.Set("GP", e.GP);
                    cmd.Set("ZP", e.ZP);
                    cmd.Set("MP", e.MP);
                    cmd.Where("UID", e.UID);
                    cmd.Execute();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public static bool CreateEntity(Client.AuthClient client, string Name)
        {
            if (Name.Length > 12)
                Name = Name.Substring(0, 12);
            if (Name == "")
                return false;
            uint UID = NextUID;
            while (UIDExists(UID)) { UID = NextUID; }
            while (true)
            {
                try
                {
                    using (var cmd = new MySqlCommand(MySqlCommandType.INSERT))
                        cmd.Insert("characters")
                            .Insert("Name", Name)
                            .Insert("GP", Constants.DefaultGP)
                            .Insert("UID", UID)
                            .Execute();
                    break;
                }
                catch
                {
                    UID = NextUID;
                }
            }
            client.Account.EntityID = UID;
            client.Account.Save();
            return true;
        }
        public static uint NextUID
        {
            get
            {
                uint now = 20000000;//20 Millions The Default UID
                using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("characters"))
                using (var reader = new MySqlReader(cmd))
                {
                    while (reader.Read())
                    {
                        uint UID = reader.ReadUInt32("UID");
                        if ((UID > 0) && (UID > now))
                        {
                            now = UID + 1;
                        }
                    }
                }
                return now;
            }
        }
        public static bool SaveEntity(Client.GameClient c)
        {
            using (var conn = Program.MySqlConnection)
            {
                conn.Open();
                return SaveEntity(c, conn);
            }
        }
        public static bool NameExists(string Name)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("characters").Where("Name", Name))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool UsernameExists(string Username)
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("accounts").Where("Username", Username))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool UIDExists(uint UID)
        {
            MySqlCommand command = new MySqlCommand(MySqlCommandType.SELECT);
            command.Select("characters").Where("UID", UID);
            MySqlReader reader = new MySqlReader(command);
            if (reader.Read())
            {
                return true;
            }
            return false;
        }
    }
}

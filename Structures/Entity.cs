using System;
using System.Collections.Generic;
using System.Linq;

namespace CF_Server
{
    public class Entity
    {
        public bool FullyLoaded = false;
        private byte _FPercent = 96;
        public byte FPercent
        {
            get
            {
                return _FPercent;
            }
            set
            {
                _FPercent = value;
                if (_FPercent > 100)
                {
                    _FPercent = 0;
                    FeverProgress++;
                    if (FeverProgress > 2)
                    {
                        FeverProgress = 2;
                        _FPercent = 100;
                    }
                    if (FeverProgress == 1) { FPercent = 0; MP += 30; FeverTimeActivated = true; FeverActivated = Time32.Now; FeverDuration = 40; Game.PacketHandler.SendFeverReward(Owner, 30, 0); }
                    if (FeverProgress == 2) { FPercent = 0; MP += 50; FeverTimeActivated = true; FeverActivated = Time32.Now; FeverDuration = 40; Game.PacketHandler.SendFeverReward(Owner, 50, 0); }
                    if (FeverProgress == 3) { FPercent = 0; MP += 100; FeverTimeActivated = true; FeverActivated = Time32.Now; FeverDuration = 1440; Game.PacketHandler.SendFeverReward(Owner, 100, 0); }
                }
            }

        }
        public bool FeverInfoSended = false;
        public Client.GameClient Owner;
        public string Name, Clan;
        public uint UID;
        public uint Experience, GP, ZP, MP;
        public uint HeadshotKills, GeneradeKills, KnifeKills, Deserion, TeamKills;
        public bool VIP, Lobby = false;
        public byte[] Settings;
        public Time32 Announce = Time32.Now;
        public Time32 FeverTime = Time32.Now;
        public byte FeverProgress;

        public ushort FeverDuration;
        public Time32 FeverActivated;
        public bool FeverTimeActivated;
        public List<Battle> Battles = new List<Battle>();

        public List<Item> Storage = new List<Item>()
            {
                new Item() { Health = 99999, RepairPrice = 0, Type = Enums.ItemsModel.Bag, ShopID = "000004701", StorageID = "D0001" },
                new Item() { Health = 99999, RepairPrice = 0, Type = Enums.ItemsModel.Bag, ShopID = "000000000", StorageID = "D0002" },
                new Item() { Health = 99999, RepairPrice = 0, Type = Enums.ItemsModel.Weapon, ShopID = "000000000", StorageID = "C0007" }
            };

        /// <summary>
        /// 4 = Very Bad, Bad = 3, Average = 2, Good = 1, Very Good = 0
        /// </summary>
        public byte Honor
        {
            get
            {
                if (TotalBattles == 0 && Deserion == 0 && TeamKills == 0) return 2;
                return (byte)(TotalKills == 0 ? (byte)System.Math.Min(TotalDeaths, 4) : System.Math.Min(TotalDeaths / TotalKills, 4));
            }
        }

        public uint TotalBattles
        {
            get
            {
                return (uint)Battles.Count;
            }
        }

        public uint Wins
        {
            get
            {
                return (uint)Battles.Where(i => i.Won).Count();
            }
        }

        public uint Loses
        {
            get
            {
                return (uint)Battles.Where(i => !i.Won).Count();
            }
        }

        public uint TotalDeaths
        {
            get
            {
                return (uint)Battles.Sum(i => i.Deaths);
            }
        }

        public uint TotalKills
        {
            get
            {
                return (uint)Battles.Sum(i => i.Kills);
            }
        }

        public ushort Rank
        {
            get
            {
                return (ushort)RankExperiences.Where(i => i.Value >= Experience).FirstOrDefault().Key;
            }
        }

        public ushort Channel;

        public byte EXPPercent
        {
            get
            {
                if (Rank == (ushort)Enums.Grades.Marshall) return 100;
                return (byte)((double)Experience / (double)RankExperiences[NextRank] * 100d);
            }
        }

        public uint NeedEXP
        {
            get
            {
                if (Rank == (ushort)Enums.Grades.Marshall)
                    return RankExperiences[(Enums.Grades)(Rank)];
                return RankExperiences[(Enums.Grades)(Rank + 1)];
            }
        }

        public Enums.Grades NextRank
        {
            get
            {
                if (Rank == (ushort)Enums.Grades.Marshall)
                    return (Enums.Grades)Rank;
                return (Enums.Grades)(Rank + 1);
            }
        }

        public static Dictionary<Enums.Grades, uint> RankExperiences = new Dictionary<Enums.Grades, uint>(100)
         {
            { Enums.Grades.Trainee_1, 0},
            { Enums.Grades.Trainee_2, 456 },
            { Enums.Grades.Private, 912 },
            { Enums.Grades.PFC, 1824 },
            { Enums.Grades.Corporal, 3192 },
            { Enums.Grades.Sergeant_1, 5016 },
            { Enums.Grades.Sergeant_2, 7296 },
            { Enums.Grades.Sergeant_3, 10032 },
            { Enums.Grades.Sergeant_4, 13224 },
            //Gradesy Officers: count, 18
            { Enums.Grades.Staff_Sergeant_1, 17784 },
            { Enums.Grades.Staff_Sergeant_2, 23940 },
            { Enums.Grades.Staff_Sergeant_3, 33060 },
            { Enums.Grades.Staff_Sergeant_4, 43092 },
            { Enums.Grades.Staff_Sergeant_5, 54036 },
            { Enums.Grades.Staff_Sergeant_6, 65892 },
            { Enums.Grades.SFC_1, 78660 },
            { Enums.Grades.SFC_2, 92340 },
            { Enums.Grades.SFC_3, 106932 },
            { Enums.Grades.SFC_4, 122436 },
            { Enums.Grades.SFC_5, 138852 },
            { Enums.Grades.SFC_6, 156180 },
            { Enums.Grades.Master_Sergeant_1, 174420 },
            { Enums.Grades.Master_Sergeant_2, 193572 },
            { Enums.Grades.Master_Sergeant_3, 213636 },
            { Enums.Grades.Master_Sergeant_4, 234612 },
            { Enums.Grades.Master_Sergeant_5, 256500 },
            { Enums.Grades.Master_Sergeant_6, 279300 },
            //Enums.Gradesany Officers: count, 25;
            { Enums.Grades.Second_Lieutenant_1, 326724 },
            { Enums.Grades.Second_Lieutenant_2, 375972 },
            { Enums.Grades.Second_Lieutenant_3, 427044 },
            { Enums.Grades.Second_Lieutenant_4, 479940 },
            { Enums.Grades.Second_Lieutenant_5, 534660 },
            { Enums.Grades.Second_Lieutenant_6, 591204 },
            { Enums.Grades.Second_Lieutenant_7, 649572 },
            { Enums.Grades.Second_Lieutenant_8, 709764 },
            { Enums.Grades.First_Lieutenant_1, 771780 },
            { Enums.Grades.First_Lieutenant_2, 835620 },
            { Enums.Grades.First_Lieutenant_3, 901284 },
            { Enums.Grades.First_Lieutenant_4, 968772 },
            { Enums.Grades.First_Lieutenant_5, 1038084 },
            { Enums.Grades.First_Lieutenant_6, 1109220 },
            { Enums.Grades.First_Lieutenant_7, 1182180 },
            { Enums.Grades.First_Lieutenant_8, 1256964 },
            { Enums.Grades.Captain_1, 1333572 },
            { Enums.Grades.Captain_2, 1412004 },
            { Enums.Grades.Captain_3, 1492260 },
            { Enums.Grades.Captain_4, 1574340 },
            { Enums.Grades.Captain_5, 1658244 },
            { Enums.Grades.Captain_6, 1743972 },
            { Enums.Grades.Captain_7, 1831524 },
            { Enums.Grades.Captain_8, 1920900 },
             //Gradesld Officers: count, 23;
            { Enums.Grades.Major_1, 2057700 },
            { Enums.Grades.Major_2, 2107236 },
            { Enums.Grades.Major_3, 2339508 },
            { Enums.Grades.Major_4, 2484516 },
            { Enums.Grades.Major_5, 2632260 },
            { Enums.Grades.Major_6, 2782740 },
            { Enums.Grades.Major_7, 2935956 },
            { Enums.Grades.Major_8, 3091908 },
            { Enums.Grades.Lieutenant_Colonel_1, 3277044 },
            { Enums.Grades.Lieutenant_Colonel_2, 3465372 },
            { Enums.Grades.Lieutenant_Colonel_3, 3673536 },
            { Enums.Grades.Lieutenant_Colonel_4, 3885177 },
            { Enums.Grades.Lieutenant_Colonel_5, 4100295 },
            { Enums.Grades.Lieutenant_Colonel_6, 4318890 },
            { Enums.Grades.Lieutenant_Colonel_7, 4540962 },
            { Enums.Grades.Lieutenant_Colonel_8, 4766511 },
            { Enums.Grades.Colonel_1, 5028198 },
            { Enums.Grades.Colonel_2, 5319183 },
            { Enums.Grades.Colonel_3, 5614500 },
            { Enums.Grades.Colonel_4, 5914149 },
            { Enums.Grades.Colonel_5, 6218130 },
            { Enums.Grades.Colonel_6, 6526500 },
            { Enums.Grades.Colonel_7, 6839202 },
            { Enums.Grades.Colonel_8, 7156236 },
             //Gradeserals:count, 25;
            { Enums.Grades.Brigadier_General_1, 7578036 },
            { Enums.Grades.Brigadier_General_2, 8026911 },
            { Enums.Grades.Brigadier_General_3, 8481771 },
            { Enums.Grades.Brigadier_General_4, 8964561 },
            { Enums.Grades.Brigadier_General_5, 9475851 },
            { Enums.Grades.Brigadier_General_6, 10016211 },
            { Enums.Grades.Major_General_1, 10586211 },
            { Enums.Grades.Major_General_2, 11186421 },
            { Enums.Grades.Major_General_3, 11817411 },
            { Enums.Grades.Major_General_4, 12479751 },
            { Enums.Grades.Major_General_5, 13174011 },
            { Enums.Grades.Major_General_6, 13900761 },
            { Enums.Grades.Lieutenant_General_1, 14660571 },
            { Enums.Grades.Lieutenant_General_2, 15454011 },
            { Enums.Grades.Lieutenant_General_3, 16281651 },
            { Enums.Grades.Lieutenant_General_4, 17144061 },
            { Enums.Grades.Lieutenant_General_5, 18041811 },
            { Enums.Grades.Lieutenant_General_6, 18975471 },
            { Enums.Grades.General_1, 19945611 },
            { Enums.Grades.General_2, 20952801 },
            { Enums.Grades.General_3, 21997611 },
            { Enums.Grades.General_4, 23080611 },
            { Enums.Grades.General_5, 24202371 },
            { Enums.Grades.General_6, 25363461 },
            { Enums.Grades.Marshall, 26564451 },
         };
    }
}
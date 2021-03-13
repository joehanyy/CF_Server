using System;
using System.Collections.Generic;
using System.Text;

namespace CF_Server
{
    public class Item
    {
        public Item() { }
        public uint RepairPrice;
        private DateTime ExpireTime;
        public ushort ExpireDays
        {
            get
            {
                if (DateTime.Now > ExpireTime) return 0;
                TimeSpan span = DateTime.Now - ExpireTime;
                return (ushort)span.Days;
            }
        }
        public ushort ExpireHours
        {
            get
            {
                if (DateTime.Now > ExpireTime) return 0;
                TimeSpan span = DateTime.Now - ExpireTime;
                return (ushort)span.Hours;
            }
        }
        public Enums.ItemsModel Type;
        public string ShopID, StorageID;
        public uint Health = 100;
    }
}
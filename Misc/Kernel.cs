using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CF_Server
{
    public class Kernel
    {
        public static System.Collections.Concurrent.ConcurrentDictionary<string, Database.AccountTable> AwaitingPool = new System.Collections.Concurrent.ConcurrentDictionary<string, Database.AccountTable>();
        public static System.Collections.Concurrent.ConcurrentDictionary<uint, Client.GameClient> GamePool = new System.Collections.Concurrent.ConcurrentDictionary<uint, Client.GameClient>();
        public static System.Collections.Generic.List<Room> Rooms = new System.Collections.Generic.List<Room>();
        public static FastRandom Random = new FastRandom();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s.ToLower()[Random.Next(s.Length)]).ToArray());
        }
    }
}

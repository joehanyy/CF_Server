using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CF_Server
{
    public class Constants
    {
        public const string ServerAnnounce = "Welcome to our CF Private Server!";
        public const byte packetStartsWith = 0xF1, packetEndsWith = 0xF2;
        public const byte timeToAutomaticallyEnterServer = 30;//In Seconds
        public const ushort ChannelsCount = 10, ChannelsCapacity = 300, RoomsPerChannel = 50;
        public const byte delayBetweenShowAnnounce = 10;//In Seconds
        public const uint DefaultGP = 200000;//The GP That the account will create with
    }
}

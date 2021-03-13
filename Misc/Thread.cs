using CF_Server.Network.Sockets;
using System;
using System.Threading;
using System.Threading.Generic;

namespace CF_Server
{
    public class Thread
    {
        public static StaticPool GenericThreadPool;
        public static StaticPool ReceivePool, SendPool;  
        public TimerRule<ClientWrapper> ConnectionReceive, ConnectionSend;
        public TimerRule<Client.GameClient> Characters;
        public Thread()
        {
            GenericThreadPool = new StaticPool(32).Run();
            ReceivePool = new StaticPool(32).Run();
            SendPool = new StaticPool(32).Run();
        }
        public void Init()
        {
            Characters = new TimerRule<Client.GameClient>(CharactersCallback, 1000, ThreadPriority.BelowNormal);
            ConnectionReceive = new TimerRule<ClientWrapper>(connectionReceive, 1);
            ConnectionSend = new TimerRule<ClientWrapper>(connectionSend, 1);
        }
        public static bool Valid(Client.GameClient client)
        {
            if (!client.Socket.Alive || client.Entity == null || client == null)
            {
                client.Disconnect();
                return false;
            }
            return true;
        }
        private void connectionReceive(ClientWrapper wrapper, int time)
        {
            ClientWrapper.TryReceive(wrapper);
        }
        private void connectionSend(ClientWrapper wrapper, int time)
        {
            ClientWrapper.TrySend(wrapper);
        }
        public bool Register(Client.GameClient client)
        {
            if (client.TimerSubscriptions == null)
            {
                client.TimerSyncRoot = new object();
                client.TimerSubscriptions = new IDisposable[]
                {
                    Subscribe(Characters, client)
                };
                return true;
            }
            return false;
        }
        public void Unregister(Client.GameClient client)
        {
            if (client.TimerSubscriptions == null) return;
            lock (client.TimerSyncRoot)
            {
                if (client.TimerSubscriptions != null)
                {
                    foreach (var timer in client.TimerSubscriptions)
                        timer.Dispose();
                    client.TimerSubscriptions = null;
                }
            }
        }
        private void CharactersCallback(Client.GameClient client, int time)
        {
            if (!Valid(client)) return;
            Time32 Now = new Time32(time);
            #region Announce
            if (Now >= client.Entity.Announce.AddSeconds(Constants.delayBetweenShowAnnounce) && client.Account.Server != null)
            {
                Game.PacketHandler.Announce(client, Constants.ServerAnnounce);
                client.Entity.Announce = Time32.Now;
            }
            #endregion
            #region Fever
            if (Now >= client.Entity.FeverTime.AddMinutes(1) && client.Account.Server != null)
            {
                //If the player is playing the Fever will increase 3% every 1 min but if he only logged and on the lobby it will increase 1% every 1 min
                //I'm leaving it 1% 'till i code the PVP
                byte oldVal = client.Entity.FPercent;
                client.Entity.FPercent++;
                client.Entity.FeverTime = Time32.Now;
                if (oldVal < 33 && client.Entity.FPercent >= 33)
                {
                    client.Entity.MP += 10;
                    Game.PacketHandler.SendFeverReward(client, 10, 1);
                }
                if (oldVal < 66 && client.Entity.FPercent >= 66)
                {
                    client.Entity.MP += 10;
                    Game.PacketHandler.SendFeverReward(client, 10, 2);
                }
            }
            #endregion
        }
        public static System.IDisposable Subscribe<T>(TimerRule<T> rule, T param)
        {
            return GenericThreadPool.Subscribe<T>(rule, param);
        }
        public static IDisposable Subscribe<T>(TimerRule<T> rule, T param, StaticPool pool)
        {
            return pool.Subscribe<T>(rule, param);
        }
    }
}

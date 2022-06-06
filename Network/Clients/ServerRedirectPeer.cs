using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public class ServerRedirectPeer : IPeer
    {
        private Server server;
        private int id;
        private int priority;

        public ServerRedirectPeer(Server server, int id, int priority)
        {
            this.server = server;
            this.id = id;
            this.priority = priority;
        }

        public void Connect()
        {
            //throw new System.NotImplementedException();
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            server.OnPeerDisconnected(this, reason);
            //throw new System.NotImplementedException();
        }

        public bool EqualsAddress(IPeer otherPeer)
        {
            return false;
        }

        public IPeerDescriptor GetDescriptor()
        {
            throw new System.NotImplementedException();
        }

        public int GetId()
        {
            return id;
        }

        public float GetLag()
        {
            //should return cumulative server->client1 + server->client2 lag
            throw new System.NotImplementedException();
        }

        public int GetPriority()
        {
            return priority;
        }

        public IPeer NewInstance(IPeerDescriptor descriptor)
        {
            throw new System.NotImplementedException();
        }

        public void Send(INetworkMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SetLag(float lag)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateClientParams(int id, int priority)
        {
            throw new System.NotImplementedException();
        }
    }
}
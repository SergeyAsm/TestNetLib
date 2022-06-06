using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public class ReplaceablePeer : IPeer
    {
        public IPeer innerPeer;

        public ReplaceablePeer(IPeer innerPeer)
        {
            this.innerPeer = innerPeer;
        }

        public void Connect()
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            throw new System.NotImplementedException();
        }

        public bool EqualsAddress(IPeer otherPeer)
        {
            return innerPeer.EqualsAddress(otherPeer);
        }

        public IPeerDescriptor GetDescriptor()
        {
            throw new System.NotImplementedException();
        }

        public int GetId()
        {
            return innerPeer.GetId();
        }

        public float GetLag()
        {
            return innerPeer.GetLag();
        }

        public int GetPriority()
        {
            return innerPeer.GetPriority();
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
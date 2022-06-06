using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IPeer
    {
        void Send(INetworkMessage message);
        int GetId();
        int GetPriority();
        void Connect();
        void UpdateClientParams(int id, int priority);
        void Disconnect(DisconnectReason reason = DisconnectReason.UNKNOWN_REASON);
        void SetLag(float lag);
        float GetLag();
        IPeerDescriptor GetDescriptor();
        bool EqualsAddress(IPeer otherPeer);
    }
}
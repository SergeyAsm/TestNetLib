using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public class TestRemotePeer : BaseRemotePeer,IPeer
    {
        public int remoteAddress;
        private NetworkInterfaces.TestNetworkInterface server;

        public TestRemotePeer(Network.Server mainServer, TestRemotePeerDescriptor descriptor)
        {
            this.server = mainServer.GetNetworkInterface<NetworkInterfaces.TestNetworkInterface>() as NetworkInterfaces.TestNetworkInterface;
            this.remoteAddress = descriptor.remoteAddress;
            this.id = descriptor.id;
            this.priority = descriptor.priority;
        }

        public TestRemotePeer(NetworkInterfaces.TestNetworkInterface server, int remoteAddress)
        {
            this.server = server;
            this.remoteAddress = remoteAddress;
            this.id = 1;
            this.priority = int.MaxValue;
        }
        public void Connect()
        {
            server.Connect(this);
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            server.Disconnect(this,reason);
        }

        public IPeerDescriptor GetDescriptor()
        {
            return new TestRemotePeerDescriptor(id, priority, remoteAddress);
        }

        public void Send(INetworkMessage message)
        {
            server.Send(remoteAddress, message);
        }

        public bool EqualsAddress(IPeer otherPeer)
        {
            if (otherPeer is TestRemotePeer p)
            {
                return p.remoteAddress == remoteAddress;
            }
            return false;
        }
        public int GetSelfAddress()
        {
            return server.address;
        }
    }
}
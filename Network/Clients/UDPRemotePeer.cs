using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Network.Clients
{
    public class UDPRemotePeer : BaseRemotePeer, IPeer
    {

        public IPEndPoint remoteAddress;
        private NetworkInterfaces.UdpNetworkInterface server;
        internal NetPeer innerPeer;

        public UDPRemotePeer(Network.Server mainServer, IPEndPoint remoteAddress)
        {
            this.server = mainServer.GetNetworkInterface<NetworkInterfaces.UdpNetworkInterface>();
            this.remoteAddress = remoteAddress;
            this.id = 1;
            this.priority = int.MaxValue;
        }

        public UDPRemotePeer(Network.Server mainServer, UDPRemotePeerDescriptor descriptor)
        {
            this.server = mainServer.GetNetworkInterface<NetworkInterfaces.UdpNetworkInterface>();
            this.remoteAddress = descriptor.remoteAddress;
            this.id = descriptor.id;
            this.priority = descriptor.priority;
        }

        public UDPRemotePeer(NetworkInterfaces.UdpNetworkInterface server, NetPeer innerPeer)
        {
            this.server = server;
            this.remoteAddress = innerPeer.EndPoint;
            this.id = 1;
            this.priority = int.MaxValue;
            this.innerPeer = innerPeer;
        }
        internal void SetInnerPeer(NetPeer peer)
        {
            innerPeer = peer;
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
            return new UDPRemotePeerDescriptor(id, priority, remoteAddress);
        }

        public void Send(INetworkMessage message)
        {
            server.OnOutcomingData(this, message);
        }

        public bool EqualsAddress(IPeer otherPeer)
        {
            if (otherPeer is UDPRemotePeer p)
            {
                return p.remoteAddress == remoteAddress;
            }
            return false;
        }
        public override string ToString()
        {
            return remoteAddress.ToString();
        }
    }
}
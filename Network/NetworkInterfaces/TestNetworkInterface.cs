using Network.Clients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Network.NetworkInterfaces
{
    public class TestNetworkInterface : BaseNetworkInterface,INetworkInterface
    {
        private Server server;
        public int address;

        private Dictionary<int, TestRemotePeer> peers = new Dictionary<int, TestRemotePeer>();

        public TestNetworkInterface(Server server)
        {
            this.server = server;
        }


        public void Start()
        {
            address = TestLocalConnector.AddServer(this);
        }

        public void Stop()
        {
            TestLocalConnector.RemoveServer(address);
        }

        public void Send(int remoteAddress, INetworkMessage mess)
        {
            TestLocalConnector.GetServer(remoteAddress).IncomingMessage(address, mess);
        }

        public bool IncomingConnection(int address)
        {
            var newPeer = new TestRemotePeer(this, address);
            if (InvokeOnPeerConnect(newPeer))
            {
                peers.Add(address, newPeer);
                return true;
            }
            return false;
        }
        public bool OutcomingConnection(TestRemotePeer peer)
        {
            return InvokeOnPeerConnect(peer);
        }
        public void OutcomingDisonnection(TestRemotePeer peer, DisconnectReason reason)
        {
            InvokeOnPeerDisconnect(peer,reason);
        }
        public void IncomingDisconnection(int address, DisconnectReason reason)
        {
            if (peers.TryGetValue(address,out var peer))
            {
                peers.Remove(address);
                InvokeOnPeerDisconnect(peer,reason);
            }
        }
        public void IncomingMessage(int address,INetworkMessage message)
        {
            if (peers.TryGetValue(address, out var peer))
            {
                InvokeOnRecieveMessage(peer,message);
            }
        }

        public void Connect(TestRemotePeer testRemotePeer) 
        {
            peers.Add(testRemotePeer.remoteAddress, testRemotePeer);
            if (TestLocalConnector.GetServer(testRemotePeer.remoteAddress).IncomingConnection(testRemotePeer.GetSelfAddress()))
            {
                if (!OutcomingConnection(testRemotePeer)) 
                { 
                    TestLocalConnector.GetServer(testRemotePeer.remoteAddress).IncomingDisconnection(testRemotePeer.GetSelfAddress(), DisconnectReason.ONE_SIDE_CONNECTION);
                    peers.Remove(testRemotePeer.remoteAddress);
                }
            }
            else
            {
                peers.Remove(testRemotePeer.remoteAddress);
            }
        }
        public void Disconnect(TestRemotePeer testRemotePeer, DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            peers.Remove(testRemotePeer.remoteAddress);
            TestLocalConnector.GetServer(testRemotePeer.remoteAddress).IncomingDisconnection(testRemotePeer.GetSelfAddress(),reason);
            OutcomingDisonnection(testRemotePeer,reason);
        }

        public void Update()
        {
          //  throw new NotImplementedException();
        }
    }
}
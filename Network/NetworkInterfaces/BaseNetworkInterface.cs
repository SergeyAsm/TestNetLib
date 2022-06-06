using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.NetworkInterfaces
{
    public abstract class BaseNetworkInterface 
    {
        private event Action<IPeer, INetworkMessage> onRecieveMessage;
        private event Server.OnTryConnection onClientConnected;
        private event Action<IPeer, DisconnectReason> onClientDisconnected;
        //-------------------------------------------------------------------
        public void OnPeerConnected(Server.OnTryConnection onClientConnected)
        {
            this.onClientConnected += onClientConnected;
        }

        public void OnPeerDisconnected(Action<IPeer, DisconnectReason> onClientDisconnected)
        {
            this.onClientDisconnected += onClientDisconnected;
        }

        public void OnRecieveMessage(Action<IPeer, INetworkMessage> onRecieveMessage)
        {
            this.onRecieveMessage += onRecieveMessage;
        }

        //-----------------------------------------------------------------
        protected bool InvokeOnPeerConnect(IPeer remoteClient)
        {
            return onClientConnected.Invoke(remoteClient);
        }
        protected void InvokeOnPeerDisconnect(IPeer remoteClient, DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            onClientDisconnected?.Invoke(remoteClient, reason);
        }
        protected void InvokeOnRecieveMessage(IPeer peer, INetworkMessage message)
        {
            onRecieveMessage?.Invoke(peer, message);
        }

    }
}
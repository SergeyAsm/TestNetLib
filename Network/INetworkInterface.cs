using Network.Clients;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface INetworkInterface
    {
        void Start();
        void Stop();

        void Update();

        //void Send(IMessage mess);
        //events subscriptions
        //--------------------------------------------------
        void OnPeerConnected(Server.OnTryConnection onTryPeerConnection);
        void OnPeerDisconnected(Action<IPeer, DisconnectReason> onClientDisconnected);
        void OnRecieveMessage(Action<IPeer, INetworkMessage> onRecieveMessage);
        //--------------------------------------------------
    }
}
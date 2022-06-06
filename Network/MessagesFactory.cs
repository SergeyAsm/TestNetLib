using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class MessagesFactory:Utils.GenericFactory<INetworkMessage>
    {
        public static readonly MessagesFactory Instance = new MessagesFactory();

        private MessagesFactory()
        {
            AddInstantiableType<Messages.AddPeer>();
            AddInstantiableType<Messages.ClientData>();
            AddInstantiableType<Messages.RemovePeer>();
            AddInstantiableType<Messages.ServerData>();
            AddInstantiableType<Messages.TestPeerMessage>();
        }
    }
}
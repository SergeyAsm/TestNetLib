using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IClientMessage : INetworkMessage, ISendedMessage
    {
        bool IsPeerMessage();
    }
}
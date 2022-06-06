using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IServerMessage : INetworkMessage, ISendedMessage
    {

    }
}
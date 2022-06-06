using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public abstract class Message : INetworkMessage
    {
        public abstract void Deserialize(NetDataReader reader);
        public abstract void Serialize(NetDataWriter writer);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IPeerMessage :INetworkMessage
    {
        //unique incremental ID
        long GetMessageId();
        float GetTimestamp();
        int GetOriginalSenderId();
    }
}
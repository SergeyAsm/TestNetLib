using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    //for automation purpose
    public interface IPeerUpdatableMessage : IPeerMessage
    {
        void SetOriginalSenderId(int senderId);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    
    public interface ISendedMessage 
    {
        //current sender of message
        void SetSenderId(int senderId);
        int GetSenderId();
    }
}
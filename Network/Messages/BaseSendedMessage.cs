using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public abstract class BaseSendedMessage : DataObject.DataSources.GenericDataSource, ISendedMessage
    {
        private int senderId;
        public int GetSenderId()
        {
            return senderId;
        }

        public void SetSenderId(int senderId)
        {
            this.senderId = senderId;
        }
    }
}
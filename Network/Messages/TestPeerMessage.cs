using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public class TestPeerMessage : IPeerUpdatableMessage
    {
        private static long curMessId=1;

        private long messageId;
        private int senderId=-1;
        private float timestamp;
        public string data;

        public TestPeerMessage()
        {
        }

        public TestPeerMessage( string data)
        {
            this.messageId = curMessId++;
//            this.senderId = senderId;
            this.timestamp = Time.time;
            this.data = data;
        }

        public long GetMessageId()
        {
            return messageId;
        }

        public int GetSenderId()
        {
            return senderId;
        }

        public float GetTimestamp()
        {
            return timestamp;
        }

        public void SetSenderId(int senderId)
        {
            this.senderId = senderId;
        }

        public void SetOriginalSenderId(int senderId)
        {
            throw new System.NotImplementedException();
        }

        public int GetOriginalSenderId()
        {
            throw new System.NotImplementedException();
        }
    }
}
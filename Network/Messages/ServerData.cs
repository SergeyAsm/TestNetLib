using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public class ServerData : BaseSendedMessage, IClientMessage
    {
        //[DataObject.DataObjectField]
        //public float timestamp;
        [DataObject.DataObjectField]
        public string version;

        public ServerData()
        {
        }

        public ServerData(string version)
        {
            this.version = version;
            //timestamp = Time.time;
        }

        public bool IsPeerMessage()
        {
            return false;
        }
    }
}
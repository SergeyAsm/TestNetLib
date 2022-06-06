using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public class RemovePeer : BaseSendedMessage, IServerMessage
    {
        [DataObject.DataObjectField]
        public int id;

        public RemovePeer()
        {
        }

        public RemovePeer(int id)
        {
            this.id = id;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public class ClientData : BaseSendedMessage, IServerMessage
    {
        [DataObject.DataObjectField]
        public int id;
        [DataObject.DataObjectField]
        public int priority;
        //[DataObject.DataObjectField]
        //public float timestamp;
        [DataObject.DataObjectField]
        public string version;
        [DataObject.DataObjectField]
        public bool ptpEnabled;
        [DataObject.DataObjectField]
        public List<IPeerDescriptor> peers;

        public ClientData()
        {
            peers = new List<IPeerDescriptor>();
        }

        public ClientData(int id, int priority,string version,bool ptpEnabled, Dictionary<int, IPeerDescriptor> peers): this()
        {
            this.id = id;
            this.priority = priority;
            this.version = version;
            this.ptpEnabled = ptpEnabled;
            SetPeers(peers);


            //timestamp = Time.time;
        }


        private void SetPeers(Dictionary<int, IPeerDescriptor> peersd)
        {
            foreach (var item in peersd)
            {
                peers.Add(item.Value);
            }
        }

    }
}
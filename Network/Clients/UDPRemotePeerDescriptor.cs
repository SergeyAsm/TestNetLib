using DataObject.DataSources;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Network.Clients
{
    public class UDPRemotePeerDescriptor : GenericDataSource,IPeerDescriptor
    {
        [DataObject.DataObjectField]
        public int id;
        [DataObject.DataObjectField]
        public int priority;
        [DataObject.DataObjectField]
        public IPEndPoint remoteAddress;

        public UDPRemotePeerDescriptor()
        {
        }

        public UDPRemotePeerDescriptor(int id, int priority, IPEndPoint remoteAddress)
        {
            this.id = id;
            this.priority = priority;
            this.remoteAddress = remoteAddress;
        }

        public int GetId()
        {
            return id;
        }

        public int GetPriority()
        {
            return priority;
        }

        public IPeer Instantiate(Server server)
        {
            return new UDPRemotePeer(server, this);
        }
    }
}
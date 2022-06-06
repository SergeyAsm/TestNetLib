using DataObject.DataSources;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public class TestRemotePeerDescriptor : GenericDataSource,IPeerDescriptor
    {
        [DataObject.DataObjectField]
        public int id;
        [DataObject.DataObjectField]
        public int priority;
        [DataObject.DataObjectField]
        public int remoteAddress;

        public TestRemotePeerDescriptor()
        {
        }

        public TestRemotePeerDescriptor(int id, int priority, int remoteAddress)
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
            return new TestRemotePeer(server, this);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.id = reader.GetInt();
            this.priority = reader.GetInt();
            this.remoteAddress = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(priority);
            writer.Put(remoteAddress);
        }
    }
}
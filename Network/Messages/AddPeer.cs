using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public class AddPeer : BaseSendedMessage, IServerMessage
    {
        [DataObject.DataObjectField]
        public IPeerDescriptor descriptor;

        public AddPeer()
        {
        }
        public AddPeer(IPeerDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }



        /*
        public void Deserialize(NetDataReader reader)
        {
            descriptor = PeerDescriptorFactory.Instance.Instantiate(reader.GetInt()) as IPeerDescriptor;
            descriptor.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PeerDescriptorFactory.Instance.GetTypeId(descriptor));
            descriptor.Serialize(writer);
        }
        */
    }
}
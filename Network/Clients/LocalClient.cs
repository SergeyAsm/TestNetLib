#define LOCAL_CLIENT_MESSAGES_DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public class LocalClient : IPeer
    {
        public int Priority;

        private int id;
        private Server server;

        public LocalClient(Server server)
        {
            this.server = server;
            id = 1;
            Priority = int.MaxValue;
        }

        public LocalClient(Server server,int id,int priority)
        {
            this.server = server;
            this.id = id;
            Priority = priority;
        }

        public void Connect()
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            server.OnPeerDisconnected(this, reason);
            //throw new System.NotImplementedException();
        }

        public bool EqualsAddress(IPeer otherPeer)
        {
            return false;
        }

        public IPeerDescriptor GetDescriptor()
        {
            throw new System.NotImplementedException();
        }

        public int GetId()
        {
            return id;
        }

        public float GetLag()
        {
            throw new System.NotImplementedException();
        }

        public int GetPriority()
        {
            return Priority;
        }

        public IPeer NewInstance(IPeerDescriptor descriptor)
        {
            throw new System.NotImplementedException();
        }

        public void Send(INetworkMessage message)
        {
#if LOCAL_CLIENT_MESSAGES_DEBUG
            if (message is DataObject.IDataSource dataSource)
            {
                var type = MessagesFactory.Instance.GetTypeId(message);

                var data = dataSource.SaveData();
                var dataBuffer = data.GetData();


                var Rmessage = MessagesFactory.Instance.Instantiate(type);
                if (Rmessage is DataObject.IDataSource RdataSource)
                {
                    var Rdata = new DataObject.DataObjects.GenericDataObject(dataBuffer);
                    RdataSource.LoadData(Rdata);
                    server.OnRecieveMessage(this, Rmessage);
                }
                else
                {
                    Debug.LogError("Cannot recieve message " + Rmessage.GetType());
                }
            }
            else
            {
                Debug.LogError("Cannot send message " + message.GetType());
            }

#else
            server.OnRecieveMessage(this, message);
#endif

        }

        public void SetLag(float lag)
        {
            throw new System.NotImplementedException();
        }

        public void TimestampMessage()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateClientParams(int id, int priority)
        {
            this.id = id;
            Priority = priority;
        }
    }
}
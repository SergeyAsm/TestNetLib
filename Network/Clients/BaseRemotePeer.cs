using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Clients
{
    public abstract class BaseRemotePeer
    {

        public float lag;
        protected int id;
        protected int priority;

        /*
        public IPeer NewInstance(IPeerDescriptor descriptor)
        {
            return NewInstance(descriptor as TestRemotePeerDescriptor);
        }
        */
        public int GetId()
        {
            return id;
        }

        public float GetLag()
        {
            return lag;
        }

        public int GetPriority()
        {
            return priority;
        }

        public void SetLag(float lag)
        {
            this.lag = lag;
        }

        public void UpdateClientParams(int id, int priority)
        {
            this.id = id;
            this.priority = priority;
        }
    }
}
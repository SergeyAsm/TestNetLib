using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IPeerDescriptor: DataObject.IDataSource
    {
        int GetPriority();
        int GetId();

        IPeer Instantiate(Server server);
    }
}
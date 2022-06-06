using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class TestLocalConnector 
    {
        private static int curServerIndex = 1;
        private static Dictionary<int, NetworkInterfaces.TestNetworkInterface> servers = new Dictionary<int, NetworkInterfaces.TestNetworkInterface>();


        public static int AddServer(NetworkInterfaces.TestNetworkInterface server)
        {
            int address = curServerIndex++;
            servers.Add(address, server);
            return address;
        }
        public static NetworkInterfaces.TestNetworkInterface GetServer(int address)
        {
            if (servers.TryGetValue(address,out var server))
            {
                return server;
            }
            throw new System.Exception("Local server " + address + " not exists!");
        }
        public static void RemoveServer(int address)
        {
            servers.Remove(address);
        }
    }
}
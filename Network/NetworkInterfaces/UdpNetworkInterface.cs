using DataObject;
using DataObject.DataObjects;
using LiteNetLib;
using LiteNetLib.Utils;
using Mono.Nat;
using Network.Clients;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

namespace Network.NetworkInterfaces
{
    public class UdpNetworkInterface : BaseNetworkInterface ,INetworkInterface
    {
        private const int port = 9050;
        private static int curPort;
        public static int PORT { get => curPort==default?port: curPort; set => curPort = value; }

        private const int MAX_CONNECTIONS = 30;

        private Server server;
        private EventBasedNetListener serverEvents;
        private NetManager netManagerServer;

        private ConcurrentDictionary<IPEndPoint, UDPRemotePeer> peers = new ConcurrentDictionary<IPEndPoint, UDPRemotePeer>();

        private NetPacketProcessor packetProcessor = new NetPacketProcessor();

        private static bool UseSeparateThread = true;

        private INatDevice natRouter;

        public UdpNetworkInterface(Server server)
        {
            this.server = server;
            /*
            packetProcessor.RegisterNestedType<Messages.RemovePeer>(
                (w, v) => { w.Put(v.id); },
                (NetDataReader r) => { return new Messages.RemovePeer(r.GetInt()); }
                );
                */
            /*
            packetProcessor.RegisterNestedType<Messages.AddPeer>(
                (w, v) => { w.Put<IPeerDescriptor>(v.descriptor); },
                (NetDataReader r) => { return new Messages.RemovePeer(r.GetInt()); }
                );
                */
            //packetProcessor.RegisterNestedType<Messages.AddPeer>(() => { return new Messages.AddPeer(); });
            //packetProcessor.RegisterNestedType((w, v) => w.Put(v), reader => reader.GetVector2());
        }

        public void Start()
        {

            serverEvents = new EventBasedNetListener();
            serverEvents.ConnectionRequestEvent += OnConnectionRequest;
            serverEvents.PeerConnectedEvent += OnPeerConnected;
            serverEvents.NetworkReceiveEvent += OnIncomingData;
            serverEvents.PeerDisconnectedEvent += OnPeerDisconnected;
            netManagerServer = new NetManager(serverEvents)
            {
                UnsyncedEvents = UseSeparateThread,
                UpdateTime = 20,
//                UpdateTime = 20,
                //IPv6Enabled = IPv6Mode.DualMode,
                //NatPunchEnabled = true
                SimulateLatency = true,
//                SimulationMinLatency = 100,
//                SimulationMaxLatency = 100
                SimulationMinLatency = 220,
                SimulationMaxLatency =240
            };

            netManagerServer.Start(PORT);

            NatUtility.DeviceFound += NatDeviceFound;

            NatUtility.StartDiscovery();

            Debug.Log("UDP server start on port " + PORT);


           // netManagerServer.FirstPeer
        }

        public void Stop()
        {
            TryUnmapPort(PORT, PORT);
            netManagerServer.Stop();
            Debug.Log("UDP server stop on port " + PORT);
        }

        //-------------------------------------------------------------------------------
        internal void Connect(UDPRemotePeer uDPRemotePeer)
        {
            Debug.Log("Try outcoming connection to "+ uDPRemotePeer.remoteAddress);
            if (!peers.TryGetValue(uDPRemotePeer.remoteAddress, out _))
            {
                if (peers.TryAdd(uDPRemotePeer.remoteAddress, uDPRemotePeer))
                {
                    if (!TryGetNetworkPeer(uDPRemotePeer.remoteAddress, out _))
                    {
                        netManagerServer.Connect(uDPRemotePeer.remoteAddress, "SomeConnectionKey");
                        return;
                    }
                }
            }
            Debug.Log("Double or counter connections error");
        }
        private bool TryGetNetworkPeer(IPEndPoint address, out NetPeer peer)
        {
            var serverPeers = netManagerServer.ConnectedPeerList;
            foreach (var sp in serverPeers)
            {
                if (sp.EndPoint == address)
                {
                    peer = sp;
                    return true;
                }
            }
            peer = null;
            return false;
        }

        internal void Disconnect(UDPRemotePeer uDPRemotePeer, DisconnectReason reason)
        {
            uDPRemotePeer.innerPeer.Disconnect();//TODO: transpond disconnect reason
        }

        //-------------------------------------------------------------------------------
        private void OnConnectionRequest(ConnectionRequest request)
        {
            Debug.Log("Connection request from " + request.RemoteEndPoint + " ...");

            if (netManagerServer.ConnectedPeersCount < MAX_CONNECTIONS)
            {
                Debug.Log("... connection request from " + request.RemoteEndPoint + " accepted");
                request.AcceptIfKey("SomeConnectionKey");
            }
            else
            {
                Debug.Log("... connection request from " + request.RemoteEndPoint + " rejected: too many connections");
                request.Reject();
            }
        }
        private void OnPeerConnected(NetPeer networkPeer)
        {
            Debug.Log("Connection " + networkPeer.EndPoint + " ...");

            if (!peers.TryGetValue(networkPeer.EndPoint, out var peer))
            {
                peer = new UDPRemotePeer(this, networkPeer);
                if (peers.TryAdd(networkPeer.EndPoint, peer))
                {
                    InvokeOnPeerConnect(peer);
                    Debug.Log("Connection " + networkPeer.EndPoint + " established!");
                    return;
                }
            }
            else
            {
                peer.innerPeer = networkPeer;
                InvokeOnPeerConnect(peer);
                Debug.Log("Connection " + networkPeer.EndPoint + " established!");
                return;
            }
            networkPeer.Disconnect();
            Debug.Log("Connection " + networkPeer.EndPoint + " fault!");
        }

        private void OnPeerDisconnected(NetPeer networkPeer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("Server disconnected from peer " + networkPeer.EndPoint + " by reason " + disconnectInfo.Reason);
            if (peers.TryRemove(networkPeer.EndPoint, out var peer))
            {
                InvokeOnPeerDisconnect(peer);
            }
        }

        private void OnIncomingData(NetPeer networkPeer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (peers.TryGetValue(networkPeer.EndPoint, out var peer))
            {
                //TODO: some deserialisation work
                var type = reader.GetInt();
                var message = MessagesFactory.Instance.Instantiate(type);
                if (message is IDataSource dataSource)
                {
                    var data = new GenericDataObject(reader.GetRemainingBytes());
                    dataSource.LoadData(data);
                    InvokeOnRecieveMessage(peer, message);
                }
                else
                {
                    Debug.LogError("Cannot recieve message " + message.GetType());
                }
            }
        }
        internal void OnOutcomingData(UDPRemotePeer peer, INetworkMessage message)
        {
            //TODO: some serialisation work
            if (message is IDataSource dataSource)
            {
                var type = MessagesFactory.Instance.GetTypeId(message);

                var data = dataSource.SaveData();
                var writer = new NetDataWriter();
                writer.Put(type);
                writer.Put(data.GetDataTruncated());

                if (writer.Length>1000)
                {
                    Debug.LogWarning("OnOutcomingData warning: writer data size is too big");
                }
                server.statistics.AddOutcoming(writer.Length, message.ToString());

                peer.innerPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                Debug.LogError("Cannot send message " + message.GetType());
            }
        }


        //-----------------------------------------------------------------------
        readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

        private async void NatDeviceFound(object sender, DeviceEventArgs args)
        {
            await locker.WaitAsync();
            try
            {
                if (natRouter == null)
                {
                    natRouter = args.Device;

                    Debug.LogFormat("Device found: {0}", natRouter.NatProtocol);

                    Debug.LogFormat("Type: {0}", natRouter.GetType().Name);

                    var remoteIp = await natRouter.GetExternalIPAsync();
                    Debug.LogFormat("IP: {0}", remoteIp);

                    Debug.LogFormat("---");

                    try
                    {
                        TryMapPort(PORT, PORT);
                    }
                    catch
                    {
                        Debug.LogError("Cant map port "+ PORT +" on nat device "+ natRouter.GetType().Name);
                    }
                    /*
                    */
                    /*
                    bool cantOpenPort = true;
                    int lastPort = 0;
                    try
                    {
                        foreach (var port in EXTERNAL_PORTS)
                        {
                            lastPort = port;
                            Mapping m = await natRouter.GetSpecificMappingAsync(Protocol.Udp, port);
                            Debug.LogFormat("Found exists mapping: protocol={0}, public={1}, private={2}", m.Protocol, m.PublicPort,
                                              m.PrivatePort);
                        }
                    }
                    catch
                    {
                        Debug.LogFormat("Found free external port " + lastPort);
                        cantOpenPort = false;
                        curExternalPort = lastPort;
                        //PlayerPrefs.SetInt(PORT_PREF, curExternalPort);
                        //PlayerPrefs.Save();
                        TryMapPort(PORT, lastPort);

                        Debug.LogFormat("Overwrite external ip={0} by router ip={1}", curExternalIP, remoteIp);

                        curExternalIP = remoteIp;
                    }
                    if (cantOpenPort)
                    {
                        Debug.LogFormat("Couldn't find free port mapping");
                    }
                    */

                    NatUtility.StopDiscovery();
                }
            }
            finally
            {
                locker.Release();
            }
        }

        private void TryMapPort(int internalPort, int externalPort)
        {
            if (natRouter != null)
            {
                var mapping = new Mapping(Protocol.Udp, internalPort, externalPort);
                natRouter.CreatePortMap(mapping);
                Debug.LogFormat("Create Mapping: protocol={0}, public={1}, private={2}", mapping.Protocol, mapping.PublicPort,
                                  mapping.PrivatePort);
            }
        }
        private void TryUnmapPort(int internalPort, int externalPort)
        {
            if (natRouter != null)
            {
                try
                {
                    var mapping = new Mapping(Protocol.Udp, internalPort, externalPort);
                    natRouter.DeletePortMap(mapping);
                    Debug.LogFormat("Deleting Mapping: protocol={0}, public={1}, private={2}", mapping.Protocol, mapping.PublicPort, mapping.PrivatePort);
                }
                catch
                {
                    Debug.LogFormat("Couldn't delete specific mapping public={0}, private={1}", internalPort, externalPort);
                }
            }
        }
        //-----------------------------------------------------------------------

        public void Update()
        {
            if (!UseSeparateThread)
            {
                netManagerServer.PollEvents();
            }
        }

        /*

private void OnConnectionRequest(ConnectionRequest request)
{
    Debug.Log("Connection request from " + request.RemoteEndPoint + " ...");

    int maxWaitTime = 5000;
    int curWaitTime = 0;
    int delay = 50;
    while (curWaitTime <= maxWaitTime)
    {
        if (TryGetConnection(request.RemoteEndPoint, out var connection))
        {
            if (connection.pierced)
            {
                if (IsAlreadyConnected(connection.internalEndpoint) || IsAlreadyConnected(connection.externalEndpoint))
                {
                    Debug.Log("... connection request from " + request.RemoteEndPoint + " rejected - already connected");
                    request.Reject();
                }
                else
                {
                    if (netManagerServer.ConnectedPeersCount < MAX_CONNECTIONS)
                    {
                        Debug.Log("... connection request from " + request.RemoteEndPoint + " accepted");
                        request.AcceptIfKey("SomeConnectionKey");
                    }
                    else
                    {
                        Debug.Log("... connection request from " + request.RemoteEndPoint + " rejected");
                        request.Reject();
                    }
                }
                break;
            }
        }
        curWaitTime += delay;
        Thread.Sleep(delay);
    }
    Debug.Log("... connection request from " + request.RemoteEndPoint + " rejected - connection timeout");
    request.Reject();
}
private void OnPeerConnected(NetPeer peer)
{
    bool isOutcoming = IsOutcoming(peer.EndPoint);
    Debug.Log("Server recieve connection from " + peer.EndPoint);

    var newClient = new RemoteClient(mainClient, null, peer.EndPoint, 0, true);

    newClient.OnPeerDisconnect += OnDisconnect;
    newClient.OnPeerLagging += OnClientLagging;
    mainClient.AddRemoteClient(newClient);

    if (isOutcoming)
    {
        //mainClient.Priority++;
        OnServerConnect?.Invoke(newClient);

    }
    else
    {
        newClient.SetPriority(mainClient.GetPriority() - 1);
        OnClientConnect?.Invoke(newClient);


    }


    newClient.Start();
    TryMapPort(newClient.GetSelfPort(), newClient.GetSelfPort());



    NetDataWriter writer = new NetDataWriter();                 // Create writer class
    writer.Put(1);

    InitMessage mess;
    if (isOutcoming)
    {
        mess = new ServerInitMessage(mainClient.GetClientId(), newClient.GetSelfPort());
    }
    else
    {
        mess = new ClientInitMessage(mainClient.GetClientId(), newClient.GetSelfPort(), mainClient.GetPriority(), mainClient.GetRemoteClients(), peer.EndPoint);

        var peerList = netManagerServer.ConnectedPeerList;
        for (int i = 0; i < peerList.Count; i++)
        {
            if (!peerList[i].EndPoint.Equals(peer.EndPoint))
            {
                NetDataWriter tmpWriter = new NetDataWriter();
                tmpWriter.Put(2);
                tmpWriter.Put(new NatPunchMessage(peer.EndPoint));
                peerList[i].Send(tmpWriter, DeliveryMethod.ReliableUnordered);
            }
        }
    }

    writer.Put(mess);
    peer.Send(writer, DeliveryMethod.ReliableOrdered);

}
private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
{
    Debug.Log("Server disconnected from peer " + peer.EndPoint + " by reason " + disconnectInfo.Reason);
    RemoveConnection(peer.EndPoint);
    //outcoming.Remove(peer.EndPoint);
}
private void OnIncomingData(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
{
    Debug.Log("Server got data from " + peer.EndPoint);

    //           newClient.Connect(new IPEndPoint(peer.EndPoint.Address, newClient.GetSelfPort()));

    int messtype = reader.GetInt();
    if (messtype == 1)
    {

        InitMessage mess;
        if (IsOutcoming(peer.EndPoint))
        {
            mess = reader.Get<ClientInitMessage>();

        }
        else
        {
            mess = reader.Get<ServerInitMessage>();
        }
        mess.OnRecieve(this, mainClient, peer.EndPoint);
    }
    else if (messtype == 2)
    {
        var punchMess = reader.Get<NatPunchMessage>();
        Debug.LogFormat("Success NAT introduction message from {0} , try connect", punchMess.targetEndpoint);
        // throw new NotImplementedException();
        netManagerServer.Connect(punchMess.targetEndpoint, "SomeConnectionKey");
    }

    reader.Recycle();
}
*/
    }
    }
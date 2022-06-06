using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class Server
    {
        //version of PROGRAM, not library!
        
        public string Version = "0.1";

        private List<INetworkInterface> networkInterfaces = new List<INetworkInterface>();

        private ConcurrentDictionary<int, IPeer> peers = new ConcurrentDictionary<int, IPeer>();

        private ConcurrentDictionary<int, IPeerDescriptor> serverPeers = new ConcurrentDictionary<int, IPeerDescriptor>();

        public Clients.LocalClient localClient;

        private IPeer currentServer;
        private IPeer reconnectTarget;

        public delegate bool OnTryConnection(IPeer peer);
        public delegate void OnConnection(IPeer peer);
        public delegate void OnDisconnection(IPeer peer, DisconnectReason reason);

        public delegate void OnServerMessage(IPeer sender, IServerMessage message);
        public delegate void OnClientMessage(IPeer sender, IClientMessage message);
        public delegate void OnPeerMessage(IPeer sender, IPeerMessage message);

        public event OnConnection onConnectToServer;
        public event OnConnection onConnectToClient;
        public event OnConnection onConnectToPeer;

        public event OnDisconnection onDisconnectFromServer;
        public event OnDisconnection onDisconnectFromClient;
        public event OnDisconnection onDisconnectFromPeer;

        public event OnServerMessage onRecieveServerMessage;
        public event OnClientMessage onRecieveClientMessage;
        public event OnPeerMessage onRecievePeerMessage;

        private PeerMessagesFilter messagesFilter = new PeerMessagesFilter();

        private bool IsPTPEnabled = false;
        private bool IsServerPTPEnabled = false;

        //external events
        private ActionsQueue events = new ActionsQueue();

        private int curClientId = 1;

        public NetworkStatistics statistics = new NetworkStatistics(); 
        static Server()
        {
            /*
            AddInstantiableType<Clients.TestRemotePeerDescriptor>();
            AddInstantiableType<Clients.UDPRemotePeerDescriptor>();
            */
            DataObject.DataSourceFactory.Instance.AddInstantiableType<Clients.TestRemotePeerDescriptor>();
            DataObject.DataSourceFactory.Instance.AddInstantiableType<Clients.UDPRemotePeerDescriptor>();
        }

        internal T GetNetworkInterface<T>()
        {
            foreach (var item in networkInterfaces)
            {
                if (item is T itemT)
                {
                    return itemT;
                }
            }
            throw new Exception("Unsupported or inactive network interface " + typeof(T));
        }
        /*
        public delegate void OnLocalClientChangeAction(IPeer localClient);
        public event OnLocalClientChangeAction OnLocalClientChange;
        */

        public void Start()
        {
            networkInterfaces.Add(new NetworkInterfaces.TestNetworkInterface(this));
            networkInterfaces.Add(new NetworkInterfaces.UdpNetworkInterface(this));

            foreach (var item in networkInterfaces)
            {
                item.OnPeerConnected(OnPeerConnected);
                item.OnPeerDisconnected(OnPeerDisconnected);
                item.OnRecieveMessage(OnRecieveMessage);
            }

            foreach (var item in networkInterfaces)
            {
                item.Start();
            }

        }
        public void Stop()
        {
            foreach (var item in networkInterfaces)
            {
                item.Stop();
            }
            networkInterfaces.Clear();
            peers.Clear();
            localClient = null;
            currentServer = null;
            //            OnLocalClientChange?.Invoke(null);
            Debug.Log(statistics);
        }
        public void Update()
        {
            for (int i = 0; i < networkInterfaces.Count; i++)
            {
                networkInterfaces[i].Update();
            }
            CallEvents();
        }

        //-----------------------------------------------------------------
        private void CallEvents()
        {
            while (events.TryDequeue(out Action ev))
            {
                ev.Invoke();
            }
        }


        //-----------------------------------------------------------------

        internal bool OnPeerConnected(IPeer peer)
        {
            //UpdateCurrentServer();
            if (currentServer == peer)
            {
                OnConnectToServer(peer);
                return true;
            }
            else if (localClient == null)
            {
                //our server is not started
                return false;
            }
            else if (IsThisCurrentServer())
            {
                OnConnectToClient(peer);
                return true;
            }
            //if found peer with same address - just replace him, it should be replaceable peer(for now)
            else if (FindEqualAddress(peer,out var existsPeer) && existsPeer is Clients.ReplaceablePeer)
            {
                peer.UpdateClientParams(existsPeer.GetId(), existsPeer.GetPriority());
                peers.TryUpdate(existsPeer.GetId(), peer, existsPeer);
                return true;
            }
            //if peer connected to server but not connected to us
            else if (serverPeers.ContainsKey(peer.GetId()) && !peers.ContainsKey(peer.GetId()))
            {
                OnConnectToPeer(peer);
                return true;
                //
            }
            return false;
        }
        private bool FindEqualAddress(IPeer peer,out IPeer existsPeer)
        {
            foreach (var item in peers)
            {
                if (item.Value.EqualsAddress(peer))
                {
                    existsPeer = item.Value;
                    return true;
                }
            }
            existsPeer = null;
            return false;
        }

        internal void OnPeerDisconnected(IPeer peer, DisconnectReason reason)
        {
            peers.TryRemove(peer.GetId(), out _);
            serverPeers.TryRemove(peer.GetId(), out _);
            if (currentServer == peer)
            {
                OnDisconnectFromServer(peer, reason);
            }
            else if (IsThisCurrentServer())
            {
                OnDisconnectFromClient(peer, reason);
            }
            else 
            {
                OnDisconnectFromPeer(peer, reason);
            }
        }
        private void OnConnectToServer(IPeer peer)
        {
            if (peers.TryAdd(peer.GetId(), peer))
            {
                Debug.Log("OnConnectToServer");

                INetworkMessage mess = new Messages.ServerData(Version);
                peer.Send(mess);
                //                events.Enqueue(() => { onConnectToServer?.Invoke(peer); });
            }
            else
            {
                Debug.LogErrorFormat("Cannot connect to server {0} - peer already added",peer);
            }
        }
        private void OnConnectToPeer(IPeer peer)
        {
            if (peers.TryAdd(peer.GetId(), peer))
            {
                Debug.Log("OnConnectToPeer");
                events.Enqueue(() => { onConnectToPeer?.Invoke(peer); });
            }
            else
            {
                Debug.LogErrorFormat("Cannot connect to peer {0} - peer already added", peer);
            }
        }
        private void OnConnectToClient(IPeer peer)
        {
            Debug.Log("OnConnectToClient");
            //before adding to peers
            peer.UpdateClientParams(GetNextClientId(), GetNextMinPriority());
            if (peers.TryAdd(peer.GetId(), peer))
            {
                if (serverPeers.Count == 0)
                {
                    UpdateServerPeers();
                }
                INetworkMessage mess = new Messages.ClientData(peer.GetId(), peer.GetPriority(), Version, IsPTPEnabled, ExtractServerPeers());
                peer.Send(mess);
                //events.Enqueue(() => { onConnectToClient?.Invoke(peer); });

                UpdateServerPeers();

                foreach (var item in peers)
                {
                    var curPeer = item.Value;
                    if (curPeer != peer && curPeer != localClient)
                    {
                        curPeer.Send(new Messages.AddPeer(peer.GetDescriptor()));
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("Cannot connect to client {0} - peer already added", peer);
            }
            //peer.UpdateClientPeers(peers);//for ptp
        }
        private void OnDisconnectFromServer(IPeer peer,DisconnectReason reason)
        {
            Debug.Log("OnDisconnectFromServer");

            List<IPeer> toDisconnect = new List<IPeer>();
            foreach (var item in peers)
            {
                if (item.Value is Clients.ServerRedirectPeer)
                {
                    toDisconnect.Add(item.Value);
                }
            }
            foreach (var item in toDisconnect)
            {
                item.Disconnect();
            }

            UpdateCurrentServer();
            events.Enqueue(() => { onDisconnectFromServer?.Invoke(peer, reason); });
            if (reconnectTarget != null )
            {
                ConnectToServer(reconnectTarget);
                reconnectTarget = null;
            }

        }
        private void OnDisconnectFromPeer(IPeer peer, DisconnectReason reason)
        {
            Debug.Log("OnDisconnectFromPeer");
            events.Enqueue(() => { onDisconnectFromPeer?.Invoke(peer, reason); });
        }
        private void OnDisconnectFromClient(IPeer peer, DisconnectReason reason)
        {
            Debug.Log("OnDisconnectFromClient");

            foreach (var item in peers)
            {
                var curPeer = item.Value;
                if (curPeer != peer && curPeer != localClient)
                {
                    curPeer.Send(new Messages.RemovePeer(peer.GetId()));
                }
            }

            events.Enqueue(() => { onDisconnectFromClient?.Invoke(peer, reason); });

        }
        //-----------------------------------------------------------------
        public void MakeServer()
        {
            curClientId = 1;
            localClient = new Clients.LocalClient(this);
            currentServer = localClient;
            peers.TryAdd(localClient.GetId(), localClient);
        }

        public void ConnectToServer(IPeer server)
        {
            Debug.Log("ConnectToServer");
            ClearPeers();
            localClient = null;

            currentServer = server;
            server.Connect();
        }
        protected void ConnectToPeer(IPeer peer)
        {
            if (!peers.ContainsKey(peer.GetId()))
            {
                Debug.Log("ConnectToPeer "+peer);
                peer.Connect();
            }
        }
        public void Disconnect(IPeer peer,DisconnectReason reason = DisconnectReason.UNKNOWN_REASON)
        {
            if (peers.ContainsKey(peer.GetId()))
            {
                Debug.Log("Disconnect by reason " + reason);
                peer.Disconnect(reason);
            }
        }
        public void ReconnectToServer()
        {
            if (currentServer != null)
            {
                Debug.LogWarningFormat("ReconnectToServer server={0}", currentServer);
                reconnectTarget = currentServer;
                Disconnect(reconnectTarget);
            }
            else
            {
                Debug.LogErrorFormat("Cant reconnect: server is not assigned!");
            }

        }
        //-----------------------------------------------------------------
        internal bool IsThisCurrentServer()
        {
            return currentServer == localClient;
        }
        internal IPeer GetCurrentServer()
        {
            return currentServer;
        }
        private void UpdateCurrentServer()
        {
            int curPriority = int.MinValue;
            IPeer curClient = localClient;
            foreach (var item in peers)
            {
                if (item.Value.GetPriority()> curPriority)
                {
                    curClient = item.Value;
                }
            }
            currentServer = curClient;
        }
        private int GetNextMinPriority()
        {
            int curPriority = int.MaxValue;
            foreach (var item in peers)
            {
                if (item.Value.GetPriority() < curPriority)
                {
                    curPriority = item.Value.GetPriority();
                }
            }
            return curPriority-1;
        }
        private int GetNextClientId()
        {
            return localClient.GetId()+(curClientId++);
            /*
            int curId = 0;
            foreach (var item in peers)
            {
                if (item.Value.GetId() > curId)
                {
                    curId = item.Value.GetId();
                }
            }
            return curId+1;
            */
        }
        //-----------------------------------------------------------------
        internal void SendMessageToServer(IClientMessage message)
        {
            currentServer.Send(message);
        }
        internal void SendMessageToClient(IPeer client, IServerMessage message)
        {
            client.Send(message);
        }
        internal void SendMessageToClient(int clientId, IServerMessage message)
        {
            if (peers.TryGetValue(clientId,out var client))
            {
                SendMessageToClient(client, message);
            }
        }
        private void SendMessageToPeer(IPeer client, IPeerMessage message)
        {
            /*
            if (IsPTPEnabled || IsThisCurrentServer())
            {
                //server always broadcast self peer messages
                BroadcastPeerMessage(message);
            }
            */
            //casual reciever of message
            //this can be message sender, if we using looping route for lag compensator
            (message as IPeerUpdatableMessage).SetOriginalSenderId(localClient.GetId());
            client.Send(message);
        }
        internal void SendMessageToPeer(IPeerMessage message)
        {
            SendMessageToPeer(localClient, message);
        }
        //strict sender.id == originalSender.id
        internal void BroadcastPeerMessage(IPeerMessage message)
        {
            foreach (var item in peers)
            {
                //dont send message to sender
                if (item.Key!= message.GetOriginalSenderId())
                {
                    item.Value.Send(message);
                }
            }
        }


        //-----------------------------------------------------------------
        internal void OnRecieveMessage(IPeer sender,INetworkMessage message)
        {
            if (message is IPeerMessage)
            {
                //another peer update his data
                OnRecievePeerMessage(sender, message as IPeerMessage);
            }
            else if (sender==currentServer && message is IServerMessage)
            {
                //real server message
                OnRecieveServerMessage(sender, message as IServerMessage);
            }
            else if (IsThisCurrentServer()  && message is IClientMessage)
            {
                //we are server
                OnRecieveClientMessage(sender, message as IClientMessage);
            }
            else
            {
                throw new System.Exception("Wrong message recieved: sender id = "+ sender.GetId()+" message = "+message);
            }
        }
        internal void OnRecieveServerMessage(IPeer sender, IServerMessage message)
        {
            //Debug.Log("OnRecieveServerMessage");
            if (sender != currentServer)
            {
                Debug.LogError("OnRecieveServerMessage error: wrong server message sender!");
                return;
            }
            (message as ISendedMessage).SetSenderId(sender.GetId());

            if (message is Messages.ClientData)
            {
                var mess = message as Messages.ClientData;
                Debug.Log("OnUpdateClientParamsMessage");
                localClient = new Clients.LocalClient(this,mess.id, mess.priority);
                peers.TryAdd(localClient.GetId(), localClient);

                if (Version != mess.version)
                {
                    Disconnect(sender, DisconnectReason.VERSIONS_MISMATCH);
                    return;
                }

                //TODO: insert here proper lag calculation alhoritm
                //sender.SetLag(Time.time - mess.timestamp);
                UpdateServerPeers(mess.peers);
                IsServerPTPEnabled = mess.ptpEnabled;
                foreach (var item in serverPeers)
                {
                    var key = item.Key;
                    if (key != localClient.GetId() && key != currentServer.GetId())
                    {
                        if (IsServerPTPEnabled)
                        {
//                            throw new System.NotImplementedException();
//                            ConnectToPeer(item.Value.Instantiate(this));
                            OnConnectToPeer(new Clients.ReplaceablePeer(item.Value.Instantiate(this)));
                        }
                        else
                        {
                            OnConnectToPeer(new Clients.ServerRedirectPeer(this, key, item.Value.GetPriority()));
                        }
                    }
                }
                events.Enqueue(() => { onConnectToServer?.Invoke(sender); });

            }
            else if (message is Messages.AddPeer)
            {
                Debug.Log("OnAddPeerMessage");
                var mess = message as Messages.AddPeer;
                if (IsServerPTPEnabled)
                {
                    var newPeer = mess.descriptor.Instantiate(this);
                    OnConnectToPeer(new Clients.ReplaceablePeer(newPeer));
                    newPeer.Connect();
                    // ConnectToPeer(mess.descriptor.Instantiate(this));
                    //                    throw new System.NotImplementedException();
                }
                else
                {
                    OnConnectToPeer(new Clients.ServerRedirectPeer(this, mess.descriptor.GetId(), mess.descriptor.GetPriority()));
                    //peers.TryAdd(mess.Id, new Clients.ServerRedirectPeer(this, mess.Id, mess.Priority));
                }
            }
            else if (message is Messages.RemovePeer)
            {
                Debug.Log("OnRemovePeerMessage");
                var mess = message as Messages.RemovePeer;
                if (peers.TryGetValue(mess.id,out var value))
                {
                    value.Disconnect();
                }
//                peers.TryRemove(mess.id,out _);
            }
            else
            {
                //Debug.Log("OnOtherServerMessage");
                events.Enqueue(() => { onRecieveServerMessage?.Invoke(sender, message); });
            }
        }
        internal void OnRecieveClientMessage(IPeer sender, IClientMessage message)
        {
            (message as ISendedMessage).SetSenderId(sender.GetId());

               //Debug.Log("OnRecieveClientMessage");
            if (message is Messages.ServerData)
            {
               Debug.Log("OnRecieveServerDataMessage");
                var mess = message as Messages.ServerData;
                //TODO: insert here proper lag calculation method
                //sender.SetLag(Time.time - mess.timestamp);
                if (Version != mess.version)
                {
                    Disconnect(sender,DisconnectReason.VERSIONS_MISMATCH);
                }
                events.Enqueue(() => { onConnectToClient?.Invoke(sender); });
            }
            else
            {
                //Debug.Log("OnOtherClientMessage");
                events.Enqueue(() => { onRecieveClientMessage?.Invoke(sender, message); });

            }
        }
        internal void OnRecievePeerMessage(IPeer sender, IPeerMessage message)
        {
         //   Debug.Log("OnRecievePeerMessage");
            //just ignore old messages
            if (messagesFilter.IsPassed(message))
            {
           //     Debug.Log("OnRecievePeerMessage - passed");

                //is here original sender
                if (sender.GetId() == message.GetOriginalSenderId()) 
                {
                    //        Debug.Log("OnRecievePeerMessage - from original sender");
                    if ((IsServerPTPEnabled && sender == localClient) || IsThisCurrentServer())
                    {
                        //peer broadcast our source messages if ptp enabled
                        //server always broadcast self peer messages
                        BroadcastPeerMessage(message);
                    }
                    else if (sender == localClient)
                    {
                        //also send message to current server
                        SendMessageToPeer(currentServer, message);
                    }
                    RecievePeerMessageEvent(sender, message);
                }//is here current server
                else if (sender == currentServer)
                {
          //          Debug.Log("OnRecievePeerMessage - from server");
                    RecievePeerMessageEvent(sender, message);
                }
            }
        }
        private void RecievePeerMessageEvent(IPeer sender, IPeerMessage message)
        {
            //can be wrapped by some lag compensator
            events.Enqueue(() => { onRecievePeerMessage?.Invoke(sender, message); });

        }
        //---------------------------------------------------------------
        private void UpdateServerPeers()
        {
            serverPeers.Clear();
            foreach (var item in peers)
            {
                if (!(item.Value is Clients.ServerRedirectPeer) && !(item.Value is Clients.LocalClient))
                {
                    serverPeers.TryAdd(item.Key,item.Value.GetDescriptor());
                }
            }
        }
        private void UpdateServerPeers(List<IPeerDescriptor> newPeers)
        {
            serverPeers.Clear();
            foreach (var item in newPeers)
            {
                serverPeers.TryAdd(item.GetId(), item);
            }
        }
        private Dictionary<int, IPeerDescriptor> ExtractServerPeers()
        {
            Dictionary<int, IPeerDescriptor> result = new Dictionary<int, IPeerDescriptor>(serverPeers.Count);
            foreach (var item in serverPeers)
            {
                    result.Add(item.Key, item.Value);
            }
            return result;
        }

        private void ClearPeers()
        {
            List<IPeer> toDisconnect = new List<IPeer>();
            foreach (var item in peers)
            {
                toDisconnect.Add(item.Value);
            }
            foreach (var item in toDisconnect)
            {
                item.Disconnect();
            }
            //peers.Clear();
            serverPeers.Clear();
        }
        //---------------------------------------------------------------
    }
}
using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeamX
{
    public static class NetworkController
    {
        //Network configuration
        private static NetPeerConfiguration netPeerConfiguration;
        private static NetClient client = null;
        private static ConnectionStatus connectionStatus;
        //Message handlers
        private static InboundMessageHandler inbound;
        private static OutboundMessageHandler outbound;
        //Network Events
        public static Action ConnectedToServer;
        public static Action DisconnectedFromServer;

        //All these events will only be fired when the network is being updated.
        public static Action<List<LevelEditorChange>> LevelEditorChangesEvent;
        public static Action<EditorStateData> ServerDataEvent;
        public static Action<List<PlayerData>> ServerPlayerDataEvent;
        public static Action<PlayerData> PlayerJoinedEvent;
        public static Action<PlayerData> PlayerLeftEvent;
        public static Action<PlayerStateData> PlayerStateEvent;
        public static Action<PlayerTransformData> PlayerTransformEvent;
        public static Action<string> CustomMessageEvent;
        public static Action<List<string>> AlreadyClaimedEvent;

        public static void Initialize()
        {
            //Create the network client
            netPeerConfiguration = new NetPeerConfiguration("Teamkist");
            netPeerConfiguration.ConnectionTimeout = 5000;
            client = new NetClient(netPeerConfiguration);

            //Create the message handlers.
            inbound = new InboundMessageHandler();
            outbound = new OutboundMessageHandler(client);

            client.Start();            
        }

        public static void SubscribeToEvents()
        {
            //When we have connected to the server, we send our login data to the server.
            ConnectedToServer += () =>
            {
                //Log in with our user data and request the server data. Once we have received the data and store it, we will load the level editor scene.
                outbound.LogIn(true);
            };

            //When we enter the level editor, send state change
            GameObserver.EnteredLevelEditor += () => { outbound.SendPlayerState((byte)CharacterMode.Build); };
            //When we go to race mode, send state change
            GameObserver.EnteredGame += () => { outbound.SendPlayerState((byte)CharacterMode.Race); };
            //State change during racing
            GameObserver.LocalStateChange += (stateData) => { outbound.SendPlayerState(stateData.state); };
            //When transform changes send it to the server.
            GameObserver.LocalTransformChange += (transformData) => { outbound.SendPlayerTransformData(transformData); };
            //Send updates about level editor changes.
            EditorObserver.LevelEditorChangesEvent += (changes) => { outbound.SendLevelEditorChanges(changes); };
            //Send messages about selection changes.
            SelectionObserver.BlocksAddedToSelection += (added) => { outbound.SendSelectionClaim(added); };
            //Send messages about unselection chages.
            SelectionObserver.BlocksRemovedFromSelection += (removed) => { outbound.SendSelectionUnclaim(removed); };
        }

        public static StatusCode Update()
        {
            if (connectionStatus == ConnectionStatus.Connecting || connectionStatus == ConnectionStatus.Connected)
            {
                return ReadMessages();
            }
            return StatusCode.InvalidConnectionStatus;
        }

        public static StatusCode ConnectToServer(string ip, int port)
        {
            if (client == null)
            {
                return StatusCode.ClientNull;
            }

            if (connectionStatus == ConnectionStatus.Connecting) 
            {
                return StatusCode.InvalidConnectionStatus;
            }

            if(connectionStatus == ConnectionStatus.Connected)
            {
                return StatusCode.InvalidConnectionStatus;
            }

            try
            {
                
                client.Connect(ip, port);
                connectionStatus = ConnectionStatus.Connecting;
                return StatusCode.Success;
            }
            catch
            {
                return StatusCode.Unexpected;
            }
        }

        public static StatusCode Disconnect()
        {
            if (client == null)
            {
                return StatusCode.ClientNull;
            }

            if(connectionStatus == ConnectionStatus.Disconnecting)
            {
                return StatusCode.InvalidConnectionStatus;
            }

            if (connectionStatus == ConnectionStatus.Disconnected)
            {
                return StatusCode.InvalidConnectionStatus;
            }

            if(connectionStatus == ConnectionStatus.Connecting)
            {
                return StatusCode.InvalidConnectionStatus;
            }

            try
            {
                client.Disconnect("");
                connectionStatus = ConnectionStatus.Disconnecting;
                return StatusCode.Success;
            }
            catch
            {
                return StatusCode.Unexpected;
            }
        }        

        public static StatusCode ReadMessages()
        {
            if (client == null)
            {
                return StatusCode.ClientNull;
            }

            try
            {
                NetIncomingMessage incomingMessage;

                while ((incomingMessage = client.ReadMessage()) != null)
                {
                    switch (incomingMessage.MessageType)
                    {
                        // Handle connection status messages
                        case NetIncomingMessageType.StatusChanged:
                            switch (incomingMessage.SenderConnection.Status)
                            {
                                case NetConnectionStatus.Connected:
                                    OnConnectedToServer();
                                    break;
                                case NetConnectionStatus.Disconnected:
                                    OnDisconnectedFromServer();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            //Get the first byte which is the type of message.
                            NetworkMessageType messageType = (NetworkMessageType)incomingMessage.ReadByte();
                            inbound.HandleInboundMessage(messageType, incomingMessage);
                            break;
                    }
                }

                return StatusCode.Success;
            }
            catch
            {
                return StatusCode.Unexpected;
            }
        }

        private static void OnConnectedToServer()
        {
            connectionStatus = ConnectionStatus.Connected;
            ConnectedToServer?.Invoke();
        }

        private static void OnDisconnectedFromServer()
        {
            connectionStatus = ConnectionStatus.Disconnected;
            DisconnectedFromServer?.Invoke();
        }
    }
}

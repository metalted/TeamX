using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeamX
{
    public static class NetworkController
    {
        //Network
        private static NetPeerConfiguration netPeerConfiguration;
        private static NetClient client = null;
        private static ConnectionStatus connectionStatus;

        private static InboundMessageHandler inbound;
        private static OutboundMessageHandler outbound;

        // Define events for connection and disconnection
        public static Action ConnectedToServer;
        public static Action DisconnectedFromServer;
        public static Action<List<LevelEditorChange>> LevelEditorChangesEvent;
        public static Action<EditorStateData> ServerDataEvent;
        public static Action<List<PlayerData>> ServerPlayerDataEvent;
        public static Action<PlayerData> PlayerJoinedEvent;
        public static Action<PlayerData> PlayerLeftEvent;
        public static Action<PlayerStateData> PlayerStateEvent;
        public static Action<PlayerTransformData> PlayerTransformEvent;
        public static Action<string> CustomMessageEvent;
        public static Action<List<string>> AlreadyClaimedEvent;

        //Initializing the network means creating and starting the client.
        public static void Initialize()
        {
            netPeerConfiguration = new NetPeerConfiguration("Teamkist");
            netPeerConfiguration.ConnectionTimeout = 5000;
            client = new NetClient(netPeerConfiguration);

            inbound = new InboundMessageHandler();
            outbound = new OutboundMessageHandler(client);

            client.Start();            
        }

        public static void SubscribeToEvents()
        {
            //When we enter the level editor, send state change
            GameObserver.EnteredLevelEditor += () =>
            {
                NetOutgoingMessage msg = outbound.CreatePlayerStateDataMessage((byte)CharacterMode.Build);
                outbound.SendMessage(msg);
            };

            //When we go to race mode, send state change
            GameObserver.EnteredGame += () =>
            {
                NetOutgoingMessage msg = outbound.CreatePlayerStateDataMessage((byte)CharacterMode.Race);
                outbound.SendMessage(msg);
            };

            //State change during racing
            GameObserver.LocalStateChange += (stateData) =>
            {
                NetOutgoingMessage msg = outbound.CreatePlayerStateDataMessage(stateData.state);
                outbound.SendMessage(msg);
            };

            //When transform changes send it to the server.
            GameObserver.LocalTransformChange += (transformData) =>
            {
                NetOutgoingMessage msg = outbound.CreatePlayerTransformDataMessage(transformData.position, transformData.euler, (byte)PlayerManagement.GetLocalCharacterMode());
                outbound.SendMessage(msg);
            };

            //Send updates about level editor changes.
            EditorObserver.LevelEditorChangesEvent += (changes) =>
            {
                NetOutgoingMessage msg = outbound.CreateLevelEditorChangesMessage(changes);
                outbound.SendMessage(msg);
            };

            //Send messages about selection changes.
            SelectionObserver.BlocksAddedToSelection += (added) =>
            {
                NetOutgoingMessage msg = outbound.CreateClaimSelectionMessage(added);
                outbound.SendMessage(msg);
            };

            SelectionObserver.BlocksRemovedFromSelection += (removed) =>
            {
                NetOutgoingMessage msg = outbound.CreateUnclaimSelectionMessage(removed);
                outbound.SendMessage(msg);
            };
        }

        //Read the messages when necessary.
        public static void Update()
        {
            if (connectionStatus == ConnectionStatus.Connecting || connectionStatus == ConnectionStatus.Connected)
            {
                ReadMessages();
            }
        }

        public static bool ConnectToServer(string ip, int port)
        {
            if (client == null)
            {
                return false;
            }

            if (connectionStatus == ConnectionStatus.Connecting || connectionStatus == ConnectionStatus.Connected)
            {
                return false;
            }

            try
            {
                connectionStatus = ConnectionStatus.Connecting;
                client.Connect(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Disconnect()
        {
            if (client != null)
            {
                return false;
            }

            if(connectionStatus == ConnectionStatus.Disconnecting || connectionStatus == ConnectionStatus.NotConnected || connectionStatus == ConnectionStatus.Connecting)
            {
                return false;
            }

            client.Disconnect("");
            return true;
        }

        private static void OnConnectedToServer()
        {
            connectionStatus = ConnectionStatus.Connected;

            //Log in with our user data and request the server data.
            outbound.LogIn(true);

            ConnectedToServer?.Invoke();           
        }

        // Method to be called when disconnected from the server
        private static void OnDisconnectedFromServer()
        {
            connectionStatus = ConnectionStatus.NotConnected;

            DisconnectedFromServer?.Invoke();
        }

        public static void ReadMessages()
        {
            if (client == null)
            {
                return;
            }

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
                        NetworkMessageType messageType = (NetworkMessageType) incomingMessage.ReadByte();
                        inbound.HandleInboundMessage(messageType, incomingMessage);
                        break;
                }
            }
        }        
    }
}

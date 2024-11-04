using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeamX
{
    public class InboundMessageHandler
    {
        public void HandleInboundMessage(NetworkMessageType messageType, NetIncomingMessage incomingMessage)
        {
            switch (messageType)
            {
                case NetworkMessageType.JoinedPlayerData:
                    HandleJoinedPlayerData(incomingMessage);
                    break;
                case NetworkMessageType.ServerPlayerData:
                    HandleServerPlayerData(incomingMessage);
                    break;
                case NetworkMessageType.PlayerTransformData:
                    HandlePlayerTransformData(incomingMessage);
                    break;
                case NetworkMessageType.PlayerStateData:
                    HandlePlayerStateData(incomingMessage);
                    break;
                case NetworkMessageType.PlayerLeft:
                    HandlePlayerLeft(incomingMessage);
                    break;
                case NetworkMessageType.ServerData:
                    HandleServerData(incomingMessage);
                    break;
                case NetworkMessageType.LevelEditorChangeEvents:
                    HandleLevelEditorChangeEvents(incomingMessage);
                    break;
                case NetworkMessageType.CustomMessage:
                    HandleCustomMessage(incomingMessage);
                    break;
                case NetworkMessageType.ClaimSelectionEvent:
                    break;
                case NetworkMessageType.UnclaimSelectionEvent:
                    break;
                case NetworkMessageType.AlreadyClaimedEvent:
                    HandleAlreadyClaimedEvent(incomingMessage);
                    break;
                default:
                    // Optional: handle unknown message types here if needed
                    break;
            }
        }
        
        private void HandleJoinedPlayerData(NetIncomingMessage incomingMessage) 
        {
            PlayerData p = new PlayerData();
            p.playerID = incomingMessage.ReadInt32();
            p.state = incomingMessage.ReadByte();
            p.name = incomingMessage.ReadString();
            p.zeepkist = incomingMessage.ReadInt32();
            p.frontWheels = incomingMessage.ReadInt32();
            p.rearWheels = incomingMessage.ReadInt32();
            p.paraglider = incomingMessage.ReadInt32();
            p.horn = incomingMessage.ReadInt32();
            p.hat = incomingMessage.ReadInt32();
            p.glasses = incomingMessage.ReadInt32();
            p.color_body = incomingMessage.ReadInt32();
            p.color_leftArm = incomingMessage.ReadInt32();
            p.color_rightArm = incomingMessage.ReadInt32();
            p.color_leftLeg = incomingMessage.ReadInt32();
            p.color_rightLeg = incomingMessage.ReadInt32();
            p.color = incomingMessage.ReadInt32();

            NetworkController.PlayerJoinedEvent?.Invoke(p);
        }
        
        private void HandleServerPlayerData(NetIncomingMessage incomingMessage) 
        { 
            List<PlayerData> playerDatas = new List<PlayerData>();

            int playerCount = incomingMessage.ReadInt32();
            for (int i = 0; i < playerCount; i++)
            {
                PlayerData p = new PlayerData();
                p.playerID = incomingMessage.ReadInt32();
                p.state = incomingMessage.ReadByte();
                p.name = incomingMessage.ReadString();
                p.zeepkist = incomingMessage.ReadInt32();
                p.frontWheels = incomingMessage.ReadInt32();
                p.rearWheels = incomingMessage.ReadInt32();
                p.paraglider = incomingMessage.ReadInt32();
                p.horn = incomingMessage.ReadInt32();
                p.hat = incomingMessage.ReadInt32();
                p.glasses = incomingMessage.ReadInt32();
                p.color_body = incomingMessage.ReadInt32();
                p.color_leftArm = incomingMessage.ReadInt32();
                p.color_rightArm = incomingMessage.ReadInt32();
                p.color_leftLeg = incomingMessage.ReadInt32();
                p.color_rightLeg = incomingMessage.ReadInt32();
                p.color = incomingMessage.ReadInt32();

                playerDatas.Add(p);
            }

            NetworkController.ServerPlayerDataEvent?.Invoke(playerDatas);
        }

        private void HandlePlayerTransformData(NetIncomingMessage incomingMessage) 
        {         
            PlayerStateData ps = new PlayerStateData();
            PlayerTransformData pt = new PlayerTransformData();

            ps.playerID = incomingMessage.ReadInt32();
            pt.playerID = ps.playerID;

            pt.position = new Vector3(incomingMessage.ReadFloat(), incomingMessage.ReadFloat(), incomingMessage.ReadFloat());
            pt.euler = new Vector3(incomingMessage.ReadFloat(), incomingMessage.ReadFloat(), incomingMessage.ReadFloat());
            
            ps.state = incomingMessage.ReadByte();

            NetworkController.PlayerStateEvent?.Invoke(ps);
            NetworkController.PlayerTransformEvent?.Invoke(pt);
        }
        
        private void HandlePlayerStateData(NetIncomingMessage incomingMessage) 
        {
            PlayerStateData ps = new PlayerStateData();
            ps.playerID = incomingMessage.ReadInt32();
            ps.state = incomingMessage.ReadByte();
            NetworkController.PlayerStateEvent?.Invoke(ps);
        }        
        
        private void HandlePlayerLeft(NetIncomingMessage incomingMessage) 
        {
            int id = incomingMessage.ReadInt32();
            PlayerData pd = PlayerManagement.GetRemotePlayerData(id);
            pd.playerID = id;
            NetworkController.PlayerLeftEvent?.Invoke(pd);
        }
        
        private void HandleServerData(NetIncomingMessage incomingMessage) 
        { 
            EditorStateData editorStateData = new EditorStateData();
            editorStateData.floor = incomingMessage.ReadInt32();
            editorStateData.skybox = incomingMessage.ReadInt32();

            int blockCount = incomingMessage.ReadInt32();
            for (int i = 0; i < blockCount; i++) 
            {
                editorStateData.blocks.Add(incomingMessage.ReadString());
            }

            NetworkController.ServerDataEvent?.Invoke(editorStateData);
        }
        
        private void HandleLevelEditorChangeEvents(NetIncomingMessage incomingMessage) 
        {
            List<LevelEditorChange> changes = new List<LevelEditorChange>();

            int changeCount = incomingMessage.ReadInt32();

            for (int i = 0; i < changeCount; i++)
            {
                NetworkMessageType changeEventType = (NetworkMessageType)incomingMessage.ReadByte();
                switch (changeEventType)
                {
                    case NetworkMessageType.BlockCreateEvent:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.BlockCreate,
                            string_data = incomingMessage.ReadString()
                        });
                        break;
                    case NetworkMessageType.BlockDestroyEvent:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.BlockDestroy,
                            UID = incomingMessage.ReadString()
                        });
                        break;
                    case NetworkMessageType.BlockChangeEvent:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.BlockUpdate,
                            UID = incomingMessage.ReadString(),
                            string_data = incomingMessage.ReadString()
                        });
                        break;
                    case NetworkMessageType.EditorFloorEvent:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Floor,
                            int_data = incomingMessage.ReadInt32()
                        });
                        break;
                    case NetworkMessageType.EditorSkyboxEvent:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Skybox,
                            int_data = incomingMessage.ReadInt32()
                        });
                        break;
                }
            }

            // Invoke the event with the populated changes list
            NetworkController.LevelEditorChangesEvent?.Invoke(changes);
        }

        private void HandleCustomMessage(NetIncomingMessage incomingMessage) 
        {
            string data = incomingMessage.ReadString();
            NetworkController.CustomMessageEvent.Invoke(data);
        }

        private void HandleAlreadyClaimedEvent(NetIncomingMessage incomingMessage)
        {
            int uidCount = incomingMessage.ReadInt32();
            List<string> uids = new List<string>();
            for (int i = 0; i < uidCount; i++)
            {
                uids.Add(incomingMessage.ReadString());
            }

            NetworkController.AlreadyClaimedEvent?.Invoke(uids);
        }
    }
}

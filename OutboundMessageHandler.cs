using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace TeamX
{
    public class OutboundMessageHandler
    {
        private NetClient client;

        public OutboundMessageHandler(NetClient client)
        {
            this.client = client;
        }        

        public bool LogIn(bool requestServerData = false)
        {
            PlayerData localPlayerData = PlayerManagement.GetLocalPlayerData();
            NetOutgoingMessage logInMessage = CreateLogInMessage(localPlayerData, true);

            if(logInMessage == null)
            {
                return false;
            }

            return SendMessage(logInMessage);
        }

        public bool SendMessage(NetOutgoingMessage message)
        {
            if (client == null)
            {
                return false;
            }

            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            return true;
        }

        private NetOutgoingMessage CreateLogInMessage(PlayerData playerData, bool requestServerData)
        {
            if(client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.LogIn);
            message.Write(playerData.name);
            message.Write(playerData.zeepkist);
            message.Write(playerData.frontWheels);
            message.Write(playerData.rearWheels);
            message.Write(playerData.paraglider);
            message.Write(playerData.horn);
            message.Write(playerData.hat);
            message.Write(playerData.glasses);
            message.Write(playerData.color_body);
            message.Write(playerData.color_leftArm);
            message.Write(playerData.color_rightArm);
            message.Write(playerData.color_leftLeg);
            message.Write(playerData.color_rightLeg);
            message.Write(playerData.color);
            message.Write(requestServerData);
            return message;
        }

        public NetOutgoingMessage CreatePlayerTransformDataMessage(Vector3 position, Vector3 euler, byte state)
        {
            if(client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.PlayerTransformData);
            message.Write(position.x);
            message.Write(position.y);
            message.Write(position.z);
            message.Write(euler.x);
            message.Write(euler.y);
            message.Write(euler.z);
            message.Write(state);
            return message;
        }

        public NetOutgoingMessage CreatePlayerStateDataMessage(byte state)
        {
            if (client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.PlayerStateData);
            message.Write(state);
            return message;
        }

        public NetOutgoingMessage CreateLevelEditorChangesMessage(List<LevelEditorChange> changes)
        {
            if (client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.LevelEditorChangeEvents);
            message.Write(changes.Count);

            foreach(LevelEditorChange change in changes)
            {
                switch(change.changeType)
                {
                    case LevelEditorChange.ChangeType.BlockCreate:
                        message.Write((byte)NetworkMessageType.BlockCreateEvent);
                        message.Write(change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockUpdate:
                        message.Write((byte)NetworkMessageType.BlockChangeEvent);
                        message.Write(change.UID);
                        message.Write(change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockDestroy:
                        message.Write((byte)NetworkMessageType.BlockDestroyEvent);
                        message.Write(change.UID);
                        break;
                    case LevelEditorChange.ChangeType.Floor:
                        message.Write((byte)NetworkMessageType.EditorFloorEvent);
                        message.Write(change.int_data);
                        break;
                    case LevelEditorChange.ChangeType.Skybox:
                        message.Write((byte)NetworkMessageType.EditorSkyboxEvent);
                        message.Write(change.int_data);
                        break;
                }
            }

            return message;
        }

        public NetOutgoingMessage CreateCustomMessage(string payload)
        {
            if (client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte) NetworkMessageType.CustomMessage);
            message.Write(payload);
            return message;
        }       
        
        public NetOutgoingMessage CreateClaimSelectionMessage(List<string> blockUIDs)
        {
            if (client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.ClaimSelectionEvent);
            message.Write(blockUIDs.Count);
            foreach(string uid in blockUIDs)
            {
                message.Write(uid);
            }
            return message;
        }

        public NetOutgoingMessage CreateUnclaimSelectionMessage(List<string> blockUIDs)
        {
            if (client == null)
            {
                return null;
            }

            NetOutgoingMessage message = client.CreateMessage();
            message.Write((byte)NetworkMessageType.UnclaimSelectionEvent);
            message.Write(blockUIDs.Count);
            foreach (string uid in blockUIDs)
            {
                message.Write(uid);
            }
            return message;
        }
    }
}

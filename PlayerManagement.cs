using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeamX
{
    public static class PlayerManagement
    {
        private static Shpleeble shpleeblePrefab;
        private static Dictionary<int, Shpleeble> remotePlayers = new Dictionary<int, Shpleeble>();
        private static CharacterMode localCharacterMode = CharacterMode.Build;

        public static void SubscribeToEvents()
        {
            //This event is always called
            GameObserver.EnteredMainMenu += ProcessMainMenuEntry;

            //These event are only called when the network is running.
            NetworkController.ServerPlayerDataEvent += ProcessServerPlayerData;
            NetworkController.PlayerJoinedEvent += ProcessRemotePlayerJoin;
            NetworkController.PlayerLeftEvent += ProcessRemotePlayerLeft;
            NetworkController.PlayerTransformEvent += ProcessRemotePlayerTransformData;
            NetworkController.PlayerStateEvent += ProcessRemotePlayerStateData;

            //Keep track of the different states of the local character, as we send this along with the transform packages. Only called when teamx is enabled.
            GameObserver.EnteredLevelEditor += () => { localCharacterMode = CharacterMode.Build; };
            GameObserver.EnteredGame += () => { localCharacterMode = CharacterMode.Race; };
            GameObserver.LocalStateChange += (stateData) => { localCharacterMode = (CharacterMode) stateData.state; };
        }

        //Clean up all the multiplayer characters and info and create a sphleeble if this is the first time the game starts.
        private static void ProcessMainMenuEntry()
        {
            Clear();

            if (shpleeblePrefab == null)
            {
                shpleeblePrefab = TeamX.Utils.CreateShpleeblePrefabInMainMenu();
            }
        }

        //Clean up any player models and references.
        public static void Clear()
        {
            foreach (Shpleeble shpleeble in remotePlayers.Values)
            {
                if (shpleeble != null)
                {
                    GameObject.Destroy(shpleeble.gameObject);
                }
            }

            remotePlayers.Clear();
        }

        public static CharacterMode GetLocalCharacterMode()
        {
            return localCharacterMode;
        }
        public static PlayerData GetLocalPlayerData()
        {
            PlayerData playerData = new PlayerData();
            playerData.playerID = -1;
            playerData.state = 255;

            try
            {
                ZeepkistNetworking.CosmeticIDs cosmeticIDs = ProgressionManager.Instance.GetAdventureCosmetics();

                playerData.name = PlayerManager.Instance.steamAchiever.GetPlayerName(false);

                playerData.zeepkist = cosmeticIDs.zeepkist;
                playerData.frontWheels = cosmeticIDs.frontWheels;
                playerData.rearWheels = cosmeticIDs.rearWheels;
                playerData.paraglider = cosmeticIDs.paraglider;
                playerData.horn = cosmeticIDs.horn;
                playerData.hat = cosmeticIDs.hat;
                playerData.glasses = cosmeticIDs.glasses;
                playerData.color_body = cosmeticIDs.color_body;
                playerData.color_leftArm = cosmeticIDs.color_leftArm;
                playerData.color_rightArm = cosmeticIDs.color_rightArm;
                playerData.color_leftLeg = cosmeticIDs.color_leftLeg;
                playerData.color_rightLeg = cosmeticIDs.color_rightLeg;
                playerData.color = cosmeticIDs.color;
            }
            catch
            {
                playerData.name = "Sphleeble";
                playerData.hat = 23000;
                playerData.color = 1000;
                playerData.zeepkist = 1000;

                playerData.zeepkist = 1000;
                playerData.frontWheels = 1000;
                playerData.rearWheels = 1000;
                playerData.paraglider = 1000;
                playerData.horn = 1000;
                playerData.hat = 23000;
                playerData.glasses = 1000;
                playerData.color_body = 1000;
                playerData.color_leftArm = 1000;
                playerData.color_rightArm = 1000;
                playerData.color_leftLeg = 1000;
                playerData.color_rightLeg = 1000;
                playerData.color = 1000;
            }

            return playerData;
        }
        public static PlayerData GetRemotePlayerData(int playerID)
        {
            if(!remotePlayers.ContainsKey(playerID))
            {
                return new PlayerData() { playerID = -1 };
            }

            return remotePlayers[playerID].GetPlayerData();
        }

        private static bool CreateRemotePlayer(PlayerData playerData)
        {
            if (remotePlayers.ContainsKey(playerData.playerID))
            {
                return false;
            }

            Shpleeble remotePlayer = GameObject.Instantiate<Shpleeble>(shpleeblePrefab);
            GameObject.DontDestroyOnLoad(remotePlayer);
            remotePlayer.SetPlayerData(playerData);
            remotePlayers.Add(playerData.playerID, remotePlayer);
            remotePlayer.gameObject.SetActive(TeamXConfiguration.showPlayers.Value);
            remotePlayer.Activate();

            return true;
        }       
        private static bool DestroyRemotePlayer(int playerID)
        {
            if (!remotePlayers.ContainsKey(playerID))
            {
                return false;
            }

            GameObject.Destroy(remotePlayers[playerID].gameObject);
            remotePlayers.Remove(playerID);
            return true;
        }

        //Process network messages
        private static void ProcessServerPlayerData(List<PlayerData> data)
        {
            foreach (PlayerData playerData in data)
            {
                CreateRemotePlayer(playerData);
            }
        }
        private static void ProcessRemotePlayerJoin(PlayerData data)
        {
            if (remotePlayers.ContainsKey(data.playerID))
            {
                return;
            }

            CreateRemotePlayer(data);
        }
        private static void ProcessRemotePlayerLeft(PlayerData data)
        {
            if (!remotePlayers.ContainsKey(data.playerID))
            {
                return;
            }

            DestroyRemotePlayer(data.playerID);
        }
        private static void ProcessRemotePlayerTransformData(PlayerTransformData playerTransformData)
        {
            if(!remotePlayers.ContainsKey(playerTransformData.playerID))
            {
                return;
            }

            remotePlayers[playerTransformData.playerID].UpdateTransform(playerTransformData.position, playerTransformData.euler);
        }
        private static void ProcessRemotePlayerStateData(PlayerStateData playerStateData)
        {
            if (!remotePlayers.ContainsKey(playerStateData.playerID))
            {
                return;
            }

            remotePlayers[playerStateData.playerID].SetMode(playerStateData.state);
        }   
    }
}

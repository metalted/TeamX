using UnityEngine;

namespace TeamX
{
    public class EventLogger
    {
        public void SubscribeToAllEvents()
        {
            // Subscribe to BlockCreatedEvent
            EditorObserver.BlockCreatedEvent += (properties) =>
            {
                Debug.LogWarning($"BlockCreatedEvent called with properties: {properties}");
            };

            // Subscribe to BlockUpdatedEvent
            EditorObserver.BlockUpdatedEvent += (UID, properties) =>
            {
                Debug.LogWarning($"BlockUpdatedEvent called with UID: {UID}, properties: {properties}");
            };

            // Subscribe to BlockDestroyedEvent
            EditorObserver.BlockDestroyedEvent += (UID) =>
            {
                Debug.LogWarning($"BlockDestroyedEvent called with blockId: {UID}");
            };

            // Subscribe to FloorUpdatedEvent
            EditorObserver.FloorUpdatedEvent += (floorID) =>
            {
                Debug.LogWarning($"FloorUpdatedEvent called with floorID: {floorID}");
            };

            // Subscribe to SkyboxUpdatedEvent
            EditorObserver.SkyboxUpdatedEvent += (skyboxID) =>
            {
                Debug.LogWarning($"SkyboxUpdatedEvent called with skyboxID: {skyboxID}");
            };

            // Subscribe to BlocksAddedToSelection
            SelectionObserver.BlocksAddedToSelection += (added) =>
            {
                Debug.LogWarning($"BlocksAddedToSelection called with added UIDs: {string.Join(',', added)}");
            };

            //Subscribe to BlocksRemovedFromSelection
            SelectionObserver.BlocksRemovedFromSelection += (removed) =>
            {
                Debug.LogWarning($"BlocksRemovedFromSelection called with removed UIDs: {string.Join(',', removed)}");
            };

            // Subscribe to EnteredLevelEditor
            GameObserver.EnteredLevelEditor += () =>
            {
                Debug.LogWarning("EnteredLevelEditor event called.");
            };

            //Subscribe to EnteredMainMenu
            GameObserver.EnteredMainMenu += () =>
            {
                Debug.LogWarning("Entered Main Menu event called.");
            };

            // Subscribe to EnteredGame
            GameObserver.EnteredGame += () =>
            {
                Debug.LogWarning("EnteredGame event called.");
            };

            // Subscribe to LocalTransformChange
            GameObserver.LocalTransformChange += (playerTransformData) =>
            {
                Debug.LogWarning($"LocalTransformChange event called with playerID: {playerTransformData.playerID}, " +
                          $"position: {playerTransformData.position}, euler: {playerTransformData.euler}");
            };

            // Subscribe to LocalStateChange
            GameObserver.LocalStateChange += (playerStateData) =>
            {
                Debug.LogWarning($"LocalStateChange event called with playerID: {playerStateData.playerID}, state: {playerStateData.state}");
            };

            // Subscribe to ConnectedToServer
            NetworkController.ConnectedToServer += () =>
            {
                Debug.LogWarning("ConnectedToServer event called.");
            };

            // Subscribe to DisconnectedFromServer
            NetworkController.DisconnectedFromServer += () =>
            {
                Debug.LogWarning("DisconnectedFromServer event called.");
            };

            // Subscribe to LevelEditorChangesEvent
            NetworkController.LevelEditorChangesEvent += (changes) =>
            {
                Debug.LogWarning($"LevelEditorChangesEvent called with {changes.Count} changes.");
            };

            // Subscribe to ServerDataEvent
            NetworkController.ServerDataEvent += (editorStateData) =>
            {
                Debug.LogWarning($"ServerDataEvent called with floor: {editorStateData.floor}, skybox: {editorStateData.skybox}, blocks count: {editorStateData.blocks.Count}");
            };

            // Subscribe to ServerPlayerDataEvent
            NetworkController.ServerPlayerDataEvent += (playerDataList) =>
            {
                Debug.LogWarning($"ServerPlayerDataEvent called with {playerDataList.Count} players.");
            };

            // Subscribe to PlayerJoinedEvent
            NetworkController.PlayerJoinedEvent += (playerData) =>
            {
                Debug.LogWarning($"PlayerJoinedEvent called with playerID: {playerData.playerID}, name: {playerData.name}");
            };

            // Subscribe to PlayerLeftEvent
            NetworkController.PlayerLeftEvent += (playerData) =>
            {
                Debug.LogWarning($"PlayerLeftEvent called with playerID: {playerData.playerID}, name: {playerData.name}");
            };

            // Subscribe to PlayerStateEvent
            NetworkController.PlayerStateEvent += (playerStateData) =>
            {
                Debug.LogWarning($"PlayerStateEvent called with playerID: {playerStateData.playerID}, state: {playerStateData.state}");
            };

            // Subscribe to PlayerTransformEvent
            NetworkController.PlayerTransformEvent += (playerTransformData) =>
            {
                Debug.LogWarning($"PlayerTransformEvent called with playerID: {playerTransformData.playerID}, position: {playerTransformData.position}, euler: {playerTransformData.euler}");
            };

            // Subscribe to CustomMessageEvent
            NetworkController.CustomMessageEvent += (message) =>
            {
                Debug.LogWarning($"CustomMessageEvent called with message: {message}");
            };
        }
    }
}

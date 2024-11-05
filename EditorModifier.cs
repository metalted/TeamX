using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public static class EditorModifier
    {
        public static void SubscribeToEvents()
        {
            //When we receive changes through the network, only apply them if we are in the level editor.
            //When we are not in the level editor, the will get stored and instantiated later when we join the editor again.
            NetworkController.LevelEditorChangesEvent += (changes) =>
            {
                if (GameObserver.InLevelEditor())
                {
                    ApplyChanges(changes);
                }                
            };         
        }

        //Called when we enter the editor, either after logging in or when we come back from a race.
        public static void LoadEditorState(EditorStateData stateData)
        {
            TeamXManager.plugin.StartCoroutine(LoadStateCoroutine(stateData));
        }

        private static IEnumerator LoadStateCoroutine(EditorStateData stateData)
        {
            yield return new WaitForEndOfFrame();

            EditorModifier.UpdateFloor(stateData.floor);
            EditorModifier.UpdateSkybox(stateData.skybox);
            foreach (string block in stateData.blocks)
            {
                CreateBlock(block);
            }

            GameObserver.GetCentral()?.validation.RecalcBlocksAndDraw(false);
        }

        //Called from the network event
        private static void ApplyChanges(List<LevelEditorChange> changes)
        {
            foreach (LevelEditorChange change in changes)
            {
                switch (change.changeType)
                {
                    case LevelEditorChange.ChangeType.BlockCreate:
                        CreateBlock(change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockUpdate:
                        UpdateBlock(change.UID, change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockDestroy:
                        DestroyBlock(change.UID);
                        break;
                    case LevelEditorChange.ChangeType.Floor:
                        UpdateFloor(change.int_data);
                        break;
                    case LevelEditorChange.ChangeType.Skybox:
                        UpdateSkybox(change.int_data);
                        break;
                }
            }
        }                

        private static bool CreateBlock(BlockPropertyJSON blockPropertyJSON)
        {
            GameObserver.GetCentral()?.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            GameObserver.GetCentral()?.validation.RecalcBlocksAndDraw(false);
            return true;
        }

        private static bool CreateBlock(string blockJSON)
        {
            BlockPropertyJSON blockPropertyJSON = LEV_UndoRedo.GetJSONblock(blockJSON);
            CreateBlock(blockPropertyJSON);
            return true;
        }

        private static bool UpdateBlock(string blockUID, string properties)
        {
            BlockProperties blockProperties = GameObserver.GetCentral()?.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                GameObserver.GetCentral()?.undoRedo.allBlocksDictionary.Remove(blockUID);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                TeamX.Utils.AssignPropertyListToBlockPropertyJSON(properties, blockPropertyJSON);
                GameObserver.GetCentral()?.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
                return true;
            }

            return false;
        }

        private static bool DestroyBlock(string blockUID)
        {
            BlockProperties blockProperties = GameObserver.GetCentral()?.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                GameObserver.GetCentral()?.undoRedo.allBlocksDictionary.Remove(blockUID);
                GameObject.Destroy(blockProperties.gameObject);
                GameObserver.GetCentral()?.validation.RecalcBlocksAndDraw(false);
                return true;
            }

            return false;
        }     
        
        private static bool UpdateFloor(int paintID)
        {
            GameObserver.GetCentral()?.painter.SetLoadGroundMaterial(paintID);
            return true;
        }

        private static bool UpdateSkybox(int skyboxID)
        {
            GameObserver.GetCentral()?.skybox.SetToSkybox(skyboxID, true);
            return true;
        }        
    }
}

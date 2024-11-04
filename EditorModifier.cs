using System;
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
            NetworkController.LevelEditorChangesEvent += ApplyRemoteChanges;
        }

        public static void ApplyRemoteChanges(List<LevelEditorChange> changes)
        {
            if(GameObserver.InLevelEditor())
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
        }

        public static bool CreateBlock(BlockPropertyJSON blockPropertyJSON)
        {
            GameObserver.central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            GameObserver.central.validation.RecalcBlocksAndDraw(false);
            return true;
        }

        public static bool CreateBlock(string blockJSON)
        {
            BlockPropertyJSON blockPropertyJSON = LEV_UndoRedo.GetJSONblock(blockJSON);
            CreateBlock(blockPropertyJSON);
            return true;
        }

        public static bool UpdateBlock(string blockUID, string properties)
        {
            BlockProperties blockProperties = GameObserver.central.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                GameObserver.central.undoRedo.allBlocksDictionary.Remove(blockUID);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                TeamX.Utils.AssignPropertyListToBlockPropertyJSON(properties, blockPropertyJSON);
                GameObserver.central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
                return true;
            }

            return false;
        }

        public static bool DestroyBlock(string blockUID)
        {
            BlockProperties blockProperties = GameObserver.central.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                GameObserver.central.undoRedo.allBlocksDictionary.Remove(blockUID);
                GameObject.Destroy(blockProperties.gameObject);
                GameObserver.central.validation.RecalcBlocksAndDraw(false);
                return true;
            }

            return false;
        }     
        
        public static bool UpdateFloor(int paintID)
        {
            GameObserver.central.painter.SetLoadGroundMaterial(paintID);
            return true;
        }

        public static bool UpdateSkybox(int skyboxID)
        {
            GameObserver.central.skybox.SetToSkybox(skyboxID, true);
            return true;
        }        
    }
}

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
        public static LEV_LevelEditorCentral central;

        public static bool CreateBlock(BlockPropertyJSON blockPropertyJSON)
        {
            central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            central.validation.RecalcBlocksAndDraw(false);
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
            BlockProperties blockProperties = central.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                central.undoRedo.allBlocksDictionary.Remove(blockUID);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                TeamX.Utils.AssignPropertyListToBlockPropertyJSON(properties, blockPropertyJSON);
                central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
                return true;
            }

            return false;
        }

        public static bool DestroyBlock(string blockUID)
        {
            BlockProperties blockProperties = central.undoRedo.TryGetBlockFromAllBlocks(blockUID);

            if (blockProperties != null)
            {
                central.undoRedo.allBlocksDictionary.Remove(blockUID);
                GameObject.Destroy(blockProperties.gameObject);
                central.validation.RecalcBlocksAndDraw(false);
                return true;
            }

            return false;
        }     
        
        public static bool UpdateFloor(int paintID)
        {
            central.painter.SetLoadGroundMaterial(paintID);
            return true;
        }

        public static bool UpdateSkybox(int skyboxID)
        {
            central.skybox.SetToSkybox(skyboxID, true);
            return true;
        }        
    }
}

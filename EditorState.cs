using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public static class EditorState
    {
        private static Dictionary<string, BlockPropertyJSON> blocks = new Dictionary<string, BlockPropertyJSON>();
        private static int skybox;
        private static int floor;

        public static void SubscribeToEvents()
        {            
            //This event is only fired when teamx is enabled.
            EditorObserver.LevelEditorChangesEvent += StoreChanges;
            //This event is only fired when the server is running.
            NetworkController.LevelEditorChangesEvent += StoreChanges;
        }

        //Clear the current state.
        public static bool Clear()
        {
            blocks.Clear();
            skybox = 0;
            floor = -1;
            return true;
        }

        //Set a state, currently called when we have received the editor state from the server.
        public static bool SetState(EditorStateData data)
        {
            Clear();

            foreach (string s in data.blocks)
            {
                AddBlock(s);
            }

            SetSkybox(data.skybox);
            SetFloor(data.floor);

            return true;
        }

        //Is called when we come back into the level editor, and we load in the current state.
        public static EditorStateData GetState()
        {
            EditorStateData state = new EditorStateData();
            foreach (BlockPropertyJSON blockPropertyJSON in blocks.Values)
            {
                string blockJSON = LEV_UndoRedo.GetJSONstring(blockPropertyJSON);
                state.blocks.Add(blockJSON);
            }
            state.skybox = skybox;
            state.floor = floor;
            return state;
        }

        //Called from local and remote changes.
        private static void StoreChanges(List<LevelEditorChange> changes)
        {
            foreach (LevelEditorChange change in changes)
            {
                switch(change.changeType)
                {
                    case LevelEditorChange.ChangeType.BlockCreate:
                        AddBlock(change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockUpdate:
                        UpdateBlock(change.UID, change.string_data);
                        break;
                    case LevelEditorChange.ChangeType.BlockDestroy:
                        RemoveBlock(change.UID);
                        break;
                    case LevelEditorChange.ChangeType.Floor:
                        SetFloor(change.int_data);
                        break;
                    case LevelEditorChange.ChangeType.Skybox:
                        SetSkybox(change.int_data);
                        break;
                }
            }
        }        

        private static bool AddBlock(string blockJSON)
        {
            BlockPropertyJSON blockPropertyJSON = LEV_UndoRedo.GetJSONblock(blockJSON);
            if(blocks.ContainsKey(blockPropertyJSON.UID))
            {
                return false;
            }
            blocks.Add(blockPropertyJSON.UID, blockPropertyJSON);
            return true;
        }

        private static bool UpdateBlock(string blockUID, string properties)
        {
            if (!blocks.ContainsKey(blockUID))
            {
                return false;
            }

            TeamX.Utils.AssignPropertyListToBlockPropertyJSON(properties, blocks[blockUID]);
            return true;
        }

        private static bool RemoveBlock(string blockUID)
        {
            if (!blocks.ContainsKey(blockUID))
            {
                return false;
            }

            blocks.Remove(blockUID);
            return true;
        }

        private static bool SetSkybox(int skyboxID)
        {
            skybox = skyboxID;
            return true;
        }

        private static bool SetFloor(int floorID)
        {
            floor = floorID;
            return true;
        }        
    }
}

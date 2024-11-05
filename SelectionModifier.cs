using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamX
{
    public static class SelectionModifier
    {
        public static void SubscribeToEvents()
        {
            //This event will only fire if the network is running.
            NetworkController.AlreadyClaimedEvent += (alreadyClaimed) =>
            {
                //Deselect all already claimed blocks.
                foreach (string claimed in alreadyClaimed)
                {
                    DeselectBlock(claimed);
                }
            };
        }

        //Deselect all blocks in the local editor.
        public static void DeselectAllBlocks(bool notify = false)
        {
            if(!GameObserver.InLevelEditor())
            {
                return;
            }

            GameObserver.GetCentral().selection.DeselectAllBlocks(true, "");

            //By syncing the selection observer, no event will be fired about the changed selection.
            if (!notify)
            {
                SelectionObserver.Sync();
            }
        }

        //Deselect a single block by UID, if it is found and selected locally.
        public static void DeselectBlock(string blockUID, bool notify = false)
        {
            if (!GameObserver.InLevelEditor())
            {
                return;
            }

            int blockIndex = GameObserver.GetCentral().selection.list.FindIndex(item => item.UID == blockUID);

            if (blockIndex != -1)
            {
                GameObserver.GetCentral().selection.RemoveBlockAt(blockIndex, true, true);

                //By syncing the selection observer, no event will be fired about the changed selection.
                if (!notify)
                {
                    SelectionObserver.Sync();
                }
            }
        }

        //Select a single block by UID, if it is found and not already selected.
        public static void SelectBlock(string blockUID, bool notify = false)
        {
            if (!GameObserver.InLevelEditor())
            {
                return;
            }

            int blockIndex = GameObserver.GetCentral().selection.list.FindIndex(item => item.UID == blockUID);
            if (blockIndex == -1)
            {
                if (GameObserver.GetCentral().undoRedo.allBlocksDictionary.ContainsKey(blockUID))
                {
                    GameObserver.GetCentral().selection.AddThisBlock(GameObserver.GetCentral().undoRedo.allBlocksDictionary[blockUID]);
                }

                //By syncing the selection observer, no event will be fired about the changed selection.
                if (!notify)
                {
                    SelectionObserver.Sync();
                }
            }
        }
    }
}

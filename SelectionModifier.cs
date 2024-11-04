using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamX
{
    public static class SelectionModifier
    {
        public static void DeselectAllBlocks(bool notify = false)
        {
            GameObserver.central.selection.DeselectAllBlocks(true, "");

            if (!notify)
            {
                SelectionObserver.Sync();
            }
        }

        public static void DeselectBlock(string blockUID, bool notify = false)
        {
            int blockIndex = GameObserver.central.selection.list.FindIndex(item => item.UID == blockUID);

            if (blockIndex != -1)
            {
                GameObserver.central.selection.RemoveBlockAt(blockIndex, true, true);

                if (!notify)
                {
                    SelectionObserver.Sync();
                }
            }
        }

        public static void SelectBlock(string blockUID, bool notify = false)
        {
            int blockIndex = GameObserver.central.selection.list.FindIndex(item => item.UID == blockUID);
            if (blockIndex == -1)
            {
                if (GameObserver.central.undoRedo.allBlocksDictionary.ContainsKey(blockUID))
                {
                    GameObserver.central.selection.AddThisBlock(GameObserver.central.undoRedo.allBlocksDictionary[blockUID]);
                }

                if (!notify)
                {
                    SelectionObserver.Sync();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamX
{
    public static class SelectionObserver
    {
        private static HashSet<string> lastSelectionUIDs;
        private static HashSet<string> currentUIDs;
        private static List<string> removedUIDs;
        private static List<string> addedUIDs;
        private static int lastListCount = 0;

        public static Action<List<string>> BlocksAddedToSelection;
        public static Action<List<string>> BlocksRemovedFromSelection;

        public static void Initialize()
        {
            lastSelectionUIDs = new HashSet<string>();
            currentUIDs = new HashSet<string>();
            removedUIDs = new List<string>();
            addedUIDs = new List<string>();           
        }

        public static void Sync()
        {
            lastListCount = GameObserver.GetCentral().selection.list.Count;
        }

        //This update function will only be called if teamx is enabled.
        public static void Update()
        {
            if(!GameObserver.InLevelEditor())
            {
                return;
            }

            int currentListCount = GameObserver.GetCentral().selection.list.Count;
            if (currentListCount != lastListCount)
            {
                InspectSelection(GameObserver.GetCentral());
                lastListCount = currentListCount;
            }
        }       

        private static void InspectSelection(LEV_LevelEditorCentral _central)
        {
            currentUIDs.Clear();
            foreach (BlockProperties block in _central.selection.list)
            {
                currentUIDs.Add(block.UID);
            }

            removedUIDs = lastSelectionUIDs.Except(currentUIDs).ToList();
            addedUIDs = currentUIDs.Except(lastSelectionUIDs).ToList();

            lastSelectionUIDs.Clear();
            lastSelectionUIDs.UnionWith(currentUIDs);

            if (removedUIDs.Count > 0)
            {
                BlocksRemovedFromSelection?.Invoke(removedUIDs);
            }

            if (addedUIDs.Count > 0)
            {
                BlocksAddedToSelection?.Invoke(addedUIDs);
            }
        }
    }    
}

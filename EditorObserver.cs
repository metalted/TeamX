﻿using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TeamX
{
    //A player selects a block.
    //The difference between the last selection and the current selection is the delta selection.
    //The delta selection is send to the server and the server tries to lock it for this player.
    //If the selection is already locked, send a Deselect Package back to the player with the json properties of all the blocks to set them back to their previous state in case they have been altered.

    //The server should have the following functions.
    //Unlock all selection (removes any locks from the blocks for this player)
    //Lock selection (adds a lock to a specific block)
    //Lo
    //A lock selection




    public static class EditorObserver
    {
        // Declare events for each change type
        public static Action<string> BlockCreatedEvent;
        public static Action<string, string> BlockUpdatedEvent;
        public static Action<string> BlockDestroyedEvent;
        public static Action<int> FloorUpdatedEvent;
        public static Action<int> SkyboxUpdatedEvent;
        private static List<string> lastSelectedUIDs = new List<string>();

        // Methods to invoke events
        public static void BlockCreated(string blockJSON)
        {
            BlockCreatedEvent?.Invoke(blockJSON);
        }

        public static void BlockUpdated(string blockUID, string properties)
        {
            BlockUpdatedEvent?.Invoke(blockUID, properties);
        }

        public static void BlockDestroyed(string blockUID)
        {
            BlockDestroyedEvent?.Invoke(blockUID);
        }

        public static void FloorUpdated(int paintID)
        {
            FloorUpdatedEvent?.Invoke(paintID);
        }

        public static void SkyboxUpdated(int skyboxID)
        {
            SkyboxUpdatedEvent?.Invoke(skyboxID);
        }
    }

    // Called when a change is made on an object.
    [HarmonyPatch(typeof(LEV_UndoRedo), "SomethingChanged")]
    public class LEV_UndoRedoSomethingChangedPatch
    {
        public static void Postfix(ref Change_Collection whatChanged, ref string source)
        {
            foreach (Change_Single changeSingle in whatChanged.changeList)
            {
                switch (whatChanged.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            EditorObserver.BlockCreated(changeSingle.after);
                        }
                        else if (changeSingle.after == null)
                        {
                            EditorObserver.BlockDestroyed(changeSingle.GetUID());
                        }
                        else
                        {
                            EditorObserver.BlockUpdated(changeSingle.GetUID(), TeamX.Utils.FixedPropertyListToString(changeSingle.after));
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        EditorObserver.FloorUpdated(changeSingle.int_after);
                        break;
                    case Change_Collection.ChangeType.skybox:
                        EditorObserver.SkyboxUpdated(changeSingle.int_after);
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }
        }
    }

    // Called when a change is undone.
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyBeforeState")]
    public class LEV_UndoRedoApplyBeforeStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            Change_Collection changes = __instance.historyList[__instance.currentHistoryPosition];

            foreach (Change_Single changeSingle in changes.changeList)
            {
                switch (changes.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            EditorObserver.BlockDestroyed(changeSingle.after);
                        }
                        else if (changeSingle.after == null)
                        {
                            EditorObserver.BlockCreated(changeSingle.GetUID());
                        }
                        else
                        {
                            EditorObserver.BlockUpdated(changeSingle.GetUID(), TeamX.Utils.FixedPropertyListToString(changeSingle.before));
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        EditorObserver.FloorUpdated(changeSingle.int_before);
                        break;
                    case Change_Collection.ChangeType.skybox:
                        EditorObserver.SkyboxUpdated(changeSingle.int_before);
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }
        }
    }

    // Called when a change is redone.
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyAfterState")]
    public class LEV_UndoRedoApplyAfterStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            Change_Collection changes = __instance.historyList[__instance.currentHistoryPosition];

            foreach (Change_Single changeSingle in changes.changeList)
            {
                switch (changes.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            EditorObserver.BlockCreated(changeSingle.after);
                        }
                        else if (changeSingle.after == null)
                        {
                            EditorObserver.BlockDestroyed(changeSingle.GetUID());
                        }
                        else
                        {
                            EditorObserver.BlockUpdated(changeSingle.GetUID(), TeamX.Utils.FixedPropertyListToString(changeSingle.after));
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        EditorObserver.FloorUpdated(changeSingle.int_after);
                        break;
                    case Change_Collection.ChangeType.skybox:
                        EditorObserver.SkyboxUpdated(changeSingle.int_after);
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LEV_Selection), "RegisterManualSelectionBreakLock")]
    public class LEV_SelectionRegisterManualSelectionBreakLockPatch
    {
        public static void Postfix(ref List<string> selectionUIDs_before, ref List<string> selectionUIDs_after)
        {
            Debug.LogError("Before (" + selectionUIDs_before.Count + "): " + string.Join(',', selectionUIDs_before));
            Debug.LogError("After (" + selectionUIDs_after.Count + "): " + string.Join(',', selectionUIDs_after));
        }
    }

    [HarmonyPatch(typeof(LEV_Selection), "DeselectAllBlocks")]
    public class LEV_SelectionDeselectAllBlocksPatch
    {
        public static void Postfix()
        {
            Debug.LogError("Deselect All Blocks");
        }
    }

    [HarmonyPatch(typeof(LEV_Selection), "UndoRedoReselection")]
    public class LEV_SelectionUndoRedoReselection
    {
        public static void Postfix(ref List<BlockProperties> newSelection)
        {
            Debug.LogWarning("UndoRedoSelect (" + newSelection.Count + "): " + string.Join(',', newSelection.Select(bp => bp.UID).ToList()));
        }
    }
}

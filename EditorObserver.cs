using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TeamX
{
    public static class EditorObserver
    {
        //This event is only called when TeamX is enabled.
        public static Action<List<LevelEditorChange>> LevelEditorChangesEvent;
    }

    // Called when a change is made on an object.
    [HarmonyPatch(typeof(LEV_UndoRedo), "SomethingChanged")]
    public class LEV_UndoRedoSomethingChangedPatch
    {
        public static void Postfix(ref Change_Collection whatChanged, ref string source)
        {
            if(!TeamXManager.IsTeamXEnabled())
            {
                return;
            }

            List<LevelEditorChange> changes = new List<LevelEditorChange>();

            foreach (Change_Single changeSingle in whatChanged.changeList)
            {
                switch (whatChanged.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockCreate,
                                string_data = changeSingle.after
                            });
                        }
                        else if (changeSingle.after == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockDestroy,
                                UID = changeSingle.GetUID()
                            });
                        }
                        else
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockUpdate,
                                string_data = TeamX.Utils.FixedPropertyListToString(changeSingle.after),
                                UID = changeSingle.GetUID()
                            });                            
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Floor,
                            int_data = changeSingle.int_after
                        });
                        break;
                    case Change_Collection.ChangeType.skybox:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Skybox,
                            int_data = changeSingle.int_after
                        });
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }

            EditorObserver.LevelEditorChangesEvent?.Invoke(changes);
        }
    }

    
    // Called when a change is undone.
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyBeforeState")]
    public class LEV_UndoRedoApplyBeforeStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            if (!TeamXManager.IsTeamXEnabled())
            {
                return;
            }

            Change_Collection changeCollection = __instance.historyList[__instance.currentHistoryPosition];
            List<LevelEditorChange> changes = new List<LevelEditorChange>();

            foreach (Change_Single changeSingle in changeCollection.changeList)
            {
                switch (changeCollection.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockDestroy,
                                UID = changeSingle.GetUID()
                            });
                        }
                        else if (changeSingle.after == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockCreate,
                                string_data = changeSingle.before
                            });
                        }
                        else
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockUpdate,
                                string_data = TeamX.Utils.FixedPropertyListToString(changeSingle.before),
                                UID = changeSingle.GetUID()
                            });
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Floor,
                            int_data = changeSingle.int_before
                        });
                        break;
                    case Change_Collection.ChangeType.skybox:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Skybox,
                            int_data = changeSingle.int_before
                        });
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }

            EditorObserver.LevelEditorChangesEvent?.Invoke(changes);
        }
    }

    // Called when a change is redone.
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyAfterState")]
    public class LEV_UndoRedoApplyAfterStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            if (!TeamXManager.IsTeamXEnabled())
            {
                return;
            }

            Change_Collection changeCollection = __instance.historyList[__instance.currentHistoryPosition];
            List<LevelEditorChange> changes = new List<LevelEditorChange>();

            foreach (Change_Single changeSingle in changeCollection.changeList)
            {
                switch (changeCollection.changeType)
                {
                    case Change_Collection.ChangeType.block:
                        if (changeSingle.before == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockCreate,
                                string_data = changeSingle.after
                            });
                        }
                        else if (changeSingle.after == null)
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockDestroy,
                                UID = changeSingle.GetUID()
                            });
                        }
                        else
                        {
                            changes.Add(new LevelEditorChange()
                            {
                                changeType = LevelEditorChange.ChangeType.BlockUpdate,
                                string_data = TeamX.Utils.FixedPropertyListToString(changeSingle.after),
                                UID = changeSingle.GetUID()
                            });
                        }
                        break;
                    case Change_Collection.ChangeType.floor:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Floor,
                            int_data = changeSingle.int_after
                        });
                        break;
                    case Change_Collection.ChangeType.skybox:
                        changes.Add(new LevelEditorChange()
                        {
                            changeType = LevelEditorChange.ChangeType.Skybox,
                            int_data = changeSingle.int_after
                        });
                        break;
                    case Change_Collection.ChangeType.selection:
                        break;
                }
            }

            EditorObserver.LevelEditorChangesEvent?.Invoke(changes);
        }
    }
}

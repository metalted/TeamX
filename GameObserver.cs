using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    //This class will observe the state of the game and scenes and watch the local players movements and actions.
    public static class GameObserver
    {
        public static Action EnteredLevelEditor;
        public static Action EnteredGame;
        public static Action EnteredMainMenu;
        public static Action<PlayerTransformData> LocalTransformChange;
        public static Action<PlayerStateData> LocalStateChange;

        public static LEV_LevelEditorCentral central { get; private set; }
        public static SetupGame game { get; private set; }
        
        public static bool InLevelEditor()
        {
            return central != null;
        }

        public static bool InGame()
        {
            return false;
        }

        public static void OnMainMenu()
        {
            EnteredMainMenu?.Invoke();
        }

        public static void OnLevelEditor(LEV_LevelEditorCentral levCentral)
        {
            central = levCentral;
            EnteredLevelEditor?.Invoke();
            LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = (byte)CharacterMode.Build });

            if(central.cam.cameraTransform.gameObject.GetComponent<PlayerObserver>() == null)
            {
                PlayerObserver observer = central.cam.cameraTransform.gameObject.AddComponent<PlayerObserver>();
                observer.TransformChange += GameObserver.OnLocalTransformChange;
            }
        }

        public static void OnGame(SetupGame setupGame)
        {
            game = setupGame;
            EnteredGame?.Invoke();
            LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = (byte)CharacterMode.Race });
        }

        public static void OnLocalTransformChange(PlayerTransformData data)
        {
            LocalTransformChange?.Invoke(data);
        }
    }

    //Called when we enter the main menu.
    [HarmonyPatch(typeof(MainMenuUI), "Awake")]
    public class TKMainMenuUIAwakePatch
    {
        public static void Prefix()
        {
            GameObserver.OnMainMenu();            
        }
    }

    [HarmonyPatch(typeof(LEV_LevelEditorCentral), "Awake")]
    public class LevelEditorCentralAwakePatch
    {
        public static void Postfix(LEV_LevelEditorCentral __instance)
        {
            GameObserver.OnLevelEditor(__instance);
        }
    }

    [HarmonyPatch(typeof(SetupGame), "Awake")]
    public class SetupGameAwakePatch
    {
        public static void Postfix(SetupGame __instance)
        {
            GameObserver.OnGame(__instance);
        }
    }

    [HarmonyPatch(typeof(GameMaster), "SpawnPlayers")]
    public class GameMasterSpawnPlayersPatch
    {
        public static void Postfix(GameMaster __instance)
        {
            Transform localRacer = __instance.PlayersReady[0].transform;
            if(localRacer.gameObject.GetComponent<PlayerObserver>() == null)
            {
                PlayerObserver observer = localRacer.gameObject.AddComponent<PlayerObserver>();
                observer.TransformChange += GameObserver.OnLocalTransformChange;
            }
        }
    }

    //Called when a players state changes
    [HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
    public class NewControlCarSetZeepkistStatePatch
    {
        public static void Prefix(ref byte newState, ref string source, ref bool playSound)
        {
            if (newState == (byte)3)
            {
                GameObserver.LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = 2 });
            }
            else
            {
                GameObserver.LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = 1 });
            }            
        }
    }
}

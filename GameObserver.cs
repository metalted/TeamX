using HarmonyLib;
using System;
using UnityEngine;

namespace TeamX
{
    public static class GameObserver
    {
        //These events are ALWAYS called.
        public static Action EnteredMainMenu;        
        public static Action ApplicationQuit;

        //The events are only called when teamx is enabled.
        public static Action EnteredLevelEditor;
        public static Action EnteredGame;
        public static Action QuitLevelEditor;
        public static Action<PlayerTransformData> LocalTransformChange;
        public static Action<PlayerStateData> LocalStateChange;

        //Reference to the current central and game.
        private static LEV_LevelEditorCentral central;
        private static SetupGame game;
        private static string currentScene = "";

        public static void SetCentral(LEV_LevelEditorCentral c)
        {
            central = c;
        }

        public static LEV_LevelEditorCentral GetCentral()
        {
            return central;
        }

        public static void SetGame(SetupGame g)
        {
            game = g;
        }

        public static SetupGame GetGame()
        {
            return game;
        }

        public static void SetCurrentScene(string scene) { currentScene = scene; }

        public static string GetCurrentScene() { return currentScene; }        
        
        public static bool InLevelEditor()
        {
            return central != null;
        }

        public static bool InGame()
        {
            return game != null;
        }
    }

    //Called when we enter the main menu.
    [HarmonyPatch(typeof(MainMenuUI), "Awake")]
    public class TKMainMenuUIAwakePatch
    {
        public static void Prefix()
        {
            GameObserver.EnteredMainMenu?.Invoke();
        }
    }

    [HarmonyPatch(typeof(LEV_LevelEditorCentral), "Awake")]
    public class LevelEditorCentralAwakePatch
    {
        public static void Postfix(LEV_LevelEditorCentral __instance)
        {
            GameObserver.SetCentral(__instance);

            if(TeamXManager.IsTeamXEnabled())
            {
                GameObserver.EnteredLevelEditor?.Invoke();
                GameObserver.LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = (byte)CharacterMode.Build });

                if (GameObserver.GetCentral().cam.cameraTransform.gameObject.GetComponent<PlayerObserver>() == null)
                {
                    PlayerObserver observer = GameObserver.GetCentral().cam.cameraTransform.gameObject.AddComponent<PlayerObserver>();
                    observer.TransformChange += (data) => { GameObserver.LocalTransformChange?.Invoke(data); };
                }
            }           
        }
    }

    [HarmonyPatch(typeof(SetupGame), "Awake")]
    public class SetupGameAwakePatch
    {
        public static void Postfix(SetupGame __instance)
        {
            GameObserver.SetGame(__instance);

            if (TeamXManager.IsTeamXEnabled())
            {
                GameObserver.EnteredGame?.Invoke();
                GameObserver.LocalStateChange?.Invoke(new PlayerStateData() { playerID = -1, state = (byte)CharacterMode.Race });
            }
        }
    }

    [HarmonyPatch(typeof(GameMaster), "SpawnPlayers")]
    public class GameMasterSpawnPlayersPatch
    {
        public static void Postfix(GameMaster __instance)
        {
            if (TeamXManager.IsTeamXEnabled())
            {
                Transform localRacer = __instance.PlayersReady[0].transform;
                if (localRacer.gameObject.GetComponent<PlayerObserver>() == null)
                {
                    PlayerObserver observer = localRacer.gameObject.AddComponent<PlayerObserver>();
                    observer.TransformChange += (data) => { GameObserver.LocalTransformChange?.Invoke(data); };
                }
            }
        }
    }

    //Called when a players state changes
    [HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
    public class NewControlCarSetZeepkistStatePatch
    {
        public static void Prefix(ref byte newState, ref string source, ref bool playSound)
        {
            if (TeamXManager.IsTeamXEnabled())
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

    //Patch will call a function with the name of the scene that is going to be loaded.
    [HarmonyPatch(typeof(UnityEngine.SceneManagement.SceneManager), "LoadScene", new[] { typeof(string) })]
    public class SceneLoadPatch
    {
        public static void Prefix(ref string sceneName)
        {
            string currentScene = GameObserver.GetCurrentScene();
            if(currentScene == "LevelEditor2" && sceneName == "3D_MainMenu")
            {
                if (TeamXManager.IsTeamXEnabled())
                {
                    GameObserver.QuitLevelEditor?.Invoke();
                }
            }
            GameObserver.SetCurrentScene(sceneName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public static class TeamXManager
    {
        private static EventLogger eventLogger;
        public static Plugin plugin;
        private static bool IsTeamXMode = false;
        private static bool logging = true;

        public static void OnAwake(Plugin _plugin)
        {
            plugin = _plugin;

            //Initialize all classes
            TeamXConfiguration.Initialize();
            NetworkController.Initialize();
            SelectionObserver.Initialize();

            //Subscribe
            NetworkController.SubscribeToEvents();
            PlayerManagement.SubscribeToEvents();
            EditorState.SubscribeToEvents();
            EditorModifier.SubscribeToEvents();
            SelectionModifier.SubscribeToEvents();
            SubscribeToEvents();

            //Log all possible events (debug)
            eventLogger = new EventLogger();
            eventLogger.SubscribeToAllEvents();
        }

        public static void OnUpdate()
        {
            if(GameObserver.GetCurrentScene() == "3D_MainMenu" || IsTeamXEnabled())
            {
                NetworkController.Update();
            }

            if(IsTeamXEnabled())
            {
                SelectionObserver.Update();
            }
        }

        public static bool IsTeamXEnabled()
        {
            return IsTeamXMode;
        }

        public static void Log(string message, int level = 0)
        {
            if (!logging) { return; }

            switch(level)
            {
                default:
                case 0:
                    Debug.Log(message); 
                    break;
                case 1:
                    Debug.LogWarning(message); 
                    break;
                case 2:
                    Debug.LogError(message); 
                    break;
            }
        }

        public static void SubscribeToEvents()
        {
            //When the game shuts down make sure we disconnected from the server
            GameObserver.ApplicationQuit += () =>
            {
                if (IsTeamXEnabled())
                {
                    NetworkController.Disconnect();
                }
            };

            //When we come back from the editor into the main menu, disconnect from the server
            GameObserver.QuitLevelEditor += () =>
            {
                NetworkController.Disconnect();                    
            };

            //When we enter the main menu
            GameObserver.EnteredMainMenu += () =>
            {
                //Create the blue teamkist button to join the level editor.
                TeamXUserInterface.GenerateLevelEditorOnlineButton();
                //As we are in the main menu we are not in the teamkist editor
                IsTeamXMode = false;
                //Clear the local storage
                EditorState.Clear();

                //When the teamkist button is pressed, try to connect to the server. This is placed here because when the button is generated it removed all subscribers from itself.
                TeamXUserInterface.TeamkistButtonPressed += () =>
                {
                    //Required by Zeepkist
                    PlayerManager.Instance.weLoadedLevelEditorFromMainMenu = true;

                    //Get address from configuration.
                    string ip = TeamXConfiguration.GetIPAddress();
                    int port = TeamXConfiguration.GetPort();

                    //Connect to address
                    NetworkController.ConnectToServer(ip, port);
                };
            };            

            //When succesfully connected, we automatically log in with our data and receive the Server Data
            NetworkController.ServerDataEvent += (serverData) =>
            {
                EditorState.SetState(serverData);

                //We received the current state of the editor, go into the level editor
                IsTeamXMode = true;
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor2");
            };

            //When we enter the level editor:
            GameObserver.EnteredLevelEditor += () =>
            {
                //Load the editor state
                EditorModifier.LoadEditorState(EditorState.GetState());

                TeamXUserInterface.DisableLoadButton();

                if (!GameObserver.GetCentral().testMap.GlobalLevel.IsTestLevel)
                {
                    return;
                }

                GameObserver.GetCentral().testMap.GlobalLevel.IsTestLevel = false;
                GameObserver.GetCentral().manager.unsavedContent = false;


                if (GameObserver.GetCentral().manager.weLoadedLevelEditorFromMainMenu)
                {
                    return;
                }

                GameObserver.GetCentral().undoRedo.historyList = GameObserver.GetCentral().manager.tempUndoList;
            };
        }
    }
}

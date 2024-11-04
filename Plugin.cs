using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace TeamX
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.0";

        private void Awake()
        {
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();
            TeamXManager.OnAwake(this);
        }

        private void Update()
        {
            TeamXManager.OnUpdate();
        }

        private void OnApplicationQuit()
        {
            GameObserver.ApplicationQuit?.Invoke();
        }
    }
}

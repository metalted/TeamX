using BepInEx.Configuration;
using System;

namespace TeamX
{
    public static class TeamXConfiguration
    {
        public static ConfigEntry<bool> showPlayers;
        public static ConfigEntry<string> serverIP;
        public static ConfigEntry<int> port;
        public static Action ConfigReloaded;

        public static void Initialize()
        {
            showPlayers = TeamXManager.plugin.Config.Bind("Settings", "Show players", true, "Show other players in the level editor.");
            serverIP = TeamXManager.plugin.Config.Bind("Settings", "ServerIP", "127.0.0.1", "The IP address of the Teamkist server.");
            port = TeamXManager.plugin.Config.Bind("Settings", "Port", 8082, "The port of the Teamkist Server");
            TeamXManager.plugin.Config.SettingChanged += Config_SettingChanged;
        }

        private static void Config_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            //Do something with the config.
            ConfigReloaded?.Invoke();
        }

        public static string GetIPAddress()
        {
            return serverIP.Value;
        }

        public static int GetPort()
        {
            return port.Value;
        }
    }
}

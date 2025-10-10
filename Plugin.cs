using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;

namespace TreasureMapOverhaul
{
    [BepInPlugin("et508.erenshor.treasuremapoverhaul", "Treasure Map Overhaul", "1.0.5")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        public static ConfigEntry<float> TreasureMapDropChancePercent;

        private void Awake()
        {
            TreasureMapDropChancePercent = Config.Bind(
                "Drop Chance",
                "TreasureMapDropChance",
                1.25f,
                new ConfigDescription(
                    "Chance to drop A Torn Treasure Map (0.0-100%). Default: 2.0%. Reload scene or wait for new respawns for changes to apply.",
                    new AcceptableValueRange<float>(0f, 100f)
                ));
            
            Log = Logger;
            Log.LogInfo("Treasure Map Overhaul loaded.");
            
            var harmony = new Harmony("et508.erenshor.treasuremapoverhaul");
            harmony.PatchAll();
        }
        
        public static float GetNormalizedChance(ConfigEntry<float> entry)
        {
            return Mathf.Clamp01(entry.Value / 100f);
        }
    }
}

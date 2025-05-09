using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [BepInPlugin("et508.erenshor.treasuremapoverhaul", "Treasure Map Overhaul", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public static ConfigEntry<float> TreasureMapDropChancePercent;

        private void Awake()
        {
            TreasureMapDropChancePercent = TreasureMapDropChancePercent = Config.Bind(
                "Drop Chance",
                "TreasureMapDropChance",
                0.4f, // Default 0.4% (game default 0.004)
                new ConfigDescription(
                    "Chance to drop A Torn Treasure Map (0.0-100%). Default: 0.4%. Reload scene or wait for new respawns for changes to apply.",
                    new AcceptableValueRange<float>(0f, 100f)
                ));

            Log = Logger;
            Log.LogInfo("Treasure Map Overhaul loaded.");
            var harmony = new Harmony("et508.erenshor.treasuremapoverhaul");
            harmony.PatchAll();
        }

        // Helper to normalize percentage values (0-100) to 0.0-1.0 range
        public static float GetNormalizedChance(ConfigEntry<float> entry)
        {
            return Mathf.Clamp01(entry.Value / 100f);
        }
    }
}
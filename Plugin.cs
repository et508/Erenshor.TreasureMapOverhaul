using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;

namespace TreasureMapOverhaul
{
    [BepInPlugin("et508.erenshor.treasuremapoverhaul", "Treasure Map Overhaul", "1.0.3")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        public static ConfigEntry<float> TreasureMapDropChancePercent;

        // Dedicated error log
        private static StreamWriter errorLog;
        private static string errorLogPath;

        private void Awake()
        {
            // Bind config for drop chance (percentage 0-100)
            TreasureMapDropChancePercent = Config.Bind(
                "Drop Chance",
                "TreasureMapDropChance",
                2.0f, // Default 2%
                new ConfigDescription(
                    "Chance to drop A Torn Treasure Map (0.0-100%). Default: 2.0%. Reload scene or wait for new respawns for changes to apply.",
                    new AcceptableValueRange<float>(0f, 100f)
                ));

            // Initialize BepInEx logger
            Log = Logger;
            Log.LogInfo("Treasure Map Overhaul loaded.");

                        // Prepare error log path in the plugin folder (file created on first error)
            try
            {
                var pluginDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                errorLogPath = Path.Combine(pluginDir, "TreasureMapOverhaul_errors.log");
            }
            catch (Exception ex)
            {
                Log.LogError($"[TMO] Failed to prepare error log path: {ex.Message}");
            }

            // Subscribe to Unity log messages
            Application.logMessageReceived += HandleLog;

            // Apply Harmony patches
            var harmony = new Harmony("et508.erenshor.treasuremapoverhaul");
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            // Unsubscribe and close error log
            Application.logMessageReceived -= HandleLog;
            errorLog?.Close();
        }

        // Capture only our mod's errors/exceptions to a separate log file
        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception)
                return;

            // Filter to our mod messages
            if (!(condition.Contains("[TMO]") || stackTrace.Contains("TreasureMapOverhaul")))
                return;

            // Lazily create error log file on first error
            if (errorLog == null)
            {
                try
                {
                    errorLog = new StreamWriter(errorLogPath, true) { AutoFlush = true };
                    Log.LogInfo($"[TMO] Error log file initialized at {errorLogPath}");
                }
                catch (Exception ex)
                {
                    Log.LogError($"[TMO] Failed to create error log file: {ex.Message}");
                    return;
                }
            }

            // Write error details
            try
            {
                errorLog.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {condition}");
                errorLog.WriteLine(stackTrace);
                errorLog.WriteLine();
            }
            catch
            {
                // ignore write failures
            }
        }

        // Helper to normalize config percentage (0-100) to 0.0-1.0
        public static float GetNormalizedChance(ConfigEntry<float> entry)
        {
            return Mathf.Clamp01(entry.Value / 100f);
        }
    }
}

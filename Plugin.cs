using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;

namespace TreasureMapOverhaul
{
    [BepInPlugin("et508.erenshor.treasuremapoverhaul", "Treasure Map Overhaul", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Treasure Map Overhaul loaded.");
            var harmony = new Harmony("et508.erenshor.treasuremapoverhaul");
            harmony.PatchAll();
            Log.LogInfo($"[TMO] Total patched methods: {harmony.GetPatchedMethods().Count()}");
        }
    }
}
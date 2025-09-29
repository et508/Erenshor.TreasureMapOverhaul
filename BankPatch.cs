using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(GlobalBank), "LoadBank")]
    public static class BankPatch
    {
        // Old vanilla map object names to convert
        private static readonly string[] OldMapNames =
        {
            "GEN - Torn Map Top Left",
            "GEN - Torn Map Top Right",
            "GEN - Torn Map Bottom Left",
            "GEN - Torn Map Bottom Right"
        };

        // Your new Torn Map object name (as authored in your assetbundle)
        private const string NewTornMapName = "GEN - A Torn Map";

        [HarmonyPostfix]
        public static void OnBankLoad(GlobalBank __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] BankPatch: GlobalBank instance is null.");
                    return;
                }

                if (__instance.StoredItems == null || __instance.Quantities == null)
                {
                    Plugin.Log.LogError("[TMO] BankPatch: StoredItems or Quantities is null.");
                    return;
                }

                // Resolve new map by OBJECT NAME in the live ItemDB
                var newMap = GameData.ItemDB?.ItemDB?.FirstOrDefault(i => i != null && i.name == NewTornMapName);
                if (newMap == null)
                {
                    Plugin.Log.LogWarning($"[TMO] BankPatch: '{NewTornMapName}' not found in ItemDB; skipping bank conversion.");
                    return;
                }

                // Convert all pages in StoredItems/Quantities (NOT just visible slots)
                int totalConverted = 0;
                int n = Math.Min(__instance.StoredItems.Count, __instance.Quantities.Count);

                for (int i = 0; i < n; i++)
                {
                    var it = __instance.StoredItems[i];
                    if (it != null && OldMapNames.Contains(it.name))
                    {
                        // accumulate quantity and replace with new map
                        totalConverted += Math.Max(0, __instance.Quantities[i]);
                        __instance.StoredItems[i] = newMap;
                        // keep the same quantity in this slot
                        // (we’re converting in place; stacking happens naturally if identical items match)
                    }
                }

                if (totalConverted > 0)
                {
                    Plugin.Log.LogInfo($"[TMO] BankPatch: Converted {totalConverted} old map(s) to '{NewTornMapName}' in bank.");

                    // Refresh bank UI to reflect StoredItems/Quantities
                    try { __instance.DisplayBankPage(); }
                    catch (Exception uiEx)
                    {
                        Plugin.Log.LogWarning($"[TMO] BankPatch: UI refresh failed after convert: {uiEx}");
                    }

                    // IMPORTANT: Persist immediately so closing/reopening bank uses the converted data
                    try { __instance.SaveBank(); }
                    catch (Exception saveEx)
                    {
                        Plugin.Log.LogError($"[TMO] BankPatch: SaveBank failed after convert: {saveEx}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] BankPatch: Exception: {ex}");
            }
        }
    }
}

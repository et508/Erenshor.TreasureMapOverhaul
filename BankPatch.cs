using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(GlobalBank), "LoadBank")]
    public static class BankPatch
    {
        private static readonly string[] OldMapNames =
        {
            "GEN - Torn Map Top Left",
            "GEN - Torn Map Top Right",
            "GEN - Torn Map Bottom Left",
            "GEN - Torn Map Bottom Right"
        };
        
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
                
                var newMap = GameData.ItemDB?.ItemDB?.FirstOrDefault(i => i != null && i.name == NewTornMapName);
                if (newMap == null)
                {
                    Plugin.Log.LogWarning($"[TMO] BankPatch: '{NewTornMapName}' not found in ItemDB; skipping bank conversion.");
                    return;
                }
                
                int totalConverted = 0;
                int n = Math.Min(__instance.StoredItems.Count, __instance.Quantities.Count);

                for (int i = 0; i < n; i++)
                {
                    var it = __instance.StoredItems[i];
                    if (it != null && OldMapNames.Contains(it.name))
                    {
                        totalConverted += Math.Max(0, __instance.Quantities[i]);
                        __instance.StoredItems[i] = newMap;
                    }
                }

                if (totalConverted > 0)
                {
                    try { __instance.DisplayBankPage(); }
                    catch (Exception uiEx)
                    {
                        Plugin.Log.LogWarning($"[TMO] BankPatch: UI refresh failed after convert: {uiEx}");
                    }
                    
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

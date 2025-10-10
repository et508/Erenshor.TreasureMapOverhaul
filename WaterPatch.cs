using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(Water), "Start")]
    public static class WaterPatch
    {
        private const string OldFishableName = "GEN - Torn Map Top Right";
        
        private const string NewTornMapName = "GEN - A Torn Map";

        [HarmonyPostfix]
        public static void OnWaterStart(Water __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] WaterPatch: Water instance is null.");
                    return;
                }
                
                var tornMap = GameData.ItemDB?.ItemDB
                    ?.FirstOrDefault(i => i != null && i.name == NewTornMapName);

                if (tornMap == null)
                {
                    Plugin.Log.LogWarning($"[TMO] WaterPatch: Torn Map '{NewTornMapName}' not found in ItemDB; skipping fishable replacement.");
                    return;
                }

                int total = 0;
                total += ReplaceInList(__instance.DayFishables, tornMap);
                total += ReplaceInList(__instance.NightFishables, tornMap);
                total += ReplaceInList(__instance.Fishables, tornMap);

                if (total > 0)
                    Plugin.Log.LogInfo($"[TMO] WaterPatch: Replaced {total} old fishable map entries with Torn Map '{NewTornMapName}'.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] WaterPatch exception: {ex}");
            }
        }

        private static int ReplaceInList(List<Item> list, Item tornMap)
        {
            if (list == null) return 0;

            int replaced = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i];
                if (it != null && it.name == OldFishableName)
                {
                    list[i] = tornMap;
                    replaced++;
                }
            }
            return replaced;
        }
    }
}

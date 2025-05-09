using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreasureMapOverhaul
{
    // Patch Water.Start to replace old fishable maps with the new torn map
    [HarmonyPatch(typeof(Water), "Start")]
    public static class WaterPatch
    {
        [HarmonyPostfix]
        public static void OnWaterStart(Water __instance)
        {
            // Plugin.Log.LogInfo("[TMO] Water.Start postfix fired. Patching fishable items.");

            // ID of old map to replace
            string oldFishableId = "28043030";
            // Get the new torn map item
            var tornMap = GameData.ItemDB.GetItemByID("et508.tornmap");
            if (tornMap == null)
            {
                Plugin.Log.LogError("[TMO] Torn map not found in ItemDB! Fishable patch aborted.");
                return;
            }

            // Helper to replace in a list
            void ReplaceInList(List<Item> list, string listName)
            {
                if (list == null) return;
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item != null && item.Id == oldFishableId)
                    {
                        list[i] = tornMap;
                        // Plugin.Log.LogInfo($"[TMO] Replaced fishable map in {listName}[{i}] from {oldFishableId} to {tornMap.Id}.");
                    }
                }
            }

            // Replace in DayFishables, NightFishables, and active Fishables lists
            ReplaceInList(__instance.DayFishables, "DayFishables");
            ReplaceInList(__instance.NightFishables, "NightFishables");
            ReplaceInList(__instance.Fishables, "Fishables");
        }
    }
}
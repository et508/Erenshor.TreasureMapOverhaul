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
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] WaterPatch failed: Water instance is null.");
                    return;
                }

                // ID of old map to replace
                const string oldFishableId = "28043030";

                // Get the new torn map item
                var tornMap = GameData.ItemDB?.GetItemByID("et508.tornmap");
                if (tornMap == null)
                {
                    Plugin.Log.LogError("[TMO] Torn map not found in ItemDB! Fishable patch aborted.");
                    return;
                }

                // Helper to replace in a list
                void ReplaceInList(List<Item> list, string listName)
                {
                    if (list == null)
                    {
                        Plugin.Log.LogError($"[TMO] {listName} list is null in WaterPatch.");
                        return;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (item != null && item.Id == oldFishableId)
                        {
                            list[i] = tornMap;
                        }
                    }
                }

                // Replace in DayFishables, NightFishables, and active Fishables lists
                ReplaceInList(__instance.DayFishables, "DayFishables");
                ReplaceInList(__instance.NightFishables, "NightFishables");
                ReplaceInList(__instance.Fishables, "Fishables");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in WaterPatch.OnWaterStart: {ex}");
            }
        }
    }
}

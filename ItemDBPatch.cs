using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(ItemDatabase), "Start")]
    public static class ItemDBPatch
    {
        [HarmonyPostfix]
        public static void InjectCustomItem(ItemDatabase __instance)
        {
            try
            {
                // Ensure the database instance and array are valid
                if (__instance == null || __instance.ItemDB == null)
                {
                    Plugin.Log.LogError("[TMO] ItemDatabase instance or ItemDB array is null in InjectCustomItem.");
                    return;
                }

                // Convert to List for easy modification
                List<Item> itemList = __instance.ItemDB.ToList();

                // Specify the base item ID to clone
                const string baseItemId = "28362792";

                // Look up base map
                var baseMap = __instance.GetItemByID(baseItemId);
                if (baseMap == null)
                {
                    Plugin.Log.LogError($"[TMO] Base item with ID '{baseItemId}' not found in ItemDatabase.");
                    return;
                }

                // Clone and configure new item
                var tornMap = UnityEngine.Object.Instantiate(baseMap);
                if (tornMap == null)
                {
                    Plugin.Log.LogError("[TMO] Failed to instantiate clone of base map item.");
                    return;
                }

                // Assign properties
                tornMap.Id = "et508.tornmap";
                tornMap.ItemName = "A Torn Treasure Map";
                tornMap.Lore = "A piece of an old map.";
                tornMap.Stackable = true;
                tornMap.Unique = true;
                tornMap.ItemValue = 0;
                tornMap.RequiredSlot = Item.SlotType.General;

                // Add to list and replace database
                itemList.Add(tornMap);
                __instance.ItemDB = itemList.ToArray();
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors
                Plugin.Log.LogError($"[TMO] Exception in ItemDBPatch.InjectCustomItem: {ex}");
            }
        }
    }
}

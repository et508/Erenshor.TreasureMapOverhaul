using HarmonyLib;
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
            if (__instance?.ItemDB == null)
            {
                Plugin.Log.LogWarning("ItemDatabase is null.");
                return;
            }

            // Convert to List for easy modification
            List<Item> itemList = __instance.ItemDB.ToList();

            // Specify the base item ID to clone
            const string baseItemId = "28362792";

            var baseMap = __instance.GetItemByID(baseItemId);
            if (baseMap == null)
            {
                Plugin.Log.LogWarning($"Base item with ID '{baseItemId}' not found.");
                return;
            }

            // Clone and configure new item
            var tornMap = Object.Instantiate(baseMap);
            tornMap.Id = "et508.tornmap";              // Internal ID
            tornMap.ItemName = "A Torn Treasure Map";  // Display name
            tornMap.Lore = "A tattered piece of parchment with strange markings.";
            tornMap.Stackable = true;
            tornMap.Unique = true;
            tornMap.ItemValue = 0;
            tornMap.RequiredSlot = Item.SlotType.General;

            itemList.Add(tornMap);

            // Replace the ItemDB array
            __instance.ItemDB = itemList.ToArray();

            Plugin.Log.LogInfo("Injected Torn Treasure Map using list-based method.");
        }
    }
}
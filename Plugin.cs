using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [BepInPlugin("et508.erenshor.treasuremapoverhaul", "Treasure Map Overhaul", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("Treasure Map Overhaul loaded.");
            Harmony harmony = new Harmony("et508.erenshor.treasuremapoverhaul");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(ItemDatabase), "Start")]
    public static class Patch_ItemDatabase
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

            // Clone a base item
            const string baseItemId = "28362792";  
            
            var baseMap = __instance.GetItemByID(baseItemId);
            if (baseMap == null)
            {
                Plugin.Log.LogWarning($"Base item with ID '{baseItemId}' not found.");
                return;
            }
            
            var tornMap = Object.Instantiate(baseMap);
            tornMap.Id = "et508.tornmap"; // internal asset name
            tornMap.ItemName = "A Torn Treasure Map";
            tornMap.Lore = "A tattered piece of parchment with strange markings.";
            tornMap.Stackable = true;
            tornMap.Unique = true;
            tornMap.ItemValue = 0;
            tornMap.RequiredSlot = Item.SlotType.General;

            itemList.Add(tornMap);

            // Replace ItemDB with updated list
            __instance.ItemDB = itemList.ToArray();

            Plugin.Log.LogInfo("Injected Torn Treasure Map using list-based method.");
        }
    }
}

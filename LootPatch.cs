using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(LootTable), "InitLootTable")]
    public static class LootInitPatch
    {
        [HarmonyPostfix]
        public static void OnInitLoot(LootTable __instance)
        {
            
            string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
            __instance.ActualDrops.RemoveAll(item => item != null && System.Array.IndexOf(oldIds, item.Id) >= 0);
            
            var tornMap = GameData.ItemDB.GetItemByID("et508.tornmap");
            if (tornMap == null)
            {
                Plugin.Log.LogError("[TMO] Torn map not found in ItemDB!");
                return;
            }

            float dropChance = 0.01f; 
            if (!__instance.ActualDrops.Contains(tornMap) && Random.value < dropChance)
            {
                __instance.ActualDrops.Add(tornMap);
                __instance.special = true;
            }
        }
    }
}
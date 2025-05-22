using System;
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
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] LootInitPatch: LootTable instance is null.");
                    return;
                }
                if (__instance.ActualDrops == null)
                {
                    Plugin.Log.LogError("[TMO] LootInitPatch: ActualDrops list is null.");
                    return;
                }

                // Remove any old map drops
                string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
                __instance.ActualDrops.RemoveAll(item => item != null && Array.IndexOf(oldIds, item.Id) >= 0);

                // Get the new torn map
                var tornMap = GameData.ItemDB?.GetItemByID("et508.tornmap");
                if (tornMap == null)
                {
                    Plugin.Log.LogError("[TMO] LootInitPatch: Torn map not found in ItemDB.");
                    return;
                }

                // Determine drop chance from config
                float dropChance = Plugin.GetNormalizedChance(Plugin.TreasureMapDropChancePercent);

                // Inject torn map based on configured chance
                if (!__instance.ActualDrops.Contains(tornMap) && UnityEngine.Random.value < dropChance)
                {
                    __instance.ActualDrops.Add(tornMap);
                    __instance.special = true;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in LootInitPatch.OnInitLoot: {ex}");
            }
        }
    }
}
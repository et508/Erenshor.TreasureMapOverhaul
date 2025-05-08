using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(GlobalBank), "LoadBank")]
    public static class BankPatch
    {
        [HarmonyPostfix]
        public static void OnBankLoad(GlobalBank __instance)
        {
            // Plugin.Log.LogInfo("[TMO] GlobalBank.LoadBank postfix fired.");
            ReplaceOldAndAddBank(__instance);
            // Refresh bank UI after modifications
            __instance.DisplayBankPage();
        }

        private static void ReplaceOldAndAddBank(GlobalBank bank)
        {
            string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
            int totalCount = 0;

            // Count and remove old map items
            for (int i = 0; i < bank.StoredItems.Count; i++)
            {
                var item = bank.StoredItems[i];
                if (item != null && oldIds.Contains(item.Id))
                {
                    totalCount += bank.Quantities[i];
                    // Plugin.Log.LogInfo($"[TMO] Removing old map {item.Id} x{bank.Quantities[i]} from bank slot {i}.");
                    bank.StoredItems[i] = GameData.PlayerInv.Empty;
                    bank.Quantities[i] = 0;
                }
            }

            if (totalCount <= 0)
                return;

            // Inject stacked torn maps
            var torn = GameData.ItemDB.GetItemByID("et508.tornmap");
            if (torn == null)
            {
                Plugin.Log.LogError("[TMO] Torn map not found in DB!");
                return;
            }

            // Try to stack into an existing torn map slot
            int stackSlot = bank.StoredItems.FindIndex(x => x == torn);
            if (stackSlot >= 0)
            {
                bank.Quantities[stackSlot] += totalCount;
                // Plugin.Log.LogInfo($"[TMO] Stacked {totalCount} Torn Treasure Map(s) into slot {stackSlot} (new qty {bank.Quantities[stackSlot]}).");
            }
            else
            {
                // Otherwise, add to first empty slot
                int emptySlot = bank.StoredItems.FindIndex(x => x == GameData.PlayerInv.Empty);
                if (emptySlot >= 0)
                {
                    bank.StoredItems[emptySlot] = torn;
                    bank.Quantities[emptySlot] = totalCount;
                    // Plugin.Log.LogInfo($"[TMO] Added {totalCount} Torn Treasure Map(s) to empty bank slot {emptySlot}.");
                }
                else
                {
                    Plugin.Log.LogWarning("[TMO] No empty bank slot available to add torn maps.");
                }
            }
        }
    }
}

using BepInEx.Logging;
using HarmonyLib;
using System;
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
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] GlobalBank instance is null in BankPatch.OnBankLoad.");
                    return;
                }

                ReplaceOldAndAddBank(__instance);

                // Refresh bank UI after modifications
                try
                {
                    __instance.DisplayBankPage();
                }
                catch (Exception uiEx)
                {
                    Plugin.Log.LogError($"[TMO] Failed to refresh bank UI: {uiEx}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in BankPatch.OnBankLoad: {ex}");
            }
        }

        private static void ReplaceOldAndAddBank(GlobalBank bank)
        {
            try
            {
                if (bank.StoredItems == null || bank.Quantities == null)
                {
                    Plugin.Log.LogError("[TMO] StoredItems or Quantities is null in BankPatch.");
                    return;
                }

                string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
                int totalCount = 0;

                // Count and remove old map items
                for (int i = 0; i < bank.StoredItems.Count; i++)
                {
                    var item = bank.StoredItems[i];
                    if (item != null && Array.IndexOf(oldIds, item.Id) >= 0)
                    {
                        totalCount += bank.Quantities[i];
                        bank.StoredItems[i] = GameData.PlayerInv.Empty;
                        bank.Quantities[i] = 0;
                    }
                }

                if (totalCount <= 0)
                {
                    return;
                }

                // Inject stacked torn maps
                var torn = GameData.ItemDB?.GetItemByID("et508.tornmap");
                if (torn == null)
                {
                    Plugin.Log.LogError("[TMO] Torn map not found in ItemDB in BankPatch.");
                    return;
                }

                // Try to stack into an existing torn map slot
                int stackSlot = bank.StoredItems.FindIndex(x => x == torn);
                if (stackSlot >= 0)
                {
                    bank.Quantities[stackSlot] += totalCount;
                }
                else
                {
                    // Otherwise, add to first empty slot
                    int emptySlot = bank.StoredItems.FindIndex(x => x == GameData.PlayerInv.Empty);
                    if (emptySlot >= 0)
                    {
                        bank.StoredItems[emptySlot] = torn;
                        bank.Quantities[emptySlot] = totalCount;
                    }
                    else
                    {
                        Plugin.Log.LogError("[TMO] No empty bank slot available to add torn maps in BankPatch.");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in BankPatch.ReplaceOldAndAddBank: {ex}");
            }
        }
    }
}

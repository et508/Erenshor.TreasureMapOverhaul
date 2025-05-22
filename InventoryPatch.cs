using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class InventoryPatch
    {
        [HarmonyPostfix]
        public static void OnCharacterEnter()
        {
            try
            {
                var inv = GameData.PlayerInv;
                if (inv == null)
                {
                    Plugin.Log.LogError("[TMO] PlayerInv is null in InventoryPatch.OnCharacterEnter.");
                    return;
                }
                if (inv.StoredSlots == null)
                {
                    Plugin.Log.LogError("[TMO] StoredSlots is null in InventoryPatch.OnCharacterEnter.");
                    return;
                }

                ReplaceOldAndAdd(inv);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in InventoryPatch.OnCharacterEnter: {ex}");
            }
        }

        private static void ReplaceOldAndAdd(Inventory inv)
        {
            try
            {
                string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
                int total = 0;

                var slots = inv.StoredSlots; // List<ItemIcon>
                if (slots == null)
                {
                    Plugin.Log.LogError("[TMO] StoredSlots is null in InventoryPatch.ReplaceOldAndAdd.");
                    return;
                }

                // Count and remove old maps
                foreach (var icon in slots)
                {
                    var item = icon.MyItem;
                    if (item != null && Array.IndexOf(oldIds, item.Id) >= 0)
                    {
                        total += icon.Quantity;
                        icon.MyItem = inv.Empty;
                        icon.Quantity = 1;
                        icon.UpdateSlotImage();
                    }
                }

                if (total == 0)
                    return;

                var torn = GameData.ItemDB?.GetItemByID("et508.tornmap");
                if (torn == null)
                {
                    Plugin.Log.LogError("[TMO] Torn map not found in ItemDB in ReplaceOldAndAdd.");
                    return;
                }

                // Add torn maps
                for (int i = 0; i < total; i++)
                {
                    bool added = inv.AddItemToInv(torn);
                    if (!added)
                        Plugin.Log.LogError($"[TMO] Failed to add torn map instance {i + 1}/{total} in ReplaceOldAndAdd.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in InventoryPatch.ReplaceOldAndAdd: {ex}");
            }
        }
    }
}

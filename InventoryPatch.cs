using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class InventoryPatch
    {
        [HarmonyPostfix]
        public static void OnCharacterEnter()
        {
            // 1) sanity check plugin loaded
            // Plugin.Log.LogInfo("[TMO] CharSelectManager.Play postfix fired.");

            var inv = GameData.PlayerInv;
            if (inv == null)
            {
                Plugin.Log.LogWarning("[TMO] PlayerInv is null. Inventory not loaded.");
                return;
            }

            // 2) dump the first few slots so we know what's actually in the inventory at this point
            for (int i = 0; i < Mathf.Min(5, inv.StoredSlots.Count); i++)
            {
                var item = inv.StoredSlots[i].MyItem;
                // Plugin.Log.LogInfo($"[TMO] Slot {i}: {(item != null ? item.Id : "EMPTY")} x{inv.StoredSlots[i].Quantity}");
            }

            // 3) now do the replace/add logic
            ReplaceOldAndAdd(inv);
        }

        private static void ReplaceOldAndAdd(Inventory inv)
        {
            string[] oldIds = { "28362792", "270986", "6188236", "28043030" };
            int total = 0;

            foreach (var slot in inv.StoredSlots)
            {
                var item = slot.MyItem;
                if (item != null && oldIds.Contains(item.Id))
                {
                    total += slot.Quantity;
                    // Plugin.Log.LogInfo($"[TMO] Removing old map {item.Id} x{slot.Quantity} from slot.");
                    slot.MyItem = inv.Empty;
                    slot.Quantity = 1;
                    slot.UpdateSlotImage();
                }
            }

            if (total == 0)
            {
                // Plugin.Log.LogInfo("[TMO] No old maps found to replace.");
                return;
            }

            var torn = GameData.ItemDB.GetItemByID("et508.tornmap");
            if (torn == null)
            {
                Plugin.Log.LogError("[TMO] Torn map not found in ItemDB!");
                return;
            }

            for (int i = 0; i < total; i++)
                inv.AddItemToInv(torn);

            // Plugin.Log.LogInfo($"[TMO] Added {total} Torn Treasure Map(s).");
        }
    }
}

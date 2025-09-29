using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    // Convert any pre-existing old map fragments in the player's inventory
    // into the new Torn Map when the character enters the world.
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class InventoryPatch
    {
        // Old (vanilla) fishable/lootable map object names we want to convert
        private static readonly string[] OldMapNames =
        {
            "GEN - Torn Map Top Left",
            "GEN - Torn Map Top Right",
            "GEN - Torn Map Bottom Left",
            "GEN - Torn Map Bottom Right"
        };

        // Your new Torn Map object name as authored in the assetbundle
        private const string NewTornMapName = "GEN - A Torn Map";

        [HarmonyPostfix]
        public static void OnCharacterEnter()
        {
            try
            {
                var inv = GameData.PlayerInv;
                if (inv == null)
                {
                    Plugin.Log.LogError("[TMO] InventoryPatch: PlayerInv is null.");
                    return;
                }

                // Find the new item by OBJECT NAME in the DB (must already be registered by your ItemDB patch)
                var torn = GameData.ItemDB?.ItemDB?.FirstOrDefault(i => i != null && i.name == NewTornMapName);
                if (torn == null)
                {
                    Plugin.Log.LogWarning($"[TMO] InventoryPatch: New map '{NewTornMapName}' not found in ItemDB; skipping migration.");
                    return;
                }

                var slots = inv.StoredSlots; // List<ItemIcon>
                if (slots == null)
                {
                    Plugin.Log.LogError("[TMO] InventoryPatch: StoredSlots is null.");
                    return;
                }

                int toConvertTotal = 0;

                // Pass 1: count and clear old maps by OBJECT NAME
                for (int s = 0; s < slots.Count; s++)
                {
                    var icon = slots[s];
                    if (icon == null) continue;

                    var item = icon.MyItem;
                    if (item != null && OldMapNames.Contains(item.name))
                    {
                        // accumulate full stack
                        toConvertTotal += Math.Max(1, icon.Quantity);

                        // clear the slot
                        icon.MyItem = inv.Empty;
                        icon.Quantity = 1; // many inventories treat Empty as quantity 1 placeholder
                        icon.UpdateSlotImage();
                    }
                }

                if (toConvertTotal <= 0)
                    return;

                // Pass 2: add the same total amount of the NEW torn map
                int added = 0;
                for (int i = 0; i < toConvertTotal; i++)
                {
                    bool ok = inv.AddItemToInv(torn);
                    if (ok) added++;
                }

                Plugin.Log.LogInfo($"[TMO] InventoryPatch: Converted {toConvertTotal} old map(s) → added {added} '{NewTornMapName}'.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] InventoryPatch exception: {ex}");
            }
        }
    }
}

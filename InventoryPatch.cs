using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class InventoryPatch
    {
        private static readonly string[] OldMapNames =
        {
            "GEN - Torn Map Top Left",
            "GEN - Torn Map Top Right",
            "GEN - Torn Map Bottom Left",
            "GEN - Torn Map Bottom Right"
        };
        
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
                
                for (int s = 0; s < slots.Count; s++)
                {
                    var icon = slots[s];
                    if (icon == null) continue;

                    var item = icon.MyItem;
                    if (item != null && OldMapNames.Contains(item.name))
                    {
                        toConvertTotal += Math.Max(1, icon.Quantity);
                        
                        icon.MyItem = inv.Empty;
                        icon.Quantity = 1; 
                        icon.UpdateSlotImage();
                    }
                }

                if (toConvertTotal <= 0)
                    return;
                
                int added = 0;
                for (int i = 0; i < toConvertTotal; i++)
                {
                    bool ok = inv.AddItemToInv(torn);
                    if (ok) added++;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] InventoryPatch exception: {ex}");
            }
        }
    }
}

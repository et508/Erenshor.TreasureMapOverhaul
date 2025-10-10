using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(ItemDatabase), "GetItemByID", new[] { typeof(string) })]
    public static class Bank_GetItemByID_Fallback
    {
        private const string TornMapId = "99999999";
        private const string PreferredAssetName = "GEN - A Torn Map";

        public static void Postfix(ItemDatabase __instance, string __0, ref Item __result)
        {
            try
            {
                if (__0 != TornMapId) return;
                if (__result != null && !IsEmpty(__result)) return;

                if (!TmoAssets.EnsureLoaded()) return;
                
                Item torn = null;
                if (!string.IsNullOrWhiteSpace(PreferredAssetName))
                    torn = TmoAssets.Load<Item>(PreferredAssetName);

                if (torn == null)
                    torn = TmoAssets.LoadAll<Item>().FirstOrDefault(i => i != null && i.Id == TornMapId);

                if (torn == null) return;

                torn.Id = TornMapId;
                
                var list = (__instance.ItemDB ?? Array.Empty<Item>()).ToList();
                list.RemoveAll(i => i != null && i.Id == TornMapId);
                list.Add(torn);
                __instance.ItemDB = list.ToArray();

                __result = torn;
                
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] BankFallback: Exception {ex}");
            }
        }

        private static bool IsEmpty(Item item)
        {
            try
            {
                return ReferenceEquals(item, GameData.PlayerInv.Empty)
                    || string.Equals(item.ItemName, "Empty", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }
}

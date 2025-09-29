using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    // Ensures the bank loader can resolve our Torn Map ID ("99999999")
    [HarmonyPatch(typeof(ItemDatabase), "GetItemByID", new[] { typeof(string) })]
    public static class Bank_GetItemByID_Fallback
    {
        private const string TornMapId = "99999999";
        private const string PreferredAssetName = "GEN - A Torn Map"; // optional object name in bundle

        public static void Postfix(ItemDatabase __instance, string __0, ref Item __result)
        {
            try
            {
                if (__0 != TornMapId) return;
                if (__result != null && !IsEmpty(__result)) return;

                if (!TmoAssets.EnsureLoaded()) return;

                // Find Torn Map asset in bundle
                Item torn = null;
                if (!string.IsNullOrWhiteSpace(PreferredAssetName))
                    torn = TmoAssets.Load<Item>(PreferredAssetName);

                if (torn == null)
                    torn = TmoAssets.LoadAll<Item>().FirstOrDefault(i => i != null && i.Id == TornMapId);

                if (torn == null) return;

                torn.Id = TornMapId;

                // Register in DB
                var list = (__instance.ItemDB ?? Array.Empty<Item>()).ToList();
                list.RemoveAll(i => i != null && i.Id == TornMapId);
                list.Add(torn);
                __instance.ItemDB = list.ToArray();

                __result = torn;

                Plugin.Log.LogInfo("[TMO] BankFallback: registered & returned Torn Map during bank load.");
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

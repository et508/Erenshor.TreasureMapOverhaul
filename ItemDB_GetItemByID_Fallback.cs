using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    internal static class TmoIds
    {
        internal const string TornMapId = "99999999";
        internal const string PreferredAssetName = ""; // optional exact asset name in tmo_assets
    }

    // Fires whenever the game resolves a string ID -> Item during load/use.
    [HarmonyPatch(typeof(ItemDatabase), "GetItemByID", new[] { typeof(string) })]
    public static class ItemDB_GetItemByID_Fallback
    {
        public static void Postfix(ItemDatabase __instance, string __0, ref Item __result)
        {
            try
            {
                // Only handle our custom ID
                if (!string.Equals(__0, TmoIds.TornMapId, StringComparison.Ordinal))
                    return;

                // If already resolved to a non-empty item, we're done
                if (__result != null && !IsEmpty(__result))
                    return;

                // Ensure our bundle is available
                if (!TmoAssets.EnsureLoaded())
                    return;

                // Load the exact asset instance (DO NOT Instantiate → preserve .name)
                Item src = null;
                if (!string.IsNullOrWhiteSpace(TmoIds.PreferredAssetName))
                    src = TmoAssets.Load<Item>(TmoIds.PreferredAssetName);
                if (src == null)
                    src = TmoAssets.LoadAll<Item>().FirstOrDefault(i => i != null && i.Id == TmoIds.TornMapId);

                if (src == null)
                    return;

                // Normalize ID
                src.Id = TmoIds.TornMapId;

                // Register/replace in DB so future lookups succeed
                var list = (__instance.ItemDB ?? Array.Empty<Item>()).ToList();
                list.RemoveAll(i => i != null && i.Id == TmoIds.TornMapId);
                list.Add(src);
                __instance.ItemDB = list.ToArray();

                // Hand back the real item to the caller (loader / inventory)
                __result = src;

                // Optional: keep GM.Maps coherent even on early load paths
                try
                {
                    if (GameData.GM?.Maps != null)
                    {
                        GameData.GM.Maps.Clear();
                        GameData.GM.Maps.Add(src);
                    }
                } catch { /* minimal */ }

                Plugin.Log.LogInfo("[TMO] GetItemByID fallback: registered & returned Torn Map during load.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[TMO] GetItemByID fallback failed: {ex}");
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

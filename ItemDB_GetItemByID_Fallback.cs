using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    internal static class TmoIds
    {
        internal const string TornMapId = "99999999";
        internal const string PreferredAssetName = "";
    }


    [HarmonyPatch(typeof(ItemDatabase), "GetItemByID", new[] { typeof(string) })]
    public static class ItemDB_GetItemByID_Fallback
    {
        public static void Postfix(ItemDatabase __instance, string __0, ref Item __result)
        {
            try
            {
                if (!string.Equals(__0, TmoIds.TornMapId, StringComparison.Ordinal))
                    return;
                
                if (__result != null && !IsEmpty(__result))
                    return;
                
                if (!TmoAssets.EnsureLoaded())
                    return;
                
                Item src = null;
                if (!string.IsNullOrWhiteSpace(TmoIds.PreferredAssetName))
                    src = TmoAssets.Load<Item>(TmoIds.PreferredAssetName);
                if (src == null)
                    src = TmoAssets.LoadAll<Item>().FirstOrDefault(i => i != null && i.Id == TmoIds.TornMapId);

                if (src == null)
                    return;
                
                src.Id = TmoIds.TornMapId;
                
                var list = (__instance.ItemDB ?? Array.Empty<Item>()).ToList();
                list.RemoveAll(i => i != null && i.Id == TmoIds.TornMapId);
                list.Add(src);
                __instance.ItemDB = list.ToArray();
                
                __result = src;
                
                try
                {
                    if (GameData.GM?.Maps != null)
                    {
                        GameData.GM.Maps.Clear();
                        GameData.GM.Maps.Add(src);
                    }
                } catch { /* minimal */ }
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

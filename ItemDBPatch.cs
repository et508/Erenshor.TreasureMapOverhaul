using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(ItemDatabase), "Start")]
    public static class ItemDBPatch
    {
        private const string TornMapId = "99999999";
        private const string PreferredAssetName = ""; // e.g. "GEN - A Torn Map"

        [HarmonyPostfix]
        public static void InjectCustomItem(ItemDatabase __instance)
        {
            try
            {
                if (__instance == null || __instance.ItemDB == null)
                {
                    Plugin.Log.LogError("[TMO] ItemDBPatch: ItemDatabase or ItemDB is null.");
                    return;
                }

                if (!TmoAssets.EnsureLoaded()) return;

                // Load the Item from bundle (no Instantiate -> keep exact Unity name, no "(Clone)")
                Item source = null;

                if (!string.IsNullOrWhiteSpace(PreferredAssetName))
                    source = TmoAssets.Load<Item>(PreferredAssetName);

                if (source == null)
                    source = TmoAssets.LoadAll<Item>().FirstOrDefault(i => i != null && i.Id == TornMapId);

                if (source == null)
                {
                    Plugin.Log.LogError("[TMO] Could not find custom Item in bundle (set PreferredAssetName or ensure Id == 99999999).");
                    return;
                }

                // Normalize Id just in case
                source.Id = TornMapId;

                // Replace/add in ItemDB with the exact asset instance
                var list = __instance.ItemDB.ToList();
                list.RemoveAll(i => i != null && i.Id == TornMapId);
                list.Add(source);
                __instance.ItemDB = list.ToArray();

                Plugin.Log.LogInfo($"[TMO] Injected custom Item into DB: {source.ItemName} (Id={source.Id}, name='{source.name}').");

                // Make GM.Maps only contain our item (same exact instance)
                if (GameData.GM?.Maps != null)
                {
                    GameData.GM.Maps.Clear();
                    GameData.GM.Maps.Add(source);
                    Plugin.Log.LogInfo("[TMO] GM.Maps cleared and set to Torn Map only.");
                }
                else
                {
                    Plugin.Log.LogWarning("[TMO] GM.Maps not available.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] ItemDBPatch failed: {ex}");
            }
        }
    }
}

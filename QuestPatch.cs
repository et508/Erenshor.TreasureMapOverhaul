using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace TreasureMapOverhaul
{
    // Patch after character select to update the TreasureMap quest requirements once all data is loaded
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class QuestPatch
    {
        [HarmonyPostfix]
        public static void OnCharacterEnter()
        {
            Plugin.Log.LogInfo("[TMO] QuestPatch triggered on CharSelectManager.Play.");

            // Ensure QuestDB and ItemDB are initialized
            if (GameData.QuestDB == null)
            {
                Plugin.Log.LogWarning("[TMO] QuestDB not initialized, skipping quest update.");
                return;
            }
            if (GameData.ItemDB == null)
            {
                Plugin.Log.LogWarning("[TMO] ItemDB not initialized, skipping quest update.");
                return;
            }

            // IDs of old map items
            string[] oldIds = { "28362792", "270986", "6188236", "28043030" };

            // Retrieve the specific TreasureMap quest
            var quest = GameData.QuestDB.GetQuestByName("TreasureMap");
            if (quest == null)
            {
                Plugin.Log.LogWarning("[TMO] Quest 'TreasureMap' not found in QuestDB.");
                return;
            }

            // Get the new torn map item
            var tornMap = GameData.ItemDB.GetItemByID("et508.tornmap");
            if (tornMap == null)
            {
                Plugin.Log.LogError("[TMO] Torn map not found in ItemDB!");
                return;
            }

            // Replace each old map requirement directly
            if (quest.RequiredItems != null)
            {
                for (int i = 0; i < quest.RequiredItems.Count; i++)
                {
                    var req = quest.RequiredItems[i];
                    if (req != null && Array.IndexOf(oldIds, req.Id) >= 0)
                    {
                        Plugin.Log.LogInfo($"[TMO] Quest '{quest.QuestName}': replacing required map {req.Id} with torn map.");
                        quest.RequiredItems[i] = tornMap;
                    }
                }
            }
        }
    }
}

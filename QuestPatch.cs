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

            // If any old map IDs are in the required items, replace entire list with a single torn map
            if (quest.RequiredItems != null)
            {
                bool hasOld = false;
                foreach (var item in quest.RequiredItems)
                {
                    if (item != null && Array.IndexOf(oldIds, item.Id) >= 0)
                    {
                        hasOld = true;
                        break;
                    }
                }
                if (hasOld)
                {
                    Plugin.Log.LogInfo($"[TMO] Quest '{quest.QuestName}': replacing all required items with single torn map.");
                    quest.RequiredItems.Clear();
                    quest.RequiredItems.Add(tornMap);
                }
            }
        }
    }
}

using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(CharSelectManager), "Play")]
    public static class QuestPatch
    {
        [HarmonyPostfix]
        public static void OnCharacterEnter()
        {
            try
            {
                // Ensure QuestDB and ItemDB are initialized
                if (GameData.QuestDB == null)
                {
                    Plugin.Log.LogError("[TMO] QuestDB not initialized in QuestPatch.OnCharacterEnter.");
                    return;
                }
                if (GameData.ItemDB == null)
                {
                    Plugin.Log.LogError("[TMO] ItemDB not initialized in QuestPatch.OnCharacterEnter.");
                    return;
                }

                // IDs of old map items
                string[] oldIds = { "28362792", "270986", "6188236", "28043030" };

                // Retrieve the specific TreasureMap quest
                var quest = GameData.QuestDB.GetQuestByName("TreasureMap");
                if (quest == null)
                {
                    Plugin.Log.LogError("[TMO] Quest 'TreasureMap' not found in QuestDB in QuestPatch.");
                    return;
                }

                // Get the new torn map item
                var tornMap = GameData.ItemDB.GetItemByID("et508.tornmap");
                if (tornMap == null)
                {
                    Plugin.Log.LogError("[TMO] Torn map not found in ItemDB in QuestPatch.");
                    return;
                }

                // Ensure RequiredItems list is valid
                if (quest.RequiredItems == null)
                {
                    Plugin.Log.LogError($"[TMO] RequiredItems is null for quest '{quest.QuestName}' in QuestPatch.");
                    return;
                }

                // Replace each old map requirement directly
                for (int i = 0; i < quest.RequiredItems.Count; i++)
                {
                    var req = quest.RequiredItems[i];
                    if (req != null && Array.IndexOf(oldIds, req.Id) >= 0)
                    {
                        quest.RequiredItems[i] = tornMap;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Exception in QuestPatch.OnCharacterEnter: {ex}");
            }
        }
    }
}

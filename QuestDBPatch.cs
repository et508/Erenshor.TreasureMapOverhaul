using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(QuestDB), "Start")]
    public static class QuestDBPatch
    {
        private const string PreferredQuestAssetName = "Treasure Map Repair";
        private const string TargetQuestDbName       = "TreasureMap";

        [HarmonyPostfix]
        public static void Postfix(QuestDB __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Plugin.Log.LogError("[TMO] QuestReplace: __instance is null.");
                    return;
                }

                var go = __instance.gameObject;
                if (go.GetComponent<TmoQuestReplacer>() == null)
                {
                    var c = go.AddComponent<TmoQuestReplacer>();
                    c.Init(__instance, PreferredQuestAssetName, TargetQuestDbName);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] QuestReplace: Exception in Postfix: {ex}");
            }
        }
    }

    public class TmoQuestReplacer : MonoBehaviour
    {
        private QuestDB _questDb;
        private string  _questAssetName;
        private string  _targetDbName;

        public void Init(QuestDB questDb, string questAssetName, string targetDbName)
        {
            _questDb        = questDb;
            _questAssetName = questAssetName;
            _targetDbName   = targetDbName;
        }

        private void OnEnable() => StartCoroutine(Run());

        private IEnumerator Run()
        {
            yield return null;

            float timeoutAt = Time.realtimeSinceStartup + 15f;
            while (GameData.ItemDB == null)
            {
                if (Time.realtimeSinceStartup > timeoutAt)
                {
                    Plugin.Log.LogError("[TMO] QuestReplace: Timed out waiting for GameData.ItemDB.");
                    Destroy(this);
                    yield break;
                }
                yield return null;
            }

            if (!TmoAssets.EnsureLoaded())
            {
                Plugin.Log.LogError("[TMO] QuestReplace: Asset bundle not available.");
                Destroy(this);
                yield break;
            }
            
            Quest donor = null;

            if (!string.IsNullOrWhiteSpace(_questAssetName))
            {
                donor = TmoAssets.Load<Quest>(_questAssetName);
                if (donor == null)
                    Plugin.Log.LogWarning($"[TMO] QuestReplace: Preferred quest asset '{_questAssetName}' not found in bundle.");
            }

            if (donor == null)
            {
                var all = TmoAssets.LoadAll<Quest>();
                donor = all.FirstOrDefault(q => q != null && MatchesQuest(q, _targetDbName));
            }

            if (donor == null)
            {
                Plugin.Log.LogError("[TMO] QuestReplace: Could not find donor Quest in bundle.");
                Destroy(this);
                yield break;
            }

            Quest live = FindLiveQuest(_questDb, _targetDbName);
            if (live == null)
            {
                Plugin.Log.LogError("[TMO] QuestReplace: Target quest not found in QuestDB.");
                Destroy(this);
                yield break;
            }
            
            try
            {
                string json = JsonUtility.ToJson(donor);
                JsonUtility.FromJsonOverwrite(json, live);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] QuestReplace: FromJsonOverwrite failed: {ex}");
            }
            
            try
            {
                RemapQuestItemsToLiveDB_ByNameThenId(live, GameData.ItemDB);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[TMO] QuestReplace: Remap failed: {ex}");
            }

            Destroy(this);
        }

        private static bool MatchesQuest(Quest q, string dbName)
        {
            if (q == null) return false;
            if (string.IsNullOrWhiteSpace(dbName)) return false;

            var fName = q.GetType().GetField("DBName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fName != null)
            {
                var v = fName.GetValue(q) as string;
                return string.Equals(v, dbName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static Quest FindLiveQuest(QuestDB db, string dbName)
        {
            try
            {
                var field = db.GetType().GetField("QuestDatabase",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var arr = field?.GetValue(db) as Quest[];
                if (arr == null) return null;

                return arr.FirstOrDefault(q => q != null && MatchesQuest(q, dbName));
            }
            catch { return null; }
        }

        private static string GetQuestName(Quest q)
        {
            try
            {
                var f = q.GetType().GetField("DBName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var n = f?.GetValue(q) as string;
                return string.IsNullOrEmpty(n) ? q.name : n;
            }
            catch { return q?.name ?? "(null)"; }
        }

        private static void RemapQuestItemsToLiveDB_ByNameThenId(Quest quest, ItemDatabase liveDb)
        {
            if (quest == null || liveDb == null) return;

            var reqField = quest.GetType().GetField("RequiredItems",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (reqField == null) return;

            var val = reqField.GetValue(quest);

            if (val is Item[] arr)
            {
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = FindLiveByNameThenId(liveDb, arr[i]) ?? arr[i];
                reqField.SetValue(quest, arr);
            }
            else if (val is System.Collections.Generic.List<Item> li)
            {
                for (int i = 0; i < li.Count; i++)
                    li[i] = FindLiveByNameThenId(liveDb, li[i]) ?? li[i];
                reqField.SetValue(quest, li);
            }
        }

        private static Item FindLiveByNameThenId(ItemDatabase db, Item donorItem)
        {
            if (donorItem == null) return null;

            var donorName = donorItem.name;
            if (!string.IsNullOrEmpty(donorName))
            {
                var foundByName = db.ItemDB?.FirstOrDefault(i => i != null && i.name == donorName);
                if (foundByName != null) return foundByName;
            }
            
            var donorId = GetItemId(donorItem);
            if (!string.IsNullOrEmpty(donorId))
            {
                var foundById = db.ItemDB?.FirstOrDefault(i => i != null && GetItemId(i) == donorId);
                if (foundById != null) return foundById;
            }

            return null;
        }

        private static string GetItemId(Item it)
        {
            try
            {
                var f = it.GetType().GetField("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f?.FieldType == typeof(string)) return (string)f.GetValue(it);

                var p = it.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p?.PropertyType == typeof(string)) return (string)p.GetValue(it, null);
            }
            catch { }
            return null;
        }
    }
}

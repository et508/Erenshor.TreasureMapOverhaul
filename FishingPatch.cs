using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(Fishing), "Update")]
    public static class FishingPatch
    {
        private static readonly MethodInfo _randomRangeInt =
            AccessTools.Method(typeof(Random), nameof(Random.Range), new[] { typeof(int), typeof(int) });

        private static readonly MethodInfo _listGetItem =
            AccessTools.Method(typeof(List<Item>), "get_Item");

        private static readonly MethodInfo _safePick =
            AccessTools.Method(typeof(FishingPatch), nameof(SafeMapPick));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var src = new List<CodeInstruction>(instructions);
            var dst = new List<CodeInstruction>(src.Count);
            bool replaced = false;

            for (int i = 0; i < src.Count; i++)
            {
                if (!replaced
                    && i + 3 < src.Count
                    && src[i    ].opcode == OpCodes.Ldc_I4_0
                    && src[i + 1].opcode == OpCodes.Ldc_I4_4
                    && src[i + 2].opcode == OpCodes.Call   && src[i + 2].operand is MethodInfo ri && ri == _randomRangeInt
                    && src[i + 3].opcode == OpCodes.Callvirt && src[i + 3].operand is MethodInfo gi && gi == _listGetItem)
                {
                    // The stack at this point has the List<Item> (GM.Maps) on top.
                    // Our helper consumes that list and returns the chosen Item.
                    dst.Add(new CodeInstruction(OpCodes.Call, _safePick));
                    i += 3; // skip ldc.i4.4, call Random.Range, callvirt get_Item
                    replaced = true;
                    continue;
                }

                dst.Add(src[i]);
            }

            if (!replaced)
                Plugin.Log.LogWarning("[TMO] FishingPatch: IL pattern not found in Fishing.Update — map fishing crash NOT patched. The game may have updated.");
            else
                Plugin.Log.LogInfo("[TMO] FishingPatch: Successfully patched Fishing.Update map index.");

            return dst;
        }
        
        public static Item SafeMapPick(List<Item> maps)
        {
            if (maps == null || maps.Count == 0)
            {
                Plugin.Log.LogError("[TMO] FishingPatch.SafeMapPick: GM.Maps is null or empty — cannot pick a map item.");
                return null;
            }
            return maps[Random.Range(0, maps.Count)];
        }
    }
}
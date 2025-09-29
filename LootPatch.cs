using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace TreasureMapOverhaul
{
    [HarmonyPatch(typeof(LootTable), "InitLootTable")]
    public static class LootTable_InitLootTable_MapChance_Transpiler
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var src   = new List<CodeInstruction>(instructions);
            var dst   = new List<CodeInstruction>(src.Count);
            bool replaced = false;
            int i = 0;

            while (i < src.Count)
            {
                var ci = src[i];
                
                if (!replaced && ci.opcode == OpCodes.Ldc_R4 && ci.operand is float f && Mathf.Approximately(f, 0.0125f))
                {
                    
                    dst.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LootTable_InitLootTable_MapChance_Transpiler), nameof(ConfigChance))));
                    
                    int j = i + 1;
                    
                    if (j < src.Count && (IsLoadLike(src[j]) ))
                    {
                        j++;
                    }
                    
                    if (j < src.Count && src[j].opcode == OpCodes.Mul)
                    {
                        j++;
                    }

                    i = j;         
                    replaced = true;
                    continue;
                }

                dst.Add(ci);
                i++;
            }

            if (!replaced)
            {
                Plugin.Log.LogWarning("[TMO] MapChance Transpiler: 0.0125f constant not found; map gate not patched.");
            }

            return dst;
        }

        private static bool IsLoadLike(CodeInstruction ci)
        {
            var op = ci.opcode;
            
            return op == OpCodes.Ldloc || op == OpCodes.Ldloc_S || op == OpCodes.Ldloc_0 || op == OpCodes.Ldloc_1
                   || op == OpCodes.Ldloc_2 || op == OpCodes.Ldloc_3
                   || op == OpCodes.Ldsfld || op == OpCodes.Ldfld || op == OpCodes.Call; // e.g., a getter
        }
        
        public static float ConfigChance()
        {
            try
            {
                float v = Plugin.GetNormalizedChance(Plugin.TreasureMapDropChancePercent);
                return Mathf.Clamp01(v);
            }
            catch
            {
                return 0.0125f;
            }
        }
    }
}

using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

public class SE_Bong : SE_Stats
{
    private Harmony harmony = new Harmony("com.drod917.Valweed");
    public float healthRegenMult;
    public float staminaRegenMult;
    public float ttl;

    // Called when the status effect first begins
    public override void Setup(Character character)
    {
        // Modify regens
        base.Setup(character);
        base.m_healthRegenMultiplier = healthRegenMult;
        base.m_staminaRegenMultiplier = staminaRegenMult;
        base.m_ttl = ttl;

        // Food rate modification
        // Runs until this script ends
        harmony.PatchAll(typeof(Player_UpdateFood_Transpiler));
    }

    public override void ResetTime()
    {
        base.ResetTime();
        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.GetCenterPoint(), m_character.transform.rotation, m_character.transform, radius * 2f);
    }

    // Called when the status effect ends
    public override void Stop()
    {
        base.Stop();
        harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdateFood))]
    public static class Player_UpdateFood_Transpiler
    {
        private static FieldInfo field_Player_m_foodUpdateTimer = AccessTools.Field(typeof(Player), nameof(Player.m_foodUpdateTimer));
        private static MethodInfo method_ComputeModifiedDt = AccessTools.Method(typeof(Player_UpdateFood_Transpiler), nameof(Player_UpdateFood_Transpiler.ComputeModifiedDT));

        /// <summary>
        /// Replaces the first load of dt inside Player::UpdateFood with a modified dt that is scaled
        /// by the food duration scaling multiplier. This ensures the food lasts longer while maintaining
        /// the same rate of regeneration.
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //if (!Configuration.Current.Food.IsEnabled) return instructions;

            List<CodeInstruction> il = instructions.ToList();

            for (int i = 0; i < il.Count - 2; ++i)
            {
                if (il[i].LoadsField(field_Player_m_foodUpdateTimer) &&
                    il[i + 1].opcode == OpCodes.Ldarg_1 /* dt */ &&
                    il[i + 2].opcode == OpCodes.Add)
                {
                    // We insert after Ldarg_1 (push dt) a call to our function, which computes the modified DT and returns it.
                    il.Insert(i + 2, new CodeInstruction(OpCodes.Call, method_ComputeModifiedDt));
                }
            }

            return il.AsEnumerable();
        }

        private static float ComputeModifiedDT(float dt)
        {
            float applyModifierValue(float targetValue, float value)
            {

                if (value <= -100)
                    value = -100;

                float newValue = targetValue;

                if (value >= 0)
                {
                    newValue = targetValue + ((targetValue / 100) * value);
                }
                else
                {
                    newValue = targetValue - ((targetValue / 100) * (value * -1));
                }

                return newValue;
            }

            return dt / applyModifierValue(1.0f, 50f);
        }
    }
}
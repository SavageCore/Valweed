using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;

public class SE_Bong : SE_Stats
{
    private Harmony harmony = new Harmony("com.drod917.Valweed.SEBong");
    public float healthRegenMult;
    public float staminaRegenMult;
    public float ttl;
    private static Color defaultFogColor;
    private static Color defaultAmbientSkyColor;
    private static Color defaultAmbientLightColor;
    private static float defaultFogDensity;
    private static float defaultAmbientIntensity;

    // Called when the status effect first begins
    public override void Setup(Character character)
    {
        // StatusEffect Setup without TriggerStartEffects()
        m_character = character;
        if (!string.IsNullOrEmpty(m_startMessage))
        {
            m_character.Message(m_startMessageType, m_startMessage);
        }

        // SE_Stats Setup without StatusEffect.Setup()
        if (m_healthOverTime > 0f && m_healthOverTimeInterval > 0f)
        {
            if (m_healthOverTimeDuration <= 0f)
            {
                m_healthOverTimeDuration = m_ttl;
            }
            m_healthOverTimeTicks = m_healthOverTimeDuration / m_healthOverTimeInterval;
            m_healthOverTimeTickHP = m_healthOverTime / m_healthOverTimeTicks;
        }
        if (m_staminaOverTime > 0f && m_staminaOverTimeDuration <= 0f)
        {
            m_staminaOverTimeDuration = m_ttl;
        }

        base.m_healthRegenMultiplier = healthRegenMult;
        base.m_staminaRegenMultiplier = staminaRegenMult;
        base.m_ttl = ttl;
        
        defaultAmbientLightColor = RenderSettings.ambientLight;
        defaultAmbientSkyColor = RenderSettings.ambientSkyColor;
        defaultAmbientIntensity = RenderSettings.ambientIntensity;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultFogColor = RenderSettings.fogColor;

        // Food rate modification
        // Runs until this script ends
        harmony.PatchAll(typeof(Player_UpdateFood_Transpiler));
        //harmony.PatchAll(typeof(GameCamera_UpdateCamera_Start));

        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
    }

    public bool IsSitting()
    {
        return m_character.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Character.m_animatorTagSitting;
    }

    public override void ResetTime()
    {
        base.ResetTime();
        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
    }

    // Called when the status effect ends
    public override void Stop()
    {
        base.Stop();
        //harmony.PatchAll(typeof(GameCamera_UpdateCamera_Stop));
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


    [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
    public static class GameCamera_UpdateCamera_Start
    {
        [HarmonyPrefix]
        public static void Prefix(GameCamera __instance)
        {
            //RenderSettings.fogColor = new Color(0.2f, 0.7f, 0.3f, 0.6f);
            //RenderSettings.fogDensity = 0.01f;
            RenderSettings.ambientLight = new Color(0.2f, 0.7f, 0.3f, 0.2f);
            RenderSettings.ambientSkyColor = new Color(0.2f, 0.7f, 0.3f, 0.2f);
            RenderSettings.ambientIntensity = 0.005f;
        }
    }

    [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
    public static class GameCamera_UpdateCamera_Stop
    {
        [HarmonyPrefix]
        public static void Prefix(GameCamera __instance)
        {
            //RenderSettings.fogColor = defaultFogColor;
            //RenderSettings.fogDensity = defaultFogDensity;
            RenderSettings.ambientLight = defaultAmbientLightColor;
            RenderSettings.ambientSkyColor = defaultAmbientSkyColor;
            RenderSettings.ambientIntensity = defaultAmbientIntensity;
        }
    }
}
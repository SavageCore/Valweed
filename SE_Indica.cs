using UnityEngine;
using System.Collections.Generic;

// Adds 10m to Rested buff. If not Rested, adds Rested.
public class SE_Indica : SE_Stats
{
    public int healthRegenVal;
    public int staminaRegenVal;
    private float healthRegenMult;
    private float staminaRegenMult;
    public float ttl;
    public bool cosmeticOnly;

    // Called when the status effect first begins
    public override void Setup(Character character)
    {
        //base.Setup(character);
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

        healthRegenMult = 1 + healthRegenVal / 100f;
        staminaRegenMult = 1 + staminaRegenVal / 100f;

        base.m_healthRegenMultiplier = cosmeticOnly ? 1 : healthRegenMult;
        base.m_staminaRegenMultiplier = cosmeticOnly ? 1 : staminaRegenMult;
        base.m_ttl = cosmeticOnly ? 10 : ttl;

        if (!cosmeticOnly)
        {
            Player player = m_character as Player;
            List<StatusEffect> statlist = player.GetSEMan().m_statusEffects;
            bool isRested = false;
            for (int i = 0; i < statlist.Count; i++)
            {
                if (statlist[i].GetType() == typeof(SE_Rested))
                {
                    // Player is Rested but not High(Indica)
                    // Set the current rested max time to (max time - time elapsed + ttl) and reset timer
                    // Adds ttl to the current Rested timer
                    statlist[i].m_ttl = (statlist[i].m_ttl - statlist[i].m_time) + ttl;
                    statlist[i].m_time = 0;
                    isRested = true;
                    break;
                }
            }
            if (!isRested)
                player.m_seman.AddStatusEffect("Rested", true);
        }

        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
    }
    public override void ResetTime()
    {
        base.ResetTime();
        float radius = m_character.GetRadius();

        // Rested management on buff refresh
        if (!cosmeticOnly)
        {
            Player player = m_character as Player;
            List<StatusEffect> statlist = player.GetSEMan().m_statusEffects;
            bool isRested = false;
            for (int i = 0; i < statlist.Count; i++)
            {
                if (statlist[i].GetType() == typeof(SE_Rested))
                {
                    // Player is Rested and already High(Indica)
                    // Set the current rested max time to (max time - time elapsed + (1/2 ttl)) and reset timer
                    // Adds 1/2 of the initial buff time to the rested timer if refreshed.
                    statlist[i].m_ttl = (statlist[i].m_ttl - statlist[i].m_time) + (ttl / 2);
                    statlist[i].m_time = 0;
                    isRested = true;
                    break;
                }
            }
            if (!isRested)
                player.m_seman.AddStatusEffect("Rested", true);
        }

        m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
    }

    public bool IsSitting()
    {
        return m_character.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Character.m_animatorTagSitting;
    }

    // Called when the status effect ends
    public override void Stop()
    {
        base.Stop();
    }
}
using UnityEngine;
using System.Collections.Generic;

// Adds 10m to Rested buff. If not Rested, adds Rested.
public class SE_Indica : SE_Stats
{
    public float healthRegenMult;
    public float staminaRegenMult;
    public float ttl;

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

        base.m_healthRegenMultiplier = healthRegenMult;
        base.m_staminaRegenMultiplier = staminaRegenMult;
        base.m_ttl = ttl;

        Player player = m_character as Player;
        List<StatusEffect> statlist = player.GetSEMan().m_statusEffects;
        bool isRested = false;
        for (int i = 0; i < statlist.Count; i++)
        {
            if (statlist[i].GetType() == typeof(SE_Rested))
            {
                // Set the current rested max time to (max time - time elapsed + 10m) and reset timer
                statlist[i].m_ttl = (statlist[i].m_ttl - statlist[i].m_time) + ttl;
                statlist[i].m_time = 0;
                isRested = true;
                break;
            }
        }
        if (!isRested)
            player.m_seman.AddStatusEffect("Rested", true);

        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
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
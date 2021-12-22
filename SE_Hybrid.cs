using UnityEngine;

public class SE_Hybrid : SE_Stats
{
    public int healthRegenVal;
    public int staminaRegenVal;
    private float healthRegenMult;
    private float staminaRegenMult;
    public float ttl;
    public bool cosmeticOnly;
    public EffectList my_effects = new EffectList();

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

        float radius = m_character.GetRadius();
        m_startEffectInstances = m_startEffects.Create(m_character.m_head.transform.position, m_character.m_head.transform.rotation, m_character.m_head.transform, radius * 2f);
    }
    public override void ResetTime()
    {
        base.ResetTime();
        float radius = m_character.GetRadius();
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
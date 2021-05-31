using UnityEngine;

public class SE_Hybrid : SE_Stats
{
    public float healthRegenMult;
    public float staminaRegenMult;
    public float ttl;

    // Called when the status effect first begins
    public override void Setup(Character character)
    {
        base.Setup(character);
        base.m_healthRegenMultiplier = healthRegenMult;
        base.m_staminaRegenMultiplier = staminaRegenMult;
        base.m_ttl = ttl;

        float radius = m_character.GetRadius();
        RemoveStartEffects();
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
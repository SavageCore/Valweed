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
        base.Setup(character);
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
    }

    // Called when the status effect ends
    public override void Stop()
    {
        base.Stop();
    }
}
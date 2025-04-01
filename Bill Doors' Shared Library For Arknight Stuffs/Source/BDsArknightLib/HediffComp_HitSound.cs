using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace BDsArknightLib
{
    public class HediffComp_HitSound : HediffComp
    {
        HediffCompProperties_HitSound Props => props as HediffCompProperties_HitSound;

        int nextHitEffectTick = 0;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (Find.TickManager.TicksGame > nextHitEffectTick)
            {
                Props.hitEffecter?.Spawn(Pawn.PositionHeld, Pawn.MapHeld);
                nextHitEffectTick = Find.TickManager.TicksGame + Props.minHitEffectInterval;
                Props.sounds.Where(s => dinfo.Amount > s.minDamage).RandomElement()?.hitSound?.PlayOneShot(new TargetInfo(Pawn.PositionHeld, Pawn.MapHeld));

            }
        }
    }
    public class HediffCompProperties_HitSound : HediffCompProperties
    {
        public HediffCompProperties_HitSound()
        {
            compClass = typeof(HediffComp_HitSound);
        }

        public int minHitEffectInterval = 30;

        public EffecterDef hitEffecter;

        public List<SoundForDamage> sounds;
    }
}

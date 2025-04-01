using System.Collections.Generic;
using System.Security.Cryptography;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace BDsArknightLib
{
    public class HediffComp_KillEffecter : HediffComp
    {
        public HediffCompProperties_KillEffecter Props => (HediffCompProperties_KillEffecter)props;
        public override void Notify_PawnKilled()
        {
            Props.effecter?.SpawnMaintainedIfPossible(parent.pawn, parent.pawn, parent.pawn.Map);
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            Props.effecter?.SpawnMaintainedIfPossible(parent.pawn, parent.pawn, parent.pawn.Map);
        }
    }



    public class HediffCompProperties_KillEffecter : HediffCompProperties
    {
        public HediffCompProperties_KillEffecter()
        {
            compClass = typeof(HediffComp_KillEffecter);
        }

        public EffecterDef effecter;
    }
}

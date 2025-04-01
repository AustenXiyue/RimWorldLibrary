using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BDsArknightLib
{
    public class Verb_CastAbilitySelf : Verb_CastAbility
    {
        protected override bool TryCastShot()
        {
            currentTarget = new LocalTargetInfo(caster);
            return base.TryCastShot();
        }
    }
}

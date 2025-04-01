using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace BDsArknightLib
{
    public class JobGiver_AICastAbilityFriendly : JobGiver_AICastAbility
    {
        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            Pawn pawn = null;
            if (caster.TryGetLord(out Lord lord))
            {
                pawn = lord.ownedPawns.Where(p => p != caster && ability.verb.CanHitTarget(p) && ability.CanApplyOn(new LocalTargetInfo(p))).RandomElementWithFallback(null);
            }
            if (pawn == null)
            {
                pawn = caster.Map.mapPawns.PawnsInFaction(caster.Faction).Where(p => p != caster && ability.verb.CanHitTarget(p) && ability.CanApplyOn(new LocalTargetInfo(p))).RandomElementWithFallback(null);
            }
            return pawn;
        }
    }

}

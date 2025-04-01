using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BDsArknightLib
{
    public class AbilityForAIDef : AbilityDef
    {
        public int priority = 0;

        public List<ThinkNode> thinkNodes = new List<ThinkNode>();
        public ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            int count = thinkNodes.Count;
            for (int i = 0; i < count; i++)
            {
                ThinkResult result = ThinkResult.NoJob;
                try
                {
                    result = thinkNodes[i].TryIssueJobPackage(pawn, jobParams);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat($"Exception in {defName} TryIssueJobPackage: ", ex.ToString()));
                }
                if (result.IsValid)
                {
                    return result;
                }
            }
            return ThinkResult.NoJob;
        }
    }

    public class ThinkNode_CastAbilities : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (pawn.abilities == null || pawn.abilities.abilities.NullOrEmpty()) return ThinkResult.NoJob;
            var result = ThinkResult.NoJob;
            var abilities = pawn.abilities.abilities.Where(x => x.def is AbilityForAIDef).Select(x => x.def as AbilityForAIDef).ToList();
            abilities.SortBy(x => -x.priority);
            foreach (var ability in abilities)
            {
                result = ability.TryIssueJobPackage(pawn, jobParams);
                if (result.IsValid) return result;
            }
            return result;
        }
    }
}

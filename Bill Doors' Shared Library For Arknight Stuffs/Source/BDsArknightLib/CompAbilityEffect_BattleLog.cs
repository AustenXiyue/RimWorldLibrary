using RimWorld;
using Verse;

namespace BDsArknightLib
{
    public class CompAbilityEffect_BattleLog : CompAbilityEffect
    {
        CompProperties_AbilityBattleLog Props => props as CompProperties_AbilityBattleLog;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.logEntryDef != null && Props.rulePackDef != null && target.HasThing)
            {
                Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(target.Thing, Props.rulePackDef, parent.pawn) { def = Props.logEntryDef });
            }
        }
    }
    public class CompProperties_AbilityBattleLog : CompProperties_AbilityEffect
    {
        public LogEntryDef logEntryDef;
        public RulePackDef rulePackDef;

        public CompProperties_AbilityBattleLog()
        {
            compClass = typeof(CompAbilityEffect_BattleLog);
        }
    }
}

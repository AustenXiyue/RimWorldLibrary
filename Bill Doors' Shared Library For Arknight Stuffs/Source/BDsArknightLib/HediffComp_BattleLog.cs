using Verse;

namespace BDsArknightLib
{
    public class HediffComp_BattleLog : HediffComp
    {
        HediffCompProperties_BattleLog Props => props as HediffCompProperties_BattleLog;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.addedLog != null && Props.addedRule != null)
            {
                Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(Pawn, Props.addedRule, dinfo?.Instigator ?? null) { def = Props.addedLog });
            }
        }

        public override void CompPostPostRemoved()
        {
            if (Props.removedRule != null && Props.removedLog != null)
            {
                Find.BattleLog.Add(new BattleLogEntry_EventWithIcon(Pawn, Props.removedRule, Pawn) { def = Props.removedLog });
            }
        }
    }

    public class HediffCompProperties_BattleLog : HediffCompProperties
    {
        public HediffCompProperties_BattleLog()
        {
            compClass = typeof(HediffComp_BattleLog);
        }

        public LogEntryDef addedLog;
        public LogEntryDef removedLog;

        public RulePackDef addedRule;
        public RulePackDef removedRule;
    }
    public class HediffComp_DisplaySeverityFlavored : HediffComp
    {
        HediffCompProperties_DisplaySeverityFlavored Props => props as HediffCompProperties_DisplaySeverityFlavored;

        public override string CompLabelPrefix => (Props.negative ? "-" : "") + parent.Severity.ToString("p0");

    }

    public class HediffCompProperties_DisplaySeverityFlavored : HediffCompProperties
    {
        public HediffCompProperties_DisplaySeverityFlavored()
        {
            compClass = typeof(HediffComp_DisplaySeverityFlavored);
        }

        public bool negative = false;
    }
}

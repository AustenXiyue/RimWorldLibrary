using RimWorld;
using Verse;
using UnityEngine;
using Verse.Grammar;

namespace BDsArknightLib
{
    public class BattleLogEntry_EventWithIcon : BattleLogEntry_Event
    {
        public BattleLogEntry_EventWithIcon()
        { }
        public BattleLogEntry_EventWithIcon(Thing subject, RulePackDef eventDef, Thing initiator) : base(subject, eventDef, initiator)
        { }

        public override Texture2D IconFromPOV(Thing pov)
        {
            if (pov == initiatorPawn)
            {
                return def.iconDamagedFromInstigatorTex;
            }
            return def.iconDamagedTex ?? null;
        }

        protected override GrammarRequest GenerateGrammarRequest()
        {
            var result = base.GenerateGrammarRequest();
            result.Constants.Add("SubjectIsNull", (subjectThing == null && subjectPawn == null) ? "True" : "False");
            result.Constants.Add("InitiatorIsNull", (initiatorPawn == null && initiatorThing == null) ? "True" : "False");
            return result;
        }
    }

    public class BattleLogEntry_ShieldStageShatter : BattleLogEntry_EventWithIcon
    {
        protected float pct = 1;

        public BattleLogEntry_ShieldStageShatter()
        { }
        public BattleLogEntry_ShieldStageShatter(Thing subject, RulePackDef eventDef, Thing initiator, float pct)
        {
            if (subject is Pawn)
            {
                subjectPawn = subject as Pawn;
            }
            else if (subject != null)
            {
                subjectThing = subject.def;
            }
            if (initiator is Pawn)
            {
                initiatorPawn = initiator as Pawn;
            }
            else if (initiator != null)
            {
                initiatorThing = initiator.def;
            }
            this.eventDef = eventDef;
            this.pct = pct;
        }

        protected override GrammarRequest GenerateGrammarRequest()
        {
            var v = base.GenerateGrammarRequest();
            v.Rules.Add(new Rule_String("SHIELD", pct.ToString("P1")));
            return v;
        }
    }
}

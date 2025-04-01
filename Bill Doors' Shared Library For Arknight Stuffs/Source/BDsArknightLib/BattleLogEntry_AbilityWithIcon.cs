using RimWorld;
using Verse;
using UnityEngine;
using Verse.Grammar;

namespace BDsArknightLib
{
    public class BattleLogEntry_AbilityWithIcon : BattleLogEntry_AbilityUsed
    {
        public BattleLogEntry_AbilityWithIcon()
        { }
        public BattleLogEntry_AbilityWithIcon(Pawn caster, Thing target, AbilityDef ability, RulePackDef eventDef) : base(caster, target, ability, eventDef) { }
        public override Texture2D IconFromPOV(Thing pov)
        {
            if (pov == initiatorPawn)
            {
                return def.iconDamagedFromInstigatorTex;
            }
            return def.iconDamagedTex;
        }
        protected override GrammarRequest GenerateGrammarRequest()
        {
            var result = base.GenerateGrammarRequest();
            result.Constants.Add("SubjectIsNull", (subjectThing == null && subjectPawn == null) ? "True" : "False");
            result.Constants.Add("InitiatorIsNull", (initiatorPawn == null && initiatorThing == null) ? "True" : "False");
            return result;
        }
    }
}

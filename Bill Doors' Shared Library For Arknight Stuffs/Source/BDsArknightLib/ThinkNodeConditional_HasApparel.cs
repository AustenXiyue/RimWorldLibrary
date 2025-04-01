using Verse;
using Verse.AI;

namespace BDsArknightLib
{
    public class ThinkNodeConditional_HasApparel : ThinkNode_Conditional
    {
        public ThingDef apparelDef;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNodeConditional_HasApparel obj = (ThinkNodeConditional_HasApparel)base.DeepCopy(resolve);
            obj.apparelDef = apparelDef;
            return obj;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.apparel != null)
            {
                foreach (var f in pawn.apparel.WornApparel)
                {
                    if (f.def == apparelDef) return true;
                }
            }
            return false;
        }
    }

}

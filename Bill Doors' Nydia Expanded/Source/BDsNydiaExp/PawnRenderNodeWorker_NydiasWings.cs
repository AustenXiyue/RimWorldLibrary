using BDsArknightLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BDsNydiaExp
{
    public class PawnRenderNodeWorker_NydiasWings : PawnRenderNodeWorker_AttachmentBody
    {
        protected virtual bool extended => true;

        protected virtual bool hediffMode => false;

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            return base.CanDrawNow(node, parms) && ShouldRender(parms.pawn);
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!ShouldRender(parms.pawn)) return Vector3.zero;
            return base.ScaleFor(node, parms);
        }

        protected bool ShouldRender(Pawn pawn)
        {
            if (BDsNydiaExp_Mod.NoWings) return hediffMode;
            return extended ? ShouldExtend(pawn) : !ShouldExtend(pawn);
        }

        bool ShouldExtend(Pawn pawn)
        {
            if (pawn.Dead || pawn.Downed) return false;
            if (pawn.InAggroMentalState) return true;
            if (pawn.drafter?.Drafted ?? false) return true;
            if (pawn.mindState?.enemyTarget != null) return true;
            if (pawn.stances?.FullBodyBusy ?? false) return true;
            return false;
        }
    }

    public class PawnRenderNodeWorker_NydiasWingsFolded : PawnRenderNodeWorker_NydiasWings
    {
        protected override bool extended => false;
    }

    public class PawnRenderNodeWorker_NydiasWingsHediff : PawnRenderNodeWorker_NydiasWings
    {
        protected override bool hediffMode => true;
    }
}

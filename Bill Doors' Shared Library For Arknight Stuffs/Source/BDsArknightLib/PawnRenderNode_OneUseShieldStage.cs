using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BDsArknightLib
{
    public class PawnRenderNode_OneUseShieldStage : PawnRenderNode
    {
        public new PawnRenderNodeProperties_OneUseShieldStage Props => base.Props as PawnRenderNodeProperties_OneUseShieldStage;

        public PawnRenderNode_OneUseShieldStage(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Color ColorFor(Pawn pawn)
        {
            if (apparel.GetComp<CompShieldOneUse>() is CompShieldOneUse comp && comp.pct < Props.pct)
            {
                return new Color(1, 1, 1, 0);
            }
            return base.ColorFor(pawn);
        }
    }
    public class PawnRenderNodeProperties_OneUseShieldStage : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_OneUseShieldStage()
        {
            nodeClass = typeof(PawnRenderNode_OneUseShieldStage);
        }
        public float pct = 0.5f;

        public float bobDist = 0.1f;

        public float bobSpeed = 1;

        public int randSeed = 0;
    }

    public class PawnRenderNodeWorker_OneUseShieldStage : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnRenderNode_OneUseShieldStage r && r.apparel.GetComp<CompShieldOneUse>() is CompShieldOneUse comp && comp.pct > r.Props.pct)
            {
                return true;
            }
            return false;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnRenderNode_OneUseShieldStage r && r.apparel.GetComp<CompShieldOneUse>() is CompShieldOneUse comp && comp.pct > r.Props.pct)
            {
                return base.ScaleFor(node, parms);
            }
            return Vector3.zero;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            var v = base.OffsetFor(node, parms, out pivot);
            if (node is PawnRenderNode_OneUseShieldStage r)
            {
                v.z += Mathf.PingPong(((float)Find.TickManager.TicksGame + r.Props.randSeed) * r.Props.bobSpeed, r.Props.bobDist);
            }
            return v;
        }
    }
}

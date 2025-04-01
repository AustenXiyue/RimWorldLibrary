using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BDsArknightLib
{
    public class CompRenderNode : ThingComp
    {
        public CompProperties_RenderNode Props => props as CompProperties_RenderNode;

        Pawn parentPawn => parent as Pawn;

        public override List<PawnRenderNode> CompRenderNodes()
        {
            if (parentPawn == null)
            {
                return null;
            }
            List<PawnRenderNode> l = new List<PawnRenderNode>();
            foreach (PawnRenderNodeProperties renderNodeProperty in Props.RenderNodeProperties)
            {
                l.Add((PawnRenderNode)Activator.CreateInstance(renderNodeProperty.nodeClass, parentPawn, renderNodeProperty, parentPawn.Drawer.renderer.renderTree));
            }
            return l;
        }
    }

    public class CompProperties_RenderNode : CompProperties, IRenderNodePropertiesParent
    {
        public bool HasDefinedGraphicProperties => RenderNodeProperties.Any();

        List<PawnRenderNodeProperties> renderNodeProperties;

        public List<PawnRenderNodeProperties> RenderNodeProperties => renderNodeProperties;

        public CompProperties_RenderNode()
        {
            compClass = typeof(CompRenderNode);
        }
    }
}

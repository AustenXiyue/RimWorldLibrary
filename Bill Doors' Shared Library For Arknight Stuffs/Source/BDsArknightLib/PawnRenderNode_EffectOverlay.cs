using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.AI;
using System.Data.Common;

namespace BDsArknightLib
{
    public class PawnRenderNode_EffectOverlay : PawnRenderNode
    {
        public PawnRenderNode_EffectOverlay(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }
        public override Graphic GraphicFor(Pawn pawn)
        {
            return GraphicDatabase.Get<Graphic_Single>(props.texPath, props.shaderTypeDef.Shader);
        }

        public override Color ColorFor(Pawn pawn)
        {
            return Color.white;
        }
    }

    public class PawnRenderNode_EffectOverlayMulti : PawnRenderNode_EffectOverlay
    {
        public PawnRenderNode_EffectOverlayMulti(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }
        public override Graphic GraphicFor(Pawn pawn)
        {
            return GraphicDatabase.Get<Graphic_Multi>(props.texPath, props.shaderTypeDef.Shader);
        }
    }

    public class PawnRenderNodeSubWorker_PawnDrawSize : PawnRenderSubWorker
    {
        public override void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
        {
            if (!parms.pawn.RaceProps.Humanlike)
            {
                var size = parms.pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                scale.x *= size.x / 2;
                scale.z *= size.y / 2;
            }
            else
            {
                scale *= node.Props.drawData.ScaleFor(parms.pawn);
            }
        }
    }

    public class PawnRenderNodeSubWorker_StatusEffect : PawnRenderSubWorker
    {
        const int columnCount = 3;
        const float gap = 0.5f;

        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
        {
            offset += Vector3.forward;
            var nodes = GetAllNodes(parms).Where(n => n.Props.subworkerClasses?.Contains(typeof(PawnRenderNodeSubWorker_StatusEffect)) ?? false);
            offset += GetOffset(nodes.Count(), nodes.FirstIndexOf(n => n == node));
        }

        public override void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
        {
            float nodes = GetAllNodes(parms).Where(n => n.Props.subworkerClasses?.Contains(typeof(PawnRenderNodeSubWorker_StatusEffect)) ?? false).Count();
            if (nodes == 1)
            {
                return;
            }
            scale *= ((float)columnCount - 1) / columnCount;
        }

        Vector3 GetOffset(int total, int index)
        {
            if (total == 1) return Vector3.zero;
            float row = 0;
            int column;
            if (total < columnCount * 2 - 1)
            {
                column = total == 2 ? 2 : (total + 1) / 2;

                if (index > column - 1)
                {
                    row += gap;
                    index -= column;
                    column = total - column;
                }
            }
            else
            {
                row = (index / columnCount) * gap;
                var reminder = total % columnCount;
                column = (total - reminder > index) ? columnCount : reminder;
                index = index % columnCount;
            }
            return new Vector3(GetRowOffset(column, index), 0, row);
        }

        float GetRowOffset(float rowWidth, int index)
        {
            if (rowWidth == 1) return 0;
            return ((-(rowWidth - 1) / 2) + index) * gap;
        }

        IEnumerable<PawnRenderNode> GetAllNodes(PawnDrawParms parms)
        {
            return GetAllNodes(parms.pawn.Drawer.renderer.renderTree.rootNode);
        }

        IEnumerable<PawnRenderNode> GetAllNodes(PawnRenderNode parentNode)
        {
            yield return parentNode;
            if (parentNode.children != null)
            {
                foreach (var node in parentNode.children)
                {
                    foreach (var subNode in GetAllNodes(node))
                    {
                        yield return subNode;
                    }
                }
            }
        }
    }
}
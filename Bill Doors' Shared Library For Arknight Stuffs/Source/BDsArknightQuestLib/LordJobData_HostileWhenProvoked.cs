using QuestEditor_Library;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse.AI.Group;
using Verse;
using UnityEngine;
using BDsArknightLib;
using BillDoorsPredefinedCharacter;
using Verse.AI;

namespace BDsArknightQuestLib
{
    public class LordJobData_HostileWhenProvoked : LordJobData
    {
        public string targetPositionName;

        public string faction;
        public string radius;

        public override bool JobSelectable => false;

        public override Type LordJob => typeof(LordJob_HostileWhenProvoked);

        public override LordJob CreateJob(Map map, Quest quest)
        {
            return new LordJob_HostileWhenProvoked(GameTools.GetFaction(faction, map), GameTools.GetTarget(null, quest, targetPositionName).Cell, float.Parse(radius));
        }

        public override void Draw(ref float y, Rect inRect, float x)
        {
            base.Draw(ref y, inRect, x);
            CQFEditorTools.DrawLabelAndText_Line(y, "TargetPositionName".Translate(), ref targetPositionName, x, 150f);
            y += 30f;
            CQFEditorTools.DrawLabelAndText_Line(y, "PawnDataFaction".Translate(), ref faction, x, 150f);
            y += 30f;
            CQFEditorTools.DrawLabelAndText_Line(y, "radius", ref radius, x, 150f);
            y += 30f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetPositionName, "targetPositionName");
            Scribe_Values.Look(ref faction, "faction");
            Scribe_Values.Look(ref radius, "radius");
        }

        public override XElement SaveToXElement(string nodeName)
        {
            XElement xElement = base.SaveToXElement(nodeName);
            if (targetPositionName != null)
            {
                xElement.Add(new XElement("targetPositionName", targetPositionName));
            }
            if (faction != null)
            {
                xElement.Add(new XElement("faction", faction));
            }
            if (faction != null)
            {
                xElement.Add(new XElement("radius", radius));
            }
            return xElement;
        }
    }
}

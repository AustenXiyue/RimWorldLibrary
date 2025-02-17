using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_Mine : Designator_Cells
{
	private static List<string> tmpIdeoMemberNames = new List<string>();

	public override int DraggableDimensions => 2;

	public override bool DragDrawMeasurements => true;

	protected override DesignationDef Designation => DesignationDefOf.Mine;

	public Designator_Mine()
	{
		defaultLabel = "DesignatorMine".Translate();
		icon = ContentFinder<Texture2D>.Get("UI/Designators/Mine");
		defaultDesc = "DesignatorMineDesc".Translate();
		useMouseIcon = true;
		soundDragSustain = SoundDefOf.Designate_DragStandard;
		soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
		soundSucceeded = SoundDefOf.Designate_Mine;
		hotKey = KeyBindingDefOf.Misc10;
		tutorTag = "Mine";
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (base.Map.designationManager.DesignationAt(c, Designation) != null)
		{
			return AcceptanceReport.WasRejected;
		}
		if (c.Fogged(base.Map))
		{
			return true;
		}
		Mineable firstMineable = c.GetFirstMineable(base.Map);
		if (firstMineable == null)
		{
			return "MessageMustDesignateMineable".Translate();
		}
		AcceptanceReport result = CanDesignateThing(firstMineable);
		if (!result.Accepted)
		{
			return result;
		}
		return AcceptanceReport.WasAccepted;
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!t.def.mineable)
		{
			return false;
		}
		if (base.Map.designationManager.DesignationAt(t.Position, Designation) != null)
		{
			return AcceptanceReport.WasRejected;
		}
		if (base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.MineVein) != null)
		{
			return AcceptanceReport.WasRejected;
		}
		return true;
	}

	public override void DesignateSingleCell(IntVec3 loc)
	{
		base.Map.designationManager.AddDesignation(new Designation(loc, Designation));
		base.Map.designationManager.TryRemoveDesignation(loc, DesignationDefOf.SmoothWall);
		PossiblyWarnPlayerOnDesignatingMining();
		if (DebugSettings.godMode)
		{
			loc.GetFirstMineable(base.Map)?.DestroyMined(null);
		}
	}

	public override void DesignateThing(Thing t)
	{
		DesignateSingleCell(t.Position);
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Mining, KnowledgeAmount.SpecificInteraction);
	}

	public override void SelectedUpdate()
	{
		GenUI.RenderMouseoverBracket();
	}

	protected static void PossiblyWarnPlayerOnDesignatingMining()
	{
		if (!ModsConfig.IdeologyActive)
		{
			return;
		}
		tmpIdeoMemberNames.Clear();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			if (allIdeo.WarnPlayerOnDesignateMine)
			{
				tmpIdeoMemberNames.Add(Find.ActiveLanguageWorker.Pluralize(allIdeo.memberName));
			}
		}
		if (tmpIdeoMemberNames.Any())
		{
			Messages.Message("MessageWarningPlayerDesignatedMining".Translate(tmpIdeoMemberNames.ToCommaList(useAnd: true)), MessageTypeDefOf.CautionInput, historical: false);
		}
		tmpIdeoMemberNames.Clear();
	}
}

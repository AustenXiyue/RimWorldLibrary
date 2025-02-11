using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompMechAutoFight : ThingComp
{
	public bool autoFight = false;

	public CompProperties_MechAutoFight Props => (CompProperties_MechAutoFight)props;

	public string gizmoLabel => Props.gizmoLabel.NullOrEmpty() ? ((string)"Ancot.AutoFight".Translate()) : Props.gizmoLabel;

	public string gizmoDesc => Props.gizmoDesc.NullOrEmpty() ? ((string)"Ancot.AutoFightDesc".Translate()) : Props.gizmoDesc;

	public bool CanAutoFight
	{
		get
		{
			if (Props.requireResearch != null)
			{
				return Props.requireResearch.IsFinished;
			}
			return true;
		}
	}

	protected Pawn PawnOwner
	{
		get
		{
			if (parent is Pawn result)
			{
				return result;
			}
			return null;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref autoFight, "autoFight", defaultValue: false);
	}

	public override void CompTickRare()
	{
		if (autoFight && PawnOwner.Spawned && !PawnOwner.Drafted && !PawnOwner.Dead && PawnOwner.needs.energy != null)
		{
			PawnOwner.needs.energy.CurLevel -= 0.1041667f;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (PawnOwner == null || PawnOwner.Faction != Faction.OfPlayer || PawnOwner.Dead || !CanAutoFight)
		{
			yield break;
		}
		foreach (Gizmo gizmo in GetGizmos())
		{
			yield return gizmo;
		}
	}

	private IEnumerable<Gizmo> GetGizmos()
	{
		yield return new Command_Toggle
		{
			Order = Props.gizmoOrder,
			defaultLabel = gizmoLabel,
			defaultDesc = gizmoDesc,
			icon = ContentFinder<Texture2D>.Get(Props.gizmoIconPath),
			toggleAction = delegate
			{
				autoFight = !autoFight;
				if (autoFight)
				{
					PawnOwner.health.AddHediff(Props.hediffDef, null, null);
				}
				else
				{
					Hediff firstHediffOfDef = PawnOwner.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
					PawnOwner.health.RemoveHediff(firstHediffOfDef);
				}
			},
			isActive = () => autoFight
		};
	}
}

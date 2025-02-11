using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompOverChargeShot : ThingComp
{
	public float charge;

	public float maxCharge => Props.maxCharge;

	public CompEquippable compEquippable => parent.TryGetComp<CompEquippable>();

	public Pawn CasterPawn => compEquippable.PrimaryVerb.caster as Pawn;

	public float warmupTime => parent.def.Verbs[0].warmupTime;

	public Color barColor => Props.barColor;

	private CompProperties_OverChargeShot Props => (CompProperties_OverChargeShot)props;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref charge, "charge", 0f);
	}

	public override void Initialize(CompProperties props)
	{
		base.props = props;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (parent == null)
		{
			charge = 0f;
		}
		else if (charge > 0f && charge <= maxCharge)
		{
			charge -= Props.cooldownChargePerTick;
			compEquippable.PrimaryVerb.verbProps.warmupTime = Props.defaultWarmupTime * (maxCharge - charge) / maxCharge;
		}
		else if (charge < 0f)
		{
			charge = 0f;
		}
		else if (charge > maxCharge)
		{
			charge = maxCharge;
		}
		if (compEquippable.PrimaryVerb.verbProps.warmupTime < Props.minWarmupTime)
		{
			compEquippable.PrimaryVerb.verbProps.warmupTime = Props.minWarmupTime;
		}
	}

	public override void Notify_UsedWeapon(Pawn pawn)
	{
		base.Notify_UsedWeapon(pawn);
		Log.Message("11");
		charge += 1f;
		if (Props.destroyOnFull && charge == maxCharge && !parent.Destroyed)
		{
			parent.Destroy();
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (Find.Selector.SelectedObjects.Count == 1)
		{
			yield return new Gizmo_OverChargeBar
			{
				compOverChargeShot = parent.TryGetComp<CompOverChargeShot>()
			};
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AncotLibrary;

public class CompAbilityDoExplosionOnCell : CompAbilityEffect
{
	private List<IntVec3> tmpCells = new List<IntVec3>();

	public new CompProperties_AbilityDoExplosionOnCell Props => (CompProperties_AbilityDoExplosionOnCell)props;

	private Pawn Caster => parent.pawn;

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return true;
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (Props.targetOnCaster)
		{
			target = Caster;
		}
		foreach (IntVec3 item in AffectedCells(target.Cell))
		{
			GenExplosion.DoExplosion(item, Caster.Map, 0.8f, Props.damageDef, Caster, Props.damageAmount, Props.armorPenetration, null, null, null, null, null, 0f, 1, null, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, null, null);
		}
	}

	private List<IntVec3> AffectedCells(IntVec3 target)
	{
		tmpCells.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(target, Props.radius, useCenter: true))
		{
			if (item.IsValid || item.InBounds(Caster.Map))
			{
				tmpCells.Add(item);
			}
		}
		tmpCells = tmpCells.Distinct().ToList();
		tmpCells.RemoveAll((IntVec3 cell) => !CanUseCell(cell));
		return tmpCells;
		bool CanUseCell(IntVec3 c)
		{
			if (!c.InBounds(Caster.Map))
			{
				return false;
			}
			if (Props.ignoreCasterCell && c == Caster.Position)
			{
				return false;
			}
			return true;
		}
	}
}

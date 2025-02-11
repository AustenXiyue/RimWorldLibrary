using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CompPawnGizmo : ThingComp
{
	private bool duplicate = false;

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		foreach (ThingComp comp in parent.comps)
		{
			if (comp is CompPawnGizmo && comp != this)
			{
				duplicate = true;
				Log.ErrorOnce(parent.def.defName + " has multiple CompPawnGizmo, duplicates has been deactivated. Please report this to the patch provider of " + parent.def.modContentPack.Name + " or CE team if the patch is integrated in CE.", parent.def.GetHashCode());
			}
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (duplicate)
		{
			yield break;
		}
		ThingWithComps equip = ((parent is Pawn pawn) ? pawn.equipment.Primary : null);
		if (equip == null || equip.AllComps.NullOrEmpty())
		{
			yield break;
		}
		foreach (ThingComp comp in equip.AllComps)
		{
			CompRangedGizmoGiver gizmoGiver = comp as CompRangedGizmoGiver;
			if (!(gizmoGiver?.isRangedGiver ?? false))
			{
				continue;
			}
			foreach (Gizmo item in gizmoGiver.CompGetGizmosExtra())
			{
				yield return item;
			}
		}
	}
}

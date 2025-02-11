using System.Collections.Generic;
using Verse;

namespace AncotLibrary;

public class CompPawnGetWeaponGizmo : ThingComp
{
	private CompProperties_PawnGetWeaponGizmo Props => (CompProperties_PawnGetWeaponGizmo)props;

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		ThingWithComps thingWithComps = parent;
		Pawn pawn = thingWithComps as Pawn;
		if (pawn == null || pawn.equipment == null)
		{
			yield break;
		}
		ThingWithComps primaryEquipment = pawn.equipment.Primary;
		if (primaryEquipment == null || primaryEquipment.AllComps.NullOrEmpty())
		{
			yield break;
		}
		foreach (ThingComp comp in primaryEquipment.AllComps)
		{
			if (comp is CompRangeWeaponVerbSwitch compRangeWeaponVerbSwitch)
			{
				foreach (Gizmo item in compRangeWeaponVerbSwitch.CompGetGizmosExtra())
				{
					yield return item;
				}
			}
			if (comp is CompWeaponCharge compWeaponCharge)
			{
				foreach (Gizmo item2 in compWeaponCharge.CompGetGizmosExtra())
				{
					yield return item2;
				}
			}
			if (comp is CompOverChargeShot compOverChargeShot)
			{
				foreach (Gizmo item3 in compOverChargeShot.CompGetGizmosExtra())
				{
					yield return item3;
				}
			}
			if (!(comp is CompMeleeWeaponCharge_Ability compMeleeWeaponCharge_Ability))
			{
				continue;
			}
			foreach (Gizmo item4 in compMeleeWeaponCharge_Ability.CompGetGizmosExtra())
			{
				yield return item4;
			}
		}
	}
}

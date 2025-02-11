using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended;

public class ShieldDefExtension : DefModExtension
{
	public List<BodyPartGroupDef> shieldCoverage = new List<BodyPartGroupDef>();

	public List<BodyPartGroupDef> crouchCoverage = new List<BodyPartGroupDef>();

	public bool drawAsTall = false;

	public bool PartIsCoveredByShield(BodyPartRecord part, Pawn pawn)
	{
		if (!shieldCoverage.NullOrEmpty())
		{
			foreach (BodyPartGroupDef item in shieldCoverage)
			{
				if (part.IsInGroup(item))
				{
					return true;
				}
			}
		}
		if (!crouchCoverage.NullOrEmpty() && pawn.IsCrouching())
		{
			foreach (BodyPartGroupDef item2 in crouchCoverage)
			{
				if (part.IsInGroup(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetShieldProtectedAreas(BodyDef body, ThingDef thingDef)
	{
		return (from part in body.AllParts.Where((BodyPartRecord x) => x.depth == BodyPartDepth.Outside && x.groups.Any((BodyPartGroupDef y) => thingDef.GetModExtension<ShieldDefExtension>().shieldCoverage.Contains(y))).Distinct()
			select part.Label).ToCommaList().CapitalizeFirst();
	}
}

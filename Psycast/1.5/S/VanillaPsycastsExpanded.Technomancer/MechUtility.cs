using Verse;

namespace VanillaPsycastsExpanded.Technomancer;

public static class MechUtility
{
	public static bool IsMechAlly(this Pawn mech, Pawn other)
	{
		return mech.RaceProps.IsMechanoid && MechanitorUtility.IsPlayerOverseerSubject(mech) && (other.Faction == mech.Faction || (other.IsColonist && mech.IsColonyMech));
	}
}

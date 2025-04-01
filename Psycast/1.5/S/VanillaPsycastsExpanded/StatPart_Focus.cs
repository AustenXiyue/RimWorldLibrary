using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public abstract class StatPart_Focus : StatPart
{
	public MeditationFocusDef focus;

	public bool ApplyOn(StatRequest req)
	{
		return req.Thing is Pawn p && focus.CanPawnUse(p) && StatPart_NearbyFoci.ShouldApply;
	}
}

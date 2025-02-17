using Verse;

namespace RimWorld;

public class ThoughtWorker_Dark : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		return p.Awake() && p.needs.mood.recentMemory.TicksSinceLastLight > 240 && !PawnUtility.IsBiologicallyOrArtificiallyBlind(p) && (p.Ideo == null || !p.Ideo.IdeoPrefersDarkness());
	}
}

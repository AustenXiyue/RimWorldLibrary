using RimWorld.Planet;
using Verse;

namespace VanillaPsycastsExpanded.Technomancer;

public class Pawn_Construct : Pawn, IMinHeatGiver, ILoadReferenceable
{
	public bool IsActive => base.Spawned || this.GetCaravan() != null;

	public int MinHeat => 20;
}

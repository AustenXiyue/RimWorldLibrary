namespace RimWorld;

public class ITab_Pawn_Prisoner : ITab_Pawn_Visitor
{
	public override bool IsVisible => SelPawn.IsPrisonerOfColony;

	public ITab_Pawn_Prisoner()
	{
		labelKey = "TabPrisoner";
		tutorTag = "Prisoner";
	}
}

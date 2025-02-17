using Verse;

namespace RimWorld;

public class PawnColumnWorker_Tame : PawnColumnWorker_Designator
{
	protected override DesignationDef DesignationType => DesignationDefOf.Tame;

	protected override string GetTip(Pawn pawn)
	{
		return "DesignatorTameDesc".Translate();
	}

	protected override bool HasCheckbox(Pawn pawn)
	{
		if (TameUtility.CanTame(pawn))
		{
			return pawn.SpawnedOrAnyParentSpawned;
		}
		return false;
	}

	protected override void Notify_DesignationAdded(Pawn pawn)
	{
		pawn.MapHeld.designationManager.TryRemoveDesignationOn(pawn, DesignationDefOf.Hunt);
		TameUtility.ShowDesignationWarnings(pawn);
	}
}

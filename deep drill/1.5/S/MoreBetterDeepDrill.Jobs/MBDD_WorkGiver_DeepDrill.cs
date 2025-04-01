using System.Collections.Generic;
using MoreBetterDeepDrill.Comp;
using MoreBetterDeepDrill.Defs;
using MoreBetterDeepDrill.Utils;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreBetterDeepDrill.Jobs;

public class MBDD_WorkGiver_DeepDrill : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

	public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		if (pawn.IsColonyMech && !StaticValues.ModSetting.EnableMechdroids)
		{
			return true;
		}
		List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			Building building = allBuildingsColonist[i];
			if (building.def == MoreBetterDeepDrill.Defs.ThingDefOf.MBDD_RangedDeepDrill || building.def == MoreBetterDeepDrill.Defs.ThingDefOf.MBDD_LargeDeepDrill || building.def == MoreBetterDeepDrill.Defs.ThingDefOf.MBDD_ArchotechDeepDrill)
			{
				CompPowerTrader comp = building.GetComp<CompPowerTrader>();
				if ((comp == null || comp.PowerOn) && building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.Faction != pawn.Faction)
		{
			return false;
		}
		if (!(t is Building building))
		{
			return false;
		}
		if (building.IsForbidden(pawn))
		{
			return false;
		}
		if (t.def == MoreBetterDeepDrill.Defs.ThingDefOf.MBDD_LargeDeepDrill)
		{
			if (!pawn.CanReserve(building, 12, 0, null, forced))
			{
				return false;
			}
		}
		else if (!pawn.CanReserve(building, 1, -1, null, forced))
		{
			return false;
		}
		MBDD_CompDeepDrill comp = building.GetComp<MBDD_CompDeepDrill>();
		if (comp == null || !comp.CanDrillNow)
		{
			return false;
		}
		if (building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) != null)
		{
			return false;
		}
		if (building.IsBurning())
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.def == MoreBetterDeepDrill.Defs.ThingDefOf.MBDD_LargeDeepDrill)
		{
			return JobMaker.MakeJob(MoreBetterDeepDrill.Defs.JobDefOf.MBDD_MultiOperateDeepDrill, t, 1500);
		}
		return JobMaker.MakeJob(MoreBetterDeepDrill.Defs.JobDefOf.MBDD_SingleOperateDeepDrill, t, 1500);
	}
}

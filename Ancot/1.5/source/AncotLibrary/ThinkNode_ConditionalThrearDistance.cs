using RimWorld;
using Verse;
using Verse.AI;

namespace AncotLibrary;

public class ThinkNode_ConditionalThrearDistance : ThinkNode_Conditional
{
	public float? threatDistance;

	protected override bool Satisfied(Pawn pawn)
	{
		Map map = pawn.Map;
		if (!pawn.Downed && !pawn.Dead && pawn.equipment != null)
		{
			float num = threatDistance ?? pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range;
			Log.Message("range" + num);
			foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
			{
				if (item.HostileTo(pawn))
				{
					float num2 = item.Position.DistanceTo(pawn.Position);
					if (num2 < num)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_ConditionalThrearDistance thinkNode_ConditionalThrearDistance = (ThinkNode_ConditionalThrearDistance)base.DeepCopy(resolve);
		thinkNode_ConditionalThrearDistance.threatDistance = threatDistance;
		return thinkNode_ConditionalThrearDistance;
	}
}

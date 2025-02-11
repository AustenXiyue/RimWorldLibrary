using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Milira;

public class CompDrawCommandRadius : ThingComp
{
	private CompProperties_DrawCommandRadius Props => (CompProperties_DrawCommandRadius)props;

	private Pawn PawnOwner
	{
		get
		{
			if (parent is Pawn result)
			{
				return result;
			}
			return null;
		}
	}

	public bool AnySelectedDraftedMechs
	{
		get
		{
			List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
			for (int i = 0; i < selectedPawns.Count; i++)
			{
				if (selectedPawns[i].RaceProps.IsMechanoid && selectedPawns[i].Drafted && selectedPawns[i].Faction.IsPlayer)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void PostDraw()
	{
		DrawCommandRadius();
	}

	public void DrawCommandRadius()
	{
		if (!PawnOwner.Downed && PawnOwner.Spawned && AnySelectedDraftedMechs && PawnOwner.Faction.IsPlayer)
		{
			GenDraw.DrawRadiusRing(PawnOwner.Position, 24.9f, Color.white, (IntVec3 c) => Milian_KnightClassCanCommandToAnywhere_Patch.CanCommandTo(PawnOwner, c));
		}
	}
}

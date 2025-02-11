using AncotLibrary;
using RimWorld;
using Verse;

namespace Milira;

public class CompAdditionalGraphic_Ablity : CompAdditionalGraphic
{
	public override void CompTick()
	{
		base.CompTick();
		Pawn pawn = parent as Pawn;
		if (pawn.abilities.AllAbilitiesForReading.Any((Ability a) => a.def.category == MiliraDefOf.Milian_UnitAssist && a.CanCast && !a.GizmoDisabled(out var _)))
		{
			drawGraphic = true;
		}
		else
		{
			drawGraphic = false;
		}
	}
}

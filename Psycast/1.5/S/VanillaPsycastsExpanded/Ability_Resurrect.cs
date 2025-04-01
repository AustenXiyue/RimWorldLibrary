using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded;

public class Ability_Resurrect : Ability_TargetCorpse
{
	public override Gizmo GetGizmo()
	{
		Gizmo gizmo = ((Ability)this).GetGizmo();
		IEnumerable<BodyPartRecord> source = from x in ((Ability)this).pawn.health.hediffSet.GetNotMissingParts()
			where x.def == VPE_DefOf.Finger
			select x;
		if (source.All((BodyPartRecord finger) => ((Ability)this).pawn.health.hediffSet.hediffs.Any((Hediff hediff) => hediff.def == VPE_DefOf.VPE_Sacrificed && hediff.Part == finger)))
		{
			gizmo.Disable("VPE.NoAvailableFingers".Translate());
		}
		return gizmo;
	}

	public override void Cast(params GlobalTargetInfo[] targets)
	{
		((Ability)this).Cast(targets);
		foreach (GlobalTargetInfo globalTargetInfo in targets)
		{
			IEnumerable<BodyPartRecord> source = from x in ((Ability)this).pawn.health.hediffSet.GetNotMissingParts()
				where x.def == VPE_DefOf.Finger
				select x;
			if (source.Where((BodyPartRecord finger) => !((Ability)this).pawn.health.hediffSet.hediffs.Any((Hediff hediff) => hediff.def == VPE_DefOf.VPE_Sacrificed && hediff.Part == finger)).TryRandomElement(out var result))
			{
				Corpse corpse = globalTargetInfo.Thing as Corpse;
				SoulFromSky soulFromSky = SkyfallerMaker.MakeSkyfaller(VPE_DefOf.VPE_SoulFromSky) as SoulFromSky;
				soulFromSky.target = corpse;
				GenPlace.TryPlaceThing(soulFromSky, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
				((Ability)this).pawn.health.AddHediff(HediffMaker.MakeHediff(VPE_DefOf.VPE_Sacrificed, ((Ability)this).pawn, result), result, null);
			}
		}
	}
}

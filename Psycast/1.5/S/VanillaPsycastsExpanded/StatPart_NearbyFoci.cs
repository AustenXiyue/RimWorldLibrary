using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaPsycastsExpanded;

public class StatPart_NearbyFoci : StatPart
{
	public static bool ShouldApply = true;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.Thing != null && req.Pawn != null && ShouldApply && req.Thing.Map != null)
		{
			ShouldApply = false;
			val += AllFociNearby(req.Thing, req.Pawn).Sum(((Thing thing, float value) tuple) => tuple.value);
			ShouldApply = true;
		}
	}

	private static IEnumerable<(Thing thing, float value)> AllFociNearby(Thing main, Pawn pawn)
	{
		HashSet<MeditationFocusDef> focusTypes = main.TryGetComp<CompMeditationFocus>().Props.focusTypes.ToHashSet();
		List<(Thing, List<MeditationFocusDef>, float)> list = (from thing in GenRadialCached.RadialDistinctThingsAround(main.Position, main.Map, MeditationUtility.FocusObjectSearchRadius, useCenter: true)
			let comp = thing.TryGetComp<CompMeditationFocus>()
			where comp != null && comp.CanPawnUse(pawn)
			let value = thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn)
			orderby value descending
			select (thing: thing, focusTypes: comp.Props.focusTypes, value: value)).ToList();
		List<(Thing, float)> list2 = new List<(Thing, float)>();
		foreach (var (item, list3, item2) in list)
		{
			if (list3.Any((MeditationFocusDef type) => !focusTypes.Contains(type)))
			{
				focusTypes.UnionWith(list3);
				list2.Add((item, item2));
			}
		}
		return list2;
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.Thing == null || req.Pawn == null || !ShouldApply || req.Thing.Map == null)
		{
			return "";
		}
		ShouldApply = false;
		List<string> list = (from tuple in AllFociNearby(req.Thing, req.Pawn)
			select tuple.thing.LabelCap + ": " + StatDefOf.MeditationFocusStrength.Worker.ValueToString(tuple.value, finalized: true, ToStringNumberSense.Offset)).ToList();
		ShouldApply = true;
		return (list.Count > 0) ? ((string)("VPE.Nearby".Translate() + ":\n" + list.ToLineList("  ", capitalizeItems: true))) : "";
	}
}

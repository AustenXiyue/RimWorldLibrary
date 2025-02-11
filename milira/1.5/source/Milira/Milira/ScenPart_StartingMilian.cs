using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Milira;

public class ScenPart_StartingMilian : ScenPart
{
	private PawnKindDef mechKind;

	private float overseenByPlayerPawnChance = 1f;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref mechKind, "mechKind");
		Scribe_Values.Look(ref overseenByPlayerPawnChance, "overseenByPlayerPawnChance", 0f);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, 2f * ScenPart.RowHeight + 4f);
		if (Widgets.ButtonText(new Rect(scenPartRect.xMin, scenPartRect.yMin, scenPartRect.width, ScenPart.RowHeight), (mechKind != null) ? mechKind.LabelCap : "RandomMech".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("RandomMech".Translate().CapitalizeFirst(), delegate
			{
				mechKind = null;
			}));
			Find.WindowStack.Add(new FloatMenu(list));
		}
		Rect rect = new Rect(scenPartRect.xMin, scenPartRect.yMin + ScenPart.RowHeight + 4f, scenPartRect.width, ScenPart.RowHeight);
		TaggedString taggedString = "StartOverseenChance".Translate();
		float x = Text.CalcSize(taggedString).x;
		Widgets.Label(new Rect(rect.xMin, rect.yMin, x, ScenPart.RowHeight), taggedString);
		Rect rect2 = new Rect(rect.xMin + x + 4f, rect.yMin, rect.width - x - 4f, ScenPart.RowHeight);
		overseenByPlayerPawnChance = Widgets.HorizontalSlider(rect2, overseenByPlayerPawnChance, 0f, 1f, middleAlignment: false, overseenByPlayerPawnChance.ToStringPercent(), null, null, 0.01f);
	}

	public override void Randomize()
	{
		overseenByPlayerPawnChance = Rand.Range(0f, 1f);
	}

	public override string Summary(Scenario scen)
	{
		return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
	}

	public override IEnumerable<string> GetSummaryListEntries(string tag)
	{
		if (tag == "PlayerStartsWith")
		{
			yield return "Milira_Milian".Translate().CapitalizeFirst() + ": " + mechKind.LabelCap;
		}
	}

	public override IEnumerable<Thing> PlayerStartingThings()
	{
		Pawn mech = PawnGenerator.GeneratePawn(new PawnGenerationRequest(mechKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null));
		if (Rand.Chance(overseenByPlayerPawnChance) && mech.OverseerSubject != null)
		{
			(from p in Find.GameInitData.startingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount)
				where MechanitorUtility.IsMechanitor(p) && p.mechanitor.CanOverseeSubject(mech)
				orderby p.mechanitor.OverseenPawns.Count
				select p).RandomElementWithFallback()?.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
		}
		yield return mech;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((mechKind != null) ? mechKind.GetHashCode() : 0);
	}
}

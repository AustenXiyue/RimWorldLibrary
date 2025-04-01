using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace VanillaPsycastsExpanded.Technomancer;

[HarmonyPatch]
public class Psyring : Apparel
{
	private AbilityDef ability;

	private bool alreadyHad;

	public AbilityDef Ability => ability;

	public bool Added => !alreadyHad;

	public PsycasterPathDef Path => ability.Psycast().path;

	public override string Label => base.Label + " (" + ((Def)(object)ability).LabelCap + ")";

	public void Init(AbilityDef ability)
	{
		this.ability = ability;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look<AbilityDef>(ref ability, "ability");
		Scribe_Values.Look(ref alreadyHad, "alreadyHad", defaultValue: false);
	}

	public override void Notify_Equipped(Pawn pawn)
	{
		base.Notify_Equipped(pawn);
		if (ability == null)
		{
			Log.Warning("[VPE] Psyring present with no ability, destroying.");
			Destroy();
			return;
		}
		CompAbilities comp = ((ThingWithComps)pawn).GetComp<CompAbilities>();
		if (comp != null)
		{
			alreadyHad = comp.HasAbility(ability);
			if (!alreadyHad)
			{
				comp.GiveAbility(ability);
			}
		}
	}

	public override void Notify_Unequipped(Pawn pawn)
	{
		base.Notify_Unequipped(pawn);
		if (ability == null)
		{
			return;
		}
		if (!alreadyHad)
		{
			((ThingWithComps)pawn).GetComp<CompAbilities>().LearnedAbilities.RemoveAll((Ability ab) => ab.def == ability);
		}
		alreadyHad = false;
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
	[HarmonyPostfix]
	public static void EquipConditions(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> opts)
	{
		IntVec3 c = IntVec3.FromVector3(clickPos);
		if (pawn.apparel == null)
		{
			return;
		}
		List<Thing> thingList = c.GetThingList(pawn.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (!(thingList[i] is Psyring psyring))
			{
				continue;
			}
			TaggedString toCheck = "ForceWear".Translate(psyring.LabelShort, psyring);
			FloatMenuOption floatMenuOption = opts.FirstOrDefault((FloatMenuOption x) => x.Label.Contains(toCheck));
			if (floatMenuOption != null)
			{
				if (pawn.Psycasts() == null)
				{
					opts.Remove(floatMenuOption);
					opts.Add(new FloatMenuOption("CannotWear".Translate(psyring.LabelShort, psyring) + " (" + "VPE.NotPsycaster".Translate() + ")", null));
				}
				if (pawn.apparel.WornApparel.OfType<Psyring>().Any())
				{
					opts.Remove(floatMenuOption);
					opts.Add(new FloatMenuOption("CannotWear".Translate(psyring.LabelShort, psyring) + " (" + "VPE.AlreadyPsyring".Translate() + ")", null));
				}
			}
			break;
		}
	}
}

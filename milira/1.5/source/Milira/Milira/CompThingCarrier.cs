using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Milira;

public class CompThingCarrier : ThingComp, IThingHolder
{
	public ThingOwner innerContainer;

	private List<Thing> tmpResources = new List<Thing>();

	private MechCarrierGizmo gizmo;

	public int maxToFill;

	public CompProperties_ThingCarrier Props => (CompProperties_ThingCarrier)props;

	public ThingDef fixedIngredient => Props.fixedIngredient;

	public int IngredientCount => innerContainer.TotalStackCountOfDef(fixedIngredient);

	public int AmountToAutofill => Mathf.Max(0, maxToFill - IngredientCount);

	public bool LowIngredientCount => IngredientCount < 250;

	public float PercentageFull => (float)IngredientCount / (float)Props.maxIngredientCount;

	public override void Initialize(CompProperties props)
	{
		base.props = props;
		innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		if (Props.startingIngredientCount > 0)
		{
			Thing thing = ThingMaker.MakeThing(fixedIngredient);
			thing.stackCount = Props.startingIngredientCount;
			innerContainer.TryAdd(thing, Props.startingIngredientCount);
		}
		maxToFill = Props.startingIngredientCount;
	}

	public void TryRemoveThingInCarrier(int num)
	{
		tmpResources.Clear();
		tmpResources.AddRange(innerContainer);
		for (int i = 0; i < tmpResources.Count; i++)
		{
			Thing thing = innerContainer.Take(tmpResources[i], Mathf.Min(tmpResources[i].stackCount, num));
			num -= thing.stackCount;
			thing.Destroy();
			if (num <= 0)
			{
				break;
			}
		}
		Log.Message("IngredientCount" + IngredientCount);
		tmpResources.Clear();
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		Pawn pawn;
		Pawn pawn2 = (pawn = parent as Pawn);
		if (pawn == null || !pawn2.IsColonyMech || pawn2.GetOverseer() == null)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (Find.Selector.SingleSelectedThing == parent && gizmo == null)
		{
			yield return new ThingCarrierGizmo(this);
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Fill with " + fixedIngredient.label,
			action = delegate
			{
				while (IngredientCount < Props.maxIngredientCount)
				{
					int stackCount = Mathf.Min(Props.maxIngredientCount - IngredientCount, fixedIngredient.stackLimit);
					Thing thing = ThingMaker.MakeThing(fixedIngredient);
					thing.stackCount = stackCount;
					innerContainer.TryAdd(thing, thing.stackCount);
				}
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Empty " + fixedIngredient.label,
			action = delegate
			{
				innerContainer.ClearAndDestroyContents();
			}
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: fix 1200 with " + fixedIngredient.label,
			action = delegate
			{
				innerContainer.ClearAndDestroyContents();
				int stackCount2 = 1200;
				Thing thing2 = ThingMaker.MakeThing(fixedIngredient);
				thing2.stackCount = stackCount2;
				innerContainer.TryAdd(thing2, thing2.stackCount);
			}
		};
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + ("CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst());
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		innerContainer?.ClearAndDestroyContents();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref maxToFill, "maxToFill", 0);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (innerContainer != null)
		{
			innerContainer.ThingOwnerTick();
		}
	}
}

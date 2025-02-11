using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AncotLibrary;

public class CompMechCarrier_Custom : ThingComp, IThingHolder
{
	private const int LowIngredientCountThreshold = 250;

	private int cooldownTicksRemaining;

	private ThingOwner innerContainer;

	public List<Pawn> spawnedPawns = new List<Pawn>();

	private MechCarrierGizmo_Custom gizmo;

	public int maxToFill;

	private List<Thing> tmpResources = new List<Thing>();

	public CompProperties_MechCarrier_Custom Props => (CompProperties_MechCarrier_Custom)props;

	public virtual int CostPerPawn => Props.costPerPawn;

	public virtual int CooldownTicks => Props.cooldownTicks;

	public virtual int RecoverTicks => 18000;

	public virtual float RecoverFactor => Props.recoverFactor;

	public CompCommandPivot compCommandPivot => parent.TryGetComp<CompCommandPivot>();

	public Pawn pivot => parent as Pawn;

	public virtual PawnKindDef SpawnPawnKind => Props.spawnPawnKind;

	public AcceptanceReport CanSpawn
	{
		get
		{
			if (pivot != null)
			{
				if (pivot.IsSelfShutdown())
				{
					return "SelfShutdown".Translate();
				}
				if (pivot.Faction == Faction.OfPlayer && !pivot.IsColonyMechPlayerControlled)
				{
					return false;
				}
				if (!pivot.Awake() || pivot.Downed || pivot.Dead || !pivot.Spawned)
				{
					return false;
				}
			}
			if (MaxCanSpawn <= 0)
			{
				return "MechCarrierNotEnoughResources".Translate();
			}
			if (cooldownTicksRemaining > 0)
			{
				return "CooldownTime".Translate() + " " + cooldownTicksRemaining.ToStringSecondsFromTicks();
			}
			return true;
		}
	}

	public AcceptanceReport CanRecover
	{
		get
		{
			if (spawnedPawns.NullOrEmpty())
			{
				return "Ancot.MechCarrierNoPawnToRecover".Translate();
			}
			return true;
		}
	}

	public int IngredientCount => innerContainer.TotalStackCountOfDef(Props.fixedIngredient);

	public int AmountToAutofill => Mathf.Max(0, maxToFill - IngredientCount);

	public int MaxCanSpawn => Mathf.Min(Mathf.FloorToInt(IngredientCount / CostPerPawn), Props.maxPawnsToSpawn);

	public bool LowIngredientCount => IngredientCount < 250;

	public float PercentageFull => (float)IngredientCount / (float)Props.maxIngredientCount;

	public override void Initialize(CompProperties props)
	{
		if (!ModLister.CheckBiotech("Mech carrier"))
		{
			parent.Destroy();
			return;
		}
		base.props = props;
		innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		if (Props.startingIngredientCount > 0)
		{
			Thing thing = ThingMaker.MakeThing(Props.fixedIngredient);
			thing.stackCount = Props.startingIngredientCount;
			innerContainer.TryAdd(thing, Props.startingIngredientCount);
		}
		maxToFill = Props.startingIngredientCount;
	}

	public void TrySpawnPawns()
	{
		int maxCanSpawn = MaxCanSpawn;
		if (maxCanSpawn <= 0)
		{
			return;
		}
		PawnGenerationRequest request = new PawnGenerationRequest(SpawnPawnKind, parent.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn, null, null, null);
		tmpResources.Clear();
		tmpResources.AddRange(innerContainer);
		Lord lord = ((parent is Pawn p) ? p.GetLord() : null);
		for (int i = 0; i < maxCanSpawn; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			GenSpawn.Spawn(pawn, parent.Position, parent.Map);
			if (Props.hediffAddToSpawnPawn != null)
			{
				pawn.health.AddHediff(Props.hediffAddToSpawnPawn, null, null);
			}
			CompCommandTerminal compCommandTerminal = pawn.TryGetComp<CompCommandTerminal>();
			if (compCommandTerminal != null)
			{
				compCommandTerminal.sortie_Terminal = compCommandPivot.sortie;
				if (pawn != null)
				{
					compCommandTerminal.pivot = pivot;
				}
			}
			spawnedPawns.Add(pawn);
			lord?.AddPawn(pawn);
			int num = CostPerPawn;
			for (int j = 0; j < tmpResources.Count; j++)
			{
				if (innerContainer.Contains(tmpResources[j]))
				{
					Thing thing = innerContainer.Take(tmpResources[j], Mathf.Min(tmpResources[j].stackCount, num));
					num -= thing.stackCount;
					thing.Destroy();
					if (num <= 0)
					{
						break;
					}
				}
			}
			if (Props.spawnedMechEffecter != null)
			{
				Effecter effecter = new Effecter(Props.spawnedMechEffecter);
				effecter.Trigger(Props.attachSpawnedMechEffecter ? ((TargetInfo)pawn) : new TargetInfo(pawn.Position, pawn.Map), TargetInfo.Invalid);
				effecter.Cleanup();
			}
		}
		tmpResources.Clear();
		cooldownTicksRemaining = CooldownTicks;
		if (Props.spawnEffecter != null)
		{
			Effecter effecter2 = new Effecter(Props.spawnEffecter);
			effecter2.Trigger(Props.attachSpawnedEffecter ? ((TargetInfo)parent) : new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
			effecter2.Cleanup();
		}
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
		if (Find.Selector.SingleSelectedThing == parent)
		{
			if (gizmo == null)
			{
				gizmo = new MechCarrierGizmo_Custom(this);
			}
			yield return gizmo;
		}
		AcceptanceReport canSpawn = CanSpawn;
		Command_ActionWithCooldown act = new Command_ActionWithCooldown
		{
			cooldownPercentGetter = () => Mathf.InverseLerp(CooldownTicks, 0f, cooldownTicksRemaining),
			action = delegate
			{
				TrySpawnPawns();
			},
			hotKey = KeyBindingDefOf.Misc2,
			Disabled = !canSpawn.Accepted,
			disabledReason = canSpawn.Reason,
			icon = ContentFinder<Texture2D>.Get(Props.iconPath),
			defaultLabel = "MechCarrierRelease".Translate(SpawnPawnKind.labelPlural),
			defaultDesc = "MechCarrierDesc".Translate(Props.maxPawnsToSpawn, SpawnPawnKind.labelPlural, SpawnPawnKind.label, CostPerPawn, Props.fixedIngredient.label)
		};
		if (!canSpawn.Reason.NullOrEmpty())
		{
			act.Disable(canSpawn.Reason);
		}
		AcceptanceReport canRecover = CanRecover;
		Command_Action act2 = new Command_Action
		{
			action = delegate
			{
				foreach (Pawn spawnedPawn in spawnedPawns)
				{
					if (!spawnedPawn.Downed && spawnedPawn.Awake())
					{
						if (IngredientCount < Props.maxIngredientCount)
						{
							Thing thing = ThingMaker.MakeThing(Props.fixedIngredient);
							int stackCount = (int)(RecoverFactor * (float)CostPerPawn * spawnedPawn.health.summaryHealth.SummaryHealthPercent);
							thing.stackCount = stackCount;
							innerContainer.TryAdd(thing, thing.stackCount);
						}
						if (Props.spawnEffecter != null)
						{
							Effecter effecter = new Effecter(Props.spawnEffecter);
							effecter.Trigger(Props.attachSpawnedEffecter ? ((TargetInfo)spawnedPawn) : new TargetInfo(spawnedPawn.Position, spawnedPawn.Map), TargetInfo.Invalid);
							effecter.Cleanup();
						}
						spawnedPawn.Destroy();
					}
				}
				spawnedPawns.Clear();
				cooldownTicksRemaining = RecoverTicks;
			},
			Disabled = !canRecover.Accepted,
			disabledReason = canRecover.Reason,
			icon = ContentFinder<Texture2D>.Get(Props.iconPathRecover),
			defaultLabel = "Ancot.MechCarrierRecover".Translate(SpawnPawnKind.labelPlural),
			defaultDesc = "Ancot.MechCarrierRecoverDesc".Translate(Props.maxPawnsToSpawn, SpawnPawnKind.labelPlural, SpawnPawnKind.label, CostPerPawn, Props.fixedIngredient.label, RecoverFactor.ToStringPercent())
		};
		if (DebugSettings.ShowDevGizmos)
		{
			if (cooldownTicksRemaining > 0)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Reset cooldown",
					action = delegate
					{
						cooldownTicksRemaining = 0;
					}
				};
			}
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fill with " + Props.fixedIngredient.label,
				action = delegate
				{
					while (IngredientCount < Props.maxIngredientCount)
					{
						int stackCount2 = Mathf.Min(Props.maxIngredientCount - IngredientCount, Props.fixedIngredient.stackLimit);
						Thing thing2 = ThingMaker.MakeThing(Props.fixedIngredient);
						thing2.stackCount = stackCount2;
						innerContainer.TryAdd(thing2, thing2.stackCount);
					}
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Empty " + Props.fixedIngredient.label,
				action = delegate
				{
					innerContainer.ClearAndDestroyContents();
				}
			};
		}
		yield return act;
		if (Props.recoverable)
		{
			yield return act2;
		}
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
		for (int i = 0; i < spawnedPawns.Count; i++)
		{
			CompCommandTerminal compCommandTerminal = spawnedPawns[i].TryGetComp<CompCommandTerminal>();
			if (compCommandTerminal != null)
			{
				compCommandTerminal.sortie_Terminal = true;
				compCommandTerminal.pivot = null;
			}
			if (Props.killSpawnedPawnIfParentDied && !spawnedPawns[i].Dead)
			{
				spawnedPawns[i].Kill(null);
			}
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		if (!Find.Selector.IsSelected(parent))
		{
			return;
		}
		for (int i = 0; i < spawnedPawns.Count; i++)
		{
			if (!spawnedPawns[i].Dead)
			{
				GenDraw.DrawLineBetween(parent.TrueCenter(), spawnedPawns[i].TrueCenter());
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
		Scribe_Values.Look(ref maxToFill, "maxToFill", 0);
		Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			spawnedPawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (innerContainer != null)
		{
			innerContainer.ThingOwnerTick();
		}
		if (cooldownTicksRemaining > 0)
		{
			cooldownTicksRemaining--;
		}
	}
}

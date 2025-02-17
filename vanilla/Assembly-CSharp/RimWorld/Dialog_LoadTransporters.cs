using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class Dialog_LoadTransporters : Window
{
	private enum Tab
	{
		Pawns,
		Items
	}

	private Map map;

	private List<CompTransporter> transporters;

	private List<TransferableOneWay> transferables;

	private TransferableOneWayWidget pawnsTransfer;

	private TransferableOneWayWidget itemsTransfer;

	private Tab tab;

	private float lastMassFlashTime = -9999f;

	public bool autoLoot;

	private bool massUsageDirty = true;

	private float cachedMassUsage;

	private bool caravanMassUsageDirty = true;

	private float cachedCaravanMassUsage;

	private bool caravanMassCapacityDirty = true;

	private float cachedCaravanMassCapacity;

	private string cachedCaravanMassCapacityExplanation;

	private bool tilesPerDayDirty = true;

	private float cachedTilesPerDay;

	private string cachedTilesPerDayExplanation;

	private bool daysWorthOfFoodDirty = true;

	private Pair<float, float> cachedDaysWorthOfFood;

	private bool foragedFoodPerDayDirty = true;

	private Pair<ThingDef, float> cachedForagedFoodPerDay;

	private string cachedForagedFoodPerDayExplanation;

	private bool visibilityDirty = true;

	private float cachedVisibility;

	private string cachedVisibilityExplanation;

	private const float TitleRectHeight = 35f;

	private const float BottomAreaHeight = 55f;

	private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

	private static List<TabRecord> tabsList = new List<TabRecord>();

	private static List<List<TransferableOneWay>> tmpLeftToLoadCopy = new List<List<TransferableOneWay>>();

	private static Dictionary<TransferableOneWay, int> tmpLeftCountToTransfer = new Dictionary<TransferableOneWay, int>();

	public bool CanChangeAssignedThingsAfterStarting => transporters[0].Props.canChangeAssignedThingsAfterStarting;

	public bool LoadingInProgressOrReadyToLaunch => transporters[0].LoadingInProgressOrReadyToLaunch;

	public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

	protected override float Margin => 0f;

	private float MassCapacity
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < transporters.Count; i++)
			{
				num += transporters[i].MassCapacity;
			}
			return num;
		}
	}

	private float CaravanMassCapacity
	{
		get
		{
			if (caravanMassCapacityDirty)
			{
				caravanMassCapacityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedCaravanMassCapacity = CollectionsMassCalculator.CapacityTransferables(transferables, stringBuilder);
				cachedCaravanMassCapacityExplanation = stringBuilder.ToString();
			}
			return cachedCaravanMassCapacity;
		}
	}

	private string TransportersLabel
	{
		get
		{
			if (transporters[0].Props.max1PerGroup)
			{
				return transporters[0].parent.Label;
			}
			return Find.ActiveLanguageWorker.Pluralize(transporters[0].parent.Label);
		}
	}

	private string TransportersLabelCap => TransportersLabel.CapitalizeFirst();

	private BiomeDef Biome => map.Biome;

	private float MassUsage
	{
		get
		{
			if (massUsageDirty)
			{
				massUsageDirty = false;
				CompShuttle shuttle = transporters[0].Shuttle;
				cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, shuttle == null || shuttle.requiredColonistCount == 0);
			}
			return cachedMassUsage;
		}
	}

	public float CaravanMassUsage
	{
		get
		{
			if (caravanMassUsageDirty)
			{
				caravanMassUsageDirty = false;
				cachedCaravanMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
			}
			return cachedCaravanMassUsage;
		}
	}

	private float TilesPerDay
	{
		get
		{
			if (tilesPerDayDirty)
			{
				tilesPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, MassUsage, MassCapacity, map.Tile, -1, stringBuilder);
				cachedTilesPerDayExplanation = stringBuilder.ToString();
			}
			return cachedTilesPerDay;
		}
	}

	private Pair<float, float> DaysWorthOfFood
	{
		get
		{
			if (daysWorthOfFoodDirty)
			{
				daysWorthOfFoodDirty = false;
				float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, Faction.OfPlayer);
				cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload));
			}
			return cachedDaysWorthOfFood;
		}
	}

	private Pair<ThingDef, float> ForagedFoodPerDay
	{
		get
		{
			if (foragedFoodPerDayDirty)
			{
				foragedFoodPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables, Biome, Faction.OfPlayer, stringBuilder);
				cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
			}
			return cachedForagedFoodPerDay;
		}
	}

	private float Visibility
	{
		get
		{
			if (visibilityDirty)
			{
				visibilityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
				cachedVisibilityExplanation = stringBuilder.ToString();
			}
			return cachedVisibility;
		}
	}

	public Dialog_LoadTransporters(Map map, List<CompTransporter> transporters)
	{
		this.map = map;
		this.transporters = new List<CompTransporter>();
		this.transporters.AddRange(transporters);
		forcePause = true;
		absorbInputAroundWindow = true;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		CalculateAndRecacheTransferables();
		if (CanChangeAssignedThingsAfterStarting && LoadingInProgressOrReadyToLaunch)
		{
			SetLoadedItemsToLoad();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Rect rect = new Rect(0f, 0f, inRect.width, 35f);
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect, "LoadTransporters".Translate(TransportersLabel));
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		if (transporters[0].Props.showOverallStats)
		{
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, "", TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation, CaravanMassUsage, CaravanMassCapacity, cachedCaravanMassCapacityExplanation), null, map.Tile, null, lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), lerpMassColor: false);
			inRect.yMin += 52f;
		}
		tabsList.Clear();
		tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
		{
			tab = Tab.Pawns;
		}, tab == Tab.Pawns));
		tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
		{
			tab = Tab.Items;
		}, tab == Tab.Items));
		inRect.yMin += 67f;
		Widgets.DrawMenuSection(inRect);
		TabDrawer.DrawTabs(inRect, tabsList);
		inRect = inRect.ContractedBy(17f);
		Widgets.BeginGroup(inRect);
		Rect rect2 = inRect.AtZero();
		DoBottomButtons(rect2);
		Rect inRect2 = rect2;
		inRect2.yMax -= 59f;
		bool anythingChanged = false;
		switch (tab)
		{
		case Tab.Pawns:
			pawnsTransfer.OnGUI(inRect2, out anythingChanged);
			break;
		case Tab.Items:
			itemsTransfer.OnGUI(inRect2, out anythingChanged);
			break;
		}
		if (anythingChanged)
		{
			CountToTransferChanged();
		}
		Widgets.EndGroup();
	}

	public override bool CausesMessageBackground()
	{
		return true;
	}

	private void AddToTransferables(Thing t)
	{
		TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
		if (transferableOneWay == null)
		{
			transferableOneWay = new TransferableOneWay();
			transferables.Add(transferableOneWay);
		}
		if (transferableOneWay.things.Contains(t))
		{
			Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
		}
		else
		{
			transferableOneWay.things.Add(t);
		}
	}

	private bool ErrorsRequireConfirmation(List<Pawn> pawns)
	{
		if (CaravanMassUsage > CaravanMassCapacity && CaravanMassCapacity != 0f && (transporters[0].Shuttle == null || transporters[0].Shuttle.shipParent == null || !transporters[0].Shuttle.shipParent.HasPredeterminedDestination))
		{
			if (transporters[0].parent != null && transporters[0].parent.def == ThingDefOf.TransportPod)
			{
				return pawns.Any((Pawn x) => x.IsFreeColonist);
			}
			return true;
		}
		return false;
	}

	private void DoBottomButtons(Rect rect)
	{
		Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
		if (Widgets.ButtonText(rect2, autoLoot ? "LoadSelected".Translate() : "AcceptButton".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			if (ErrorsRequireConfirmation(pawnsFromTransferables))
			{
				if (CheckForErrors(pawnsFromTransferables))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("TransportersCaravanWillBeImmobile".Translate(), delegate
					{
						if (TryAccept())
						{
							if (autoLoot)
							{
								LoadInstantly();
							}
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
							Close(doCloseSound: false);
						}
					}));
				}
			}
			else if (TryAccept())
			{
				if (autoLoot)
				{
					LoadInstantly();
				}
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				Close(doCloseSound: false);
			}
		}
		if (Widgets.ButtonText(new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			CalculateAndRecacheTransferables();
		}
		if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			Close();
		}
		if (Prefs.DevMode)
		{
			float width = 200f;
			float num = BottomButtonSize.y / 2f;
			if (!LoadingInProgressOrReadyToLaunch && Widgets.ButtonText(new Rect(0f, rect.height - 55f, width, num), "DEV: Load instantly", drawBackground: true, doMouseoverSound: true, active: true, null) && DebugTryLoadInstantly())
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				Close(doCloseSound: false);
			}
			if (Widgets.ButtonText(new Rect(0f, rect.height - 55f + num, width, num), "DEV: Select everything", drawBackground: true, doMouseoverSound: true, active: true, null))
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				SetToLoadEverything();
			}
		}
	}

	private void CalculateAndRecacheTransferables()
	{
		transferables = new List<TransferableOneWay>();
		AddPawnsToTransferables();
		AddItemsToTransferables();
		if (CanChangeAssignedThingsAfterStarting && LoadingInProgressOrReadyToLaunch)
		{
			for (int i = 0; i < transporters.Count; i++)
			{
				for (int j = 0; j < transporters[i].innerContainer.Count; j++)
				{
					AddToTransferables(transporters[i].innerContainer[j]);
				}
			}
			foreach (Thing item in TransporterUtility.ThingsBeingHauledTo(transporters, map))
			{
				AddToTransferables(item);
			}
		}
		pawnsTransfer = new TransferableOneWayWidget(null, null, null, "FormCaravanColonyThingCountTip".Translate(), drawMass: true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMassInMassUsage: true, () => MassCapacity - MassUsage, 0f, ignoreSpawnedCorpseGearAndInventoryMass: false, map.Tile, drawMarketValue: true, drawEquippedWeapon: true, drawNutritionEatenPerDay: true, drawMechEnergy: false, drawItemNutrition: false, drawForagedFoodPerDay: true);
		CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
		itemsTransfer = new TransferableOneWayWidget(transferables.Where((TransferableOneWay x) => x.ThingDef.category != ThingCategory.Pawn), null, null, "FormCaravanColonyThingCountTip".Translate(), drawMass: true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMassInMassUsage: true, () => MassCapacity - MassUsage, 0f, ignoreSpawnedCorpseGearAndInventoryMass: false, map.Tile, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawMechEnergy: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);
		CountToTransferChanged();
	}

	private bool DebugTryLoadInstantly()
	{
		TransporterUtility.InitiateLoading(transporters);
		int i;
		for (i = 0; i < transferables.Count; i++)
		{
			TransferableUtility.Transfer(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing splitPiece, IThingHolder originalThing)
			{
				transporters[i % transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece);
			});
		}
		return true;
	}

	private void LoadInstantly()
	{
		TransporterUtility.InitiateLoading(transporters);
		int i;
		for (i = 0; i < transferables.Count; i++)
		{
			TransferableUtility.Transfer(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing splitPiece, IThingHolder originalThing)
			{
				transporters[i % transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece.TryMakeMinified());
			});
		}
	}

	private bool TryAccept()
	{
		List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
		if (!CheckForErrors(pawnsFromTransferables))
		{
			return false;
		}
		if (LoadingInProgressOrReadyToLaunch)
		{
			AssignTransferablesToRandomTransporters();
			TransporterUtility.MakeLordsAsAppropriate(pawnsFromTransferables, transporters, map);
			IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter && transporters.Contains(((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter))
				{
					allPawnsSpawned[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
		else
		{
			TransporterUtility.InitiateLoading(transporters);
			AssignTransferablesToRandomTransporters();
			TransporterUtility.MakeLordsAsAppropriate(pawnsFromTransferables, transporters, map);
			if (transporters[0].Props.max1PerGroup)
			{
				Messages.Message("MessageTransporterSingleLoadingProcessStarted".Translate(), transporters[0].parent, MessageTypeDefOf.TaskCompletion, historical: false);
			}
			else
			{
				Messages.Message("MessageTransportersLoadingProcessStarted".Translate(), transporters[0].parent, MessageTypeDefOf.TaskCompletion, historical: false);
			}
		}
		return true;
	}

	private void SetLoadedItemsToLoad()
	{
		for (int i = 0; i < transporters.Count; i++)
		{
			int j;
			for (j = 0; j < transporters[i].innerContainer.Count; j++)
			{
				TransferableOneWay transferableOneWay = transferables.Find((TransferableOneWay x) => x.things.Contains(transporters[i].innerContainer[j]));
				if (transferableOneWay != null && transferableOneWay.CanAdjustBy(transporters[i].innerContainer[j].stackCount).Accepted)
				{
					transferableOneWay.AdjustBy(transporters[i].innerContainer[j].stackCount);
				}
			}
			if (transporters[i].leftToLoad == null)
			{
				continue;
			}
			for (int k = 0; k < transporters[i].leftToLoad.Count; k++)
			{
				TransferableOneWay transferableOneWay2 = transporters[i].leftToLoad[k];
				if (transferableOneWay2.CountToTransfer != 0 && transferableOneWay2.HasAnyThing)
				{
					TransferableOneWay transferableOneWay3 = TransferableUtility.TransferableMatchingDesperate(transferableOneWay2.AnyThing, transferables, TransferAsOneMode.PodsOrCaravanPacking);
					if (transferableOneWay3 != null && transferableOneWay3.CanAdjustBy(transferableOneWay2.CountToTransferToDestination).Accepted)
					{
						transferableOneWay3.AdjustBy(transferableOneWay2.CountToTransferToDestination);
					}
				}
			}
		}
	}

	private void AssignTransferablesToRandomTransporters()
	{
		tmpLeftToLoadCopy.Clear();
		for (int i = 0; i < transporters.Count; i++)
		{
			tmpLeftToLoadCopy.Add((transporters[i].leftToLoad != null) ? transporters[i].leftToLoad.ToList() : new List<TransferableOneWay>());
			if (transporters[i].leftToLoad != null)
			{
				transporters[i].leftToLoad.Clear();
			}
		}
		tmpLeftCountToTransfer.Clear();
		for (int j = 0; j < transferables.Count; j++)
		{
			tmpLeftCountToTransfer.Add(transferables[j], transferables[j].CountToTransfer);
		}
		if (LoadingInProgressOrReadyToLaunch)
		{
			int k;
			for (k = 0; k < transferables.Count; k++)
			{
				if (!transferables[k].HasAnyThing || tmpLeftCountToTransfer[transferables[k]] <= 0)
				{
					continue;
				}
				for (int l = 0; l < tmpLeftToLoadCopy.Count; l++)
				{
					TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(transferables[k].AnyThing, tmpLeftToLoadCopy[l], TransferAsOneMode.PodsOrCaravanPacking);
					if (transferableOneWay != null)
					{
						int num = Mathf.Min(tmpLeftCountToTransfer[transferables[k]], transferableOneWay.CountToTransfer);
						if (num > 0)
						{
							transporters[l].AddToTheToLoadList(transferables[k], num);
							tmpLeftCountToTransfer[transferables[k]] -= num;
						}
					}
					Thing thing = transporters[l].innerContainer.FirstOrDefault((Thing x) => TransferableUtility.TransferAsOne(transferables[k].AnyThing, x, TransferAsOneMode.PodsOrCaravanPacking));
					if (thing != null)
					{
						int num2 = Mathf.Min(tmpLeftCountToTransfer[transferables[k]], thing.stackCount);
						if (num2 > 0)
						{
							transporters[l].AddToTheToLoadList(transferables[k], num2);
							tmpLeftCountToTransfer[transferables[k]] -= num2;
						}
					}
				}
			}
		}
		tmpLeftToLoadCopy.Clear();
		if (transferables.Any())
		{
			transferables.SortByDescending((TransferableOneWay x) => x.AnyThing.GetStatValue(StatDefOf.Mass) * (float)tmpLeftCountToTransfer[x]);
			foreach (TransferableOneWay transferable in transferables)
			{
				if (tmpLeftCountToTransfer[transferable] == 0)
				{
					continue;
				}
				TransferableOneWay transferableOneWay2 = transferable;
				int num3 = tmpLeftCountToTransfer[transferableOneWay2];
				int num4 = Mathf.CeilToInt((float)num3 / (float)transporters.Count);
				int num5 = int.MaxValue;
				int num6 = 0;
				for (int m = 0; m < transporters.Count; m++)
				{
					if (transporters[m].leftToLoad == null)
					{
						num6 = m;
						break;
					}
					if (transporters[m].leftToLoad.Count < num5)
					{
						num5 = transporters[m].leftToLoad.Count;
						num6 = m;
					}
				}
				for (int n = 0; n < transporters.Count; n++)
				{
					if (num3 == 0)
					{
						break;
					}
					int num7 = ((n == transporters.Count - 1) ? num3 : num4);
					if (num7 > 0)
					{
						transporters[num6].AddToTheToLoadList(transferableOneWay2, num7);
					}
					num3 -= num7;
					num6 = (num6 + 1) % transporters.Count;
				}
				tmpLeftCountToTransfer[transferableOneWay2] = 0;
			}
		}
		tmpLeftCountToTransfer.Clear();
		for (int num8 = 0; num8 < transporters.Count; num8++)
		{
			for (int num9 = 0; num9 < transporters[num8].innerContainer.Count; num9++)
			{
				Thing thing2 = transporters[num8].innerContainer[num9];
				int num10 = transporters[num8].SubtractFromToLoadList(thing2, thing2.stackCount, sendMessageOnFinished: false);
				if (num10 < thing2.stackCount)
				{
					transporters[num8].innerContainer.TryDrop(thing2, ThingPlaceMode.Near, thing2.stackCount - num10, out var _);
				}
			}
		}
	}

	private bool CheckForErrors(List<Pawn> pawns)
	{
		if (!CanChangeAssignedThingsAfterStarting && !transferables.Any((TransferableOneWay x) => x.CountToTransfer != 0))
		{
			if (transporters[0].Props.max1PerGroup)
			{
				Messages.Message("CantSendEmptyTransporterSingle".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("CantSendEmptyTransportPods".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (transporters[0].Props.max1PerGroup)
		{
			CompShuttle shuttle = transporters[0].Shuttle;
			if (shuttle != null && shuttle.requiredColonistCount > 0 && pawns.Count > shuttle.requiredColonistCount)
			{
				Messages.Message("TransporterSingleTooManyColonists".Translate(shuttle.requiredColonistCount), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
		}
		if (MassUsage > MassCapacity)
		{
			FlashMass();
			if (transporters[0].Props.max1PerGroup)
			{
				Messages.Message("TooBigTransporterSingleMassUsage".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("TooBigTransportersMassUsage".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		Pawn pawn = pawns.Find((Pawn x) => !x.MapHeld.reachability.CanReach(x.PositionHeld, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)) && !transporters.Any((CompTransporter y) => y.innerContainer.Contains(x)));
		if (pawn != null)
		{
			if (transporters[0].Props.max1PerGroup)
			{
				Messages.Message("PawnCantReachTransporterSingle".Translate(pawn.LabelShort, pawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("PawnCantReachTransporters".Translate(pawn.LabelShort, pawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		Map map = transporters[0].parent.Map;
		for (int i = 0; i < transferables.Count; i++)
		{
			if (transferables[i].ThingDef.category != ThingCategory.Item)
			{
				continue;
			}
			int countToTransfer = transferables[i].CountToTransfer;
			int num = 0;
			if (countToTransfer <= 0)
			{
				continue;
			}
			for (int j = 0; j < transferables[i].things.Count; j++)
			{
				Thing t = transferables[i].things[j];
				Pawn_CarryTracker pawn_CarryTracker = t.ParentHolder as Pawn_CarryTracker;
				if (map.reachability.CanReach(t.Position, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)) || transporters.Any((CompTransporter x) => x.innerContainer.Contains(t)) || (pawn_CarryTracker != null && pawn_CarryTracker.pawn.MapHeld.reachability.CanReach(pawn_CarryTracker.pawn.PositionHeld, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors))))
				{
					num += t.stackCount;
					if (num >= countToTransfer)
					{
						break;
					}
				}
			}
			if (num >= countToTransfer)
			{
				continue;
			}
			if (countToTransfer == 1)
			{
				if (transporters[0].Props.max1PerGroup)
				{
					Messages.Message("TransporterSingleItemIsUnreachableSingle".Translate(transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					Messages.Message("TransporterItemIsUnreachableSingle".Translate(transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			else if (transporters[0].Props.max1PerGroup)
			{
				Messages.Message("TransporterSingleItemIsUnreachableMulti".Translate(countToTransfer, transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("TransporterItemIsUnreachableMulti".Translate(countToTransfer, transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	private void AddPawnsToTransferables()
	{
		foreach (Pawn item in TransporterUtility.AllSendablePawns(transporters, map, autoLoot))
		{
			AddToTransferables(item);
		}
	}

	private void AddItemsToTransferables()
	{
		foreach (Thing item in TransporterUtility.AllSendableItems(transporters, map, autoLoot))
		{
			AddToTransferables(item);
		}
	}

	private void FlashMass()
	{
		lastMassFlashTime = Time.time;
	}

	private void SetToLoadEverything()
	{
		for (int i = 0; i < transferables.Count; i++)
		{
			transferables[i].AdjustTo(transferables[i].GetMaximumToTransfer());
		}
		CountToTransferChanged();
	}

	private void CountToTransferChanged()
	{
		massUsageDirty = true;
		caravanMassUsageDirty = true;
		caravanMassCapacityDirty = true;
		tilesPerDayDirty = true;
		daysWorthOfFoodDirty = true;
		foragedFoodPerDayDirty = true;
		visibilityDirty = true;
	}
}

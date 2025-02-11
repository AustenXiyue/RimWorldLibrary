using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using CombatExtended.RocketGUI;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatExtended;

public class ITab_Inventory : ITab_Pawn_Gear
{
	private const float _barHeight = 20f;

	private const float _margin = 15f;

	private const float _thingIconSize = 28f;

	private const float _thingLeftX = 36f;

	private const float _thingRowHeight = 28f;

	private const float _topPadding = 20f;

	private static readonly CachedTexture _iconEditAttachments = new CachedTexture("UI/Icons/gear");

	private const float _standardLineHeight = 22f;

	private static readonly Color _highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private Vector2 _scrollPosition = Vector2.zero;

	private Dictionary<BodyPartRecord, float> sharpArmorCache = new Dictionary<BodyPartRecord, float>();

	private Dictionary<BodyPartRecord, float> bluntArmorCache = new Dictionary<BodyPartRecord, float>();

	private Dictionary<BodyPartRecord, float> heatArmorCache = new Dictionary<BodyPartRecord, float>();

	private int lastArmorTooltipTick = 0;

	private Pawn lastArmorTooltipPawn = null;

	private float _scrollViewHeight;

	public ITab_Inventory()
	{
		size = new Vector2(480f, 550f);
	}

	public override void FillTab()
	{
		CompInventory compInventory = SelPawn.TryGetComp<CompInventory>();
		Rect position = new Rect(15f, 20f, size.x - 30f, size.y - 20f - 15f);
		if (compInventory != null)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_InventoryWeightBulk, KnowledgeAmount.FrameDisplayed);
			position.height -= 55f;
			Rect rect = new Rect(15f, position.yMax + 7.5f, position.width, 20f);
			Rect rect2 = new Rect(15f, rect.yMax + 7.5f, position.width, 20f);
			Utility_Loadouts.DrawBar(rect2, compInventory.currentBulk, compInventory.capacityBulk, "CE_Bulk".Translate(), SelPawn.GetBulkTip());
			Utility_Loadouts.DrawBar(rect, compInventory.currentWeight, compInventory.capacityWeight, "CE_Weight".Translate(), SelPawn.GetWeightTip());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			string text = CE_StatDefOf.CarryBulk.ValueToString(compInventory.currentBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
			string text2 = CE_StatDefOf.CarryBulk.ValueToString(compInventory.capacityBulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
			Widgets.Label(rect2, text + "/" + text2);
			string text3 = compInventory.currentWeight.ToString("0.#");
			string text4 = CE_StatDefOf.CarryWeight.ValueToString(compInventory.capacityWeight, CE_StatDefOf.CarryWeight.toStringNumberSense);
			Widgets.Label(rect, text3 + "/" + text4);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		GUI.BeginGroup(position);
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		Rect outRect = new Rect(0f, 0f, position.width, position.height);
		Rect viewRect = new Rect(0f, 0f, position.width - 16f, _scrollViewHeight);
		Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
		float curY = 0f;
		TryDrawComfyTemperatureRange(ref curY, viewRect.width);
		if (ShouldShowOverallArmorCE(base.SelPawnForGear))
		{
			Widgets.ListSeparator(ref curY, viewRect.width, "OverallArmor".Translate());
			int ticksAbs = Find.TickManager.TicksAbs;
			if (base.SelPawnForGear != lastArmorTooltipPawn || lastArmorTooltipTick != ticksAbs)
			{
				RebuildArmorCache(sharpArmorCache, StatDefOf.ArmorRating_Sharp);
				RebuildArmorCache(bluntArmorCache, StatDefOf.ArmorRating_Blunt);
				RebuildArmorCache(heatArmorCache, StatDefOf.ArmorRating_Heat);
				lastArmorTooltipPawn = base.SelPawnForGear;
				lastArmorTooltipTick = ticksAbs;
			}
			TryDrawOverallArmor(bluntArmorCache, ref curY, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), " " + "CE_MPa".Translate());
			TryDrawOverallArmor(sharpArmorCache, ref curY, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), "CE_mmRHA".Translate());
			TryDrawOverallArmor(heatArmorCache, ref curY, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), "%");
		}
		if (ShouldShowEquipment(base.SelPawnForGear))
		{
			bool flag = false;
			Loadout loadout = base.SelPawnForGear.GetLoadout();
			if (base.SelPawnForGear.IsColonist && (loadout == null || loadout.Slots.NullOrEmpty()) && (base.SelPawnForGear.inventory.innerContainer.Any() || base.SelPawnForGear.equipment?.Primary != null))
			{
				flag = true;
			}
			if (flag)
			{
				curY += 3f;
			}
			float y = curY;
			Widgets.ListSeparator(ref curY, viewRect.width, "Equipment".Translate());
			foreach (ThingWithComps item in base.SelPawnForGear.equipment.AllEquipmentListForReading)
			{
				DrawThingRowCE(ref curY, viewRect.width, item);
			}
			if (flag)
			{
				Rect rect3 = new Rect(viewRect.width / 2f, y, viewRect.width / 2f, 26f);
				Color color = GUI.color;
				TextAnchor anchor = Text.Anchor;
				GUI.color = Color.cyan;
				if (Mouse.IsOver(rect3))
				{
					GUI.color = Color.white;
				}
				Text.Anchor = TextAnchor.UpperRight;
				Widgets.Label(rect3, "CE_MakeLoadout".Translate());
				if (Widgets.ButtonInvisible(rect3.ContractedBy(2f)))
				{
					SyncedAddLoadout(base.SelPawnForGear);
				}
				Text.Anchor = anchor;
				GUI.color = color;
			}
		}
		if (ShouldShowApparel(base.SelPawnForGear))
		{
			Widgets.ListSeparator(ref curY, viewRect.width, "Apparel".Translate());
			foreach (Apparel item2 in base.SelPawnForGear.apparel.WornApparel.OrderByDescending((Apparel ap) => ap.def.apparel.bodyPartGroups[0].listOrder))
			{
				DrawThingRowCE(ref curY, viewRect.width, item2);
			}
		}
		if (ShouldShowInventory(base.SelPawnForGear))
		{
			Widgets.ListSeparator(ref curY, viewRect.width, "Inventory".Translate());
			ITab_Pawn_Gear.workingInvList.Clear();
			ITab_Pawn_Gear.workingInvList.AddRange(base.SelPawnForGear.inventory.innerContainer);
			for (int i = 0; i < ITab_Pawn_Gear.workingInvList.Count; i++)
			{
				DrawThingRowCE(ref curY, viewRect.width, ITab_Pawn_Gear.workingInvList[i], showDropButtonIfPrisoner: true);
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			_scrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public void DrawThingRowCE(ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
	{
		Rect rect = new Rect(0f, y, width, 28f);
		Widgets.InfoCardButton(rect.width - 24f, y, thing.GetInnerIfMinified());
		rect.width -= 24f;
		if ((base.CanControl || (base.SelPawnForGear.Faction == Faction.OfPlayer && base.SelPawnForGear.RaceProps.packAnimal) || (showDropButtonIfPrisoner && base.SelPawnForGear.IsPrisonerOfColony)) && !base.SelPawnForGear.IsItemMechanoidWeapon(thing))
		{
			bool flag = base.SelPawnForGear.IsItemQuestLocked(thing);
			Color baseColor = (flag ? Color.grey : Color.white);
			Color mouseoverColor = (flag ? Color.grey : GenUI.MouseoverColor);
			Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
			TooltipHandler.TipRegion(rect2, flag ? "DropThingLocked".Translate() : "DropThing".Translate());
			if (Widgets.ButtonImage(rect2, TexButton.Drop, baseColor, mouseoverColor) && !flag)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				SyncedInterfaceDrop(thing);
			}
			rect.width -= 24f;
		}
		if (base.CanControlColonist && FoodUtility.WillIngestFromInventoryNow(base.SelPawn, thing))
		{
			Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
			TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(thing.LabelNoCount, thing));
			if (Widgets.ButtonImage(rect3, TexButton.Ingest))
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				SyncedInterfaceIngest(thing);
			}
			rect.width -= 24f;
		}
		if (thing == SelPawn.equipment?.Primary && thing is WeaponPlatform weapon)
		{
			Rect rect4 = new Rect(rect.width - 24f, y, 24f, 24f);
			TooltipHandler.TipRegion(rect4, "CE_EditWeapon".Translate());
			if (Widgets.ButtonImage(rect4, _iconEditAttachments.Texture))
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				if (!Find.WindowStack.IsOpen<Window_AttachmentsEditor>())
				{
					Find.WindowStack.Add(new Window_AttachmentsEditor(weapon));
				}
				CloseTab();
			}
			rect.width -= 24f;
		}
		Rect rect5 = rect;
		rect5.xMin = rect5.xMax - 60f;
		CaravanThingsTabUtility.DrawMass(thing, rect5);
		rect.width -= 60f;
		if (Mouse.IsOver(rect))
		{
			GUI.color = _highlightColor;
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
		if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
		{
			if (thing is WeaponPlatform weapon2)
			{
				global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(new Rect(4f, y, 28f, 28f), weapon2, null, null);
			}
			else
			{
				Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f, null);
			}
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		GUI.color = ITab_Pawn_Gear.ThingLabelColor;
		Rect rect6 = new Rect(36f, y, rect.width - 36f, 28f);
		string text = thing.LabelCap;
		if ((thing is Apparel && base.SelPawnForGear.outfits != null && base.SelPawnForGear.outfits.forcedHandler.IsForced((Apparel)thing)) || (base.SelPawnForGear.inventory != null && base.SelPawnForGear.HoldTrackerIsHeld(thing)))
		{
			text = text + ", " + "ApparelForcedLower".Translate();
		}
		Text.WordWrap = false;
		Widgets.Label(rect6, text.Truncate(rect6.width));
		Text.WordWrap = true;
		string text2 = string.Concat(new object[5]
		{
			thing.LabelCap,
			"\n",
			thing.DescriptionDetailed,
			"\n",
			thing.GetWeightAndBulkTip()
		});
		if (thing.def.useHitPoints)
		{
			string text3 = text2;
			text2 = string.Concat(text3, "\n", "HitPointsBasic".Translate().CapitalizeFirst(), ": ", thing.HitPoints, " / ", thing.MaxHitPoints);
		}
		TooltipHandler.TipRegion(rect6, text2);
		y += 28f;
		if (!Widgets.ButtonInvisible(rect6) || Event.current.button != 1)
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("ThingInfo".Translate(), delegate
		{
			Find.WindowStack.Add(new Dialog_InfoCard(thing));
		}));
		if (base.CanControl)
		{
			ThingWithComps eq = thing as ThingWithComps;
			if (eq != null && eq.TryGetComp<CompEquippable>() != null)
			{
				CompInventory compInventory = base.SelPawnForGear.TryGetComp<CompInventory>();
				CompBiocodable compBiocodable = eq.TryGetComp<CompBiocodable>();
				if (compInventory != null)
				{
					string text4 = GenLabel.ThingLabel(eq.def, eq.Stuff);
					FloatMenuOption item;
					if (compBiocodable != null && compBiocodable.Biocoded && compBiocodable.CodedPawn != base.SelPawnForGear)
					{
						item = new FloatMenuOption("CannotEquip".Translate(text4) + ": " + "BiocodedCodedForSomeoneElse".Translate(), null);
					}
					else if (base.SelPawnForGear.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(eq, base.SelPawnForGear))
					{
						TaggedString taggedString = (base.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(eq) ? "CE_CannotPutAway".Translate(text4) : "CannotEquip".Translate(text4));
						item = new FloatMenuOption(taggedString + ": " + "CE_CannotChangeEquipment".Translate(), null);
					}
					else if (base.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(eq) && base.SelPawnForGear.inventory != null)
					{
						item = new FloatMenuOption("CE_PutAway".Translate(text4), delegate
						{
							SyncedTryTransferEquipmentToContainer(base.SelPawnForGear);
						});
					}
					else if (!base.SelPawnForGear.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						item = new FloatMenuOption("CannotEquip".Translate(text4), null);
					}
					else
					{
						string text5 = "Equip".Translate(text4);
						if (eq.def.IsRangedWeapon && base.SelPawnForGear.story != null && base.SelPawnForGear.story.traits.HasTrait(TraitDefOf.Brawler))
						{
							text5 = text5 + " " + "EquipWarningBrawler".Translate();
						}
						item = new FloatMenuOption(text5, (base.SelPawnForGear.story != null && base.SelPawnForGear.WorkTagIsDisabled(WorkTags.Violent)) ? null : ((Action)delegate
						{
							SyncedTrySwitchToWeapon(compInventory, eq);
						}));
					}
					list.Add(item);
				}
			}
			List<Apparel> list2 = base.SelPawnForGear?.apparel?.WornApparel;
			foreach (Apparel apparel in list2)
			{
				CompApparelReloadable compApparelReloadable = apparel.TryGetComp<CompApparelReloadable>();
				if (compApparelReloadable != null && compApparelReloadable.AmmoDef == thing.def && compApparelReloadable.NeedsReload(allowForcedReload: true) && !base.SelPawnForGear.Drafted)
				{
					FloatMenuOption item2 = new FloatMenuOption("CE_ReloadApparel".Translate(apparel.Label, thing.Label), delegate
					{
						SyncedReloadApparel(base.SelPawnForGear, apparel, thing);
					});
					list.Add(item2);
				}
			}
			if (base.CanControl && thing.IngestibleNow && base.SelPawn.RaceProps.CanEverEat(thing))
			{
				Action action = delegate
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SyncedInterfaceIngest(thing);
				};
				string text6 = (thing.def.ingestible.ingestCommandString.NullOrEmpty() ? ((string)"ConsumeThing".Translate(thing.LabelShort, thing)) : string.Format(thing.def.ingestible.ingestCommandString, thing.LabelShort));
				if (FoodUtility.MoodFromIngesting(base.SelPawnForGear, thing, thing.def) < 0f)
				{
					text6 = string.Format("{0}: ({1})", text6, "WarningFoodDisliked".Translate());
				}
				FloatMenuOption opt = null;
				if (ModsConfig.IdeologyActive && thing.def.IsDrug)
				{
					if (!new HistoryEvent(HistoryEventDefOf.IngestedDrug, base.SelPawnForGear.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text6) && !PawnUtility.CanTakeDrugForDependency(base.SelPawnForGear, thing.def))
					{
						list.Add(opt);
					}
					else if (thing.def.ingestible.drugCategory == DrugCategory.Medical && !new HistoryEvent(HistoryEventDefOf.IngestedRecreationalDrug, base.SelPawnForGear.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text6) && !PawnUtility.CanTakeDrugForDependency(base.SelPawnForGear, thing.def))
					{
						list.Add(opt);
					}
					else if (thing.def.ingestible.drugCategory == DrugCategory.Hard && !new HistoryEvent(HistoryEventDefOf.IngestedHardDrug, base.SelPawnForGear.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text6))
					{
						list.Add(opt);
					}
				}
				if (opt == null)
				{
					if (thing.def.IsNonMedicalDrug && !base.SelPawnForGear.CanTakeDrug(thing.def))
					{
						list.Add(new FloatMenuOption(text6 + ": " + TraitDefOf.DrugDesire.DataAtDegree(-1).GetLabelCapFor(base.SelPawnForGear), null));
					}
					else if (FoodUtility.InappropriateForTitle(thing.def, base.SelPawnForGear, allowIfStarving: true))
					{
						list.Add(new FloatMenuOption(text6 + ": " + "FoodBelowTitleRequirements".Translate(base.SelPawnForGear.royalty.MostSeniorTitle.def.GetLabelFor(base.SelPawnForGear).CapitalizeFirst()).CapitalizeFirst(), null));
					}
					else
					{
						list.Add(new FloatMenuOption(text6, action));
					}
				}
			}
			if (base.SelPawnForGear.IsItemQuestLocked(eq))
			{
				list.Add(new FloatMenuOption("CE_CannotDropThing".Translate() + ": " + "DropThingLocked".Translate(), null));
				list.Add(new FloatMenuOption("CE_CannotDropThingHaul".Translate() + ": " + "DropThingLocked".Translate(), null));
			}
			else if (!base.SelPawnForGear.IsItemMechanoidWeapon(eq))
			{
				list.Add(new FloatMenuOption("DropThing".Translate(), delegate
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SyncedInterfaceDrop(thing);
				}));
				list.Add(new FloatMenuOption("CE_DropThingHaul".Translate(), delegate
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SyncedInterfaceDropHaul(thing);
				}));
			}
			if (base.CanControl && base.SelPawnForGear.HoldTrackerIsHeld(thing))
			{
				Action action2 = delegate
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SyncedHoldTrackerForget(base.SelPawnForGear, thing);
				};
				list.Add(new FloatMenuOption("CE_HoldTrackerForget".Translate(), action2));
			}
		}
		FloatMenu window = new FloatMenu(list, thing.LabelCap);
		Find.WindowStack.Add(window);
	}

	private void RebuildArmorCache(Dictionary<BodyPartRecord, float> armorCache, StatDef stat)
	{
		armorCache.Clear();
		List<Apparel> list = base.SelPawnForGear.apparel?.WornApparel;
		Apparel apparel = list?.FirstOrDefault((Apparel x) => x is Apparel_Shield);
		foreach (BodyPartRecord allPart in base.SelPawnForGear.RaceProps.body.AllParts)
		{
			if (allPart.depth != BodyPartDepth.Outside || (!((double)allPart.coverage >= 0.1) && !allPart.def.tags.Contains(BodyPartTagDefOf.BreathingPathway) && !allPart.def.tags.Contains(BodyPartTagDefOf.SightSource)))
			{
				continue;
			}
			float num = base.SelPawnForGear.PartialStat(stat, allPart);
			if (list != null)
			{
				foreach (Apparel item in list)
				{
					num += item.PartialStat(stat, allPart);
				}
				if (apparel != null && !apparel.def.apparel.CoversBodyPart(allPart) && apparel.def?.GetModExtension<ShieldDefExtension>()?.PartIsCoveredByShield(allPart, base.SelPawnForGear) == true)
				{
					num += apparel.GetStatValue(stat);
				}
			}
			armorCache[allPart] = num;
		}
	}

	private void TryDrawOverallArmor(Dictionary<BodyPartRecord, float> armorCache, ref float curY, float width, StatDef stat, string label, string unit)
	{
		Rect rect = new Rect(0f, curY, width, 22f);
		string text = "";
		float num = 0f;
		float num2 = 0f;
		foreach (KeyValuePair<BodyPartRecord, float> item in armorCache)
		{
			BodyPartRecord key = item.Key;
			float value = item.Value;
			num += value * key.coverage;
			num2 += key.coverage;
			text = text + key.LabelCap + ": ";
			text = text + formatArmorValue(value, unit) + "\n";
		}
		num /= num2;
		TooltipHandler.TipRegion(rect, text);
		Widgets.Label(rect, label.Truncate(200f));
		rect.xMin += 200f;
		Widgets.Label(rect, formatArmorValue(num, unit));
		curY += 22f;
	}

	private void InterfaceDropHaul(Thing t)
	{
		if (base.SelPawnForGear.HoldTrackerIsHeld(t))
		{
			base.SelPawnForGear.HoldTrackerForget(t);
		}
		ThingWithComps thingWithComps = t as ThingWithComps;
		if (t is Apparel apparel && base.SelPawnForGear.apparel != null && base.SelPawnForGear.apparel.WornApparel.Contains(apparel))
		{
			Job job = JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel);
			job.haulDroppedApparel = true;
			base.SelPawnForGear.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
		else if (thingWithComps != null && base.SelPawnForGear.equipment != null && base.SelPawnForGear.equipment.AllEquipmentListForReading.Contains(thingWithComps))
		{
			base.SelPawnForGear.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, thingWithComps), JobTag.Misc);
		}
		else if (!t.def.destroyOnDrop)
		{
			SelPawn.inventory.innerContainer.TryDrop(t, SelPawn.Position, SelPawn.Map, ThingPlaceMode.Near, out var _);
		}
	}

	[Multiplayer.SyncMethod(syncContext = 2)]
	private void SyncedInterfaceIngest(Thing t)
	{
		InterfaceIngest(t);
	}

	[Multiplayer.SyncMethod(syncContext = 2)]
	private void SyncedInterfaceDropHaul(Thing t)
	{
		InterfaceDropHaul(t);
	}

	private void InterfaceIngest(Thing t)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Ingest, t);
		job.count = Mathf.Min(t.stackCount, t.def.ingestible.maxNumToIngestAtOnce);
		job.count = Mathf.Min(job.count, FoodUtility.WillIngestStackCountOf(base.SelPawnForGear, t.def, t.GetStatValue(StatDefOf.Nutrition)));
		base.SelPawnForGear.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	private bool ShouldShowOverallArmorCE(Pawn p)
	{
		return p.RaceProps.Humanlike || ShouldShowApparel(p) || p.GetStatValue(StatDefOf.ArmorRating_Sharp) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Blunt) > 0f || p.GetStatValue(StatDefOf.ArmorRating_Heat) > 0f;
	}

	private string formatArmorValue(float value, string unit)
	{
		bool flag = unit.Equals("%");
		if (flag)
		{
			value *= 100f;
		}
		return value.ToStringByStyle(flag ? ToStringStyle.FloatMaxOne : ToStringStyle.FloatMaxTwo) + unit;
	}

	[Multiplayer.SyncMethod(syncContext = 2)]
	private void SyncedInterfaceDrop(Thing thing)
	{
		InterfaceDrop(thing);
	}

	[Multiplayer.SyncMethod]
	private static void SyncedTryTransferEquipmentToContainer(Pawn p)
	{
		p.equipment.TryTransferEquipmentToContainer(p.equipment.Primary, p.inventory.innerContainer);
	}

	[Multiplayer.SyncMethod]
	private static void SyncedTrySwitchToWeapon(CompInventory compInventory, ThingWithComps eq)
	{
		compInventory.TrySwitchToWeapon(eq);
	}

	[Multiplayer.SyncMethod]
	private static void SyncedReloadApparel(Pawn p, Apparel apparel, Thing thing)
	{
		p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Reload, apparel, thing), JobTag.Misc);
	}

	[Multiplayer.SyncMethod]
	private static void SyncedHoldTrackerForget(Pawn p, Thing thing)
	{
		p.HoldTrackerForget(thing);
	}

	[Multiplayer.SyncMethod]
	private static void SyncedAddLoadout(Pawn p)
	{
		Loadout loadout = p.GenerateLoadoutFromPawn();
		LoadoutManager.AddLoadout(loadout);
		p.SetLoadout(loadout);
		if (Multiplayer.IsExecutingCommandsIssuedBySelf)
		{
			Find.WindowStack.Add(new Dialog_ManageLoadouts(p.GetLoadout()));
		}
	}
}

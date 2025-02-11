using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CombatExtended.Compatibility;
using CombatExtended.RocketGUI;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class Dialog_ManageLoadouts : Window
{
	private class VisibilityCache
	{
		public int ticksToRecheck = 0;

		public bool check = true;

		public int position = 0;
	}

	private class SelectableItem
	{
		public ThingDef thingDef;

		public bool isGreyedOut;
	}

	private static int[] _dropOptions2 = new int[2] { 0, 1 };

	private static Texture2D _darkBackground = SolidColorMaterials.NewSolidColorTexture(0f, 0f, 0f, 0.2f);

	private static Texture2D _iconClear = ContentFinder<Texture2D>.Get("UI/Icons/clear");

	private static Texture2D _iconAmmo = ContentFinder<Texture2D>.Get("UI/Icons/ammo");

	private static Texture2D _iconRanged = ContentFinder<Texture2D>.Get("UI/Icons/ranged");

	private static Texture2D _iconMelee = ContentFinder<Texture2D>.Get("UI/Icons/melee");

	private static Texture2D _iconMinified = ContentFinder<Texture2D>.Get("UI/Icons/minified");

	private static Texture2D _iconGeneric = ContentFinder<Texture2D>.Get("UI/Icons/generic");

	private static Texture2D _iconAll = ContentFinder<Texture2D>.Get("UI/Icons/all");

	private static Texture2D _iconAmmoAdd = ContentFinder<Texture2D>.Get("UI/Icons/ammoAdd");

	private static Texture2D _iconEditAttachments = ContentFinder<Texture2D>.Get("UI/Icons/gear");

	private static Texture2D _iconSearch = ContentFinder<Texture2D>.Get("UI/Icons/search");

	private static Texture2D _iconMove = ContentFinder<Texture2D>.Get("UI/Icons/move");

	private static Texture2D _iconPickupDrop = ContentFinder<Texture2D>.Get("UI/Icons/loadoutPickupDrop");

	private static Texture2D _iconDropExcess = ContentFinder<Texture2D>.Get("UI/Icons/loadoutDropExcess");

	private static Regex validNameRegex = new Regex("^.*$");

	private Vector2 _availableScrollPosition = Vector2.zero;

	private const float _barHeight = 24f;

	private Vector2 _countFieldSize = new Vector2(40f, 24f);

	private Loadout _currentLoadout;

	private LoadoutSlot _draggedSlot;

	private bool _dragging;

	private string _filter = "";

	private const float _iconSize = 16f;

	private const float _margin = 6f;

	private const float _rowHeight = 28f;

	private const float _topAreaHeight = 30f;

	private Vector2 _slotScrollPosition = Vector2.zero;

	private List<SelectableItem> _source;

	private List<LoadoutGenericDef> _sourceGeneric;

	private SourceSelection _sourceType = SourceSelection.Ranged;

	private readonly List<ThingDef> _allSuitableDefs;

	private readonly List<LoadoutGenericDef> _allDefsGeneric;

	private readonly List<SelectableItem> _selectableItems;

	private static readonly Dictionary<LoadoutGenericDef, VisibilityCache> genericVisibility = new Dictionary<LoadoutGenericDef, VisibilityCache>();

	private const int advanceTicks = 1;

	public Loadout CurrentLoadout
	{
		get
		{
			return _currentLoadout;
		}
		set
		{
			if (Multiplayer.InMultiplayer && _currentLoadout != null)
			{
				SyncedSetName(_currentLoadout, _currentLoadout.label);
			}
			_currentLoadout = value;
		}
	}

	public LoadoutSlot Dragging
	{
		get
		{
			if (_dragging)
			{
				return _draggedSlot;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				_dragging = false;
			}
			else
			{
				_dragging = true;
			}
			_draggedSlot = value;
		}
	}

	public override Vector2 InitialSize => new Vector2(1000f, 700f);

	public Dialog_ManageLoadouts(Loadout loadout)
	{
		CurrentLoadout = null;
		if (loadout != null && !loadout.defaultLoadout)
		{
			CurrentLoadout = loadout;
		}
		_allSuitableDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => !td.IsMenuHidden() && IsSuitableThingDef(td)).ToList();
		_allDefsGeneric = DefDatabase<LoadoutGenericDef>.AllDefs.OrderBy((LoadoutGenericDef g) => g.label).ToList();
		_selectableItems = new List<SelectableItem>();
		List<ThingDef> list = (from thing in Find.CurrentMap.listerThings.AllThings
			where !thing.PositionHeld.Fogged(thing.MapHeld) && !thing.GetInnerIfMinified().def.Minifiable
			select thing.def).Distinct().Intersect(_allSuitableDefs).ToList();
		foreach (ThingDef td2 in _allSuitableDefs)
		{
			_selectableItems.Add(new SelectableItem
			{
				thingDef = td2,
				isGreyedOut = (list.Find((ThingDef def) => def == td2) == null)
			});
		}
		SetSource(SourceSelection.Ranged);
		doCloseX = true;
		forcePause = true;
		absorbInputAroundWindow = true;
		closeOnClickedOutside = true;
		Utility_Loadouts.UpdateColonistCapacities();
	}

	private bool IsSuitableThingDef(ThingDef td)
	{
		return (td.thingClass != typeof(Corpse) && !td.IsBlueprint && !td.IsFrame && td != ThingDefOf.ActiveDropPod && td.thingClass != typeof(MinifiedThing) && td.thingClass != typeof(UnfinishedThing) && !td.destroyOnDrop && td.category == ThingCategory.Item) || td.Minifiable;
	}

	public override void DoWindowContents(Rect canvas)
	{
		Text.Font = GameFont.Small;
		float width = canvas.width * (2f / 15f);
		Rect rect = new Rect(0f, 0f, width, 30f);
		Rect rect2 = new Rect(rect.xMax + 6f, 0f, width, 30f);
		Rect rect3 = new Rect(rect2.xMax + 6f, 0f, width, 30f);
		Rect rect4 = new Rect(rect3.xMax + 6f, 0f, width, 30f);
		Rect rect5 = new Rect(rect4.xMax + 6f, 0f, width, 30f);
		Rect rect6 = new Rect(rect5.xMax + 6f, 0f, width, 30f);
		Rect canvas2 = new Rect(0f, 42f, (canvas.width - 6f) / 2f, 24f);
		Rect canvas3 = new Rect(0f, canvas2.yMax + 6f, (canvas.width - 6f) / 2f, canvas.height - 30f - canvas2.height - 48f - 30f);
		Rect rect7 = new Rect(canvas3.xMin, canvas3.yMax + 6f, canvas3.width, 24f);
		Rect rect8 = new Rect(rect7.xMin, rect7.yMax + 6f, rect7.width, 24f);
		Rect canvas4 = new Rect(canvas3.xMax + 6f, 42f, (canvas.width - 6f) / 2f, 24f);
		Rect canvas5 = new Rect(canvas3.xMax + 6f, canvas4.yMax + 6f, (canvas.width - 6f) / 2f, canvas.height - 24f - 30f - 18f);
		LoadoutManager.SortLoadouts();
		List<Loadout> loadouts = LoadoutManager.Loadouts.Where((Loadout l) => !l.defaultLoadout).ToList();
		if (Widgets.ButtonText(rect, "CE_SelectLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (loadouts.Count == 0)
			{
				list.Add(new FloatMenuOption("CE_NoLoadouts".Translate(), null));
			}
			else
			{
				for (int i = 0; i < loadouts.Count; i++)
				{
					int local_i = i;
					list.Add(new FloatMenuOption(loadouts[i].LabelCap, delegate
					{
						CurrentLoadout = loadouts[local_i];
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		if (Widgets.ButtonText(rect2, "CE_NewLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			CurrentLoadout = NewLoadout();
		}
		if (CurrentLoadout != null && Widgets.ButtonText(rect3, "CE_CopyLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			CurrentLoadout = CopyLoadout(CurrentLoadout);
		}
		if (loadouts.Any((Loadout l) => l.canBeDeleted) && Widgets.ButtonText(rect4, "CE_DeleteLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			List<FloatMenuOption> list2 = new List<FloatMenuOption>();
			for (int j = 0; j < loadouts.Count; j++)
			{
				int local_i2 = j;
				if (!loadouts[j].canBeDeleted)
				{
					continue;
				}
				list2.Add(new FloatMenuOption(loadouts[j].LabelCap, delegate
				{
					if (CurrentLoadout == loadouts[local_i2])
					{
						CurrentLoadout = null;
					}
					RemoveLoadout(loadouts[local_i2]);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list2));
		}
		if (Widgets.ButtonText(rect5, "CE_LoadLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			Find.WindowStack.Add(new LoadLoadoutDialog("loadout", delegate(FileInfo fileInfo, FileListDialog dialog)
			{
				Log.Message("Loading loadout from file '" + fileInfo.FullName + "'...");
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoadoutConfig));
				using FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open);
				LoadoutConfig loadoutConfig = (LoadoutConfig)xmlSerializer.Deserialize(stream);
				CurrentLoadout = Loadout.FromConfig(loadoutConfig, out var unloadableDefNames);
				if (unloadableDefNames.Count > 0)
				{
					Messages.Message("CE_MissingLoadoutSlots".Translate(string.Join(", ", unloadableDefNames)), null, MessageTypeDefOf.RejectInput);
				}
				AddLoadoutExpose(CurrentLoadout);
				dialog.Close();
			}));
		}
		if (CurrentLoadout != null && Widgets.ButtonText(rect6, "CE_SaveLoadout".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			Find.WindowStack.Add(new SaveLoadoutDialog("loadout", delegate(FileInfo fileInfo, FileListDialog dialog)
			{
				Log.Message("Saving loadout '" + CurrentLoadout.label + "' to file '" + fileInfo.FullName + "'...");
				XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(LoadoutConfig));
				using TextWriter textWriter = new StreamWriter(fileInfo.FullName);
				xmlSerializer2.Serialize(textWriter, CurrentLoadout.ToConfig());
				dialog.Close();
			}, CurrentLoadout.label));
		}
		if (CurrentLoadout == null)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = Color.grey;
			Widgets.Label(canvas, "CE_NoLoadoutSelected".Translate());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			return;
		}
		DrawNameField(canvas2);
		DrawSourceSelection(canvas4);
		DrawSlotSelection(canvas5);
		DrawSlotList(canvas3);
		if (CurrentLoadout != null)
		{
			Utility_Loadouts.DrawBar(rect7, CurrentLoadout.Weight, Utility_Loadouts.medianWeightCapacity, "CE_Weight".Translate(), CurrentLoadout.GetWeightTip());
			Utility_Loadouts.DrawBar(rect8, CurrentLoadout.Bulk, Utility_Loadouts.medianBulkCapacity, "CE_Bulk".Translate(), CurrentLoadout.GetBulkTip());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			string text = CE_StatDefOf.CarryBulk.ValueToString(CurrentLoadout.Bulk, CE_StatDefOf.CarryBulk.toStringNumberSense);
			string text2 = CE_StatDefOf.CarryBulk.ValueToString(Utility_Loadouts.medianBulkCapacity, CE_StatDefOf.CarryBulk.toStringNumberSense);
			Widgets.Label(rect8, text + "/" + text2);
			Widgets.Label(rect7, CurrentLoadout.Weight.ToString("0.#") + "/" + Utility_Loadouts.medianWeightCapacity.ToStringMass());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	public void DrawSourceSelection(Rect canvas)
	{
		Rect rect = new Rect(canvas.xMin, canvas.yMin + (canvas.height - 24f) / 2f, 24f, 24f);
		GUI.color = ((_sourceType == SourceSelection.Ranged) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconRanged))
		{
			SetSource(SourceSelection.Ranged);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceRangedTip".Translate());
		rect.x += 30f;
		GUI.color = ((_sourceType == SourceSelection.Melee) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconMelee))
		{
			SetSource(SourceSelection.Melee);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceMeleeTip".Translate());
		rect.x += 30f;
		GUI.color = ((_sourceType == SourceSelection.Ammo) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconAmmo))
		{
			SetSource(SourceSelection.Ammo);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceAmmoTip".Translate());
		rect.x += 30f;
		GUI.color = ((_sourceType == SourceSelection.Minified) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconMinified))
		{
			SetSource(SourceSelection.Minified);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceMinifiedTip".Translate());
		rect.x += 30f;
		GUI.color = ((_sourceType == SourceSelection.Generic) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconGeneric))
		{
			SetSource(SourceSelection.Generic);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceGenericTip".Translate());
		rect.x += 30f;
		GUI.color = ((_sourceType == SourceSelection.All) ? GenUI.MouseoverColor : Color.white);
		if (Widgets.ButtonImage(rect, _iconAll))
		{
			SetSource(SourceSelection.All);
		}
		TooltipHandler.TipRegion(rect, "CE_SourceAllTip".Translate());
		Rect rect2 = new Rect(canvas.xMax - 75f, canvas.yMin + (canvas.height - 24f) / 2f, 75f, 24f);
		DrawFilterField(rect2);
		TooltipHandler.TipRegion(rect2, "CE_SourceFilterTip".Translate());
		rect.x = rect2.xMin - 12f - 16f;
		GUI.DrawTexture(rect, _iconSearch);
		TooltipHandler.TipRegion(rect, "CE_SourceFilterTip".Translate());
		GUI.color = Color.white;
	}

	public void FilterSource(string filter)
	{
		SetSource(_sourceType, preserveFilter: true);
		_source = _source.Where((SelectableItem td) => td.thingDef.label.ToUpperInvariant().Contains(_filter.ToUpperInvariant())).ToList();
	}

	public void SetSource(SourceSelection source, bool preserveFilter = false)
	{
		_sourceGeneric = _allDefsGeneric;
		if (!preserveFilter)
		{
			_filter = "";
		}
		switch (source)
		{
		case SourceSelection.Ranged:
			_source = _selectableItems.Where((SelectableItem row) => row.thingDef.IsRangedWeapon).ToList();
			_sourceType = SourceSelection.Ranged;
			break;
		case SourceSelection.Melee:
			_source = _selectableItems.Where((SelectableItem row) => row.thingDef.IsMeleeWeapon).ToList();
			_sourceType = SourceSelection.Melee;
			break;
		case SourceSelection.Ammo:
			_source = _selectableItems.Where((SelectableItem row) => row.thingDef is AmmoDef).ToList();
			_sourceType = SourceSelection.Ammo;
			break;
		case SourceSelection.Minified:
			_source = _selectableItems.Where((SelectableItem row) => row.thingDef.Minifiable).ToList();
			_sourceType = SourceSelection.Minified;
			break;
		case SourceSelection.Generic:
			_sourceType = SourceSelection.Generic;
			initGenericVisibilityDictionary();
			break;
		default:
			_source = _selectableItems;
			_sourceType = SourceSelection.All;
			break;
		}
		if (!_source.NullOrEmpty())
		{
			_source = _source.OrderBy((SelectableItem td) => td.thingDef.label).ToList();
		}
	}

	private void DrawCountField(Rect canvas, LoadoutSlot slot)
	{
		if (slot == null)
		{
			return;
		}
		int val = slot.count;
		string buffer = val.ToString();
		Widgets.TextFieldNumeric(canvas, ref val, ref buffer);
		TooltipHandler.TipRegion(canvas, "CE_CountFieldTip".Translate(slot.count));
		if (slot.count != val)
		{
			if (Multiplayer.InMultiplayer)
			{
				SetSlotCount(CurrentLoadout, CurrentLoadout.Slots.IndexOf(slot), val);
			}
			else
			{
				slot.count = val;
			}
		}
	}

	private void DrawFilterField(Rect canvas)
	{
		string text = GUI.TextField(canvas, _filter);
		if (text != _filter)
		{
			_filter = text;
			FilterSource(_filter);
		}
	}

	private void DrawNameField(Rect canvas)
	{
		string text = GUI.TextField(canvas, CurrentLoadout.label);
		if (validNameRegex.IsMatch(text))
		{
			CurrentLoadout.label = text;
		}
	}

	private void DrawSlot(Rect row, LoadoutSlot slot, bool slotDraggable = true)
	{
		Rect rect = new Rect(row);
		rect.width = row.height;
		Rect rect2 = new Rect(row);
		if (slotDraggable)
		{
			rect2.xMin = rect.xMax;
		}
		rect2.xMax = row.xMax - _countFieldSize.x - 16f - 12f;
		Rect canvas = new Rect(row.xMax - _countFieldSize.x - 16f - 12f, row.yMin + (row.height - _countFieldSize.y) / 2f, _countFieldSize.x, _countFieldSize.y);
		Rect butRect = new Rect(canvas.xMin - 16f - 6f, row.yMin + (row.height - 16f) / 2f, 16f, 16f);
		Rect rect3 = new Rect(butRect.xMin - 16f - 6f, row.yMin + (row.height - 16f) / 2f, 16f, 16f);
		Rect butRect2 = new Rect(rect3.xMin - 16f - 6f, row.yMin + (row.height - 16f) / 2f, 16f, 16f);
		Rect rect4 = new Rect(canvas.xMax + 6f, row.yMin + (row.height - 16f) / 2f, 16f, 16f);
		if (slot.isWeaponPlatform && Widgets.ButtonImage(butRect2, _iconEditAttachments))
		{
			global::CombatExtended.RocketGUI.GUIUtility.DropDownMenu((int i) => i switch
			{
				0 => "CE_AttachmentsEditLoadout".Translate(), 
				1 => "CE_AttachmentsClearLoadout".Translate(), 
				_ => throw new NotImplementedException(), 
			}, delegate(int i)
			{
				if (i == 0)
				{
					if (Find.WindowStack.IsOpen<Window_AttachmentsEditor>())
					{
						Find.WindowStack.TryRemove(typeof(Window_AttachmentsEditor));
					}
					else
					{
						Find.WindowStack.Add(new Window_AttachmentsEditor(slot.weaponPlatformDef, slot.attachmentLinks, delegate(List<AttachmentLink> links)
						{
							if (links != null)
							{
								slot.attachments.Clear();
								slot.attachments.AddRange(links.Select((AttachmentLink l) => l.attachment));
							}
						}));
					}
				}
				if (i == 1)
				{
					slot.attachments.Clear();
				}
			}, _dropOptions2);
		}
		if (slotDraggable)
		{
			TooltipHandler.TipRegion(rect, "CE_DragToReorder".Translate());
			GUI.DrawTexture(rect, _iconMove);
			if (Mouse.IsOver(rect) && Input.GetMouseButtonDown(0))
			{
				Dragging = slot;
			}
		}
		if (!Mouse.IsOver(rect4))
		{
			Widgets.DrawHighlightIfMouseover(row);
			TooltipHandler.TipRegion(row, (slot.genericDef != null) ? slot.genericDef.GetWeightAndBulkTip(slot.count) : slot.thingDef.GetWeightAndBulkTip(slot.count));
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Widgets.Label(rect2, slot.LabelCap);
		Text.WordWrap = true;
		Text.Anchor = TextAnchor.UpperLeft;
		if (slot.thingDef != null && slot.thingDef.IsRangedWeapon)
		{
			AmmoSetDef ammoSetDef = ((slot.thingDef.GetCompProperties<CompProperties_AmmoUser>() == null) ? null : slot.thingDef.GetCompProperties<CompProperties_AmmoUser>().ammoSet);
			bool? flag = !(ammoSetDef?.ammoTypes).NullOrEmpty();
			if (flag == true && Widgets.ButtonImage(butRect, _iconAmmoAdd))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				int magazineSize = ((slot.thingDef.GetCompProperties<CompProperties_AmmoUser>() != null) ? slot.thingDef.GetCompProperties<CompProperties_AmmoUser>().magazineSize : 0);
				foreach (AmmoLink link in ammoSetDef?.ammoTypes)
				{
					list.Add(new FloatMenuOption(link.ammo.LabelCap, delegate
					{
						AddLoadoutSlotSpecific(CurrentLoadout, link.ammo, (magazineSize <= 1) ? link.ammo.defaultAmmoCount : magazineSize);
					}));
				}
				LoadoutGenericDef generic = DefDatabase<LoadoutGenericDef>.GetNamed("GenericAmmo-" + slot.thingDef.defName);
				if (generic != null)
				{
					list.Add(new FloatMenuOption(generic.LabelCap, delegate
					{
						AddLoadoutSlotGeneric(CurrentLoadout, generic);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list, "CE_AddAmmoFor".Translate(slot.thingDef.LabelCap)));
			}
		}
		DrawCountField(canvas, slot);
		if (slot.genericDef != null)
		{
			Texture2D tex = ((slot.countType == LoadoutCountType.dropExcess) ? _iconDropExcess : _iconPickupDrop);
			string text = ((slot.countType == LoadoutCountType.dropExcess) ? "CE_DropExcess".Translate() : "CE_PickupMissingAndDropExcess".Translate());
			if (Widgets.ButtonImage(rect3, tex))
			{
				if (Multiplayer.InMultiplayer)
				{
					ChangeCountType(CurrentLoadout, CurrentLoadout.Slots.IndexOf(slot));
				}
				else
				{
					slot.countType = ((slot.countType != LoadoutCountType.dropExcess) ? LoadoutCountType.dropExcess : LoadoutCountType.pickupDrop);
				}
			}
			TooltipHandler.TipRegion(rect3, text);
		}
		if (Mouse.IsOver(rect4))
		{
			GUI.DrawTexture(row, TexUI.HighlightTex);
		}
		if (Widgets.ButtonImage(rect4, _iconClear))
		{
			RemoveSlot(CurrentLoadout, CurrentLoadout.Slots.IndexOf(slot));
		}
		TooltipHandler.TipRegion(rect4, "CE_DeleteFilter".Translate());
	}

	private void DrawSlotList(Rect canvas)
	{
		Rect rect = new Rect(0f, 0f, canvas.width, 28f * (float)CurrentLoadout.SlotCount + 1f);
		if (Dragging != null)
		{
			rect.height += 28f;
		}
		if (rect.height > canvas.height)
		{
			rect.width -= 16f;
		}
		GUI.DrawTexture(canvas, _darkBackground);
		Widgets.BeginScrollView(canvas, ref _slotScrollPosition, rect);
		int i = 0;
		float num = 0f;
		for (; i < CurrentLoadout.SlotCount; i++)
		{
			Rect rect2 = new Rect(0f, num, rect.width, 28f);
			num += 28f;
			if (Dragging != null && Mouse.IsOver(rect2) && Dragging != CurrentLoadout.Slots[i])
			{
				GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				DrawSlot(rect2, Dragging);
				GUI.color = Color.white;
				if (Input.GetMouseButtonUp(0))
				{
					if (Multiplayer.InMultiplayer)
					{
						MoveSlot(CurrentLoadout, CurrentLoadout.Slots.IndexOf(Dragging), i);
					}
					else
					{
						CurrentLoadout.MoveSlot(Dragging, i);
					}
					Dragging = null;
				}
				rect2.y += 28f;
				num += 28f;
			}
			if (i % 2 == 0)
			{
				GUI.DrawTexture(rect2, _darkBackground);
			}
			if (Dragging == CurrentLoadout.Slots[i] && !Mouse.IsOver(rect2))
			{
				GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.4f);
			}
			DrawSlot(rect2, CurrentLoadout.Slots[i], CurrentLoadout.SlotCount > 1);
			GUI.color = Color.white;
		}
		if (Dragging != null)
		{
			Rect rect3 = new Rect(0f, num, rect.width, 28f);
			if (Mouse.IsOver(rect3))
			{
				GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
				DrawSlot(rect3, Dragging);
				GUI.color = Color.white;
				if (Input.GetMouseButtonUp(0))
				{
					if (Multiplayer.InMultiplayer)
					{
						MoveSlot(CurrentLoadout, CurrentLoadout.Slots.IndexOf(Dragging), CurrentLoadout.Slots.Count - 1);
					}
					else
					{
						CurrentLoadout.MoveSlot(Dragging, CurrentLoadout.Slots.Count - 1);
					}
					Dragging = null;
				}
			}
		}
		if (!Mouse.IsOver(rect) || Input.GetMouseButtonUp(0))
		{
			Dragging = null;
		}
		Widgets.EndScrollView();
	}

	private void DrawSlotSelection(Rect canvas)
	{
		int num = ((_sourceType == SourceSelection.Generic) ? _sourceGeneric.Count : _source.Count);
		GUI.DrawTexture(canvas, _darkBackground);
		if ((_sourceType != SourceSelection.Generic && _source.NullOrEmpty()) || (_sourceType == SourceSelection.Generic && _sourceGeneric.NullOrEmpty()))
		{
			return;
		}
		Rect rect = new Rect(canvas);
		rect.width -= 16f;
		rect.height = (float)num * 28f;
		Widgets.BeginScrollView(canvas, ref _availableScrollPosition, rect.AtZero());
		int num2 = (int)Math.Floor((decimal)(_availableScrollPosition.y / 28f));
		num2 = ((num2 >= 0) ? num2 : 0);
		int num3 = num2 + (int)Math.Ceiling((decimal)(canvas.height / 28f));
		num3 = ((num3 > num) ? num : num3);
		for (int i = num2; i < num3; i++)
		{
			Color color = GUI.color;
			if (_sourceType == SourceSelection.Generic)
			{
				if (GetVisibleGeneric(_sourceGeneric[i]))
				{
					GUI.color = Color.gray;
				}
			}
			else if (_source[i].isGreyedOut)
			{
				GUI.color = Color.gray;
			}
			Rect rect2 = new Rect(0f, (float)i * 28f, canvas.width, 28f);
			Rect rect3 = new Rect(rect2);
			if (_sourceType == SourceSelection.Generic)
			{
				TooltipHandler.TipRegion(rect2, _sourceGeneric[i].GetWeightAndBulkTip());
			}
			else
			{
				TooltipHandler.TipRegion(rect2, _source[i].thingDef.GetWeightAndBulkTip());
			}
			rect3.xMin += 6f;
			if (i % 2 == 0)
			{
				GUI.DrawTexture(rect2, _darkBackground);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			if (_sourceType == SourceSelection.Generic)
			{
				Widgets.Label(rect3, _sourceGeneric[i].LabelCap);
			}
			else
			{
				Widgets.Label(rect3, _source[i].thingDef.LabelCap);
			}
			Text.WordWrap = true;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.DrawHighlightIfMouseover(rect2);
			if (Widgets.ButtonInvisible(rect2))
			{
				if (_sourceType == SourceSelection.Generic)
				{
					AddLoadoutSlotGeneric(CurrentLoadout, _sourceGeneric[i]);
				}
				else
				{
					AddLoadoutSlotSpecific(CurrentLoadout, _source[i].thingDef);
				}
			}
			GUI.color = color;
		}
		Widgets.EndScrollView();
	}

	public override void Close(bool doCloseSound = true)
	{
		base.Close(doCloseSound);
		if (Multiplayer.InMultiplayer && CurrentLoadout != null)
		{
			SyncedSetName(CurrentLoadout, CurrentLoadout.label);
		}
	}

	[Multiplayer.SyncMethod]
	private static void SyncedSetName(Loadout loadout, string name)
	{
		loadout.label = name;
	}

	[Multiplayer.SyncMethod]
	private static void AddLoadoutSlotGeneric(Loadout loadout, LoadoutGenericDef generic)
	{
		loadout.AddSlot(new LoadoutSlot(generic));
	}

	[Multiplayer.SyncMethod]
	private static void AddLoadoutSlotSpecific(Loadout loadout, ThingDef def, int count = 1)
	{
		loadout.AddSlot(new LoadoutSlot(def, count));
	}

	[Multiplayer.SyncMethod]
	private static void RemoveSlot(Loadout loadout, int index)
	{
		if (index >= 0)
		{
			loadout.RemoveSlot(index);
		}
	}

	[Multiplayer.SyncMethod]
	private static void SetSlotCount(Loadout loadout, int index, int count)
	{
		if (index >= 0)
		{
			loadout.Slots[index].count = count;
		}
	}

	[Multiplayer.SyncMethod]
	private static Loadout NewLoadout()
	{
		Loadout loadout = new Loadout();
		loadout.AddBasicSlots();
		LoadoutManager.AddLoadout(loadout);
		if (Multiplayer.IsExecutingCommandsIssuedBySelf)
		{
			Find.WindowStack.WindowOfType<Dialog_ManageLoadouts>().CurrentLoadout = loadout;
		}
		return loadout;
	}

	[Multiplayer.SyncMethod]
	private static Loadout CopyLoadout(Loadout loadout)
	{
		Loadout loadout2 = loadout.Copy();
		LoadoutManager.AddLoadout(loadout2);
		if (Multiplayer.IsExecutingCommandsIssuedBySelf)
		{
			Find.WindowStack.WindowOfType<Dialog_ManageLoadouts>().CurrentLoadout = loadout2;
		}
		return loadout2;
	}

	[Multiplayer.SyncMethod]
	private static void RemoveLoadout(Loadout loadout)
	{
		if (Multiplayer.InMultiplayer)
		{
			Find.WindowStack.WindowOfType<Dialog_ManageLoadouts>().CurrentLoadout = null;
		}
		LoadoutManager.RemoveLoadout(loadout);
	}

	[Multiplayer.SyncMethod(exposeParameters = new int[] { 0 })]
	private static void AddLoadoutExpose(Loadout loadout)
	{
		LoadoutManager.AddLoadout(loadout);
	}

	[Multiplayer.SyncMethod]
	private static void MoveSlot(Loadout loadout, int index, int moveIndex)
	{
		if (index >= 0)
		{
			LoadoutSlot slot = loadout.Slots[index];
			loadout.MoveSlot(slot, moveIndex);
		}
	}

	[Multiplayer.SyncMethod]
	private static void ChangeCountType(Loadout loadout, int index)
	{
		if (index >= 0)
		{
			LoadoutSlot loadoutSlot = loadout.Slots[index];
			loadoutSlot.countType = ((loadoutSlot.countType != LoadoutCountType.dropExcess) ? LoadoutCountType.dropExcess : LoadoutCountType.pickupDrop);
		}
	}

	private bool GetVisibleGeneric(LoadoutGenericDef def)
	{
		if (GenTicks.TicksAbs >= genericVisibility[def].ticksToRecheck)
		{
			genericVisibility[def].ticksToRecheck = GenTicks.TicksAbs + genericVisibility[def].position;
			genericVisibility[def].check = Find.CurrentMap.listerThings.AllThings.Find((Thing x) => def.lambda(x.GetInnerIfMinified().def) && !x.def.Minifiable) == null;
		}
		return genericVisibility[def].check;
	}

	private void initGenericVisibilityDictionary()
	{
		int num = GenTicks.TicksAbs;
		int num2 = 1;
		List<ThingDef> list = (from thing in Find.CurrentMap.listerThings.AllThings
			where !thing.PositionHeld.Fogged(thing.MapHeld) && !thing.GetInnerIfMinified().def.Minifiable
			select thing.def).Distinct().ToList();
		foreach (LoadoutGenericDef loadoutDef in _sourceGeneric)
		{
			if (!genericVisibility.ContainsKey(loadoutDef))
			{
				genericVisibility.Add(loadoutDef, new VisibilityCache());
			}
			genericVisibility[loadoutDef].ticksToRecheck = num;
			genericVisibility[loadoutDef].check = list.Find((ThingDef def) => loadoutDef.lambda(def)) == null;
			genericVisibility[loadoutDef].position = num2;
			num2++;
			num++;
		}
	}
}

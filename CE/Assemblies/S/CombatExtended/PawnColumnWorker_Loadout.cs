using System.Collections.Generic;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class PawnColumnWorker_Loadout : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	private const int ManageOutfitsButtonHeight = 32;

	internal const float _MinWidth = 158f;

	internal const float _OptimalWidth = 188f;

	internal static float IconSize = 16f;

	public static Texture2D EditImage => ContentFinder<Texture2D>.Get("UI/Icons/edit");

	public static Texture2D ClearImage => ContentFinder<Texture2D>.Get("UI/Icons/clear");

	internal static string textGetter(string untranslatedString)
	{
		return "CE_EditX".Translate(untranslatedString.Translate());
	}

	private IEnumerable<Widgets.DropdownMenuElement<Loadout>> Button_GenerateMenu(Pawn pawn)
	{
		foreach (Loadout loadout in LoadoutManager.Loadouts)
		{
			yield return new Widgets.DropdownMenuElement<Loadout>
			{
				option = new FloatMenuOption(loadout.LabelCap, delegate
				{
					SetLoadout(pawn, loadout);
				}),
				payload = loadout
			};
		}
	}

	[Multiplayer.SyncMethod]
	private static void SetLoadout(Pawn pawn, Loadout loadout)
	{
		pawn.SetLoadout(loadout);
	}

	[Multiplayer.SyncMethod]
	private static void HoldTrackerClear(Pawn pawn)
	{
		pawn.HoldTrackerClear();
	}

	[Multiplayer.SyncMethod]
	private static void UpdateLoadoutNow(Pawn pawn)
	{
		Job job = pawn.thinker?.GetMainTreeThinkNode<JobGiver_UpdateLoadout>()?.TryGiveJob(pawn);
		if (job != null)
		{
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
		}
	}

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
		if (Widgets.ButtonText(rect2, "CE_ManageLoadouts".Translate(), drawBackground: true, doMouseoverSound: false, active: true, null))
		{
			Find.WindowStack.Add(new Dialog_ManageLoadouts(null));
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(CE_ConceptDefOf.CE_Loadouts, KnowledgeAmount.Total);
		}
		UIHighlighter.HighlightOpportunity(rect2, "CE_ManageLoadouts");
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.outfits == null)
		{
			return;
		}
		int num = Mathf.FloorToInt(rect.width - 4f - IconSize);
		int num2 = Mathf.FloorToInt(IconSize);
		float x = rect.x;
		float y = rect.y + (rect.height - IconSize) / 2f;
		float widthCached = "CE_UpdateLoadoutNow".Translate().GetWidthCached();
		bool flag = pawn.HoldTrackerAnythingHeld();
		Rect rect2 = new Rect(x, rect.y + 2f, num, rect.height - 4f);
		if (flag)
		{
			rect2.width -= 4f + (float)num2;
		}
		Rect rect3 = (pawn.Spawned ? rect2.LeftPartPixels(rect2.width - widthCached - 16f) : rect2);
		rect3.xMax -= 2f;
		string buttonLabel = pawn.GetLoadout().label.Truncate(rect3.width);
		Widgets.Dropdown(rect3, pawn, (Pawn p) => p.GetLoadout(), Button_GenerateMenu, buttonLabel, null, null, null, null, paintable: true);
		if (pawn.Spawned)
		{
			Rect rect4 = rect2.RightPartPixels(widthCached + 12f);
			if (Widgets.ButtonText(rect4, "CE_UpdateLoadoutNow".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
			{
				UpdateLoadoutNow(pawn);
			}
		}
		x += rect2.width;
		x += 4f;
		Rect rect5 = new Rect(x, y, num2, num2);
		if (flag)
		{
			if (Widgets.ButtonImage(rect5, ClearImage))
			{
				HoldTrackerClear(pawn);
			}
			TooltipHandler.TipRegion(rect5, new TipSignal(delegate
			{
				string text = "CE_ForcedHold".Translate() + ":\n";
				foreach (HoldRecord holdRecord in LoadoutManager.GetHoldRecords(pawn))
				{
					if (holdRecord.pickedUp)
					{
						text = string.Concat(text + "\n   " + holdRecord.thingDef.LabelCap + " x", holdRecord.count.ToString());
					}
				}
				return text;
			}, pawn.GetHashCode() * 613));
			x += (float)num2;
			x += 4f;
		}
		Rect rect6 = new Rect(x, y, num2, num2);
		if (Widgets.ButtonImage(rect6, EditImage))
		{
			Find.WindowStack.Add(new Dialog_ManageLoadouts(pawn.GetLoadout()));
		}
		TooltipHandler.TipRegion(rect6, new TipSignal(textGetter("CE_Loadouts"), pawn.GetHashCode() * 613));
		x += (float)num2;
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(158f));
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(Mathf.CeilToInt(188f), GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 65);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return a.GetLoadoutId().CompareTo(b.GetLoadoutId());
	}
}

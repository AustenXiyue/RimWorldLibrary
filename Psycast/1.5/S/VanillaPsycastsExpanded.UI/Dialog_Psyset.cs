using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaPsycastsExpanded.Technomancer;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace VanillaPsycastsExpanded.UI;

public class Dialog_Psyset : Window
{
	private readonly Dictionary<AbilityDef, Vector2> abilityPos = new Dictionary<AbilityDef, Vector2>();

	private readonly CompAbilities compAbilities;

	private readonly Hediff_PsycastAbilities hediff;

	private readonly PsySet psyset;

	public List<PsycasterPathDef> paths;

	private int curIdx;

	private Pawn pawn;

	public override Vector2 InitialSize => new Vector2(480f, 520f);

	public Dialog_Psyset(PsySet psyset, Pawn pawn)
	{
		this.psyset = psyset;
		this.pawn = pawn;
		hediff = pawn.Psycasts();
		compAbilities = ((ThingWithComps)pawn).GetComp<CompAbilities>();
		doCloseButton = true;
		doCloseX = true;
		forcePause = true;
		closeOnClickedOutside = true;
		paths = hediff.unlockedPaths.ListFullCopy();
		foreach (PsycasterPathDef item in pawn.AllPathsFromPsyrings())
		{
			if (!paths.Contains(item))
			{
				paths.Add(item);
			}
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		inRect.yMax -= 50f;
		Text.Font = GameFont.Medium;
		Widgets.Label(UIUtility.TakeTopPart(ref inRect, 40f).LeftHalf(), psyset.Name);
		Text.Font = GameFont.Small;
		int group = DragAndDropWidget.NewGroup();
		Rect rect2 = inRect.LeftHalf().ContractedBy(3f);
		rect2.xMax -= 8f;
		Widgets.Label(UIUtility.TakeTopPart(ref rect2, 20f), "VPE.Contents".Translate());
		Widgets.DrawMenuSection(rect2);
		DragAndDropWidget.DropArea(group, rect2, delegate(object obj)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			psyset.Abilities.Add((AbilityDef)obj);
		}, null);
		Vector2 position = rect2.position + new Vector2(8f, 8f);
		foreach (AbilityDef def2 in psyset.Abilities.ToList())
		{
			Rect rect3 = new Rect(position, new Vector2(36f, 36f));
			PsycastsUIUtility.DrawAbility(rect3, def2);
			TooltipHandler.TipRegion(rect3, () => string.Format("{0}\n\n{1}\n\n{2}", ((Def)(object)def2).LabelCap, ((Def)(object)def2).description, "VPE.ClickRemove".Translate().Resolve().ToUpper()), ((object)def2).GetHashCode() + 2);
			if (Widgets.ButtonInvisible(rect3))
			{
				psyset.Abilities.Remove(def2);
			}
			position.x += 44f;
			if (position.x + 36f >= rect2.xMax)
			{
				position.x = rect2.xMin + 8f;
				position.y += 44f;
			}
		}
		Rect rect4 = inRect.RightHalf().ContractedBy(3f);
		Rect rect5 = UIUtility.TakeTopPart(ref rect4, 50f);
		Rect rect6 = UIUtility.TakeLeftPart(ref rect5, 40f).ContractedBy(0f, 5f);
		Rect rect7 = UIUtility.TakeRightPart(ref rect5, 40f).ContractedBy(0f, 5f);
		if (curIdx > 0 && Widgets.ButtonText(rect6, "<", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			curIdx--;
		}
		if (curIdx < paths.Count - 1 && Widgets.ButtonText(rect7, ">", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			curIdx++;
		}
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect5, $"{((paths.Count > 0) ? (curIdx + 1) : 0)} / {paths.Count}");
		Text.Anchor = TextAnchor.UpperLeft;
		if (paths.Count > 0)
		{
			PsycasterPathDef psycasterPathDef = paths[curIdx];
			PsycastsUIUtility.DrawPathBackground(ref rect4, psycasterPathDef);
			PsycastsUIUtility.DoPathAbilities(rect4, psycasterPathDef, abilityPos, delegate(Rect rect, AbilityDef def)
			{
				PsycastsUIUtility.DrawAbility(rect, def);
				if (compAbilities.HasAbility(def))
				{
					DragAndDropWidget.Draggable(group, rect, def);
					TooltipHandler.TipRegion(rect, () => $"{((Def)(object)def).LabelCap}\n\n{((Def)(object)def).description}", ((object)def).GetHashCode() + 1);
				}
				else
				{
					Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.6f));
				}
			});
		}
		object obj2 = DragAndDropWidget.CurrentlyDraggedDraggable();
		AbilityDef val = (AbilityDef)((obj2 is AbilityDef) ? obj2 : null);
		if (val != null)
		{
			PsycastsUIUtility.DrawAbility(new Rect(Event.current.mousePosition, new Vector2(36f, 36f)), val);
		}
		Rect? rect8 = DragAndDropWidget.HoveringDropAreaRect(group, null);
		if (rect8.HasValue)
		{
			Rect valueOrDefault = rect8.GetValueOrDefault();
			if (true)
			{
				Widgets.DrawHighlight(valueOrDefault);
			}
		}
	}
}

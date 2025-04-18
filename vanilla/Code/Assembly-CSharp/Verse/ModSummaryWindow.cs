using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Steamworks;
using UnityEngine;

namespace Verse;

public class ModSummaryWindow
{
	private static Vector2 modListScrollPos;

	private static float modListLastHeight;

	private static readonly Vector2 WindowSize = new Vector2(776f, 410f);

	private static readonly Vector2 ListElementSize = new Vector2(238f, 36f);

	private const float WindowHeightCollapsed = 226f;

	private const float ExpansionListHeight = 94f;

	private const float ModListHeight = 224f;

	private const float ModListHeightCollapsed = 40f;

	private const float ListElementIconSize = 32f;

	private static readonly Color DisabledIconTint = new Color(0.35f, 0.35f, 0.35f);

	private static readonly Color ModInfoListBackground = new Color(0.13f, 0.13f, 0.13f);

	private static readonly Color ModInfoListItemBackground = new Color(0.32f, 0.32f, 0.32f);

	private static readonly Color ModInfoListItemBackgroundIncompatible = new Color(0.31f, 0.29f, 0.15f);

	private static readonly Color ModInfoListItemBackgroundDisabled = new Color(0.1f, 0.1f, 0.1f);

	private static bool AnyMods => ModLister.AllInstalledMods.Any((ModMetaData m) => !m.Official && m.Active);

	public static void DrawWindow(Vector2 offset, bool useWindowStack)
	{
		Rect rect = new Rect(offset.x, offset.y, WindowSize.x, GetEffectiveSize().y);
		if (useWindowStack)
		{
			Find.WindowStack.ImmediateWindow(62893996, rect, WindowLayer.Super, delegate
			{
				DrawContents(rect.AtZero());
			});
		}
		else
		{
			Widgets.DrawShadowAround(rect);
			Widgets.DrawWindowBackground(rect);
			DrawContents(rect);
		}
	}

	private static void DrawContents(Rect rect)
	{
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		float num = 0f;
		float num2 = 17f;
		float itemListInnerMargin = 8f;
		float num3 = num2 + 4f;
		Rect rect2 = new Rect(rect.x + num2, rect.y, rect.width - num2 * 2f, 0f);
		Rect rect3 = rect;
		rect3.x += num3;
		rect3.y += 10f;
		Widgets.Label(rect3, "OfficialContent".Translate());
		num += 10f + Text.LineHeight + 4f;
		Rect rect4 = rect2;
		rect4.y += num;
		rect4.height = 94f;
		Widgets.DrawBoxSolid(rect4, ModInfoListBackground);
		num += 104f;
		List<GenUI.AnonymousStackElement> list = new List<GenUI.AnonymousStackElement>();
		Text.Anchor = TextAnchor.MiddleLeft;
		for (int i = 0; i < ModLister.AllExpansions.Count; i++)
		{
			ExpansionDef exp = ModLister.AllExpansions[i];
			list.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					bool flag = exp.Status == ExpansionStatus.Active;
					Widgets.DrawBoxSolid(r, flag ? ModInfoListItemBackground : ModInfoListItemBackgroundDisabled);
					Widgets.DrawHighlightIfMouseover(r);
					if (!exp.isCore && !exp.StoreURL.NullOrEmpty() && Widgets.ButtonInvisible(r))
					{
						SteamUtility.OpenUrl(exp.StoreURL);
					}
					GUI.color = (flag ? Color.white : DisabledIconTint);
					Material material = (flag ? null : TexUI.GrayscaleGUI);
					Rect rect5 = new Rect(r.x + itemListInnerMargin, r.y + 2f, 32f, 32f);
					float num4 = 42f;
					GenUI.DrawTextureWithMaterial(rect5, exp.IconFromStatus, material);
					GUI.color = (flag ? Color.white : Color.grey);
					Rect rect6 = new Rect(r.x + itemListInnerMargin + num4, r.y, r.width - num4, r.height);
					if (exp.Status != 0)
					{
						TaggedString taggedString = ((exp.Status == ExpansionStatus.Installed) ? "DisabledLower" : "ContentNotInstalled").Translate().ToLower();
						Widgets.Label(rect6, exp.label + " (" + taggedString + ")");
					}
					else
					{
						Widgets.Label(rect6, exp.label);
					}
					GUI.color = Color.white;
					if (Mouse.IsOver(r))
					{
						string description = exp.label + "\n" + exp.StatusDescription + "\n\n" + exp.description.StripTags();
						TooltipHandler.TipRegion(tip: new TipSignal(() => description, exp.GetHashCode() * 37), rect: r);
					}
				}
			});
		}
		GenUI.DrawElementStackVertical(new Rect(rect4.x + itemListInnerMargin, rect4.y + itemListInnerMargin, rect4.width - itemListInnerMargin * 2f, 94f), ListElementSize.y, list, delegate(Rect r, GenUI.AnonymousStackElement obj)
		{
			obj.drawer(r);
		}, (GenUI.AnonymousStackElement obj) => ListElementSize.x, 6f);
		list.Clear();
		Rect rect7 = rect;
		rect7.x += num3;
		rect7.y += num;
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.Label(rect7, "Mods".Translate());
		num += Text.LineHeight + 4f;
		Rect rect8 = rect2;
		rect8.y += num;
		rect8.height = (AnyMods ? 224f : 40f);
		Widgets.DrawBoxSolid(rect8, ModInfoListBackground);
		if (AnyMods)
		{
			Text.Anchor = TextAnchor.MiddleLeft;
			foreach (ModMetaData mod in ModLister.AllInstalledMods.Where((ModMetaData m) => !m.Official && m.Active))
			{
				list.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Widgets.DrawBoxSolid(r, mod.VersionCompatible ? ModInfoListItemBackground : ModInfoListItemBackgroundIncompatible);
						Widgets.DrawHighlightIfMouseover(r);
						if (mod.OnSteamWorkshop && mod.GetPublishedFileId() != PublishedFileId_t.Invalid && Widgets.ButtonInvisible(r))
						{
							SteamUtility.OpenWorkshopPage(mod.GetPublishedFileId());
						}
						Rect position = new Rect(r.x + itemListInnerMargin, r.y + 2f, 32f, 32f);
						float num5 = 42f;
						if (!mod.Icon.NullOrBad())
						{
							GUI.DrawTexture(position, mod.Icon);
						}
						Rect rect9 = new Rect(r.x + itemListInnerMargin + num5, r.y, r.width - num5, r.height);
						string label = mod.Name.Truncate(rect9.width - itemListInnerMargin - 4f);
						Widgets.Label(rect9, label);
						if (Mouse.IsOver(r))
						{
							string description2 = mod.Name + "\n\n" + mod.Description.StripTags();
							if (!mod.VersionCompatible)
							{
								description2 = description2 + "\n\n" + "ModNotMadeForThisVersionShort".Translate().Colorize(Color.yellow);
							}
							TooltipHandler.TipRegion(tip: new TipSignal(() => description2, mod.GetHashCode() * 37), rect: r);
						}
						GUI.color = Color.white;
					}
				});
			}
			Widgets.BeginScrollView(rect8, ref modListScrollPos, new Rect(0f, 0f, rect8.width - 16f, modListLastHeight + itemListInnerMargin * 2f));
			modListLastHeight = GenUI.DrawElementStack(new Rect(itemListInnerMargin, itemListInnerMargin, rect8.width - itemListInnerMargin * 2f, 99999f), ListElementSize.y, list, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => ListElementSize.x, 6f).height;
			Widgets.EndScrollView();
		}
		else
		{
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect10 = rect8;
			rect10.x += itemListInnerMargin;
			rect10.y += itemListInnerMargin;
			GUI.color = Color.gray;
			Widgets.Label(rect10, "None".Translate());
			GUI.color = Color.white;
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static Vector2 GetEffectiveSize()
	{
		return new Vector2(WindowSize.x, AnyMods ? WindowSize.y : 226f);
	}
}

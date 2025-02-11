using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.RocketGUI;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Fragment_AttachmentEditor
{
	public readonly WeaponPlatform weapon;

	public readonly WeaponPlatformDef weaponDef;

	private static List<StatDef> displayStats;

	private static List<StatDef> positiveStats;

	private static List<StatDef> negativeStats;

	private AttachmentLink hoveringOver = null;

	private List<ThingDefCountClass> cost = new List<ThingDefCountClass>();

	private List<string> tags = new List<string>();

	private Dictionary<StatDef, float> statBases = new Dictionary<StatDef, float>();

	private Dictionary<StatDef, float> stats = new Dictionary<StatDef, float>();

	private Dictionary<AttachmentLink, bool> attachedByAt = new Dictionary<AttachmentLink, bool>();

	private Dictionary<AttachmentLink, bool> additionByAt = new Dictionary<AttachmentLink, bool>();

	private Dictionary<AttachmentLink, bool> removalByAt = new Dictionary<AttachmentLink, bool>();

	private List<WeaponPlatformDef.WeaponGraphicPart> visibleDefaultParts = new List<WeaponPlatformDef.WeaponGraphicPart>();

	private List<AttachmentLink> links = new List<AttachmentLink>();

	private List<AttachmentLink> target = new List<AttachmentLink>();

	private Dictionary<string, List<AttachmentLink>> linksByTag = new Dictionary<string, List<AttachmentLink>>();

	private readonly Listing_Collapsible collapsible_radios = new Listing_Collapsible(expanded: true);

	private readonly Listing_Collapsible collapsible_stats = new Listing_Collapsible(expanded: true);

	private readonly Listing_Collapsible collapsible_center = new Listing_Collapsible(expanded: true);

	public List<AttachmentLink> CurConfig => links.Where((AttachmentLink l) => additionByAt[l] || (attachedByAt[l] && !removalByAt[l])).ToList();

	public List<AttachmentLink> CurAdditions => links.Where((AttachmentLink t) => !attachedByAt[t] && additionByAt[t]).ToList();

	public List<AttachmentLink> CurDeletions => links.Where((AttachmentLink t) => removalByAt[t] && attachedByAt[t]).ToList();

	public Fragment_AttachmentEditor(WeaponPlatformDef weaponDef, List<AttachmentLink> config)
	{
		InitializeFragment();
		links = weaponDef.attachmentLinks;
		weapon = null;
		this.weaponDef = weaponDef;
		foreach (AttachmentLink attachmentLink in weaponDef.attachmentLinks)
		{
			string text = attachmentLink.attachment.slotTags.First();
			if (!linksByTag.TryGetValue(text, out var _))
			{
				tags.Add(text);
				linksByTag[text] = new List<AttachmentLink>();
			}
			linksByTag[text].Add(attachmentLink);
			attachedByAt[attachmentLink] = false;
			additionByAt[attachmentLink] = false;
			removalByAt[attachmentLink] = false;
		}
		tags.SortBy((string x) => x);
		foreach (AttachmentLink item in config)
		{
			attachedByAt[item] = true;
			AddAttachment(item, update: false);
		}
		foreach (StatDef displayStat in displayStats)
		{
			statBases[displayStat] = weaponDef.GetWeaponStatAbstractWith(displayStat, config);
		}
		Update();
	}

	public Fragment_AttachmentEditor(WeaponPlatform weapon)
	{
		InitializeFragment();
		links = weapon.Platform.attachmentLinks;
		this.weapon = weapon;
		weaponDef = weapon.Platform;
		foreach (AttachmentLink attachmentLink in weaponDef.attachmentLinks)
		{
			string text = attachmentLink.attachment.slotTags.First();
			if (!linksByTag.TryGetValue(text, out var _))
			{
				tags.Add(text);
				linksByTag[text] = new List<AttachmentLink>();
			}
			linksByTag[text].Add(attachmentLink);
			attachedByAt[attachmentLink] = false;
			additionByAt[attachmentLink] = false;
			removalByAt[attachmentLink] = false;
		}
		tags.SortBy((string x) => x);
		foreach (AttachmentLink link in weapon.Platform.attachmentLinks.Where((AttachmentLink l) => weapon.TargetConfig.Any((AttachmentDef a) => a == l.attachment)))
		{
			if (weapon.attachments.Any((AttachmentLink l) => l.attachment.index == link.attachment.index))
			{
				attachedByAt[link] = true;
			}
			AddAttachment(link, update: false);
		}
		List<AttachmentDef> targetConfig = weapon.TargetConfig;
		foreach (AttachmentLink link2 in weapon.Platform.attachmentLinks)
		{
			if (weapon.attachments.Any((AttachmentLink l) => l.attachment == link2.attachment))
			{
				attachedByAt[link2] = true;
				if (!targetConfig.Contains(link2.attachment))
				{
					removalByAt[link2] = true;
				}
			}
			else if (targetConfig.Contains(link2.attachment))
			{
				additionByAt[link2] = true;
			}
		}
		foreach (StatDef displayStat in displayStats)
		{
			statBases[displayStat] = weapon.GetWeaponStatWith(displayStat, null);
		}
		Update();
	}

	private void InitializeFragment()
	{
		collapsible_center.CollapsibleBGBorderColor = Color.gray;
		collapsible_center.Margins = new Vector2(3f, 0f);
		collapsible_stats.CollapsibleBGBorderColor = Color.gray;
		collapsible_stats.Margins = new Vector2(3f, 0f);
		collapsible_radios.CollapsibleBGBorderColor = Color.gray;
		collapsible_radios.Margins = new Vector2(3f, 0f);
		if (displayStats == null)
		{
			displayStats = new List<StatDef>
			{
				StatDefOf.MarketValue,
				StatDefOf.Mass,
				CE_StatDefOf.Bulk,
				StatDefOf.RangedWeapon_Cooldown,
				CE_StatDefOf.SightsEfficiency,
				CE_StatDefOf.Recoil,
				CE_StatDefOf.NightVisionEfficiency_Weapon,
				CE_StatDefOf.TicksBetweenBurstShots,
				CE_StatDefOf.BurstShotCount,
				CE_StatDefOf.ReloadSpeed,
				CE_StatDefOf.ReloadTime,
				CE_StatDefOf.MuzzleFlash,
				CE_StatDefOf.MagazineCapacity,
				CE_StatDefOf.ShotSpread,
				CE_StatDefOf.SwayFactor
			};
			positiveStats = new List<StatDef>
			{
				StatDefOf.MarketValue,
				CE_StatDefOf.BurstShotCount,
				CE_StatDefOf.SightsEfficiency,
				CE_StatDefOf.NightVisionEfficiency_Weapon,
				CE_StatDefOf.MagazineCapacity,
				CE_StatDefOf.ReloadSpeed
			};
			negativeStats = new List<StatDef>
			{
				StatDefOf.Mass,
				StatDefOf.RangedWeapon_Cooldown,
				CE_StatDefOf.TicksBetweenBurstShots,
				CE_StatDefOf.ReloadTime,
				CE_StatDefOf.Recoil,
				CE_StatDefOf.Bulk,
				CE_StatDefOf.MuzzleFlash,
				CE_StatDefOf.ShotSpread,
				CE_StatDefOf.SwayFactor
			};
		}
	}

	public void DoContents(Rect inRect)
	{
		float width = inRect.width;
		hoveringOver = null;
		DoRightPanel(inRect.RightPartPixels(width * 0.3f).ContractedBy(2f));
		inRect.xMax -= width * 0.3f;
		DoCenterPanel(inRect.RightPartPixels(width * 0.4f).ContractedBy(2f));
		inRect.xMax -= width * 0.4f;
		DoLeftPanel(inRect.ContractedBy(2f));
	}

	private void DoRightPanel(Rect inRect)
	{
		inRect.xMin += 5f;
		collapsible_radios.Expanded = true;
		collapsible_radios.Begin(inRect);
		collapsible_radios.Label("CE_Attachments_Options".Translate(), Color.white, null, invert: false, hightlightIfMouseOver: false, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
		collapsible_radios.Gap(2f);
		collapsible_radios.Label("CE_Attachments_Options_Tip".Translate());
		collapsible_radios.Gap(1f);
		bool flag = false;
		bool stop = false;
		foreach (string tag in tags)
		{
			if (flag)
			{
				collapsible_radios.Gap(2f);
			}
			collapsible_radios.Gap(3f);
			collapsible_radios.Label(("CE_AttachmentSlot_" + tag).Translate(), Color.gray, null, invert: false, hightlightIfMouseOver: false, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
			collapsible_radios.Line(1f);
			collapsible_radios.Gap(2f);
			foreach (AttachmentLink link in linksByTag[tag])
			{
				AttachmentDef attachment = link.attachment;
				collapsible_radios.Gap(2f);
				collapsible_radios.Lambda(20f, delegate(Rect rect)
				{
					bool flag2 = (attachedByAt[link] || additionByAt[link]) && !removalByAt[link];
					bool checkOn = flag2;
					Widgets.DefIcon(rect.LeftPartPixels(20f).ContractedBy(2f), attachment, null, 1f, null, drawPlaceholder: false, null, null, null);
					rect.xMin += 25f;
					Color radioColor = ((attachedByAt[link] && !removalByAt[link]) ? Color.white : (additionByAt[link] ? Color.green : (removalByAt[link] ? Color.red : Color.white)));
					global::CombatExtended.RocketGUI.GUIUtility.CheckBoxLabeled(rect, attachment.label.CapitalizeFirst(), radioColor, ref checkOn, disabled: false, monotone: false, 20f, GameFont.Small, FontStyle.Normal, placeCheckboxNearText: false, drawHighlightIfMouseover: false, Widgets.RadioButOnTex, Widgets.RadioButOffTex);
					if (checkOn != flag2)
					{
						if (checkOn)
						{
							AddAttachment(link);
						}
						else
						{
							RemoveAttachment(link);
						}
						stop = true;
					}
					if (Mouse.IsOver(rect.ExpandedBy(2f)))
					{
						hoveringOver = link;
						TooltipHandler.TipRegion(rect, attachment.description.CapitalizeFirst());
					}
				}, invert: false, useMargins: true, hightlightIfMouseOver: true);
				collapsible_radios.Gap(2f);
				if (stop)
				{
					break;
				}
			}
			flag = true;
			if (stop)
			{
				break;
			}
		}
		collapsible_radios.End(ref inRect);
	}

	private void DoCenterPanel(Rect inRect)
	{
		DoLoadoutPanel(inRect.SliceYPixels(40f));
		inRect.yMin += 5f;
		global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Widgets.DrawMenuSection(inRect);
			if (cost.Count != 0)
			{
				Rect inRect2 = inRect.BottomPartPixels((float)(cost.Count + 2) * 22f);
				inRect2.xMin += 5f;
				inRect2.xMax -= 5f;
				collapsible_center.Expanded = true;
				collapsible_center.Begin(inRect2);
				collapsible_center.Label("CE_EditAttachmentsCost".Translate(), Color.gray, null, invert: false, hightlightIfMouseOver: false, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
				collapsible_center.Line(1f);
				collapsible_center.Gap(2f);
				foreach (ThingDefCountClass countClass in cost)
				{
					collapsible_center.Lambda(20f, delegate(Rect rect)
					{
						Text.Font = GameFont.Small;
						Text.Anchor = TextAnchor.UpperLeft;
						Widgets.DefIcon(rect.LeftPartPixels(20f).ContractedBy(2f), countClass.thingDef, null, 1f, null, drawPlaceholder: false, null, null, null);
						rect.xMin += 25f;
						Widgets.Label(rect, countClass.thingDef.label);
						Text.Anchor = TextAnchor.UpperRight;
						Widgets.Label(rect, $"x{countClass.count}");
					}, invert: false, useMargins: true);
					collapsible_center.Gap(2f);
				}
				collapsible_center.End(ref inRect2);
				inRect.yMax -= ((float)cost.Count + 1.5f) * 20f / 2f;
			}
			Rect rect2 = inRect;
			rect2.width = Mathf.Min(rect2.width, rect2.height);
			rect2.height = rect2.width;
			rect2 = rect2.CenteredOnXIn(inRect);
			global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(rect2.ContractedBy(10f), weaponDef, target.ToHashSet(), visibleDefaultParts, hoveringOver, null);
			inRect.yMin += rect2.height;
			if (Prefs.DevMode && DebugSettings.godMode)
			{
				global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
				{
					GUI.color = Color.cyan;
					if (Widgets.ButtonText(rect2.TopPartPixels(20f).LeftPartPixels(75f), "edit offsets", drawBackground: true, doMouseoverSound: true, active: true, null))
					{
						Find.WindowStack.Add(new Window_AttachmentsDebugger(weaponDef));
					}
				});
			}
		});
	}

	private void DoLeftPanel(Rect inRect)
	{
		inRect.xMax -= 5f;
		collapsible_stats.Expanded = true;
		collapsible_stats.Begin(inRect);
		collapsible_stats.Label("CE_Attachments_Information".Translate(), Color.white, null, invert: false, hightlightIfMouseOver: false, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
		collapsible_stats.Gap(2f);
		collapsible_stats.Label("CE_Attachments_Information_Tip".Translate(), null, invert: false, hightlightIfMouseOver: false);
		collapsible_stats.Gap(4f);
		collapsible_stats.Label("CE_EditAttachmentsStats".Translate(), Color.gray, null, invert: false, hightlightIfMouseOver: false, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
		collapsible_stats.Line(1f);
		foreach (StatDef stat in displayStats)
		{
			global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				collapsible_stats.Gap(2f);
				collapsible_stats.Lambda(stat.label.GetTextHeight(inRect.width * 0.65f - 10f), delegate(Rect rect)
				{
					rect.xMin += 5f;
					TooltipHandler.TipRegion(rect, stat.description);
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.LabelFit(rect.LeftPart(0.85f), stat.label.CapitalizeFirst());
					GUI.color = GetStatColor(stat);
					Widgets.Label(rect.RightPart(0.15f), " " + ((float)Math.Round(statBases[stat] + stats[stat], 2)).ToStringByStyle(stat.toStringStyle));
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect.LeftPart(0.85f), ((float)Math.Round(statBases[stat], 2)).ToStringByStyle(stat.toStringStyle) + " |");
				}, invert: false, useMargins: true, hightlightIfMouseOver: true);
			});
		}
		collapsible_stats.End(ref inRect);
	}

	private void DoLoadoutPanel(Rect inRect)
	{
		Widgets.DrawMenuSection(inRect);
	}

	private void AddAttachment(AttachmentLink attachmentLink, bool update = true)
	{
		foreach (AttachmentLink link in links)
		{
			if ((attachedByAt[link] || additionByAt[link]) && !removalByAt[link] && !attachmentLink.CompatibleWith(link))
			{
				RemoveAttachment(link, update: false);
			}
		}
		removalByAt[attachmentLink] = false;
		additionByAt[attachmentLink] = true;
		if (update)
		{
			Update();
		}
	}

	private void RemoveAttachment(AttachmentLink attachmentLink, bool update = true)
	{
		additionByAt[attachmentLink] = false;
		if (attachedByAt[attachmentLink])
		{
			removalByAt[attachmentLink] = true;
		}
		if (update)
		{
			Update();
		}
	}

	private void Update()
	{
		target.Clear();
		target.AddRange(links.Where((AttachmentLink l) => (attachedByAt[l] && !removalByAt[l]) || additionByAt[l]));
		visibleDefaultParts.Clear();
		visibleDefaultParts.AddRange(weaponDef.defaultGraphicParts);
		visibleDefaultParts.RemoveAll((WeaponPlatformDef.WeaponGraphicPart p) => target.Any((AttachmentLink l) => weaponDef.AttachmentRemoves(l.attachment, p)));
		stats.Clear();
		foreach (StatDef displayStat in displayStats)
		{
			float num = ((weapon != null) ? weapon.GetWeaponStatWith(displayStat, target) : weaponDef.GetWeaponStatAbstractWith(displayStat, target));
			stats[displayStat] = num - statBases[displayStat];
		}
		cost.Clear();
		foreach (AttachmentLink curAddition in CurAdditions)
		{
			foreach (ThingDefCountClass countClass in curAddition.attachment.costList)
			{
				ThingDefCountClass thingDefCountClass = cost.FirstOrFallback((ThingDefCountClass c) => c.thingDef == countClass.thingDef);
				if (thingDefCountClass == null)
				{
					thingDefCountClass = new ThingDefCountClass(countClass.thingDef, 0);
					cost.Add(thingDefCountClass);
				}
				thingDefCountClass.count += countClass.count;
			}
		}
	}

	private Color GetStatColor(StatDef stat)
	{
		float num = stats[stat];
		if (positiveStats.Contains(stat))
		{
			return (num > 0f) ? Color.green : ((num == 0f) ? Color.white : Color.red);
		}
		if (negativeStats.Contains(stat))
		{
			return (num > 0f) ? Color.red : ((num == 0f) ? Color.white : Color.green);
		}
		throw new NotImplementedException();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.RocketGUI;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Window_AttachmentsDebugger : Window
{
	private const int PANEL_RIGHT_WIDTH = 350;

	public readonly WeaponPlatformDef weaponDef;

	private List<AttachmentLink> links = null;

	private List<WeaponPlatformDef.WeaponGraphicPart> parts = new List<WeaponPlatformDef.WeaponGraphicPart>();

	private Dictionary<AttachmentLink, bool> hidden = new Dictionary<AttachmentLink, bool>();

	private Dictionary<AttachmentLink, bool> fake = new Dictionary<AttachmentLink, bool>();

	private readonly Listing_Collapsible collapsible = new Listing_Collapsible(expanded: true, scrollViewOnOverflow: true, drawBorder: true, drawBackground: true);

	private string searchText = "";

	private int _counter = 0;

	public override Vector2 InitialSize => new Vector2(800f, 600f);

	public Window_AttachmentsDebugger(WeaponPlatformDef weaponDef)
	{
		links = weaponDef.attachmentLinks.ToList();
		layer = WindowLayer.Super;
		base.resizer = new WindowResizer();
		forcePause = true;
		doCloseButton = false;
		doCloseX = false;
		this.weaponDef = weaponDef;
		foreach (AttachmentLink link in links)
		{
			hidden.Add(link, value: true);
			fake.Add(link, value: false);
		}
		foreach (AttachmentDef def in DefDatabase<AttachmentDef>.AllDefs)
		{
			if (!links.Any((AttachmentLink l) => l.attachment == def))
			{
				AttachmentLink attachmentLink = new AttachmentLink();
				attachmentLink.attachment = def;
				attachmentLink.PrepareTexture(weaponDef);
				links.Add(attachmentLink);
				fake.Add(attachmentLink, value: true);
				hidden.Add(attachmentLink, value: true);
			}
		}
		UpdateRenderingCache();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Exception ex = null;
		try
		{
			global::CombatExtended.RocketGUI.GUIUtility.StashGUIState();
			DoContent(inRect);
		}
		catch (Exception ex2)
		{
			ex = ex2;
		}
		finally
		{
			global::CombatExtended.RocketGUI.GUIUtility.RestoreGUIState();
			if (ex != null)
			{
				throw ex;
			}
		}
	}

	public override void Close(bool doCloseSound = true)
	{
		base.Close(doCloseSound);
		weaponDef.attachmentLinks = links.Where((AttachmentLink l) => weaponDef.attachmentLinks.Contains(l)).ToList();
		foreach (AttachmentLink attachmentLink in weaponDef.attachmentLinks)
		{
			attachmentLink.PrepareTexture(weaponDef);
		}
	}

	public override void WindowOnGUI()
	{
		base.WindowOnGUI();
		if (_counter++ % 60 != 0)
		{
			return;
		}
		weaponDef.attachmentLinks = links.Where((AttachmentLink l) => weaponDef.attachmentLinks.Contains(l)).ToList();
		foreach (AttachmentLink attachmentLink in weaponDef.attachmentLinks)
		{
			attachmentLink.PrepareTexture(weaponDef);
		}
		UpdateRenderingCache();
	}

	private void DoContent(Rect inRect)
	{
		Rect inRect2 = inRect.RightPartPixels(350f).ContractedBy(2f);
		DoRightPanel(inRect2);
		Rect rect = inRect.LeftPartPixels(inRect.width - 350f).ContractedBy(2f);
		Widgets.DrawMenuSection(rect);
		DoLeftPanel(rect);
	}

	private void DoRightPanel(Rect inRect)
	{
		searchText = Widgets.TextField(inRect.TopPartPixels(20f), searchText) ?? "";
		string text = searchText.Trim().ToLower();
		inRect.yMin += 25f;
		collapsible.Begin(inRect, "Offset simulator", drawInfo: false, drawIcon: false);
		bool flag = false;
		foreach (AttachmentLink link in links)
		{
			if (!text.NullOrEmpty() && !link.attachment.label.ToLower().Contains(text))
			{
				continue;
			}
			if (!flag && fake[link])
			{
				flag = true;
				collapsible.Label("<color=red>Warning</color>", null, invert: false, hightlightIfMouseOver: true, GameFont.Small, FontStyle.Bold);
				collapsible.Label("Attachments below are not from this weapon", null, invert: false, hightlightIfMouseOver: true, GameFont.Small);
				collapsible.Gap(10f);
			}
			bool checkOn = !hidden[link];
			if (collapsible.CheckboxLabeled(link.attachment.label ?? "", ref checkOn, null, invert: false, disabled: false, hightlightIfMouseOver: true, GameFont.Small))
			{
				UpdateRenderingCache();
			}
			hidden[link] = !checkOn;
			if (checkOn)
			{
				collapsible.Gap(2f);
				collapsible.Label("current drawOffset value:");
				collapsible.Lambda(18f, delegate(Rect rect)
				{
					Text.Font = GameFont.Tiny;
					GUI.color = Color.green;
					Widgets.TextField(rect, $"({Math.Round(link.drawOffset.x, 3)},{Math.Round(link.drawOffset.y, 3)})");
				}, invert: false, useMargins: true);
				collapsible.Gap(2f);
				collapsible.Lambda(20f, delegate
				{
					Text.Font = GameFont.Tiny;
					link.drawOffset.x = 1f;
				}, invert: false, useMargins: true);
				collapsible.Gap(2f);
				collapsible.Lambda(20f, delegate
				{
					Text.Font = GameFont.Tiny;
					link.drawOffset.y = 1f;
				}, invert: false, useMargins: true);
				collapsible.Gap(2f);
				collapsible.Label("current drawScale value:");
				collapsible.Lambda(18f, delegate(Rect rect)
				{
					Text.Font = GameFont.Tiny;
					GUI.color = Color.green;
					Widgets.TextField(rect, $"({Math.Round(link.drawScale.x, 3)}, {Math.Round(link.drawScale.y, 3)})");
				}, invert: false, useMargins: true);
				collapsible.Gap(2f);
				collapsible.Lambda(20f, delegate
				{
					Text.Font = GameFont.Tiny;
					link.drawScale.x = 1f;
				}, invert: false, useMargins: true);
				collapsible.Gap(2f);
				collapsible.Lambda(20f, delegate
				{
					Text.Font = GameFont.Tiny;
					link.drawScale.y = 1f;
				}, invert: false, useMargins: true);
			}
			collapsible.Line(1f);
		}
		collapsible.End(ref inRect);
	}

	private void UpdateRenderingCache()
	{
		parts.Clear();
		HashSet<AttachmentLink> source = links.Where((AttachmentLink l) => !hidden[l]).ToHashSet();
		foreach (WeaponPlatformDef.WeaponGraphicPart part in weaponDef.defaultGraphicParts)
		{
			if (!source.Any((AttachmentLink l) => l.attachment.slotTags?.Any((string s) => part.slotTags?.Contains(s) ?? false) ?? false))
			{
				parts.Add(part);
			}
		}
	}

	private void DoLeftPanel(Rect inRect)
	{
		DoPreview(inRect);
	}

	private void DoPreview(Rect inRect)
	{
		Rect rect = inRect;
		rect.width = Mathf.Min(inRect.width, inRect.height);
		rect.height = rect.width;
		rect = rect.CenteredOnXIn(inRect);
		rect = rect.CenteredOnYIn(inRect);
		Widgets.DrawBoxSolid(rect, Widgets.MenuSectionBGBorderColor);
		Widgets.DrawBoxSolid(rect.ContractedBy(1f), new Color(0.2f, 0.2f, 0.2f));
		global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(rect, weaponDef, links.Where((AttachmentLink l) => !hidden[l]).ToHashSet(), parts, null, null);
	}
}

using System.Linq;
using CombatExtended.RocketGUI;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ITab_AttachmentView : ITab
{
	private AttachmentLink highlighted = null;

	private static readonly Listing_Collapsible collapsible = new Listing_Collapsible();

	public override bool IsVisible => base.SelThing is WeaponPlatform;

	public WeaponPlatform Weapon => base.SelThing as WeaponPlatform;

	public ITab_AttachmentView()
	{
		size = new Vector2(460f, 450f);
		labelKey = "TabAttachments";
		tutorTag = "Attachments";
		collapsible.CollapsibleBGBorderColor = Color.gray;
		collapsible.Margins = new Vector2(3f, 0f);
	}

	public override void FillTab()
	{
		collapsible.Expanded = true;
		highlighted = null;
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);
		global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
		{
			DoContent(rect);
		});
	}

	private void DoContent(Rect inRect)
	{
		collapsible.Begin(inRect);
		collapsible.Label(Weapon.def.DescriptionDetailed);
		Rect weaponRect = inRect;
		collapsible.Lambda(100f, delegate(Rect rect)
		{
			weaponRect = rect;
		});
		collapsible.Lambda(20f, delegate(Rect rect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.LowerRight;
			bool flag = Find.WindowStack.IsOpen<Window_AttachmentsEditor>();
			GUI.color = (flag ? Color.gray : (Mouse.IsOver(rect) ? Color.white : Color.cyan));
			Widgets.Label(rect, "CE_AttachmentsEdit".Translate());
			if (!flag && Widgets.ButtonInvisible(rect))
			{
				Find.WindowStack.Add(new Window_AttachmentsEditor(Weapon));
			}
			Text.Anchor = TextAnchor.LowerLeft;
			GUI.color = Color.gray;
			Widgets.Label(rect, "CE_Attachments".Translate());
		}, invert: false, useMargins: true);
		collapsible.Line(1f);
		collapsible.Gap(1f);
		AttachmentLink[] curLinks = Weapon.CurLinks;
		foreach (AttachmentLink link in curLinks)
		{
			collapsible.Lambda(28f, delegate(Rect rect)
			{
				Widgets.DefLabelWithIcon(rect, link.attachment);
				Widgets.InfoCardButton(rect.RightPartPixels(rect.height).ContractedBy(1f), link.attachment);
				global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
				{
					GUI.color = Color.gray;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect.LeftPartPixels(rect.width - rect.height - 5f), link.attachment.slotTags[0]);
				});
				if (Mouse.IsOver(rect))
				{
					highlighted = link;
				}
			}, invert: false, useMargins: true);
		}
		collapsible.Gap(4f);
		collapsible.Label("CE_AttachmentsAdditions".Translate(), Color.gray, null, invert: false, hightlightIfMouseOver: true, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
		collapsible.Line(1f);
		foreach (AttachmentDef attachment in Weapon.AdditionList)
		{
			collapsible.Lambda(28f, delegate(Rect rect)
			{
				Widgets.DefLabelWithIcon(rect, attachment);
				Widgets.InfoCardButton(rect.RightPartPixels(rect.height).ContractedBy(1f), attachment);
				global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
				{
					GUI.color = Color.gray;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect.LeftPartPixels(rect.width - rect.height - 5f), attachment.slotTags[0]);
				});
				if (Mouse.IsOver(rect))
				{
					highlighted = Weapon.Platform.attachmentLinks.First((AttachmentLink l) => l.attachment == attachment);
				}
			}, invert: false, useMargins: true);
		}
		collapsible.Gap(4f);
		collapsible.Label("CE_AttachmentsRemovals".Translate(), Color.gray, null, invert: false, hightlightIfMouseOver: true, GameFont.Small, FontStyle.Normal, TextAnchor.LowerLeft);
		collapsible.Line(1f);
		foreach (AttachmentDef attachment2 in Weapon.RemovalList)
		{
			collapsible.Lambda(28f, delegate(Rect rect)
			{
				Widgets.DefLabelWithIcon(rect, attachment2);
				Widgets.InfoCardButton(rect.RightPartPixels(rect.height).ContractedBy(1f), attachment2);
				global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
				{
					GUI.color = Color.gray;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect.LeftPartPixels(rect.width - rect.height - 5f), attachment2.slotTags[0]);
				});
				if (Mouse.IsOver(rect))
				{
					highlighted = Weapon.Platform.attachmentLinks.First((AttachmentLink l) => l.attachment == attachment2);
				}
			}, invert: false, useMargins: true);
		}
		weaponRect.width = Mathf.Min(weaponRect.height, weaponRect.width);
		weaponRect = weaponRect.CenteredOnXIn(inRect);
		global::CombatExtended.RocketGUI.GUIUtility.DrawWeaponWithAttachments(weaponRect.ExpandedBy(10f), Weapon, highlighted, null);
		collapsible.End(ref inRect);
	}
}

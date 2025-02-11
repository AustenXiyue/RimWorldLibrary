using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.RocketGUI;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class Window_AttachmentsEditor : Window
{
	private readonly WeaponPlatform weapon;

	private readonly WeaponPlatformDef weaponDef;

	private readonly Fragment_AttachmentEditor editor;

	private readonly Action<List<AttachmentLink>> applyAction;

	public override Vector2 InitialSize => new Vector2(1000f, 675f);

	public Window_AttachmentsEditor(WeaponPlatform weapon)
	{
		this.weapon = weapon;
		weaponDef = weapon.Platform;
		editor = new Fragment_AttachmentEditor(weapon);
		layer = WindowLayer.Dialog;
		base.resizer = new WindowResizer();
		forcePause = true;
		doCloseButton = false;
		doCloseX = false;
		draggable = true;
	}

	public Window_AttachmentsEditor(WeaponPlatformDef weaponDef, List<AttachmentLink> attachments, Action<List<AttachmentLink>> applyAction)
	{
		this.applyAction = applyAction;
		this.weaponDef = weaponDef;
		editor = new Fragment_AttachmentEditor(weaponDef, attachments?.ToList());
		layer = WindowLayer.Dialog;
		base.resizer = new WindowResizer();
		forcePause = true;
		doCloseButton = false;
		doCloseX = false;
		draggable = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Rect rect = inRect;
			Text.Font = GameFont.Medium;
			rect.xMin += 5f;
			rect.height = 35f;
			Widgets.DefIcon(rect.LeftPartPixels(rect.height).ContractedBy(2f), weaponDef, null, 1.7f, null, drawPlaceholder: false, null, null, null);
			rect.xMin += rect.height + 10f;
			Widgets.Label(rect, "CE_EditAttachments".Translate() + (": " + weaponDef.label.CapitalizeFirst()));
		});
		inRect.yMin += 40f;
		Rect inRect2 = inRect;
		inRect2.height = 550f;
		editor.DoContents(inRect2);
		inRect.yMin += 555f;
		global::CombatExtended.RocketGUI.GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Text.Font = GameFont.Small;
			Rect rect2 = inRect;
			rect2.yMin += 5f;
			rect2.width = 300f;
			rect2 = rect2.CenteredOnXIn(inRect);
			GUI.color = Color.red;
			if (Widgets.ButtonText(rect2.RightPartPixels(146f), "CE_Close".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
			{
				Close();
			}
			GUI.color = Color.white;
			if (Widgets.ButtonText(rect2.LeftPartPixels(146f), "CE_Apply".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
			{
				Apply();
				Close();
			}
		});
	}

	private void Apply()
	{
		List<AttachmentLink> curConfig = editor.CurConfig;
		if (weapon != null)
		{
			weapon.TargetConfig = curConfig.Select((AttachmentLink l) => l.attachment).ToList();
			weapon.UpdateConfiguration();
			if (weapon.Wielder != null)
			{
				Job job = WorkGiver_ModifyWeapon.TryGetModifyWeaponJob(weapon.Wielder, weapon);
				if (job != null)
				{
					weapon.Wielder.jobs.StartJob(job, JobCondition.InterruptOptional, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, null);
				}
			}
			if (Prefs.DevMode && DebugSettings.godMode)
			{
				weapon.attachments.Clear();
				weapon.attachments.AddRange(curConfig);
				weapon.UpdateConfiguration();
			}
		}
		if (applyAction != null)
		{
			applyAction(curConfig);
		}
	}
}

using System.Collections.Generic;
using CombatExtended.Compatibility;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Dialog_SetMagCount : Window
{
	private readonly CompMechAmmo mechAmmo;

	private readonly Dictionary<AmmoDef, int> tmpLoadout;

	private const float BotAreaWidth = 30f;

	private const float BotAreaHeight = 30f;

	private new const float Margin = 3f;

	public override Vector2 InitialSize => new Vector2(300f, 130f);

	public Dialog_SetMagCount(CompMechAmmo mechAmmo)
	{
		if (mechAmmo == null)
		{
			Log.Error("null CompMechAmmo for Dialog_SetMagCount");
			return;
		}
		this.mechAmmo = mechAmmo;
		tmpLoadout = new Dictionary<AmmoDef, int>(mechAmmo.Loadouts);
	}

	public override void PreOpen()
	{
		if (mechAmmo != null)
		{
			Vector2 initialSize = InitialSize;
			initialSize.y = (float)(mechAmmo.AmmoUser.Props.ammoSet.ammoTypes.Count + 4) * 33f;
			windowRect = new Rect(((float)UI.screenWidth - initialSize.x) / 2f, ((float)UI.screenHeight - initialSize.y) / 2f, initialSize.x, initialSize.y);
			windowRect = windowRect.Rounded();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (mechAmmo == null || !mechAmmo.parent.Spawned || mechAmmo.ParentPawn.Dead)
		{
			Close();
			return;
		}
		float curY = 0f;
		string label = "MTA_MagazinePrefix".Translate(mechAmmo.AmmoUser.Props.magazineSize);
		DrawLabel(inRect, ref curY, label);
		foreach (AmmoLink ammoType in mechAmmo.AmmoUser.Props.ammoSet.ammoTypes)
		{
			int value = 0;
			tmpLoadout.TryGetValue(ammoType.ammo, out value);
			string label2 = ((ammoType.ammo.ammoClass.labelShort != null) ? ammoType.ammo.ammoClass.labelShort : ammoType.ammo.ammoClass.label);
			DrawThingRow(inRect, ref curY, ref value, ammoType.ammo, label2);
			tmpLoadout.SetOrAdd(ammoType.ammo, value);
		}
		curY += 3f;
		if (Widgets.ButtonText(new Rect(inRect.x, curY, inRect.width, 30f), "OK".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			SetMagCount(mechAmmo, tmpLoadout);
			Close();
		}
	}

	[Multiplayer.SyncMethod]
	private static void SetMagCount(CompMechAmmo mechAmmo, Dictionary<AmmoDef, int> tmpLoadout)
	{
		foreach (var (key, value) in tmpLoadout)
		{
			mechAmmo.Loadouts.SetOrAdd(key, value);
		}
		mechAmmo.TakeAmmoNow();
	}

	public void DrawThingRow(Rect rect, ref float curY, ref int count, Def defForIcon, string label)
	{
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.DefIcon(new Rect(rect.x, curY, 30f, 30f), defForIcon, null, 1f, null, drawPlaceholder: false, null, null, null);
		Widgets.Label(new Rect(rect.x + 30f + 3f, curY + 7.5f, rect.width - 120f, 30f), label);
		if (Widgets.ButtonText(new Rect(rect.x + rect.width - 120f, curY, 30f, 30f), "-", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			count -= GenUI.CurrentAdjustmentMultiplier();
		}
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(new Rect(rect.x + rect.width - 90f, curY + 7.5f, 60f, 30f), count.ToString());
		Text.Anchor = TextAnchor.UpperLeft;
		if (Widgets.ButtonText(new Rect(rect.x + rect.width - 30f, curY, 30f, 30f), "+", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			count += GenUI.CurrentAdjustmentMultiplier();
		}
		count = ((count >= 0) ? count : 0);
		curY += 33f;
	}

	public void DrawLabel(Rect rect, ref float curY, string label)
	{
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(new Rect(rect.x + 30f, curY, rect.width - 60f, 30f), label.ToString());
		curY += 33f;
	}
}

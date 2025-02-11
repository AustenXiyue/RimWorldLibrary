using System.Collections.Generic;
using CombatExtended.Compatibility;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Dialog_SetMagCountBatched : Window
{
	private readonly List<CompMechAmmo> _mechAmmoList;

	private readonly Dictionary<AmmoDef, int> _tmpLoadouts;

	private readonly List<AmmoLink> _mechAmmoTypes;

	private readonly int _mechMagazineSize;

	private const float BotAreaWidth = 30f;

	private const float BotAreaHeight = 30f;

	private new const float Margin = 10f;

	private float headerHeight;

	private string Maglabel;

	public override Vector2 InitialSize => new Vector2(300f, 130f);

	public Dialog_SetMagCountBatched(List<CompMechAmmo> mechAmmoList)
	{
		if (mechAmmoList == null || mechAmmoList.Count == 0)
		{
			Log.Error("null or empty CompMechAmmo list for Dialog_SetMagCount");
			_mechAmmoList = new List<CompMechAmmo>();
			return;
		}
		_mechAmmoList = mechAmmoList;
		_mechAmmoTypes = mechAmmoList[0].AmmoUser.Props.ammoSet.ammoTypes;
		_mechMagazineSize = _mechAmmoList[0].AmmoUser.Props.magazineSize;
		_tmpLoadouts = new Dictionary<AmmoDef, int>(mechAmmoList[0].Loadouts);
	}

	public override void PreOpen()
	{
		if (_mechAmmoList.Count != 0)
		{
			Vector2 initialSize = InitialSize;
			Maglabel = "MTA_MagazinePrefix".Translate(_mechMagazineSize);
			Text.Font = GameFont.Small;
			headerHeight = Text.CalcHeight(Maglabel, initialSize.x);
			initialSize.y = (float)(_mechAmmoTypes.Count + 3) * 30f + headerHeight;
			windowRect = new Rect(((float)UI.screenWidth - initialSize.x) / 2f, ((float)UI.screenHeight - initialSize.y) / 2f, initialSize.x, initialSize.y);
			windowRect = windowRect.Rounded();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		_mechAmmoList.RemoveAll((CompMechAmmo m) => !m.parent.Spawned || m.ParentPawn.Dead);
		if (_mechAmmoList.Count == 0)
		{
			Close();
			return;
		}
		Text.Font = GameFont.Small;
		float num = 0f;
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(inRect, Maglabel);
		num += headerHeight + 10f;
		foreach (AmmoLink mechAmmoType in _mechAmmoTypes)
		{
			int value = 0;
			_tmpLoadouts.TryGetValue(mechAmmoType.ammo, out value);
			string label = ((mechAmmoType.ammo.ammoClass.labelShort != null) ? mechAmmoType.ammo.ammoClass.labelShort : mechAmmoType.ammo.ammoClass.label);
			DrawThingRow(inRect, ref num, ref value, mechAmmoType.ammo, label);
			_tmpLoadouts.SetOrAdd(mechAmmoType.ammo, value);
		}
		num += 10f;
		if (Widgets.ButtonText(new Rect(inRect.x, num, inRect.width, 30f), "OK".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			SetMagCount(_mechAmmoList, _tmpLoadouts);
			Close();
		}
	}

	[Multiplayer.SyncMethod]
	private static void SetMagCount(List<CompMechAmmo> mechAmmoList, Dictionary<AmmoDef, int> tmpLoadouts)
	{
		foreach (CompMechAmmo mechAmmo in mechAmmoList)
		{
			foreach (var (key, value) in tmpLoadouts)
			{
				mechAmmo.Loadouts.SetOrAdd(key, value);
			}
			mechAmmo.TakeAmmoNow();
		}
	}

	public void DrawThingRow(Rect rect, ref float curY, ref int count, Def defForIcon, string label)
	{
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.DefIcon(new Rect(rect.x, curY, 30f, 30f), defForIcon, null, 1f, null, drawPlaceholder: false, null, null, null);
		Widgets.Label(new Rect(rect.x + 30f + 10f, curY, rect.width - 120f, 30f), label);
		if (Widgets.ButtonText(new Rect(rect.x + rect.width - 120f, curY, 30f, 30f), "-", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			count -= GenUI.CurrentAdjustmentMultiplier();
		}
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(new Rect(rect.x + rect.width - 90f, curY, 60f, 30f), count + " (" + count * _mechMagazineSize + ")");
		Text.Anchor = TextAnchor.UpperLeft;
		if (Widgets.ButtonText(new Rect(rect.x + rect.width - 30f, curY, 30f, 30f), "+", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			count += GenUI.CurrentAdjustmentMultiplier();
		}
		count = ((count >= 0) ? count : 0);
		curY += 30f;
	}
}

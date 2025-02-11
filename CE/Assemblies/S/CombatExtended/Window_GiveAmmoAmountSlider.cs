using UnityEngine;
using Verse;

namespace CombatExtended;

public class Window_GiveAmmoAmountSlider : Window
{
	public float ammoToGiveAmount = 1f;

	public CompAmmoGiver sourceComp;

	public Thing sourceAmmo;

	public Pawn selPawn;

	public Pawn dad;

	public bool finalized = false;

	public int maxAmmoCount;

	public override Vector2 InitialSize => new Vector2(350f, 125f);

	public override void DoWindowContents(Rect inRect)
	{
		Widgets.HorizontalSlider(inRect.TopHalf().BottomHalf(), ref ammoToGiveAmount, new FloatRange(0f, maxAmmoCount), "CE_AmmoAmount".Translate() + " " + ammoToGiveAmount.ToString(), 1f);
		if (Widgets.ButtonText(inRect.BottomHalf().LeftHalf(), "Cancel".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			Close();
		}
		if (Widgets.ButtonText(inRect.BottomHalf().RightHalf(), "OK".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			finalized = true;
			Close();
		}
	}

	public override void Close(bool doCloseSound = true)
	{
		if (finalized)
		{
			sourceComp.GiveAmmo(selPawn, sourceAmmo, (int)ammoToGiveAmount);
		}
		base.Close(doCloseSound);
	}
}

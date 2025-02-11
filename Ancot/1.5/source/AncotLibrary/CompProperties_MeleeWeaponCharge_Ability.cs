using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompProperties_MeleeWeaponCharge_Ability : CompProperties
{
	public int chargePerUse = 1;

	public bool destroyOnEmpty = false;

	public bool autoRecharge = true;

	public bool maxChargeCanMultiply = false;

	public bool maxChargeSpeedCanMultiply = false;

	public Color barColor = new Color(0.35f, 0.35f, 0.2f);

	public CompProperties_MeleeWeaponCharge_Ability()
	{
		compClass = typeof(CompMeleeWeaponCharge_Ability);
	}
}

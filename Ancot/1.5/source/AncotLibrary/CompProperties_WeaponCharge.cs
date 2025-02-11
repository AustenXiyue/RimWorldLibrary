using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompProperties_WeaponCharge : CompProperties
{
	public ThingDef projectileCharged;

	public ThingDef projectileCharged_Switched;

	public float chargeOnResetRatio = 1f;

	public bool destroyOnEmpty = false;

	public bool autoRecharge = true;

	public int resetTicks = -1;

	public float emptyWarmupFactor = 1f;

	public bool maxChargeCanMultiply = true;

	public bool maxChargeSpeedCanMultiply = true;

	public EffecterDef chargeFireEffecter;

	public Color barColor = new Color(0.35f, 0.35f, 0.2f);

	public CompProperties_WeaponCharge()
	{
		compClass = typeof(CompWeaponCharge);
	}
}

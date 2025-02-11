using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompProperties_OverChargeShot : CompProperties
{
	public Color barColor = new Color(0.35f, 0.35f, 0.2f);

	public float defaultWarmupTime = 1f;

	public float minWarmupTime = 1f;

	public float warmupTimeDecreasePerShot = 0.1f;

	public float cooldownChargePerTick = 0.01f;

	public float maxCharge = 100f;

	public bool destroyOnFull = false;

	public CompProperties_OverChargeShot()
	{
		compClass = typeof(CompOverChargeShot);
	}
}

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AncotLibrary;

public class CompProperties_PhysicalShield : CompProperties
{
	public float maxStamina = 100f;

	public float staminaGainPerTick = 0.04f;

	public float staminaConsumeRateRange = 0.05f;

	public float staminaConsumeRateMelee = 2f;

	public int startingTicksToReset = 1200;

	public int shieldBreakStanceTick = 60;

	public float defenseAngle = 180f;

	public float thresholdStaminaCostPct = 10f;

	public Color shieldBarColor = new Color(0.75f, 0.75f, 0.75f);

	public HediffDef holdShieldHediff;

	public List<Tool> tools;

	public string graphicPath_Holding;

	public string graphicPath_Ready;

	public string graphicPath_Disabled;

	public int gizmoOrder = -99;

	public string barGizmoLabel = "Shield";

	public string gizmoLabel = "Hold shield";

	public string gizmoDesc = "hold shield if possible.";

	public string gizmoIconPath = "AncotLibrary/Gizmos/SwitchShield";

	public bool blocksRangedWeapons = true;

	public bool alwaysHoldShield = false;

	public bool recordLastHarmTickWhenBlocked = true;

	public EffecterDef blockEffecter;

	public EffecterDef breakEffecter;

	public CompProperties_PhysicalShield()
	{
		compClass = typeof(CompPhysicalShield);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
	}
}

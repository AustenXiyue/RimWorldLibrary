using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ProjectilePropertiesCE : ProjectileProperties
{
	public TravelingShellProperties shellingProps;

	public int pelletCount = 1;

	public float spreadMult = 1f;

	public List<SecondaryDamage> secondaryDamage = new List<SecondaryDamage>();

	public bool damageAdjacentTiles = false;

	public bool dropsCasings = false;

	public string casingMoteDefname = "Fleck_EmptyCasing";

	public string casingFilthDefname = "Filth_RifleAmmoCasings";

	public float gravityFactor = 1f;

	public bool isInstant = false;

	public bool damageFalloff = true;

	public float armorPenetrationSharp;

	public float armorPenetrationBlunt;

	public bool castShadow = true;

	public float suppressionFactor = 1f;

	public float airborneSuppressionFactor = 1f;

	public float dangerFactor = 1f;

	public FloatRange ballisticCoefficient = new FloatRange(1f, 1f);

	public FloatRange mass = new FloatRange(1f, 1f);

	public FloatRange diameter = new FloatRange(1f, 1f);

	public bool lerpPosition = true;

	public ThingDef detonateMoteDef;

	public FleckDef detonateFleckDef;

	public float detonateEffectsScaleOverride = -1f;

	public int? tickToTruePos;

	[MustTranslate]
	public string genericLabelOverride = null;

	public int fuze_delay = 2;

	public bool HP_penetration = false;

	public float HP_penetration_ratio = 1f;

	public int armingDelay = 0;

	public float aimHeightOffset = 0f;

	public float empShieldBreakChance = 1f;

	public int TickToTruePos => tickToTruePos ?? Mathf.Max(3, Mathf.CeilToInt(600f / speed));

	public float Gravity => 1.96f * gravityFactor;
}

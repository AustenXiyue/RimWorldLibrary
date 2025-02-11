using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class CompProperties_ExplosiveCE : CompProperties
{
	public float damageAmountBase = -1f;

	public List<ThingDefCountClass> fragments = new List<ThingDefCountClass>();

	public float fragSpeedFactor = 1f;

	public float explosiveRadius = 1.9f;

	public DamageDef explosiveDamageType;

	public SoundDef explosionSound = null;

	public ThingDef postExplosionSpawnThingDef = null;

	public float postExplosionSpawnChance = 0f;

	public int postExplosionSpawnThingCount = 1;

	public bool applyDamageToExplosionCellsNeighbors = false;

	public ThingDef preExplosionSpawnThingDef = null;

	public float preExplosionSpawnChance = 0f;

	public int preExplosionSpawnThingCount = 1;

	public bool damageFalloff = true;

	public float chanceToStartFire;

	public GasType? postExplosionGasType;

	public CompProperties_ExplosiveCE()
	{
		compClass = typeof(CompExplosiveCE);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (explosiveRadius <= 0f)
		{
			yield return "explosiveRadius smaller or equal to zero, this explosion cannot occur";
		}
		if (parentDef.tickerType != TickerType.Normal)
		{
			yield return "CompExplosiveCE requires Normal ticker type";
		}
		if (fragments.Any())
		{
			yield return "fragments is removed from CompExplosiveCE, please use CombatExtended.CompFragments instead";
		}
	}

	public override void ResolveReferences(ThingDef parentDef)
	{
		base.ResolveReferences(parentDef);
		if (explosiveDamageType == null)
		{
			explosiveDamageType = DamageDefOf.Bomb;
		}
	}
}

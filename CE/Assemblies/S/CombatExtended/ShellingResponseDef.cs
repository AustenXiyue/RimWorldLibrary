using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class ShellingResponseDef : Def
{
	public class ShellingResponsePart_WorldObject
	{
		public WorldObjectDef worldObject;

		public float shellingPropability;

		public float raidPropability;

		public float raidMTBDays;
	}

	public class ShellingResponsePart_Projectile
	{
		public ThingDef projectile;

		public float points;

		public float weight = 0.1f;
	}

	public float defaultShellingPropability = 0f;

	public float defaultRaidPropability = 0f;

	public float defaultRaidMTBDays = 0f;

	public FloatRange retaliationShellingCooldownImpact = new FloatRange(1f, 10f);

	public List<ShellingResponsePart_Projectile> projectiles = new List<ShellingResponsePart_Projectile>();

	public List<ShellingResponsePart_WorldObject> worldObjects = new List<ShellingResponsePart_WorldObject>();

	public override void PostLoad()
	{
		if (projectiles == null)
		{
			projectiles = new List<ShellingResponsePart_Projectile>();
		}
		if (worldObjects == null)
		{
			worldObjects = new List<ShellingResponsePart_WorldObject>();
		}
		base.PostLoad();
	}
}

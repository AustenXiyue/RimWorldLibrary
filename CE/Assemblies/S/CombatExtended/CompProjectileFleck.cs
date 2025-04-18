using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class CompProjectileFleck : ThingComp
{
	public Vector3 lastPos;

	private ProjectileCE projectile;

	public int age => projectile.FlightTicks;

	private CompProperties_ProjectileFleck Props => (CompProperties_ProjectileFleck)props;

	private bool IsOn
	{
		get
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (projectile == null)
			{
				return false;
			}
			return true;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		projectile = parent as ProjectileCE;
	}

	public override void CompTick()
	{
		if (IsOn)
		{
			if (lastPos == Vector3.zero && parent.Spawned)
			{
				lastPos = parent.DrawPos;
			}
			Emit();
		}
	}

	private void Emit()
	{
		Vector3 vector = lastPos - parent.DrawPos;
		foreach (ProjectileFleckDataCE fleckData in Props.FleckDatas)
		{
			if ((fleckData.cutoffTickRange.max >= 0 && age >= fleckData.cutoffTickRange.max) || !fleckData.shouldEmit(age))
			{
				continue;
			}
			float num = 0f;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < fleckData.emissionAmount; i++)
			{
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(parent.DrawPos - fleckData.originOffsetInternal * vector + zero, parent.MapHeld, fleckData.fleck, (fleckData.scale.RandomInRange + num) * fleckData.cutoffScaleOffset(age));
				dataStatic.rotation = fleckData.rotation.RandomInRange;
				for (int j = 0; j < fleckData.flecksPerEmission; j++)
				{
					parent.MapHeld.flecks.CreateFleck(dataStatic);
				}
				if (fleckData.emissionAmount > 1)
				{
					zero += vector / fleckData.emissionAmount;
					num += fleckData.scaleOffsetInternal;
				}
			}
		}
		lastPos = parent.DrawPos;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastPos, "lastPos");
	}
}

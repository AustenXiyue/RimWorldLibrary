using Verse;
using Verse.Sound;

namespace CombatExtended.Lasers;

public class Building_LaserGunCE : Building_TurretGunCE, IBeamColorThing
{
	public bool isCharged = false;

	public int previousBurstCooldownTicksLeft = 0;

	private int beamColorIndex = -1;

	private Building_LaserGunDef laserGunDef => def as Building_LaserGunDef;

	public int BurstCooldownTicksLeft => burstCooldownTicksLeft;

	public int BurstWarmupTicksLeft => burstWarmupTicksLeft;

	public int BeamColor
	{
		get
		{
			return LaserColor.IndexBasedOnThingQuality(beamColorIndex, this);
		}
		set
		{
			beamColorIndex = value;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref isCharged, "isCharged", defaultValue: false);
		Scribe_Values.Look(ref previousBurstCooldownTicksLeft, "previousBurstCooldownTicksLeft", 0);
		Scribe_Values.Look(ref beamColorIndex, "beamColorIndex", -1);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
	}

	public override void Tick()
	{
		if (burstCooldownTicksLeft > previousBurstCooldownTicksLeft)
		{
			isCharged = false;
		}
		previousBurstCooldownTicksLeft = burstCooldownTicksLeft;
		if (!isCharged && Drain(laserGunDef.beamPowerConsumption))
		{
			isCharged = true;
		}
		if (isCharged || burstCooldownTicksLeft > 1)
		{
			int num = burstWarmupTicksLeft;
			base.Tick();
			if (burstWarmupTicksLeft == def.building.turretBurstWarmupTime.RandomInRange.SecondsToTicks() - 1 && num == burstWarmupTicksLeft + 1 && AttackVerb.verbProps.soundAiming != null)
			{
				AttackVerb.verbProps.soundAiming.PlayOneShot(new TargetInfo(base.Position, base.Map));
			}
		}
	}

	public float AvailablePower()
	{
		if (powerComp.PowerNet == null)
		{
			return 0f;
		}
		return powerComp.PowerNet.CurrentStoredEnergy();
	}

	public bool Drain(float amount)
	{
		if (amount <= 0f)
		{
			return true;
		}
		if (AvailablePower() < amount)
		{
			return false;
		}
		powerComp.PowerNet.ChangeStoredEnergy(0f - amount);
		return true;
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!isCharged)
		{
			text += "\n";
			text += "LaserTurretNotCharged".Translate();
		}
		return text;
	}
}

using Verse;

namespace CombatExtended.Lasers;

internal class SpinningLaserGunTurret : SpinningLaserGunBase
{
	internal Building_LaserGunCE turret;

	public override void UpdateState()
	{
		if (turret == null)
		{
			return;
		}
		switch (state)
		{
		case State.Idle:
			if (turret.BurstWarmupTicksLeft > 0)
			{
				state = State.Spinup;
				ReachRotationSpeed(base.def.rotationSpeed, turret.BurstWarmupTicksLeft);
			}
			break;
		case State.Spinup:
			if (turret.BurstWarmupTicksLeft == 0 || turret.AttackVerb.state == VerbState.Bursting)
			{
				state = State.Spinning;
			}
			break;
		case State.Spinning:
			if (turret.AttackVerb.state != VerbState.Bursting)
			{
				state = State.Idle;
				int burstCooldownTicksLeft = turret.BurstCooldownTicksLeft;
				ReachRotationSpeed(0f, (burstCooldownTicksLeft == -1) ? 30 : burstCooldownTicksLeft);
			}
			break;
		}
	}
}

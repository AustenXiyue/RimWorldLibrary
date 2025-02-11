using Verse;

namespace CombatExtended.Lasers;

internal class SpinningLaserGun : SpinningLaserGunBase
{
	private bool IsBrusting(Pawn pawn)
	{
		if (pawn.CurrentEffectiveVerb == null)
		{
			return false;
		}
		return pawn.CurrentEffectiveVerb.Bursting;
	}

	public override void UpdateState()
	{
		if (!(base.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker))
		{
			return;
		}
		Stance curStance = pawn_EquipmentTracker.pawn.stances.curStance;
		switch (state)
		{
		case State.Idle:
			if (curStance is Stance_Warmup stance_Warmup2)
			{
				state = State.Spinup;
				ReachRotationSpeed(base.def.rotationSpeed, stance_Warmup2.ticksLeft);
			}
			break;
		case State.Spinup:
		{
			if (IsBrusting(pawn_EquipmentTracker.pawn))
			{
				state = State.Spinning;
				break;
			}
			Stance_Warmup stance_Warmup = curStance as Stance_Warmup;
			if (stance_Warmup == null)
			{
				state = State.Idle;
				ReachRotationSpeed(0f, 30);
			}
			break;
		}
		case State.Spinning:
			if (!IsBrusting(pawn_EquipmentTracker.pawn))
			{
				state = State.Idle;
				if (curStance is Stance_Cooldown stance_Cooldown)
				{
					ReachRotationSpeed(0f, stance_Cooldown.ticksLeft);
				}
				else
				{
					ReachRotationSpeed(0f, 0);
				}
			}
			break;
		}
	}
}

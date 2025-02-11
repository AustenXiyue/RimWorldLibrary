using AncotLibrary;
using Verse;

namespace Milira;

public class CompMechCarrier_Consul : CompMechCarrier_Custom
{
	public override PawnKindDef SpawnPawnKind
	{
		get
		{
			if (ModsConfig.IsActive("Ancot.MilianModification"))
			{
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_ShieldUnitLauncher) != null)
				{
					return MiliraDefOf.Milian_FloatUnit_SmallShield;
				}
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_BombUnitLauncher) != null)
				{
					return MiliraDefOf.Milian_FloatUnit_SmallBomb;
				}
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_SniperUnitLauncher) != null)
				{
					return MiliraDefOf.Milian_FloatUnit_SmallSniper;
				}
			}
			return base.SpawnPawnKind;
		}
	}

	public override int CostPerPawn
	{
		get
		{
			int num = base.CostPerPawn;
			if (ModsConfig.IsActive("Ancot.MilianModification"))
			{
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_ImprovedAssemblyProcess) != null)
				{
					num -= 10;
				}
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_IncrementalAssemblyProcess) != null)
				{
					num += 10;
				}
			}
			return num;
		}
	}

	public override int CooldownTicks
	{
		get
		{
			int num = base.CooldownTicks;
			if (ModsConfig.IsActive("Ancot.MilianModification"))
			{
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_ImprovedAssemblyProcess) != null)
				{
					num = (int)((float)num * 1.3f);
				}
				if (base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_IncrementalAssemblyProcess) != null)
				{
					num = 600;
				}
			}
			return num;
		}
	}

	public override int RecoverTicks
	{
		get
		{
			int num = base.RecoverTicks;
			if (ModsConfig.IsActive("Ancot.MilianModification") && base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_ImprovedRecycleProcess) != null)
			{
				num = (int)((float)num * 0.6f);
			}
			return num;
		}
	}

	public override float RecoverFactor
	{
		get
		{
			float result = base.RecoverFactor;
			if (ModsConfig.IsActive("Ancot.MilianModification") && base.pivot.health.hediffSet.GetFirstHediffOfDef(MiliraDefOf.MilianFitting_ImprovedRecycleProcess) != null)
			{
				result = 1f;
			}
			return result;
		}
	}
}

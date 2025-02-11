using System;
using Verse;

namespace CombatExtended;

public class CompProperties_BipodComp : CompProperties
{
	public int additionalrange = 12;

	public float recoilMulton = 0.5f;

	public float recoilMultoff = 1f;

	public float warmupMult = 0.85f;

	public float warmupPenalty = 2f;

	public float swayMult = 1f;

	public float swayPenalty = 1f;

	public int ticksToSetUp = 60;

	public BipodCategoryDef catDef;

	public CompProperties_BipodComp()
	{
		compClass = typeof(BipodComp);
	}

	public CompProperties_BipodComp(Type compClass)
		: base(compClass)
	{
		base.compClass = compClass;
	}
}

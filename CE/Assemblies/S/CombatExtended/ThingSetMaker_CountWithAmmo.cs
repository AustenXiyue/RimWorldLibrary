using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class ThingSetMaker_CountWithAmmo : ThingSetMaker_Count
{
	private IntRange magCount = new IntRange(2, 5);

	private bool random = false;

	private bool canGenerateAdvanced = false;

	public override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		base.Generate(parms, outThings);
		CE_ThingSetMakerUtility.GenerateAmmoForWeapon(outThings, random, canGenerateAdvanced, magCount);
	}
}

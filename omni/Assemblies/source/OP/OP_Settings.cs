using Verse;

namespace OP;

public class OP_Settings : ModSettings
{
	public bool EnableClearEntropy = true;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref EnableClearEntropy, "OP_EnableClearEntropy", defaultValue: true);
	}
}

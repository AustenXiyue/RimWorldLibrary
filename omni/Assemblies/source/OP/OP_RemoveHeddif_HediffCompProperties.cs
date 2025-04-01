using Verse;

namespace OP;

public class OP_RemoveHeddif_HediffCompProperties : HediffCompProperties
{
	public int OP_Tick;

	public float OP_Psycast;

	public OP_RemoveHeddif_HediffCompProperties()
	{
		compClass = typeof(OP_PsyRegeneration_HediffComp);
	}
}

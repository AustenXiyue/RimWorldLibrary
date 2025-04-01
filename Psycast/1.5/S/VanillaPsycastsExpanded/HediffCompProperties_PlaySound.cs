using Verse;

namespace VanillaPsycastsExpanded;

public class HediffCompProperties_PlaySound : HediffCompProperties
{
	public SoundDef sustainer;

	public SoundDef endSound;

	public HediffCompProperties_PlaySound()
	{
		compClass = typeof(HediffComp_PlaySound);
	}
}

using Verse;

namespace CombatExtended;

public class ProjectileFleckDataCE
{
	public FleckDef fleck;

	private float emissionsPerTick = 1f;

	public int flecksPerEmission = 1;

	public FloatRange rotation = new FloatRange(0f, 360f);

	public FloatRange scale = new FloatRange(1f, 1f);

	public IntRange cutoffTickRange = new IntRange(-1, -1);

	public float originOffset = 0.7f;

	public int emissionAmount => (!(emissionsPerTick > 1f)) ? 1 : ((int)emissionsPerTick);

	public float originOffsetInternal => 1f - originOffset;

	public float scaleOffsetInternal => fleck.growthRate / (emissionsPerTick * 60f);

	public float cutoffScaleOffset(int age)
	{
		if (cutoffTickRange.max < 0 || age < cutoffTickRange.min)
		{
			return 1f;
		}
		return ((float)cutoffTickRange.max - (float)age) / (float)(cutoffTickRange.max - cutoffTickRange.min);
	}

	public bool shouldEmit(int age)
	{
		if (emissionsPerTick >= 1f)
		{
			return true;
		}
		return age % (int)(1f / emissionsPerTick) == 0;
	}
}

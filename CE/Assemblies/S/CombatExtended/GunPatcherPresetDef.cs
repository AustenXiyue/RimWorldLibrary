using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatExtended;

public class GunPatcherPresetDef : Def
{
	public VerbPropertiesCE gunStats;

	public float ReloadTime;

	public int AmmoCapacity;

	public float CooldownTime;

	public SimpleCurve MassCurve;

	public float Mass;

	public SimpleCurve rangeCurve;

	public SimpleCurve warmupCurve;

	public SimpleCurve cooldownCurve;

	public List<StatModifier> MiscOtherStats;

	public float Bulk;

	public float Spread;

	public float Sway;

	public AmmoSetDef setCaliber;

	public bool reloadOneAtATime = false;

	public CompProperties_FireModes fireModes;

	public bool addBipods;

	public string bipodTag;

	public List<string> addTags;

	public List<string> tags;

	public List<string> names;

	public FloatRange RangeRange;

	public FloatRange WarmupRange;

	public FloatRange damageRange;

	public FloatRange projSpeedRange;

	public bool DiscardDesignations;

	public List<LabelGun> specialGuns = new List<LabelGun>();

	public bool DetermineCaliber;

	public List<CaliberFloatRange> CaliberRanges = new List<CaliberFloatRange>();

	public FloatRange DamageRange
	{
		get
		{
			FloatRange result = damageRange;
			if (CaliberRanges != null)
			{
				if (CaliberRanges.Any((CaliberFloatRange x) => x.DamageRange.max > result.max))
				{
					result.max = CaliberRanges.MaxBy((CaliberFloatRange z) => z.DamageRange.max).DamageRange.max;
				}
				if (CaliberRanges.Any((CaliberFloatRange x) => x.DamageRange.min < result.min))
				{
					result.min = CaliberRanges.MinBy((CaliberFloatRange z) => z.DamageRange.min).DamageRange.min;
				}
			}
			return result;
		}
	}

	public FloatRange ProjSpeedRange
	{
		get
		{
			FloatRange result = projSpeedRange;
			if (CaliberRanges != null)
			{
				if (CaliberRanges.Any((CaliberFloatRange x) => x.SpeedRange.max > result.max))
				{
					result.max = CaliberRanges.MaxBy((CaliberFloatRange z) => z.SpeedRange.max).SpeedRange.max;
				}
				if (CaliberRanges.Any((CaliberFloatRange x) => x.SpeedRange.min < result.min))
				{
					result.min = CaliberRanges.MinBy((CaliberFloatRange z) => z.SpeedRange.min).SpeedRange.min;
				}
			}
			return result;
		}
	}
}

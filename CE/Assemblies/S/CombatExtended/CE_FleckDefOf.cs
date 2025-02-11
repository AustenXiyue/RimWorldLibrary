using RimWorld;
using Verse;

namespace CombatExtended;

[DefOf]
public class CE_FleckDefOf
{
	public static FleckDef BlastFlame;

	public static FleckDef ElectricalSpark;

	public static FleckDef Fleck_HeatGlow_API;

	public static FleckDef Fleck_BulletHole;

	public static FleckDef Fleck_ElectricGlow_EMP;

	public static FleckDef Fleck_SparkThrownFast;

	public static FleckDef Fleck_EmptyCasing;

	static CE_FleckDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(CE_FleckDefOf));
	}
}

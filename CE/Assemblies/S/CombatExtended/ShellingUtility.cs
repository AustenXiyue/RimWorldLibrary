using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public static class ShellingUtility
{
	private static Map map;

	private static ThingDef shellDef;

	private static ProjectilePropertiesCE props;

	private static DamageDef projectileDamageDef;

	private static readonly List<IntVec3> potentialCells = new List<IntVec3>(10000);

	public static IntVec3 FindRandomImpactCell(Map map, ThingDef shellDef = null)
	{
		ShellingUtility.map = map;
		ShellingUtility.shellDef = shellDef;
		IntVec3 result = IntVec3.Invalid;
		if (map.IsPlayerHome)
		{
			props = (ProjectilePropertiesCE)shellDef.projectile;
			projectileDamageDef = props.damageDef;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			int num = 5;
			while (num-- >= 0 && (float)(stopwatch.ElapsedTicks / Stopwatch.Frequency) < 0.01f)
			{
				result = GetRandomPlayerHomeImpactCell();
				if (result.IsValid)
				{
					return result;
				}
			}
			stopwatch.Stop();
		}
		if (!result.IsValid)
		{
			result = GetRandomImpactCell();
		}
		map = null;
		return result;
	}

	private static IntVec3 GetRandomPlayerHomeImpactCell()
	{
		if (projectileDamageDef == CE_DamageDefOf.Bomb)
		{
			if (Rand.Chance((float)((ProjectileProperties)props).damageAmountBase / 5000f) && ((Area)map.areaManager.Home).innerGrid.trueCountInt > 0)
			{
				return GetRandomImpectCell(((Area)map.areaManager.Home).innerGrid);
			}
			if (Rand.Chance(0.05f) && map.zoneManager.zoneGrid.Length != 0)
			{
				IEnumerable<Zone> enumerable = map.zoneManager.zoneGrid.Where(delegate(Zone z)
				{
					int result;
					if (z != null)
					{
						List<IntVec3> cells = z.cells;
						result = ((cells != null && cells.Count > 0) ? 1 : 0);
					}
					else
					{
						result = 0;
					}
					return (byte)result != 0;
				});
				if (!enumerable.EnumerableNullOrEmpty())
				{
					return GetRandomImpectCell(enumerable.RandomElement().cells);
				}
			}
		}
		else if (projectileDamageDef == CE_DamageDefOf.PrometheumFlame && Rand.Chance(0.05f))
		{
			foreach (Zone item in map.zoneManager.zoneGrid.InRandomOrder())
			{
				if (item is Zone_Growing zone_Growing && !zone_Growing.cells.NullOrEmpty())
				{
					return GetRandomImpectCell(item.cells);
				}
			}
		}
		return IntVec3.Invalid;
	}

	private static IntVec3 GetRandomImpectCell(BoolGrid grid)
	{
		potentialCells.Clear();
		int num = 0;
		int num2 = ((!grid.minPossibleTrueIndexDirty) ? grid.minPossibleTrueIndexCached : 0);
		for (int i = num2; i < grid.arr.Length; i += Rand.Range(1, 32))
		{
			if (num >= grid.trueCountInt / 2)
			{
				break;
			}
			if (grid.arr[i])
			{
				IntVec3 intVec = CellIndicesUtility.IndexToCell(i, grid.mapSizeX);
				if (Valid(intVec))
				{
					num++;
					potentialCells.Add(intVec);
				}
			}
		}
		return potentialCells.RandomElementWithFallback(IntVec3.Invalid);
	}

	private static IntVec3 GetRandomImpectCell(List<IntVec3> cells)
	{
		int num = 0;
		while (num++ < cells.Count / 2)
		{
			IntVec3 intVec = cells[Rand.Range(0, 100000) % cells.Count];
			if (Valid(intVec))
			{
				return intVec;
			}
		}
		return IntVec3.Invalid;
	}

	private static IntVec3 GetRandomImpactCell()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while ((float)(stopwatch.ElapsedTicks / Stopwatch.Frequency) < 0.005f)
		{
			IntVec3 intVec = new IntVec3((int)CE_Utility.RandomGaussian(1f, map.Size.x - 1), 0, (int)CE_Utility.RandomGaussian(1f, map.Size.z - 1));
			if (Valid(intVec))
			{
				return intVec;
			}
		}
		stopwatch.Stop();
		return IntVec3.Invalid;
	}

	private static bool Valid(IntVec3 cell)
	{
		RoofDef roofDef = map.roofGrid.RoofAt(cell);
		if (roofDef == null || roofDef == RoofDefOf.RoofConstructed)
		{
			return true;
		}
		return false;
	}
}

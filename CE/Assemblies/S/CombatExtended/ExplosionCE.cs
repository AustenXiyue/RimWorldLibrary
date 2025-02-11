using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class ExplosionCE : Explosion
{
	public float height;

	public bool radiusChange = false;

	public bool toBeMerged = false;

	private const int DamageAtEdge = 2;

	private const float PenAtEdge = 0.6f;

	private const float PressurePerDamage = 0.3f;

	private const float MaxMergeTicks = 3f;

	public const float MaxMergeRange = 3f;

	public const bool MergeExplosions = false;

	public virtual IEnumerable<IntVec3> ExplosionCellsToHit
	{
		get
		{
			bool flag = base.Position.Roofed(base.Map);
			bool flag2 = height >= 2f;
			List<IntVec3> list = new List<IntVec3>();
			List<IntVec3> list2 = new List<IntVec3>();
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(base.Map))
				{
					continue;
				}
				if (flag2)
				{
					if ((!flag && GenSight.LineOfSight(base.Position, intVec, base.Map, skipFirstCell: false, null, 0, 0)) || !intVec.Roofed(base.Map))
					{
						list.Add(intVec);
					}
				}
				else
				{
					if (!GenSight.LineOfSight(base.Position, intVec, base.Map, skipFirstCell: true))
					{
						continue;
					}
					if (needLOSToCell1.HasValue || needLOSToCell2.HasValue)
					{
						bool flag3 = needLOSToCell1.HasValue && GenSight.LineOfSight(needLOSToCell1.Value, intVec, base.Map, skipFirstCell: false, null, 0, 0);
						bool flag4 = needLOSToCell2.HasValue && GenSight.LineOfSight(needLOSToCell2.Value, intVec, base.Map, skipFirstCell: false, null, 0, 0);
						if (!flag3 && !flag4)
						{
							continue;
						}
					}
					list.Add(intVec);
				}
			}
			foreach (IntVec3 item in list)
			{
				if (!item.Walkable(base.Map))
				{
					continue;
				}
				for (int j = 0; j < 4; j++)
				{
					IntVec3 intVec2 = item + GenAdj.CardinalDirections[j];
					if (intVec2.InHorDistOf(base.Position, radius) && intVec2.InBounds(base.Map) && !intVec2.Standable(base.Map) && intVec2.GetEdifice(base.Map) != null && !list.Contains(intVec2) && list2.Contains(intVec2))
					{
						list2.Add(intVec2);
					}
				}
			}
			return list.Concat(list2);
		}
	}

	public virtual bool MergeWith(ExplosionCE other, out ExplosionCE merged, out ExplosionCE nonMerged)
	{
		merged = null;
		nonMerged = null;
		if (other == null)
		{
			return false;
		}
		int num = ((((Explosion)other).startTick == 0) ? Find.TickManager.TicksGame : ((Explosion)other).startTick) - ((base.startTick == 0) ? Find.TickManager.TicksGame : base.startTick);
		if ((float)Mathf.Abs(num) > 3f)
		{
			return false;
		}
		if (other.damageFalloff != damageFalloff)
		{
			return false;
		}
		if (other.damType != damType)
		{
			return false;
		}
		if (Mathf.Sign(other.height - 2f) != Mathf.Sign(height - 2f))
		{
			return false;
		}
		if (preExplosionSpawnThingDef != null && other.preExplosionSpawnThingDef != null && other.preExplosionSpawnThingDef != preExplosionSpawnThingDef)
		{
			return false;
		}
		if (postExplosionSpawnThingDef != null && other.postExplosionSpawnThingDef != null && other.postExplosionSpawnThingDef != postExplosionSpawnThingDef)
		{
			return false;
		}
		Thing thing = null;
		if (other.instigator != null && instigator != null)
		{
			if (other.intendedTarget != null && intendedTarget != null && other.intendedTarget.HostileTo(other.instigator) && intendedTarget.HostileTo(instigator))
			{
				if (instigator.Faction != null && other.instigator.Faction != null && instigator.Faction != other.instigator.Faction)
				{
					return false;
				}
			}
			else if (other.instigator != instigator)
			{
				if (instigator is Pawn && other.instigator is Pawn)
				{
					return false;
				}
				if (instigator is Pawn)
				{
					thing = instigator;
				}
				else if (other.instigator is Pawn)
				{
					thing = other.instigator;
				}
				thing = ((!(instigator is AmmoThing) && !(other.instigator is AmmoThing)) ? ((Rand.Value < 0.5f) ? instigator : other.instigator) : ((instigator is AmmoThing) ? other.instigator : instigator));
			}
		}
		if (other.weapon != null && weapon != null && other.weapon != weapon)
		{
			return false;
		}
		if (other.projectile != null && projectile != null && other.projectile != projectile)
		{
			return false;
		}
		if (other.needLOSToCell1 != needLOSToCell1)
		{
			return false;
		}
		if (other.needLOSToCell2 != needLOSToCell2)
		{
			return false;
		}
		merged = ((num <= 0) ? this : other);
		nonMerged = ((num <= 0) ? other : this);
		merged.instigator = thing;
		HashSet<Thing> hashSet = new HashSet<Thing>();
		if (base.ignoredThings != null && ((Explosion)other).ignoredThings != null)
		{
			hashSet.AddRange(base.ignoredThings.Where((Thing x) => ((Explosion)other).ignoredThings.Contains(x)));
		}
		if (base.ignoredThings != null && base.ignoredThings.Contains(instigator))
		{
			hashSet.Add(instigator);
		}
		if (((Explosion)other).ignoredThings != null && ((Explosion)other).ignoredThings.Contains(other.instigator))
		{
			hashSet.Add(other.instigator);
		}
		((Explosion)merged).ignoredThings = hashSet.ToList();
		merged.chanceToStartFire = 1f - (1f - chanceToStartFire) * (1f - other.chanceToStartFire);
		merged.preExplosionSpawnThingCount = Mathf.RoundToInt(((float)preExplosionSpawnThingCount * preExplosionSpawnChance + (float)other.preExplosionSpawnThingCount * other.preExplosionSpawnChance) / (1f - (1f - preExplosionSpawnChance) * (1f - other.preExplosionSpawnChance)));
		merged.preExplosionSpawnChance = 1f - (1f - preExplosionSpawnChance) * (1f - other.preExplosionSpawnChance);
		merged.postExplosionSpawnThingCount = Mathf.RoundToInt(((float)postExplosionSpawnThingCount * postExplosionSpawnChance + (float)other.postExplosionSpawnThingCount * other.postExplosionSpawnChance) / (1f - (1f - postExplosionSpawnChance) * (1f - other.postExplosionSpawnChance)));
		merged.postExplosionSpawnChance = 1f - (1f - postExplosionSpawnChance) * (1f - other.postExplosionSpawnChance);
		merged.armorPenetration = Mathf.Max((float)damAmount * 0.3f, armorPenetration) + Mathf.Max((float)other.damAmount * 0.3f, other.armorPenetration);
		merged.damAmount = damAmount + other.damAmount;
		if (!merged.applyDamageToExplosionCellsNeighbors && nonMerged.applyDamageToExplosionCellsNeighbors)
		{
			merged.applyDamageToExplosionCellsNeighbors = true;
			merged.radiusChange = true;
		}
		if (radius != other.radius && merged.radius < nonMerged.radius)
		{
			merged.radius = Mathf.Max(radius, other.radius);
			merged.radiusChange = true;
		}
		return true;
	}

	public void RestartAfterMerge(SoundDef explosionSound)
	{
		if (base.startTick == 0 || radiusChange)
		{
			StartExplosionCE(explosionSound, base.ignoredThings);
			radiusChange = false;
		}
	}

	public void StartExplosionCE(SoundDef explosionSound, List<Thing> ignoredThings)
	{
		if (Controller.settings.MergeExplosions)
		{
			ExplosionCE explosionCE = base.Position.GetThingList(base.Map).FirstOrDefault((Thing x) => x.def == CE_ThingDefOf.ExplosionCE && x != this && !(x as ExplosionCE).toBeMerged) as ExplosionCE;
			if (explosionCE == null)
			{
				int i = 1;
				for (int num = GenRadial.NumCellsInRadius(3f); i < num; i++)
				{
					if (explosionCE != null)
					{
						break;
					}
					IntVec3 c = base.Position + GenRadial.RadialPattern[i];
					if (c.InBounds(base.Map))
					{
						explosionCE = c.GetThingList(base.Map).FirstOrDefault((Thing x) => x.def == CE_ThingDefOf.ExplosionCE && x != this && !(x as ExplosionCE).toBeMerged) as ExplosionCE;
					}
				}
			}
			if (explosionCE != null && MergeWith(explosionCE, out var merged, out var nonMerged))
			{
				nonMerged.toBeMerged = true;
				merged.RestartAfterMerge(explosionSound);
				return;
			}
		}
		if (!base.Spawned)
		{
			Log.Error("Called StartExplosion() on unspawned thing.");
			return;
		}
		if (base.ignoredThings.NullOrEmpty())
		{
			base.ignoredThings = ignoredThings;
		}
		base.startTick = Find.TickManager.TicksGame;
		base.cellsToAffect.Clear();
		base.damagedThings.Clear();
		base.addedCellsAffectedOnlyByDamage.Clear();
		base.cellsToAffect.AddRange(ExplosionCellsToHit);
		if (applyDamageToExplosionCellsNeighbors)
		{
			AddCellsNeighbors(base.cellsToAffect);
		}
		damType.Worker.ExplosionStart(this, base.cellsToAffect);
		PlayExplosionSound(explosionSound);
		FleckMakerCE.WaterSplash(base.Position.ToVector3Shifted(), base.Map, radius * 6f, 20f);
		base.cellsToAffect.Sort((IntVec3 a, IntVec3 b) => GetCellAffectTick(b).CompareTo(GetCellAffectTick(a)));
		RegionTraverser.BreadthFirstTraverse(base.Position, base.Map, (Region from, Region to) => true, delegate(Region x)
		{
			List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				((Pawn)list[num2]).mindState.Notify_Explosion(this);
			}
			return false;
		}, 25);
	}

	public override void Tick()
	{
		int ticksGame = Find.TickManager.TicksGame;
		int num = base.cellsToAffect.Count - 1;
		while (!toBeMerged && num >= 0 && ticksGame >= GetCellAffectTick(base.cellsToAffect[num]))
		{
			try
			{
				AffectCell(base.cellsToAffect[num]);
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat("Explosion could not affect cell ", base.cellsToAffect[num], ": ", ex));
			}
			base.cellsToAffect.RemoveAt(num);
			num--;
		}
		if (toBeMerged || !base.cellsToAffect.Any())
		{
			Destroy();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref base.startTick, "startTick", 0);
		Scribe_Collections.Look(ref base.cellsToAffect, "cellsToAffect", LookMode.Value);
		Scribe_Collections.Look(ref base.damagedThings, "damagedThings", LookMode.Reference);
		Scribe_Collections.Look(ref base.addedCellsAffectedOnlyByDamage, "addedCellsAffectedOnlyByDamage", LookMode.Value);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			base.damagedThings.RemoveAll((Thing x) => x == null);
		}
	}

	public int GetDamageAmountAtCE(IntVec3 c)
	{
		if (!damageFalloff)
		{
			return damAmount;
		}
		float f = c.DistanceTo(base.Position) / radius;
		f = Mathf.Pow(f, 0.333f);
		return Mathf.Max(GenMath.RoundRandom(Mathf.Lerp(damAmount, 2f, f)), 1);
	}

	public float GetArmorPenetrationAtCE(IntVec3 c)
	{
		float num = Mathf.Max((float)damAmount * 0.3f, armorPenetration);
		if (!damageFalloff)
		{
			return num;
		}
		float f = c.DistanceTo(base.Position) / radius;
		f = Mathf.Pow(f, 0.55f);
		return Mathf.Lerp(num, 0.6f, f);
	}
}

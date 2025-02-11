using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace CombatExtended.Utilities;

public class ThingsTrackingModel
{
	private struct ThingPositionInfo : IComparable<ThingPositionInfo>
	{
		public Thing thing;

		public int createdOn;

		public int Age
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return GenTicks.TicksGame - createdOn;
			}
		}

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return thing != null && thing.Spawned && thing.Position.InBounds(thing.Map);
			}
		}

		public ThingPositionInfo(Thing thing)
		{
			this.thing = thing;
			createdOn = GenTicks.TicksGame;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return thing.thingIDNumber;
		}

		public override bool Equals(object obj)
		{
			return obj.GetHashCode() == GetHashCode();
		}

		public override string ToString()
		{
			return thing.ToString() + ":" + thing.Position.ToString() + ":" + createdOn;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(ThingPositionInfo other)
		{
			return thing.Position.x.CompareTo(other.thing.Position.x);
		}

		public static bool operator >(ThingPositionInfo operand1, ThingPositionInfo operand2)
		{
			return operand1.CompareTo(operand2) == 1;
		}

		public static bool operator <(ThingPositionInfo operand1, ThingPositionInfo operand2)
		{
			return operand1.CompareTo(operand2) == -1;
		}

		public static bool operator >=(ThingPositionInfo operand1, ThingPositionInfo operand2)
		{
			return operand1.CompareTo(operand2) >= 0;
		}

		public static bool operator <=(ThingPositionInfo operand1, ThingPositionInfo operand2)
		{
			return operand1.CompareTo(operand2) <= 0;
		}
	}

	public readonly ThingDef def;

	public readonly ThingsTracker parent;

	public readonly Map map;

	private int count = 0;

	private Dictionary<Thing, int> indexByThing = new Dictionary<Thing, int>();

	private ThingPositionInfo[] sortedThings = new ThingPositionInfo[100];

	public ThingsTrackingModel(ThingDef def, Map map, ThingsTracker parent)
	{
		this.def = def;
		this.map = map;
		this.parent = parent;
	}

	public void Register(Thing thing)
	{
		if (indexByThing.ContainsKey(thing))
		{
			Notify_ThingPositionChanged(thing);
			return;
		}
		if (count + 1 >= sortedThings.Length)
		{
			ThingPositionInfo[] array = new ThingPositionInfo[sortedThings.Length * 2];
			for (int i = 0; i < sortedThings.Length; i++)
			{
				array[i] = sortedThings[i];
			}
			sortedThings = array;
		}
		sortedThings[count] = new ThingPositionInfo(thing);
		indexByThing[thing] = count;
		count++;
		int num = count - 1;
		while (num - 1 >= 0 && sortedThings[num] < sortedThings[num - 1])
		{
			Swap(num - 1, num, sortedThings);
			indexByThing[sortedThings[num].thing] = num;
			indexByThing[sortedThings[num - 1].thing] = num - 1;
			num--;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DeRegister(Thing thing)
	{
		if (indexByThing.TryGetValue(thing, out var value))
		{
			RemoveClean(value);
			indexByThing.Remove(thing);
		}
	}

	public void Notify_ThingPositionChanged(Thing thing)
	{
		if (!indexByThing.TryGetValue(thing, out var value))
		{
			Register(thing);
			return;
		}
		int i;
		for (i = value; i + 1 < count && sortedThings[i] > sortedThings[i + 1]; i++)
		{
			Swap(i + 1, i, sortedThings);
			indexByThing[sortedThings[i].thing] = i;
			indexByThing[sortedThings[i + 1].thing] = i + 1;
		}
		i = value;
		while (i - 1 >= 0 && sortedThings[i] < sortedThings[i - 1])
		{
			Swap(i - 1, i, sortedThings);
			indexByThing[sortedThings[i].thing] = i;
			indexByThing[sortedThings[i - 1].thing] = i - 1;
			i--;
		}
	}

	public IEnumerable<Thing> ThingsNearSegment(IntVec3 origin, IntVec3 destination, float range, bool behind, bool infront)
	{
		IntVec3 direction = destination - origin;
		float lengthSq = direction.x * direction.x + direction.z * direction.z;
		if (lengthSq == 0f)
		{
			if (!infront && !behind)
			{
				yield break;
			}
			foreach (Thing item in ThingsInRangeOf(origin, range))
			{
				yield return item;
			}
			yield break;
		}
		int minX = -Mathf.RoundToInt(range);
		int maxX = Mathf.RoundToInt(range);
		if (origin.x > destination.x)
		{
			minX += destination.x;
			maxX += origin.x;
		}
		else
		{
			minX += origin.x;
			maxX += destination.x;
		}
		int bottom = 0;
		int mid = 0;
		int top = count;
		int limiter = 0;
		while (top != bottom && limiter++ < 20)
		{
			mid = (top + bottom) / 2;
			int midX = sortedThings[mid].thing.Position.x;
			if (midX > minX && midX < maxX)
			{
				break;
			}
			if (midX > maxX)
			{
				top = mid - 1;
				continue;
			}
			if (midX < minX)
			{
				bottom = mid + 1;
				continue;
			}
			break;
		}
		float length = Mathf.Sqrt(lengthSq);
		float rlength = length + range;
		float rangeSq = range * range;
		float rangeSqLengthSq = rlength * rlength;
		int index = mid;
		while (index < count)
		{
			Thing t = sortedThings[index++].thing;
			IntVec3 curPosition = t.Position;
			if (curPosition.x < minX || curPosition.x > maxX)
			{
				break;
			}
			IntVec3 relativePosition = curPosition - origin;
			float rp_direction_dot = (float)(relativePosition.x * direction.x + relativePosition.z * direction.z) / length;
			if (rp_direction_dot < 0f)
			{
				if (behind && (float)relativePosition.LengthHorizontalSquared <= rangeSq)
				{
					yield return t;
				}
				continue;
			}
			if (rp_direction_dot > lengthSq)
			{
				if (infront && (float)destination.DistanceToSquared(curPosition) <= rangeSq)
				{
					yield return t;
				}
				continue;
			}
			float rp_cwdirection_dot = relativePosition.x * direction.z - relativePosition.z * direction.x;
			if (rp_cwdirection_dot * rp_cwdirection_dot <= rangeSqLengthSq)
			{
				yield return t;
			}
		}
		index = mid - 1;
		while (index >= 0)
		{
			Thing t2 = sortedThings[index--].thing;
			IntVec3 curPosition2 = t2.Position;
			if (curPosition2.x < minX || curPosition2.x > maxX)
			{
				break;
			}
			IntVec3 relativePosition2 = curPosition2 - origin;
			float rp_direction_dot2 = relativePosition2.x * direction.x + relativePosition2.z * direction.z;
			if (rp_direction_dot2 < 0f)
			{
				if (behind && (float)relativePosition2.LengthHorizontalSquared <= rangeSq)
				{
					yield return t2;
				}
				continue;
			}
			if (rp_direction_dot2 > lengthSq)
			{
				if (infront && (float)destination.DistanceToSquared(curPosition2) <= rangeSq)
				{
					yield return t2;
				}
				continue;
			}
			float rp_cwdirection_dot2 = relativePosition2.x * direction.z - relativePosition2.z * direction.x;
			if (rp_cwdirection_dot2 * rp_cwdirection_dot2 <= rangeSqLengthSq)
			{
				yield return t2;
			}
		}
	}

	public IEnumerable<Thing> ThingsInRangeOf(IntVec3 cell, float range)
	{
		float rangeSq = range * range;
		int bottom = 0;
		int top = count;
		int mid = (top + bottom) / 2;
		int limiter = 0;
		while (top != bottom && limiter++ < 20)
		{
			mid = (top + bottom) / 2;
			IntVec3 midPosition = sortedThings[mid].thing.Position;
			if ((float)midPosition.DistanceToSquared(cell) <= rangeSq)
			{
				break;
			}
			if (midPosition.x > cell.x)
			{
				top = mid - 1;
				continue;
			}
			if (midPosition.x < cell.x)
			{
				bottom = mid + 1;
				continue;
			}
			break;
		}
		int index = mid;
		while (index < count)
		{
			Thing t = sortedThings[index++].thing;
			IntVec3 curPosition = t.Position;
			if ((float)Mathf.Abs(curPosition.x - cell.x) > range)
			{
				break;
			}
			if ((float)curPosition.DistanceToSquared(cell) <= rangeSq)
			{
				yield return t;
			}
		}
		index = mid - 1;
		while (index >= 0)
		{
			Thing t2 = sortedThings[index--].thing;
			IntVec3 curPosition2 = t2.Position;
			if ((float)Mathf.Abs(curPosition2.x - cell.x) > range)
			{
				break;
			}
			if ((float)curPosition2.DistanceToSquared(cell) <= rangeSq)
			{
				yield return t2;
			}
		}
	}

	private void RemoveClean(int index)
	{
		int num = 1;
		for (int i = index + 1; i < count && i - num >= 0; i++)
		{
			ThingPositionInfo thingPositionInfo = sortedThings[i];
			if (!thingPositionInfo.IsValid)
			{
				num++;
				if (indexByThing.ContainsKey(thingPositionInfo.thing))
				{
					indexByThing.Remove(thingPositionInfo.thing);
				}
				thingPositionInfo.thing = null;
			}
			else if (num > 0)
			{
				indexByThing[thingPositionInfo.thing] = i - num;
				sortedThings[i - num] = thingPositionInfo;
			}
		}
		for (int i = count - num; i < count; i++)
		{
			sortedThings[i].thing = null;
		}
		count -= num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Swap<T>(int a, int b, T[] list)
	{
		T val = list[a];
		list[a] = list[b];
		list[b] = val;
	}
}

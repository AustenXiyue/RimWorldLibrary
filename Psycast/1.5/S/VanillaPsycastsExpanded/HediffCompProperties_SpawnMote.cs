using UnityEngine;
using Verse;

namespace VanillaPsycastsExpanded;

public class HediffCompProperties_SpawnMote : HediffCompProperties
{
	public ThingDef moteDef;

	public Vector3 offset;

	public float maxScale;

	public HediffCompProperties_SpawnMote()
	{
		compClass = typeof(HediffComp_SpawnMote);
	}
}

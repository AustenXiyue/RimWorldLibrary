using System.Collections.Generic;
using Verse;

namespace AncotLibrary;

public class CompProperties_TurretGun_Custom : CompProperties
{
	public ThingDef turretDef;

	public float angleOffset;

	public bool autoAttack = true;

	public bool attackUndrafted = true;

	public FloatGraph float_yAxis;

	public FloatGraph float_xAxis;

	public List<PawnRenderNodeProperties> renderNodeProperties;

	public CompProperties_TurretGun_Custom()
	{
		compClass = typeof(CompTurretGun_Custom);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (renderNodeProperties.NullOrEmpty())
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties renderNodeProperty in renderNodeProperties)
		{
			if (!typeof(PawnRenderNode_TurretGun_Custom).IsAssignableFrom(renderNodeProperty.nodeClass))
			{
				yield return "contains nodeClass which is not PawnRenderNode_FloatTurret or subclass thereof.";
			}
		}
	}
}

using AncotLibrary;
using UnityEngine;
using Verse;

namespace Milira;

[StaticConstructorOnStartup]
public class CompBroadShieldUnit : CompAdditionalGraphic
{
	private static readonly Material LightEffect = MaterialPool.MatFrom("Milian/Apparel/FloatingSystem/CombatDrone_ShieldUnit/ShieldUnit_WorkingLight", ShaderDatabase.MoteGlow, Color.white);

	private bool isFlashing = false;

	private int flashTimer = 0;

	private int flashDuration = Rand.Range(1, 3);

	private CompProperties_AdditionalGraphic Props => (CompProperties_AdditionalGraphic)props;

	public override void PostDraw()
	{
		base.PostDraw();
		if (base.drawAdditionalGraphic || drawGraphic)
		{
			Mesh mesh = Props.graphicData.Graphic.MeshAt(parent.Rotation);
			Vector3 drawPos = parent.DrawPos;
			drawPos.z = parent.DrawPos.z + floatOffset;
			drawPos.y = Props.altitudeLayer.AltitudeFor();
			Color color = LightEffect.color;
			float a = (isFlashing ? 0.5f : 1f);
			LightEffect.color = new Color(color.r, color.g, color.b, a);
			Graphics.DrawMesh(mesh, drawPos + Props.graphicData.drawOffset.RotatedBy(parent.Rotation), Quaternion.identity, LightEffect, 0);
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		flashTimer++;
		if (flashTimer >= flashDuration)
		{
			isFlashing = false;
			flashTimer = 0;
		}
		if (!isFlashing && Rand.Chance(0.01f))
		{
			isFlashing = true;
		}
	}
}

using UnityEngine;
using Verse;

namespace CombatExtended;

public class MoteThrownCE : MoteThrown
{
	public Thing attachedAltitudeThing = null;

	public Vector3 drawOffset;

	private Graphic_Mote _moteGraphic = null;

	private Graphic_Mote MoteGraphic
	{
		get
		{
			if (_moteGraphic == null)
			{
				_moteGraphic = (Graphic_Mote)Graphic;
			}
			return _moteGraphic;
		}
	}

	public override Vector3 DrawPos
	{
		get
		{
			Vector3 drawPos = base.DrawPos;
			Thing thing = attachedAltitudeThing;
			if (thing != null && thing.Spawned)
			{
				drawPos.y = attachedAltitudeThing.DrawPos.y;
			}
			return drawPos + drawOffset;
		}
	}

	public float CurAltitude
	{
		get
		{
			Thing thing = attachedAltitudeThing;
			if (thing != null && thing.Spawned)
			{
				return attachedAltitudeThing.DrawPos.y;
			}
			return exactPosition.y;
		}
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		float alpha = Alpha;
		if (!(alpha <= 0f))
		{
			Color color = MoteGraphic.Color * instanceColor;
			color.a *= alpha;
			Vector3 exactScale = base.ExactScale;
			exactScale.x *= MoteGraphic.data.drawSize.x;
			exactScale.z *= MoteGraphic.data.drawSize.y;
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(DrawPos, Quaternion.AngleAxis(exactRotation, Vector3.up), exactScale);
			Material matSingle = Graphic.MatSingle;
			if (!MoteGraphic.ForcePropertyBlock && color.IndistinguishableFrom(matSingle.color))
			{
				Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, 0, null, 0);
				return;
			}
			Graphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
			Graphics.DrawMesh(MeshPool.plane10, matrix, matSingle, 0, null, 0, Graphic_Mote.propertyBlock);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref attachedAltitudeThing, "attachedAltitudeThing");
	}
}

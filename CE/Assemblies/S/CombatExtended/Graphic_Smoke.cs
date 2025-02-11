using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Graphic_Smoke : Graphic_Gas
{
	private const float DistinctAlphaLevels = 128f;

	public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Rand.PushState();
		Rand.Seed = thing.thingIDNumber.GetHashCode();
		Smoke smoke = thing as Smoke;
		float num = Mathf.Round(smoke.GetOpacity() * 128f) / 128f;
		Material material = MaterialPool.MatFrom(new MaterialRequest(color: new Color(color.r, color.g, color.r, color.a * num), tex: (Texture2D)MatSingle.mainTexture, shader: MatSingle.shader));
		float angle = (float)Rand.Range(0, 360) + smoke.graphicRotation;
		Vector3 pos = thing.TrueCenter() + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.45f, 0.45f));
		Vector3 s = new Vector3(Rand.Range(0.8f, 1.2f) * drawSize.x, 0f, Rand.Range(0.8f, 1.2f) * drawSize.y);
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
		Rand.PopState();
	}
}

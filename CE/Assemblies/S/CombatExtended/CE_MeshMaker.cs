using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public static class CE_MeshMaker
{
	public const float DEPTH_TOP = -0f;

	public const float DEPTH_MID = -0.015f;

	public const float DEPTH_BOT = -0.03f;

	public static readonly Mesh plane10Top;

	public static readonly Mesh plane10Mid;

	public static readonly Mesh plane10Bot;

	public static readonly Mesh plane10FlipTop;

	public static readonly Mesh plane10FlipMid;

	public static readonly Mesh plane10FlipBot;

	static CE_MeshMaker()
	{
		plane10Top = NewPlaneMesh(Vector2.zero, Vector2.one);
		plane10Mid = NewPlaneMesh(Vector2.zero, Vector2.one, -0.015f);
		plane10Bot = NewPlaneMesh(Vector2.zero, Vector2.one, -0.03f);
		plane10FlipTop = NewPlaneMesh(Vector2.zero, Vector2.one, -0f, flipped: true);
		plane10FlipMid = NewPlaneMesh(Vector2.zero, Vector2.one, -0.015f, flipped: true);
		plane10FlipBot = NewPlaneMesh(Vector2.zero, Vector2.one, -0.03f, flipped: true);
	}

	public static Mesh NewPlaneMesh(Vector2 offset, Vector2 scale, float depth = 0f, bool flipped = false)
	{
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		if (flipped)
		{
			array[0] = new Vector3(-0.5f + offset.x, depth, -0.5f + offset.y);
			array[1] = array[0] + new Vector3(0f, 0f, scale.y);
			array[2] = array[0] + new Vector3(scale.x, 0f, scale.y);
			array[3] = array[0] + new Vector3(scale.x, 0f, 0f);
		}
		else
		{
			array[0] = new Vector3(0.5f - offset.x - scale.x, depth, -0.5f + offset.y);
			array[1] = array[0] + new Vector3(0f, 0f, scale.y);
			array[2] = array[0] + new Vector3(scale.x, 0f, scale.y);
			array[3] = array[0] + new Vector3(scale.x, 0f, 0f);
		}
		if (!flipped)
		{
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 1f);
			array2[3] = new Vector2(1f, 0f);
		}
		else
		{
			array2[0] = new Vector2(1f, 0f);
			array2[1] = new Vector2(1f, 1f);
			array2[2] = new Vector2(0f, 1f);
			array2[3] = new Vector2(0f, 0f);
		}
		array3[0] = 0;
		array3[1] = 1;
		array3[2] = 2;
		array3[3] = 0;
		array3[4] = 2;
		array3[5] = 3;
		Mesh mesh = new Mesh();
		mesh.name = "NewPlaneMesh()";
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.SetTriangles(array3, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}
}

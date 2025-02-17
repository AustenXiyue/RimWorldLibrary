using System;
using UnityEngine.Scripting;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.AI;

[UsedByNativeCode]
[MovedFrom("UnityEngine")]
public struct NavMeshTriangulation
{
	public Vector3[] vertices;

	public int[] indices;

	public int[] areas;

	[Obsolete("Use areas instead.")]
	public int[] layers => areas;
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeClass("UI::CanvasRenderer")]
[NativeHeader("Modules/UI/CanvasRenderer.h")]
public sealed class CanvasRenderer : Component
{
	public extern bool hasPopInstruction
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int materialCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int popMaterialCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int absoluteDepth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool hasMoved
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool cullTransparentMesh
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("RectClipping", false, TargetType.Function)]
	public extern bool hasRectClipping
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeProperty("Depth", false, TargetType.Function)]
	public extern int relativeDepth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeProperty("ShouldCull", false, TargetType.Function)]
	public extern bool cull
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[Obsolete("isMask is no longer supported.See EnableClipping for vertex clipping configuration", false)]
	public bool isMask { get; set; }

	public Vector2 clippingSoftness
	{
		get
		{
			get_clippingSoftness_Injected(out var ret);
			return ret;
		}
		set
		{
			set_clippingSoftness_Injected(ref value);
		}
	}

	public void SetColor(Color color)
	{
		SetColor_Injected(ref color);
	}

	public Color GetColor()
	{
		GetColor_Injected(out var ret);
		return ret;
	}

	public void EnableRectClipping(Rect rect)
	{
		EnableRectClipping_Injected(ref rect);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void DisableRectClipping();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetMaterial(Material material, int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern Material GetMaterial(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetPopMaterial(Material material, int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern Material GetPopMaterial(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetTexture(Texture texture);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetAlphaTexture(Texture texture);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetMesh(Mesh mesh);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Clear();

	public float GetAlpha()
	{
		return GetColor().a;
	}

	public void SetAlpha(float alpha)
	{
		Color color = GetColor();
		color.a = alpha;
		SetColor(color);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern float GetInheritedAlpha();

	public void SetMaterial(Material material, Texture texture)
	{
		materialCount = Math.Max(1, materialCount);
		SetMaterial(material, 0);
		SetTexture(texture);
	}

	public Material GetMaterial()
	{
		return GetMaterial(0);
	}

	public static void SplitUIVertexStreams(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector3> normals, List<Vector4> tangents, List<int> indices)
	{
		SplitUIVertexStreams(verts, positions, colors, uv0S, uv1S, new List<Vector2>(), new List<Vector2>(), normals, tangents, indices);
	}

	public static void SplitUIVertexStreams(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector2> uv2S, List<Vector2> uv3S, List<Vector3> normals, List<Vector4> tangents, List<int> indices)
	{
		SplitUIVertexStreamsInternal(verts, positions, colors, uv0S, uv1S, uv2S, uv3S, normals, tangents);
		SplitIndicesStreamsInternal(verts, indices);
	}

	public static void CreateUIVertexStream(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector3> normals, List<Vector4> tangents, List<int> indices)
	{
		CreateUIVertexStream(verts, positions, colors, uv0S, uv1S, new List<Vector2>(), new List<Vector2>(), normals, tangents, indices);
	}

	public static void CreateUIVertexStream(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector2> uv2S, List<Vector2> uv3S, List<Vector3> normals, List<Vector4> tangents, List<int> indices)
	{
		CreateUIVertexStreamInternal(verts, positions, colors, uv0S, uv1S, uv2S, uv3S, normals, tangents, indices);
	}

	public static void AddUIVertexStream(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector3> normals, List<Vector4> tangents)
	{
		AddUIVertexStream(verts, positions, colors, uv0S, uv1S, new List<Vector2>(), new List<Vector2>(), normals, tangents);
	}

	public static void AddUIVertexStream(List<UIVertex> verts, List<Vector3> positions, List<Color32> colors, List<Vector2> uv0S, List<Vector2> uv1S, List<Vector2> uv2S, List<Vector2> uv3S, List<Vector3> normals, List<Vector4> tangents)
	{
		SplitUIVertexStreamsInternal(verts, positions, colors, uv0S, uv1S, uv2S, uv3S, normals, tangents);
	}

	[Obsolete("UI System now uses meshes.Generate a mesh and use 'SetMesh' instead", false)]
	public void SetVertices(List<UIVertex> vertices)
	{
		SetVertices(vertices.ToArray(), vertices.Count);
	}

	[Obsolete("UI System now uses meshes.Generate a mesh and use 'SetMesh' instead", false)]
	public void SetVertices(UIVertex[] vertices, int size)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<Color32> list2 = new List<Color32>();
		List<Vector2> list3 = new List<Vector2>();
		List<Vector2> list4 = new List<Vector2>();
		List<Vector2> list5 = new List<Vector2>();
		List<Vector2> list6 = new List<Vector2>();
		List<Vector3> list7 = new List<Vector3>();
		List<Vector4> list8 = new List<Vector4>();
		List<int> list9 = new List<int>();
		for (int i = 0; i < size; i += 4)
		{
			for (int j = 0; j < 4; j++)
			{
				list.Add(vertices[i + j].position);
				list2.Add(vertices[i + j].color);
				list3.Add(vertices[i + j].uv0);
				list4.Add(vertices[i + j].uv1);
				list5.Add(vertices[i + j].uv2);
				list6.Add(vertices[i + j].uv3);
				list7.Add(vertices[i + j].normal);
				list8.Add(vertices[i + j].tangent);
			}
			list9.Add(i);
			list9.Add(i + 1);
			list9.Add(i + 2);
			list9.Add(i + 2);
			list9.Add(i + 3);
			list9.Add(i);
		}
		mesh.SetVertices(list);
		mesh.SetColors(list2);
		mesh.SetNormals(list7);
		mesh.SetTangents(list8);
		mesh.SetUVs(0, list3);
		mesh.SetUVs(1, list4);
		mesh.SetUVs(2, list5);
		mesh.SetUVs(3, list6);
		mesh.SetIndices(list9.ToArray(), MeshTopology.Triangles, 0);
		SetMesh(mesh);
		Object.DestroyImmediate(mesh);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("UI", StaticAccessorType.DoubleColon)]
	private static extern void SplitIndicesStreamsInternal(object verts, object indices);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("UI", StaticAccessorType.DoubleColon)]
	private static extern void SplitUIVertexStreamsInternal(object verts, object positions, object colors, object uv0S, object uv1S, object uv2S, object uv3S, object normals, object tangents);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("UI", StaticAccessorType.DoubleColon)]
	private static extern void CreateUIVertexStreamInternal(object verts, object positions, object colors, object uv0S, object uv1S, object uv2S, object uv3S, object normals, object tangents, object indices);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetColor_Injected(ref Color color);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetColor_Injected(out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void EnableRectClipping_Injected(ref Rect rect);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_clippingSoftness_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_clippingSoftness_Injected(ref Vector2 value);
}

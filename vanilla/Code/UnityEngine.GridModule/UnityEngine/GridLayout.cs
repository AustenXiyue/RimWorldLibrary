using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeType(Header = "Modules/Grid/Public/Grid.h")]
[NativeHeader("Modules/Grid/Public/GridMarshalling.h")]
[RequireComponent(typeof(Transform))]
public class GridLayout : Behaviour
{
	public enum CellLayout
	{
		Rectangle,
		Hexagon,
		Isometric,
		IsometricZAsY
	}

	public enum CellSwizzle
	{
		XYZ,
		XZY,
		YXZ,
		YZX,
		ZXY,
		ZYX
	}

	public Vector3 cellSize
	{
		[FreeFunction("GridLayoutBindings::GetCellSize", HasExplicitThis = true)]
		get
		{
			get_cellSize_Injected(out var ret);
			return ret;
		}
	}

	public Vector3 cellGap
	{
		[FreeFunction("GridLayoutBindings::GetCellGap", HasExplicitThis = true)]
		get
		{
			get_cellGap_Injected(out var ret);
			return ret;
		}
	}

	public extern CellLayout cellLayout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern CellSwizzle cellSwizzle
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[FreeFunction("GridLayoutBindings::GetBoundsLocal", HasExplicitThis = true)]
	public Bounds GetBoundsLocal(Vector3Int cellPosition)
	{
		GetBoundsLocal_Injected(ref cellPosition, out var ret);
		return ret;
	}

	public Bounds GetBoundsLocal(Vector3 origin, Vector3 size)
	{
		return GetBoundsLocalOriginSize(origin, size);
	}

	[FreeFunction("GridLayoutBindings::GetBoundsLocalOriginSize", HasExplicitThis = true)]
	private Bounds GetBoundsLocalOriginSize(Vector3 origin, Vector3 size)
	{
		GetBoundsLocalOriginSize_Injected(ref origin, ref size, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::CellToLocal", HasExplicitThis = true)]
	public Vector3 CellToLocal(Vector3Int cellPosition)
	{
		CellToLocal_Injected(ref cellPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::LocalToCell", HasExplicitThis = true)]
	public Vector3Int LocalToCell(Vector3 localPosition)
	{
		LocalToCell_Injected(ref localPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::CellToLocalInterpolated", HasExplicitThis = true)]
	public Vector3 CellToLocalInterpolated(Vector3 cellPosition)
	{
		CellToLocalInterpolated_Injected(ref cellPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::LocalToCellInterpolated", HasExplicitThis = true)]
	public Vector3 LocalToCellInterpolated(Vector3 localPosition)
	{
		LocalToCellInterpolated_Injected(ref localPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::CellToWorld", HasExplicitThis = true)]
	public Vector3 CellToWorld(Vector3Int cellPosition)
	{
		CellToWorld_Injected(ref cellPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::WorldToCell", HasExplicitThis = true)]
	public Vector3Int WorldToCell(Vector3 worldPosition)
	{
		WorldToCell_Injected(ref worldPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::LocalToWorld", HasExplicitThis = true)]
	public Vector3 LocalToWorld(Vector3 localPosition)
	{
		LocalToWorld_Injected(ref localPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::WorldToLocal", HasExplicitThis = true)]
	public Vector3 WorldToLocal(Vector3 worldPosition)
	{
		WorldToLocal_Injected(ref worldPosition, out var ret);
		return ret;
	}

	[FreeFunction("GridLayoutBindings::GetLayoutCellCenter", HasExplicitThis = true)]
	public Vector3 GetLayoutCellCenter()
	{
		GetLayoutCellCenter_Injected(out var ret);
		return ret;
	}

	[RequiredByNativeCode]
	private void DoNothing()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_cellSize_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_cellGap_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetBoundsLocal_Injected(ref Vector3Int cellPosition, out Bounds ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetBoundsLocalOriginSize_Injected(ref Vector3 origin, ref Vector3 size, out Bounds ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void CellToLocal_Injected(ref Vector3Int cellPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void LocalToCell_Injected(ref Vector3 localPosition, out Vector3Int ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void CellToLocalInterpolated_Injected(ref Vector3 cellPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void LocalToCellInterpolated_Injected(ref Vector3 localPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void CellToWorld_Injected(ref Vector3Int cellPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void WorldToCell_Injected(ref Vector3 worldPosition, out Vector3Int ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void LocalToWorld_Injected(ref Vector3 localPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void WorldToLocal_Injected(ref Vector3 worldPosition, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetLayoutCellCenter_Injected(out Vector3 ret);
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Unity.Mathematics;

[Serializable]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
public struct uint4 : IEquatable<uint4>, IFormattable
{
	internal sealed class DebuggerProxy
	{
		public uint x;

		public uint y;

		public uint z;

		public uint w;

		public DebuggerProxy(uint4 v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}
	}

	public uint x;

	public uint y;

	public uint z;

	public uint w;

	public static readonly uint4 zero;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xxww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, x, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, z, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
			z = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xywx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xywy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xywz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, w, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
			w = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xyww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, y, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, y, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			z = value.y;
			y = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, w, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			z = value.y;
			w = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xzww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, z, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, y, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			w = value.y;
			y = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, z, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			w = value.y;
			z = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 xwww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(x, w, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, z, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			x = value.y;
			z = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, w, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			x = value.y;
			w = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yxww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, x, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yywx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yywy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yywz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yyww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, y, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, x, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			z = value.y;
			x = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, w, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			z = value.y;
			w = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 yzww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, z, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, x, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			w = value.y;
			x = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, z, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			w = value.y;
			z = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 ywww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(y, w, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, y, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			x = value.y;
			y = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, w, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			x = value.y;
			w = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zxww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, x, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, x, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			y = value.y;
			x = value.z;
			w = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zywx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, w, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			y = value.y;
			w = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zywy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zywz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zyww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, y, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zzww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, z, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			w = value.y;
			x = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, y, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			w = value.y;
			y = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 zwww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(z, w, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, y, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			x = value.y;
			y = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, z, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			x = value.y;
			z = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wxww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, x, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, x, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			y = value.y;
			x = value.z;
			z = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, z, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			y = value.y;
			z = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wywx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wywy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wywz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wyww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, y, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			z = value.y;
			x = value.z;
			y = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, y, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			z = value.y;
			y = value.z;
			x = value.w;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wzww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, z, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint4 wwww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint4(w, w, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, y, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, y, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, z, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			z = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, z, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			z = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, w, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			w = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, w, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			w = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 xww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(x, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, x, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, x, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			x = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, x, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			x = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, y, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, z, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			z = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, z, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			z = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 ywx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, w, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			w = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 ywy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 ywz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, w, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			w = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 yww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(y, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			x = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, x, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, x, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			x = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, y, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			y = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, y, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, y, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			y = value.y;
			w = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, z, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, z, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, w, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			w = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, w, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			w = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 zww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(z, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wxx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wxy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			x = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wxz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, x, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			x = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wxw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, x, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wyx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, y, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			y = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wyy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wyz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, y, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			y = value.y;
			z = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wyw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, y, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wzx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, z, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			z = value.y;
			x = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wzy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, z, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			z = value.y;
			y = value.z;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wzz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wzw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, z, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wwx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, w, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wwy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, w, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 wwz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, w, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint3 www
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint3(w, w, w);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 xx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(x, x);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 xy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 xz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(x, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			z = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 xw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(x, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			x = value.x;
			w = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 yx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(y, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			x = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 yy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(y, y);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 yz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(y, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			z = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 yw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(y, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			y = value.x;
			w = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 zx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(z, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			x = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 zy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(z, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			y = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 zz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(z, z);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 zw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(z, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			z = value.x;
			w = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 wx
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(w, x);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			x = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 wy
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(w, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			y = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 wz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(w, z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			w = value.x;
			z = value.y;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public uint2 ww
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new uint2(w, w);
		}
	}

	public unsafe uint this[int index]
	{
		get
		{
			fixed (uint4* ptr = &this)
			{
				return *(uint*)((byte*)ptr + (nint)index * (nint)4);
			}
		}
		set
		{
			fixed (uint* ptr = &x)
			{
				ptr[index] = value;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint x, uint y, uint z, uint w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint x, uint y, uint2 zw)
	{
		this.x = x;
		this.y = y;
		z = zw.x;
		w = zw.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint x, uint2 yz, uint w)
	{
		this.x = x;
		y = yz.x;
		z = yz.y;
		this.w = w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint x, uint3 yzw)
	{
		this.x = x;
		y = yzw.x;
		z = yzw.y;
		w = yzw.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint2 xy, uint z, uint w)
	{
		x = xy.x;
		y = xy.y;
		this.z = z;
		this.w = w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint2 xy, uint2 zw)
	{
		x = xy.x;
		y = xy.y;
		z = zw.x;
		w = zw.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint3 xyz, uint w)
	{
		x = xyz.x;
		y = xyz.y;
		z = xyz.z;
		this.w = w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint4 xyzw)
	{
		x = xyzw.x;
		y = xyzw.y;
		z = xyzw.z;
		w = xyzw.w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(uint v)
	{
		x = v;
		y = v;
		z = v;
		w = v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(bool v)
	{
		x = (v ? 1u : 0u);
		y = (v ? 1u : 0u);
		z = (v ? 1u : 0u);
		w = (v ? 1u : 0u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(bool4 v)
	{
		x = (v.x ? 1u : 0u);
		y = (v.y ? 1u : 0u);
		z = (v.z ? 1u : 0u);
		w = (v.w ? 1u : 0u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(int v)
	{
		x = (uint)v;
		y = (uint)v;
		z = (uint)v;
		w = (uint)v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(int4 v)
	{
		x = (uint)v.x;
		y = (uint)v.y;
		z = (uint)v.z;
		w = (uint)v.w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(float v)
	{
		x = (uint)v;
		y = (uint)v;
		z = (uint)v;
		w = (uint)v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(float4 v)
	{
		x = (uint)v.x;
		y = (uint)v.y;
		z = (uint)v.z;
		w = (uint)v.w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(double v)
	{
		x = (uint)v;
		y = (uint)v;
		z = (uint)v;
		w = (uint)v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint4(double4 v)
	{
		x = (uint)v.x;
		y = (uint)v.y;
		z = (uint)v.z;
		w = (uint)v.w;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator uint4(uint v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(bool v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(bool4 v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(int v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(int4 v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(float v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(float4 v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(double v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator uint4(double4 v)
	{
		return new uint4(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator *(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z, lhs.w * rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator *(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator *(uint lhs, uint4 rhs)
	{
		return new uint4(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z, lhs * rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator +(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator +(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs, lhs.w + rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator +(uint lhs, uint4 rhs)
	{
		return new uint4(lhs + rhs.x, lhs + rhs.y, lhs + rhs.z, lhs + rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator -(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator -(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs, lhs.w - rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator -(uint lhs, uint4 rhs)
	{
		return new uint4(lhs - rhs.x, lhs - rhs.y, lhs - rhs.z, lhs - rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator /(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z, lhs.w / rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator /(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator /(uint lhs, uint4 rhs)
	{
		return new uint4(lhs / rhs.x, lhs / rhs.y, lhs / rhs.z, lhs / rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator %(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x % rhs.x, lhs.y % rhs.y, lhs.z % rhs.z, lhs.w % rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator %(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x % rhs, lhs.y % rhs, lhs.z % rhs, lhs.w % rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator %(uint lhs, uint4 rhs)
	{
		return new uint4(lhs % rhs.x, lhs % rhs.y, lhs % rhs.z, lhs % rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator ++(uint4 val)
	{
		return new uint4(++val.x, ++val.y, ++val.z, ++val.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator --(uint4 val)
	{
		return new uint4(--val.x, --val.y, --val.z, --val.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x < rhs.x, lhs.y < rhs.y, lhs.z < rhs.z, lhs.w < rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x < rhs, lhs.y < rhs, lhs.z < rhs, lhs.w < rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <(uint lhs, uint4 rhs)
	{
		return new bool4(lhs < rhs.x, lhs < rhs.y, lhs < rhs.z, lhs < rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <=(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x <= rhs.x, lhs.y <= rhs.y, lhs.z <= rhs.z, lhs.w <= rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <=(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x <= rhs, lhs.y <= rhs, lhs.z <= rhs, lhs.w <= rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator <=(uint lhs, uint4 rhs)
	{
		return new bool4(lhs <= rhs.x, lhs <= rhs.y, lhs <= rhs.z, lhs <= rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x > rhs.x, lhs.y > rhs.y, lhs.z > rhs.z, lhs.w > rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x > rhs, lhs.y > rhs, lhs.z > rhs, lhs.w > rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >(uint lhs, uint4 rhs)
	{
		return new bool4(lhs > rhs.x, lhs > rhs.y, lhs > rhs.z, lhs > rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >=(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x >= rhs.x, lhs.y >= rhs.y, lhs.z >= rhs.z, lhs.w >= rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >=(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x >= rhs, lhs.y >= rhs, lhs.z >= rhs, lhs.w >= rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator >=(uint lhs, uint4 rhs)
	{
		return new bool4(lhs >= rhs.x, lhs >= rhs.y, lhs >= rhs.z, lhs >= rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator -(uint4 val)
	{
		return new uint4((uint)(0uL - (ulong)val.x), (uint)(0uL - (ulong)val.y), (uint)(0uL - (ulong)val.z), (uint)(0uL - (ulong)val.w));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator +(uint4 val)
	{
		return new uint4(val.x, val.y, val.z, val.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator <<(uint4 x, int n)
	{
		return new uint4(x.x << n, x.y << n, x.z << n, x.w << n);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator >>(uint4 x, int n)
	{
		return new uint4(x.x >> n, x.y >> n, x.z >> n, x.w >> n);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator ==(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x == rhs.x, lhs.y == rhs.y, lhs.z == rhs.z, lhs.w == rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator ==(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x == rhs, lhs.y == rhs, lhs.z == rhs, lhs.w == rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator ==(uint lhs, uint4 rhs)
	{
		return new bool4(lhs == rhs.x, lhs == rhs.y, lhs == rhs.z, lhs == rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator !=(uint4 lhs, uint4 rhs)
	{
		return new bool4(lhs.x != rhs.x, lhs.y != rhs.y, lhs.z != rhs.z, lhs.w != rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator !=(uint4 lhs, uint rhs)
	{
		return new bool4(lhs.x != rhs, lhs.y != rhs, lhs.z != rhs, lhs.w != rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool4 operator !=(uint lhs, uint4 rhs)
	{
		return new bool4(lhs != rhs.x, lhs != rhs.y, lhs != rhs.z, lhs != rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator ~(uint4 val)
	{
		return new uint4(~val.x, ~val.y, ~val.z, ~val.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator &(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x & rhs.x, lhs.y & rhs.y, lhs.z & rhs.z, lhs.w & rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator &(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x & rhs, lhs.y & rhs, lhs.z & rhs, lhs.w & rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator &(uint lhs, uint4 rhs)
	{
		return new uint4(lhs & rhs.x, lhs & rhs.y, lhs & rhs.z, lhs & rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator |(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x | rhs.x, lhs.y | rhs.y, lhs.z | rhs.z, lhs.w | rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator |(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x | rhs, lhs.y | rhs, lhs.z | rhs, lhs.w | rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator |(uint lhs, uint4 rhs)
	{
		return new uint4(lhs | rhs.x, lhs | rhs.y, lhs | rhs.z, lhs | rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator ^(uint4 lhs, uint4 rhs)
	{
		return new uint4(lhs.x ^ rhs.x, lhs.y ^ rhs.y, lhs.z ^ rhs.z, lhs.w ^ rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator ^(uint4 lhs, uint rhs)
	{
		return new uint4(lhs.x ^ rhs, lhs.y ^ rhs, lhs.z ^ rhs, lhs.w ^ rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 operator ^(uint lhs, uint4 rhs)
	{
		return new uint4(lhs ^ rhs.x, lhs ^ rhs.y, lhs ^ rhs.z, lhs ^ rhs.w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(uint4 rhs)
	{
		if (x == rhs.x && y == rhs.y && z == rhs.z)
		{
			return w == rhs.w;
		}
		return false;
	}

	public override bool Equals(object o)
	{
		return Equals((uint4)o);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return (int)math.hash(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return $"uint4({x}, {y}, {z}, {w})";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format, IFormatProvider formatProvider)
	{
		return $"uint4({x.ToString(format, formatProvider)}, {y.ToString(format, formatProvider)}, {z.ToString(format, formatProvider)}, {w.ToString(format, formatProvider)})";
	}
}

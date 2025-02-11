using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct D3DMATRIX
{
	[FieldOffset(0)]
	internal float _11;

	[FieldOffset(4)]
	internal float _12;

	[FieldOffset(8)]
	internal float _13;

	[FieldOffset(12)]
	internal float _14;

	[FieldOffset(16)]
	internal float _21;

	[FieldOffset(20)]
	internal float _22;

	[FieldOffset(24)]
	internal float _23;

	[FieldOffset(28)]
	internal float _24;

	[FieldOffset(32)]
	internal float _31;

	[FieldOffset(36)]
	internal float _32;

	[FieldOffset(40)]
	internal float _33;

	[FieldOffset(44)]
	internal float _34;

	[FieldOffset(48)]
	internal float _41;

	[FieldOffset(52)]
	internal float _42;

	[FieldOffset(56)]
	internal float _43;

	[FieldOffset(60)]
	internal float _44;

	internal D3DMATRIX(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
	{
		_11 = m11;
		_12 = m12;
		_13 = m13;
		_14 = m14;
		_21 = m21;
		_22 = m22;
		_23 = m23;
		_24 = m24;
		_31 = m31;
		_32 = m32;
		_33 = m33;
		_34 = m34;
		_41 = m41;
		_42 = m42;
		_43 = m43;
		_44 = m44;
	}
}

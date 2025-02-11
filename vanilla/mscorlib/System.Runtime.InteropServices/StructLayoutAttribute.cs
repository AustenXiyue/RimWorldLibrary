using System.Reflection;
using System.Security;

namespace System.Runtime.InteropServices;

/// <summary>Lets you control the physical layout of the data fields of a class or structure.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
[ComVisible(true)]
public sealed class StructLayoutAttribute : Attribute
{
	private const int DEFAULT_PACKING_SIZE = 8;

	internal LayoutKind _val;

	/// <summary>Controls the alignment of data fields of a class or structure in memory.</summary>
	public int Pack;

	/// <summary>Indicates the absolute size of the class or structure.</summary>
	public int Size;

	/// <summary>Indicates whether string data fields within the class should be marshaled as LPWSTR or LPSTR by default.</summary>
	public CharSet CharSet;

	/// <summary>Gets the <see cref="T:System.Runtime.InteropServices.LayoutKind" /> value that specifies how the class or structure is arranged.</summary>
	/// <returns>One of the enumeration values that specifies how the class or structure is arranged.</returns>
	public LayoutKind Value => _val;

	[SecurityCritical]
	internal static StructLayoutAttribute GetCustomAttribute(RuntimeType type)
	{
		if (!IsDefined(type))
		{
			return null;
		}
		int packing = 0;
		int size = 0;
		LayoutKind layoutKind = LayoutKind.Auto;
		switch (type.Attributes & TypeAttributes.LayoutMask)
		{
		case TypeAttributes.ExplicitLayout:
			layoutKind = LayoutKind.Explicit;
			break;
		case TypeAttributes.NotPublic:
			layoutKind = LayoutKind.Auto;
			break;
		case TypeAttributes.SequentialLayout:
			layoutKind = LayoutKind.Sequential;
			break;
		}
		CharSet charSet = CharSet.None;
		switch (type.Attributes & TypeAttributes.StringFormatMask)
		{
		case TypeAttributes.NotPublic:
			charSet = CharSet.Ansi;
			break;
		case TypeAttributes.AutoClass:
			charSet = CharSet.Auto;
			break;
		case TypeAttributes.UnicodeClass:
			charSet = CharSet.Unicode;
			break;
		}
		type.GetPacking(out packing, out size);
		if (packing == 0)
		{
			packing = 8;
		}
		return new StructLayoutAttribute(layoutKind, packing, size, charSet);
	}

	internal static bool IsDefined(RuntimeType type)
	{
		if (type.IsInterface || type.HasElementType || type.IsGenericParameter)
		{
			return false;
		}
		return true;
	}

	internal StructLayoutAttribute(LayoutKind layoutKind, int pack, int size, CharSet charSet)
	{
		_val = layoutKind;
		Pack = pack;
		Size = size;
		CharSet = charSet;
	}

	/// <summary>Initalizes a new instance of the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.LayoutKind" /> enumeration member.</summary>
	/// <param name="layoutKind">One of the enumeration values that specifes how the class or structure should be arranged. </param>
	public StructLayoutAttribute(LayoutKind layoutKind)
	{
		_val = layoutKind;
	}

	/// <summary>Initalizes a new instance of the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.LayoutKind" /> enumeration member.</summary>
	/// <param name="layoutKind">A 16-bit integer that represents one of the <see cref="T:System.Runtime.InteropServices.LayoutKind" /> values that specifes how the class or structure should be arranged. </param>
	public StructLayoutAttribute(short layoutKind)
	{
		_val = (LayoutKind)layoutKind;
	}
}

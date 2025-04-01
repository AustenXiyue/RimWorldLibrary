namespace System.Runtime.InteropServices;

/// <summary>Indicates how to marshal the data between managed and unmanaged code.</summary>
[StructLayout(LayoutKind.Sequential)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
[ComVisible(true)]
public sealed class MarshalAsAttribute : Attribute
{
	/// <summary>Provides additional information to a custom marshaler.</summary>
	public string MarshalCookie;

	/// <summary>Specifies the fully qualified name of a custom marshaler.</summary>
	[ComVisible(true)]
	public string MarshalType;

	/// <summary>Implements <see cref="F:System.Runtime.InteropServices.MarshalAsAttribute.MarshalType" /> as a type.</summary>
	[ComVisible(true)]
	public Type MarshalTypeRef;

	/// <summary>Indicates the user-defined element type of the <see cref="F:System.Runtime.InteropServices.UnmanagedType.SafeArray" />.</summary>
	public Type SafeArrayUserDefinedSubType;

	private UnmanagedType utype;

	/// <summary>Specifies the element type of the unmanaged <see cref="F:System.Runtime.InteropServices.UnmanagedType.LPArray" /> or <see cref="F:System.Runtime.InteropServices.UnmanagedType.ByValArray" />.</summary>
	public UnmanagedType ArraySubType;

	/// <summary>Indicates the element type of the <see cref="F:System.Runtime.InteropServices.UnmanagedType.SafeArray" />.</summary>
	public VarEnum SafeArraySubType;

	/// <summary>Indicates the number of elements in the fixed-length array or the number of characters (not bytes) in a string to import.</summary>
	public int SizeConst;

	/// <summary>Specifies the parameter index of the unmanaged iid_is attribute used by COM.</summary>
	public int IidParameterIndex;

	/// <summary>Indicates the zero-based parameter that contains the count of array elements, similar to size_is in COM.</summary>
	public short SizeParamIndex;

	/// <summary>Gets the <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> value the data is to be marshaled as.</summary>
	/// <returns>The <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> value the data is to be marshaled as.</returns>
	public UnmanagedType Value => utype;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.MarshalAsAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> value.</summary>
	/// <param name="unmanagedType">The value the data is to be marshaled as. </param>
	public MarshalAsAttribute(short unmanagedType)
	{
		utype = (UnmanagedType)unmanagedType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.MarshalAsAttribute" /> class with the specified <see cref="T:System.Runtime.InteropServices.UnmanagedType" /> enumeration member.</summary>
	/// <param name="unmanagedType">The value the data is to be marshaled as. </param>
	public MarshalAsAttribute(UnmanagedType unmanagedType)
	{
		utype = unmanagedType;
	}

	internal MarshalAsAttribute Copy()
	{
		return (MarshalAsAttribute)MemberwiseClone();
	}
}

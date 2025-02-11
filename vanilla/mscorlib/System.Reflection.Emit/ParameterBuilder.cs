using System.Runtime.InteropServices;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Creates or associates parameter information.</summary>
[StructLayout(LayoutKind.Sequential)]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_ParameterBuilder))]
public class ParameterBuilder : _ParameterBuilder
{
	private MethodBase methodb;

	private string name;

	private CustomAttributeBuilder[] cattrs;

	private UnmanagedMarshal marshal_info;

	private ParameterAttributes attrs;

	private int position;

	private int table_idx;

	private object def_value;

	/// <summary>Retrieves the attributes for this parameter.</summary>
	/// <returns>Read-only. Retrieves the attributes for this parameter.</returns>
	public virtual int Attributes => (int)attrs;

	/// <summary>Retrieves whether this is an input parameter.</summary>
	/// <returns>Read-only. Retrieves whether this is an input parameter.</returns>
	public bool IsIn => (attrs & ParameterAttributes.In) != 0;

	/// <summary>Retrieves whether this parameter is an output parameter.</summary>
	/// <returns>Read-only. Retrieves whether this parameter is an output parameter.</returns>
	public bool IsOut => (attrs & ParameterAttributes.Out) != 0;

	/// <summary>Retrieves whether this parameter is optional.</summary>
	/// <returns>Read-only. Specifies whether this parameter is optional.</returns>
	public bool IsOptional => (attrs & ParameterAttributes.Optional) != 0;

	/// <summary>Retrieves the name of this parameter.</summary>
	/// <returns>Read-only. Retrieves the name of this parameter.</returns>
	public virtual string Name => name;

	/// <summary>Retrieves the signature position for this parameter.</summary>
	/// <returns>Read-only. Retrieves the signature position for this parameter.</returns>
	public virtual int Position => position;

	internal ParameterBuilder(MethodBase mb, int pos, ParameterAttributes attributes, string strParamName)
	{
		name = strParamName;
		position = pos;
		attrs = attributes;
		methodb = mb;
		if (mb is DynamicMethod)
		{
			table_idx = 0;
		}
		else
		{
			table_idx = mb.get_next_table_index(this, 8, inc: true);
		}
	}

	/// <summary>Retrieves the token for this parameter.</summary>
	/// <returns>Returns the token for this parameter.</returns>
	public virtual ParameterToken GetToken()
	{
		return new ParameterToken(8 | table_idx);
	}

	/// <summary>Sets the default value of the parameter.</summary>
	/// <param name="defaultValue">The default value of this parameter. </param>
	/// <exception cref="T:System.ArgumentException">The parameter is not one of the supported types.-or-The type of <paramref name="defaultValue" /> does not match the type of the parameter.-or-The parameter is of type <see cref="T:System.Object" /> or other reference type, <paramref name="defaultValue" /> is not null, and the value cannot be assigned to the reference type. </exception>
	public virtual void SetConstant(object defaultValue)
	{
		if (position > 0)
		{
			Type parameterType = methodb.GetParameterType(position - 1);
			if (defaultValue != null && parameterType != defaultValue.GetType() && (!parameterType.IsEnum || parameterType.UnderlyingSystemType != defaultValue.GetType()))
			{
				throw new ArgumentException("Constant does not match the defined type.");
			}
			if (parameterType.IsValueType && !parameterType.IsPrimitive && !parameterType.IsEnum && parameterType != typeof(DateTime))
			{
				throw new ArgumentException(string.Concat(parameterType, " is not a supported constant type."));
			}
		}
		def_value = defaultValue;
		attrs |= ParameterAttributes.HasDefault;
	}

	/// <summary>Set a custom attribute using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class to define the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null. </exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		switch (customBuilder.Ctor.ReflectedType.FullName)
		{
		case "System.Runtime.InteropServices.InAttribute":
			attrs |= ParameterAttributes.In;
			return;
		case "System.Runtime.InteropServices.OutAttribute":
			attrs |= ParameterAttributes.Out;
			return;
		case "System.Runtime.InteropServices.OptionalAttribute":
			attrs |= ParameterAttributes.Optional;
			return;
		case "System.Runtime.InteropServices.MarshalAsAttribute":
			attrs |= ParameterAttributes.HasFieldMarshal;
			marshal_info = CustomAttributeBuilder.get_umarshal(customBuilder, is_field: false);
			return;
		case "System.Runtime.InteropServices.DefaultParameterValueAttribute":
			SetConstant(CustomAttributeBuilder.decode_cattr(customBuilder).ctorArgs[0]);
			return;
		}
		if (cattrs != null)
		{
			CustomAttributeBuilder[] array = new CustomAttributeBuilder[cattrs.Length + 1];
			cattrs.CopyTo(array, 0);
			array[cattrs.Length] = customBuilder;
			cattrs = array;
		}
		else
		{
			cattrs = new CustomAttributeBuilder[1];
			cattrs[0] = customBuilder;
		}
	}

	/// <summary>Set a custom attribute using a specified custom attribute blob.</summary>
	/// <param name="con">The constructor for the custom attribute. </param>
	/// <param name="binaryAttribute">A byte blob representing the attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> or <paramref name="binaryAttribute" /> is null. </exception>
	[ComVisible(true)]
	public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
	{
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	/// <summary>Specifies the marshaling for this parameter.</summary>
	/// <param name="unmanagedMarshal">The marshaling information for this parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="unmanagedMarshal" /> is null. </exception>
	[Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
	public virtual void SetMarshal(UnmanagedMarshal unmanagedMarshal)
	{
		marshal_info = unmanagedMarshal;
		attrs |= ParameterAttributes.HasFieldMarshal;
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ParameterBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ParameterBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ParameterBuilder.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ParameterBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	internal ParameterBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}

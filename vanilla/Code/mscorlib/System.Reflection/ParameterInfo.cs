using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace System.Reflection;

/// <summary>Discovers the attributes of a parameter and provides access to parameter metadata.</summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ComDefaultInterface(typeof(_ParameterInfo))]
public class ParameterInfo : ICustomAttributeProvider, _ParameterInfo, IObjectReference
{
	/// <summary>The Type of the parameter.</summary>
	protected Type ClassImpl;

	/// <summary>The default value of the parameter.</summary>
	protected object DefaultValueImpl;

	/// <summary>The member in which the field is implemented.</summary>
	protected MemberInfo MemberImpl;

	/// <summary>The name of the parameter.</summary>
	protected string NameImpl;

	/// <summary>The zero-based position of the parameter in the parameter list.</summary>
	protected int PositionImpl;

	/// <summary>The attributes of the parameter.</summary>
	protected ParameterAttributes AttrsImpl;

	internal MarshalAsAttribute marshalAs;

	/// <summary>Gets the Type of this parameter.</summary>
	/// <returns>The Type object that represents the Type of this parameter.</returns>
	public virtual Type ParameterType => ClassImpl;

	/// <summary>Gets the attributes for this parameter.</summary>
	/// <returns>A ParameterAttributes object representing the attributes for this parameter.</returns>
	public virtual ParameterAttributes Attributes => AttrsImpl;

	/// <summary>Gets a value indicating whether this is an input parameter.</summary>
	/// <returns>true if the parameter is an input parameter; otherwise, false.</returns>
	public bool IsIn => (Attributes & ParameterAttributes.In) != 0;

	/// <summary>Gets a value indicating whether this parameter is a locale identifier (lcid).</summary>
	/// <returns>true if the parameter is a locale identifier; otherwise, false.</returns>
	public bool IsLcid => (Attributes & ParameterAttributes.Lcid) != 0;

	/// <summary>Gets a value indicating whether this parameter is optional.</summary>
	/// <returns>true if the parameter is optional; otherwise, false.</returns>
	public bool IsOptional => (Attributes & ParameterAttributes.Optional) != 0;

	/// <summary>Gets a value indicating whether this is an output parameter.</summary>
	/// <returns>true if the parameter is an output parameter; otherwise, false.</returns>
	public bool IsOut => (Attributes & ParameterAttributes.Out) != 0;

	/// <summary>Gets a value indicating whether this is a Retval parameter.</summary>
	/// <returns>true if the parameter is a Retval; otherwise, false.</returns>
	public bool IsRetval => (Attributes & ParameterAttributes.Retval) != 0;

	/// <summary>Gets a value indicating the member in which the parameter is implemented.</summary>
	/// <returns>The member which implanted the parameter represented by this <see cref="T:System.Reflection.ParameterInfo" />.</returns>
	public virtual MemberInfo Member => MemberImpl;

	/// <summary>Gets the name of the parameter.</summary>
	/// <returns>The simple name of this parameter.</returns>
	public virtual string Name => NameImpl;

	/// <summary>Gets the zero-based position of the parameter in the formal parameter list.</summary>
	/// <returns>An integer representing the position this parameter occupies in the parameter list.</returns>
	public virtual int Position => PositionImpl;

	/// <summary>Gets a collection that contains this parameter's custom attributes.</summary>
	/// <returns>A collection that contains this parameter's custom attributes.</returns>
	public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

	/// <summary>Gets a value that indicates whether this parameter has a default value.</summary>
	/// <returns>true if this parameter has a default value; otherwise, false.</returns>
	public virtual bool HasDefaultValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value indicating the default value if the parameter has a default value.</summary>
	/// <returns>The default value of the parameter, or <see cref="F:System.DBNull.Value" /> if the parameter has no default value.</returns>
	public virtual object DefaultValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value indicating the default value if the parameter has a default value.</summary>
	/// <returns>The default value of the parameter, or <see cref="F:System.DBNull.Value" /> if the parameter has no default value.</returns>
	public virtual object RawDefaultValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value that identifies this parameter in metadata.</summary>
	/// <returns>A value which, in combination with the module, uniquely identifies this parameter in metadata.</returns>
	public virtual int MetadataToken => 134217728;

	/// <summary>Initializes a new instance of the ParameterInfo class.</summary>
	protected ParameterInfo()
	{
	}

	/// <summary>Gets the parameter type and name represented as a string.</summary>
	/// <returns>A string containing the type and the name of the parameter.</returns>
	public override string ToString()
	{
		Type type = ClassImpl;
		while (type.HasElementType)
		{
			type = type.GetElementType();
		}
		string text = ((type.IsPrimitive || ClassImpl == typeof(void) || ClassImpl.Namespace == MemberImpl.DeclaringType.Namespace) ? ClassImpl.Name : ClassImpl.FullName);
		if (!IsRetval)
		{
			text += " ";
			text += NameImpl;
		}
		return text;
	}

	internal static void FormatParameters(StringBuilder sb, ParameterInfo[] p, CallingConventions callingConvention, bool serialization)
	{
		for (int i = 0; i < p.Length; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}
			Type parameterType = p[i].ParameterType;
			string text = parameterType.FormatTypeName(serialization);
			if (parameterType.IsByRef && !serialization)
			{
				sb.Append(text.TrimEnd('&'));
				sb.Append(" ByRef");
			}
			else
			{
				sb.Append(text);
			}
		}
		if ((callingConvention & CallingConventions.VarArgs) != 0)
		{
			if (p.Length != 0)
			{
				sb.Append(", ");
			}
			sb.Append("...");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern int GetMetadataToken();

	internal object[] GetPseudoCustomAttributes()
	{
		int num = 0;
		if (IsIn)
		{
			num++;
		}
		if (IsOut)
		{
			num++;
		}
		if (IsOptional)
		{
			num++;
		}
		if (marshalAs != null)
		{
			num++;
		}
		if (num == 0)
		{
			return null;
		}
		object[] array = new object[num];
		num = 0;
		if (IsIn)
		{
			array[num++] = new InAttribute();
		}
		if (IsOptional)
		{
			array[num++] = new OptionalAttribute();
		}
		if (IsOut)
		{
			array[num++] = new OutAttribute();
		}
		if (marshalAs != null)
		{
			array[num++] = marshalAs.Copy();
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern Type[] GetTypeModifiers(bool optional);

	internal object GetDefaultValueImpl()
	{
		return DefaultValueImpl;
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ParameterInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ParameterInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ParameterInfo.GetTypeInfoCount(out uint pcTInfo)
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
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ParameterInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets all the custom attributes defined on this parameter.</summary>
	/// <returns>An array that contains all the custom attributes applied to this parameter.</returns>
	/// <param name="inherit">This argument is ignored for objects of this type. See Remarks.</param>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type could not be loaded. </exception>
	public virtual object[] GetCustomAttributes(bool inherit)
	{
		return new object[0];
	}

	/// <summary>Gets the custom attributes of the specified type or its derived types that are applied to this parameter.</summary>
	/// <returns>An array that contains the custom attributes of the specified type or its derived types.</returns>
	/// <param name="attributeType">The custom attributes identified by type. </param>
	/// <param name="inherit">This argument is ignored for objects of this type. See Remarks.</param>
	/// <exception cref="T:System.ArgumentException">The type must be a type provided by the underlying runtime system.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null.</exception>
	/// <exception cref="T:System.TypeLoadException">A custom attribute type could not be loaded. </exception>
	public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return new object[0];
	}

	/// <summary>Returns the real object that should be deserialized instead of the object that the serialized stream specifies.</summary>
	/// <returns>The actual object that is put into the graph.</returns>
	/// <param name="context">The serialized stream from which the current object is deserialized.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The parameter's position in the parameter list of its associated member is not valid for that member's type.</exception>
	[SecurityCritical]
	public object GetRealObject(StreamingContext context)
	{
		throw new NotImplementedException();
	}

	/// <summary>Determines whether the custom attribute of the specified type or its derived types is applied to this parameter.</summary>
	/// <returns>true if one or more instances of <paramref name="attributeType" /> or its derived types are applied to this parameter; otherwise, false.</returns>
	/// <param name="attributeType">The Type object to search for. </param>
	/// <param name="inherit">This argument is ignored for objects of this type. See Remarks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="attributeType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="attributeType" /> is not a <see cref="T:System.Type" /> object supplied by the common language runtime.</exception>
	public virtual bool IsDefined(Type attributeType, bool inherit)
	{
		return false;
	}

	/// <summary>Gets the required custom modifiers of the parameter.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that identify the required custom modifiers of the current parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" /> or <see cref="T:System.Runtime.CompilerServices.IsImplicitlyDereferenced" />.</returns>
	public virtual Type[] GetRequiredCustomModifiers()
	{
		return new Type[0];
	}

	/// <summary>Gets the optional custom modifiers of the parameter.</summary>
	/// <returns>An array of <see cref="T:System.Type" /> objects that identify the optional custom modifiers of the current parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" /> or <see cref="T:System.Runtime.CompilerServices.IsImplicitlyDereferenced" />.</returns>
	public virtual Type[] GetOptionalCustomModifiers()
	{
		return new Type[0];
	}

	/// <summary>Returns a list of <see cref="T:System.Reflection.CustomAttributeData" /> objects for the current parameter, which can be used in the reflection-only context.</summary>
	/// <returns>A generic list of <see cref="T:System.Reflection.CustomAttributeData" /> objects representing data about the attributes that have been applied to the current parameter.</returns>
	public virtual IList<CustomAttributeData> GetCustomAttributesData()
	{
		throw new NotImplementedException();
	}

	internal static ParameterInfo New(ParameterBuilder pb, Type type, MemberInfo member, int position)
	{
		return new MonoParameterInfo(pb, type, member, position);
	}

	internal static ParameterInfo New(ParameterInfo pinfo, Type type, MemberInfo member, int position)
	{
		return new MonoParameterInfo(pinfo, type, member, position);
	}

	internal static ParameterInfo New(ParameterInfo pinfo, MemberInfo member)
	{
		return new MonoParameterInfo(pinfo, member);
	}

	internal static ParameterInfo New(Type type, MemberInfo member, MarshalAsAttribute marshalAs)
	{
		return new MonoParameterInfo(type, member, marshalAs);
	}
}

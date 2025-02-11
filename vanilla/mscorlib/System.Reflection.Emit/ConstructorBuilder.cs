using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and represents a constructor of a dynamic class.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_ConstructorBuilder))]
[ClassInterface(ClassInterfaceType.None)]
public sealed class ConstructorBuilder : ConstructorInfo, _ConstructorBuilder
{
	private RuntimeMethodHandle mhandle;

	private ILGenerator ilgen;

	internal Type[] parameters;

	private MethodAttributes attrs;

	private MethodImplAttributes iattrs;

	private int table_idx;

	private CallingConventions call_conv;

	private TypeBuilder type;

	internal ParameterBuilder[] pinfo;

	private CustomAttributeBuilder[] cattrs;

	private bool init_locals;

	private Type[][] paramModReq;

	private Type[][] paramModOpt;

	private RefEmitPermissionSet[] permissions;

	/// <summary>Gets a <see cref="T:System.Reflection.CallingConventions" /> value that depends on whether the declaring type is generic.</summary>
	/// <returns>
	///   <see cref="F:System.Reflection.CallingConventions.HasThis" /> if the declaring type is generic; otherwise, <see cref="F:System.Reflection.CallingConventions.Standard" />. </returns>
	[MonoTODO]
	public override CallingConventions CallingConvention => call_conv;

	/// <summary>Gets or sets whether the local variables in this constructor should be zero-initialized.</summary>
	/// <returns>Read/write. Gets or sets whether the local variables in this constructor should be zero-initialized.</returns>
	public bool InitLocals
	{
		get
		{
			return init_locals;
		}
		set
		{
			init_locals = value;
		}
	}

	internal TypeBuilder TypeBuilder => type;

	/// <summary>Retrieves the internal handle for the method. Use this handle to access the underlying metadata handle.</summary>
	/// <returns>Returns the internal handle for the method. Use this handle to access the underlying metadata handle.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported on this class. </exception>
	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw not_supported();
		}
	}

	/// <summary>Retrieves the attributes for this constructor.</summary>
	/// <returns>Returns the attributes for this constructor.</returns>
	public override MethodAttributes Attributes => attrs;

	/// <summary>Holds a reference to the <see cref="T:System.Type" /> object from which this object was obtained.</summary>
	/// <returns>Returns the Type object from which this object was obtained.</returns>
	public override Type ReflectedType => type;

	/// <summary>Retrieves a reference to the <see cref="T:System.Type" /> object for the type that declares this member.</summary>
	/// <returns>Returns the <see cref="T:System.Type" /> object for the type that declares this member.</returns>
	public override Type DeclaringType => type;

	/// <summary>Gets null.</summary>
	/// <returns>Returns null.</returns>
	[Obsolete]
	public Type ReturnType => null;

	/// <summary>Retrieves the name of this constructor.</summary>
	/// <returns>Returns the name of this constructor.</returns>
	public override string Name
	{
		get
		{
			if ((attrs & MethodAttributes.Static) == 0)
			{
				return ConstructorInfo.ConstructorName;
			}
			return ConstructorInfo.TypeConstructorName;
		}
	}

	/// <summary>Retrieves the signature of the field in the form of a string.</summary>
	/// <returns>Returns the signature of the field.</returns>
	public string Signature => "constructor signature";

	/// <summary>Gets the dynamic module in which this constructor is defined.</summary>
	/// <returns>A <see cref="T:System.Reflection.Module" /> object that represents the dynamic module in which this constructor is defined.</returns>
	public override Module Module => GetModule();

	internal ConstructorBuilder(TypeBuilder tb, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt)
	{
		init_locals = true;
		base._002Ector();
		attrs = attributes | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
		call_conv = callingConvention;
		if (parameterTypes != null)
		{
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if (parameterTypes[i] == null)
				{
					throw new ArgumentException("Elements of the parameterTypes array cannot be null", "parameterTypes");
				}
			}
			parameters = new Type[parameterTypes.Length];
			Array.Copy(parameterTypes, parameters, parameterTypes.Length);
		}
		type = tb;
		this.paramModReq = paramModReq;
		this.paramModOpt = paramModOpt;
		table_idx = get_next_table_index(this, 6, inc: true);
		((ModuleBuilder)tb.Module).RegisterToken(this, GetToken().Token);
	}

	/// <summary>Returns the method implementation flags for this constructor.</summary>
	/// <returns>The method implementation flags for this constructor.</returns>
	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return iattrs;
	}

	/// <summary>Returns the parameters of this constructor.</summary>
	/// <returns>Returns an array of <see cref="T:System.Reflection.ParameterInfo" /> objects that represent the parameters of this constructor.</returns>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> has not been called on this constructor's type, in the .NET Framework versions 1.0 and 1.1. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> has not been called on this constructor's type, in the .NET Framework version 2.0. </exception>
	public override ParameterInfo[] GetParameters()
	{
		if (!type.is_created)
		{
			throw not_created();
		}
		return GetParametersInternal();
	}

	internal override ParameterInfo[] GetParametersInternal()
	{
		if (parameters == null)
		{
			return EmptyArray<ParameterInfo>.Value;
		}
		ParameterInfo[] array = new ParameterInfo[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = ParameterInfo.New((pinfo == null) ? null : pinfo[i + 1], parameters[i], this, i + 1);
		}
		return array;
	}

	internal override int GetParametersCount()
	{
		if (parameters == null)
		{
			return 0;
		}
		return parameters.Length;
	}

	internal override Type GetParameterType(int pos)
	{
		return parameters[pos];
	}

	internal MethodBase RuntimeResolve()
	{
		return type.RuntimeResolve().GetConstructor(this);
	}

	/// <summary>Dynamically invokes the constructor reflected by this instance with the specified arguments, under the constraints of the specified Binder.</summary>
	/// <returns>An instance of the class associated with the constructor.</returns>
	/// <param name="obj">The object that needs to be reinitialized. </param>
	/// <param name="invokeAttr">One of the BindingFlags values that specifies the type of binding that is desired. </param>
	/// <param name="binder">A Binder that defines a set of properties and enables the binding, coercion of argument types, and invocation of members using reflection. If <paramref name="binder" /> is null, then Binder.DefaultBinding is used. </param>
	/// <param name="parameters">An argument list. This is an array of arguments with the same number, order, and type as the parameters of the constructor to be invoked. If there are no parameters, this should be a null reference (Nothing in Visual Basic). </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> used to govern the coercion of types. If this is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. You can retrieve the constructor using <see cref="M:System.Type.GetConstructor(System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.ConstructorInfo.Invoke(System.Reflection.BindingFlags,System.Reflection.Binder,System.Object[],System.Globalization.CultureInfo)" /> on the returned <see cref="T:System.Reflection.ConstructorInfo" />. </exception>
	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw not_supported();
	}

	/// <summary>Invokes the constructor dynamically reflected by this instance on the given object, passing along the specified parameters, and under the constraints of the given binder.</summary>
	/// <returns>Returns an <see cref="T:System.Object" /> that is the return value of the invoked constructor.</returns>
	/// <param name="invokeAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" />, such as InvokeMethod, NonPublic, and so on. </param>
	/// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of MemberInfo objects using reflection. If binder is null, the default binder is used. See <see cref="T:System.Reflection.Binder" />. </param>
	/// <param name="parameters">An argument list. This is an array of arguments with the same number, order, and type as the parameters of the constructor to be invoked. If there are no parameters this should be null. </param>
	/// <param name="culture">An instance of <see cref="T:System.Globalization.CultureInfo" /> used to govern the coercion of types. If this is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. (For example, this is necessary to convert a <see cref="T:System.String" /> that represents 1000 to a <see cref="T:System.Double" /> value, since 1000 is represented differently by different cultures.) </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. You can retrieve the constructor using <see cref="M:System.Type.GetConstructor(System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.ConstructorInfo.Invoke(System.Reflection.BindingFlags,System.Reflection.Binder,System.Object[],System.Globalization.CultureInfo)" /> on the returned <see cref="T:System.Reflection.ConstructorInfo" />. </exception>
	public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw not_supported();
	}

	/// <summary>Adds declarative security to this constructor.</summary>
	/// <param name="action">The security action to be taken, such as Demand, Assert, and so on. </param>
	/// <param name="pset">The set of permissions the action applies to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="action" /> is invalid (RequestMinimum, RequestOptional, and RequestRefuse are invalid). </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- The permission set <paramref name="pset" /> contains an action that was added earlier by AddDeclarativeSecurity. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pset" /> is null. </exception>
	public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
	{
		if (pset == null)
		{
			throw new ArgumentNullException("pset");
		}
		if (action == SecurityAction.RequestMinimum || action == SecurityAction.RequestOptional || action == SecurityAction.RequestRefuse)
		{
			throw new ArgumentOutOfRangeException("action", "Request* values are not permitted");
		}
		RejectIfCreated();
		if (permissions != null)
		{
			RefEmitPermissionSet[] array = permissions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].action == action)
				{
					throw new InvalidOperationException("Multiple permission sets specified with the same SecurityAction.");
				}
			}
			RefEmitPermissionSet[] array2 = new RefEmitPermissionSet[permissions.Length + 1];
			permissions.CopyTo(array2, 0);
			permissions = array2;
		}
		else
		{
			permissions = new RefEmitPermissionSet[1];
		}
		permissions[permissions.Length - 1] = new RefEmitPermissionSet(action, pset.ToXml().ToString());
		attrs |= MethodAttributes.HasSecurity;
	}

	/// <summary>Defines a parameter of this constructor.</summary>
	/// <returns>Returns a ParameterBuilder object that represents the new parameter of this constructor.</returns>
	/// <param name="iSequence">The position of the parameter in the parameter list. Parameters are indexed beginning with the number 1 for the first parameter. </param>
	/// <param name="attributes">The attributes of the parameter. </param>
	/// <param name="strParamName">The name of the parameter. The name can be the null string. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="iSequence" /> is less than 0 (zero), or it is greater than the number of parameters of the constructor. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public ParameterBuilder DefineParameter(int iSequence, ParameterAttributes attributes, string strParamName)
	{
		if (iSequence < 0 || iSequence > GetParametersCount())
		{
			throw new ArgumentOutOfRangeException("iSequence");
		}
		if (type.is_created)
		{
			throw not_after_created();
		}
		ParameterBuilder parameterBuilder = new ParameterBuilder(this, iSequence, attributes, strParamName);
		if (pinfo == null)
		{
			pinfo = new ParameterBuilder[parameters.Length + 1];
		}
		pinfo[iSequence] = parameterBuilder;
		return parameterBuilder;
	}

	/// <summary>Checks if the specified custom attribute type is defined.</summary>
	/// <returns>true if the specified custom attribute type is defined; otherwise, false.</returns>
	/// <param name="attributeType">A custom attribute type. </param>
	/// <param name="inherit">Controls inheritance of custom attributes from base classes. This parameter is ignored. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. You can retrieve the constructor using <see cref="M:System.Type.GetConstructor(System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.MemberInfo.IsDefined(System.Type,System.Boolean)" /> on the returned <see cref="T:System.Reflection.ConstructorInfo" />. </exception>
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Returns all the custom attributes defined for this constructor.</summary>
	/// <returns>Returns an array of objects representing all the custom attributes of the constructor represented by this <see cref="T:System.Reflection.Emit.ConstructorBuilder" /> instance.</returns>
	/// <param name="inherit">Controls inheritance of custom attributes from base classes. This parameter is ignored. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	public override object[] GetCustomAttributes(bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Returns the custom attributes identified by the given type.</summary>
	/// <returns>Returns an array of type <see cref="T:System.Object" /> representing the attributes of this constructor.</returns>
	/// <param name="attributeType">The custom attribute type. </param>
	/// <param name="inherit">Controls inheritance of custom attributes from base classes. This parameter is ignored. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. </exception>
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw not_supported();
	}

	/// <summary>Gets an <see cref="T:System.Reflection.Emit.ILGenerator" /> for this constructor.</summary>
	/// <returns>Returns an <see cref="T:System.Reflection.Emit.ILGenerator" /> object for this constructor.</returns>
	/// <exception cref="T:System.InvalidOperationException">The constructor is a default constructor.-or-The constructor has <see cref="T:System.Reflection.MethodAttributes" /> or <see cref="T:System.Reflection.MethodImplAttributes" /> flags indicating that it should not have a method body.</exception>
	public ILGenerator GetILGenerator()
	{
		return GetILGenerator(64);
	}

	/// <summary>Gets an <see cref="T:System.Reflection.Emit.ILGenerator" /> object, with the specified MSIL stream size, that can be used to build a method body for this constructor.</summary>
	/// <returns>An <see cref="T:System.Reflection.Emit.ILGenerator" /> for this constructor.</returns>
	/// <param name="streamSize">The size of the MSIL stream, in bytes.</param>
	/// <exception cref="T:System.InvalidOperationException">The constructor is a default constructor.-or-The constructor has <see cref="T:System.Reflection.MethodAttributes" /> or <see cref="T:System.Reflection.MethodImplAttributes" /> flags indicating that it should not have a method body. </exception>
	public ILGenerator GetILGenerator(int streamSize)
	{
		if (ilgen != null)
		{
			return ilgen;
		}
		ilgen = new ILGenerator(type.Module, ((ModuleBuilder)type.Module).GetTokenGenerator(), streamSize);
		return ilgen;
	}

	/// <summary>Creates the body of the constructor by using a specified byte array of Microsoft intermediate language (MSIL) instructions.</summary>
	/// <param name="il">An array that contains valid MSIL instructions.</param>
	/// <param name="maxStack">The maximum stack evaluation depth.</param>
	/// <param name="localSignature">An array of bytes that contain the serialized local variable structure. Specify null if the constructor has no local variables.</param>
	/// <param name="exceptionHandlers">A collection that contains the exception handlers for the constructor. Specify null if the constructor has no exception handlers.</param>
	/// <param name="tokenFixups">A collection of values that represent offsets in <paramref name="il" />, each of which specifies the beginning of a token that may be modified. Specify null if the constructor has no tokens that have to be modified.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="il" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxStack" /> is negative.-or- One of <paramref name="exceptionHandlers" /> specifies an offset outside of <paramref name="il" />.-or- One of <paramref name="tokenFixups" /> specifies an offset that is outside the <paramref name="il" /> array.</exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using the <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method.-or- This method was called previously on this <see cref="T:System.Reflection.Emit.ConstructorBuilder" /> object.</exception>
	public void SetMethodBody(byte[] il, int maxStack, byte[] localSignature, IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups)
	{
		GetILGenerator().Init(il, maxStack, localSignature, exceptionHandlers, tokenFixups);
	}

	/// <summary>Set a custom attribute using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class to define the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customBuilder" /> is null. </exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (customBuilder == null)
		{
			throw new ArgumentNullException("customBuilder");
		}
		if (customBuilder.Ctor.ReflectedType.FullName == "System.Runtime.CompilerServices.MethodImplAttribute")
		{
			byte[] data = customBuilder.Data;
			int num = data[2];
			num |= data[3] << 8;
			SetImplementationFlags((MethodImplAttributes)num);
		}
		else if (cattrs != null)
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
		if (con == null)
		{
			throw new ArgumentNullException("con");
		}
		if (binaryAttribute == null)
		{
			throw new ArgumentNullException("binaryAttribute");
		}
		SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
	}

	/// <summary>Sets the method implementation flags for this constructor.</summary>
	/// <param name="attributes">The method implementation flags. </param>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />. </exception>
	public void SetImplementationFlags(MethodImplAttributes attributes)
	{
		if (type.is_created)
		{
			throw not_after_created();
		}
		iattrs = attributes;
	}

	/// <summary>Returns a reference to the module that contains this constructor.</summary>
	/// <returns>The module that contains this constructor.</returns>
	public Module GetModule()
	{
		return type.Module;
	}

	/// <summary>Returns the <see cref="T:System.Reflection.Emit.MethodToken" /> that represents the token for this constructor.</summary>
	/// <returns>Returns the <see cref="T:System.Reflection.Emit.MethodToken" /> of this constructor.</returns>
	public MethodToken GetToken()
	{
		return new MethodToken(0x6000000 | table_idx);
	}

	/// <summary>Sets this constructor's custom attribute associated with symbolic information.</summary>
	/// <param name="name">The name of the custom attribute. </param>
	/// <param name="data">The value of the custom attribute. </param>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- The module does not have a symbol writer defined. For example, the module is not a debug module. </exception>
	[MonoTODO]
	public void SetSymCustomAttribute(string name, byte[] data)
	{
		if (type.is_created)
		{
			throw not_after_created();
		}
	}

	/// <summary>Returns this <see cref="T:System.Reflection.Emit.ConstructorBuilder" /> instance as a <see cref="T:System.String" />.</summary>
	/// <returns>Returns a <see cref="T:System.String" /> containing the name, attributes, and exceptions of this constructor, followed by the current Microsoft intermediate language (MSIL) stream.</returns>
	public override string ToString()
	{
		return "ConstructorBuilder ['" + type.Name + "']";
	}

	internal void fixup()
	{
		if ((attrs & (MethodAttributes.Abstract | MethodAttributes.PinvokeImpl)) == 0 && (iattrs & (MethodImplAttributes)4099) == 0 && (ilgen == null || ilgen.ILOffset == 0))
		{
			throw new InvalidOperationException("Method '" + Name + "' does not have a method body.");
		}
		if (ilgen != null)
		{
			ilgen.label_fixup(this);
		}
	}

	internal void ResolveUserTypes()
	{
		TypeBuilder.ResolveUserTypes(parameters);
		if (paramModReq != null)
		{
			Type[][] array = paramModReq;
			for (int i = 0; i < array.Length; i++)
			{
				TypeBuilder.ResolveUserTypes(array[i]);
			}
		}
		if (paramModOpt != null)
		{
			Type[][] array = paramModOpt;
			for (int i = 0; i < array.Length; i++)
			{
				TypeBuilder.ResolveUserTypes(array[i]);
			}
		}
	}

	internal void FixupTokens(Dictionary<int, int> token_map, Dictionary<int, MemberInfo> member_map)
	{
		if (ilgen != null)
		{
			ilgen.FixupTokens(token_map, member_map);
		}
	}

	internal void GenerateDebugInfo(ISymbolWriter symbolWriter)
	{
		if (ilgen != null && ilgen.HasDebugInfo)
		{
			SymbolToken symbolToken = new SymbolToken(GetToken().Token);
			symbolWriter.OpenMethod(symbolToken);
			symbolWriter.SetSymAttribute(symbolToken, "__name", Encoding.UTF8.GetBytes(Name));
			ilgen.GenerateDebugInfo(symbolWriter);
			symbolWriter.CloseMethod();
		}
	}

	internal override int get_next_table_index(object obj, int table, bool inc)
	{
		return type.get_next_table_index(obj, table, inc);
	}

	private void RejectIfCreated()
	{
		if (type.is_created)
		{
			throw new InvalidOperationException("Type definition of the method is complete.");
		}
	}

	private Exception not_supported()
	{
		return new NotSupportedException("The invoked member is not supported in a dynamic module.");
	}

	private Exception not_after_created()
	{
		return new InvalidOperationException("Unable to change after type has been created.");
	}

	private Exception not_created()
	{
		return new NotSupportedException("The type is not yet created.");
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ConstructorBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ConstructorBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _ConstructorBuilder.GetTypeInfoCount(out uint pcTInfo)
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
	void _ConstructorBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	internal ConstructorBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}

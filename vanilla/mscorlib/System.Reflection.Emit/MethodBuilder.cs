using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Defines and represents a method (or constructor) on a dynamic class.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_MethodBuilder))]
[ClassInterface(ClassInterfaceType.None)]
public sealed class MethodBuilder : MethodInfo, _MethodBuilder
{
	private RuntimeMethodHandle mhandle;

	private Type rtype;

	internal Type[] parameters;

	private MethodAttributes attrs;

	private MethodImplAttributes iattrs;

	private string name;

	private int table_idx;

	private byte[] code;

	private ILGenerator ilgen;

	private TypeBuilder type;

	internal ParameterBuilder[] pinfo;

	private CustomAttributeBuilder[] cattrs;

	private MethodInfo[] override_methods;

	private string pi_dll;

	private string pi_entry;

	private CharSet charset;

	private uint extra_flags;

	private CallingConvention native_cc;

	private CallingConventions call_conv;

	private bool init_locals;

	private IntPtr generic_container;

	internal GenericTypeParameterBuilder[] generic_params;

	private Type[] returnModReq;

	private Type[] returnModOpt;

	private Type[][] paramModReq;

	private Type[][] paramModOpt;

	private RefEmitPermissionSet[] permissions;

	/// <summary>Not supported for this type.</summary>
	/// <returns>Not supported.</returns>
	/// <exception cref="T:System.NotSupportedException">The invoked method is not supported in the base class.</exception>
	public override bool ContainsGenericParameters
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets or sets a Boolean value that specifies whether the local variables in this method are zero initialized. The default value of this property is true.</summary>
	/// <returns>true if the local variables in this method should be zero initialized; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. (Get or set.)</exception>
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
	/// <returns>Read-only. The internal handle for the method. Use this handle to access the underlying metadata handle.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="P:System.Reflection.MethodBase.MethodHandle" /> on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw NotSupported();
		}
	}

	internal RuntimeMethodHandle MethodHandleInternal => mhandle;

	/// <summary>Gets the return type of the method represented by this <see cref="T:System.Reflection.Emit.MethodBuilder" />.</summary>
	/// <returns>The return type of the method.</returns>
	public override Type ReturnType => rtype;

	/// <summary>Retrieves the class that was used in reflection to obtain this object.</summary>
	/// <returns>Read-only. The type used to obtain this method.</returns>
	public override Type ReflectedType => type;

	/// <summary>Returns the type that declares this method.</summary>
	/// <returns>Read-only. The type that declares this method.</returns>
	public override Type DeclaringType => type;

	/// <summary>Retrieves the name of this method.</summary>
	/// <returns>Read-only. Retrieves a string containing the simple name of this method.</returns>
	public override string Name => name;

	/// <summary>Retrieves the attributes for this method.</summary>
	/// <returns>Read-only. Retrieves the MethodAttributes for this method.</returns>
	public override MethodAttributes Attributes => attrs;

	/// <summary>Returns the custom attributes of the method's return type.</summary>
	/// <returns>Read-only. The custom attributes of the method's return type.</returns>
	public override ICustomAttributeProvider ReturnTypeCustomAttributes => null;

	/// <summary>Returns the calling convention of the method.</summary>
	/// <returns>Read-only. The calling convention of the method.</returns>
	public override CallingConventions CallingConvention => call_conv;

	/// <summary>Retrieves the signature of the method.</summary>
	/// <returns>Read-only. A String containing the signature of the method reflected by this MethodBase instance.</returns>
	[MonoTODO("Not implemented")]
	public string Signature
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal bool BestFitMapping
	{
		set
		{
			extra_flags = (uint)((extra_flags & -49) | (uint)(value ? 16 : 32));
		}
	}

	internal bool ThrowOnUnmappableChar
	{
		set
		{
			extra_flags = (uint)((extra_flags & -12289) | (uint)(value ? 4096 : 8192));
		}
	}

	internal bool ExactSpelling
	{
		set
		{
			extra_flags = (uint)((extra_flags & -2) | (uint)(value ? 1 : 0));
		}
	}

	internal bool SetLastError
	{
		set
		{
			extra_flags = (uint)((extra_flags & -65) | (uint)(value ? 64 : 0));
		}
	}

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Reflection.Emit.MethodBuilder" /> object represents the definition of a generic method.</summary>
	/// <returns>true if the current <see cref="T:System.Reflection.Emit.MethodBuilder" /> object represents the definition of a generic method; otherwise, false.</returns>
	public override bool IsGenericMethodDefinition => generic_params != null;

	/// <summary>Gets a value indicating whether the method is a generic method.</summary>
	/// <returns>true if the method is generic; otherwise, false.</returns>
	public override bool IsGenericMethod => generic_params != null;

	/// <summary>Gets the module in which the current method is being defined.</summary>
	/// <returns>The <see cref="T:System.Reflection.Module" /> in which the member represented by the current <see cref="T:System.Reflection.MemberInfo" /> is being defined.</returns>
	public override Module Module => GetModule();

	/// <summary>Gets a <see cref="T:System.Reflection.ParameterInfo" /> object that contains information about the return type of the method, such as whether the return type has custom modifiers. </summary>
	/// <returns>A <see cref="T:System.Reflection.ParameterInfo" /> object that contains information about the return type.</returns>
	/// <exception cref="T:System.InvalidOperationException">The declaring type has not been created.</exception>
	public override ParameterInfo ReturnParameter => base.ReturnParameter;

	internal MethodBuilder(TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt)
	{
		init_locals = true;
		base._002Ector();
		this.name = name;
		attrs = attributes;
		call_conv = callingConvention;
		rtype = returnType;
		this.returnModReq = returnModReq;
		this.returnModOpt = returnModOpt;
		this.paramModReq = paramModReq;
		this.paramModOpt = paramModOpt;
		if ((attributes & MethodAttributes.Static) == 0)
		{
			call_conv |= CallingConventions.HasThis;
		}
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
		table_idx = get_next_table_index(this, 6, inc: true);
		((ModuleBuilder)tb.Module).RegisterToken(this, GetToken().Token);
	}

	internal MethodBuilder(TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt, string dllName, string entryName, CallingConvention nativeCConv, CharSet nativeCharset)
		: this(tb, name, attributes, callingConvention, returnType, returnModReq, returnModOpt, parameterTypes, paramModReq, paramModOpt)
	{
		pi_dll = dllName;
		pi_entry = entryName;
		native_cc = nativeCConv;
		charset = nativeCharset;
	}

	/// <summary>Returns the MethodToken that represents the token for this method.</summary>
	/// <returns>Returns the MethodToken of this method.</returns>
	public MethodToken GetToken()
	{
		return new MethodToken(0x6000000 | table_idx);
	}

	/// <summary>Return the base implementation for a method.</summary>
	/// <returns>The base implementation of this method.</returns>
	public override MethodInfo GetBaseDefinition()
	{
		return this;
	}

	/// <summary>Returns the implementation flags for the method.</summary>
	/// <returns>Returns the implementation flags for the method.</returns>
	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return iattrs;
	}

	/// <summary>Returns the parameters of this method.</summary>
	/// <returns>An array of ParameterInfo objects that represent the parameters of the method.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call GetParameters on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override ParameterInfo[] GetParameters()
	{
		if (!type.is_created)
		{
			throw NotSupported();
		}
		return GetParametersInternal();
	}

	internal override ParameterInfo[] GetParametersInternal()
	{
		if (parameters == null)
		{
			return null;
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
		return type.RuntimeResolve().GetMethod(this);
	}

	/// <summary>Returns a reference to the module that contains this method.</summary>
	/// <returns>Returns a reference to the module that contains this method.</returns>
	public Module GetModule()
	{
		return type.Module;
	}

	/// <summary>Creates the body of the method using a supplied byte array of Microsoft intermediate language (MSIL) instructions.</summary>
	/// <param name="il">An array containing valid MSIL instructions. If this parameter is null, the method's body is cleared. </param>
	/// <param name="count">The number of valid bytes in the MSIL array. This value is ignored if MSIL is null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="count" /> is not within the range of indexes of the supplied MSIL instruction array and <paramref name="il" /> is not null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- This method was called previously on this MethodBuilder with an <paramref name="il" /> argument that was not null.-or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
	public void CreateMethodBody(byte[] il, int count)
	{
		if (il != null && (count < 0 || count > il.Length))
		{
			throw new ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.");
		}
		if (code != null || type.is_created)
		{
			throw new InvalidOperationException("Type definition of the method is complete.");
		}
		if (il == null)
		{
			code = null;
			return;
		}
		code = new byte[count];
		Array.Copy(il, code, count);
	}

	/// <summary>Creates the body of the method by using a specified byte array of Microsoft intermediate language (MSIL) instructions.</summary>
	/// <param name="il">An array that contains valid MSIL instructions.</param>
	/// <param name="maxStack">The maximum stack evaluation depth.</param>
	/// <param name="localSignature">An array of bytes that contain the serialized local variable structure. Specify null if the method has no local variables.</param>
	/// <param name="exceptionHandlers">A collection that contains the exception handlers for the method. Specify null if the method has no exception handlers.</param>
	/// <param name="tokenFixups">A collection of values that represent offsets in <paramref name="il" />, each of which specifies the beginning of a token that may be modified. Specify null if the method has no tokens that have to be modified.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="il" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxStack" /> is negative.-or- One of <paramref name="exceptionHandlers" /> specifies an offset outside of <paramref name="il" />.-or- One of <paramref name="tokenFixups" /> specifies an offset that is outside the <paramref name="il" /> array.</exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using the <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method.-or- This method was called previously on this <see cref="T:System.Reflection.Emit.MethodBuilder" /> object.</exception>
	public void SetMethodBody(byte[] il, int maxStack, byte[] localSignature, IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups)
	{
		GetILGenerator().Init(il, maxStack, localSignature, exceptionHandlers, tokenFixups);
	}

	/// <summary>Dynamically invokes the method reflected by this instance on the given object, passing along the specified parameters, and under the constraints of the given binder.</summary>
	/// <returns>Returns an object containing the return value of the invoked method.</returns>
	/// <param name="obj">The object on which to invoke the specified method. If the method is static, this parameter is ignored. </param>
	/// <param name="invokeAttr">This must be a bit flag from <see cref="T:System.Reflection.BindingFlags" /> : InvokeMethod, NonPublic, and so on. </param>
	/// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of MemberInfo objects via reflection. If binder is null, the default binder is used. For more details, see <see cref="T:System.Reflection.Binder" />. </param>
	/// <param name="parameters">An argument list. This is an array of arguments with the same number, order, and type as the parameters of the method to be invoked. If there are no parameters this should be null. </param>
	/// <param name="culture">An instance of <see cref="T:System.Globalization.CultureInfo" /> used to govern the coercion of types. If this is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. (Note that this is necessary to, for example, convert a <see cref="T:System.String" /> that represents 1000 to a <see cref="T:System.Double" /> value, since 1000 is represented differently by different cultures.) </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[],System.Reflection.ParameterModifier[],System.Globalization.CultureInfo,System.String[])" /> on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw NotSupported();
	}

	/// <summary>Checks if the specified custom attribute type is defined.</summary>
	/// <returns>true if the specified custom attribute type is defined; otherwise, false.</returns>
	/// <param name="attributeType">The custom attribute type. </param>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the custom attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.MemberInfo.IsDefined(System.Type,System.Boolean)" /> on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw NotSupported();
	}

	/// <summary>Returns all the custom attributes defined for this method.</summary>
	/// <returns>Returns an array of objects representing all the custom attributes of this method.</returns>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the custom attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Boolean)" /> on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override object[] GetCustomAttributes(bool inherit)
	{
		if (type.is_created)
		{
			return MonoCustomAttrs.GetCustomAttributes(this, inherit);
		}
		throw NotSupported();
	}

	/// <summary>Returns the custom attributes identified by the given type.</summary>
	/// <returns>Returns an array of objects representing the attributes of this method that are of type <paramref name="attributeType" />.</returns>
	/// <param name="attributeType">The custom attribute type. </param>
	/// <param name="inherit">Specifies whether to search this member's inheritance chain to find the custom attributes. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not currently supported. Retrieve the method using <see cref="M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])" /> and call <see cref="M:System.Reflection.MemberInfo.GetCustomAttributes(System.Boolean)" /> on the returned <see cref="T:System.Reflection.MethodInfo" />. </exception>
	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		if (type.is_created)
		{
			return MonoCustomAttrs.GetCustomAttributes(this, attributeType, inherit);
		}
		throw NotSupported();
	}

	/// <summary>Returns an ILGenerator for this method with a default Microsoft intermediate language (MSIL) stream size of 64 bytes.</summary>
	/// <returns>Returns an ILGenerator object for this method.</returns>
	/// <exception cref="T:System.InvalidOperationException">The method should not have a body because of its <see cref="T:System.Reflection.MethodAttributes" /> or <see cref="T:System.Reflection.MethodImplAttributes" /> flags, for example because it has the <see cref="F:System.Reflection.MethodAttributes.PinvokeImpl" /> flag. -or-The method is a generic method, but not a generic method definition. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
	public ILGenerator GetILGenerator()
	{
		return GetILGenerator(64);
	}

	/// <summary>Returns an ILGenerator for this method with the specified Microsoft intermediate language (MSIL) stream size.</summary>
	/// <returns>Returns an ILGenerator object for this method.</returns>
	/// <param name="size">The size of the MSIL stream, in bytes. </param>
	/// <exception cref="T:System.InvalidOperationException">The method should not have a body because of its <see cref="T:System.Reflection.MethodAttributes" /> or <see cref="T:System.Reflection.MethodImplAttributes" /> flags, for example because it has the <see cref="F:System.Reflection.MethodAttributes.PinvokeImpl" /> flag. -or-The method is a generic method, but not a generic method definition. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.   </exception>
	public ILGenerator GetILGenerator(int size)
	{
		if ((iattrs & MethodImplAttributes.CodeTypeMask) != 0 || (iattrs & MethodImplAttributes.ManagedMask) != 0)
		{
			throw new InvalidOperationException("Method body should not exist.");
		}
		if (ilgen != null)
		{
			return ilgen;
		}
		ilgen = new ILGenerator(type.Module, ((ModuleBuilder)type.Module).GetTokenGenerator(), size);
		return ilgen;
	}

	/// <summary>Sets the parameter attributes and the name of a parameter of this method, or of the return value of this method. Returns a ParameterBuilder that can be used to apply custom attributes.</summary>
	/// <returns>Returns a ParameterBuilder object that represents a parameter of this method or the return value of this method.</returns>
	/// <param name="position">The position of the parameter in the parameter list. Parameters are indexed beginning with the number 1 for the first parameter; the number 0 represents the return value of the method. </param>
	/// <param name="attributes">The parameter attributes of the parameter. </param>
	/// <param name="strParamName">The name of the parameter. The name can be the null string. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The method has no parameters.-or- <paramref name="position" /> is less than zero.-or- <paramref name="position" /> is greater than the number of the method's parameters. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
	public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
	{
		RejectIfCreated();
		if (position < 0 || parameters == null || position > parameters.Length)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		ParameterBuilder parameterBuilder = new ParameterBuilder(this, position, attributes, strParamName);
		if (pinfo == null)
		{
			pinfo = new ParameterBuilder[parameters.Length + 1];
		}
		pinfo[position] = parameterBuilder;
		return parameterBuilder;
	}

	internal void check_override()
	{
		if (override_methods == null)
		{
			return;
		}
		MethodInfo[] array = override_methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.IsVirtual && !base.IsVirtual)
			{
				throw new TypeLoadException($"Method '{name}' override '{methodInfo}' but it is not virtual");
			}
		}
	}

	internal void fixup()
	{
		if ((attrs & (MethodAttributes.Abstract | MethodAttributes.PinvokeImpl)) == 0 && (iattrs & (MethodImplAttributes)4099) == 0 && (ilgen == null || ilgen.ILOffset == 0) && (code == null || code.Length == 0))
		{
			throw new InvalidOperationException($"Method '{DeclaringType.FullName}.{Name}' does not have a method body.");
		}
		if (ilgen != null)
		{
			ilgen.label_fixup(this);
		}
	}

	internal void ResolveUserTypes()
	{
		rtype = TypeBuilder.ResolveUserType(rtype);
		TypeBuilder.ResolveUserTypes(parameters);
		TypeBuilder.ResolveUserTypes(returnModReq);
		TypeBuilder.ResolveUserTypes(returnModOpt);
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

	/// <summary>Sets a custom attribute using a custom attribute builder.</summary>
	/// <param name="customBuilder">An instance of a helper class to describe the custom attribute. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customBuilder" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
	public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
	{
		if (customBuilder == null)
		{
			throw new ArgumentNullException("customBuilder");
		}
		switch (customBuilder.Ctor.ReflectedType.FullName)
		{
		case "System.Runtime.CompilerServices.MethodImplAttribute":
		{
			byte[] data = customBuilder.Data;
			int num = data[2];
			num |= data[3] << 8;
			iattrs |= (MethodImplAttributes)num;
			return;
		}
		case "System.Runtime.InteropServices.DllImportAttribute":
		{
			CustomAttributeBuilder.CustomAttributeInfo customAttributeInfo = CustomAttributeBuilder.decode_cattr(customBuilder);
			bool flag = true;
			pi_dll = (string)customAttributeInfo.ctorArgs[0];
			if (pi_dll == null || pi_dll.Length == 0)
			{
				throw new ArgumentException("DllName cannot be empty");
			}
			native_cc = System.Runtime.InteropServices.CallingConvention.Winapi;
			for (int i = 0; i < customAttributeInfo.namedParamNames.Length; i++)
			{
				string text = customAttributeInfo.namedParamNames[i];
				object obj = customAttributeInfo.namedParamValues[i];
				switch (text)
				{
				case "CallingConvention":
					native_cc = (CallingConvention)obj;
					break;
				case "CharSet":
					charset = (CharSet)obj;
					break;
				case "EntryPoint":
					pi_entry = (string)obj;
					break;
				case "ExactSpelling":
					ExactSpelling = (bool)obj;
					break;
				case "SetLastError":
					SetLastError = (bool)obj;
					break;
				case "PreserveSig":
					flag = (bool)obj;
					break;
				case "BestFitMapping":
					BestFitMapping = (bool)obj;
					break;
				case "ThrowOnUnmappableChar":
					ThrowOnUnmappableChar = (bool)obj;
					break;
				}
			}
			attrs |= MethodAttributes.PinvokeImpl;
			if (flag)
			{
				iattrs |= MethodImplAttributes.PreserveSig;
			}
			return;
		}
		case "System.Runtime.InteropServices.PreserveSigAttribute":
			iattrs |= MethodImplAttributes.PreserveSig;
			return;
		case "System.Runtime.CompilerServices.SpecialNameAttribute":
			attrs |= MethodAttributes.SpecialName;
			return;
		case "System.Security.SuppressUnmanagedCodeSecurityAttribute":
			attrs |= MethodAttributes.HasSecurity;
			break;
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

	/// <summary>Sets a custom attribute using a specified custom attribute blob.</summary>
	/// <param name="con">The constructor for the custom attribute. </param>
	/// <param name="binaryAttribute">A byte blob representing the attributes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> or <paramref name="binaryAttribute" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
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

	/// <summary>Sets the implementation flags for this method.</summary>
	/// <param name="attributes">The implementation flags to set. </param>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
	public void SetImplementationFlags(MethodImplAttributes attributes)
	{
		RejectIfCreated();
		iattrs = attributes;
	}

	/// <summary>Adds declarative security to this method.</summary>
	/// <param name="action">The security action to be taken (Demand, Assert, and so on). </param>
	/// <param name="pset">The set of permissions the action applies to. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="action" /> is invalid (RequestMinimum, RequestOptional, and RequestRefuse are invalid). </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-The permission set <paramref name="pset" /> contains an action that was added earlier by <see cref="M:System.Reflection.Emit.MethodBuilder.AddDeclarativeSecurity(System.Security.Permissions.SecurityAction,System.Security.PermissionSet)" />.-or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
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
			throw new ArgumentOutOfRangeException("Request* values are not permitted", "action");
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

	/// <summary>Sets marshaling information for the return type of this method.</summary>
	/// <param name="unmanagedMarshal">Marshaling information for the return type of this method. </param>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false. </exception>
	[Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
	public void SetMarshal(UnmanagedMarshal unmanagedMarshal)
	{
		RejectIfCreated();
		throw new NotImplementedException();
	}

	/// <summary>Set a symbolic custom attribute using a blob.</summary>
	/// <param name="name">The name of the symbolic custom attribute. </param>
	/// <param name="data">The byte blob that represents the value of the symbolic custom attribute. </param>
	/// <exception cref="T:System.InvalidOperationException">The containing type was previously created using <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" />.-or- The module that contains this method is not a debug module. -or-For the current method, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
	[MonoTODO]
	public void SetSymCustomAttribute(string name, byte[] data)
	{
		RejectIfCreated();
		throw new NotImplementedException();
	}

	/// <summary>Returns this MethodBuilder instance as a string.</summary>
	/// <returns>Returns a string containing the name, attributes, method signature, exceptions, and local signature of this method followed by the current Microsoft intermediate language (MSIL) stream.</returns>
	[SecuritySafeCritical]
	public override string ToString()
	{
		return "MethodBuilder [" + type.Name + "::" + name + "]";
	}

	/// <summary>Determines whether the given object is equal to this instance.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of MethodBuilder and is equal to this object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this MethodBuilder instance. </param>
	[MonoTODO]
	[SecuritySafeCritical]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	/// <summary>Gets the hash code for this method.</summary>
	/// <returns>The hash code for this method.</returns>
	public override int GetHashCode()
	{
		return name.GetHashCode();
	}

	internal override int get_next_table_index(object obj, int table, bool inc)
	{
		return type.get_next_table_index(obj, table, inc);
	}

	private void ExtendArray<T>(ref T[] array, T elem)
	{
		if (array == null)
		{
			array = new T[1];
		}
		else
		{
			T[] array2 = new T[array.Length + 1];
			Array.Copy(array, array2, array.Length);
			array = array2;
		}
		array[array.Length - 1] = elem;
	}

	internal void set_override(MethodInfo mdecl)
	{
		ExtendArray(ref override_methods, mdecl);
	}

	private void RejectIfCreated()
	{
		if (type.is_created)
		{
			throw new InvalidOperationException("Type definition of the method is complete.");
		}
	}

	private Exception NotSupported()
	{
		return new NotSupportedException("The invoked member is not supported in a dynamic module.");
	}

	/// <summary>Returns a generic method constructed from the current generic method definition using the specified generic type arguments.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> representing the generic method constructed from the current generic method definition using the specified generic type arguments.</returns>
	/// <param name="typeArguments">An array of <see cref="T:System.Type" /> objects that represent the type arguments for the generic method.</param>
	public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
	{
		if (!IsGenericMethodDefinition)
		{
			throw new InvalidOperationException("Method is not a generic method definition");
		}
		if (typeArguments == null)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (generic_params.Length != typeArguments.Length)
		{
			throw new ArgumentException("Incorrect length", "typeArguments");
		}
		for (int i = 0; i < typeArguments.Length; i++)
		{
			if (typeArguments[i] == null)
			{
				throw new ArgumentNullException("typeArguments");
			}
		}
		return new MethodOnTypeBuilderInst(this, typeArguments);
	}

	/// <summary>Returns this method.</summary>
	/// <returns>The current instance of <see cref="T:System.Reflection.Emit.MethodBuilder" />. </returns>
	/// <exception cref="T:System.InvalidOperationException">The current method is not generic. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property returns false.</exception>
	public override MethodInfo GetGenericMethodDefinition()
	{
		if (!IsGenericMethodDefinition)
		{
			throw new InvalidOperationException();
		}
		return this;
	}

	/// <summary>Returns an array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects that represent the type parameters of the method, if it is generic.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects representing the type parameters, if the method is generic, or null if the method is not generic. </returns>
	public override Type[] GetGenericArguments()
	{
		if (generic_params == null)
		{
			return null;
		}
		Type[] array = new Type[generic_params.Length];
		for (int i = 0; i < generic_params.Length; i++)
		{
			array[i] = generic_params[i];
		}
		return array;
	}

	/// <summary>Sets the number of generic type parameters for the current method, specifies their names, and returns an array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects that can be used to define their constraints.</summary>
	/// <returns>An array of <see cref="T:System.Reflection.Emit.GenericTypeParameterBuilder" /> objects representing the type parameters of the generic method.</returns>
	/// <param name="names">An array of strings that represent the names of the generic type parameters.</param>
	/// <exception cref="T:System.InvalidOperationException">Generic type parameters have already been defined for this method.-or-The method has been completed already.-or-The <see cref="M:System.Reflection.Emit.MethodBuilder.SetImplementationFlags(System.Reflection.MethodImplAttributes)" /> method has been called for the current method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="names" /> is null.-or-An element of <paramref name="names" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="names" /> is an empty array.</exception>
	public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
	{
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		if (names.Length == 0)
		{
			throw new ArgumentException("names");
		}
		generic_params = new GenericTypeParameterBuilder[names.Length];
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			if (text == null)
			{
				throw new ArgumentNullException("names");
			}
			generic_params[i] = new GenericTypeParameterBuilder(type, this, text, i);
		}
		return generic_params;
	}

	/// <summary>Sets the return type of the method.</summary>
	/// <param name="returnType">A <see cref="T:System.Type" /> object that represents the return type of the method.</param>
	/// <exception cref="T:System.InvalidOperationException">The current method is generic, but is not a generic method definition. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
	public void SetReturnType(Type returnType)
	{
		rtype = returnType;
	}

	/// <summary>Sets the number and types of parameters for a method. </summary>
	/// <param name="parameterTypes">An array of <see cref="T:System.Type" /> objects representing the parameter types.</param>
	/// <exception cref="T:System.InvalidOperationException">The current method is generic, but is not a generic method definition. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
	public void SetParameters(params Type[] parameterTypes)
	{
		if (parameterTypes == null)
		{
			return;
		}
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

	/// <summary>Sets the method signature, including the return type, the parameter types, and the required and optional custom modifiers of the return type and parameter types.</summary>
	/// <param name="returnType">The return type of the method.</param>
	/// <param name="returnTypeRequiredCustomModifiers">An array of types representing the required custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no required custom modifiers, specify null.</param>
	/// <param name="returnTypeOptionalCustomModifiers">An array of types representing the optional custom modifiers, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />, for the return type of the method. If the return type has no optional custom modifiers, specify null.</param>
	/// <param name="parameterTypes">The types of the parameters of the method.</param>
	/// <param name="parameterTypeRequiredCustomModifiers">An array of arrays of types. Each array of types represents the required custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no required custom modifiers, specify null instead of an array of types. If none of the parameters have required custom modifiers, specify null instead of an array of arrays.</param>
	/// <param name="parameterTypeOptionalCustomModifiers">An array of arrays of types. Each array of types represents the optional custom modifiers for the corresponding parameter, such as <see cref="T:System.Runtime.CompilerServices.IsConst" />. If a particular parameter has no optional custom modifiers, specify null instead of an array of types. If none of the parameters have optional custom modifiers, specify null instead of an array of arrays.</param>
	/// <exception cref="T:System.InvalidOperationException">The current method is generic, but is not a generic method definition. That is, the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethod" /> property is true, but the <see cref="P:System.Reflection.Emit.MethodBuilder.IsGenericMethodDefinition" /> property is false.</exception>
	public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
	{
		SetReturnType(returnType);
		SetParameters(parameterTypes);
		returnModReq = returnTypeRequiredCustomModifiers;
		returnModOpt = returnTypeOptionalCustomModifiers;
		paramModReq = parameterTypeRequiredCustomModifiers;
		paramModOpt = parameterTypeOptionalCustomModifiers;
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodBuilder.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DispIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _MethodBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	internal MethodBuilder()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}

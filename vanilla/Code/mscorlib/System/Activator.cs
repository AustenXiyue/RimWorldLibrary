using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Security;
using System.Security.Policy;
using System.Threading;
using Unity;

namespace System;

/// <summary>Contains methods to create types of objects locally or remotely, or obtain references to existing remote objects. This class cannot be inherited. </summary>
/// <filterpriority>2</filterpriority>
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_Activator))]
public sealed class Activator : _Activator
{
	internal const int LookupMask = 255;

	internal const BindingFlags ConLookup = BindingFlags.Instance | BindingFlags.Public;

	internal const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

	private Activator()
	{
	}

	/// <summary>Creates an instance of the specified type using the constructor that best matches the specified parameters.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="type" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="type" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="type" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported. -or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />.-or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
	{
		return CreateInstance(type, bindingAttr, binder, args, culture, null);
	}

	/// <summary>Creates an instance of the specified type using the constructor that best matches the specified parameters.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="type" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="type" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="type" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />.-or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (type is TypeBuilder)
		{
			throw new NotSupportedException(Environment.GetResourceString("CreateInstance cannot be used with an object of type TypeBuilder."));
		}
		if ((bindingAttr & (BindingFlags)255) == 0)
		{
			bindingAttr |= BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;
		}
		if (activationAttributes != null && activationAttributes.Length != 0)
		{
			if (!type.IsMarshalByRef)
			{
				throw new NotSupportedException(Environment.GetResourceString("Activation Attributes are not supported for types not deriving from MarshalByRefObject."));
			}
			if (!type.IsContextful && (activationAttributes.Length > 1 || !(activationAttributes[0] is UrlAttribute)))
			{
				throw new NotSupportedException(Environment.GetResourceString("UrlAttribute is the only attribute supported for MarshalByRefObject."));
			}
		}
		RuntimeType obj = type.UnderlyingSystemType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "type");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return obj.CreateInstanceImpl(bindingAttr, binder, args, culture, activationAttributes, ref stackMark);
	}

	/// <summary>Creates an instance of the specified type using the constructor that best matches the specified parameters.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported. -or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />.-or-The constructor that best matches <paramref name="args" /> has varargs arguments. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.No matching public constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	public static object CreateInstance(Type type, params object[] args)
	{
		return CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null, null);
	}

	/// <summary>Creates an instance of the specified type using the constructor that best matches the specified parameters.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />.-or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	public static object CreateInstance(Type type, object[] args, object[] activationAttributes)
	{
		return CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null, activationAttributes);
	}

	/// <summary>Creates an instance of the specified type using that type's default constructor.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.No matching public constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public static object CreateInstance(Type type)
	{
		return CreateInstance(type, nonPublic: false);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. For more information, see the Remarks section. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">You cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static ObjectHandle CreateInstance(string assemblyName, string typeName)
	{
		if (assemblyName == null)
		{
			assemblyName = Assembly.GetCallingAssembly().GetName().Name;
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return CreateInstance(assemblyName, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, null, null, ref stackMark);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />.-or-<paramref name="activationAttributes" /> is not a <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" />array. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">An error occurred when attempting remote activation in a target specified in <paramref name="activationAttributes" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
	{
		if (assemblyName == null)
		{
			assemblyName = Assembly.GetCallingAssembly().GetName().Name;
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return CreateInstance(assemblyName, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, activationAttributes, null, ref stackMark);
	}

	/// <summary>Creates an instance of the specified type using that type's default constructor.</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <param name="type">The type of object to create. </param>
	/// <param name="nonPublic">true if a public or nonpublic default constructor can match; false if only a public default constructor can match. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a RuntimeType. -or-<paramref name="type" /> is an open generic type (that is, the <see cref="P:System.Type.ContainsGenericParameters" /> property returns true).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="type" /> cannot be a <see cref="T:System.Reflection.Emit.TypeBuilder" />.-or- Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported. -or-The assembly that contains <paramref name="type" /> is a dynamic assembly that was created with <see cref="F:System.Reflection.Emit.AssemblyBuilderAccess.Save" />.</exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor being called throws an exception. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">
	///   <paramref name="type" /> is a COM object but the class identifier used to obtain the type is invalid, or the identified class is not registered. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="type" /> is not a valid type. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static object CreateInstance(Type type, bool nonPublic)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		RuntimeType obj = type.UnderlyingSystemType as RuntimeType;
		if (obj == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Type must be a type provided by the runtime."), "type");
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return obj.CreateInstanceDefaultCtor(!nonPublic, skipCheckThis: false, fillCache: true, ref stackMark);
	}

	/// <summary>Creates an instance of the type designated by the specified generic type parameter, using the parameterless constructor .</summary>
	/// <returns>A reference to the newly created object.</returns>
	/// <typeparam name="T">The type to create.</typeparam>
	/// <exception cref="T:System.MissingMethodException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MissingMemberException" />, instead.The type that is specified for <paramref name="T" /> does not have a parameterless constructor. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static T CreateInstance<T>()
	{
		RuntimeType obj = typeof(T) as RuntimeType;
		if (obj.HasElementType)
		{
			throw new MissingMethodException(Environment.GetResourceString("No parameterless constructor defined for this object."));
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return (T)obj.CreateInstanceDefaultCtor(publicOnly: true, skipCheckThis: true, fillCache: true, ref stackMark);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly file and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
	{
		return CreateInstanceFrom(assemblyFile, typeName, null);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly file and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
	{
		return CreateInstanceFrom(assemblyFile, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, activationAttributes);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <param name="securityInfo">Information used to make security policy decisions and grant code permissions. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, RemotingConfiguration" />
	/// </PermissionSet>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
	public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
	{
		if (assemblyName == null)
		{
			assemblyName = Assembly.GetCallingAssembly().GetName().Name;
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityInfo, ref stackMark);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SecuritySafeCritical]
	public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		if (assemblyName == null)
		{
			assemblyName = Assembly.GetCallingAssembly().GetName().Name;
		}
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null, ref stackMark);
	}

	[SecurityCritical]
	internal static ObjectHandle CreateInstance(string assemblyString, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo, ref StackCrawlMark stackMark)
	{
		Type type = null;
		Assembly assembly = null;
		if (assemblyString == null)
		{
			assembly = RuntimeAssembly.GetExecutingAssembly(ref stackMark);
		}
		else
		{
			RuntimeAssembly assemblyFromResolveEvent;
			AssemblyName assemblyName = RuntimeAssembly.CreateAssemblyName(assemblyString, forIntrospection: false, out assemblyFromResolveEvent);
			if (assemblyFromResolveEvent != null)
			{
				assembly = assemblyFromResolveEvent;
			}
			else if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
			{
				type = Type.GetType(typeName + ", " + assemblyString, throwOnError: true, ignoreCase);
			}
			else
			{
				assembly = RuntimeAssembly.InternalLoadAssemblyName(assemblyName, securityInfo, null, ref stackMark, throwOnFileNotFound: true, forIntrospection: false, suppressSecurityChecks: false);
			}
		}
		if (type == null)
		{
			if (assembly == null)
			{
				return null;
			}
			type = assembly.GetType(typeName, throwOnError: true, ignoreCase);
		}
		object obj = CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
		if (obj == null)
		{
			return null;
		}
		return new ObjectHandle(obj);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly file and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <param name="securityInfo">Information used to make security policy decisions and grant code permissions. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
	public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
	{
		return CreateInstanceFromInternal(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityInfo);
	}

	/// <summary>Creates an instance of the type whose name is specified, using the named assembly file and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		return CreateInstanceFromInternal(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
	}

	private static ObjectHandle CreateInstanceFromInternal(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
	{
		object obj = CreateInstance(Assembly.LoadFrom(assemblyFile, securityInfo).GetType(typeName, throwOnError: true, ignoreCase), bindingAttr, binder, args, culture, activationAttributes);
		if (obj == null)
		{
			return null;
		}
		return new ObjectHandle(obj);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The remote domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> or <paramref name="domain" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract type. -or-This member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0. </exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	[SecurityCritical]
	public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <param name="securityAttributes">Information used to make security policy decisions and grant code permissions. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="domain" /> or <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0. </exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	[SecurityCritical]
	[Obsolete("Methods which use evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstance which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
	public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyName">The name of the assembly where the type named <paramref name="typeName" /> is sought. If <paramref name="assemblyName" /> is null, the executing assembly is searched. </param>
	/// <param name="typeName">The fully qualified name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="domain" /> or <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.InvalidComObjectException">The COM type was not obtained through <see cref="Overload:System.Type.GetTypeFromProgID" /> or <see cref="Overload:System.Type.GetTypeFromCLSID" />. </exception>
	/// <exception cref="T:System.NotSupportedException">Creation of <see cref="T:System.TypedReference" />, <see cref="T:System.ArgIterator" />, <see cref="T:System.Void" />, and <see cref="T:System.RuntimeArgumentHandle" /> types, or arrays of those types, is not supported.-or- <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. -or-The constructor that best matches <paramref name="args" /> has varargs arguments.</exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	/// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. -or-The assembly name or code base is invalid. </exception>
	[SecurityCritical]
	public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly file and default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The remote domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="domain" /> or <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	[SecurityCritical]
	public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly file and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The remote domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.  </param>
	/// <param name="securityAttributes">Information used to make security policy decisions and grant code permissions. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="domain" /> or <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-The common language runtime (CLR) version 2.0 or later is currently loaded, and <paramref name="assemblyName" /> was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	[Obsolete("Methods which use Evidence to sandbox are obsolete and will be removed in a future release of the .NET Framework. Please use an overload of CreateInstanceFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
	[SecurityCritical]
	public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
	}

	/// <summary>Creates an instance of the type whose name is specified in the specified remote domain, using the named assembly file and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="domain">The remote domain where the type named <paramref name="typeName" /> is created.</param>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="ignoreCase">true to specify that the search for <paramref name="typeName" /> is not case-sensitive; false to specify that the search is case-sensitive. </param>
	/// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
	/// <param name="binder">An object that uses <paramref name="bindingAttr" /> and <paramref name="args" /> to seek and identify the <paramref name="typeName" /> constructor. If <paramref name="binder" /> is null, the default binder is used. </param>
	/// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args" /> is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked. </param>
	/// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args" /> to the formal types declared for the <paramref name="typeName" /> constructor. If <paramref name="culture" /> is null, the <see cref="T:System.Globalization.CultureInfo" /> for the current thread is used. </param>
	/// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="domain" /> or <paramref name="typeName" /> is null. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.TypeLoadException">
	///   <paramref name="typename" /> was not found in <paramref name="assemblyFile" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> was not found. </exception>
	/// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does have the required <see cref="T:System.Security.Permissions.FileIOPermission" />. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="activationAttributes" /> is not an empty array, and the type being created does not derive from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. -or-<paramref name="assemblyName" /> was compiled for a version of the common language runtime that is later than the version that is currently loaded. </exception>
	[SecurityCritical]
	public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, null);
	}

	/// <summary>Creates an instance of the COM object whose name is specified, using the named assembly file and the constructor that best matches the specified parameters.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> or <paramref name="assemblyName" /> is null. </exception>
	/// <exception cref="T:System.TypeLoadException">An instance cannot be created through COM. -or-<paramref name="typename" /> was not found in <paramref name="assemblyName" />.</exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> is not found, or the module you are trying to load does not specify a file name extension. </exception>
	/// <exception cref="T:System.MemberAccessException">Cannot create an instance of an abstract class.-or-This member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.NotSupportedException">The caller cannot provide activation attributes for an object that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyName" /> is the empty string (""). </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
	{
		return CreateComInstanceFrom(assemblyName, typeName, null, AssemblyHashAlgorithm.None);
	}

	/// <summary>Creates an instance of the COM object whose name is specified, using the named assembly file and the default constructor.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <param name="assemblyName">The name of a file that contains an assembly where the type named <paramref name="typeName" /> is sought. </param>
	/// <param name="typeName">The name of the preferred type. </param>
	/// <param name="hashValue">The value of the computed hash code. </param>
	/// <param name="hashAlgorithm">The hash algorithm used for hashing files and generating the strong name. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeName" /> or <paramref name="assemblyName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyName" /> is the empty string (""). </exception>
	/// <exception cref="T:System.IO.PathTooLongException">An assembly or module was loaded twice with two different evidences, or the assembly name is longer than MAX_PATH characters. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyName" /> is not found, or the module you are trying to load does not specify a file name extension. </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	///   <paramref name="assemblyName" /> is found but cannot be loaded. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyName" /> is not a valid assembly. </exception>
	/// <exception cref="T:System.Security.SecurityException">A code base that does not start with "file://" was specified without the required WebPermission. </exception>
	/// <exception cref="T:System.TypeLoadException">An instance cannot be created through COM.-or- <paramref name="typename" /> was not found in <paramref name="assemblyName" />. </exception>
	/// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
	/// <exception cref="T:System.MemberAccessException">An instance of an abstract class cannot be created. -or-This member was invoked with a late-binding mechanism. </exception>
	/// <exception cref="T:System.NotSupportedException">The caller cannot provide activation attributes for an object that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
	{
		Assembly assembly = Assembly.LoadFrom(assemblyName, hashValue, hashAlgorithm);
		Type type = assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
		object[] customAttributes = type.GetCustomAttributes(typeof(ComVisibleAttribute), inherit: false);
		if (customAttributes.Length != 0 && !((ComVisibleAttribute)customAttributes[0]).Value)
		{
			throw new TypeLoadException(Environment.GetResourceString("The specified type must be visible from COM."));
		}
		if (assembly == null)
		{
			return null;
		}
		object obj = CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, null);
		if (obj == null)
		{
			return null;
		}
		return new ObjectHandle(obj);
	}

	/// <summary>Creates a proxy for the well-known object indicated by the specified type and URL.</summary>
	/// <returns>A proxy that points to an endpoint served by the requested well-known object.</returns>
	/// <param name="type">The type of the well-known object to which you want to connect. </param>
	/// <param name="url">The URL of the well-known object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="url" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="type" /> is not marshaled by reference and is not an interface.</exception>
	/// <exception cref="T:System.MemberAccessException">This member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object GetObject(Type type, string url)
	{
		return GetObject(type, url, null);
	}

	/// <summary>Creates a proxy for the well-known object indicated by the specified type, URL, and channel data.</summary>
	/// <returns>A proxy that points to an endpoint served by the requested well-known object.</returns>
	/// <param name="type">The type of the well-known object to which you want to connect. </param>
	/// <param name="url">The URL of the well-known object. </param>
	/// <param name="state">Channel-specific data or null. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> or <paramref name="url" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Remoting.RemotingException">
	///   <paramref name="type" /> is not marshaled by reference and is not an interface.</exception>
	/// <exception cref="T:System.MemberAccessException">This member was invoked with a late-binding mechanism. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object GetObject(Type type, string url, object state)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return RemotingServices.Connect(type, url, state);
	}

	[Conditional("_DEBUG")]
	private static void Log(bool test, string title, string success, string failure)
	{
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">When this method returns, contains a pointer to a location that receives the number of type information interfaces provided by the object. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Activator.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">An object that receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Activator.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">The passed-in array of names to map.</param>
	/// <param name="cNames">The count of the names to map.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">The caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Activator.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">A dispatch identifier that identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">A pointer to a structure that contains an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">A pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">A pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Activator.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates an instance of the type designated by the specified <see cref="T:System.ActivationContext" /> object.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created object.</returns>
	/// <param name="activationContext">An activation context object that specifies the object to create.</param>
	[SecuritySafeCritical]
	public static ObjectHandle CreateInstance(ActivationContext activationContext)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Creates an instance of the type that is designated by the specified <see cref="T:System.ActivationContext" /> object and activated with the specified custom activation data.</summary>
	/// <returns>A handle that must be unwrapped to access the newly created object.</returns>
	/// <param name="activationContext">An activation context object that specifies the object to create.</param>
	/// <param name="activationCustomData">An array of Unicode strings that contain custom activation data.</param>
	[SecuritySafeCritical]
	public static ObjectHandle CreateInstance(ActivationContext activationContext, string[] activationCustomData)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}
}

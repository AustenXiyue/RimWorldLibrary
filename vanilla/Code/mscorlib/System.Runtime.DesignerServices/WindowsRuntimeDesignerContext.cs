using System.Collections.Generic;
using System.Reflection;
using System.Security;
using Unity;

namespace System.Runtime.DesignerServices;

/// <summary>Provides customized assembly binding for designers that are used to create Windows Store apps.</summary>
public sealed class WindowsRuntimeDesignerContext
{
	/// <summary>Gets the name of the designer binding context. </summary>
	/// <returns>The name of the context. </returns>
	public string Name
	{
		get
		{
			ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.DesignerServices.WindowsRuntimeDesignerContext" /> class, specifying the set of paths to search for third-party Windows Runtime types and for managed assemblies, and specifying the name of the context. </summary>
	/// <param name="paths">The paths to search. </param>
	/// <param name="name">The name of the context. </param>
	/// <exception cref="T:System.NotSupportedException">The current application domain is not the default application domain. -or-The process is not running in the app container. -or-The computer does not have a developer license. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="paths" /> is null. </exception>
	[SecurityCritical]
	public WindowsRuntimeDesignerContext(IEnumerable<string> paths, string name)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Loads the specified assembly from the current context. </summary>
	/// <returns>The assembly, if it is found in the current context; otherwise, null. </returns>
	/// <param name="assemblyName">The full name of the assembly to load. For a description of full assembly names, see the <see cref="P:System.Reflection.Assembly.FullName" /> property. </param>
	[SecurityCritical]
	public Assembly GetAssembly(string assemblyName)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Loads the specified type from the current context. </summary>
	/// <returns>The type, if it is found in the current context; otherwise, null. </returns>
	/// <param name="typeName">The assembly-qualified name of the type to load. For a description of assembly-qualified names, see the <see cref="P:System.Type.AssemblyQualifiedName" /> property. </param>
	[SecurityCritical]
	public Type GetType(string typeName)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Creates a context and sets it as the shared context. </summary>
	/// <param name="paths">An enumerable collection of paths that are used to resolve binding requests that cannot be satisfied by the iteration context. </param>
	/// <exception cref="T:System.NotSupportedException">The shared context has already been set in this application domain. -or-The current application domain is not the default application domain.-or-The process is not running in the app container. -or-The computer does not have a developer license. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="paths" /> is null.</exception>
	[SecurityCritical]
	public static void InitializeSharedContext(IEnumerable<string> paths)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Sets a context to handle iterations of assembly binding requests, as assemblies are recompiled during the design process. </summary>
	/// <param name="context">The context that handles iterations of assembly binding requests. </param>
	/// <exception cref="T:System.NotSupportedException">The current application domain is not the default application domain. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	[SecurityCritical]
	public static void SetIterationContext(WindowsRuntimeDesignerContext context)
	{
		ThrowStub.ThrowNotSupportedException();
	}
}

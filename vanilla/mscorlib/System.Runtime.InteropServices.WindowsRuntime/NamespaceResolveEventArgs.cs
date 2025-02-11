using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>Provides data for the <see cref="E:System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve" /> event.</summary>
[ComVisible(false)]
public class NamespaceResolveEventArgs : EventArgs
{
	/// <summary>Gets the name of the namespace to resolve. </summary>
	/// <returns>The name of the namespace to resolve. </returns>
	public string NamespaceName { get; private set; }

	/// <summary>Gets the name of the assembly whose dependency is being resolved. </summary>
	/// <returns>The name of the assembly whose dependency is being resolved. </returns>
	public Assembly RequestingAssembly { get; private set; }

	/// <summary>Gets a collection of assemblies; when the event handler for the <see cref="E:System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve" /> event is invoked, the collection is empty, and the event handler is responsible for adding the necessary assemblies. </summary>
	/// <returns>A collection of assemblies that define the requested namespace. </returns>
	public Collection<Assembly> ResolvedAssemblies { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.WindowsRuntime.NamespaceResolveEventArgs" /> class, specifying the namespace to resolve and the assembly whose dependency is being resolved. </summary>
	/// <param name="namespaceName">The namespace to resolve. </param>
	/// <param name="requestingAssembly">The assembly whose dependency is being resolved. </param>
	public NamespaceResolveEventArgs(string namespaceName, Assembly requestingAssembly)
	{
		NamespaceName = namespaceName;
		RequestingAssembly = requestingAssembly;
		ResolvedAssemblies = new Collection<Assembly>();
	}
}

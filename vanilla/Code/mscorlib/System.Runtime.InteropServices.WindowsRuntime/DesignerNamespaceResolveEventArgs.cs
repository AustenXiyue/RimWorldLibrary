using System.Collections.ObjectModel;

namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>Provides data for the <see cref="E:System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMetadata.DesignerNamespaceResolve" /> event. </summary>
[ComVisible(false)]
public class DesignerNamespaceResolveEventArgs : EventArgs
{
	/// <summary>Gets the name of the namespace to resolve. </summary>
	/// <returns>The name of the namespace to resolve. </returns>
	public string NamespaceName { get; private set; }

	/// <summary>Gets a collection of assembly file paths; when the event handler for the <see cref="E:System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMetadata.DesignerNamespaceResolve" /> event is invoked, the collection is empty, and the event handler is responsible for adding the necessary assembly files. </summary>
	/// <returns>A collection of assembly files that define the requested namespace. </returns>
	public Collection<string> ResolvedAssemblyFiles { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.WindowsRuntime.DesignerNamespaceResolveEventArgs" /> class. </summary>
	/// <param name="namespaceName">The name of the namespace to resolve. </param>
	public DesignerNamespaceResolveEventArgs(string namespaceName)
	{
		NamespaceName = namespaceName;
		ResolvedAssemblyFiles = new Collection<string>();
	}
}

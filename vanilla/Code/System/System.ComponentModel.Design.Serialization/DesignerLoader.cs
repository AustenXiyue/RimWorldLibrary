using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides a basic designer loader interface that can be used to implement a custom designer loader.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public abstract class DesignerLoader
{
	/// <summary>Gets a value indicating whether the loader is currently loading a document.</summary>
	/// <returns>true if the loader is currently loading a document; otherwise, false.</returns>
	public virtual bool Loading => false;

	/// <summary>Begins loading a designer.</summary>
	/// <param name="host">The loader host through which this loader loads components. </param>
	public abstract void BeginLoad(IDesignerLoaderHost host);

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Design.Serialization.DesignerLoader" />.</summary>
	public abstract void Dispose();

	/// <summary>Writes cached changes to the location that the designer was loaded from.</summary>
	public virtual void Flush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.DesignerLoader" /> class. </summary>
	protected DesignerLoader()
	{
	}
}

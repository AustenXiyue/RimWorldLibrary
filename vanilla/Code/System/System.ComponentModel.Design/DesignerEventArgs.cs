using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerCreated" /> and <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerDisposed" /> events.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public class DesignerEventArgs : EventArgs
{
	private readonly IDesignerHost host;

	/// <summary>Gets the host of the document.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> of the document.</returns>
	public IDesignerHost Designer => host;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerEventArgs" /> class.</summary>
	/// <param name="host">The <see cref="T:System.ComponentModel.Design.IDesignerHost" /> of the document. </param>
	public DesignerEventArgs(IDesignerHost host)
	{
		this.host = host;
	}
}

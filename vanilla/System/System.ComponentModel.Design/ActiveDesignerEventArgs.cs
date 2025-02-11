using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="P:System.ComponentModel.Design.IDesignerEventService.ActiveDesigner" /> event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class ActiveDesignerEventArgs : EventArgs
{
	private readonly IDesignerHost oldDesigner;

	private readonly IDesignerHost newDesigner;

	/// <summary>Gets the document that is losing activation.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that represents the document losing activation.</returns>
	public IDesignerHost OldDesigner => oldDesigner;

	/// <summary>Gets the document that is gaining activation.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.Design.IDesignerHost" /> that represents the document gaining activation.</returns>
	public IDesignerHost NewDesigner => newDesigner;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ActiveDesignerEventArgs" /> class.</summary>
	/// <param name="oldDesigner">The document that is losing activation. </param>
	/// <param name="newDesigner">The document that is gaining activation. </param>
	public ActiveDesignerEventArgs(IDesignerHost oldDesigner, IDesignerHost newDesigner)
	{
		this.oldDesigner = oldDesigner;
		this.newDesigner = newDesigner;
	}
}

using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentAdded" />, <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentAdding" />, <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentRemoved" />, and <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentRemoving" /> events.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class ComponentEventArgs : EventArgs
{
	private IComponent component;

	/// <summary>Gets the component associated with the event.</summary>
	/// <returns>The component associated with the event.</returns>
	public virtual IComponent Component => component;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ComponentEventArgs" /> class.</summary>
	/// <param name="component">The component that is the source of the event. </param>
	public ComponentEventArgs(IComponent component)
	{
		this.component = component;
	}
}

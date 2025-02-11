using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging" /> event. This class cannot be inherited.</summary>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public sealed class ComponentChangingEventArgs : EventArgs
{
	private object component;

	private MemberDescriptor member;

	/// <summary>Gets the component that is about to be changed or the component that is the parent container of the member that is about to be changed.</summary>
	/// <returns>The component that is about to have a member changed.</returns>
	public object Component => component;

	/// <summary>Gets the member that is about to be changed.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.MemberDescriptor" /> indicating the member that is about to be changed, if known, or null otherwise.</returns>
	public MemberDescriptor Member => member;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.ComponentChangingEventArgs" /> class.</summary>
	/// <param name="component">The component that is about to be changed. </param>
	/// <param name="member">A <see cref="T:System.ComponentModel.MemberDescriptor" /> indicating the member of the component that is about to be changed. </param>
	public ComponentChangingEventArgs(object component, MemberDescriptor member)
	{
		this.component = component;
		this.member = member;
	}
}

using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents the method that will handle a <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged" /> event.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">A <see cref="T:System.ComponentModel.Design.ComponentChangedEventArgs" /> that contains the event data. </param>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void ComponentChangedEventHandler(object sender, ComponentChangedEventArgs e);

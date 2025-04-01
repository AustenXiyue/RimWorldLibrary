using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents the method that will handle the <see cref="E:System.ComponentModel.Design.IDesignerEventService.ActiveDesignerChanged" /> event.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">An <see cref="T:System.ComponentModel.Design.ActiveDesignerEventArgs" /> that contains the event data. </param>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void ActiveDesignerEventHandler(object sender, ActiveDesignerEventArgs e);

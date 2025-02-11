using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents the method that will handle the <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerCreated" /> and <see cref="E:System.ComponentModel.Design.IDesignerEventService.DesignerDisposed" /> events that are raised when a document is created or disposed of.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">A <see cref="T:System.ComponentModel.Design.DesignerEventArgs" /> that contains the event data. </param>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void DesignerEventHandler(object sender, DesignerEventArgs e);

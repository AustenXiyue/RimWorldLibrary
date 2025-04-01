using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Represents the method that handles the <see cref="E:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.ResolveName" /> event of a serialization manager.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">A <see cref="T:System.ComponentModel.Design.Serialization.ResolveNameEventArgs" />  that contains the event data.</param>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void ResolveNameEventHandler(object sender, ResolveNameEventArgs e);

using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Forms.BindingSource.AddingNew" /> event.</summary>
/// <param name="sender">The source of the event, typically a data container or data-bound collection. </param>
/// <param name="e">A <see cref="T:System.ComponentModel.AddingNewEventArgs" /> that contains the event data. </param>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public delegate void AddingNewEventHandler(object sender, AddingNewEventArgs e);

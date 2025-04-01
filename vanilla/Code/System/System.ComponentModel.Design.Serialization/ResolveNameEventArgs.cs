using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Design.Serialization.IDesignerSerializationManager.ResolveName" /> event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class ResolveNameEventArgs : EventArgs
{
	private string name;

	private object value;

	/// <summary>Gets the name of the object to resolve.</summary>
	/// <returns>The name of the object to resolve.</returns>
	public string Name => name;

	/// <summary>Gets or sets the object that matches the name.</summary>
	/// <returns>The object that the name is associated with.</returns>
	public object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.ResolveNameEventArgs" /> class.</summary>
	/// <param name="name">The name to resolve. </param>
	public ResolveNameEventArgs(string name)
	{
		this.name = name;
		value = null;
	}
}

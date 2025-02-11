using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides data for the <see cref="E:System.Windows.Forms.BindingSource.AddingNew" /> event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class AddingNewEventArgs : EventArgs
{
	private object newObject;

	/// <summary>Gets or sets the object to be added to the binding list. </summary>
	/// <returns>The <see cref="T:System.Object" /> to be added as a new item to the associated collection. </returns>
	public object NewObject
	{
		get
		{
			return newObject;
		}
		set
		{
			newObject = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AddingNewEventArgs" /> class using no parameters.</summary>
	public AddingNewEventArgs()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.AddingNewEventArgs" /> class using the specified object as the new item.</summary>
	/// <param name="newObject">An <see cref="T:System.Object" /> to use as the new item value.</param>
	public AddingNewEventArgs(object newObject)
	{
		this.newObject = newObject;
	}
}

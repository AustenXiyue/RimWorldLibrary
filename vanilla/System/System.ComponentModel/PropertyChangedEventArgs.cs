using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class PropertyChangedEventArgs : EventArgs
{
	private readonly string propertyName;

	/// <summary>Gets the name of the property that changed.</summary>
	/// <returns>The name of the property that changed.</returns>
	public virtual string PropertyName => propertyName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> class.</summary>
	/// <param name="propertyName">The name of the property that changed. </param>
	public PropertyChangedEventArgs(string propertyName)
	{
		this.propertyName = propertyName;
	}
}

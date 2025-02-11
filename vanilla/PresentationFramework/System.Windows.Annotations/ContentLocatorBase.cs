using System.ComponentModel;
using MS.Internal.Annotations;

namespace System.Windows.Annotations;

/// <summary>Represents an object that identifies an item of content.</summary>
public abstract class ContentLocatorBase : INotifyPropertyChanged2, INotifyPropertyChanged, IOwnedObject
{
	private bool _owned;

	bool IOwnedObject.Owned
	{
		get
		{
			return _owned;
		}
		set
		{
			_owned = value;
		}
	}

	/// <summary>For a description of this member, see <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged += value;
		}
		remove
		{
			_propertyChanged -= value;
		}
	}

	private event PropertyChangedEventHandler _propertyChanged;

	internal ContentLocatorBase()
	{
	}

	/// <summary>Creates a modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorBase" />.</summary>
	/// <returns>A modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorBase" />.</returns>
	public abstract object Clone();

	internal void FireLocatorChanged(string name)
	{
		if (this._propertyChanged != null)
		{
			this._propertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

	internal abstract ContentLocatorBase Merge(ContentLocatorBase other);
}

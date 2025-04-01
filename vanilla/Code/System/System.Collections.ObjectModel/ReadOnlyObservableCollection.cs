using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel;

/// <summary>Represents a read-only <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" />.</summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
[Serializable]
[TypeForwardedFrom("WindowsBase, Version=3.0.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
public class ReadOnlyObservableCollection<T> : ReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	/// <summary>Occurs when the collection changes.</summary>
	event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			CollectionChanged += value;
		}
		remove
		{
			CollectionChanged -= value;
		}
	}

	/// <summary>Occurs when an item is added or removed.</summary>
	protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyObservableCollection`1" /> class that serves as a wrapper around the specified <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" />.</summary>
	/// <param name="list">The <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> with which to create this instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyObservableCollection`1" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="list" /> is null.</exception>
	public ReadOnlyObservableCollection(ObservableCollection<T> list)
		: base((IList<T>)list)
	{
		((INotifyCollectionChanged)base.Items).CollectionChanged += HandleCollectionChanged;
		((INotifyPropertyChanged)base.Items).PropertyChanged += HandlePropertyChanged;
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ReadOnlyObservableCollection`1.CollectionChanged" /> event using the provided arguments.</summary>
	/// <param name="args">Arguments of the event being raised.</param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, args);
		}
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ReadOnlyObservableCollection`1.PropertyChanged" /> event using the provided arguments.</summary>
	/// <param name="args">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, args);
		}
	}

	private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		OnCollectionChanged(e);
	}

	private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnPropertyChanged(e);
	}
}

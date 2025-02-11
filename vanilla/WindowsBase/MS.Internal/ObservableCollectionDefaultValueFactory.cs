using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class ObservableCollectionDefaultValueFactory<T> : DefaultValueFactory
{
	private class ObservableCollectionDefaultPromoter
	{
		private readonly DependencyObject _owner;

		private readonly DependencyProperty _property;

		private readonly ObservableCollection<T> _collection;

		internal ObservableCollectionDefaultPromoter(DependencyObject owner, DependencyProperty property, ObservableCollection<T> collection)
		{
			_owner = owner;
			_property = property;
			_collection = collection;
			_collection.CollectionChanged += OnDefaultValueChanged;
		}

		internal void OnDefaultValueChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_property.GetMetadata(_owner.DependencyObjectType).ClearCachedDefaultValue(_owner, _property);
			if (_owner.ReadLocalValue(_property) == DependencyProperty.UnsetValue)
			{
				if (_property.ReadOnly)
				{
					_owner.SetValue(_property.DependencyPropertyKey, _collection);
				}
				else
				{
					_owner.SetValue(_property, _collection);
				}
			}
			_collection.CollectionChanged -= OnDefaultValueChanged;
		}
	}

	private ObservableCollection<T> _default;

	internal override object DefaultValue => _default;

	internal ObservableCollectionDefaultValueFactory()
	{
		_default = new ObservableCollection<T>();
	}

	internal override object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		ObservableCollection<T> observableCollection = new ObservableCollection<T>();
		new ObservableCollectionDefaultPromoter(owner, property, observableCollection);
		return observableCollection;
	}
}

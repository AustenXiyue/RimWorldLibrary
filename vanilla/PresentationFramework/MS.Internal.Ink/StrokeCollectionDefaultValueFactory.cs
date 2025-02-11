using System.Windows;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class StrokeCollectionDefaultValueFactory : DefaultValueFactory
{
	private class StrokeCollectionDefaultPromoter
	{
		private readonly DependencyObject _owner;

		private readonly DependencyProperty _dependencyProperty;

		internal StrokeCollectionDefaultPromoter(DependencyObject owner, DependencyProperty property)
		{
			_owner = owner;
			_dependencyProperty = property;
		}

		internal void OnStrokeCollectionChanged<TEventArgs>(object sender, TEventArgs e)
		{
			StrokeCollection strokeCollection = (StrokeCollection)sender;
			strokeCollection.StrokesChanged -= OnStrokeCollectionChanged;
			strokeCollection.PropertyDataChanged -= OnStrokeCollectionChanged;
			if (_owner.ReadLocalValue(_dependencyProperty) == DependencyProperty.UnsetValue)
			{
				_owner.SetValue(_dependencyProperty, strokeCollection);
			}
			_dependencyProperty.GetMetadata(_owner.DependencyObjectType).ClearCachedDefaultValue(_owner, _dependencyProperty);
		}
	}

	internal override object DefaultValue => new StrokeCollection();

	internal StrokeCollectionDefaultValueFactory()
	{
	}

	internal override object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		StrokeCollectionDefaultPromoter @object = new StrokeCollectionDefaultPromoter(owner, property);
		strokeCollection.StrokesChanged += @object.OnStrokeCollectionChanged;
		strokeCollection.PropertyDataChanged += @object.OnStrokeCollectionChanged;
		return strokeCollection;
	}
}

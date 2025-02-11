using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace MS.Internal.Ink;

internal class DrawingAttributesDefaultValueFactory : DefaultValueFactory
{
	private class DrawingAttributesDefaultPromoter
	{
		private readonly InkCanvas _owner;

		internal DrawingAttributesDefaultPromoter(InkCanvas owner)
		{
			_owner = owner;
		}

		internal void OnDrawingAttributesChanged(object sender, PropertyDataChangedEventArgs e)
		{
			DrawingAttributes drawingAttributes = (DrawingAttributes)sender;
			drawingAttributes.AttributeChanged -= OnDrawingAttributesChanged;
			drawingAttributes.PropertyDataChanged -= OnDrawingAttributesChanged;
			if (_owner.ReadLocalValue(InkCanvas.DefaultDrawingAttributesProperty) == DependencyProperty.UnsetValue)
			{
				_owner.SetValue(InkCanvas.DefaultDrawingAttributesProperty, drawingAttributes);
			}
			InkCanvas.DefaultDrawingAttributesProperty.GetMetadata(_owner.DependencyObjectType).ClearCachedDefaultValue(_owner, InkCanvas.DefaultDrawingAttributesProperty);
		}
	}

	internal override object DefaultValue => new DrawingAttributes();

	internal DrawingAttributesDefaultValueFactory()
	{
	}

	internal override object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		DrawingAttributes drawingAttributes = new DrawingAttributes();
		DrawingAttributesDefaultPromoter @object = new DrawingAttributesDefaultPromoter((InkCanvas)owner);
		drawingAttributes.AttributeChanged += @object.OnDrawingAttributesChanged;
		drawingAttributes.PropertyDataChanged += @object.OnDrawingAttributesChanged;
		return drawingAttributes;
	}
}

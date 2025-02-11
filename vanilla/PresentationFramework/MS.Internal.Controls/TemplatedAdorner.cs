using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Controls;

internal sealed class TemplatedAdorner : Adorner
{
	private Control _child;

	private FrameworkElement _referenceElement;

	public FrameworkElement ReferenceElement
	{
		get
		{
			return _referenceElement;
		}
		set
		{
			_referenceElement = value;
		}
	}

	protected override int VisualChildrenCount => (_child != null) ? 1 : 0;

	public void ClearChild()
	{
		RemoveVisualChild(_child);
		_child = null;
	}

	public TemplatedAdorner(UIElement adornedElement, ControlTemplate adornerTemplate)
		: base(adornedElement)
	{
		_child = new Control
		{
			DataContext = Validation.GetErrors(adornedElement),
			IsTabStop = false,
			Template = adornerTemplate
		};
		AddVisualChild(_child);
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		if (ReferenceElement == null)
		{
			return transform;
		}
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		if (transform != null)
		{
			generalTransformGroup.Children.Add(transform);
		}
		GeneralTransform generalTransform = TransformToDescendant(ReferenceElement);
		if (generalTransform != null)
		{
			generalTransformGroup.Children.Add(generalTransform);
		}
		return generalTransformGroup;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (_child == null || index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _child;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (ReferenceElement != null && base.AdornedElement != null && base.AdornedElement.IsMeasureValid && !DoubleUtil.AreClose(ReferenceElement.DesiredSize, base.AdornedElement.DesiredSize))
		{
			ReferenceElement.InvalidateMeasure();
		}
		_child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		return _child.DesiredSize;
	}

	protected override Size ArrangeOverride(Size size)
	{
		Size size2 = base.ArrangeOverride(size);
		if (_child != null)
		{
			_child.Arrange(new Rect(default(Point), size2));
		}
		return size2;
	}

	internal override bool NeedsUpdate(Size oldSize)
	{
		bool result = base.NeedsUpdate(oldSize);
		Visibility visibility = ((!base.AdornedElement.IsVisible) ? Visibility.Collapsed : Visibility.Visible);
		if (visibility != base.Visibility)
		{
			base.Visibility = visibility;
			result = true;
		}
		return result;
	}
}

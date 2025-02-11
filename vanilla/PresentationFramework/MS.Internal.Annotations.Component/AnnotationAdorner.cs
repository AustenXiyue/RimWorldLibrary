using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Annotations.Component;

internal sealed class AnnotationAdorner : Adorner
{
	private IAnnotationComponent _annotationComponent;

	private UIElement _annotatedElement;

	protected override int VisualChildrenCount => (_annotationComponent != null) ? 1 : 0;

	internal IAnnotationComponent AnnotationComponent => _annotationComponent;

	public AnnotationAdorner(IAnnotationComponent component, UIElement annotatedElement)
		: base(annotatedElement)
	{
		if (component is UIElement)
		{
			_annotationComponent = component;
			AddVisualChild((UIElement)_annotationComponent);
			return;
		}
		throw new ArgumentException(SR.AnnotationAdorner_NotUIElement, "component");
	}

	public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		if (!(_annotationComponent is UIElement))
		{
			return null;
		}
		transform = base.GetDesiredTransform(transform);
		GeneralTransform desiredTransform = _annotationComponent.GetDesiredTransform(transform);
		if (_annotationComponent.AnnotatedElement == null)
		{
			return null;
		}
		if (desiredTransform == null)
		{
			_annotatedElement = _annotationComponent.AnnotatedElement;
			_annotatedElement.LayoutUpdated += OnLayoutUpdated;
			return transform;
		}
		return desiredTransform;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index != 0 || _annotationComponent == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return (UIElement)_annotationComponent;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		Size availableSize2 = new Size(double.PositiveInfinity, double.PositiveInfinity);
		Invariant.Assert(_annotationComponent != null, "AnnotationAdorner should only have one child - the annotation component.");
		((UIElement)_annotationComponent).Measure(availableSize2);
		return new Size(0.0, 0.0);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		Invariant.Assert(_annotationComponent != null, "AnnotationAdorner should only have one child - the annotation component.");
		((UIElement)_annotationComponent).Arrange(new Rect(((UIElement)_annotationComponent).DesiredSize));
		return finalSize;
	}

	internal void RemoveChildren()
	{
		RemoveVisualChild((UIElement)_annotationComponent);
		_annotationComponent = null;
	}

	internal void InvalidateTransform()
	{
		AdornerLayer obj = (AdornerLayer)VisualTreeHelper.GetParent(this);
		InvalidateMeasure();
		obj.InvalidateVisual();
	}

	private void OnLayoutUpdated(object sender, EventArgs args)
	{
		_annotatedElement.LayoutUpdated -= OnLayoutUpdated;
		_annotatedElement = null;
		if (_annotationComponent.AttachedAnnotations.Count > 0)
		{
			_annotationComponent.PresentationContext.Host.InvalidateMeasure();
			InvalidateMeasure();
		}
	}
}

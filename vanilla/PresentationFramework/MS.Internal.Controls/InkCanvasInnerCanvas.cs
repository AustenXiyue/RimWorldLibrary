using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MS.Internal.Controls;

internal class InkCanvasInnerCanvas : Panel
{
	private InkCanvas _inkCanvas;

	protected internal override IEnumerator LogicalChildren => EmptyEnumerator.Instance;

	internal IEnumerator PrivateLogicalChildren => base.LogicalChildren;

	internal InkCanvas InkCanvas => _inkCanvas;

	internal InkCanvasInnerCanvas(InkCanvas inkCanvas)
	{
		_inkCanvas = inkCanvas;
	}

	private InkCanvasInnerCanvas()
	{
	}

	protected internal override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
		base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		if (visualRemoved is UIElement removedElement)
		{
			InkCanvas.InkCanvasSelection.RemoveElement(removedElement);
		}
		InkCanvas.RaiseOnVisualChildrenChanged(visualAdded, visualRemoved);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		Size result = default(Size);
		foreach (UIElement internalChild in base.InternalChildren)
		{
			if (internalChild != null)
			{
				internalChild.Measure(availableSize);
				double num = InkCanvas.GetLeft(internalChild);
				if (!double.IsNaN(num))
				{
					result.Width = Math.Max(result.Width, num + internalChild.DesiredSize.Width);
				}
				else
				{
					result.Width = Math.Max(result.Width, internalChild.DesiredSize.Width);
				}
				double num2 = InkCanvas.GetTop(internalChild);
				if (!double.IsNaN(num2))
				{
					result.Height = Math.Max(result.Height, num2 + internalChild.DesiredSize.Height);
				}
				else
				{
					result.Height = Math.Max(result.Height, internalChild.DesiredSize.Height);
				}
			}
		}
		return result;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		foreach (UIElement internalChild in base.InternalChildren)
		{
			if (internalChild == null)
			{
				continue;
			}
			double x = 0.0;
			double y = 0.0;
			double num = InkCanvas.GetLeft(internalChild);
			if (!double.IsNaN(num))
			{
				x = num;
			}
			else
			{
				double num2 = InkCanvas.GetRight(internalChild);
				if (!double.IsNaN(num2))
				{
					x = arrangeSize.Width - internalChild.DesiredSize.Width - num2;
				}
			}
			double num3 = InkCanvas.GetTop(internalChild);
			if (!double.IsNaN(num3))
			{
				y = num3;
			}
			else
			{
				double num4 = InkCanvas.GetBottom(internalChild);
				if (!double.IsNaN(num4))
				{
					y = arrangeSize.Height - internalChild.DesiredSize.Height - num4;
				}
			}
			internalChild.Arrange(new Rect(new Point(x, y), internalChild.DesiredSize));
		}
		return arrangeSize;
	}

	protected override void OnChildDesiredSizeChanged(UIElement child)
	{
		base.OnChildDesiredSizeChanged(child);
		InvalidateMeasure();
	}

	protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
	{
		return base.CreateUIElementCollection(_inkCanvas);
	}

	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		if (base.ClipToBounds)
		{
			return base.GetLayoutClip(layoutSlotSize);
		}
		return null;
	}

	internal UIElement HitTestOnElements(Point point)
	{
		UIElement result = null;
		HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, point);
		if (hitTestResult != null)
		{
			Visual visual = hitTestResult.VisualHit as Visual;
			Visual3D visual3D = hitTestResult.VisualHit as Visual3D;
			DependencyObject dependencyObject = null;
			if (visual != null)
			{
				dependencyObject = visual;
			}
			else if (visual3D != null)
			{
				dependencyObject = visual3D;
			}
			while (dependencyObject != null)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
				if (parent == InkCanvas.InnerCanvas)
				{
					result = dependencyObject as UIElement;
					break;
				}
				dependencyObject = parent;
			}
		}
		return result;
	}
}

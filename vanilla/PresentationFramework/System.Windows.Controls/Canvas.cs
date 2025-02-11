using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Defines an area within which you can explicitly position child elements by using coordinates that are relative to the <see cref="T:System.Windows.Controls.Canvas" /> area.</summary>
public class Canvas : Panel
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Canvas.Left" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Canvas.Left" /> attached property.</returns>
	public static readonly DependencyProperty LeftProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Canvas.Top" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Canvas.Top" /> attached property.</returns>
	public static readonly DependencyProperty TopProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Canvas.Right" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Canvas.Right" /> attached property.</returns>
	public static readonly DependencyProperty RightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Canvas.Bottom" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Canvas.Bottom" /> attached property.</returns>
	public static readonly DependencyProperty BottomProperty;

	internal override int EffectiveValuesInitialSize => 9;

	static Canvas()
	{
		LeftProperty = DependencyProperty.RegisterAttached("Left", typeof(double), typeof(Canvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		TopProperty = DependencyProperty.RegisterAttached("Top", typeof(double), typeof(Canvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		RightProperty = DependencyProperty.RegisterAttached("Right", typeof(double), typeof(Canvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		BottomProperty = DependencyProperty.RegisterAttached("Bottom", typeof(double), typeof(Canvas), new FrameworkPropertyMetadata(double.NaN, OnPositioningChanged), Shape.IsDoubleFiniteOrNaN);
		ControlsTraceLogger.AddControl(TelemetryControls.Canvas);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Canvas" /> class. </summary>
	public Canvas()
	{
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Canvas.Left" /> attached property for a given dependency object. </summary>
	/// <returns>The <see cref="P:System.Windows.Controls.Canvas.Left" /> coordinate of the specified element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetLeft(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(LeftProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Canvas.Left" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="length">Sets the <see cref="P:System.Windows.Controls.Canvas.Left" /> coordinate of the specified element.</param>
	public static void SetLeft(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LeftProperty, length);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Canvas.Top" /> attached property for a given dependency object. </summary>
	/// <returns>The <see cref="P:System.Windows.Controls.Canvas.Top" /> coordinate of the specified element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetTop(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(TopProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Canvas.Top" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="length">Sets the <see cref="P:System.Windows.Controls.Canvas.Top" /> coordinate of the specified element.</param>
	public static void SetTop(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TopProperty, length);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Canvas.Right" /> attached property for a given dependency object. </summary>
	/// <returns>The <see cref="P:System.Windows.Controls.Canvas.Right" /> coordinate of the specified element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetRight(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(RightProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Canvas.Right" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="length">Sets the <see cref="P:System.Windows.Controls.Canvas.Right" /> coordinate of the specified element.</param>
	public static void SetRight(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(RightProperty, length);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.Canvas.Bottom" /> attached property for a given dependency object. </summary>
	/// <returns>The <see cref="P:System.Windows.Controls.Canvas.Bottom" /> coordinate of the specified element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetBottom(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(BottomProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Canvas.Bottom" /> attached property for a given dependency object. </summary>
	/// <param name="element">The element to which the property value is written.</param>
	/// <param name="length">Sets the <see cref="P:System.Windows.Controls.Canvas.Bottom" /> coordinate of the specified element.</param>
	public static void SetBottom(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(BottomProperty, length);
	}

	private static void OnPositioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement reference && VisualTreeHelper.GetParent(reference) is Canvas canvas)
		{
			canvas.InvalidateArrange();
		}
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.Canvas" /> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.Canvas.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that represents the size that is required to arrange child content.</returns>
	/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		foreach (UIElement internalChild in base.InternalChildren)
		{
			internalChild?.Measure(availableSize);
		}
		return default(Size);
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Canvas" /> element.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.Canvas" /> element and its descendants.</returns>
	/// <param name="arrangeSize">The size that this <see cref="T:System.Windows.Controls.Canvas" /> element should use to arrange its child elements.</param>
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
			double left = GetLeft(internalChild);
			if (!double.IsNaN(left))
			{
				x = left;
			}
			else
			{
				double right = GetRight(internalChild);
				if (!double.IsNaN(right))
				{
					x = arrangeSize.Width - internalChild.DesiredSize.Width - right;
				}
			}
			double top = GetTop(internalChild);
			if (!double.IsNaN(top))
			{
				y = top;
			}
			else
			{
				double bottom = GetBottom(internalChild);
				if (!double.IsNaN(bottom))
				{
					y = arrangeSize.Height - internalChild.DesiredSize.Height - bottom;
				}
			}
			internalChild.Arrange(new Rect(new Point(x, y), internalChild.DesiredSize));
		}
		return arrangeSize;
	}

	/// <summary>Returns a clipping geometry that indicates the area that will be clipped if the <see cref="P:System.Windows.UIElement.ClipToBounds" /> property is set to true. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> that represents the area that is clipped if <see cref="P:System.Windows.UIElement.ClipToBounds" /> is true.</returns>
	/// <param name="layoutSlotSize">The available size of the element.</param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		if (base.ClipToBounds)
		{
			return new RectangleGeometry(new Rect(base.RenderSize));
		}
		return null;
	}
}

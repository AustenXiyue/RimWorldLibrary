using System.Collections;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Defines a content decorator that can stretch and scale a single child to fill the available space.</summary>
public class Viewbox : Decorator
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> dependency property.</returns>
	public static readonly DependencyProperty StretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" /> dependency property.</returns>
	public static readonly DependencyProperty StretchDirectionProperty;

	private ContainerVisual _internalVisual;

	private ContainerVisual InternalVisual
	{
		get
		{
			if (_internalVisual == null)
			{
				_internalVisual = new ContainerVisual();
				AddVisualChild(_internalVisual);
			}
			return _internalVisual;
		}
	}

	private UIElement InternalChild
	{
		get
		{
			VisualCollection children = InternalVisual.Children;
			if (children.Count != 0)
			{
				return children[0] as UIElement;
			}
			return null;
		}
		set
		{
			VisualCollection children = InternalVisual.Children;
			if (children.Count != 0)
			{
				children.Clear();
			}
			children.Add(value);
		}
	}

	private Transform InternalTransform
	{
		get
		{
			return InternalVisual.Transform;
		}
		set
		{
			InternalVisual.Transform = value;
		}
	}

	/// <summary>Gets or sets the single child of a <see cref="T:System.Windows.Controls.Viewbox" /> element. </summary>
	/// <returns>The single child of a <see cref="T:System.Windows.Controls.Viewbox" /> element. This property has no default value.</returns>
	public override UIElement Child
	{
		get
		{
			return InternalChild;
		}
		set
		{
			UIElement internalChild = InternalChild;
			if (internalChild != value)
			{
				RemoveLogicalChild(internalChild);
				if (value != null)
				{
					AddLogicalChild(value);
				}
				InternalChild = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets the number of child <see cref="T:System.Windows.Media.Visual" /> objects in this instance of <see cref="T:System.Windows.Controls.Viewbox" />.</summary>
	/// <returns>The number of <see cref="T:System.Windows.Media.Visual" /> children.</returns>
	protected override int VisualChildrenCount => 1;

	/// <summary>Gets an enumerator that can iterate the logical children of this <see cref="T:System.Windows.Controls.Viewbox" /> element.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" />. This property has no default value.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (InternalChild == null)
			{
				return EmptyEnumerator.Instance;
			}
			return new SingleChildEnumerator(InternalChild);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.Viewbox" /> <see cref="T:System.Windows.Media.Stretch" /> mode, which determines how content fits into the available space.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Stretch" /> that determines how content fits in the available space. The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
	public Stretch Stretch
	{
		get
		{
			return (Stretch)GetValue(StretchProperty);
		}
		set
		{
			SetValue(StretchProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.StretchDirection" />, which determines how scaling is applied to the contents of a <see cref="T:System.Windows.Controls.Viewbox" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.StretchDirection" /> that determines how scaling is applied to the contents of a <see cref="T:System.Windows.Controls.Viewbox" />. The default is <see cref="F:System.Windows.Controls.StretchDirection.Both" />.</returns>
	public StretchDirection StretchDirection
	{
		get
		{
			return (StretchDirection)GetValue(StretchDirectionProperty);
		}
		set
		{
			SetValue(StretchDirectionProperty, value);
		}
	}

	static Viewbox()
	{
		StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(Viewbox), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateStretchValue);
		StretchDirectionProperty = DependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(Viewbox), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateStretchDirectionValue);
		ControlsTraceLogger.AddControl(TelemetryControls.ViewBox);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Viewbox" /> class. </summary>
	public Viewbox()
	{
	}

	private static bool ValidateStretchValue(object value)
	{
		Stretch stretch = (Stretch)value;
		if (stretch != Stretch.Uniform && stretch != 0 && stretch != Stretch.Fill)
		{
			return stretch == Stretch.UniformToFill;
		}
		return true;
	}

	private static bool ValidateStretchDirectionValue(object value)
	{
		StretchDirection stretchDirection = (StretchDirection)value;
		if (stretchDirection != StretchDirection.Both && stretchDirection != StretchDirection.DownOnly)
		{
			return stretchDirection == StretchDirection.UpOnly;
		}
		return true;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.Visual" /> child at the specified <paramref name="index" /> position.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> child of the parent <see cref="T:System.Windows.Controls.Viewbox" /> element.</returns>
	/// <param name="index">The index position of the wanted <see cref="T:System.Windows.Media.Visual" /> child.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return InternalVisual;
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.Viewbox" /> prior to arranging them during the <see cref="M:System.Windows.Controls.WrapPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the element size you want.</returns>
	/// <param name="constraint">A <see cref="T:System.Windows.Size" /> limit that <see cref="T:System.Windows.Controls.Viewbox" /> cannot exceed.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		UIElement internalChild = InternalChild;
		Size result = default(Size);
		if (internalChild != null)
		{
			Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			internalChild.Measure(availableSize);
			Size desiredSize = internalChild.DesiredSize;
			Size size = ComputeScaleFactor(constraint, desiredSize, Stretch, StretchDirection);
			result.Width = size.Width * desiredSize.Width;
			result.Height = size.Height * desiredSize.Height;
		}
		return result;
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Viewbox" /> element.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.Viewbox" /> element and its child elements.</returns>
	/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> this element uses to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		UIElement internalChild = InternalChild;
		if (internalChild != null)
		{
			Size desiredSize = internalChild.DesiredSize;
			Size size = ComputeScaleFactor(arrangeSize, desiredSize, Stretch, StretchDirection);
			InternalTransform = new ScaleTransform(size.Width, size.Height);
			internalChild.Arrange(new Rect(default(Point), internalChild.DesiredSize));
			arrangeSize.Width = size.Width * desiredSize.Width;
			arrangeSize.Height = size.Height * desiredSize.Height;
		}
		return arrangeSize;
	}

	internal static Size ComputeScaleFactor(Size availableSize, Size contentSize, Stretch stretch, StretchDirection stretchDirection)
	{
		double num = 1.0;
		double num2 = 1.0;
		bool flag = !double.IsPositiveInfinity(availableSize.Width);
		bool flag2 = !double.IsPositiveInfinity(availableSize.Height);
		if ((stretch == Stretch.Uniform || stretch == Stretch.UniformToFill || stretch == Stretch.Fill) && (flag || flag2))
		{
			num = (DoubleUtil.IsZero(contentSize.Width) ? 0.0 : (availableSize.Width / contentSize.Width));
			num2 = (DoubleUtil.IsZero(contentSize.Height) ? 0.0 : (availableSize.Height / contentSize.Height));
			if (!flag)
			{
				num = num2;
			}
			else if (!flag2)
			{
				num2 = num;
			}
			else
			{
				switch (stretch)
				{
				case Stretch.Uniform:
					num = (num2 = ((num < num2) ? num : num2));
					break;
				case Stretch.UniformToFill:
					num = (num2 = ((num > num2) ? num : num2));
					break;
				}
			}
			switch (stretchDirection)
			{
			case StretchDirection.UpOnly:
				if (num < 1.0)
				{
					num = 1.0;
				}
				if (num2 < 1.0)
				{
					num2 = 1.0;
				}
				break;
			case StretchDirection.DownOnly:
				if (num > 1.0)
				{
					num = 1.0;
				}
				if (num2 > 1.0)
				{
					num2 = 1.0;
				}
				break;
			}
		}
		return new Size(num, num2);
	}
}

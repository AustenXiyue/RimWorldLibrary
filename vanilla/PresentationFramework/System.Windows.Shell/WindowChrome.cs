using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Standard;

namespace System.Windows.Shell;

/// <summary>Represents an object that describes the customizations to the non-client area of a window.</summary>
public class WindowChrome : Freezable
{
	private struct _SystemParameterBoundProperty
	{
		public string SystemParameterPropertyName { get; set; }

		public DependencyProperty DependencyProperty { get; set; }
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.WindowChrome" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.WindowChrome" /> dependency property.</returns>
	public static readonly DependencyProperty WindowChromeProperty = DependencyProperty.RegisterAttached("WindowChrome", typeof(WindowChrome), typeof(WindowChrome), new PropertyMetadata(null, _OnChromeChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> dependency property.</returns>
	public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached("IsHitTestVisibleInChrome", typeof(bool), typeof(WindowChrome), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> dependency property.</returns>
	public static readonly DependencyProperty ResizeGripDirectionProperty = DependencyProperty.RegisterAttached("ResizeGripDirection", typeof(ResizeGripDirection), typeof(WindowChrome), new FrameworkPropertyMetadata(ResizeGripDirection.None, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.CaptionHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.CaptionHeight" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight", typeof(double), typeof(WindowChrome), new PropertyMetadata(0.0, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint();
	}), (object value) => (double)value >= 0.0);

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.ResizeBorderThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.ResizeBorderThickness" /> dependency property.</returns>
	public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register("ResizeBorderThickness", typeof(Thickness), typeof(WindowChrome), new PropertyMetadata(default(Thickness)), (object value) => Utility.IsThicknessNonNegative((Thickness)value));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.GlassFrameThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.GlassFrameThickness" /> dependency property.</returns>
	public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register("GlassFrameThickness", typeof(Thickness), typeof(WindowChrome), new PropertyMetadata(default(Thickness), delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint();
	}, (DependencyObject d, object o) => _CoerceGlassFrameThickness((Thickness)o)));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.UseAeroCaptionButtons" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.UseAeroCaptionButtons" /> dependency property.</returns>
	public static readonly DependencyProperty UseAeroCaptionButtonsProperty = DependencyProperty.Register("UseAeroCaptionButtons", typeof(bool), typeof(WindowChrome), new FrameworkPropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.CornerRadius" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.CornerRadius" /> dependency property.</returns>
	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(WindowChrome), new PropertyMetadata(default(CornerRadius), delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint();
	}), (object value) => Utility.IsCornerRadiusValid((CornerRadius)value));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.WindowChrome.NonClientFrameEdges" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.WindowChrome.NonClientFrameEdges" /> dependency property.</returns>
	public static readonly DependencyProperty NonClientFrameEdgesProperty = DependencyProperty.Register("NonClientFrameEdges", typeof(NonClientFrameEdges), typeof(WindowChrome), new PropertyMetadata(NonClientFrameEdges.None, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint();
	}), _NonClientFrameEdgesAreValid);

	private static readonly NonClientFrameEdges NonClientFrameEdges_All = NonClientFrameEdges.Left | NonClientFrameEdges.Top | NonClientFrameEdges.Right | NonClientFrameEdges.Bottom;

	private static readonly List<_SystemParameterBoundProperty> _BoundProperties = new List<_SystemParameterBoundProperty>
	{
		new _SystemParameterBoundProperty
		{
			DependencyProperty = CornerRadiusProperty,
			SystemParameterPropertyName = "WindowCornerRadius"
		},
		new _SystemParameterBoundProperty
		{
			DependencyProperty = CaptionHeightProperty,
			SystemParameterPropertyName = "WindowCaptionHeight"
		},
		new _SystemParameterBoundProperty
		{
			DependencyProperty = ResizeBorderThicknessProperty,
			SystemParameterPropertyName = "WindowResizeBorderThickness"
		},
		new _SystemParameterBoundProperty
		{
			DependencyProperty = GlassFrameThicknessProperty,
			SystemParameterPropertyName = "WindowNonClientFrameThickness"
		}
	};

	/// <summary>Gets a uniform thickness of -1.</summary>
	/// <returns>A uniform thickness of -1 in all cases.</returns>
	public static Thickness GlassFrameCompleteThickness => new Thickness(-1.0);

	/// <summary>Gets or sets the height of the caption area at the top of a window.</summary>
	/// <returns>The height of the caption area.</returns>
	public double CaptionHeight
	{
		get
		{
			return (double)GetValue(CaptionHeightProperty);
		}
		set
		{
			SetValue(CaptionHeightProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the width of the border that is used to resize a window.</summary>
	/// <returns>The width of the border that is used to resize a window.</returns>
	public Thickness ResizeBorderThickness
	{
		get
		{
			return (Thickness)GetValue(ResizeBorderThicknessProperty);
		}
		set
		{
			SetValue(ResizeBorderThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the width of the glass border around a window.</summary>
	/// <returns>The width of the glass border around a window.</returns>
	public Thickness GlassFrameThickness
	{
		get
		{
			return (Thickness)GetValue(GlassFrameThicknessProperty);
		}
		set
		{
			SetValue(GlassFrameThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether hit-testing is enabled on the Windows Aero caption buttons.</summary>
	/// <returns>true if hit-testing is enabled on the caption buttons; otherwise, false. The registered default is true. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool UseAeroCaptionButtons
	{
		get
		{
			return (bool)GetValue(UseAeroCaptionButtonsProperty);
		}
		set
		{
			SetValue(UseAeroCaptionButtonsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the amount that the corners of a window are rounded.</summary>
	/// <returns>A value that describes the amount that corners are rounded.</returns>
	public CornerRadius CornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates which edges of the window frame are not owned by the client.</summary>
	/// <returns>A bitwise combination of the enumeration values that specify which edges of the frame are not owned by the client.The registered default is <see cref="F:System.Windows.Shell.NonClientFrameEdges.None" />. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public NonClientFrameEdges NonClientFrameEdges
	{
		get
		{
			return (NonClientFrameEdges)GetValue(NonClientFrameEdgesProperty);
		}
		set
		{
			SetValue(NonClientFrameEdgesProperty, value);
		}
	}

	internal event EventHandler PropertyChangedThatRequiresRepaint;

	private static void _OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!DesignerProperties.GetIsInDesignMode(d))
		{
			Window window = (Window)d;
			WindowChrome windowChrome = (WindowChrome)e.NewValue;
			WindowChromeWorker windowChromeWorker = WindowChromeWorker.GetWindowChromeWorker(window);
			if (windowChromeWorker == null)
			{
				windowChromeWorker = new WindowChromeWorker();
				WindowChromeWorker.SetWindowChromeWorker(window, windowChromeWorker);
			}
			windowChromeWorker.SetWindowChrome(windowChrome);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Shell.WindowChrome.WindowChrome" /> attached property from the specified <see cref="T:System.Windows.Window" />.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.Shell.WindowChrome" /> that is attached to the specified <see cref="T:System.Windows.Window" />.</returns>
	/// <param name="window">The <see cref="T:System.Windows.Window" /> from which to read the property value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="window" /> is null.</exception>
	public static WindowChrome GetWindowChrome(Window window)
	{
		Verify.IsNotNull(window, "window");
		return (WindowChrome)window.GetValue(WindowChromeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Shell.WindowChrome.WindowChrome" /> attached property on the specified <see cref="T:System.Windows.Window" />.</summary>
	/// <param name="window">The <see cref="T:System.Windows.Window" /> on which to set the <see cref="P:System.Windows.Shell.WindowChrome.WindowChrome" /> attached property.</param>
	/// <param name="chrome">The instance of <see cref="T:System.Windows.Shell.WindowChrome" /> to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="window" /> is null.</exception>
	public static void SetWindowChrome(Window window, WindowChrome chrome)
	{
		Verify.IsNotNull(window, "window");
		window.SetValue(WindowChromeProperty, chrome);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> attached property from the specified input element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> attached property.</returns>
	/// <param name="inputElement">The input element from which to read the property value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputElement" /> is not a <see cref="T:System.Windows.DependencyObject" />.</exception>
	public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
	{
		Verify.IsNotNull(inputElement, "inputElement");
		return (bool)((inputElement as DependencyObject) ?? throw new ArgumentException("The element must be a DependencyObject", "inputElement")).GetValue(IsHitTestVisibleInChromeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> attached property on the specified input element.</summary>
	/// <param name="inputElement">The element on which to set the <see cref="P:System.Windows.Shell.WindowChrome.IsHitTestVisibleInChrome" /> attached property.</param>
	/// <param name="hitTestVisible">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputElement" /> is not a <see cref="T:System.Windows.DependencyObject" />.</exception>
	public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
	{
		Verify.IsNotNull(inputElement, "inputElement");
		((inputElement as DependencyObject) ?? throw new ArgumentException("The element must be a DependencyObject", "inputElement")).SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> attached property from the specified input element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> attached property.</returns>
	/// <param name="inputElement">The input element from which to read the property value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputElement" /> is not a <see cref="T:System.Windows.DependencyObject" />.</exception>
	public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement)
	{
		Verify.IsNotNull(inputElement, "inputElement");
		return (ResizeGripDirection)((inputElement as DependencyObject) ?? throw new ArgumentException("The element must be a DependencyObject", "inputElement")).GetValue(ResizeGripDirectionProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> attached property on the specified input element.</summary>
	/// <param name="inputElement">The element on which to set the <see cref="P:System.Windows.Shell.WindowChrome.ResizeGripDirection" /> attached property.</param>
	/// <param name="direction">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inputElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="inputElement" /> is not a <see cref="T:System.Windows.DependencyObject" />.</exception>
	public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction)
	{
		Verify.IsNotNull(inputElement, "inputElement");
		((inputElement as DependencyObject) ?? throw new ArgumentException("The element must be a DependencyObject", "inputElement")).SetValue(ResizeGripDirectionProperty, direction);
	}

	private static object _CoerceGlassFrameThickness(Thickness thickness)
	{
		if (!Utility.IsThicknessNonNegative(thickness))
		{
			return GlassFrameCompleteThickness;
		}
		return thickness;
	}

	private static bool _NonClientFrameEdgesAreValid(object value)
	{
		NonClientFrameEdges nonClientFrameEdges = NonClientFrameEdges.None;
		try
		{
			nonClientFrameEdges = (NonClientFrameEdges)value;
		}
		catch (InvalidCastException)
		{
			return false;
		}
		if (nonClientFrameEdges == NonClientFrameEdges.None)
		{
			return true;
		}
		if ((nonClientFrameEdges | NonClientFrameEdges_All) != NonClientFrameEdges_All)
		{
			return false;
		}
		if (nonClientFrameEdges == NonClientFrameEdges_All)
		{
			return false;
		}
		return true;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Shell.WindowChrome" /> class.</summary>
	/// <returns>The new instance of this class.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new WindowChrome();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.WindowChrome" /> class.</summary>
	public WindowChrome()
	{
		foreach (_SystemParameterBoundProperty boundProperty in _BoundProperties)
		{
			BindingOperations.SetBinding(this, boundProperty.DependencyProperty, new Binding
			{
				Path = new PropertyPath("(SystemParameters." + boundProperty.SystemParameterPropertyName + ")"),
				Mode = BindingMode.OneWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});
		}
	}

	private void _OnPropertyChangedThatRequiresRepaint()
	{
		this.PropertyChangedThatRequiresRepaint?.Invoke(this, EventArgs.Empty);
	}
}

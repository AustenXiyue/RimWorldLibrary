using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Input;

/// <summary>Represents a tablet pen used with a Tablet PC.</summary>
public sealed class StylusDevice : InputDevice
{
	internal StylusDeviceBase StylusDeviceImpl { get; set; }

	/// <summary>Gets the element that receives input.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> object that receives input.</returns>
	public override IInputElement Target
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.Target;
		}
	}

	public bool IsValid => StylusDeviceImpl.IsValid;

	/// <summary>Gets the <see cref="T:System.Windows.PresentationSource" /> that reports current input for the stylus.</summary>
	/// <returns>The <see cref="T:System.Windows.PresentationSource" /> that reports current input for the stylus.</returns>
	public override PresentationSource ActiveSource => StylusDeviceImpl.ActiveSource;

	/// <summary>Gets the <see cref="T:System.Windows.IInputElement" /> that the pointer is positioned over.</summary>
	/// <returns>The element the pointer is over.</returns>
	public IInputElement DirectlyOver
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.DirectlyOver;
		}
	}

	/// <summary>Gets the element that captured the stylus.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> that captured the stylus.</returns>
	public IInputElement Captured
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.Captured;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.TabletDevice" /> representing the digitizer associated with the current <see cref="T:System.Windows.Input.StylusDevice" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.TabletDevice" /> represents the digitizer associated with the current <see cref="T:System.Windows.Input.StylusDevice" />.</returns>
	public TabletDevice TabletDevice => StylusDeviceImpl.TabletDevice;

	/// <summary>Gets the name of the stylus.</summary>
	/// <returns>The name of the stylus.</returns>
	public string Name
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.Name;
		}
	}

	/// <summary>Gets the identifier for the stylus device.</summary>
	/// <returns>The identifier for the stylus device.</returns>
	public int Id
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.Id;
		}
	}

	/// <summary>Gets the stylus buttons on the stylus.</summary>
	/// <returns>A reference to a <see cref="T:System.Windows.Input.StylusButtonCollection" /> object representing all of the buttons on the stylus.</returns>
	public StylusButtonCollection StylusButtons
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.StylusButtons;
		}
	}

	/// <summary>Gets whether the tablet pen is positioned above, yet not in contact with, the digitizer.</summary>
	/// <returns>true if the pen is positioned above, yet not in contact with, the digitizer; otherwise, false. The default is false.</returns>
	public bool InAir
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.InAir;
		}
	}

	/// <summary>Gets a value that indicates whether the secondary tip of the stylus is in use.</summary>
	/// <returns>true if the secondary tip of the stylus is in use; otherwise, false. The default is false.</returns>
	public bool Inverted
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.Inverted;
		}
	}

	/// <summary>Gets a value that indicates whether the tablet pen is in range of the digitizer.</summary>
	/// <returns>true if the pen is within range of the digitizer; otherwise, false. The default is false.</returns>
	public bool InRange
	{
		get
		{
			VerifyAccess();
			return StylusDeviceImpl.InRange;
		}
	}

	internal int DoubleTapDeltaX => StylusDeviceImpl.DoubleTapDeltaX;

	internal int DoubleTapDeltaY => StylusDeviceImpl.DoubleTapDeltaY;

	internal int DoubleTapDeltaTime => StylusDeviceImpl.DoubleTapDeltaTime;

	internal StylusDevice(StylusDeviceBase impl)
	{
		if (impl == null)
		{
			throw new ArgumentNullException("impl");
		}
		StylusDeviceImpl = impl;
	}

	/// <summary>Binds the stylus to the specified element.</summary>
	/// <returns>true if the input element is captured successfully; otherwise, false. The default is false.</returns>
	/// <param name="captureMode">One of the <see cref="T:System.Windows.Input.CaptureMode" /> values.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> or <paramref name="captureMode" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public bool Capture(IInputElement element, CaptureMode captureMode)
	{
		return StylusDeviceImpl.Capture(element, captureMode);
	}

	/// <summary>Binds input from the stylus to the specified element.</summary>
	/// <returns>true if the input element is captured successfully; otherwise, false. The default is false.</returns>
	/// <param name="element">The element to which the stylus is bound.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="element" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public bool Capture(IInputElement element)
	{
		return Capture(element, CaptureMode.Element);
	}

	/// <summary>Synchronizes the cursor and the user interface.</summary>
	public void Synchronize()
	{
		StylusDeviceImpl.Synchronize();
	}

	/// <summary>Returns the name of the stylus device.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.Input.StylusDevice" />.</returns>
	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", base.ToString(), Name);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects collected from the stylus.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects that the stylus collected.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> to which the (<paramref name="x,y" />) coordinates in the <see cref="T:System.Windows.Input.StylusPointCollection" /> are mapped.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public StylusPointCollection GetStylusPoints(IInputElement relativeTo)
	{
		VerifyAccess();
		return StylusDeviceImpl.GetStylusPoints(relativeTo);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects collected from the stylus. Uses the specified <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects collected from the stylus.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> to which the (<paramref name="x y" />) coordinates in the <see cref="T:System.Windows.Input.StylusPointCollection" /> are mapped.</param>
	/// <param name="subsetToReformatTo">The <see cref="T:System.Windows.Input.StylusPointDescription" /> to be used by the <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public StylusPointCollection GetStylusPoints(IInputElement relativeTo, StylusPointDescription subsetToReformatTo)
	{
		return StylusDeviceImpl.GetStylusPoints(relativeTo, subsetToReformatTo);
	}

	/// <summary>Gets the position of the stylus.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> that represents the position of the stylus, in relation to <paramref name="relativeTo" />.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> to which the (<paramref name="x,y" />) coordinates are mapped.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public Point GetPosition(IInputElement relativeTo)
	{
		VerifyAccess();
		return StylusDeviceImpl.GetPosition(relativeTo);
	}

	internal Point GetMouseScreenPosition(MouseDevice mouseDevice)
	{
		return StylusDeviceImpl.GetMouseScreenPosition(mouseDevice);
	}

	internal MouseButtonState GetMouseButtonState(MouseButton mouseButton, MouseDevice mouseDevice)
	{
		return StylusDeviceImpl.GetMouseButtonState(mouseButton, mouseDevice);
	}

	internal static IInputElement LocalHitTest(PresentationSource inputSource, Point pt)
	{
		return MouseDevice.LocalHitTest(pt, inputSource);
	}

	internal static IInputElement GlobalHitTest(PresentationSource inputSource, Point pt)
	{
		return MouseDevice.GlobalHitTest(pt, inputSource);
	}

	internal static GeneralTransform GetElementTransform(IInputElement relativeTo)
	{
		GeneralTransform result = Transform.Identity;
		if (relativeTo is DependencyObject o)
		{
			Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(InputElement.GetContainingVisual(o));
			GeneralTransform generalTransform = VisualTreeHelper.GetContainingVisual2D(InputElement.GetRootVisual(o)).TransformToDescendant(containingVisual2D);
			if (generalTransform != null)
			{
				result = generalTransform;
			}
		}
		return result;
	}

	internal T As<T>() where T : StylusDeviceBase
	{
		return StylusDeviceImpl as T;
	}
}

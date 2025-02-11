using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides data for the <see cref="E:System.Windows.UIElement.StylusSystemGesture" /> event. </summary>
public class StylusSystemGestureEventArgs : StylusEventArgs
{
	private SystemGesture _id;

	private int _buttonState;

	private int _gestureX;

	private int _gestureY;

	/// <summary>Gets the <see cref="T:System.Windows.Input.SystemGesture" /> that raises the event.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.SystemGesture" /> that raises the event.</returns>
	public SystemGesture SystemGesture => _id;

	internal int ButtonState => _buttonState;

	internal int GestureX => _gestureX;

	internal int GestureY => _gestureY;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusSystemGestureEventArgs" /> class. </summary>
	/// <param name="stylusDevice">The <see cref="T:System.Windows.Input.StylusDevice" /> to associate with the event.</param>
	/// <param name="systemGesture">The <see cref="T:System.Windows.Input.SystemGesture" /> that raises the event.</param>
	public StylusSystemGestureEventArgs(StylusDevice stylusDevice, int timestamp, SystemGesture systemGesture)
		: base(stylusDevice, timestamp)
	{
		if (!RawStylusSystemGestureInputReport.IsValidSystemGesture(systemGesture, allowFlick: false, allowDoubleTap: false))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "systemGesture"));
		}
		_id = systemGesture;
	}

	internal StylusSystemGestureEventArgs(StylusDevice stylusDevice, int timestamp, SystemGesture systemGesture, int gestureX, int gestureY, int buttonState)
		: base(stylusDevice, timestamp)
	{
		if (!RawStylusSystemGestureInputReport.IsValidSystemGesture(systemGesture, allowFlick: true, allowDoubleTap: false))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "systemGesture"));
		}
		_id = systemGesture;
		_buttonState = buttonState;
		_gestureX = gestureX;
		_gestureY = gestureY;
	}

	/// <summary>Invokes a type-specific handler on the target whenever the <see cref="E:System.Windows.UIElement.StylusSystemGesture" /> event is raised.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((StylusSystemGestureEventHandler)genericHandler)(genericTarget, this);
	}
}

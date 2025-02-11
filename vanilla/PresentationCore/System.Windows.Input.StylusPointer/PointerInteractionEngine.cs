using System.Collections.Generic;
using System.Windows.Media;
using MS.Internal;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerInteractionEngine : IDisposable
{
	private enum HoverState
	{
		AwaitingHover,
		TimingHover,
		HoverCancelled,
		InHover
	}

	private const int HoverActivationThresholdTicks = 275;

	private const double DragThresholdInches = 0.106299;

	private static List<UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION> DefaultConfiguration = new List<UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION>
	{
		new UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION
		{
			enable = UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION,
			interactionId = UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_TAP
		},
		new UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION
		{
			enable = UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION,
			interactionId = UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_HOLD
		},
		new UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION
		{
			enable = UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION,
			interactionId = UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_SECONDARY_TAP
		},
		new UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION
		{
			enable = (UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION | UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_X | UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_Y | UnsafeNativeMethods.INTERACTION_CONFIGURATION_FLAGS.INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_INERTIA),
			interactionId = UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_MANIPULATION
		}
	};

	private MS.Internal.SecurityCriticalDataForSet<nint> _interactionContext = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);

	private PointerStylusDevice _stylusDevice;

	private UnsafeNativeMethods.INTERACTION_CONTEXT_OUTPUT_CALLBACK _callbackDelegate;

	private bool _firedDrag;

	private bool _firedHold;

	private bool _firedFlick;

	private HoverState _hoverState;

	private uint _hoverStartTicks;

	private PointerFlickEngine _flickEngine;

	private bool _disposed;

	internal event EventHandler<RawStylusSystemGestureInputReport> InteractionDetected;

	internal PointerInteractionEngine(PointerStylusDevice stylusDevice, List<UnsafeNativeMethods.INTERACTION_CONTEXT_CONFIGURATION> configuration = null)
	{
		_stylusDevice = stylusDevice;
		_ = _stylusDevice.TabletDevice.Type;
		nint interactionContext = IntPtr.Zero;
		UnsafeNativeMethods.CreateInteractionContext(out interactionContext);
		_interactionContext = new MS.Internal.SecurityCriticalDataForSet<nint>(interactionContext);
		if (configuration == null)
		{
			configuration = DefaultConfiguration;
		}
		if (_interactionContext.Value != IntPtr.Zero)
		{
			UnsafeNativeMethods.SetPropertyInteractionContext(_interactionContext.Value, UnsafeNativeMethods.INTERACTION_CONTEXT_PROPERTY.INTERACTION_CONTEXT_PROPERTY_FILTER_POINTERS, Convert.ToUInt32(value: false));
			UnsafeNativeMethods.SetPropertyInteractionContext(_interactionContext.Value, UnsafeNativeMethods.INTERACTION_CONTEXT_PROPERTY.INTERACTION_CONTEXT_PROPERTY_MEASUREMENT_UNITS, 1u);
			UnsafeNativeMethods.SetInteractionConfigurationInteractionContext(_interactionContext.Value, (uint)configuration.Count, configuration.ToArray());
			_callbackDelegate = Callback;
			UnsafeNativeMethods.RegisterOutputCallbackInteractionContext(_interactionContext.Value, _callbackDelegate, 0);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (_interactionContext.Value != IntPtr.Zero)
			{
				UnsafeNativeMethods.DestroyInteractionContext(_interactionContext.Value);
				_interactionContext.Value = IntPtr.Zero;
			}
			_disposed = true;
		}
	}

	~PointerInteractionEngine()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal void Update(RawStylusInputReport rsir)
	{
		try
		{
			UnsafeNativeMethods.BufferPointerPacketsInteractionContext(_interactionContext.Value, 1u, new UnsafeNativeMethods.POINTER_INFO[1] { _stylusDevice.CurrentPointerInfo });
			DetectHover();
			DetectFlick(rsir);
			UnsafeNativeMethods.ProcessBufferedPacketsInteractionContext(_interactionContext.Value);
		}
		catch
		{
		}
	}

	private void Callback(nint clientData, ref UnsafeNativeMethods.INTERACTION_CONTEXT_OUTPUT output)
	{
		SystemGesture systemGesture = SystemGesture.None;
		switch (output.interactionId)
		{
		case UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_TAP:
			systemGesture = SystemGesture.Tap;
			break;
		case UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_SECONDARY_TAP:
			systemGesture = SystemGesture.RightTap;
			break;
		case UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_HOLD:
			_firedHold = true;
			systemGesture = ((!output.interactionFlags.HasFlag(UnsafeNativeMethods.INTERACTION_FLAGS.INTERACTION_FLAG_BEGIN)) ? SystemGesture.HoldLeave : SystemGesture.HoldEnter);
			break;
		case UnsafeNativeMethods.INTERACTION_ID.INTERACTION_ID_MANIPULATION:
			systemGesture = DetectDragOrFlick(output);
			break;
		}
		if (systemGesture != 0)
		{
			this.InteractionDetected?.Invoke(this, new RawStylusSystemGestureInputReport(InputMode.Foreground, Environment.TickCount, _stylusDevice.CriticalActiveSource, (Func<StylusPointDescription>)null, -1, -1, systemGesture, Convert.ToInt32(output.x), Convert.ToInt32(output.y), 0));
		}
	}

	private void DetectFlick(RawStylusInputReport rsir)
	{
		_flickEngine?.Update(rsir);
		if (rsir.Actions == RawStylusActions.Up && _flickEngine?.Result?.CanBeFlick == true)
		{
			this.InteractionDetected?.Invoke(this, new RawStylusSystemGestureInputReport(InputMode.Foreground, Environment.TickCount, _stylusDevice.CriticalActiveSource, (Func<StylusPointDescription>)null, -1, -1, SystemGesture.Flick, Convert.ToInt32(_flickEngine.Result.TabletStart.X), Convert.ToInt32(_flickEngine.Result.TabletStart.Y), 0));
			_firedFlick = true;
		}
	}

	private void DetectHover()
	{
		if (_stylusDevice.TabletDevice.Type != 0)
		{
			return;
		}
		SystemGesture systemGesture = SystemGesture.None;
		if (_stylusDevice.IsNew)
		{
			_hoverState = HoverState.AwaitingHover;
		}
		switch (_hoverState)
		{
		case HoverState.AwaitingHover:
			if (_stylusDevice.InAir)
			{
				_hoverStartTicks = _stylusDevice.TimeStamp;
				_hoverState = HoverState.TimingHover;
			}
			break;
		case HoverState.TimingHover:
			if (_stylusDevice.InAir)
			{
				if (_stylusDevice.TimeStamp < _hoverStartTicks)
				{
					_hoverStartTicks = _stylusDevice.TimeStamp;
				}
				else if (_stylusDevice.TimeStamp - _hoverStartTicks > 275)
				{
					systemGesture = SystemGesture.HoverEnter;
					_hoverState = HoverState.InHover;
				}
			}
			else if (_stylusDevice.IsDown)
			{
				_hoverState = HoverState.HoverCancelled;
			}
			break;
		case HoverState.HoverCancelled:
			if (_stylusDevice.InAir)
			{
				_hoverState = HoverState.AwaitingHover;
			}
			break;
		case HoverState.InHover:
			if (_stylusDevice.IsDown || !_stylusDevice.InRange)
			{
				systemGesture = SystemGesture.HoverLeave;
				_hoverState = HoverState.HoverCancelled;
			}
			break;
		}
		if (systemGesture != 0)
		{
			this.InteractionDetected?.Invoke(this, new RawStylusSystemGestureInputReport(InputMode.Foreground, Environment.TickCount, _stylusDevice.CriticalActiveSource, (Func<StylusPointDescription>)null, -1, -1, systemGesture, Convert.ToInt32(_stylusDevice.RawStylusPoint.X), Convert.ToInt32(_stylusDevice.RawStylusPoint.Y), 0));
		}
	}

	private SystemGesture DetectDragOrFlick(UnsafeNativeMethods.INTERACTION_CONTEXT_OUTPUT output)
	{
		SystemGesture result = SystemGesture.None;
		if (output.interactionFlags.HasFlag(UnsafeNativeMethods.INTERACTION_FLAGS.INTERACTION_FLAG_END))
		{
			_firedDrag = false;
			_firedHold = false;
			_firedFlick = false;
		}
		else if (!_firedDrag && !_firedFlick)
		{
			PointerFlickEngine flickEngine = _flickEngine;
			bool? obj;
			if (flickEngine == null)
			{
				obj = null;
			}
			else
			{
				PointerFlickEngine.FlickResult result2 = flickEngine.Result;
				obj = ((result2 != null) ? new bool?(!result2.CanBeFlick) : ((bool?)null));
			}
			if (obj ?? true)
			{
				DpiScale dpi = VisualTreeHelper.GetDpi(_stylusDevice.CriticalActiveSource.RootVisual);
				double num = (double)output.arguments.manipulation.cumulative.translationX / dpi.PixelsPerInchX;
				double num2 = (double)output.arguments.manipulation.cumulative.translationY / dpi.PixelsPerInchY;
				if (num > 0.106299 || num2 > 0.106299)
				{
					result = (_firedHold ? SystemGesture.RightDrag : SystemGesture.Drag);
					_firedDrag = true;
				}
			}
		}
		return result;
	}
}

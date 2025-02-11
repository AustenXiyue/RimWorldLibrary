using System.Windows.Input.StylusPlugIns;
using System.Windows.Threading;

namespace System.Windows.Input;

internal abstract class StylusDeviceBase : DispatcherObject, IDisposable
{
	protected bool _disposed;

	internal StylusDevice StylusDevice { get; private set; }

	internal abstract IInputElement Target { get; }

	internal abstract PresentationSource ActiveSource { get; }

	internal abstract PresentationSource CriticalActiveSource { get; }

	internal abstract StylusPoint RawStylusPoint { get; }

	internal abstract bool IsValid { get; }

	internal abstract IInputElement DirectlyOver { get; }

	internal abstract IInputElement Captured { get; }

	internal abstract CaptureMode CapturedMode { get; }

	internal abstract TabletDevice TabletDevice { get; }

	internal abstract string Name { get; }

	internal abstract int Id { get; }

	internal abstract StylusButtonCollection StylusButtons { get; }

	internal abstract bool InAir { get; }

	internal abstract bool Inverted { get; }

	internal abstract bool InRange { get; }

	internal abstract int DoubleTapDeltaX { get; }

	internal abstract int DoubleTapDeltaY { get; }

	internal abstract int DoubleTapDeltaTime { get; }

	internal abstract int TapCount { get; set; }

	internal abstract StylusPlugInCollection CurrentVerifiedTarget { get; set; }

	internal T As<T>() where T : StylusDeviceBase
	{
		return this as T;
	}

	protected StylusDeviceBase()
	{
		StylusDevice = new StylusDevice(this);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected abstract void Dispose(bool disposing);

	~StylusDeviceBase()
	{
		Dispose(disposing: false);
	}

	internal abstract void UpdateEventStylusPoints(RawStylusInputReport report, bool resetIfNoOverride);

	internal abstract bool Capture(IInputElement element, CaptureMode captureMode);

	internal abstract bool Capture(IInputElement element);

	internal abstract void Synchronize();

	internal abstract StylusPointCollection GetStylusPoints(IInputElement relativeTo);

	internal abstract StylusPointCollection GetStylusPoints(IInputElement relativeTo, StylusPointDescription subsetToReformatTo);

	internal abstract Point GetPosition(IInputElement relativeTo);

	internal abstract Point GetMouseScreenPosition(MouseDevice mouseDevice);

	internal abstract MouseButtonState GetMouseButtonState(MouseButton mouseButton, MouseDevice mouseDevice);

	internal abstract StylusPlugInCollection GetCapturedPlugInCollection(ref bool elementHasCapture);
}

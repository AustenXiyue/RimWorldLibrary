namespace System.Windows.Input;

/// <summary>Provides an application-level service that processes multitouch input from the operating system and raises the <see cref="E:System.Windows.Input.Touch.FrameReported" /> event.</summary>
public static class Touch
{
	internal static readonly RoutedEvent PreviewTouchDownEvent = EventManager.RegisterRoutedEvent("PreviewTouchDown", RoutingStrategy.Tunnel, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent TouchDownEvent = EventManager.RegisterRoutedEvent("TouchDown", RoutingStrategy.Bubble, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent PreviewTouchMoveEvent = EventManager.RegisterRoutedEvent("PreviewTouchMove", RoutingStrategy.Tunnel, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent TouchMoveEvent = EventManager.RegisterRoutedEvent("TouchMove", RoutingStrategy.Bubble, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent PreviewTouchUpEvent = EventManager.RegisterRoutedEvent("PreviewTouchUp", RoutingStrategy.Tunnel, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent TouchUpEvent = EventManager.RegisterRoutedEvent("TouchUp", RoutingStrategy.Bubble, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent GotTouchCaptureEvent = EventManager.RegisterRoutedEvent("GotTouchCapture", RoutingStrategy.Bubble, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent LostTouchCaptureEvent = EventManager.RegisterRoutedEvent("LostTouchCapture", RoutingStrategy.Bubble, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent TouchEnterEvent = EventManager.RegisterRoutedEvent("TouchEnter", RoutingStrategy.Direct, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	internal static readonly RoutedEvent TouchLeaveEvent = EventManager.RegisterRoutedEvent("TouchLeave", RoutingStrategy.Direct, typeof(EventHandler<TouchEventArgs>), typeof(Touch));

	/// <summary>Occurs when a touch message is sent.</summary>
	public static event TouchFrameEventHandler FrameReported;

	internal static void ReportFrame()
	{
		if (Touch.FrameReported != null)
		{
			TouchFrameEventArgs e = new TouchFrameEventArgs(Environment.TickCount);
			Touch.FrameReported(null, e);
		}
	}
}

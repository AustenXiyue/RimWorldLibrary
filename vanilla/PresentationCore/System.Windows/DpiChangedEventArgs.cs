namespace System.Windows;

public sealed class DpiChangedEventArgs : RoutedEventArgs
{
	public DpiScale OldDpi { get; private set; }

	public DpiScale NewDpi { get; private set; }

	internal DpiChangedEventArgs(DpiScale oldDpi, DpiScale newDpi, RoutedEvent routedEvent, object source)
		: base(routedEvent, source)
	{
		OldDpi = oldDpi;
		NewDpi = newDpi;
	}
}

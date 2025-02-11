namespace System.Windows;

/// <summary>Provides data related to the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event. </summary>
public class SizeChangedEventArgs : RoutedEventArgs
{
	private Size _previousSize;

	private UIElement _element;

	private byte _bits;

	private static byte _widthChangedBit = 1;

	private static byte _heightChangedBit = 2;

	/// <summary>Gets the previous <see cref="T:System.Windows.Size" /> of the object. </summary>
	/// <returns>The previous <see cref="T:System.Windows.Size" /> of the object.</returns>
	public Size PreviousSize => _previousSize;

	/// <summary>Gets the new <see cref="T:System.Windows.Size" /> of the object.</summary>
	/// <returns>The new <see cref="T:System.Windows.Size" /> of the object.</returns>
	public Size NewSize => _element.RenderSize;

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.FrameworkElement.Width" /> component of the size changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Width" /> component of the size changed; otherwise, false.</returns>
	public bool WidthChanged => (_bits & _widthChangedBit) != 0;

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Windows.FrameworkElement.Height" /> component of the size changed.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Height" /> component of the size changed; otherwise, false.</returns>
	public bool HeightChanged => (_bits & _heightChangedBit) != 0;

	internal SizeChangedEventArgs(UIElement element, SizeChangedInfo info)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		_element = element;
		_previousSize = info.PreviousSize;
		if (info.WidthChanged)
		{
			_bits |= _widthChangedBit;
		}
		if (info.HeightChanged)
		{
			_bits |= _heightChangedBit;
		}
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((SizeChangedEventHandler)genericHandler)(genericTarget, this);
	}
}

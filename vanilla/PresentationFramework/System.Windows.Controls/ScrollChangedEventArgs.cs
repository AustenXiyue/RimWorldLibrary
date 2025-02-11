namespace System.Windows.Controls;

/// <summary>Describes a change in the scrolling state and contains the required arguments for a <see cref="E:System.Windows.Controls.ScrollViewer.ScrollChanged" /> event. </summary>
public class ScrollChangedEventArgs : RoutedEventArgs
{
	private Vector _offset;

	private Vector _offsetChange;

	private Size _extent;

	private Vector _extentChange;

	private Size _viewport;

	private Vector _viewportChange;

	/// <summary>Gets the updated horizontal offset value for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated value of the horizontal offset for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double HorizontalOffset => _offset.X;

	/// <summary>Gets the updated value of the vertical offset for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated vertical offset of a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double VerticalOffset => _offset.Y;

	/// <summary>Gets a value that indicates the change in horizontal offset for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in horizontal offset for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double HorizontalChange => _offsetChange.X;

	/// <summary>Gets a value that indicates the change in vertical offset of a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in vertical offset of a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double VerticalChange => _offsetChange.Y;

	/// <summary>Gets the updated value of the viewport width for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated value of the viewport width for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double ViewportWidth => _viewport.Width;

	/// <summary>Gets the updated value of the viewport height for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated viewport height of a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double ViewportHeight => _viewport.Height;

	/// <summary>Gets a value that indicates the change in viewport width of a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in viewport width for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double ViewportWidthChange => _viewportChange.X;

	/// <summary>Gets a value that indicates the change in value of the viewport height for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in vertical viewport height for a <see cref="T:System.Windows.Controls.ScrollViewer" />.</returns>
	public double ViewportHeightChange => _viewportChange.Y;

	/// <summary>Gets the updated width of the <see cref="T:System.Windows.Controls.ScrollViewer" /> extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated extent width.</returns>
	public double ExtentWidth => _extent.Width;

	/// <summary>Gets the updated height of the <see cref="T:System.Windows.Controls.ScrollViewer" /> extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the updated extent height.</returns>
	public double ExtentHeight => _extent.Height;

	/// <summary>Gets a value that indicates the change in width of the <see cref="T:System.Windows.Controls.ScrollViewer" /> extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in extent width.</returns>
	public double ExtentWidthChange => _extentChange.X;

	/// <summary>Gets a value that indicates the change in height of the <see cref="T:System.Windows.Controls.ScrollViewer" /> extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the change in extent height.</returns>
	public double ExtentHeightChange => _extentChange.Y;

	internal ScrollChangedEventArgs(Vector offset, Vector offsetChange, Size extent, Vector extentChange, Size viewport, Vector viewportChange)
	{
		_offset = offset;
		_offsetChange = offsetChange;
		_extent = extent;
		_extentChange = extentChange;
		_viewport = viewport;
		_viewportChange = viewportChange;
	}

	/// <summary>Performs proper type casting before invoking the type-safe <see cref="T:System.Windows.Controls.ScrollChangedEventHandler" /> delegate </summary>
	/// <param name="genericHandler">The event handler to invoke, in this case the <see cref="T:System.Windows.Controls.ScrollChangedEventHandler" /> delegate.</param>
	/// <param name="genericTarget">The target upon which the <paramref name="genericHandler" /> is invoked.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((ScrollChangedEventHandler)genericHandler)(genericTarget, this);
	}
}

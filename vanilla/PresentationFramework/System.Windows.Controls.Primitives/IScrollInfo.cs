using System.Windows.Media;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents the main scrollable region inside a <see cref="T:System.Windows.Controls.ScrollViewer" /> control.</summary>
public interface IScrollInfo
{
	/// <summary>Gets or sets a value that indicates whether scrolling on the vertical axis is possible. </summary>
	/// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
	bool CanVerticallyScroll { get; set; }

	/// <summary>Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.</summary>
	/// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
	bool CanHorizontallyScroll { get; set; }

	/// <summary>Gets the horizontal size of the extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the extent. This property has no default value.</returns>
	double ExtentWidth { get; }

	/// <summary>Gets the vertical size of the extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the extent.This property has no default value.</returns>
	double ExtentHeight { get; }

	/// <summary>Gets the horizontal size of the viewport for this content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the viewport for this content. This property has no default value.</returns>
	double ViewportWidth { get; }

	/// <summary>Gets the vertical size of the viewport for this content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the viewport for this content. This property has no default value.</returns>
	double ViewportHeight { get; }

	/// <summary>Gets the horizontal offset of the scrolled content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal offset. This property has no default value.</returns>
	double HorizontalOffset { get; }

	/// <summary>Gets the vertical offset of the scrolled content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical offset of the scrolled content. Valid values are between zero and the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ExtentHeight" /> minus the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ViewportHeight" />. This property has no default value.</returns>
	double VerticalOffset { get; }

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior.</summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior. This property has no default value.</returns>
	ScrollViewer ScrollOwner { get; set; }

	/// <summary>Scrolls up within content by one logical unit. </summary>
	void LineUp();

	/// <summary>Scrolls down within content by one logical unit. </summary>
	void LineDown();

	/// <summary>Scrolls left within content by one logical unit.</summary>
	void LineLeft();

	/// <summary>Scrolls right within content by one logical unit.</summary>
	void LineRight();

	/// <summary>Scrolls up within content by one page.</summary>
	void PageUp();

	/// <summary>Scrolls down within content by one page.</summary>
	void PageDown();

	/// <summary>Scrolls left within content by one page.</summary>
	void PageLeft();

	/// <summary>Scrolls right within content by one page.</summary>
	void PageRight();

	/// <summary>Scrolls up within content after a user clicks the wheel button on a mouse.</summary>
	void MouseWheelUp();

	/// <summary>Scrolls down within content after a user clicks the wheel button on a mouse.</summary>
	void MouseWheelDown();

	/// <summary>Scrolls left within content after a user clicks the wheel button on a mouse.</summary>
	void MouseWheelLeft();

	/// <summary>Scrolls right within content after a user clicks the wheel button on a mouse.</summary>
	void MouseWheelRight();

	/// <summary>Sets the amount of horizontal offset.</summary>
	/// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
	void SetHorizontalOffset(double offset);

	/// <summary>Sets the amount of vertical offset.</summary>
	/// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
	void SetVerticalOffset(double offset);

	/// <summary>Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object is visible. </summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that is visible.</returns>
	/// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
	/// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
	Rect MakeVisible(Visual visual, Rect rectangle);
}

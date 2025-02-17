namespace System.Windows.Controls.Primitives;

/// <summary>Describes how a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control animates when it opens.</summary>
public enum PopupAnimation
{
	/// <summary>The <see cref="T:System.Windows.Controls.Primitives.Popup" /> control appears without animation. </summary>
	None,
	/// <summary>The <see cref="T:System.Windows.Controls.Primitives.Popup" /> control gradually appears, or fades in. This effect is created by increasing the opacity of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> window over time.</summary>
	Fade,
	/// <summary>The <see cref="T:System.Windows.Controls.Primitives.Popup" /> control slides down or up into place. By default, a <see cref="T:System.Windows.Controls.Primitives.Popup" /> slides down. However, if the screen does not provide enough room for the <see cref="T:System.Windows.Controls.Primitives.Popup" /> to slide down, it slides up instead.</summary>
	Slide,
	/// <summary>The <see cref="T:System.Windows.Controls.Primitives.Popup" /> control scrolls from the upper-left corner of its parent. If the screen does not provide enough room to allow the <see cref="T:System.Windows.Controls.Primitives.Popup" /> default behavior, the <see cref="T:System.Windows.Controls.Primitives.Popup" /> scrolls from the lower-right corner instead.</summary>
	Scroll
}

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a method that provides custom positioning for a <see cref="T:System.Windows.Controls.Primitives.Popup" /> control. </summary>
/// <returns>An array of possible <see cref="T:System.Windows.Controls.Primitives.CustomPopupPlacement" /> positions for the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control relative to the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" />.</returns>
/// <param name="popupSize">The <see cref="T:System.Windows.Size" /> of the <see cref="T:System.Windows.Controls.Primitives.Popup" /> control.</param>
/// <param name="targetSize">The <see cref="T:System.Windows.Size" /> of the <see cref="P:System.Windows.Controls.Primitives.Popup.PlacementTarget" />.</param>
/// <param name="offset">The <see cref="T:System.Windows.Point" /> computed from the <see cref="P:System.Windows.Controls.Primitives.Popup.HorizontalOffset" /> and <see cref="P:System.Windows.Controls.Primitives.Popup.VerticalOffset" /> property values.</param>
public delegate CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset);

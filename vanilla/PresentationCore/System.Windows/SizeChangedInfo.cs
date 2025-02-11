namespace System.Windows;

/// <summary>Report the specifics of a value change involving a <see cref="T:System.Windows.Size" />. This is used as a parameter in <see cref="M:System.Windows.UIElement.OnRenderSizeChanged(System.Windows.SizeChangedInfo)" /> overrides.</summary>
public class SizeChangedInfo
{
	private UIElement _element;

	private Size _previousSize;

	private bool _widthChanged;

	private bool _heightChanged;

	internal SizeChangedInfo Next;

	/// <summary> Gets the previous size of the size-related value being reported as changed. </summary>
	/// <returns>The previous size.</returns>
	public Size PreviousSize => _previousSize;

	/// <summary>Gets the new size being reported. </summary>
	/// <returns>The new size.</returns>
	public Size NewSize => _element.RenderSize;

	/// <summary> Gets a value that declares whether the Width component of the size changed. </summary>
	/// <returns>true if the width changed; otherwise, false. </returns>
	public bool WidthChanged => _widthChanged;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Windows.SizeChangedInfo" />  reports a size change that includes a significant change to the Height component. </summary>
	/// <returns>true if there is a significant Height component change; otherwise, false.</returns>
	public bool HeightChanged => _heightChanged;

	internal UIElement Element => _element;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.SizeChangedInfo" /> class. </summary>
	/// <param name="element">The element where the size is being changed.</param>
	/// <param name="previousSize">The previous size, before the change.</param>
	/// <param name="widthChanged">true if the Width component of the size changed.</param>
	/// <param name="heightChanged">true if the Height component of the size changed.</param>
	public SizeChangedInfo(UIElement element, Size previousSize, bool widthChanged, bool heightChanged)
	{
		_element = element;
		_previousSize = previousSize;
		_widthChanged = widthChanged;
		_heightChanged = heightChanged;
	}

	internal void Update(bool widthChanged, bool heightChanged)
	{
		_widthChanged |= widthChanged;
		_heightChanged |= heightChanged;
	}
}

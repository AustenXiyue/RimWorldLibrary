namespace System.Windows.Input;

/// <summary>Specifies the mouse capture policies. </summary>
public enum CaptureMode
{
	/// <summary>No mouse capture.  Mouse input goes to the element under the mouse.</summary>
	None,
	/// <summary>Mouse capture is applied to a single element.  Mouse input goes to the captured element.</summary>
	Element,
	/// <summary>Mouse capture is applied to a subtree of elements.  If the mouse is over a child of the element with capture, mouse input is sent to the child element.  Otherwise, mouse input is sent to the element with mouse capture.</summary>
	SubTree
}

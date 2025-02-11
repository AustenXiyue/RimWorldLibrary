namespace System.Windows.Input;

/// <summary>Specifies the direction within a user interface (UI) in which a desired focus change request is attempted. The direction is either based on tab order or by relative direction in layout.</summary>
public enum FocusNavigationDirection
{
	/// <summary>Move focus to the next focusable element in tab order. Not supported for <see cref="M:System.Windows.UIElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" />.</summary>
	Next,
	/// <summary>Move focus to the previous focusable element in tab order. Not supported for <see cref="M:System.Windows.UIElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" />.</summary>
	Previous,
	/// <summary>Move focus to the first focusable element in tab order. Not supported for <see cref="M:System.Windows.UIElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" />.</summary>
	First,
	/// <summary>Move focus to the last focusable element in tab order. Not supported for <see cref="M:System.Windows.UIElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" />.</summary>
	Last,
	/// <summary>Move focus to another focusable element to the left of the currently focused element.</summary>
	Left,
	/// <summary>Move focus to another focusable element to the right of the currently focused element.</summary>
	Right,
	/// <summary>Move focus to another focusable element upwards from the currently focused element.</summary>
	Up,
	/// <summary>Move focus to another focusable element downwards from the currently focused element.</summary>
	Down
}

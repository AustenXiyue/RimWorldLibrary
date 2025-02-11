namespace System.Windows;

/// <summary>Provides data for the <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanging" /> and <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanged" /> events. </summary>
public sealed class VisualStateChangedEventArgs : EventArgs
{
	private VisualState _oldState;

	private VisualState _newState;

	private FrameworkElement _control;

	private FrameworkElement _stateGroupsRoot;

	/// <summary>Gets the state that the element is transitioning to or has transitioned from.</summary>
	/// <returns>The state that the element is transitioning to or has transitioned from.</returns>
	public VisualState OldState => _oldState;

	/// <summary>Gets the state that the element is transitioning to or has transitioned to.</summary>
	/// <returns>The state that the element is transitioning to or has transitioned to.</returns>
	public VisualState NewState => _newState;

	/// <summary>Gets the element that is transitioning states.</summary>
	/// <returns>The element that is transitioning states if the <see cref="T:System.Windows.VisualStateGroup" /> is in a <see cref="T:System.Windows.Controls.ControlTemplate" />; otherwise, null.</returns>
	public FrameworkElement Control => _control;

	/// <summary>Gets the root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</summary>
	/// <returns>The root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</returns>
	public FrameworkElement StateGroupsRoot => _stateGroupsRoot;

	internal VisualStateChangedEventArgs(VisualState oldState, VisualState newState, FrameworkElement control, FrameworkElement stateGroupsRoot)
	{
		_oldState = oldState;
		_newState = newState;
		_control = control;
		_stateGroupsRoot = stateGroupsRoot;
	}
}

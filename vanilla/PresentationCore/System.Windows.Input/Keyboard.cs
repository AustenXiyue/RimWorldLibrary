namespace System.Windows.Input;

/// <summary>Represents the keyboard device. </summary>
public static class Keyboard
{
	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event.</returns>
	public static readonly RoutedEvent PreviewKeyDownEvent = EventManager.RegisterRoutedEvent("PreviewKeyDown", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event.</returns>
	public static readonly RoutedEvent KeyDownEvent = EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event.</returns>
	public static readonly RoutedEvent PreviewKeyUpEvent = EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event.</returns>
	public static readonly RoutedEvent KeyUpEvent = EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event.</returns>
	public static readonly RoutedEvent PreviewGotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewGotKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyboardInputProviderAcquireFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyboardInputProviderAcquireFocus" /> attached event.</returns>
	public static readonly RoutedEvent PreviewKeyboardInputProviderAcquireFocusEvent = EventManager.RegisterRoutedEvent("PreviewKeyboardInputProviderAcquireFocus", RoutingStrategy.Tunnel, typeof(KeyboardInputProviderAcquireFocusEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.KeyboardInputProviderAcquireFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.KeyboardInputProviderAcquireFocus" /> attached event.</returns>
	public static readonly RoutedEvent KeyboardInputProviderAcquireFocusEvent = EventManager.RegisterRoutedEvent("KeyboardInputProviderAcquireFocus", RoutingStrategy.Bubble, typeof(KeyboardInputProviderAcquireFocusEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event.</returns>
	public static readonly RoutedEvent GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("GotKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.PreviewLostKeyboardFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.PreviewLostKeyboardFocus" />  attached event.</returns>
	public static readonly RoutedEvent PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewLostKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event.</returns>
	public static readonly RoutedEvent LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Gets the element that has keyboard focus. </summary>
	/// <returns>The focused element.</returns>
	public static IInputElement FocusedElement => PrimaryDevice.FocusedElement;

	/// <summary>Gets or sets the behavior of Windows Presentation Foundation (WPF) when restoring focus.</summary>
	/// <returns>An enumeration value that specifies the behavior of WPF when restoring focus. The default in <see cref="F:System.Windows.Input.RestoreFocusMode.Auto" />.</returns>
	public static RestoreFocusMode DefaultRestoreFocusMode
	{
		get
		{
			return PrimaryDevice.DefaultRestoreFocusMode;
		}
		set
		{
			PrimaryDevice.DefaultRestoreFocusMode = value;
		}
	}

	/// <summary>Gets the set of <see cref="T:System.Windows.Input.ModifierKeys" /> that are currently pressed. </summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Windows.Input.ModifierKeys" /> values.</returns>
	public static ModifierKeys Modifiers => PrimaryDevice.Modifiers;

	/// <summary>Gets the primary keyboard input device. </summary>
	/// <returns>The device.</returns>
	public static KeyboardDevice PrimaryDevice => InputManager.UnsecureCurrent.PrimaryKeyboardDevice;

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddPreviewKeyDownHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewKeyDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemovePreviewKeyDownHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewKeyDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddKeyDownHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.AddHandler(element, KeyDownEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveKeyDownHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.RemoveHandler(element, KeyDownEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddPreviewKeyUpHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewKeyUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemovePreviewKeyUpHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewKeyUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddKeyUpHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.AddHandler(element, KeyUpEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyUp" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveKeyUpHandler(DependencyObject element, KeyEventHandler handler)
	{
		UIElement.RemoveHandler(element, KeyUpEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddPreviewGotKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewGotKeyboardFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewGotKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemovePreviewGotKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewGotKeyboardFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyboardInputProviderAcquireFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddPreviewKeyboardInputProviderAcquireFocusHandler(DependencyObject element, KeyboardInputProviderAcquireFocusEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewKeyboardInputProviderAcquireFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewKeyboardInputProviderAcquireFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemovePreviewKeyboardInputProviderAcquireFocusHandler(DependencyObject element, KeyboardInputProviderAcquireFocusEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewKeyboardInputProviderAcquireFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyboardInputProviderAcquireFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddKeyboardInputProviderAcquireFocusHandler(DependencyObject element, KeyboardInputProviderAcquireFocusEventHandler handler)
	{
		UIElement.AddHandler(element, KeyboardInputProviderAcquireFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.KeyboardInputProviderAcquireFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveKeyboardInputProviderAcquireFocusHandler(DependencyObject element, KeyboardInputProviderAcquireFocusEventHandler handler)
	{
		UIElement.RemoveHandler(element, KeyboardInputProviderAcquireFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddGotKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.AddHandler(element, GotKeyboardFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.GotKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveGotKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.RemoveHandler(element, GotKeyboardFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewLostKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddPreviewLostKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.AddHandler(element, PreviewLostKeyboardFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.PreviewLostKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemovePreviewLostKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.RemoveHandler(element, PreviewLostKeyboardFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddLostKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.AddHandler(element, LostKeyboardFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveLostKeyboardFocusHandler(DependencyObject element, KeyboardFocusChangedEventHandler handler)
	{
		UIElement.RemoveHandler(element, LostKeyboardFocusEvent, handler);
	}

	/// <summary>Clears focus. </summary>
	public static void ClearFocus()
	{
		PrimaryDevice.ClearFocus();
	}

	/// <summary>Sets keyboard focus on the specified element.</summary>
	/// <returns>The element with keyboard focus.</returns>
	/// <param name="element">The element on which to set keyboard focus.</param>
	public static IInputElement Focus(IInputElement element)
	{
		return PrimaryDevice.Focus(element);
	}

	/// <summary>Determines whether the specified key is pressed. </summary>
	/// <returns>true if <paramref name="key" /> is in the down state; otherwise, false.</returns>
	/// <param name="key">The specified key.</param>
	public static bool IsKeyDown(Key key)
	{
		return PrimaryDevice.IsKeyDown(key);
	}

	/// <summary>Determines whether the specified key is released. </summary>
	/// <returns>true if <paramref name="key" /> is in the up state; otherwise, false.</returns>
	/// <param name="key">The key to check.</param>
	public static bool IsKeyUp(Key key)
	{
		return PrimaryDevice.IsKeyUp(key);
	}

	/// <summary>Determines whether the specified key is toggled. </summary>
	/// <returns>true if <paramref name="key" /> is in the toggled state; otherwise, false.</returns>
	/// <param name="key">The specified key.</param>
	public static bool IsKeyToggled(Key key)
	{
		return PrimaryDevice.IsKeyToggled(key);
	}

	/// <summary>Gets the set of key states for the specified key.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Windows.Input.KeyStates" /> values.</returns>
	/// <param name="key">The specified key.</param>
	public static KeyStates GetKeyStates(Key key)
	{
		return PrimaryDevice.GetKeyStates(key);
	}

	internal static bool IsValidKey(Key key)
	{
		if (key >= Key.None)
		{
			return key <= Key.OemClear;
		}
		return false;
	}

	internal static bool IsFocusable(DependencyObject element)
	{
		if (element == null)
		{
			return false;
		}
		UIElement uIElement = element as UIElement;
		if (uIElement != null && !uIElement.IsVisible)
		{
			return false;
		}
		if (!(bool)element.GetValue(UIElement.IsEnabledProperty))
		{
			return false;
		}
		bool hasModifiers = false;
		BaseValueSourceInternal valueSource = element.GetValueSource(UIElement.FocusableProperty, null, out hasModifiers);
		bool flag = (bool)element.GetValue(UIElement.FocusableProperty);
		if (!flag && valueSource == BaseValueSourceInternal.Default && !hasModifiers)
		{
			if (FocusManager.GetIsFocusScope(element))
			{
				return true;
			}
			if (uIElement != null && uIElement.InternalVisualParent == null && PresentationSource.CriticalFromVisual(uIElement) != null)
			{
				return true;
			}
		}
		return flag;
	}
}

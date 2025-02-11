using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.Commands;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents the base class for all <see cref="T:System.Windows.Controls.Button" /> controls. </summary>
[DefaultEvent("Click")]
[Localizability(LocalizationCategory.Button)]
public abstract class ButtonBase : ContentControl, ICommandSource
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> routed event.</returns>
	public static readonly RoutedEvent ClickEvent;

	/// <summary>Identifies the routed <see cref="P:System.Windows.Controls.Primitives.ButtonBase.Command" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.Command" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty CommandProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.CommandParameter" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.CommandParameter" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty CommandParameterProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.CommandTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.CommandTarget" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty CommandTargetProperty;

	internal static readonly DependencyPropertyKey IsPressedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.IsPressed" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.IsPressed" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty IsPressedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.ClickMode" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.ClickMode" /> dependency property.</returns>
	public static readonly DependencyProperty ClickModeProperty;

	private bool IsInMainFocusScope
	{
		get
		{
			if (FocusManager.GetFocusScope(this) is Visual reference)
			{
				return VisualTreeHelper.GetParent(reference) == null;
			}
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.Primitives.ButtonBase" /> is currently activated.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.ButtonBase" /> is activated; otherwise false. The default is false.</returns>
	[Browsable(false)]
	[Category("Appearance")]
	[ReadOnly(true)]
	public bool IsPressed
	{
		get
		{
			return (bool)GetValue(IsPressedProperty);
		}
		protected set
		{
			SetValue(IsPressedPropertyKey, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the command to invoke when this button is pressed.  </summary>
	/// <returns>A command to invoke when this button is pressed. The default value is null.</returns>
	[Bindable(true)]
	[Category("Action")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public ICommand Command
	{
		get
		{
			return (ICommand)GetValue(CommandProperty);
		}
		set
		{
			SetValue(CommandProperty, value);
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.ContentElement.IsEnabled" /> property.</summary>
	/// <returns>true if the control is enabled; otherwise, false.</returns>
	protected override bool IsEnabledCore
	{
		get
		{
			if (base.IsEnabledCore)
			{
				return CanExecute;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the parameter to pass to the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.Command" /> property.  </summary>
	/// <returns>Parameter to pass to the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.Command" /> property.</returns>
	[Bindable(true)]
	[Category("Action")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public object CommandParameter
	{
		get
		{
			return GetValue(CommandParameterProperty);
		}
		set
		{
			SetValue(CommandParameterProperty, value);
		}
	}

	/// <summary>Gets or sets the element on which to raise the specified command.  </summary>
	/// <returns>Element on which to raise a command.</returns>
	[Bindable(true)]
	[Category("Action")]
	public IInputElement CommandTarget
	{
		get
		{
			return (IInputElement)GetValue(CommandTargetProperty);
		}
		set
		{
			SetValue(CommandTargetProperty, value);
		}
	}

	/// <summary>Gets or sets when the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event occurs.  </summary>
	/// <returns>When the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event occurs. The default value is <see cref="F:System.Windows.Controls.ClickMode.Release" />. </returns>
	[Bindable(true)]
	[Category("Behavior")]
	public ClickMode ClickMode
	{
		get
		{
			return (ClickMode)GetValue(ClickModeProperty);
		}
		set
		{
			SetValue(ClickModeProperty, value);
		}
	}

	private bool IsSpaceKeyDown
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.IsSpaceKeyDown);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.IsSpaceKeyDown, value);
		}
	}

	private bool CanExecute
	{
		get
		{
			return !ReadControlFlag(ControlBoolFlags.CommandDisabled);
		}
		set
		{
			if (value != CanExecute)
			{
				WriteControlFlag(ControlBoolFlags.CommandDisabled, !value);
				CoerceValue(UIElement.IsEnabledProperty);
			}
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.Button" /> is clicked. </summary>
	[Category("Behavior")]
	public event RoutedEventHandler Click
	{
		add
		{
			AddHandler(ClickEvent, value);
		}
		remove
		{
			RemoveHandler(ClickEvent, value);
		}
	}

	static ButtonBase()
	{
		ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ButtonBase));
		CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ButtonBase), new FrameworkPropertyMetadata(null, OnCommandChanged));
		CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ButtonBase), new FrameworkPropertyMetadata(null, OnCommandParameterChanged));
		CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ButtonBase), new FrameworkPropertyMetadata((object)null));
		IsPressedPropertyKey = DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(ButtonBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsPressedChanged));
		IsPressedProperty = IsPressedPropertyKey.DependencyProperty;
		ClickModeProperty = DependencyProperty.Register("ClickMode", typeof(ClickMode), typeof(ButtonBase), new FrameworkPropertyMetadata(ClickMode.Release), IsValidClickMode);
		EventManager.RegisterClassHandler(typeof(ButtonBase), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(typeof(ButtonBase), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		InputMethod.IsInputMethodEnabledProperty.OverrideMetadata(typeof(ButtonBase), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(ButtonBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(ButtonBase), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ButtonBase" /> class. </summary>
	protected ButtonBase()
	{
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> routed event. </summary>
	protected virtual void OnClick()
	{
		RoutedEventArgs e = new RoutedEventArgs(ClickEvent, this);
		RaiseEvent(e);
		CommandHelpers.ExecuteCommandSource(this);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.IsPressed" /> property changes.</summary>
	/// <param name="e">The data for <see cref="T:System.Windows.DependencyPropertyChangedEventArgs" />.</param>
	protected virtual void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
	{
		Control.OnVisualStatePropertyChanged(this, e);
	}

	internal void AutomationButtonBaseClick()
	{
		OnClick();
	}

	private static bool IsValidClickMode(object o)
	{
		ClickMode clickMode = (ClickMode)o;
		if (clickMode != ClickMode.Press && clickMode != 0)
		{
			return clickMode == ClickMode.Hover;
		}
		return true;
	}

	/// <summary> Called when the rendered size of a control changes. </summary>
	/// <param name="sizeInfo">Specifies the size changes.</param>
	protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		if (base.IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed && !IsSpaceKeyDown)
		{
			UpdateIsPressed();
		}
	}

	private static void OnIsPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ButtonBase)d).OnIsPressedChanged(e);
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		if (!e.Handled && e.Scope == null && e.Target == null)
		{
			e.Target = (UIElement)sender;
		}
	}

	private void UpdateIsPressed()
	{
		Point position = Mouse.PrimaryDevice.GetPosition(this);
		if (position.X >= 0.0 && position.X <= base.ActualWidth && position.Y >= 0.0 && position.Y <= base.ActualHeight)
		{
			if (!IsPressed)
			{
				SetIsPressed(pressed: true);
			}
		}
		else if (IsPressed)
		{
			SetIsPressed(pressed: false);
		}
	}

	private void SetIsPressed(bool pressed)
	{
		if (pressed)
		{
			SetValue(IsPressedPropertyKey, BooleanBoxes.Box(pressed));
		}
		else
		{
			ClearValue(IsPressedPropertyKey);
		}
	}

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ButtonBase)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
	}

	private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
	{
		if (oldCommand != null)
		{
			UnhookCommand(oldCommand);
		}
		if (newCommand != null)
		{
			HookCommand(newCommand);
		}
	}

	private void UnhookCommand(ICommand command)
	{
		CanExecuteChangedEventManager.RemoveHandler(command, OnCanExecuteChanged);
		UpdateCanExecute();
	}

	private void HookCommand(ICommand command)
	{
		CanExecuteChangedEventManager.AddHandler(command, OnCanExecuteChanged);
		UpdateCanExecute();
	}

	private void OnCanExecuteChanged(object sender, EventArgs e)
	{
		UpdateCanExecute();
	}

	private void UpdateCanExecute()
	{
		if (Command != null)
		{
			CanExecute = CommandHelpers.CanExecuteCommandSource(this);
		}
		else
		{
			CanExecute = true;
		}
	}

	private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ButtonBase)d).UpdateCanExecute();
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> routed event that occurs when the left mouse button is pressed while the mouse pointer is over this control.</summary>
	/// <param name="e">The event data. </param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (ClickMode != ClickMode.Hover)
		{
			e.Handled = true;
			Focus();
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				CaptureMouse();
				if (base.IsMouseCaptured)
				{
					if (e.ButtonState == MouseButtonState.Pressed)
					{
						if (!IsPressed)
						{
							SetIsPressed(pressed: true);
						}
					}
					else
					{
						ReleaseMouseCapture();
					}
				}
			}
			if (ClickMode == ClickMode.Press)
			{
				bool flag = true;
				try
				{
					OnClick();
					flag = false;
				}
				finally
				{
					if (flag)
					{
						SetIsPressed(pressed: false);
						ReleaseMouseCapture();
					}
				}
			}
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> routed event that occurs when the left mouse button is released while the mouse pointer is over this control. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		if (ClickMode != ClickMode.Hover)
		{
			e.Handled = true;
			bool num = !IsSpaceKeyDown && IsPressed && ClickMode == ClickMode.Release;
			if (base.IsMouseCaptured && !IsSpaceKeyDown)
			{
				ReleaseMouseCapture();
			}
			if (num)
			{
				OnClick();
			}
		}
		base.OnMouseLeftButtonUp(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseMove" /> routed event that occurs when the mouse pointer moves while over this element.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (ClickMode != ClickMode.Hover && base.IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed && !IsSpaceKeyDown)
		{
			UpdateIsPressed();
			e.Handled = true;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> routed event that occurs when this control is no longer receiving mouse event messages. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> event.</param>
	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		if (e.OriginalSource == this && ClickMode != ClickMode.Hover && !IsSpaceKeyDown)
		{
			if (base.IsKeyboardFocused && !IsInMainFocusScope)
			{
				Keyboard.Focus(null);
			}
			SetIsPressed(pressed: false);
		}
	}

	/// <summary>Provides class handling for the <see cref="P:System.Windows.Controls.Primitives.ButtonBase.ClickMode" /> routed event that occurs when the mouse enters this control. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> event.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseEnter(e);
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeave" /> routed event that occurs when the mouse leaves an element. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.MouseLeave" /> event.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	private bool HandleIsMouseOverChanged()
	{
		if (ClickMode == ClickMode.Hover)
		{
			if (base.IsMouseOver)
			{
				SetIsPressed(pressed: true);
				OnClick();
			}
			else
			{
				SetIsPressed(pressed: false);
			}
			return true;
		}
		return false;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyDown" /> routed event that occurs when the user presses a key while this control has focus.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (ClickMode == ClickMode.Hover)
		{
			return;
		}
		if (e.Key == Key.Space)
		{
			if ((Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) != ModifierKeys.Alt && !base.IsMouseCaptured && e.OriginalSource == this)
			{
				IsSpaceKeyDown = true;
				SetIsPressed(pressed: true);
				CaptureMouse();
				if (ClickMode == ClickMode.Press)
				{
					OnClick();
				}
				e.Handled = true;
			}
		}
		else if (e.Key == Key.Return && (bool)GetValue(KeyboardNavigation.AcceptsReturnProperty))
		{
			if (e.OriginalSource == this)
			{
				IsSpaceKeyDown = false;
				SetIsPressed(pressed: false);
				if (base.IsMouseCaptured)
				{
					ReleaseMouseCapture();
				}
				OnClick();
				e.Handled = true;
			}
		}
		else if (IsSpaceKeyDown)
		{
			SetIsPressed(pressed: false);
			IsSpaceKeyDown = false;
			if (base.IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.KeyUp" /> routed event that occurs when the user releases a key while this control has focus.</summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.UIElement.KeyUp" /> event.</param>
	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (ClickMode == ClickMode.Hover || e.Key != Key.Space || !IsSpaceKeyDown || (Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) == ModifierKeys.Alt)
		{
			return;
		}
		IsSpaceKeyDown = false;
		if (GetMouseLeftButtonReleased())
		{
			bool num = IsPressed && ClickMode == ClickMode.Release;
			if (base.IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
			if (num)
			{
				OnClick();
			}
		}
		else if (base.IsMouseCaptured)
		{
			UpdateIsPressed();
		}
		e.Handled = true;
	}

	/// <summary> Called when an element loses keyboard focus. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.IInputElement.LostKeyboardFocus" /> event.</param>
	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		if (ClickMode != ClickMode.Hover && e.OriginalSource == this)
		{
			if (IsPressed)
			{
				SetIsPressed(pressed: false);
			}
			if (base.IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
			IsSpaceKeyDown = false;
		}
	}

	/// <summary>Responds when the <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for this control is called. </summary>
	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.AccessKeyManager.AccessKeyPressed" /> event.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (e.IsMultiple)
		{
			base.OnAccessKey(e);
		}
		else
		{
			OnClick();
		}
	}

	private bool GetMouseLeftButtonReleased()
	{
		return InputManager.Current.PrimaryMouseDevice.LeftButton == MouseButtonState.Released;
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStateManager.GoToState(this, "Disabled", useTransitions);
		}
		else if (IsPressed)
		{
			VisualStateManager.GoToState(this, "Pressed", useTransitions);
		}
		else if (base.IsMouseOver)
		{
			VisualStateManager.GoToState(this, "MouseOver", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		if (base.IsKeyboardFocused)
		{
			VisualStateManager.GoToState(this, "Focused", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Unfocused", useTransitions);
		}
		base.ChangeVisualState(useTransitions);
	}
}

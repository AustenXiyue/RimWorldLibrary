using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MS.Internal.Commands;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a Windows button control, which reacts to the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event.</summary>
public class Button : ButtonBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Button.IsDefault" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Button.IsDefault" /> dependency property.</returns>
	public static readonly DependencyProperty IsDefaultProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Button.IsCancel" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Button.IsCancel" /> dependency property.</returns>
	public static readonly DependencyProperty IsCancelProperty;

	private static readonly DependencyPropertyKey IsDefaultedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Button.IsDefaulted" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Button.IsDefaulted" /> dependency property.</returns>
	public static readonly DependencyProperty IsDefaultedProperty;

	private static readonly UncommonField<KeyboardFocusChangedEventHandler> FocusChangedEventHandlerField;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.Button" /> is the default button. A user invokes the default button by pressing the ENTER key.   </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Button" /> is the default button; otherwise, false. The default is false.</returns>
	public bool IsDefault
	{
		get
		{
			return (bool)GetValue(IsDefaultProperty);
		}
		set
		{
			SetValue(IsDefaultProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.Button" /> is a Cancel button. A user can activate the Cancel button by pressing the ESC key.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Button" /> is a Cancel button; otherwise, false. The default is false.</returns>
	public bool IsCancel
	{
		get
		{
			return (bool)GetValue(IsCancelProperty);
		}
		set
		{
			SetValue(IsCancelProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates whether a <see cref="T:System.Windows.Controls.Button" /> is the button that is activated when a user presses ENTER.   </summary>
	/// <returns>true if the button is activated when the user presses ENTER; otherwise, false. The default is false.</returns>
	public bool IsDefaulted => (bool)GetValue(IsDefaultedProperty);

	internal override int EffectiveValuesInitialSize => 42;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static Button()
	{
		IsDefaultProperty = DependencyProperty.Register("IsDefault", typeof(bool), typeof(Button), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsDefaultChanged));
		IsCancelProperty = DependencyProperty.Register("IsCancel", typeof(bool), typeof(Button), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsCancelChanged));
		IsDefaultedPropertyKey = DependencyProperty.RegisterReadOnly("IsDefaulted", typeof(bool), typeof(Button), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsDefaultedProperty = IsDefaultedPropertyKey.DependencyProperty;
		FocusChangedEventHandlerField = new UncommonField<KeyboardFocusChangedEventHandler>();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Button));
		if (ButtonBase.CommandProperty != null)
		{
			UIElement.IsEnabledProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(OnIsEnabledChanged));
		}
		ControlsTraceLogger.AddControl(TelemetryControls.Button);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Button" /> class. </summary>
	public Button()
	{
	}

	private static void OnIsDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Button button = d as Button;
		KeyboardFocusChangedEventHandler keyboardFocusChangedEventHandler = FocusChangedEventHandlerField.GetValue(button);
		if (keyboardFocusChangedEventHandler == null)
		{
			keyboardFocusChangedEventHandler = button.OnFocusChanged;
			FocusChangedEventHandlerField.SetValue(button, keyboardFocusChangedEventHandler);
		}
		if ((bool)e.NewValue)
		{
			AccessKeyManager.Register("\r", button);
			KeyboardNavigation.Current.FocusChanged += keyboardFocusChangedEventHandler;
			button.UpdateIsDefaulted(Keyboard.FocusedElement);
		}
		else
		{
			AccessKeyManager.Unregister("\r", button);
			KeyboardNavigation.Current.FocusChanged -= keyboardFocusChangedEventHandler;
			button.UpdateIsDefaulted(null);
		}
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Button button = (Button)d;
		if (button.IsDefault)
		{
			button.UpdateIsDefaulted(Keyboard.FocusedElement);
		}
	}

	private static void OnIsCancelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Button element = d as Button;
		if ((bool)e.NewValue)
		{
			AccessKeyManager.Register("\u001b", element);
		}
		else
		{
			AccessKeyManager.Unregister("\u001b", element);
		}
	}

	private void OnFocusChanged(object sender, KeyboardFocusChangedEventArgs e)
	{
		UpdateIsDefaulted(Keyboard.FocusedElement);
	}

	private void UpdateIsDefaulted(IInputElement focus)
	{
		if (!IsDefault || focus == null || !base.IsEnabled)
		{
			SetValue(IsDefaultedPropertyKey, BooleanBoxes.FalseBox);
			return;
		}
		DependencyObject dependencyObject = focus as DependencyObject;
		object value = BooleanBoxes.FalseBox;
		try
		{
			AccessKeyPressedEventArgs accessKeyPressedEventArgs = new AccessKeyPressedEventArgs();
			focus.RaiseEvent(accessKeyPressedEventArgs);
			object scope = accessKeyPressedEventArgs.Scope;
			accessKeyPressedEventArgs = new AccessKeyPressedEventArgs();
			RaiseEvent(accessKeyPressedEventArgs);
			if (accessKeyPressedEventArgs.Scope == scope && (dependencyObject == null || !(bool)dependencyObject.GetValue(KeyboardNavigation.AcceptsReturnProperty)))
			{
				value = BooleanBoxes.TrueBox;
			}
		}
		finally
		{
			SetValue(IsDefaultedPropertyKey, value);
		}
	}

	/// <summary>Creates an appropriate <see cref="T:System.Windows.Automation.Peers.ButtonAutomationPeer" /> for this control as part of the WPF infrastructure.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.ButtonAutomationPeer" /> for this control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ButtonAutomationPeer(this);
	}

	/// <summary>Called when a <see cref="T:System.Windows.Controls.Button" /> is clicked. </summary>
	protected override void OnClick()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
		{
			UIElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
		try
		{
			base.OnClick();
		}
		finally
		{
			if (base.Command == null && IsCancel)
			{
				CommandHelpers.ExecuteCommand(Window.DialogCancelCommand, null, this);
			}
		}
	}
}

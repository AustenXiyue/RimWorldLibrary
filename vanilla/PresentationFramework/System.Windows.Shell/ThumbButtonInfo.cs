using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Shell;

/// <summary>Represents information about how to display a button in the Windows 7 taskbar thumbnail.</summary>
public sealed class ThumbButtonInfo : Freezable, ICommandSource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Visibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Visibility" /> dependency property.</returns>
	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ThumbButtonInfo), new PropertyMetadata(Visibility.Visible));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.DismissWhenClicked" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.DismissWhenClicked" /> dependency property.</returns>
	public static readonly DependencyProperty DismissWhenClickedProperty = DependencyProperty.Register("DismissWhenClicked", typeof(bool), typeof(ThumbButtonInfo), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.ImageSource" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.ImageSource" /> dependency property.</returns>
	public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ThumbButtonInfo), new PropertyMetadata(null));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsBackgroundVisible" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsBackgroundVisible" /> dependency property.</returns>
	public static readonly DependencyProperty IsBackgroundVisibleProperty = DependencyProperty.Register("IsBackgroundVisible", typeof(bool), typeof(ThumbButtonInfo), new PropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Description" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Description" /> dependency property.</returns>
	public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ThumbButtonInfo), new PropertyMetadata(string.Empty, null, CoerceDescription));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsEnabled" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ThumbButtonInfo), new PropertyMetadata(true, null, (DependencyObject d, object e) => ((ThumbButtonInfo)d).CoerceIsEnabledValue(e)));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsInteractive" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.IsInteractive" /> dependency property.</returns>
	public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register("IsInteractive", typeof(bool), typeof(ThumbButtonInfo), new PropertyMetadata(true));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Command" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Command" /> dependency property.</returns>
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ThumbButtonInfo), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ThumbButtonInfo)d).OnCommandChanged(e);
	}));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.CommandParameter" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.CommandParameter" /> dependency property.</returns>
	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ThumbButtonInfo), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ThumbButtonInfo)d).UpdateCanExecute();
	}));

	/// <summary>Identifies the <see cref="P:System.Windows.Shell.ThumbButtonInfo.CommandTarget" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Shell.ThumbButtonInfo.CommandTarget" /> dependency property.</returns>
	public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ThumbButtonInfo), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ThumbButtonInfo)d).UpdateCanExecute();
	}));

	private static readonly DependencyProperty _CanExecuteProperty = DependencyProperty.Register("_CanExecute", typeof(bool), typeof(ThumbButtonInfo), new PropertyMetadata(true, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(IsEnabledProperty);
	}));

	/// <summary>Gets or sets a value that specifies the display state of the thumbnail button.</summary>
	/// <returns>An enumeration value that specifies the display state of the thumbnail button. The default is <see cref="F:System.Windows.Visibility.Visible" />.</returns>
	public Visibility Visibility
	{
		get
		{
			return (Visibility)GetValue(VisibilityProperty);
		}
		set
		{
			SetValue(VisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the taskbar thumbnail closes when the thumbnail button is clicked.</summary>
	/// <returns>true if the thumbnail closes; otherwise, false. The default is false.</returns>
	public bool DismissWhenClicked
	{
		get
		{
			return (bool)GetValue(DismissWhenClickedProperty);
		}
		set
		{
			SetValue(DismissWhenClickedProperty, value);
		}
	}

	/// <summary>Gets or sets the image that is displayed on the thumbnail button.</summary>
	/// <returns>The image that is displayed on the thumbnail button. The default is null.</returns>
	public ImageSource ImageSource
	{
		get
		{
			return (ImageSource)GetValue(ImageSourceProperty);
		}
		set
		{
			SetValue(ImageSourceProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a border and highlight is displayed around the thumbnail button.</summary>
	/// <returns>true if a border and highlight is displayed around the thumbnail button; otherwise, false. The default is true.</returns>
	public bool IsBackgroundVisible
	{
		get
		{
			return (bool)GetValue(IsBackgroundVisibleProperty);
		}
		set
		{
			SetValue(IsBackgroundVisibleProperty, value);
		}
	}

	/// <summary>Gets or sets the text to display for the thumbnail button tooltip.</summary>
	/// <returns>The text to display for the thumbnail button tooltip. The default is an empty string.</returns>
	public string Description
	{
		get
		{
			return (string)GetValue(DescriptionProperty);
		}
		set
		{
			SetValue(DescriptionProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the thumbnail button is enabled.</summary>
	/// <returns>true if the thumbnail button is enabled; otherwise, false. The default is true.</returns>
	public bool IsEnabled
	{
		get
		{
			return (bool)GetValue(IsEnabledProperty);
		}
		set
		{
			SetValue(IsEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can interact with the thumbnail button.</summary>
	/// <returns>true if the user can interact with the thumbnail button; otherwise, false. The default is true.</returns>
	public bool IsInteractive
	{
		get
		{
			return (bool)GetValue(IsInteractiveProperty);
		}
		set
		{
			SetValue(IsInteractiveProperty, value);
		}
	}

	private bool CanExecute
	{
		get
		{
			return (bool)GetValue(_CanExecuteProperty);
		}
		set
		{
			SetValue(_CanExecuteProperty, value);
		}
	}

	/// <summary>Gets or sets the command to invoke when this thumbnail button is clicked.</summary>
	/// <returns>The command to invoke when this thumbnail button is clicked. The default is null.</returns>
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

	/// <summary>Gets or sets the parameter to pass to the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Command" /> property.</summary>
	/// <returns>The parameter to pass to the <see cref="P:System.Windows.Shell.ThumbButtonInfo.Command" /> property. The default is null.</returns>
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

	/// <summary>Gets or sets the element on which to raise the specified command.</summary>
	/// <returns>The element on which to raise the specified command. The default is null.</returns>
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

	/// <summary>Occurs when the thumbnail button is clicked.</summary>
	public event EventHandler Click;

	protected override Freezable CreateInstanceCore()
	{
		return new ThumbButtonInfo();
	}

	private static object CoerceDescription(DependencyObject d, object value)
	{
		string text = (string)value;
		if (text != null && text.Length >= 260)
		{
			text = text.Substring(0, 259);
		}
		return text;
	}

	private object CoerceIsEnabledValue(object value)
	{
		return (bool)value && CanExecute;
	}

	private void OnCommandChanged(DependencyPropertyChangedEventArgs e)
	{
		ICommand command = (ICommand)e.OldValue;
		ICommand command2 = (ICommand)e.NewValue;
		if (command != command2)
		{
			if (command != null)
			{
				UnhookCommand(command);
			}
			if (command2 != null)
			{
				HookCommand(command2);
			}
		}
	}

	internal void InvokeClick()
	{
		this.Click?.Invoke(this, EventArgs.Empty);
		_InvokeCommand();
	}

	private void _InvokeCommand()
	{
		ICommand command = Command;
		if (command == null)
		{
			return;
		}
		object commandParameter = CommandParameter;
		IInputElement commandTarget = CommandTarget;
		if (command is RoutedCommand routedCommand)
		{
			if (routedCommand.CanExecute(commandParameter, commandTarget))
			{
				routedCommand.Execute(commandParameter, commandTarget);
			}
		}
		else if (command.CanExecute(commandParameter))
		{
			command.Execute(commandParameter);
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
			object commandParameter = CommandParameter;
			IInputElement commandTarget = CommandTarget;
			if (Command is RoutedCommand routedCommand)
			{
				CanExecute = routedCommand.CanExecute(commandParameter, commandTarget);
			}
			else
			{
				CanExecute = Command.CanExecute(commandParameter);
			}
		}
		else
		{
			CanExecute = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.ThumbButtonInfo" /> class.</summary>
	public ThumbButtonInfo()
	{
	}
}

using System.ComponentModel;
using MS.Internal;

namespace System.Windows.Input;

/// <summary>Represents a binding between an <see cref="T:System.Windows.Input.InputGesture" /> and a command. The command is potentially a <see cref="T:System.Windows.Input.RoutedCommand" />. </summary>
public class InputBinding : Freezable, ICommandSource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputBinding.Command" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.InputBinding.Command" /> dependency property.</returns>
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(InputBinding), new UIPropertyMetadata(null, OnCommandPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputBinding.CommandParameter" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.InputBinding.CommandParameter" /> dependency property.</returns>
	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(InputBinding));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.InputBinding.CommandTarget" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.InputBinding.CommandTarget" /> dependency property.</returns>
	public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(InputBinding));

	private InputGesture _gesture;

	internal static object _dataLock = new object();

	private DependencyObject _inheritanceContext;

	private bool _hasMultipleInheritanceContexts;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.ICommand" /> associated with this input binding. </summary>
	/// <returns>The associated command.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Windows.Input.InputBinding.Command" /> value is null.</exception>
	[TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
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

	/// <summary>Gets or sets the command-specific data for a particular command.</summary>
	/// <returns>The command-specific data. The default is null.</returns>
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

	/// <summary>Gets or sets the target element of the command.</summary>
	/// <returns>The target of the command. The default is null.</returns>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.InputGesture" /> associated with this input binding. </summary>
	/// <returns>The associated gesture. The default is null.</returns>
	public virtual InputGesture Gesture
	{
		get
		{
			ReadPreamble();
			return _gesture;
		}
		set
		{
			WritePreamble();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			lock (_dataLock)
			{
				_gesture = value;
			}
			WritePostscript();
		}
	}

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	internal override bool HasMultipleInheritanceContexts => _hasMultipleInheritanceContexts;

	/// <summary>Provides base initialization for classes derived from <see cref="T:System.Windows.Input.InputBinding" />. </summary>
	protected InputBinding()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputBinding" /> class with the specified command and input gesture.</summary>
	/// <param name="command">The command to associate with <paramref name="gesture" />.</param>
	/// <param name="gesture">The input gesture to associate with <paramref name="command" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="command" /> or <paramref name="gesture" /> is null.</exception>
	public InputBinding(ICommand command, InputGesture gesture)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		if (gesture == null)
		{
			throw new ArgumentNullException("gesture");
		}
		Command = command;
		_gesture = gesture;
	}

	private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	/// <summary>Creates an instance of an <see cref="T:System.Windows.Input.InputBinding" />.</summary>
	/// <returns>The new object.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new InputBinding();
	}

	/// <summary>Copies the base (non-animated) values of the properties of the specified object.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		base.CloneCore(sourceFreezable);
		_gesture = ((InputBinding)sourceFreezable).Gesture;
	}

	/// <summary>Copies the current values of the properties of the specified object.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		base.CloneCurrentValueCore(sourceFreezable);
		_gesture = ((InputBinding)sourceFreezable).Gesture;
	}

	/// <summary>Makes the instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" /> by using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetAsFrozenCore(sourceFreezable);
		_gesture = ((InputBinding)sourceFreezable).Gesture;
	}

	/// <summary>Makes the current instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If the object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		_gesture = ((InputBinding)sourceFreezable).Gesture;
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		InheritanceContextHelper.AddInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		InheritanceContextHelper.RemoveInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
	}
}

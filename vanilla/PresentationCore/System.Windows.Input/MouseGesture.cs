using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Defines a mouse input gesture that can be used to invoke a command.</summary>
[TypeConverter(typeof(MouseGestureConverter))]
[ValueSerializer(typeof(MouseGestureValueSerializer))]
public class MouseGesture : InputGesture
{
	private MouseAction _mouseAction;

	private ModifierKeys _modifiers;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.MouseAction" /> associated with this gesture. </summary>
	/// <returns>The mouse action associated with this gesture. The default value is <see cref="F:System.Windows.Input.MouseAction.None" />.</returns>
	public MouseAction MouseAction
	{
		get
		{
			return _mouseAction;
		}
		set
		{
			if (!IsDefinedMouseAction(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(MouseAction));
			}
			if (_mouseAction != value)
			{
				_mouseAction = value;
				OnPropertyChanged("MouseAction");
			}
		}
	}

	/// <summary>Gets or sets the modifier keys associated with this <see cref="T:System.Windows.Input.MouseGesture" />.</summary>
	/// <returns>The modifier keys associated with this gesture. The default value is <see cref="F:System.Windows.Input.ModifierKeys.None" />.</returns>
	public ModifierKeys Modifiers
	{
		get
		{
			return _modifiers;
		}
		set
		{
			if (!ModifierKeysConverter.IsDefinedModifierKeys(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ModifierKeys));
			}
			if (_modifiers != value)
			{
				_modifiers = value;
				OnPropertyChanged("Modifiers");
			}
		}
	}

	internal event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseGesture" /> class.</summary>
	public MouseGesture()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseGesture" /> class using the specified <see cref="T:System.Windows.Input.MouseAction" />.</summary>
	/// <param name="mouseAction">The action associated with this gesture.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="mouseAction" /> is not a valid <see cref="T:System.Windows.Input.MouseAction" /> value.</exception>
	public MouseGesture(MouseAction mouseAction)
		: this(mouseAction, ModifierKeys.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseGesture" /> class using the specified <see cref="T:System.Windows.Input.MouseAction" /> and <see cref="T:System.Windows.Input.ModifierKeys" />.</summary>
	/// <param name="mouseAction">The action associated with this gesture.</param>
	/// <param name="modifiers">The modifiers associated with this gesture.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="mouseAction" /> is not a valid <see cref="T:System.Windows.Input.MouseAction" /> value-or-<paramref name="modifiers" /> is not a valid <see cref="T:System.Windows.Input.ModifierKeys" /> value.</exception>
	public MouseGesture(MouseAction mouseAction, ModifierKeys modifiers)
	{
		if (!IsDefinedMouseAction(mouseAction))
		{
			throw new InvalidEnumArgumentException("mouseAction", (int)mouseAction, typeof(MouseAction));
		}
		if (!ModifierKeysConverter.IsDefinedModifierKeys(modifiers))
		{
			throw new InvalidEnumArgumentException("modifiers", (int)modifiers, typeof(ModifierKeys));
		}
		_modifiers = modifiers;
		_mouseAction = mouseAction;
	}

	/// <summary>Determines whether <see cref="T:System.Windows.Input.MouseGesture" /> matches the input associated with the specified <see cref="T:System.Windows.Input.InputEventArgs" /> object.</summary>
	/// <returns>true if the event data matches this <see cref="T:System.Windows.Input.MouseGesture" />; otherwise, false.</returns>
	/// <param name="targetElement">The target.</param>
	/// <param name="inputEventArgs">The input event data to compare with this gesture.</param>
	public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
	{
		MouseAction mouseAction = GetMouseAction(inputEventArgs);
		if (mouseAction != 0)
		{
			if (MouseAction == mouseAction)
			{
				return Modifiers == Keyboard.Modifiers;
			}
			return false;
		}
		return false;
	}

	internal static bool IsDefinedMouseAction(MouseAction mouseAction)
	{
		if ((int)mouseAction >= 0)
		{
			return (int)mouseAction <= 7;
		}
		return false;
	}

	internal virtual void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	internal static MouseAction GetMouseAction(InputEventArgs inputArgs)
	{
		MouseAction result = MouseAction.None;
		if (inputArgs is MouseEventArgs)
		{
			if (inputArgs is MouseWheelEventArgs)
			{
				result = MouseAction.WheelClick;
			}
			else
			{
				MouseButtonEventArgs mouseButtonEventArgs = inputArgs as MouseButtonEventArgs;
				switch (mouseButtonEventArgs.ChangedButton)
				{
				case MouseButton.Left:
					if (mouseButtonEventArgs.ClickCount == 2)
					{
						result = MouseAction.LeftDoubleClick;
					}
					else if (mouseButtonEventArgs.ClickCount == 1)
					{
						result = MouseAction.LeftClick;
					}
					break;
				case MouseButton.Right:
					if (mouseButtonEventArgs.ClickCount == 2)
					{
						result = MouseAction.RightDoubleClick;
					}
					else if (mouseButtonEventArgs.ClickCount == 1)
					{
						result = MouseAction.RightClick;
					}
					break;
				case MouseButton.Middle:
					if (mouseButtonEventArgs.ClickCount == 2)
					{
						result = MouseAction.MiddleDoubleClick;
					}
					else if (mouseButtonEventArgs.ClickCount == 1)
					{
						result = MouseAction.MiddleClick;
					}
					break;
				}
			}
		}
		return result;
	}
}

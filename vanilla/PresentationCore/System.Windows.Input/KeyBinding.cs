using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Binds a <see cref="T:System.Windows.Input.KeyGesture" /> to a <see cref="T:System.Windows.Input.RoutedCommand" /> (or another  <see cref="T:System.Windows.Input.ICommand" /> implementation).</summary>
public class KeyBinding : InputBinding
{
	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyBinding.Modifiers" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyBinding.Modifiers" /> dependency property.</returns>
	public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyBinding), new UIPropertyMetadata(ModifierKeys.None, OnModifiersPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyBinding.Key" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyBinding.Key" /> dependency property.</returns>
	public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(KeyBinding), new UIPropertyMetadata(Key.None, OnKeyPropertyChanged));

	private bool _settingGesture;

	/// <summary>Gets or sets the gesture associated with this <see cref="T:System.Windows.Input.KeyBinding" />. </summary>
	/// <returns>The key sequence. The default value is null.</returns>
	/// <exception cref="T:System.ArgumentException">the value <paramref name="gesture" /> is being set to is not a <see cref="T:System.Windows.Input.KeyGesture" />.</exception>
	[TypeConverter(typeof(KeyGestureConverter))]
	[ValueSerializer(typeof(KeyGestureValueSerializer))]
	public override InputGesture Gesture
	{
		get
		{
			return base.Gesture as KeyGesture;
		}
		set
		{
			if (value is KeyGesture keyGesture)
			{
				base.Gesture = value;
				SynchronizePropertiesFromGesture(keyGesture);
				return;
			}
			throw new ArgumentException(SR.Format(SR.InputBinding_ExpectedInputGesture, typeof(KeyGesture)));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.ModifierKeys" /> of the <see cref="T:System.Windows.Input.KeyGesture" /> associated with this <see cref="T:System.Windows.Input.KeyBinding" />. </summary>
	/// <returns>The modifier keys of the <see cref="T:System.Windows.Input.KeyGesture" />.  The default value is <see cref="F:System.Windows.Input.ModifierKeys.None" />.</returns>
	public ModifierKeys Modifiers
	{
		get
		{
			return (ModifierKeys)GetValue(ModifiersProperty);
		}
		set
		{
			SetValue(ModifiersProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.Key" /> of the <see cref="T:System.Windows.Input.KeyGesture" /> associated with this <see cref="T:System.Windows.Input.KeyBinding" />.</summary>
	/// <returns>The key part of the <see cref="T:System.Windows.Input.KeyGesture" />. The default value is <see cref="F:System.Windows.Input.Key.None" />.</returns>
	public Key Key
	{
		get
		{
			return (Key)GetValue(KeyProperty);
		}
		set
		{
			SetValue(KeyProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyBinding" /> class. </summary>
	public KeyBinding()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyBinding" /> class using the specified <see cref="T:System.Windows.Input.ICommand" /> and <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <param name="command">The command to associate with <paramref name="gesture" />.</param>
	/// <param name="gesture">The key combination to associate with <paramref name="command" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="command" /> or <paramref name="gesture" /> is null.</exception>
	public KeyBinding(ICommand command, KeyGesture gesture)
		: base(command, gesture)
	{
		SynchronizePropertiesFromGesture(gesture);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyBinding" /> class using the specified <see cref="T:System.Windows.Input.ICommand" /> and the specified <see cref="T:System.Windows.Input.Key" /> and <see cref="T:System.Windows.Input.ModifierKeys" /> which will be converted into a <see cref="T:System.Windows.Input.KeyGesture" />. </summary>
	/// <param name="command">The command to invoke.</param>
	/// <param name="key">The key to be associated with <paramref name="command" />.</param>
	/// <param name="modifiers">The modifiers to be associated with <paramref name="command" />.</param>
	public KeyBinding(ICommand command, Key key, ModifierKeys modifiers)
		: this(command, new KeyGesture(key, modifiers))
	{
	}

	private static void OnModifiersPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		KeyBinding obj = (KeyBinding)d;
		obj.SynchronizeGestureFromProperties(obj.Key, (ModifierKeys)e.NewValue);
	}

	private static void OnKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		KeyBinding keyBinding = (KeyBinding)d;
		keyBinding.SynchronizeGestureFromProperties((Key)e.NewValue, keyBinding.Modifiers);
	}

	/// <summary>Creates an instance of a <see cref="T:System.Windows.Input.KeyBinding" />.</summary>
	/// <returns>The new object.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new KeyBinding();
	}

	private void SynchronizePropertiesFromGesture(KeyGesture keyGesture)
	{
		if (!_settingGesture)
		{
			_settingGesture = true;
			try
			{
				Key = keyGesture.Key;
				Modifiers = keyGesture.Modifiers;
			}
			finally
			{
				_settingGesture = false;
			}
		}
	}

	private void SynchronizeGestureFromProperties(Key key, ModifierKeys modifiers)
	{
		if (!_settingGesture)
		{
			_settingGesture = true;
			try
			{
				Gesture = new KeyGesture(key, modifiers, validateGesture: false);
			}
			finally
			{
				_settingGesture = false;
			}
		}
	}
}

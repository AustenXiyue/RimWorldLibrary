using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Defines a keyboard combination that can be used to invoke a command.</summary>
[TypeConverter(typeof(KeyGestureConverter))]
[ValueSerializer(typeof(KeyGestureValueSerializer))]
public class KeyGesture : InputGesture
{
	private ModifierKeys _modifiers;

	private Key _key;

	private string _displayString;

	private const char MULTIPLEGESTURE_DELIMITER = ';';

	private static TypeConverter _keyGestureConverter = new KeyGestureConverter();

	/// <summary>Gets the modifier keys associated with this <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>The modifier keys associated with the gesture. The default value is <see cref="F:System.Windows.Input.ModifierKeys.None" />.</returns>
	public ModifierKeys Modifiers => _modifiers;

	/// <summary>Gets the key associated with this <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>The key associated with the gesture.  The default value is <see cref="F:System.Windows.Input.Key.None" />.</returns>
	public Key Key => _key;

	/// <summary>Gets a string representation of this <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>The display string for this <see cref="T:System.Windows.Input.KeyGesture" />. The default value is <see cref="F:System.String.Empty" />.</returns>
	public string DisplayString => _displayString;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyGesture" /> class with the specified <see cref="T:System.Windows.Input.Key" />. </summary>
	/// <param name="key">The key associated with this gesture.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="key" /> is not a valid <see cref="T:System.Windows.Input.Key" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="key" /> is not a valid <see cref="T:System.Windows.Input.KeyGesture" />.</exception>
	public KeyGesture(Key key)
		: this(key, ModifierKeys.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyGesture" /> class with the specified <see cref="T:System.Windows.Input.Key" /> and <see cref="T:System.Windows.Input.ModifierKeys" />.</summary>
	/// <param name="key">The key associated with the gesture.</param>
	/// <param name="modifiers">The modifier keys associated with the gesture.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="modifiers" /> is not a valid <see cref="T:System.Windows.Input.ModifierKeys" />-or-<paramref name="key" /> is not a valid <see cref="T:System.Windows.Input.Key" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="key" /> and <paramref name="modifiers" /> do not form a valid <see cref="T:System.Windows.Input.KeyGesture" />.</exception>
	public KeyGesture(Key key, ModifierKeys modifiers)
		: this(key, modifiers, string.Empty, validateGesture: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyGesture" /> class with the specified <see cref="T:System.Windows.Input.Key" />, <see cref="T:System.Windows.Input.ModifierKeys" />, and display string.</summary>
	/// <param name="key">The key associated with the gesture.</param>
	/// <param name="modifiers">The modifier keys associated with the gesture.</param>
	/// <param name="displayString">A string representation of the <see cref="T:System.Windows.Input.KeyGesture" />.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="modifiers" /> is not a valid <see cref="T:System.Windows.Input.ModifierKeys" />-or-<paramref name="key" /> is not a valid <see cref="T:System.Windows.Input.Key" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="displayString" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="key" /> and <paramref name="modifiers" /> do not form a valid <see cref="T:System.Windows.Input.KeyGesture" />.</exception>
	public KeyGesture(Key key, ModifierKeys modifiers, string displayString)
		: this(key, modifiers, displayString, validateGesture: true)
	{
	}

	internal KeyGesture(Key key, ModifierKeys modifiers, bool validateGesture)
		: this(key, modifiers, string.Empty, validateGesture)
	{
	}

	private KeyGesture(Key key, ModifierKeys modifiers, string displayString, bool validateGesture)
	{
		if (!ModifierKeysConverter.IsDefinedModifierKeys(modifiers))
		{
			throw new InvalidEnumArgumentException("modifiers", (int)modifiers, typeof(ModifierKeys));
		}
		if (!IsDefinedKey(key))
		{
			throw new InvalidEnumArgumentException("key", (int)key, typeof(Key));
		}
		if (displayString == null)
		{
			throw new ArgumentNullException("displayString");
		}
		if (validateGesture && !IsValid(key, modifiers))
		{
			throw new NotSupportedException(SR.Format(SR.KeyGesture_Invalid, modifiers, key));
		}
		_modifiers = modifiers;
		_key = key;
		_displayString = displayString;
	}

	/// <summary>Returns a string that can be used to display the <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>The string to display </returns>
	/// <param name="culture">The culture specific information.</param>
	public string GetDisplayStringForCulture(CultureInfo culture)
	{
		if (!string.IsNullOrEmpty(_displayString))
		{
			return _displayString;
		}
		return (string)_keyGestureConverter.ConvertTo(null, culture, this, typeof(string));
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Input.KeyGesture" /> matches the input associated with the specified <see cref="T:System.Windows.Input.InputEventArgs" /> object.</summary>
	/// <returns>true if the event data matches this <see cref="T:System.Windows.Input.KeyGesture" />; otherwise, false.</returns>
	/// <param name="targetElement">The target.</param>
	/// <param name="inputEventArgs">The input event data to compare this gesture to.</param>
	public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
	{
		if (inputEventArgs is KeyEventArgs keyEventArgs && IsDefinedKey(keyEventArgs.Key))
		{
			if (Key == keyEventArgs.RealKey)
			{
				return Modifiers == Keyboard.Modifiers;
			}
			return false;
		}
		return false;
	}

	internal static bool IsDefinedKey(Key key)
	{
		if (key >= Key.None)
		{
			return key <= Key.OemClear;
		}
		return false;
	}

	internal static bool IsValid(Key key, ModifierKeys modifiers)
	{
		if ((key < Key.F1 || key > Key.F24) && (key < Key.NumPad0 || key > Key.Divide))
		{
			if ((modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows)) != 0)
			{
				if ((uint)(key - 70) <= 1u || (uint)(key - 118) <= 3u)
				{
					return false;
				}
				return true;
			}
			if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.A && key <= Key.Z))
			{
				return false;
			}
		}
		return true;
	}

	internal static void AddGesturesFromResourceStrings(string keyGestures, string displayStrings, InputGestureCollection gestures)
	{
		while (!string.IsNullOrEmpty(keyGestures))
		{
			int num = keyGestures.IndexOf(';');
			string keyGestureToken;
			if (num >= 0)
			{
				keyGestureToken = keyGestures.Substring(0, num);
				keyGestures = keyGestures.Substring(num + 1);
			}
			else
			{
				keyGestureToken = keyGestures;
				keyGestures = string.Empty;
			}
			num = displayStrings.IndexOf(';');
			string keyDisplayString;
			if (num >= 0)
			{
				keyDisplayString = displayStrings.Substring(0, num);
				displayStrings = displayStrings.Substring(num + 1);
			}
			else
			{
				keyDisplayString = displayStrings;
				displayStrings = string.Empty;
			}
			KeyGesture keyGesture = CreateFromResourceStrings(keyGestureToken, keyDisplayString);
			if (keyGesture != null)
			{
				gestures.Add(keyGesture);
			}
		}
	}

	internal static KeyGesture CreateFromResourceStrings(string keyGestureToken, string keyDisplayString)
	{
		if (!string.IsNullOrEmpty(keyDisplayString))
		{
			keyGestureToken = keyGestureToken + "," + keyDisplayString;
		}
		return _keyGestureConverter.ConvertFromInvariantString(keyGestureToken) as KeyGesture;
	}
}

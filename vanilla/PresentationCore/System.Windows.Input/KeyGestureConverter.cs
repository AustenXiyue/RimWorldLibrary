using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.KeyGesture" /> object to and from other types.</summary>
public class KeyGestureConverter : TypeConverter
{
	private const char MODIFIERS_DELIMITER = '+';

	internal const char DISPLAYSTRING_SEPARATOR = ',';

	private static KeyConverter keyConverter = new KeyConverter();

	private static ModifierKeysConverter modifierKeysConverter = new ModifierKeysConverter();

	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.KeyGesture" />, using the specified context. </summary>
	/// <returns>true if <paramref name="sourceType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.KeyGesture" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) && context != null && context.Instance != null && context.Instance is KeyGesture keyGesture)
		{
			if (ModifierKeysConverter.IsDefinedModifierKeys(keyGesture.Modifiers))
			{
				return IsDefinedKey(keyGesture.Key);
			}
			return false;
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.KeyGesture" />, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (source != null && source is string)
		{
			string text = ((string)source).Trim();
			if (text.Length == 0)
			{
				return new KeyGesture(Key.None);
			}
			int num = text.IndexOf(',');
			string displayString;
			if (num >= 0)
			{
				displayString = text.Substring(num + 1).Trim();
				text = text.Substring(0, num).Trim();
			}
			else
			{
				displayString = string.Empty;
			}
			num = text.LastIndexOf('+');
			string value;
			string value2;
			if (num >= 0)
			{
				value = text.Substring(0, num);
				value2 = text.Substring(num + 1);
			}
			else
			{
				value = string.Empty;
				value2 = text;
			}
			ModifierKeys modifiers = ModifierKeys.None;
			object obj = keyConverter.ConvertFrom(context, culture, value2);
			if (obj != null)
			{
				object obj2 = modifierKeysConverter.ConvertFrom(context, culture, value);
				if (obj2 != null)
				{
					modifiers = (ModifierKeys)obj2;
				}
				return new KeyGesture((Key)obj, modifiers, displayString);
			}
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.KeyGesture" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object, or an empty string if <paramref name="value" /> is null.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			if (value == null)
			{
				return string.Empty;
			}
			if (value is KeyGesture keyGesture)
			{
				if (keyGesture.Key == Key.None)
				{
					return string.Empty;
				}
				string text = "";
				string text2 = (string)keyConverter.ConvertTo(context, culture, keyGesture.Key, destinationType);
				if (text2 != string.Empty)
				{
					text += modifierKeysConverter.ConvertTo(context, culture, keyGesture.Modifiers, destinationType) as string;
					if (text != string.Empty)
					{
						text += "+";
					}
					text += text2;
					if (!string.IsNullOrEmpty(keyGesture.DisplayString))
					{
						text = text + "," + keyGesture.DisplayString;
					}
				}
				return text;
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static bool IsDefinedKey(Key key)
	{
		if (key >= Key.None)
		{
			return key <= Key.OemClear;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyGesture" /> class. </summary>
	public KeyGestureConverter()
	{
	}
}

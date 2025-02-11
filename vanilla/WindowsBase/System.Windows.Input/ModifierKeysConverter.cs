using System.ComponentModel;
using System.Globalization;
using MS.Internal.WindowsBase;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.ModifierKeys" /> object to and from other types.</summary>
public class ModifierKeysConverter : TypeConverter
{
	private const char Modifier_Delimiter = '+';

	private static ModifierKeys ModifierKeysFlag = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows;

	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.ModifierKeys" />, using the specified context.</summary>
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

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.ModifierKeys" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) && context != null && context.Instance != null && context.Instance is ModifierKeys)
		{
			return IsDefinedModifierKeys((ModifierKeys)context.Instance);
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.ModifierKeys" />, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (source is string)
		{
			string modifiersToken = ((string)source).Trim();
			return GetModifierKeys(modifiersToken, CultureInfo.InvariantCulture);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.ModifierKeys" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="value" /> does not map to a valid <see cref="T:System.Windows.Input.ModifierKeys" />.</exception>
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
			ModifierKeys modifierKeys = (ModifierKeys)value;
			if (!IsDefinedModifierKeys(modifierKeys))
			{
				throw new InvalidEnumArgumentException("value", (int)modifierKeys, typeof(ModifierKeys));
			}
			string text = "";
			if ((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
			{
				text += MatchModifiers(ModifierKeys.Control);
			}
			if ((modifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
			{
				if (text.Length > 0)
				{
					text += "+";
				}
				text += MatchModifiers(ModifierKeys.Alt);
			}
			if ((modifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
			{
				if (text.Length > 0)
				{
					text += "+";
				}
				text += MatchModifiers(ModifierKeys.Windows);
			}
			if ((modifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
			{
				if (text.Length > 0)
				{
					text += "+";
				}
				text += MatchModifiers(ModifierKeys.Shift);
			}
			return text;
		}
		throw GetConvertToException(value, destinationType);
	}

	private ModifierKeys GetModifierKeys(string modifiersToken, CultureInfo culture)
	{
		ModifierKeys modifierKeys = ModifierKeys.None;
		if (modifiersToken.Length != 0)
		{
			int num = 0;
			do
			{
				num = modifiersToken.IndexOf('+');
				string text = ((num < 0) ? modifiersToken : modifiersToken.Substring(0, num));
				text = text.Trim();
				text = text.ToUpper(culture);
				if (text == string.Empty)
				{
					break;
				}
				switch (text)
				{
				case "CONTROL":
				case "CTRL":
					modifierKeys |= ModifierKeys.Control;
					break;
				case "SHIFT":
					modifierKeys |= ModifierKeys.Shift;
					break;
				case "ALT":
					modifierKeys |= ModifierKeys.Alt;
					break;
				case "WINDOWS":
				case "WIN":
					modifierKeys |= ModifierKeys.Windows;
					break;
				default:
					throw new NotSupportedException(SR.Format(SR.Unsupported_Modifier, text));
				}
				modifiersToken = modifiersToken.Substring(num + 1);
			}
			while (num != -1);
		}
		return modifierKeys;
	}

	/// <summary>Determines whether the specified value is a valid <see cref="T:System.Windows.Input.ModifierKeys" /> value. </summary>
	/// <returns>true if input is a valid <see cref="T:System.Windows.Input.ModifierKeys" /> value; otherwise, false.</returns>
	/// <param name="modifierKeys">The value to check for validity.</param>
	public static bool IsDefinedModifierKeys(ModifierKeys modifierKeys)
	{
		if (modifierKeys != 0)
		{
			return (modifierKeys & ~ModifierKeysFlag) == 0;
		}
		return true;
	}

	internal static string MatchModifiers(ModifierKeys modifierKeys)
	{
		string result = string.Empty;
		switch (modifierKeys)
		{
		case ModifierKeys.Control:
			result = "Ctrl";
			break;
		case ModifierKeys.Shift:
			result = "Shift";
			break;
		case ModifierKeys.Alt:
			result = "Alt";
			break;
		case ModifierKeys.Windows:
			result = "Windows";
			break;
		}
		return result;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ModifierKeysConverter" /> class.</summary>
	public ModifierKeysConverter()
	{
	}
}

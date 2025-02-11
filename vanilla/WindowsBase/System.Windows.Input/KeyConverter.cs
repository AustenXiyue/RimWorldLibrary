using System.ComponentModel;
using System.Globalization;
using MS.Internal.WindowsBase;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.Key" /> object to and from other types.</summary>
public class KeyConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.Key" />, using the specified context. </summary>
	/// <returns>true if <paramref name="sourceType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
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

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.Key" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) && context != null && context.Instance != null)
		{
			Key key = (Key)context.Instance;
			if (key >= Key.None)
			{
				return key <= Key.DeadCharProcessed;
			}
			return false;
		}
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.Key" />, using the specified context.</summary>
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
			string text = ((string)source).Trim();
			object key = GetKey(text, CultureInfo.InvariantCulture);
			if (key != null)
			{
				return (Key)key;
			}
			throw new NotSupportedException(SR.Format(SR.Unsupported_Key, text));
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.Key" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted to <paramref name="destinationType" />.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value != null)
		{
			Key key = (Key)value;
			switch (key)
			{
			case Key.None:
				return string.Empty;
			case Key.D0:
			case Key.D1:
			case Key.D2:
			case Key.D3:
			case Key.D4:
			case Key.D5:
			case Key.D6:
			case Key.D7:
			case Key.D8:
			case Key.D9:
				return char.ToString((char)(key - 34 + 48));
			}
			if (key >= Key.A && key <= Key.Z)
			{
				return char.ToString((char)(key - 44 + 65));
			}
			string text = MatchKey(key, culture);
			if (text != null)
			{
				return text;
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	private object GetKey(string keyToken, CultureInfo culture)
	{
		if (keyToken.Length == 0)
		{
			return Key.None;
		}
		keyToken = keyToken.ToUpper(culture);
		if (keyToken.Length == 1 && char.IsLetterOrDigit(keyToken[0]))
		{
			if (char.IsDigit(keyToken[0]) && keyToken[0] >= '0' && keyToken[0] <= '9')
			{
				return 34 + keyToken[0] - 48;
			}
			if (char.IsLetter(keyToken[0]) && keyToken[0] >= 'A' && keyToken[0] <= 'Z')
			{
				return 44 + keyToken[0] - 65;
			}
			throw new ArgumentException(SR.Format(SR.CannotConvertStringToType, keyToken, typeof(Key)));
		}
		Key key = (Key)(-1);
		key = keyToken switch
		{
			"ENTER" => Key.Return, 
			"ESC" => Key.Escape, 
			"PGUP" => Key.Prior, 
			"PGDN" => Key.Next, 
			"PRTSC" => Key.Snapshot, 
			"INS" => Key.Insert, 
			"DEL" => Key.Delete, 
			"WINDOWS" => Key.LWin, 
			"WIN" => Key.LWin, 
			"LEFTWINDOWS" => Key.LWin, 
			"RIGHTWINDOWS" => Key.RWin, 
			"APPS" => Key.Apps, 
			"APPLICATION" => Key.Apps, 
			"BREAK" => Key.Cancel, 
			"BACKSPACE" => Key.Back, 
			"BKSP" => Key.Back, 
			"BS" => Key.Back, 
			"SHIFT" => Key.LeftShift, 
			"LEFTSHIFT" => Key.LeftShift, 
			"RIGHTSHIFT" => Key.RightShift, 
			"CONTROL" => Key.LeftCtrl, 
			"CTRL" => Key.LeftCtrl, 
			"LEFTCTRL" => Key.LeftCtrl, 
			"RIGHTCTRL" => Key.RightCtrl, 
			"ALT" => Key.LeftAlt, 
			"LEFTALT" => Key.LeftAlt, 
			"RIGHTALT" => Key.RightAlt, 
			"SEMICOLON" => Key.Oem1, 
			"PLUS" => Key.OemPlus, 
			"COMMA" => Key.OemComma, 
			"MINUS" => Key.OemMinus, 
			"PERIOD" => Key.OemPeriod, 
			"QUESTION" => Key.Oem2, 
			"TILDE" => Key.Oem3, 
			"OPENBRACKETS" => Key.Oem4, 
			"PIPE" => Key.Oem5, 
			"CLOSEBRACKETS" => Key.Oem6, 
			"QUOTES" => Key.Oem7, 
			"BACKSLASH" => Key.Oem102, 
			"FINISH" => Key.OemFinish, 
			"ATTN" => Key.Attn, 
			"CRSEL" => Key.CrSel, 
			"EXSEL" => Key.ExSel, 
			"ERASEEOF" => Key.EraseEof, 
			"PLAY" => Key.Play, 
			"ZOOM" => Key.Zoom, 
			"PA1" => Key.Pa1, 
			_ => (Key)Enum.Parse(typeof(Key), keyToken, ignoreCase: true), 
		};
		if (key != (Key)(-1))
		{
			return key;
		}
		return null;
	}

	private static string MatchKey(Key key, CultureInfo culture)
	{
		if (key == Key.None)
		{
			return string.Empty;
		}
		if (key != Key.Back)
		{
			if (key != Key.LineFeed)
			{
				if (key == Key.Escape)
				{
					return "Esc";
				}
				if (key >= Key.None && key <= Key.DeadCharProcessed)
				{
					return key.ToString();
				}
				return null;
			}
			return "Clear";
		}
		return "Backspace";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.Key" /> class. </summary>
	public KeyConverter()
	{
	}
}

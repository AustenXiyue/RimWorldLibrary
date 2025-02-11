using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.MouseGesture" /> object to and from other types.</summary>
public class MouseGestureConverter : TypeConverter
{
	private const char MODIFIERS_DELIMITER = '+';

	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.MouseGesture" />, using the specified context.</summary>
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

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.MouseGesture" />, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converter.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (source is string && source != null)
		{
			string text = ((string)source).Trim();
			if (text.Length == 0)
			{
				return new MouseGesture(MouseAction.None, ModifierKeys.None);
			}
			int num = text.LastIndexOf('+');
			string text2;
			string value;
			if (num >= 0)
			{
				text2 = text.Substring(0, num);
				value = text.Substring(num + 1);
			}
			else
			{
				text2 = string.Empty;
				value = text;
			}
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(MouseAction));
			if (converter != null)
			{
				object obj = converter.ConvertFrom(context, culture, value);
				if (obj != null)
				{
					if (!(text2 != string.Empty))
					{
						return new MouseGesture((MouseAction)obj);
					}
					TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(ModifierKeys));
					if (converter2 != null)
					{
						object obj2 = converter2.ConvertFrom(context, culture, text2);
						if (obj2 != null && obj2 is ModifierKeys)
						{
							return new MouseGesture((MouseAction)obj, (ModifierKeys)obj2);
						}
					}
				}
			}
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.MouseGesture" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) && context != null && context.Instance != null && context.Instance is MouseGesture mouseGesture)
		{
			if (ModifierKeysConverter.IsDefinedModifierKeys(mouseGesture.Modifiers))
			{
				return MouseActionConverter.IsDefinedMouseAction(mouseGesture.MouseAction);
			}
			return false;
		}
		return false;
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.MouseGesture" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">A format context that provides information about the environment from which this converter is being invoked.</param>
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
			if (value is MouseGesture mouseGesture)
			{
				string text = "";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(ModifierKeys));
				if (converter != null)
				{
					text += converter.ConvertTo(context, culture, mouseGesture.Modifiers, destinationType) as string;
					if (text != string.Empty)
					{
						text += "+";
					}
				}
				TypeConverter converter2 = TypeDescriptor.GetConverter(typeof(MouseAction));
				if (converter2 != null)
				{
					text += converter2.ConvertTo(context, culture, mouseGesture.MouseAction, destinationType) as string;
				}
				return text;
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseGestureConverter" /> class. </summary>
	public MouseGestureConverter()
	{
	}
}

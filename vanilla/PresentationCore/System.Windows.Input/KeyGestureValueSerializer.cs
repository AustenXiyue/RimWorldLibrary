using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
public class KeyGestureValueSerializer : ValueSerializer
{
	/// <summary>Determines if the specified <see cref="T:System.String" /> can be convert to an instance of <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines if the specified <see cref="T:System.Windows.Input.KeyGesture" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">The gesture to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (value is KeyGesture keyGesture && ModifierKeysConverter.IsDefinedModifierKeys(keyGesture.Modifiers))
		{
			return KeyGestureConverter.IsDefinedKey(keyGesture.Key);
		}
		return false;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Input.KeyGesture" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Input.KeyGesture" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">The string to convert into a <see cref="T:System.Windows.Input.KeyGesture" />.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(KeyGesture));
		if (converter != null)
		{
			return converter.ConvertFromString(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Input.KeyGesture" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>An invariant string representation of the specified <see cref="T:System.Windows.Input.KeyGesture" />.</returns>
	/// <param name="value">The gesture to convert into a string.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(KeyGesture));
		if (converter != null)
		{
			return converter.ConvertToInvariantString(value);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.KeyGestureValueSerializer" /> class.</summary>
	public KeyGestureValueSerializer()
	{
	}
}

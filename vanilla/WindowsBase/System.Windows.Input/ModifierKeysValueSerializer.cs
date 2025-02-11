using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Input.ModifierKeys" />.</summary>
public class ModifierKeysValueSerializer : ValueSerializer
{
	/// <summary>Determines if the specified <see cref="T:System.String" /> can be convert to an instance of <see cref="T:System.Windows.Input.ModifierKeys" />.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines if the specified <see cref="T:System.Windows.Input.ModifierKeys" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">The modifier keys to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (value is ModifierKeys)
		{
			return ModifierKeysConverter.IsDefinedModifierKeys((ModifierKeys)value);
		}
		return false;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Input.ModifierKeys" /> value.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Input.ModifierKeys" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">The string to convert into a <see cref="T:System.Windows.Input.ModifierKeys" />.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(ModifierKeys));
		if (converter != null)
		{
			return converter.ConvertFromString(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Input.ModifierKeys" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>An invariant string representation of the specified <see cref="T:System.Windows.Input.ModifierKeys" /> value.</returns>
	/// <param name="value">The key to convert into a string.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(ModifierKeys));
		if (converter != null)
		{
			return converter.ConvertToInvariantString(value);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ModifierKeysValueSerializer" /> class.</summary>
	public ModifierKeysValueSerializer()
	{
	}
}

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.InputScope" /> to and from other types.</summary>
public class InputScopeConverter : TypeConverter
{
	/// <summary>Determines whether an <see cref="T:System.Windows.Input.InputScope" /> object can be converted from an object of a specified type.</summary>
	/// <returns>true if <paramref name="sourceType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">An object that describes any type descriptor context.  This object must implement the <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> interface.  This parameter may be null.</param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> to check for conversion compatibility.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an <see cref="T:System.Windows.Input.InputScope" /> object can be converted to an object of a specified type.</summary>
	/// <returns>true if <paramref name="destinationType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">An object that describes any type descriptor context.  This object must implement the <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> interface.  This parameter may be null.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> to check for conversion compatibility.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return false;
	}

	/// <summary>Converts a source object (string) into an <see cref="T:System.Windows.Input.InputScope" /> object.</summary>
	/// <returns>An <see cref="T:System.Windows.Input.InputScope" /> object converted from the specified source object.</returns>
	/// <param name="context">An object that describes any type descriptor context.  This object must implement the <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> interface.  This parameter may be null.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object containing any cultural context for the conversion.  This parameter may be null.</param>
	/// <param name="source">A source object to convert from.  This object must be a string.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		string text = source as string;
		InputScopeNameValue nameValue = InputScopeNameValue.Default;
		if (text != null)
		{
			ReadOnlySpan<char> readOnlySpan = text;
			readOnlySpan = readOnlySpan.Trim();
			int num = readOnlySpan.LastIndexOf('.');
			if (num != -1)
			{
				readOnlySpan = readOnlySpan.Slice(num + 1);
			}
			if (!readOnlySpan.IsEmpty)
			{
				nameValue = Enum.Parse<InputScopeNameValue>(readOnlySpan);
			}
		}
		return new InputScope
		{
			Names = { (object?)new InputScopeName(nameValue) }
		};
	}

	/// <summary>Converts an <see cref="T:System.Windows.Input.InputScope" /> object into a specified object type (string).</summary>
	/// <returns>A new object of the specified type (string) converted from the given <see cref="T:System.Windows.Input.InputScope" /> object.</returns>
	/// <param name="context">An object that describes any type descriptor context.  This object must implement the <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> interface.  This parameter may be null.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object containing any cultural context for the conversion.  This parameter may be null.</param>
	/// <param name="value">An object to convert from.  This object must be of type <see cref="T:System.Windows.Input.InputScope" />.</param>
	/// <param name="destinationType">A destination type to convert type.  This type must be string.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		InputScope inputScope = value as InputScope;
		if (null != destinationType && inputScope != null && destinationType == typeof(string))
		{
			return Enum.GetName(typeof(InputScopeNameValue), ((InputScopeName)inputScope.Names[0]).NameValue);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScopeConverter" /> class.</summary>
	public InputScopeConverter()
	{
	}
}

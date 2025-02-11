using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Converts instances of <see cref="T:System.Windows.Input.InputScopeName" /> to and from other data types.</summary>
public class InputScopeNameConverter : TypeConverter
{
	/// <summary>Indicates whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Input.InputScopeName" />.</summary>
	/// <returns>true if <paramref name="sourceType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="sourceType">The source <see cref="T:System.Type" /> that is being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether instances of <see cref="T:System.Windows.Input.InputScopeName" /> can be converted to the specified type.</summary>
	/// <returns>true if <paramref name="destinationType" /> is type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Input.InputScopeName" /> is being evaluated to be converted to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (typeof(string) == destinationType && context != null && context.Instance != null && context.Instance is InputScopeName)
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Input.InputScopeName" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.InputScopeName" /> created from converting <paramref name="source" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="source">The object being converted.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		string text = source as string;
		InputScopeNameValue nameValue = InputScopeNameValue.Default;
		if (text != null)
		{
			text = text.Trim();
			if (-1 != text.LastIndexOf('.'))
			{
				text = text.Substring(text.LastIndexOf('.') + 1);
			}
			if (!text.Equals(string.Empty))
			{
				nameValue = (InputScopeNameValue)Enum.Parse(typeof(InputScopeNameValue), text);
			}
		}
		return new InputScopeName
		{
			NameValue = nameValue
		};
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Input.InputScopeName" /> to the specified type.</summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Input.InputScopeName" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Input.InputScopeName" /> to convert.</param>
	/// <param name="destinationType">The type to convert the <see cref="T:System.Windows.Input.InputScopeName" /> to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		InputScopeName inputScopeName = value as InputScopeName;
		if (null != destinationType && inputScopeName != null && destinationType == typeof(string))
		{
			return Enum.GetName(typeof(InputScopeNameValue), inputScopeName.NameValue);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScopeNameConverter" /> class.</summary>
	public InputScopeNameConverter()
	{
	}
}

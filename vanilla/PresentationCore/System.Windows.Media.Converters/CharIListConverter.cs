using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Converters;

/// <summary>Converts the members of an <see cref="T:System.Collections.IList" /> collection of Unicode characters to and from instances of <see cref="T:System.String" />.</summary>
public sealed class CharIListConverter : BaseIListConverter
{
	internal override object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value)
	{
		return value.ToCharArray();
	}

	internal override object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		IList<char> obj = (value as IList<char>) ?? throw GetConvertToException(value, destinationType);
		char[] array = new char[obj.Count];
		obj.CopyTo(array, 0);
		return new string(array);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.CharIListConverter" /> class.</summary>
	public CharIListConverter()
	{
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using MS.Internal;

namespace System.Windows.Media.Converters;

/// <summary>Converts the members of an <see cref="T:System.Collections.IList" /> collection of Boolean values to and from instances of <see cref="T:System.String" />.</summary>
public sealed class BoolIListConverter : BaseIListConverter
{
	private const int EstimatedCharCountPerItem = 2;

	internal override object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value)
	{
		_tokenizer = new TokenizerHelper(value, '\0', ' ');
		List<bool> list = new List<bool>(Math.Min(256, value.Length / 2 + 1));
		while (_tokenizer.NextToken())
		{
			list.Add(Convert.ToInt32(_tokenizer.GetCurrentToken(), ci) != 0);
		}
		return list;
	}

	internal override object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is IList<bool> list))
		{
			throw GetConvertToException(value, destinationType);
		}
		StringBuilder stringBuilder = new StringBuilder(2 * list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(list[i] ? 1 : 0);
		}
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.BoolIListConverter" /> class.</summary>
	public BoolIListConverter()
	{
	}
}

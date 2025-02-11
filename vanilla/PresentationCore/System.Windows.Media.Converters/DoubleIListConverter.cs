using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using MS.Internal;

namespace System.Windows.Media.Converters;

/// <summary>Converts members of an <see cref="T:System.Collections.IList" /> collection containing <see cref="T:System.Double" /> numbers to and from instances of <see cref="T:System.String" />.</summary>
public sealed class DoubleIListConverter : BaseIListConverter
{
	private const int EstimatedCharCountPerItem = 6;

	internal sealed override object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value)
	{
		_tokenizer = new TokenizerHelper(value, '\0', ' ');
		List<double> list = new List<double>(Math.Min(256, value.Length / 6 + 1));
		while (_tokenizer.NextToken())
		{
			list.Add(Convert.ToDouble(_tokenizer.GetCurrentToken(), ci));
		}
		return list;
	}

	internal override object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is IList<double> list))
		{
			throw GetConvertToException(value, destinationType);
		}
		StringBuilder stringBuilder = new StringBuilder(6 * list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(list[i].ToString(culture));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.DoubleIListConverter" /> class.</summary>
	public DoubleIListConverter()
	{
	}
}

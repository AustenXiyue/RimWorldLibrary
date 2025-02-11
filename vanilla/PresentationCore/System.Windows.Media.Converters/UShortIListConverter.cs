using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using MS.Internal;

namespace System.Windows.Media.Converters;

/// <summary>Converts an <see cref="T:System.Collections.IList" /> collection of UShort number values to and from instances of <see cref="T:System.String" />.</summary>
public sealed class UShortIListConverter : BaseIListConverter
{
	private const int EstimatedCharCountPerItem = 3;

	internal override object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value)
	{
		_tokenizer = new TokenizerHelper(value, '\0', ' ');
		List<ushort> list = new List<ushort>(Math.Min(256, value.Length / 3 + 1));
		while (_tokenizer.NextToken())
		{
			list.Add(Convert.ToUInt16(_tokenizer.GetCurrentToken(), ci));
		}
		return list;
	}

	internal override object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is IList<ushort> list))
		{
			throw GetConvertToException(value, destinationType);
		}
		StringBuilder stringBuilder = new StringBuilder(3 * list.Count);
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.UShortIListConverter" /> class.</summary>
	public UShortIListConverter()
	{
	}
}

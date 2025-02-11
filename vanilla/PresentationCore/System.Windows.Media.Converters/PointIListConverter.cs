using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using MS.Internal;

namespace System.Windows.Media.Converters;

/// <summary>Converts an <see cref="T:System.Collections.IList" /> collection of <see cref="T:System.Windows.Point" /> values to and from instances of <see cref="T:System.String" />.</summary>
public sealed class PointIListConverter : BaseIListConverter
{
	private PointConverter converter = new PointConverter();

	private const int EstimatedCharCountPerItem = 12;

	internal override object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value)
	{
		_tokenizer = new TokenizerHelper(value, '\0', ' ');
		List<Point> list = new List<Point>(Math.Min(256, value.Length / 12 + 1));
		while (_tokenizer.NextToken())
		{
			list.Add((Point)converter.ConvertFrom(td, ci, _tokenizer.GetCurrentToken()));
		}
		return list;
	}

	internal override object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is IList<Point> list))
		{
			throw GetConvertToException(value, destinationType);
		}
		StringBuilder stringBuilder = new StringBuilder(12 * list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append((string)converter.ConvertTo(context, culture, list[i], typeof(string)));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.PointIListConverter" /> class.</summary>
	public PointIListConverter()
	{
	}
}

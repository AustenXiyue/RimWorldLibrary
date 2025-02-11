using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal sealed class MarkerProperties
{
	private TextMarkerStyle _style;

	private double _offset;

	private int _index;

	internal MarkerProperties(List list, int index)
	{
		_offset = list.MarkerOffset;
		if (double.IsNaN(_offset))
		{
			double lineHeightValue = DynamicPropertyReader.GetLineHeightValue(list);
			_offset = -0.5 * lineHeightValue;
		}
		else
		{
			_offset = 0.0 - _offset;
		}
		_style = list.MarkerStyle;
		_index = index;
	}

	internal TextMarkerProperties GetTextMarkerProperties(TextParagraphProperties textParaProps)
	{
		return new TextSimpleMarkerProperties(_style, _offset, _index, textParaProps);
	}
}

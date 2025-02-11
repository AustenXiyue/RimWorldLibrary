using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal interface IShapeableTextCollector
{
	void Add(IList<TextShapeableSymbols> shapeableList, CharacterBufferRange characterBufferRange, TextRunProperties textRunProperties, ItemProps textItem, ShapeTypeface shapeTypeface, double emScale, bool nullShape, TextFormattingMode textFormattingMode);
}

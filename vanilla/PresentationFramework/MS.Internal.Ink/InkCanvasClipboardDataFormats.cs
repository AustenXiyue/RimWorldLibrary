using System;

namespace MS.Internal.Ink;

[Flags]
internal enum InkCanvasClipboardDataFormats
{
	None = 0,
	XAML = 1,
	ISF = 2
}

using System;

namespace MS.Internal.Controls.StickyNote;

[Flags]
internal enum XmlToken
{
	MetaData = 1,
	Left = 4,
	Top = 8,
	XOffset = 0x10,
	YOffset = 0x20,
	Width = 0x80,
	Height = 0x100,
	IsExpanded = 0x200,
	Author = 0x400,
	Text = 0x2000,
	Ink = 0x8000,
	ZOrder = 0x20000
}

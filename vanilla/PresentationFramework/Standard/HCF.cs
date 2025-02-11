using System;

namespace Standard;

[Flags]
internal enum HCF
{
	HIGHCONTRASTON = 1,
	AVAILABLE = 2,
	HOTKEYACTIVE = 4,
	CONFIRMHOTKEY = 8,
	HOTKEYSOUND = 0x10,
	INDICATOR = 0x20,
	HOTKEYAVAILABLE = 0x40
}

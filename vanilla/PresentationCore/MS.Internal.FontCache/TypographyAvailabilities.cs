using System;

namespace MS.Internal.FontCache;

[Flags]
internal enum TypographyAvailabilities
{
	None = 0,
	Available = 1,
	IdeoTypographyAvailable = 2,
	FastTextTypographyAvailable = 4,
	FastTextMajorLanguageLocalizedFormAvailable = 8,
	FastTextExtraLanguageLocalizedFormAvailable = 0x10
}

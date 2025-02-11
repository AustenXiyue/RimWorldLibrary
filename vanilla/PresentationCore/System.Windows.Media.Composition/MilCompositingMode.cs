namespace System.Windows.Media.Composition;

internal enum MilCompositingMode
{
	SourceOver = 0,
	SourceCopy = 1,
	SourceAdd = 2,
	SourceAlphaMultiply = 3,
	SourceInverseAlphaMultiply = 4,
	SourceUnder = 5,
	SourceOverNonPremultiplied = 6,
	SourceInverseAlphaOverNonPremultiplied = 7,
	DestInvert = 8,
	Last = 9,
	FORCE_DWORD = -1
}

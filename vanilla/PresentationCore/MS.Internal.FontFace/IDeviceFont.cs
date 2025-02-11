namespace MS.Internal.FontFace;

internal interface IDeviceFont
{
	string Name { get; }

	bool ContainsCharacter(int unicodeScalar);

	unsafe void GetAdvanceWidths(char* characterString, int characterLength, double emSize, int* pAdvances);
}

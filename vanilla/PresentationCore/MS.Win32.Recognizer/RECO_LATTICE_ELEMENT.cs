namespace MS.Win32.Recognizer;

internal struct RECO_LATTICE_ELEMENT
{
	public int score;

	public ushort type;

	public nint pData;

	public uint ulNextColumn;

	public uint ulStrokeNumber;

	public RECO_LATTICE_PROPERTIES epProp;
}

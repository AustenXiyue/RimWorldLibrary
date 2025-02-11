namespace MS.Win32.Recognizer;

internal struct RECO_LATTICE
{
	public uint ulColumnCount;

	public nint pLatticeColumns;

	public uint ulPropertyCount;

	public nint pGuidProperties;

	public uint ulBestResultColumnCount;

	public nint pulBestResultColumns;

	public nint pulBestResultIndexes;
}

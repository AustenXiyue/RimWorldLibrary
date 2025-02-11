namespace MS.Win32.Recognizer;

internal struct RECO_LATTICE_COLUMN
{
	public uint key;

	public RECO_LATTICE_PROPERTIES cpProp;

	public uint cStrokes;

	public nint pStrokes;

	public uint cLatticeElements;

	public nint pLatticeElements;
}

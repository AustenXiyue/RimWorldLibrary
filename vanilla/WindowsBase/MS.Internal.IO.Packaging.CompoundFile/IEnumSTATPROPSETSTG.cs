namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IEnumSTATPROPSETSTG
{
	int Next(uint celt, STATPROPSETSTG rgelt, out uint pceltFetched);

	void Skip(uint celt);

	void Reset();

	void Clone(out IEnumSTATPROPSETSTG ppenum);
}

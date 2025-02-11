namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IEnumSTATPROPSTG
{
	int Next(uint celt, STATPROPSTG rgelt, out uint pceltFetched);

	void Skip(uint celt);

	void Reset();

	void Clone(out IEnumSTATPROPSTG ppenum);
}

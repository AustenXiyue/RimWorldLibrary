using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IEnumSTATSTG
{
	void Next(uint celt, out STATSTG rgelt, out uint pceltFetched);

	void Skip(uint celt);

	void Reset();

	void Clone(out IEnumSTATSTG ppenum);
}

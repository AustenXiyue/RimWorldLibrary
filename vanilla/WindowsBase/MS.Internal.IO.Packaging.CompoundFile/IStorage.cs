using System;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IStorage
{
	int CreateStream(string pwcsName, int grfMode, int reserved1, int reserved2, out IStream ppstm);

	int OpenStream(string pwcsName, int reserved1, int grfMode, int reserved2, out IStream ppstm);

	int CreateStorage(string pwcsName, int grfMode, int reserved1, int reserved2, out IStorage ppstg);

	int OpenStorage(string pwcsName, IStorage pstgPriority, int grfMode, nint snbExclude, int reserved, out IStorage ppstg);

	void CopyTo(int ciidExclude, Guid[] rgiidExclude, nint snbExclude, IStorage ppstg);

	void MoveElementTo(string pwcsName, IStorage pstgDest, string pwcsNewName, int grfFlags);

	void Commit(int grfCommitFlags);

	void Revert();

	void EnumElements(int reserved1, nint reserved2, int reserved3, out IEnumSTATSTG ppEnum);

	void DestroyElement(string pwcsName);

	void RenameElement(string pwcsOldName, string pwcsNewName);

	void SetElementTimes(string pwcsName, FILETIME pctime, FILETIME patime, FILETIME pmtime);

	void SetClass(ref Guid clsid);

	void SetStateBits(int grfStateBits, int grfMask);

	void Stat(out STATSTG pstatstg, int grfStatFlag);
}

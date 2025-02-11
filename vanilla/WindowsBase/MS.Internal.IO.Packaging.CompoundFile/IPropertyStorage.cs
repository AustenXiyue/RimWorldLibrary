using System;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal interface IPropertyStorage
{
	int ReadMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar);

	void WriteMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar, uint propidNameFirst);

	void DeleteMultiple(uint cpspec, PROPSPEC[] rgpspec);

	void ReadPropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName);

	void WritePropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName);

	void DeletePropertyNames(uint cpropid, uint[] rgpropid);

	void Commit(uint grfCommitFlags);

	void Revert();

	void Enum(out IEnumSTATPROPSTG ppenum);

	void SetTimes(ref FILETIME pctime, ref FILETIME patime, ref FILETIME pmtime);

	void SetClass(ref Guid clsid);

	void Stat(out STATPROPSETSTG pstatpsstg);
}

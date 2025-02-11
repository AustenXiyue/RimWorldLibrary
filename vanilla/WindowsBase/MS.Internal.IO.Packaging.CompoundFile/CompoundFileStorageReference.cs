using System;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class CompoundFileStorageReference : CompoundFileReference, IComparable
{
	private string _fullName;

	public override string FullName => _fullName;

	public CompoundFileStorageReference(string fullName)
	{
		SetFullName(fullName);
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (o.GetType() != GetType())
		{
			return false;
		}
		CompoundFileStorageReference compoundFileStorageReference = (CompoundFileStorageReference)o;
		return string.CompareOrdinal(_fullName.ToUpperInvariant(), compoundFileStorageReference._fullName.ToUpperInvariant()) == 0;
	}

	public override int GetHashCode()
	{
		return _fullName.GetHashCode();
	}

	int IComparable.CompareTo(object o)
	{
		if (o == null)
		{
			return 1;
		}
		if (o.GetType() != GetType())
		{
			throw new ArgumentException(SR.CanNotCompareDiffTypes);
		}
		CompoundFileStorageReference compoundFileStorageReference = (CompoundFileStorageReference)o;
		return string.CompareOrdinal(_fullName.ToUpperInvariant(), compoundFileStorageReference._fullName.ToUpperInvariant());
	}

	private void SetFullName(string fullName)
	{
		if (fullName == null || fullName.Length == 0)
		{
			_fullName = string.Empty;
			return;
		}
		if (fullName.StartsWith(ContainerUtilities.PathSeparatorAsString, StringComparison.Ordinal))
		{
			throw new ArgumentException(SR.DelimiterLeading, "fullName");
		}
		_fullName = fullName;
		if (ContainerUtilities.ConvertBackSlashPathToStringArrayPath(_fullName).Length != 0)
		{
			return;
		}
		throw new ArgumentException(SR.CompoundFilePathNullEmpty, "fullName");
	}
}

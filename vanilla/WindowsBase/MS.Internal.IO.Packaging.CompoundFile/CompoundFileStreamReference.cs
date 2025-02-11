using System;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class CompoundFileStreamReference : CompoundFileReference, IComparable
{
	private string _fullName;

	public override string FullName => _fullName;

	public CompoundFileStreamReference(string fullName)
	{
		SetFullName(fullName);
	}

	public CompoundFileStreamReference(string storageName, string streamName)
	{
		ContainerUtilities.CheckStringAgainstNullAndEmpty(streamName, "streamName");
		if (storageName == null || storageName.Length == 0)
		{
			_fullName = streamName;
			return;
		}
		_fullName = $"{storageName}{ContainerUtilities.PathSeparator}{streamName}";
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
		CompoundFileStreamReference compoundFileStreamReference = (CompoundFileStreamReference)o;
		return string.CompareOrdinal(_fullName.ToUpperInvariant(), compoundFileStreamReference._fullName.ToUpperInvariant()) == 0;
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
		CompoundFileStreamReference compoundFileStreamReference = (CompoundFileStreamReference)o;
		return string.CompareOrdinal(_fullName.ToUpperInvariant(), compoundFileStreamReference._fullName.ToUpperInvariant());
	}

	private void SetFullName(string fullName)
	{
		ContainerUtilities.CheckStringAgainstNullAndEmpty(fullName, "fullName");
		if (fullName.StartsWith(ContainerUtilities.PathSeparatorAsString, StringComparison.Ordinal))
		{
			throw new ArgumentException(SR.DelimiterLeading, "fullName");
		}
		_fullName = fullName;
		if (ContainerUtilities.ConvertBackSlashPathToStringArrayPath(fullName).Length == 0)
		{
			throw new ArgumentException(SR.CompoundFilePathNullEmpty, "fullName");
		}
	}
}

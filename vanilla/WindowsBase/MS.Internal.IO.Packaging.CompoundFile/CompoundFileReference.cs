using System;
using System.Collections.Specialized;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal abstract class CompoundFileReference : IComparable
{
	private enum RefComponentType
	{
		Stream,
		Storage
	}

	public abstract string FullName { get; }

	int IComparable.CompareTo(object ob)
	{
		return 0;
	}

	public override bool Equals(object o)
	{
		return false;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	internal static int Save(CompoundFileReference reference, BinaryWriter writer)
	{
		int num = 0;
		bool flag = writer == null;
		CompoundFileStreamReference compoundFileStreamReference = reference as CompoundFileStreamReference;
		if (compoundFileStreamReference == null && !(reference is CompoundFileStorageReference))
		{
			throw new ArgumentException(SR.UnknownReferenceSerialize, "reference");
		}
		string[] array = ContainerUtilities.ConvertBackSlashPathToStringArrayPath(reference.FullName);
		int value = array.Length;
		if (!flag)
		{
			writer.Write(value);
		}
		num += ContainerUtilities.Int32Size;
		for (int i = 0; i < array.Length - ((compoundFileStreamReference != null) ? 1 : 0); i++)
		{
			if (!flag)
			{
				writer.Write(1);
			}
			num += ContainerUtilities.Int32Size;
			num += ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(writer, array[i]);
		}
		if (compoundFileStreamReference != null)
		{
			if (!flag)
			{
				writer.Write(0);
			}
			num += ContainerUtilities.Int32Size;
			num += ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(writer, array[^1]);
		}
		return num;
	}

	internal static CompoundFileReference Load(BinaryReader reader, out int bytesRead)
	{
		ContainerUtilities.CheckAgainstNull(reader, "reader");
		bytesRead = 0;
		int num = reader.ReadInt32();
		bytesRead += ContainerUtilities.Int32Size;
		if (num < 0)
		{
			throw new FileFormatException(SR.CFRCorrupt);
		}
		StringCollection stringCollection = null;
		string text = null;
		while (num > 0)
		{
			RefComponentType refComponentType = (RefComponentType)reader.ReadInt32();
			bytesRead += ContainerUtilities.Int32Size;
			int bytesRead2;
			switch (refComponentType)
			{
			case RefComponentType.Storage:
			{
				if (text != null)
				{
					throw new FileFormatException(SR.CFRCorruptStgFollowStm);
				}
				if (stringCollection == null)
				{
					stringCollection = new StringCollection();
				}
				string value = ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(reader, out bytesRead2);
				bytesRead += bytesRead2;
				stringCollection.Add(value);
				break;
			}
			case RefComponentType.Stream:
				if (text != null)
				{
					throw new FileFormatException(SR.CFRCorruptMultiStream);
				}
				text = ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(reader, out bytesRead2);
				bytesRead += bytesRead2;
				break;
			default:
				throw new FileFormatException(SR.UnknownReferenceComponentType);
			}
			num--;
		}
		CompoundFileReference compoundFileReference = null;
		if (text == null)
		{
			return new CompoundFileStorageReference(ContainerUtilities.ConvertStringArrayPathToBackSlashPath(stringCollection));
		}
		return new CompoundFileStreamReference(ContainerUtilities.ConvertStringArrayPathToBackSlashPath(stringCollection, text));
	}
}

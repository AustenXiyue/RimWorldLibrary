using System;
using System.IO;
using System.Text;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class FormatVersion
{
	private VersionPair _reader;

	private VersionPair _updater;

	private VersionPair _writer;

	private string _featureIdentifier;

	public VersionPair ReaderVersion
	{
		get
		{
			return _reader;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_reader = value;
		}
	}

	public VersionPair WriterVersion
	{
		get
		{
			return _writer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_writer = value;
		}
	}

	public VersionPair UpdaterVersion
	{
		get
		{
			return _updater;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_updater = value;
		}
	}

	public string FeatureIdentifier => _featureIdentifier;

	private FormatVersion()
	{
	}

	public FormatVersion(string featureId, VersionPair version)
		: this(featureId, version, version, version)
	{
	}

	public FormatVersion(string featureId, VersionPair writerVersion, VersionPair readerVersion, VersionPair updaterVersion)
	{
		if (featureId == null)
		{
			throw new ArgumentNullException("featureId");
		}
		if (writerVersion == null)
		{
			throw new ArgumentNullException("writerVersion");
		}
		if (readerVersion == null)
		{
			throw new ArgumentNullException("readerVersion");
		}
		if (updaterVersion == null)
		{
			throw new ArgumentNullException("updaterVersion");
		}
		if (featureId.Length == 0)
		{
			throw new ArgumentException(SR.ZeroLengthFeatureID);
		}
		_featureIdentifier = featureId;
		_reader = readerVersion;
		_updater = updaterVersion;
		_writer = writerVersion;
	}

	public static FormatVersion LoadFromStream(Stream stream)
	{
		int bytesRead;
		return LoadFromStream(stream, out bytesRead);
	}

	public int SaveToStream(Stream stream)
	{
		BinaryWriter binaryWriter = null;
		if (stream != null)
		{
			binaryWriter = new BinaryWriter(stream, Encoding.Unicode);
		}
		checked
		{
			int num = 0 + ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(binaryWriter, _featureIdentifier);
			if (stream != null)
			{
				binaryWriter.Write(_reader.Major);
				binaryWriter.Write(_reader.Minor);
			}
			int num2 = num + ContainerUtilities.Int16Size + ContainerUtilities.Int16Size;
			if (stream != null)
			{
				binaryWriter.Write(_updater.Major);
				binaryWriter.Write(_updater.Minor);
			}
			int num3 = num2 + ContainerUtilities.Int16Size + ContainerUtilities.Int16Size;
			if (stream != null)
			{
				binaryWriter.Write(_writer.Major);
				binaryWriter.Write(_writer.Minor);
			}
			return num3 + ContainerUtilities.Int16Size + ContainerUtilities.Int16Size;
		}
	}

	public bool IsReadableBy(VersionPair version)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		return _reader <= version;
	}

	public bool IsUpdatableBy(VersionPair version)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		return _updater <= version;
	}

	private static FormatVersion LoadFromBinaryReader(BinaryReader reader, out int bytesRead)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		FormatVersion formatVersion = new FormatVersion();
		bytesRead = 0;
		formatVersion._featureIdentifier = ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(reader, out var bytesRead2);
		checked
		{
			bytesRead += bytesRead2;
			short major = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			short minor = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			formatVersion.ReaderVersion = new VersionPair(major, minor);
			major = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			minor = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			formatVersion.UpdaterVersion = new VersionPair(major, minor);
			major = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			minor = reader.ReadInt16();
			bytesRead += ContainerUtilities.Int16Size;
			formatVersion.WriterVersion = new VersionPair(major, minor);
			return formatVersion;
		}
	}

	internal static FormatVersion LoadFromStream(Stream stream, out int bytesRead)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return LoadFromBinaryReader(new BinaryReader(stream, Encoding.Unicode), out bytesRead);
	}
}

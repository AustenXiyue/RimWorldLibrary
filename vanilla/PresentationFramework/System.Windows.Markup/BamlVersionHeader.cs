using System.Globalization;
using System.IO;
using MS.Internal.IO.Packaging.CompoundFile;

namespace System.Windows.Markup;

internal class BamlVersionHeader
{
	internal static readonly VersionPair BamlWriterVersion;

	private FormatVersion _bamlVersion;

	public FormatVersion BamlVersion
	{
		get
		{
			return _bamlVersion;
		}
		set
		{
			_bamlVersion = value;
		}
	}

	public static int BinarySerializationSize => 28;

	static BamlVersionHeader()
	{
		BamlWriterVersion = new VersionPair(0, 96);
	}

	public BamlVersionHeader()
	{
		_bamlVersion = new FormatVersion("MSBAML", BamlWriterVersion);
	}

	internal void LoadVersion(BinaryReader bamlBinaryReader)
	{
		BamlVersion = FormatVersion.LoadFromStream(bamlBinaryReader.BaseStream);
		if (BamlVersion.ReaderVersion != BamlWriterVersion)
		{
			throw new InvalidOperationException(SR.Format(SR.ParserBamlVersion, BamlVersion.ReaderVersion.Major.ToString(CultureInfo.CurrentCulture) + "." + BamlVersion.ReaderVersion.Minor.ToString(CultureInfo.CurrentCulture), BamlWriterVersion.Major.ToString(CultureInfo.CurrentCulture) + "." + BamlWriterVersion.Minor.ToString(CultureInfo.CurrentCulture)));
		}
	}

	internal void WriteVersion(BinaryWriter bamlBinaryWriter)
	{
		BamlVersion.SaveToStream(bamlBinaryWriter.BaseStream);
	}
}

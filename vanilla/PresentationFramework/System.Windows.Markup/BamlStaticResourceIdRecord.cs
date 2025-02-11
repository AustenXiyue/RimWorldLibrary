using System.Globalization;
using System.IO;

namespace System.Windows.Markup;

internal class BamlStaticResourceIdRecord : BamlRecord
{
	private short _staticResourceId = -1;

	internal override BamlRecordType RecordType => BamlRecordType.StaticResourceId;

	internal override int RecordSize
	{
		get
		{
			return 2;
		}
		set
		{
		}
	}

	internal short StaticResourceId
	{
		get
		{
			return _staticResourceId;
		}
		set
		{
			_staticResourceId = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		StaticResourceId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(StaticResourceId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlStaticResourceIdRecord)record)._staticResourceId = _staticResourceId;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} staticResourceId({1})", RecordType, StaticResourceId);
	}
}

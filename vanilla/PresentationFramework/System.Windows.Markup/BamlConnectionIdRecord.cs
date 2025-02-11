using System.IO;

namespace System.Windows.Markup;

internal class BamlConnectionIdRecord : BamlRecord
{
	private int _connectionId = -1;

	internal override BamlRecordType RecordType => BamlRecordType.ConnectionId;

	internal int ConnectionId
	{
		get
		{
			return _connectionId;
		}
		set
		{
			_connectionId = value;
		}
	}

	internal override int RecordSize
	{
		get
		{
			return 4;
		}
		set
		{
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		ConnectionId = bamlBinaryReader.ReadInt32();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(ConnectionId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		((BamlConnectionIdRecord)record)._connectionId = _connectionId;
	}
}

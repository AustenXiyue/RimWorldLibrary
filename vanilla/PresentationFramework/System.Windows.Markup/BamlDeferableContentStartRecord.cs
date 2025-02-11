using System.IO;

namespace System.Windows.Markup;

internal class BamlDeferableContentStartRecord : BamlRecord
{
	private const long ContentSizeSize = 4L;

	private int _contentSize = -1;

	private long _contentSizePosition = -1L;

	private byte[] _valuesBuffer;

	internal override BamlRecordType RecordType => BamlRecordType.DeferableContentStart;

	internal int ContentSize
	{
		get
		{
			return _contentSize;
		}
		set
		{
			_contentSize = value;
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

	internal byte[] ValuesBuffer
	{
		get
		{
			return _valuesBuffer;
		}
		set
		{
			_valuesBuffer = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		ContentSize = bamlBinaryReader.ReadInt32();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		_contentSizePosition = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		bamlBinaryWriter.Write(ContentSize);
	}

	internal void UpdateContentSize(int contentSize, BinaryWriter bamlBinaryWriter)
	{
		long num = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		int num2 = (int)(_contentSizePosition - num);
		bamlBinaryWriter.Seek(num2, SeekOrigin.Current);
		bamlBinaryWriter.Write(contentSize);
		bamlBinaryWriter.Seek((int)(-4L - (long)num2), SeekOrigin.Current);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlDeferableContentStartRecord obj = (BamlDeferableContentStartRecord)record;
		obj._contentSize = _contentSize;
		obj._contentSizePosition = _contentSizePosition;
		obj._valuesBuffer = _valuesBuffer;
	}
}

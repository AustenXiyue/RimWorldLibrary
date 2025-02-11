using System.Collections.Specialized;
using System.IO;

namespace System.Windows.Markup;

internal class BamlDefAttributeKeyTypeRecord : BamlElementStartRecord, IBamlDictionaryKey
{
	private static BitVector32.Section _sharedSection = BitVector32.CreateSection(1, BamlElementStartRecord.LastFlagsSection);

	private static BitVector32.Section _sharedSetSection = BitVector32.CreateSection(1, _sharedSection);

	internal const int ValuePositionSize = 4;

	private int _valuePosition;

	private long _valuePositionPosition = -1L;

	private object _keyObject;

	private object[] _staticResourceValues;

	internal override BamlRecordType RecordType => BamlRecordType.DefAttributeKeyType;

	int IBamlDictionaryKey.ValuePosition
	{
		get
		{
			return _valuePosition;
		}
		set
		{
			_valuePosition = value;
		}
	}

	object IBamlDictionaryKey.KeyObject
	{
		get
		{
			return _keyObject;
		}
		set
		{
			_keyObject = value;
		}
	}

	long IBamlDictionaryKey.ValuePositionPosition
	{
		get
		{
			return _valuePositionPosition;
		}
		set
		{
			_valuePositionPosition = value;
		}
	}

	bool IBamlDictionaryKey.Shared
	{
		get
		{
			return _flags[_sharedSection] == 1;
		}
		set
		{
			_flags[_sharedSection] = (value ? 1 : 0);
		}
	}

	bool IBamlDictionaryKey.SharedSet
	{
		get
		{
			return _flags[_sharedSetSection] == 1;
		}
		set
		{
			_flags[_sharedSetSection] = (value ? 1 : 0);
		}
	}

	object[] IBamlDictionaryKey.StaticResourceValues
	{
		get
		{
			return _staticResourceValues;
		}
		set
		{
			_staticResourceValues = value;
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _sharedSetSection;

	internal BamlDefAttributeKeyTypeRecord()
	{
		Pin();
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.LoadRecordData(bamlBinaryReader);
		_valuePosition = bamlBinaryReader.ReadInt32();
		((IBamlDictionaryKey)this).Shared = bamlBinaryReader.ReadBoolean();
		((IBamlDictionaryKey)this).SharedSet = bamlBinaryReader.ReadBoolean();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		base.WriteRecordData(bamlBinaryWriter);
		_valuePositionPosition = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		bamlBinaryWriter.Write(_valuePosition);
		bamlBinaryWriter.Write(((IBamlDictionaryKey)this).Shared);
		bamlBinaryWriter.Write(((IBamlDictionaryKey)this).SharedSet);
	}

	void IBamlDictionaryKey.UpdateValuePosition(int newPosition, BinaryWriter bamlBinaryWriter)
	{
		long num = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		int num2 = (int)(_valuePositionPosition - num);
		bamlBinaryWriter.Seek(num2, SeekOrigin.Current);
		bamlBinaryWriter.Write(newPosition);
		bamlBinaryWriter.Seek(-4 - num2, SeekOrigin.Current);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlDefAttributeKeyTypeRecord obj = (BamlDefAttributeKeyTypeRecord)record;
		obj._valuePosition = _valuePosition;
		obj._valuePositionPosition = _valuePositionPosition;
		obj._keyObject = _keyObject;
		obj._staticResourceValues = _staticResourceValues;
	}
}

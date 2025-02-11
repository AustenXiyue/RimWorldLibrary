using System.Collections.Specialized;
using System.IO;

namespace System.Windows.Markup;

internal class BamlTypeInfoRecord : BamlVariableSizedRecord
{
	[Flags]
	private enum TypeInfoFlags : byte
	{
		Internal = 1,
		UnusedTwo = 2,
		UnusedThree = 4
	}

	private static BitVector32.Section _typeIdLowSection = BitVector32.CreateSection(255, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _typeIdHighSection = BitVector32.CreateSection(255, _typeIdLowSection);

	private TypeInfoFlags _typeInfoFlags;

	private short _assemblyId = -1;

	private string _typeFullName;

	private Type _type;

	internal short TypeId
	{
		get
		{
			return (short)((short)_flags[_typeIdLowSection] | (short)(_flags[_typeIdHighSection] << 8));
		}
		set
		{
			_flags[_typeIdLowSection] = (short)(value & 0xFF);
			_flags[_typeIdHighSection] = (short)((value & 0xFF00) >> 8);
		}
	}

	internal short AssemblyId
	{
		get
		{
			return _assemblyId;
		}
		set
		{
			if (_assemblyId > 4095)
			{
				throw new XamlParseException(SR.ParserTooManyAssemblies);
			}
			_assemblyId = value;
		}
	}

	internal string TypeFullName
	{
		get
		{
			return _typeFullName;
		}
		set
		{
			_typeFullName = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.TypeInfo;

	internal Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal string ClrNamespace
	{
		get
		{
			int num = _typeFullName.LastIndexOf('.');
			if (num <= 0)
			{
				return string.Empty;
			}
			return _typeFullName.Substring(0, num);
		}
	}

	internal virtual bool HasSerializer => false;

	internal bool IsInternalType
	{
		get
		{
			return (_typeInfoFlags & TypeInfoFlags.Internal) == TypeInfoFlags.Internal;
		}
		set
		{
			if (value)
			{
				_typeInfoFlags |= TypeInfoFlags.Internal;
			}
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _typeIdHighSection;

	internal BamlTypeInfoRecord()
	{
		Pin();
		TypeId = -1;
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		TypeId = bamlBinaryReader.ReadInt16();
		AssemblyId = bamlBinaryReader.ReadInt16();
		TypeFullName = bamlBinaryReader.ReadString();
		_typeInfoFlags = (TypeInfoFlags)(AssemblyId >> 12);
		_assemblyId &= 4095;
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(TypeId);
		bamlBinaryWriter.Write((short)((ushort)AssemblyId | ((uint)_typeInfoFlags << 12)));
		bamlBinaryWriter.Write(TypeFullName);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlTypeInfoRecord obj = (BamlTypeInfoRecord)record;
		obj._typeInfoFlags = _typeInfoFlags;
		obj._assemblyId = _assemblyId;
		obj._typeFullName = _typeFullName;
		obj._type = _type;
	}
}

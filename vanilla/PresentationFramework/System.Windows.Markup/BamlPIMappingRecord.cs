using System.Collections.Specialized;
using System.IO;

namespace System.Windows.Markup;

internal class BamlPIMappingRecord : BamlVariableSizedRecord
{
	private static BitVector32.Section _assemblyIdLowSection = BitVector32.CreateSection(255, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _assemblyIdHighSection = BitVector32.CreateSection(255, _assemblyIdLowSection);

	private string _xmlns;

	private string _clrns;

	internal override BamlRecordType RecordType => BamlRecordType.PIMapping;

	internal string XmlNamespace
	{
		get
		{
			return _xmlns;
		}
		set
		{
			_xmlns = value;
		}
	}

	internal string ClrNamespace
	{
		get
		{
			return _clrns;
		}
		set
		{
			_clrns = value;
		}
	}

	internal short AssemblyId
	{
		get
		{
			return (short)((short)_flags[_assemblyIdLowSection] | (short)(_flags[_assemblyIdHighSection] << 8));
		}
		set
		{
			_flags[_assemblyIdLowSection] = (short)(value & 0xFF);
			_flags[_assemblyIdHighSection] = (short)((value & 0xFF00) >> 8);
		}
	}

	internal new static BitVector32.Section LastFlagsSection => _assemblyIdHighSection;

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		XmlNamespace = bamlBinaryReader.ReadString();
		ClrNamespace = bamlBinaryReader.ReadString();
		AssemblyId = bamlBinaryReader.ReadInt16();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(XmlNamespace);
		bamlBinaryWriter.Write(ClrNamespace);
		bamlBinaryWriter.Write(AssemblyId);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlPIMappingRecord obj = (BamlPIMappingRecord)record;
		obj._xmlns = _xmlns;
		obj._clrns = _clrns;
	}
}

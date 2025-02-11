using System.Collections.Specialized;
using System.IO;
using System.Reflection;

namespace System.Windows.Markup;

internal class BamlAssemblyInfoRecord : BamlVariableSizedRecord
{
	private static BitVector32.Section _assemblyIdLowSection = BitVector32.CreateSection(255, BamlVariableSizedRecord.LastFlagsSection);

	private static BitVector32.Section _assemblyIdHighSection = BitVector32.CreateSection(255, _assemblyIdLowSection);

	private string _assemblyFullName;

	private Assembly _assembly;

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

	internal string AssemblyFullName
	{
		get
		{
			return _assemblyFullName;
		}
		set
		{
			_assemblyFullName = value;
		}
	}

	internal override BamlRecordType RecordType => BamlRecordType.AssemblyInfo;

	internal Assembly Assembly
	{
		get
		{
			return _assembly;
		}
		set
		{
			_assembly = value;
		}
	}

	internal BamlAssemblyInfoRecord()
	{
		Pin();
		AssemblyId = -1;
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		AssemblyId = bamlBinaryReader.ReadInt16();
		AssemblyFullName = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(AssemblyId);
		bamlBinaryWriter.Write(AssemblyFullName);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlAssemblyInfoRecord obj = (BamlAssemblyInfoRecord)record;
		obj._assemblyFullName = _assemblyFullName;
		obj._assembly = _assembly;
	}
}

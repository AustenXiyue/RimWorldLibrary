using System.IO;

namespace System.Windows.Markup;

internal class BamlXmlnsPropertyRecord : BamlVariableSizedRecord
{
	private string _prefix;

	private string _xmlNamespace;

	private short[] _assemblyIds;

	internal override BamlRecordType RecordType => BamlRecordType.XmlnsProperty;

	internal string Prefix
	{
		get
		{
			return _prefix;
		}
		set
		{
			_prefix = value;
		}
	}

	internal string XmlNamespace
	{
		get
		{
			return _xmlNamespace;
		}
		set
		{
			_xmlNamespace = value;
		}
	}

	internal short[] AssemblyIds
	{
		get
		{
			return _assemblyIds;
		}
		set
		{
			_assemblyIds = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		Prefix = bamlBinaryReader.ReadString();
		XmlNamespace = bamlBinaryReader.ReadString();
		short num = bamlBinaryReader.ReadInt16();
		if (num > 0)
		{
			AssemblyIds = new short[num];
			for (short num2 = 0; num2 < num; num2++)
			{
				AssemblyIds[num2] = bamlBinaryReader.ReadInt16();
			}
		}
		else
		{
			AssemblyIds = null;
		}
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(Prefix);
		bamlBinaryWriter.Write(XmlNamespace);
		short num = 0;
		if (AssemblyIds != null && AssemblyIds.Length != 0)
		{
			num = (short)AssemblyIds.Length;
		}
		bamlBinaryWriter.Write(num);
		if (num > 0)
		{
			for (short num2 = 0; num2 < num; num2++)
			{
				bamlBinaryWriter.Write(AssemblyIds[num2]);
			}
		}
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlXmlnsPropertyRecord obj = (BamlXmlnsPropertyRecord)record;
		obj._prefix = _prefix;
		obj._xmlNamespace = _xmlNamespace;
		obj._assemblyIds = _assemblyIds;
	}
}

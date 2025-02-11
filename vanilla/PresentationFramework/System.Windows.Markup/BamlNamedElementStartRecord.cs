using System.IO;

namespace System.Windows.Markup;

internal class BamlNamedElementStartRecord : BamlElementStartRecord
{
	private bool _isTemplateChild;

	private string _runtimeName;

	internal string RuntimeName
	{
		get
		{
			return _runtimeName;
		}
		set
		{
			_runtimeName = value;
		}
	}

	internal bool IsTemplateChild
	{
		get
		{
			return _isTemplateChild;
		}
		set
		{
			_isTemplateChild = value;
		}
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		base.TypeId = bamlBinaryReader.ReadInt16();
		RuntimeName = bamlBinaryReader.ReadString();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(base.TypeId);
		if (RuntimeName != null)
		{
			bamlBinaryWriter.Write(RuntimeName);
		}
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlNamedElementStartRecord obj = (BamlNamedElementStartRecord)record;
		obj._isTemplateChild = _isTemplateChild;
		obj._runtimeName = _runtimeName;
	}
}

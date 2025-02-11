using System.IO;

namespace System.Windows.Markup;

internal class BamlDocumentStartRecord : BamlRecord
{
	private int _maxAsyncRecords = -1;

	private bool _loadAsync;

	private long _filePos = -1L;

	private bool _debugBaml;

	internal override BamlRecordType RecordType => BamlRecordType.DocumentStart;

	internal bool LoadAsync
	{
		get
		{
			return _loadAsync;
		}
		set
		{
			_loadAsync = value;
		}
	}

	internal int MaxAsyncRecords
	{
		get
		{
			return _maxAsyncRecords;
		}
		set
		{
			_maxAsyncRecords = value;
		}
	}

	internal long FilePos
	{
		get
		{
			return _filePos;
		}
		set
		{
			_filePos = value;
		}
	}

	internal bool DebugBaml
	{
		get
		{
			return _debugBaml;
		}
		set
		{
			_debugBaml = value;
		}
	}

	internal override void Write(BinaryWriter bamlBinaryWriter)
	{
		if (FilePos == -1 && bamlBinaryWriter != null)
		{
			FilePos = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		}
		base.Write(bamlBinaryWriter);
	}

	internal virtual void UpdateWrite(BinaryWriter bamlBinaryWriter)
	{
		long num = bamlBinaryWriter.Seek(0, SeekOrigin.Current);
		bamlBinaryWriter.Seek((int)FilePos, SeekOrigin.Begin);
		Write(bamlBinaryWriter);
		bamlBinaryWriter.Seek((int)num, SeekOrigin.Begin);
	}

	internal override void LoadRecordData(BinaryReader bamlBinaryReader)
	{
		LoadAsync = bamlBinaryReader.ReadBoolean();
		MaxAsyncRecords = bamlBinaryReader.ReadInt32();
		DebugBaml = bamlBinaryReader.ReadBoolean();
	}

	internal override void WriteRecordData(BinaryWriter bamlBinaryWriter)
	{
		bamlBinaryWriter.Write(LoadAsync);
		bamlBinaryWriter.Write(MaxAsyncRecords);
		bamlBinaryWriter.Write(DebugBaml);
	}

	internal override void Copy(BamlRecord record)
	{
		base.Copy(record);
		BamlDocumentStartRecord obj = (BamlDocumentStartRecord)record;
		obj._maxAsyncRecords = _maxAsyncRecords;
		obj._loadAsync = _loadAsync;
		obj._filePos = _filePos;
		obj._debugBaml = _debugBaml;
	}
}

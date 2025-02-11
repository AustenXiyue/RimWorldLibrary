using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MS.Internal.IO.Packaging;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

internal class DataSpaceManager
{
	private class TransformInstance
	{
		internal string typeName;

		internal IDataTransform transformReference;

		internal TransformEnvironment transformEnvironment;

		internal Stream transformPrimaryStream;

		internal StorageInfo transformStorage;

		private int _classType;

		private byte[] _extraData;

		internal byte[] ExtraData
		{
			get
			{
				return _extraData;
			}
			set
			{
				_extraData = value;
			}
		}

		internal int ClassType => _classType;

		internal TransformInstance(int classType, string name)
			: this(classType, name, null, null, null, null)
		{
		}

		internal TransformInstance(int classType, string name, IDataTransform instance, TransformEnvironment environment)
			: this(classType, name, instance, environment, null, null)
		{
		}

		internal TransformInstance(int classType, string name, IDataTransform instance, TransformEnvironment environment, Stream primaryStream, StorageInfo storage)
		{
			typeName = name;
			transformReference = instance;
			transformEnvironment = environment;
			transformPrimaryStream = primaryStream;
			transformStorage = storage;
			_classType = classType;
		}
	}

	private class DirtyStateTrackingStream : Stream
	{
		private bool _dirty;

		private Stream _baseStream;

		public override bool CanRead
		{
			get
			{
				if (_baseStream != null)
				{
					return _baseStream.CanRead;
				}
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (_baseStream != null)
				{
					return _baseStream.CanSeek;
				}
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (_baseStream != null)
				{
					return _baseStream.CanWrite;
				}
				return false;
			}
		}

		public override long Length
		{
			get
			{
				CheckDisposed();
				return _baseStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				CheckDisposed();
				return _baseStream.Position;
			}
			set
			{
				CheckDisposed();
				_baseStream.Position = value;
			}
		}

		internal bool DirtyFlag
		{
			get
			{
				if (_baseStream != null)
				{
					return _dirty;
				}
				return false;
			}
		}

		internal Stream BaseStream => _baseStream;

		public override void SetLength(long newLength)
		{
			CheckDisposed();
			if (newLength != _baseStream.Length)
			{
				_dirty = true;
			}
			_baseStream.SetLength(newLength);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			CheckDisposed();
			return _baseStream.Seek(offset, origin);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			return _baseStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			_baseStream.Write(buffer, offset, count);
			_dirty = true;
		}

		public override void Flush()
		{
			CheckDisposed();
			_baseStream.Flush();
		}

		internal DirtyStateTrackingStream(Stream baseStream)
		{
			_baseStream = baseStream;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _baseStream != null)
				{
					_baseStream.Close();
				}
			}
			finally
			{
				_baseStream = null;
				base.Dispose(disposing);
			}
		}

		private void CheckDisposed()
		{
			if (_baseStream == null)
			{
				throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
			}
		}
	}

	private struct DataSpaceDefinition
	{
		private ArrayList _transformStack;

		private byte[] _extraData;

		internal ArrayList TransformStack => _transformStack;

		internal byte[] ExtraData => _extraData;

		internal DataSpaceDefinition(ArrayList transformStack, byte[] extraData)
		{
			_transformStack = transformStack;
			_extraData = extraData;
		}
	}

	internal delegate void TransformInitializeEventHandler(object sender, TransformInitializationEventArgs e);

	private const int KnownBytesInMapTableHeader = 8;

	private const int KnownBytesInDataSpaceDefinitionHeader = 8;

	private const int KnownBytesInTransformDefinitionHeader = 8;

	private const int AllowedExtraDataMaximumSize = 8192;

	private const string DataSpaceStorageName = "\u0006DataSpaces";

	private const string DataSpaceVersionName = "Version";

	private const string DataSpaceMapTableName = "DataSpaceMap";

	private const string DataSpaceDefinitionsStorageName = "DataSpaceInfo";

	private const string TransformDefinitions = "TransformInfo";

	private const string TransformPrimaryInfo = "\u0006Primary";

	private static readonly string DataSpaceVersionIdentifier;

	private static readonly VersionPair DataSpaceCurrentWriterVersion;

	private static readonly VersionPair DataSpaceCurrentReaderVersion;

	private static readonly VersionPair DataSpaceCurrentUpdaterVersion;

	private FormatVersion _fileFormatVersion;

	private bool _dirtyFlag;

	private StorageRoot _associatedStorage;

	private SortedList _dataSpaceMap;

	private byte[] _mapTableHeaderPreservation;

	private Hashtable _dataSpaceDefinitions;

	private Hashtable _transformDefinitions;

	private ArrayList _transformedStreams;

	private static readonly Hashtable _transformLookupTable;

	internal const int TransformIdentifierTypes_PredefinedTransformName = 1;

	internal int Count
	{
		get
		{
			CheckDisposedStatus();
			return _dataSpaceMap.Count;
		}
	}

	private bool DirtyFlag
	{
		get
		{
			if (_dirtyFlag)
			{
				return true;
			}
			foreach (string key in _transformDefinitions.Keys)
			{
				if (((DirtyStateTrackingStream)GetTransformInstanceOf(key).transformPrimaryStream).DirtyFlag)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal event TransformInitializeEventHandler OnTransformInitialization;

	static DataSpaceManager()
	{
		DataSpaceVersionIdentifier = "Microsoft.Container.DataSpaces";
		DataSpaceCurrentWriterVersion = new VersionPair(1, 0);
		DataSpaceCurrentReaderVersion = new VersionPair(1, 0);
		DataSpaceCurrentUpdaterVersion = new VersionPair(1, 0);
		_transformLookupTable = new Hashtable(ContainerUtilities.StringCaseInsensitiveComparer);
		_transformLookupTable[RightsManagementEncryptionTransform.ClassTransformIdentifier] = "System.IO.Packaging.RightsManagementEncryptionTransform";
		_transformLookupTable[CompressionTransform.ClassTransformIdentifier] = "System.IO.Packaging.CompressionTransform";
	}

	internal DataSpaceManager(StorageRoot containerInstance)
	{
		_associatedStorage = containerInstance;
		StorageInfo storageInfo = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
		_dataSpaceMap = new SortedList();
		_mapTableHeaderPreservation = Array.Empty<byte>();
		_dataSpaceDefinitions = new Hashtable(ContainerUtilities.StringCaseInsensitiveComparer);
		_transformDefinitions = new Hashtable(ContainerUtilities.StringCaseInsensitiveComparer);
		_transformedStreams = new ArrayList();
		if (storageInfo.Exists)
		{
			ReadDataSpaceMap();
			ReadDataSpaceDefinitions();
			ReadTransformDefinitions();
		}
	}

	public void Dispose()
	{
		CheckDisposedStatus();
		foreach (StreamWithDictionary transformedStream in _transformedStreams)
		{
			if (!transformedStream.Disposed)
			{
				transformedStream.Flush();
			}
		}
		_transformedStreams.Clear();
		foreach (TransformInstance value in _transformDefinitions.Values)
		{
			IDataTransform transformReference = value.transformReference;
			if (transformReference != null && transformReference is IDisposable)
			{
				((IDisposable)transformReference).Dispose();
			}
		}
		if (FileAccess.Read != _associatedStorage.OpenAccess && DirtyFlag)
		{
			WriteDataSpaceMap();
			WriteDataSpaceDefinitions();
			WriteTransformDefinitions();
		}
		_dataSpaceMap = null;
		_dataSpaceDefinitions = null;
		_transformDefinitions = null;
	}

	internal void RemoveContainerFromDataSpaceMap(CompoundFileReference target)
	{
		CheckDisposedStatus();
		if (_dataSpaceMap.Contains(target))
		{
			_dataSpaceMap.Remove(target);
			_dirtyFlag = true;
		}
	}

	internal void CheckDisposedStatus()
	{
		_associatedStorage.CheckRootDisposedStatus();
		if (_dataSpaceMap == null)
		{
			throw new ObjectDisposedException(null, SR.DataSpaceManagerDisposed);
		}
	}

	internal void DefineDataSpace(string[] transformStack, string newDataSpaceLabel)
	{
		CheckDisposedStatus();
		if (transformStack == null || transformStack.Length == 0)
		{
			throw new ArgumentException(SR.TransformStackValid);
		}
		ContainerUtilities.CheckStringAgainstNullAndEmpty(newDataSpaceLabel, "newDataSpaceLabel");
		ContainerUtilities.CheckStringAgainstReservedName(newDataSpaceLabel, "newDataSpaceLabel");
		if (DataSpaceIsDefined(newDataSpaceLabel))
		{
			throw new ArgumentException(SR.DataSpaceLabelInUse);
		}
		foreach (string text in transformStack)
		{
			ContainerUtilities.CheckStringAgainstNullAndEmpty(text, "Transform label");
			if (!TransformLabelIsDefined(text))
			{
				throw new ArgumentException(SR.TransformLabelUndefined);
			}
		}
		SetDataSpaceDefinition(newDataSpaceLabel, new DataSpaceDefinition(new ArrayList(transformStack), null));
		_dirtyFlag = true;
	}

	internal bool DataSpaceIsDefined(string dataSpaceLabel)
	{
		ContainerUtilities.CheckStringAgainstNullAndEmpty(dataSpaceLabel, "dataSpaceLabel");
		return _dataSpaceDefinitions.Contains(dataSpaceLabel);
	}

	private void SetDataSpaceDefinition(string dataSpaceLabel, DataSpaceDefinition definition)
	{
		_dataSpaceDefinitions[dataSpaceLabel] = definition;
	}

	private DataSpaceDefinition GetDataSpaceDefinition(string dataSpaceLabel)
	{
		return (DataSpaceDefinition)_dataSpaceDefinitions[dataSpaceLabel];
	}

	internal string DataSpaceOf(CompoundFileReference target)
	{
		if (_dataSpaceMap.Contains(target))
		{
			return (string)_dataSpaceMap[target];
		}
		return null;
	}

	internal List<IDataTransform> GetTransformsForStreamInfo(StreamInfo streamInfo)
	{
		string text = DataSpaceOf(streamInfo.StreamReference);
		if (text == null)
		{
			return new List<IDataTransform>(0);
		}
		ArrayList transformStack = GetDataSpaceDefinition(text).TransformStack;
		List<IDataTransform> list = new List<IDataTransform>(transformStack.Count);
		for (int i = 0; i < transformStack.Count; i++)
		{
			list.Add(GetTransformFromName(transformStack[i] as string));
		}
		return list;
	}

	internal string DefineDataSpace(string[] transformStack)
	{
		CheckDisposedStatus();
		long num = DateTime.Now.ToFileTime();
		string text = num.ToString(CultureInfo.InvariantCulture);
		while (DataSpaceIsDefined(text))
		{
			num++;
			text = num.ToString(CultureInfo.InvariantCulture);
		}
		DefineDataSpace(transformStack, text);
		return text;
	}

	private IDataTransform InstantiateDataTransformObject(int transformClassType, string transformClassName, TransformEnvironment transformEnvironment)
	{
		object obj = null;
		if (transformClassType != 1)
		{
			throw new NotSupportedException(SR.TransformTypeUnsupported);
		}
		if (((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)transformClassName, (object?)RightsManagementEncryptionTransform.ClassTransformIdentifier))
		{
			obj = new RightsManagementEncryptionTransform(transformEnvironment);
		}
		else
		{
			if (!((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)transformClassName, (object?)CompressionTransform.ClassTransformIdentifier))
			{
				throw new ArgumentException(SR.TransformLabelUndefined);
			}
			obj = new CompressionTransform(transformEnvironment);
		}
		if (obj != null)
		{
			if (!(obj is IDataTransform))
			{
				throw new ArgumentException(SR.TransformObjectImplementIDataTransform);
			}
			return (IDataTransform)obj;
		}
		return null;
	}

	internal bool TransformLabelIsDefined(string transformLabel)
	{
		return _transformDefinitions.Contains(transformLabel);
	}

	private void SetTransformDefinition(string transformLabel, TransformInstance definition)
	{
		_transformDefinitions[transformLabel] = definition;
	}

	private TransformInstance GetTransformInstanceOf(string transformLabel)
	{
		return _transformDefinitions[transformLabel] as TransformInstance;
	}

	internal Stream GetPrimaryInstanceStreamOf(string transformLabel)
	{
		TransformInstance transformInstanceOf = GetTransformInstanceOf(transformLabel);
		if (transformInstanceOf.transformPrimaryStream == null)
		{
			if (_associatedStorage.OpenAccess == FileAccess.Read)
			{
				transformInstanceOf.transformPrimaryStream = new DirtyStateTrackingStream(new MemoryStream(Array.Empty<byte>(), writable: false));
			}
			else
			{
				transformInstanceOf.transformPrimaryStream = new DirtyStateTrackingStream(new MemoryStream());
			}
		}
		return transformInstanceOf.transformPrimaryStream;
	}

	internal StorageInfo GetInstanceDataStorageOf(string transformLabel)
	{
		TransformInstance transformInstanceOf = GetTransformInstanceOf(transformLabel);
		if (transformInstanceOf.transformStorage == null)
		{
			StorageInfo storageInfo = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
			if (!storageInfo.Exists)
			{
				storageInfo.Create();
			}
			StorageInfo storageInfo2 = new StorageInfo(storageInfo, "TransformInfo");
			if (!storageInfo2.Exists)
			{
				storageInfo2.Create();
			}
			transformInstanceOf.transformStorage = new StorageInfo(storageInfo2, transformLabel);
		}
		return transformInstanceOf.transformStorage;
	}

	internal IDataTransform GetTransformFromName(string transformLabel)
	{
		if (!(_transformDefinitions[transformLabel] is TransformInstance { transformReference: var dataTransform } transformInstance))
		{
			return null;
		}
		if (dataTransform == null)
		{
			TransformEnvironment transformEnvironment = new TransformEnvironment(this, transformLabel);
			dataTransform = InstantiateDataTransformObject(transformInstance.ClassType, transformInstance.typeName, transformEnvironment);
			transformInstance.transformReference = dataTransform;
		}
		return dataTransform;
	}

	internal void DefineTransform(string transformClassName, string newTransformLabel)
	{
		CheckDisposedStatus();
		ContainerUtilities.CheckStringAgainstNullAndEmpty(transformClassName, "Transform identifier name");
		ContainerUtilities.CheckStringAgainstNullAndEmpty(newTransformLabel, "Transform label");
		ContainerUtilities.CheckStringAgainstReservedName(newTransformLabel, "Transform label");
		if (TransformLabelIsDefined(newTransformLabel))
		{
			throw new ArgumentException(SR.TransformLabelInUse);
		}
		TransformEnvironment transformEnvironment = new TransformEnvironment(this, newTransformLabel);
		TransformInstance transformInstance = new TransformInstance(1, transformClassName, null, transformEnvironment);
		SetTransformDefinition(newTransformLabel, transformInstance);
		IDataTransform dataTransform = (transformInstance.transformReference = InstantiateDataTransformObject(1, transformClassName, transformEnvironment));
		if (!dataTransform.IsReady)
		{
			CallTransformInitializers(new TransformInitializationEventArgs(dataTransform, null, null, newTransformLabel));
		}
		_dirtyFlag = true;
	}

	internal string DefineTransform(string transformClassName)
	{
		CheckDisposedStatus();
		long num = DateTime.Now.ToFileTime();
		string text = num.ToString(CultureInfo.InvariantCulture);
		while (TransformLabelIsDefined(text))
		{
			num++;
			text = num.ToString(CultureInfo.InvariantCulture);
		}
		DefineTransform(transformClassName, text);
		return text;
	}

	internal void CallTransformInitializers(TransformInitializationEventArgs initArguments)
	{
		if (this.OnTransformInitialization != null)
		{
			this.OnTransformInitialization(this, initArguments);
		}
	}

	private void ReadDataSpaceMap()
	{
		StorageInfo storageInfo = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
		StreamInfo streamInfo = new StreamInfo(storageInfo, "DataSpaceMap");
		if (!storageInfo.StreamExists("DataSpaceMap"))
		{
			return;
		}
		ReadDataSpaceVersionInformation(storageInfo);
		ThrowIfIncorrectReaderVersion();
		using Stream input = streamInfo.GetStream(FileMode.Open);
		using BinaryReader binaryReader = new BinaryReader(input, Encoding.Unicode);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		if (num < 8 || num2 < 0)
		{
			throw new FileFormatException(SR.CorruptedData);
		}
		int num3 = num - 8;
		if (0 < num3)
		{
			if (num3 > 8192)
			{
				throw new FileFormatException(SR.CorruptedData);
			}
			_mapTableHeaderPreservation = binaryReader.ReadBytes(num3);
			if (_mapTableHeaderPreservation.Length != num3)
			{
				throw new FileFormatException(SR.CorruptedData);
			}
		}
		_dataSpaceMap.Capacity = num2;
		for (int i = 0; i < num2; i++)
		{
			int num4 = binaryReader.ReadInt32();
			if (num4 < 0)
			{
				throw new FileFormatException(SR.CorruptedData);
			}
			int num5 = 4;
			int bytesRead;
			CompoundFileReference key = CompoundFileReference.Load(binaryReader, out bytesRead);
			checked
			{
				num5 += bytesRead;
				string value = ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(binaryReader, out bytesRead);
				num5 += bytesRead;
				_dataSpaceMap[key] = value;
				if (num4 != num5)
				{
					throw new IOException(SR.DataSpaceMapEntryInvalid);
				}
			}
		}
	}

	private void WriteDataSpaceMap()
	{
		ThrowIfIncorrectUpdaterVersion();
		StorageInfo storageInfo = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
		StreamInfo streamInfo = new StreamInfo(storageInfo, "DataSpaceMap");
		checked
		{
			if (0 < _dataSpaceMap.Count)
			{
				StreamInfo streamInfo2 = null;
				streamInfo2 = ((!storageInfo.StreamExists("Version")) ? storageInfo.CreateStream("Version") : storageInfo.GetStreamInfo("Version"));
				Stream stream = streamInfo2.GetStream();
				_fileFormatVersion.SaveToStream(stream);
				stream.Close();
				using Stream output = streamInfo.GetStream(FileMode.Create);
				using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.Unicode);
				binaryWriter.Write(8 + _mapTableHeaderPreservation.Length);
				binaryWriter.Write(_dataSpaceMap.Count);
				binaryWriter.Write(_mapTableHeaderPreservation);
				foreach (CompoundFileReference key in _dataSpaceMap.Keys)
				{
					string outputString = (string)_dataSpaceMap[key];
					int num = CompoundFileReference.Save(key, null);
					num += ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(null, outputString);
					num += 4;
					binaryWriter.Write(num);
					CompoundFileReference.Save(key, binaryWriter);
					ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(binaryWriter, outputString);
				}
				return;
			}
			if (storageInfo.StreamExists("DataSpaceMap"))
			{
				storageInfo.DeleteStream("DataSpaceMap");
			}
		}
	}

	private void ReadDataSpaceDefinitions()
	{
		ThrowIfIncorrectReaderVersion();
		StorageInfo storageInfo = new StorageInfo(new StorageInfo(_associatedStorage, "\u0006DataSpaces"), "DataSpaceInfo");
		if (!storageInfo.Exists)
		{
			return;
		}
		StreamInfo[] streams = storageInfo.GetStreams();
		foreach (StreamInfo streamInfo in streams)
		{
			using Stream input = streamInfo.GetStream(FileMode.Open);
			using BinaryReader binaryReader = new BinaryReader(input, Encoding.Unicode);
			int num = binaryReader.ReadInt32();
			int num2 = binaryReader.ReadInt32();
			if (num < 8 || num2 < 0)
			{
				throw new FileFormatException(SR.CorruptedData);
			}
			ArrayList arrayList = new ArrayList(num2);
			byte[] array = null;
			int num3 = num - 8;
			if (num3 > 8192)
			{
				throw new FileFormatException(SR.CorruptedData);
			}
			if (num3 > 0)
			{
				array = binaryReader.ReadBytes(num3);
				if (array.Length != num3)
				{
					throw new FileFormatException(SR.CorruptedData);
				}
			}
			for (int j = 0; j < num2; j++)
			{
				arrayList.Add(ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(binaryReader));
			}
			SetDataSpaceDefinition(streamInfo.Name, new DataSpaceDefinition(arrayList, array));
		}
	}

	private void WriteDataSpaceDefinitions()
	{
		ThrowIfIncorrectUpdaterVersion();
		StorageInfo parent = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
		if (0 >= _dataSpaceDefinitions.Count)
		{
			return;
		}
		StorageInfo storageInfo = new StorageInfo(parent, "DataSpaceInfo");
		storageInfo.Create();
		foreach (string key in _dataSpaceDefinitions.Keys)
		{
			using Stream output = new StreamInfo(storageInfo, key).GetStream();
			using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.Unicode);
			DataSpaceDefinition dataSpaceDefinition = (DataSpaceDefinition)_dataSpaceDefinitions[key];
			int num = 8;
			if (dataSpaceDefinition.ExtraData != null)
			{
				num = checked(num + dataSpaceDefinition.ExtraData.Length);
			}
			binaryWriter.Write(num);
			binaryWriter.Write(dataSpaceDefinition.TransformStack.Count);
			if (dataSpaceDefinition.ExtraData != null)
			{
				binaryWriter.Write(dataSpaceDefinition.ExtraData);
			}
			foreach (object item in dataSpaceDefinition.TransformStack)
			{
				ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(binaryWriter, (string)item);
			}
		}
	}

	private void ReadTransformDefinitions()
	{
		ThrowIfIncorrectReaderVersion();
		StorageInfo storageInfo = new StorageInfo(new StorageInfo(_associatedStorage, "\u0006DataSpaces"), "TransformInfo");
		if (!storageInfo.Exists)
		{
			return;
		}
		StorageInfo[] subStorages = storageInfo.GetSubStorages();
		checked
		{
			foreach (StorageInfo storageInfo2 in subStorages)
			{
				using Stream stream = new StreamInfo(storageInfo2, "\u0006Primary").GetStream(FileMode.Open);
				using BinaryReader binaryReader = new BinaryReader(stream, Encoding.Unicode);
				int num = binaryReader.ReadInt32();
				int classType = binaryReader.ReadInt32();
				if (num < 0)
				{
					throw new FileFormatException(SR.CorruptedData);
				}
				TransformInstance transformInstance = new TransformInstance(classType, ContainerUtilities.ReadByteLengthPrefixedDWordPaddedUnicodeString(binaryReader));
				int num2 = (int)(num - stream.Position);
				if (num2 < 0)
				{
					throw new FileFormatException(SR.CorruptedData);
				}
				if (num2 > 0)
				{
					if (num2 > 8192)
					{
						throw new FileFormatException(SR.CorruptedData);
					}
					byte[] array = binaryReader.ReadBytes(num2);
					if (array.Length != num2)
					{
						throw new FileFormatException(SR.CorruptedData);
					}
					transformInstance.ExtraData = array;
				}
				if (stream.Length > stream.Position)
				{
					int num3 = (int)(stream.Length - stream.Position);
					byte[] buffer = new byte[num3];
					PackagingUtilities.ReliableRead(stream, buffer, 0, num3);
					MemoryStream memoryStream;
					if (_associatedStorage.OpenAccess == FileAccess.Read)
					{
						memoryStream = new MemoryStream(buffer, writable: false);
					}
					else
					{
						memoryStream = new MemoryStream();
						memoryStream.Write(buffer, 0, num3);
					}
					memoryStream.Seek(0L, SeekOrigin.Begin);
					transformInstance.transformPrimaryStream = new DirtyStateTrackingStream(memoryStream);
				}
				transformInstance.transformStorage = storageInfo2;
				SetTransformDefinition(storageInfo2.Name, transformInstance);
			}
		}
	}

	private void WriteTransformDefinitions()
	{
		ThrowIfIncorrectUpdaterVersion();
		StorageInfo storageInfo = new StorageInfo(_associatedStorage, "\u0006DataSpaces");
		StorageInfo storageInfo2 = new StorageInfo(storageInfo, "TransformInfo");
		checked
		{
			if (0 < _transformDefinitions.Count)
			{
				foreach (string key in _transformDefinitions.Keys)
				{
					string text2 = null;
					TransformInstance transformInstanceOf = GetTransformInstanceOf(key);
					text2 = ((transformInstanceOf.transformEnvironment == null) ? key : transformInstanceOf.transformEnvironment.TransformLabel);
					using Stream stream = new StreamInfo(new StorageInfo(storageInfo2, text2), "\u0006Primary").GetStream();
					using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.Unicode);
					int num = 8 + ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(null, transformInstanceOf.typeName);
					if (transformInstanceOf.ExtraData != null)
					{
						num += transformInstanceOf.ExtraData.Length;
					}
					binaryWriter.Write(num);
					binaryWriter.Write(1);
					ContainerUtilities.WriteByteLengthPrefixedDWordPaddedUnicodeString(binaryWriter, transformInstanceOf.typeName);
					if (transformInstanceOf.ExtraData != null)
					{
						binaryWriter.Write(transformInstanceOf.ExtraData);
					}
					if (transformInstanceOf.transformPrimaryStream != null)
					{
						byte[] buffer = ((MemoryStream)((DirtyStateTrackingStream)transformInstanceOf.transformPrimaryStream).BaseStream).GetBuffer();
						stream.Write(buffer, 0, buffer.Length);
					}
				}
				return;
			}
			if (storageInfo2.Exists)
			{
				storageInfo.Delete(recursive: true, "TransformInfo");
			}
		}
	}

	internal void CreateDataSpaceMapping(CompoundFileReference containerReference, string label)
	{
		_dataSpaceMap[containerReference] = label;
		_dirtyFlag = true;
	}

	internal Stream CreateDataSpaceStream(CompoundFileStreamReference containerReference, Stream rawStream)
	{
		Stream stream = rawStream;
		IDictionary dictionary = new Hashtable();
		string text = _dataSpaceMap[containerReference] as string;
		foreach (string item in GetDataSpaceDefinition(text).TransformStack)
		{
			TransformInstance transformInstanceOf = GetTransformInstanceOf(item);
			if (transformInstanceOf.transformReference == null)
			{
				transformInstanceOf.transformEnvironment = new TransformEnvironment(this, item);
				transformInstanceOf.transformReference = InstantiateDataTransformObject(transformInstanceOf.ClassType, transformInstanceOf.typeName, transformInstanceOf.transformEnvironment);
			}
			IDataTransform transformReference = transformInstanceOf.transformReference;
			if (!transformReference.IsReady)
			{
				CallTransformInitializers(new TransformInitializationEventArgs(transformReference, text, containerReference.FullName, item));
				if (!transformReference.IsReady)
				{
					throw new InvalidOperationException(SR.TransformObjectInitFailed);
				}
			}
			stream = transformReference.GetTransformedStream(stream, dictionary);
		}
		stream = new BufferedStream(stream);
		stream = new StreamWithDictionary(stream, dictionary);
		_transformedStreams.Add(stream);
		return stream;
	}

	private void ReadDataSpaceVersionInformation(StorageInfo dataSpaceStorage)
	{
		if (_fileFormatVersion != null || !dataSpaceStorage.StreamExists("Version"))
		{
			return;
		}
		using Stream stream = dataSpaceStorage.GetStreamInfo("Version").GetStream(FileMode.Open);
		_fileFormatVersion = FormatVersion.LoadFromStream(stream);
		if (!((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)_fileFormatVersion.FeatureIdentifier, (object?)DataSpaceVersionIdentifier))
		{
			throw new FileFormatException(SR.Format(SR.InvalidTransformFeatureName, _fileFormatVersion.FeatureIdentifier, DataSpaceVersionIdentifier));
		}
		_fileFormatVersion.WriterVersion = DataSpaceCurrentWriterVersion;
	}

	private void EnsureDataSpaceVersionInformation()
	{
		if (_fileFormatVersion == null)
		{
			_fileFormatVersion = new FormatVersion(DataSpaceVersionIdentifier, DataSpaceCurrentWriterVersion, DataSpaceCurrentReaderVersion, DataSpaceCurrentUpdaterVersion);
		}
	}

	private void ThrowIfIncorrectReaderVersion()
	{
		EnsureDataSpaceVersionInformation();
		if (!_fileFormatVersion.IsReadableBy(DataSpaceCurrentReaderVersion))
		{
			throw new FileFormatException(SR.Format(SR.ReaderVersionError, _fileFormatVersion.ReaderVersion, DataSpaceCurrentReaderVersion));
		}
	}

	private void ThrowIfIncorrectUpdaterVersion()
	{
		EnsureDataSpaceVersionInformation();
		if (!_fileFormatVersion.IsUpdatableBy(DataSpaceCurrentUpdaterVersion))
		{
			throw new FileFormatException(SR.Format(SR.UpdaterVersionError, _fileFormatVersion.UpdaterVersion, DataSpaceCurrentUpdaterVersion));
		}
	}
}

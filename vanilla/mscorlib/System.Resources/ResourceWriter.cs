using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Versioning;
using System.Security;
using System.Text;

namespace System.Resources;

/// <summary>Writes resources in the system-default format to an output file or an output stream. This class cannot be inherited.</summary>
[ComVisible(true)]
public sealed class ResourceWriter : IResourceWriter, IDisposable
{
	private class PrecannedResource
	{
		internal string TypeName;

		internal byte[] Data;

		internal PrecannedResource(string typeName, byte[] data)
		{
			TypeName = typeName;
			Data = data;
		}
	}

	private class StreamWrapper
	{
		internal Stream m_stream;

		internal bool m_closeAfterWrite;

		internal StreamWrapper(Stream s, bool closeAfterWrite)
		{
			m_stream = s;
			m_closeAfterWrite = closeAfterWrite;
		}
	}

	private Func<Type, string> typeConverter;

	private const int _ExpectedNumberOfResources = 1000;

	private const int AverageNameSize = 40;

	private const int AverageValueSize = 40;

	private Dictionary<string, object> _resourceList;

	internal Stream _output;

	private Dictionary<string, object> _caseInsensitiveDups;

	private Dictionary<string, PrecannedResource> _preserializedData;

	private const int _DefaultBufferSize = 4096;

	/// <summary>Gets or sets a delegate that enables resource assemblies to be written that target versions of the .NET Framework prior to the .NET Framework 4 by using qualified assembly names.</summary>
	/// <returns>The type that is encapsulated by the delegate.</returns>
	public Func<Type, string> TypeNameConverter
	{
		get
		{
			return typeConverter;
		}
		set
		{
			typeConverter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceWriter" /> class that writes the resources to the specified file.</summary>
	/// <param name="fileName">The output file name. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileName" /> parameter is null. </exception>
	public ResourceWriter(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		_output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		_resourceList = new Dictionary<string, object>(1000, FastResourceComparer.Default);
		_caseInsensitiveDups = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceWriter" /> class that writes the resources to the provided stream.</summary>
	/// <param name="stream">The output stream. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="stream" /> parameter is not writable. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="stream" /> parameter is null. </exception>
	public ResourceWriter(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream was not writable."));
		}
		_output = stream;
		_resourceList = new Dictionary<string, object>(1000, FastResourceComparer.Default);
		_caseInsensitiveDups = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Adds a string resource to the list of resources to be written.</summary>
	/// <param name="name">The name of the resource. </param>
	/// <param name="value">The value of the resource. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this ResourceWriter. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed and its hash table is unavailable. </exception>
	public void AddResource(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	/// <summary>Adds a named resource specified as an object to the list of resources to be written.</summary>
	/// <param name="name">The name of the resource. </param>
	/// <param name="value">The value of the resource. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this <see cref="T:System.Resources.ResourceWriter" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed and its hash table is unavailable. </exception>
	public void AddResource(string name, object value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		if (value != null && value is Stream)
		{
			AddResourceInternal(name, (Stream)value, closeAfterWrite: false);
			return;
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	/// <summary>Adds a named resource specified as a stream to the list of resources to be written.</summary>
	/// <param name="name">The name of the resource to add. </param>
	/// <param name="value">The value of the resource to add. The resource must support the <see cref="P:System.IO.Stream.Length" /> property. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this <see cref="T:System.Resources.ResourceWriter" />.-or-The stream does not support the <see cref="P:System.IO.Stream.Length" /> property.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed.</exception>
	public void AddResource(string name, Stream value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		AddResourceInternal(name, value, closeAfterWrite: false);
	}

	/// <summary>Adds a named resource specified as a stream to the list of resources to be written, and specifies whether the stream should be closed after the <see cref="M:System.Resources.ResourceWriter.Generate" /> method is called. </summary>
	/// <param name="name">The name of the resource to add. </param>
	/// <param name="value">The value of the resource to add. The resource must support the <see cref="P:System.IO.Stream.Length" /> property. </param>
	/// <param name="closeAfterWrite">true to close the stream after the <see cref="M:System.Resources.ResourceWriter.Generate" /> method is called; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this <see cref="T:System.Resources.ResourceWriter" />.-or-The stream does not support the <see cref="P:System.IO.Stream.Length" /> property.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> or <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed.</exception>
	public void AddResource(string name, Stream value, bool closeAfterWrite)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		AddResourceInternal(name, value, closeAfterWrite);
	}

	private void AddResourceInternal(string name, Stream value, bool closeAfterWrite)
	{
		if (value == null)
		{
			_caseInsensitiveDups.Add(name, null);
			_resourceList.Add(name, value);
			return;
		}
		if (!value.CanSeek)
		{
			throw new ArgumentException(Environment.GetResourceString("Stream does not support seeking."));
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, new StreamWrapper(value, closeAfterWrite));
	}

	/// <summary>Adds a named resource specified as a byte array to the list of resources to be written.</summary>
	/// <param name="name">The name of the resource. </param>
	/// <param name="value">Value of the resource as an 8-bit unsigned integer array. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this <see cref="T:System.Resources.ResourceWriter" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed and its hash table is unavailable. </exception>
	public void AddResource(string name, byte[] value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		_caseInsensitiveDups.Add(name, null);
		_resourceList.Add(name, value);
	}

	/// <summary>Adds a unit of data as a resource to the list of resources to be written. </summary>
	/// <param name="name">A name that identifies the resource that contains the added data.</param>
	/// <param name="typeName">The type name of the added data. For more information, see the Remarks section.</param>
	/// <param name="serializedData">A byte array that contains the binary representation of the added data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" />, <paramref name="typeName" />, or <paramref name="serializedData" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> (or a name that varies only by capitalization) has already been added to this <see cref="T:System.Resources.ResourceWriter" /> object. </exception>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Resources.ResourceWriter" /> object is not initialized. The probable cause is that the <see cref="T:System.Resources.ResourceWriter" /> object is closed.</exception>
	public void AddResourceData(string name, string typeName, byte[] serializedData)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (serializedData == null)
		{
			throw new ArgumentNullException("serializedData");
		}
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		_caseInsensitiveDups.Add(name, null);
		if (_preserializedData == null)
		{
			_preserializedData = new Dictionary<string, PrecannedResource>(FastResourceComparer.Default);
		}
		_preserializedData.Add(name, new PrecannedResource(typeName, serializedData));
	}

	/// <summary>Saves the resources to the output stream and then closes it.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An error has occurred during serialization of the object. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void Close()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_resourceList != null)
			{
				Generate();
			}
			if (_output != null)
			{
				_output.Close();
			}
		}
		_output = null;
		_caseInsensitiveDups = null;
	}

	/// <summary>Allows users to close the resource file or stream, explicitly releasing resources.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An error has occurred during serialization of the object. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Saves all resources to the output stream in the system default format.</summary>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">An error occurred during serialization of the object. </exception>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Resources.ResourceWriter" /> has been closed and its hash table is unavailable. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public void Generate()
	{
		if (_resourceList == null)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The resource writer has already been closed and cannot be edited."));
		}
		BinaryWriter binaryWriter = new BinaryWriter(_output, Encoding.UTF8);
		List<string> list = new List<string>();
		binaryWriter.Write(ResourceManager.MagicNumber);
		binaryWriter.Write(ResourceManager.HeaderVersionNumber);
		MemoryStream memoryStream = new MemoryStream(240);
		BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream);
		binaryWriter2.Write(MultitargetingHelpers.GetAssemblyQualifiedName(typeof(ResourceReader), typeConverter));
		binaryWriter2.Write(ResourceManager.ResSetTypeName);
		binaryWriter2.Flush();
		binaryWriter.Write((int)memoryStream.Length);
		binaryWriter.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		binaryWriter.Write(2);
		int num = _resourceList.Count;
		if (_preserializedData != null)
		{
			num += _preserializedData.Count;
		}
		binaryWriter.Write(num);
		int[] array = new int[num];
		int[] array2 = new int[num];
		int num2 = 0;
		MemoryStream memoryStream2 = new MemoryStream(num * 40);
		BinaryWriter binaryWriter3 = new BinaryWriter(memoryStream2, Encoding.Unicode);
		Stream stream = null;
		try
		{
			string tempFileName = Path.GetTempFileName();
			File.SetAttributes(tempFileName, FileAttributes.NotContentIndexed | FileAttributes.Temporary);
			stream = new FileStream(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
		}
		catch (UnauthorizedAccessException)
		{
			stream = new MemoryStream();
		}
		catch (IOException)
		{
			stream = new MemoryStream();
		}
		using (stream)
		{
			BinaryWriter binaryWriter4 = new BinaryWriter(stream, Encoding.UTF8);
			IFormatter objFormatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence));
			SortedList sortedList = new SortedList(_resourceList, FastResourceComparer.Default);
			if (_preserializedData != null)
			{
				foreach (KeyValuePair<string, PrecannedResource> preserializedDatum in _preserializedData)
				{
					sortedList.Add(preserializedDatum.Key, preserializedDatum.Value);
				}
			}
			IDictionaryEnumerator enumerator2 = sortedList.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				array[num2] = FastResourceComparer.HashFunction((string)enumerator2.Key);
				array2[num2++] = (int)binaryWriter3.Seek(0, SeekOrigin.Current);
				binaryWriter3.Write((string)enumerator2.Key);
				binaryWriter3.Write((int)binaryWriter4.Seek(0, SeekOrigin.Current));
				object value = enumerator2.Value;
				ResourceTypeCode resourceTypeCode = FindTypeCode(value, list);
				Write7BitEncodedInt(binaryWriter4, (int)resourceTypeCode);
				if (value is PrecannedResource precannedResource)
				{
					binaryWriter4.Write(precannedResource.Data);
				}
				else
				{
					WriteValue(resourceTypeCode, value, binaryWriter4, objFormatter);
				}
			}
			binaryWriter.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				binaryWriter.Write(list[i]);
			}
			Array.Sort(array, array2);
			binaryWriter.Flush();
			int num3 = (int)binaryWriter.BaseStream.Position & 7;
			if (num3 > 0)
			{
				for (int j = 0; j < 8 - num3; j++)
				{
					binaryWriter.Write("PAD"[j % 3]);
				}
			}
			int[] array3 = array;
			foreach (int value2 in array3)
			{
				binaryWriter.Write(value2);
			}
			array3 = array2;
			foreach (int value3 in array3)
			{
				binaryWriter.Write(value3);
			}
			binaryWriter.Flush();
			binaryWriter3.Flush();
			binaryWriter4.Flush();
			int num4 = (int)(binaryWriter.Seek(0, SeekOrigin.Current) + memoryStream2.Length);
			num4 += 4;
			binaryWriter.Write(num4);
			binaryWriter.Write(memoryStream2.GetBuffer(), 0, (int)memoryStream2.Length);
			binaryWriter3.Close();
			stream.Position = 0L;
			stream.CopyTo(binaryWriter.BaseStream);
			binaryWriter4.Close();
		}
		binaryWriter.Flush();
		_resourceList = null;
	}

	private ResourceTypeCode FindTypeCode(object value, List<string> types)
	{
		if (value == null)
		{
			return ResourceTypeCode.Null;
		}
		Type type = value.GetType();
		if (type == typeof(string))
		{
			return ResourceTypeCode.String;
		}
		if (type == typeof(int))
		{
			return ResourceTypeCode.Int32;
		}
		if (type == typeof(bool))
		{
			return ResourceTypeCode.Boolean;
		}
		if (type == typeof(char))
		{
			return ResourceTypeCode.Char;
		}
		if (type == typeof(byte))
		{
			return ResourceTypeCode.Byte;
		}
		if (type == typeof(sbyte))
		{
			return ResourceTypeCode.SByte;
		}
		if (type == typeof(short))
		{
			return ResourceTypeCode.Int16;
		}
		if (type == typeof(long))
		{
			return ResourceTypeCode.Int64;
		}
		if (type == typeof(ushort))
		{
			return ResourceTypeCode.UInt16;
		}
		if (type == typeof(uint))
		{
			return ResourceTypeCode.UInt32;
		}
		if (type == typeof(ulong))
		{
			return ResourceTypeCode.UInt64;
		}
		if (type == typeof(float))
		{
			return ResourceTypeCode.Single;
		}
		if (type == typeof(double))
		{
			return ResourceTypeCode.Double;
		}
		if (type == typeof(decimal))
		{
			return ResourceTypeCode.Decimal;
		}
		if (type == typeof(DateTime))
		{
			return ResourceTypeCode.DateTime;
		}
		if (type == typeof(TimeSpan))
		{
			return ResourceTypeCode.TimeSpan;
		}
		if (type == typeof(byte[]))
		{
			return ResourceTypeCode.ByteArray;
		}
		if (type == typeof(StreamWrapper))
		{
			return ResourceTypeCode.Stream;
		}
		string text;
		if (type == typeof(PrecannedResource))
		{
			text = ((PrecannedResource)value).TypeName;
			if (text.StartsWith("ResourceTypeCode.", StringComparison.Ordinal))
			{
				text = text.Substring(17);
				return (ResourceTypeCode)Enum.Parse(typeof(ResourceTypeCode), text);
			}
		}
		else
		{
			text = MultitargetingHelpers.GetAssemblyQualifiedName(type, typeConverter);
		}
		int num = types.IndexOf(text);
		if (num == -1)
		{
			num = types.Count;
			types.Add(text);
		}
		return (ResourceTypeCode)(num + 64);
	}

	private void WriteValue(ResourceTypeCode typeCode, object value, BinaryWriter writer, IFormatter objFormatter)
	{
		switch (typeCode)
		{
		case ResourceTypeCode.String:
			writer.Write((string)value);
			break;
		case ResourceTypeCode.Boolean:
			writer.Write((bool)value);
			break;
		case ResourceTypeCode.Char:
			writer.Write((ushort)(char)value);
			break;
		case ResourceTypeCode.Byte:
			writer.Write((byte)value);
			break;
		case ResourceTypeCode.SByte:
			writer.Write((sbyte)value);
			break;
		case ResourceTypeCode.Int16:
			writer.Write((short)value);
			break;
		case ResourceTypeCode.UInt16:
			writer.Write((ushort)value);
			break;
		case ResourceTypeCode.Int32:
			writer.Write((int)value);
			break;
		case ResourceTypeCode.UInt32:
			writer.Write((uint)value);
			break;
		case ResourceTypeCode.Int64:
			writer.Write((long)value);
			break;
		case ResourceTypeCode.UInt64:
			writer.Write((ulong)value);
			break;
		case ResourceTypeCode.Single:
			writer.Write((float)value);
			break;
		case ResourceTypeCode.Double:
			writer.Write((double)value);
			break;
		case ResourceTypeCode.Decimal:
			writer.Write((decimal)value);
			break;
		case ResourceTypeCode.DateTime:
		{
			long value2 = ((DateTime)value).ToBinary();
			writer.Write(value2);
			break;
		}
		case ResourceTypeCode.TimeSpan:
			writer.Write(((TimeSpan)value).Ticks);
			break;
		case ResourceTypeCode.ByteArray:
		{
			byte[] array2 = (byte[])value;
			writer.Write(array2.Length);
			writer.Write(array2, 0, array2.Length);
			break;
		}
		case ResourceTypeCode.Stream:
		{
			StreamWrapper streamWrapper = (StreamWrapper)value;
			if (streamWrapper.m_stream.GetType() == typeof(MemoryStream))
			{
				MemoryStream obj = (MemoryStream)streamWrapper.m_stream;
				if (obj.Length > int.MaxValue)
				{
					throw new ArgumentException(Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
				}
				obj.InternalGetOriginAndLength(out var origin, out var length);
				byte[] buffer = obj.InternalGetBuffer();
				writer.Write(length);
				writer.Write(buffer, origin, length);
				break;
			}
			Stream stream = streamWrapper.m_stream;
			if (stream.Length > int.MaxValue)
			{
				throw new ArgumentException(Environment.GetResourceString("Stream length must be non-negative and less than 2^31 - 1 - origin."));
			}
			stream.Position = 0L;
			writer.Write((int)stream.Length);
			byte[] array = new byte[4096];
			int num = 0;
			while ((num = stream.Read(array, 0, array.Length)) != 0)
			{
				writer.Write(array, 0, num);
			}
			if (streamWrapper.m_closeAfterWrite)
			{
				stream.Close();
			}
			break;
		}
		default:
			objFormatter.Serialize(writer.BaseStream, value);
			break;
		case ResourceTypeCode.Null:
			break;
		}
	}

	private static void Write7BitEncodedInt(BinaryWriter store, int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			store.Write((byte)(num | 0x80));
		}
		store.Write((byte)num);
	}
}

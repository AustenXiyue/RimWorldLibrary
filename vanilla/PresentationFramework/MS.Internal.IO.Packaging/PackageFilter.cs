using System;
using System.Collections;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using MS.Internal.Interop;
using MS.Internal.IO.Packaging.Extensions;
using MS.Internal.Utility;

namespace MS.Internal.IO.Packaging;

internal class PackageFilter : IFilter
{
	private enum Progress
	{
		FilteringNotStarted,
		FilteringCoreProperties,
		FilteringContent,
		FilteringCompleted
	}

	private readonly string[] _IFilterAddinPath = new string[4] { "CLSID", null, "PersistentAddinsRegistered", "{89BCB740-6119-101A-BCB7-00DD010655AF}" };

	private readonly string[] _mimeContentTypeKey = new string[3] { "MIME", "Database", "Content Type" };

	private readonly string[] _persistentHandlerKey = new string[2] { null, "PersistentHandler" };

	private Package _package;

	private uint _currentChunkID;

	private IEnumerator _partIterator;

	private IFilter _currentFilter;

	private Stream _currentStream;

	private bool _firstChunkFromFilter;

	private Progress _progress;

	private bool _isInternalFilter;

	private IFILTER_INIT _grfFlags;

	private uint _cAttributes;

	private FULLPROPSPEC[] _aAttributes;

	private const string _extension = "Extension";

	internal PackageFilter(Package package)
	{
		if (package == null)
		{
			throw new ArgumentNullException("package");
		}
		_package = package;
		_partIterator = _package.GetParts().GetEnumerator();
	}

	public IFILTER_FLAGS Init(IFILTER_INIT grfFlags, uint cAttributes, FULLPROPSPEC[] aAttributes)
	{
		_grfFlags = grfFlags;
		_cAttributes = cAttributes;
		_aAttributes = aAttributes;
		_partIterator.Reset();
		_progress = Progress.FilteringNotStarted;
		return IFILTER_FLAGS.IFILTER_FLAGS_NONE;
	}

	public STAT_CHUNK GetChunk()
	{
		if (_progress == Progress.FilteringNotStarted)
		{
			MoveToNextFilter();
		}
		if (_progress == Progress.FilteringCompleted)
		{
			throw new COMException(SR.FilterEndOfChunks, -2147215616);
		}
		do
		{
			try
			{
				STAT_CHUNK chunk = _currentFilter.GetChunk();
				if ((!_isInternalFilter || chunk.idChunk != 0) && (_progress == Progress.FilteringCoreProperties || (chunk.flags & CHUNKSTATE.CHUNK_VALUE) != CHUNKSTATE.CHUNK_VALUE))
				{
					chunk.idChunk = AllocateChunkID();
					chunk.idChunkSource = chunk.idChunk;
					if (_firstChunkFromFilter)
					{
						chunk.breakType = CHUNK_BREAKTYPE.CHUNK_EOP;
						_firstChunkFromFilter = false;
					}
					return chunk;
				}
			}
			catch (COMException)
			{
			}
			catch (IOException)
			{
				if (_isInternalFilter)
				{
					throw;
				}
			}
			MoveToNextFilter();
		}
		while (_progress != Progress.FilteringCompleted);
		throw new COMException(SR.FilterEndOfChunks, -2147215616);
	}

	public void GetText(ref uint bufferCharacterCount, nint pBuffer)
	{
		if (_progress != Progress.FilteringContent)
		{
			throw new COMException(SR.FilterGetTextNotSupported, -2147215611);
		}
		_currentFilter.GetText(ref bufferCharacterCount, pBuffer);
	}

	public nint GetValue()
	{
		if (_progress != Progress.FilteringCoreProperties)
		{
			throw new COMException(SR.FilterGetValueNotSupported, -2147215610);
		}
		return _currentFilter.GetValue();
	}

	public nint BindRegion(FILTERREGION origPos, ref Guid riid)
	{
		throw new NotImplementedException(SR.FilterBindRegionNotImplemented);
	}

	private IFilter GetFilterFromClsid(Guid clsid)
	{
		Type typeFromCLSID = Type.GetTypeFromCLSID(clsid);
		try
		{
			return (IFilter)Activator.CreateInstance(typeFromCLSID);
		}
		catch (InvalidCastException)
		{
			return null;
		}
		catch (COMException)
		{
			return null;
		}
	}

	private void MoveToNextFilter()
	{
		_isInternalFilter = false;
		switch (_progress)
		{
		case Progress.FilteringNotStarted:
		{
			IndexingFilterMarshaler indexingFilterMarshaler2 = new IndexingFilterMarshaler(new CorePropertiesFilter(_package.PackageProperties));
			indexingFilterMarshaler2.ThrowOnEndOfChunks = false;
			_currentFilter = indexingFilterMarshaler2;
			_currentFilter.Init(_grfFlags, _cAttributes, _aAttributes);
			_isInternalFilter = true;
			_progress = Progress.FilteringCoreProperties;
			break;
		}
		case Progress.FilteringCoreProperties:
		case Progress.FilteringContent:
			if (_currentStream != null)
			{
				_currentStream.Close();
				_currentStream = null;
			}
			_currentFilter = null;
			while (_partIterator.MoveNext())
			{
				PackagePart packagePart = (PackagePart)_partIterator.Current;
				ContentType contentType = packagePart.ValidatedContentType();
				string filterClsid = GetFilterClsid(contentType, packagePart.Uri);
				if (filterClsid != null)
				{
					_currentFilter = GetFilterFromClsid(new Guid(filterClsid));
					if (_currentFilter != null)
					{
						_currentStream = packagePart.GetSeekableStream();
						ManagedIStream pstm = new ManagedIStream(_currentStream);
						try
						{
							((IPersistStreamWithArrays)_currentFilter).Load(pstm);
							_currentFilter.Init(_grfFlags, _cAttributes, _aAttributes);
						}
						catch (InvalidCastException)
						{
							goto IL_0139;
						}
						catch (COMException)
						{
							goto IL_0139;
						}
						catch (IOException)
						{
							goto IL_0139;
						}
						break;
					}
				}
				goto IL_0139;
				IL_0139:
				if (MS.Internal.Utility.BindUriHelper.IsXamlMimeType(contentType))
				{
					if (_currentStream == null)
					{
						_currentStream = packagePart.GetSeekableStream();
					}
					IndexingFilterMarshaler indexingFilterMarshaler = new IndexingFilterMarshaler(new XamlFilter(_currentStream));
					indexingFilterMarshaler.ThrowOnEndOfChunks = false;
					_currentFilter = indexingFilterMarshaler;
					_currentFilter.Init(_grfFlags, _cAttributes, _aAttributes);
					_isInternalFilter = true;
					break;
				}
				if (_currentStream != null)
				{
					_currentStream.Close();
					_currentStream = null;
				}
				_currentFilter = null;
			}
			if (_currentFilter == null)
			{
				_progress = Progress.FilteringCompleted;
				break;
			}
			_firstChunkFromFilter = true;
			_progress = Progress.FilteringContent;
			break;
		case Progress.FilteringCompleted:
			break;
		}
	}

	private uint AllocateChunkID()
	{
		Invariant.Assert(_currentChunkID <= uint.MaxValue);
		_currentChunkID++;
		return _currentChunkID;
	}

	private string GetFilterClsid(ContentType contentType, Uri partUri)
	{
		string text = null;
		if (contentType != null && !ContentType.Empty.AreTypeAndSubTypeEqual(contentType))
		{
			text = FileTypeGuidFromMimeType(contentType);
		}
		else
		{
			string partExtension = GetPartExtension(partUri);
			if (partExtension != null)
			{
				text = FileTypeGuidFromFileExtension(partExtension);
			}
		}
		if (text == null)
		{
			return null;
		}
		RegistryKey registryKey = FindSubkey(Registry.ClassesRoot, MakeRegistryPath(_IFilterAddinPath, text));
		if (registryKey == null)
		{
			return null;
		}
		return (string)registryKey.GetValue(null);
	}

	private static RegistryKey FindSubkey(RegistryKey containingKey, string[] keyPath)
	{
		RegistryKey registryKey = containingKey;
		for (int i = 0; i < keyPath.Length; i++)
		{
			if (registryKey == null)
			{
				return null;
			}
			registryKey = registryKey.OpenSubKey(keyPath[i]);
		}
		return registryKey;
	}

	private string FileTypeGuidFromMimeType(ContentType contentType)
	{
		RegistryKey registryKey = FindSubkey(Registry.ClassesRoot, _mimeContentTypeKey)?.OpenSubKey(contentType.ToString());
		if (registryKey == null)
		{
			return null;
		}
		string text = (string)registryKey.GetValue("Extension");
		if (text == null)
		{
			return null;
		}
		return FileTypeGuidFromFileExtension(text);
	}

	private string FileTypeGuidFromFileExtension(string dottedExtensionName)
	{
		RegistryKey registryKey = FindSubkey(Registry.ClassesRoot, MakeRegistryPath(_persistentHandlerKey, dottedExtensionName));
		if (registryKey != null)
		{
			return (string)registryKey.GetValue(null);
		}
		return null;
	}

	private string GetPartExtension(Uri partUri)
	{
		Invariant.Assert(partUri != null);
		string extension = Path.GetExtension(PackUriHelper.GetStringForPartUri(partUri));
		if (extension == string.Empty)
		{
			return null;
		}
		return extension;
	}

	private static string[] MakeRegistryPath(string[] pathWithGaps, params string[] stopGaps)
	{
		string[] array = (string[])pathWithGaps.Clone();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				array[i] = stopGaps[num];
				num++;
			}
		}
		return array;
	}
}

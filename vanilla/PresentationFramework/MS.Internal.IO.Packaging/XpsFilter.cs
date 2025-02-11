using System;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[Guid("0B8732A6-AF74-498c-A251-9DC86B0538B0")]
internal sealed class XpsFilter : IFilter, IPersistFile, IPersistStream
{
	[ComVisible(false)]
	private static readonly Guid _filterClsid = new Guid(193409702u, 44916, 18828, 162, 81, 157, 200, 107, 5, 56, 176);

	[ComVisible(false)]
	private IFilter _filter;

	[ComVisible(false)]
	private Package _package;

	[ComVisible(false)]
	private EncryptedPackageEnvelope _encryptedPackage;

	[ComVisible(false)]
	private string _xpsFileName;

	[ComVisible(false)]
	private Stream _packageStream;

	[ComVisible(false)]
	private const int _int16Size = 2;

	[ComVisible(false)]
	private const uint _maxTextBufferSizeInCharacters = 4096u;

	[ComVisible(false)]
	private const int _maxMemoryStreamBuffer = 1048576;

	IFILTER_FLAGS IFilter.Init([In] IFILTER_INIT grfFlags, [In] uint cAttributes, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] FULLPROPSPEC[] aAttributes)
	{
		if (_filter == null)
		{
			throw new COMException(SR.FileToFilterNotLoaded, -2147467259);
		}
		if (cAttributes != 0 && aAttributes == null)
		{
			throw new COMException(SR.FilterInitInvalidAttributes, -2147024809);
		}
		return _filter.Init(grfFlags, cAttributes, aAttributes);
	}

	STAT_CHUNK IFilter.GetChunk()
	{
		if (_filter == null)
		{
			throw new COMException(SR.FileToFilterNotLoaded, -2147215613);
		}
		try
		{
			return _filter.GetChunk();
		}
		catch (COMException ex)
		{
			if (ex.ErrorCode == -2147215616)
			{
				ReleaseResources();
			}
			throw;
		}
	}

	void IFilter.GetText(ref uint bufCharacterCount, nint pBuffer)
	{
		if (_filter == null)
		{
			throw new COMException(SR.FileToFilterNotLoaded, -2147215613);
		}
		if (pBuffer == IntPtr.Zero)
		{
			throw new NullReferenceException(SR.FilterNullGetTextBufferPointer);
		}
		if (bufCharacterCount == 0)
		{
			return;
		}
		if (bufCharacterCount == 1)
		{
			Marshal.WriteInt16(pBuffer, 0);
			return;
		}
		uint num = bufCharacterCount;
		if (bufCharacterCount > 4096)
		{
			bufCharacterCount = 4096u;
		}
		uint num2 = --bufCharacterCount;
		_filter.GetText(ref bufCharacterCount, pBuffer);
		if (bufCharacterCount > num2)
		{
			throw new COMException(SR.AuxiliaryFilterReturnedAnomalousCountOfCharacters, -2147215613);
		}
		if (num == 2 && Marshal.ReadInt16(pBuffer) == 0)
		{
			bufCharacterCount = 2u;
			_filter.GetText(ref bufCharacterCount, pBuffer);
			if (bufCharacterCount > 2)
			{
				throw new COMException(SR.AuxiliaryFilterReturnedAnomalousCountOfCharacters, -2147215613);
			}
			if (bufCharacterCount == 2)
			{
				Invariant.Assert(Marshal.ReadInt16(pBuffer, 2) == 0);
				bufCharacterCount = 1u;
			}
		}
		Marshal.WriteInt16(pBuffer, (int)(bufCharacterCount * 2), 0);
		bufCharacterCount++;
	}

	nint IFilter.GetValue()
	{
		if (_filter == null)
		{
			throw new COMException(SR.FileToFilterNotLoaded, -2147215613);
		}
		return _filter.GetValue();
	}

	nint IFilter.BindRegion([In] FILTERREGION origPos, [In] ref Guid riid)
	{
		throw new NotImplementedException(SR.FilterBindRegionNotImplemented);
	}

	void IPersistFile.GetClassID(out Guid pClassID)
	{
		pClassID = _filterClsid;
	}

	[PreserveSig]
	int IPersistFile.GetCurFile(out string ppszFileName)
	{
		ppszFileName = null;
		if (_filter == null || _xpsFileName == null)
		{
			ppszFileName = "*.xps";
			return 1;
		}
		ppszFileName = _xpsFileName;
		return 0;
	}

	[PreserveSig]
	int IPersistFile.IsDirty()
	{
		return 1;
	}

	void IPersistFile.Load(string pszFileName, int dwMode)
	{
		if (pszFileName == null || pszFileName == string.Empty)
		{
			throw new ArgumentException(SR.FileNameNullOrEmpty, "pszFileName");
		}
		if ((dwMode & 0x1000) == 4096)
		{
			throw new ArgumentException(SR.FilterLoadInvalidModeFlag, "dwMode");
		}
		FileMode fileMode = FileMode.Open;
		STGM_FLAGS sTGM_FLAGS = (STGM_FLAGS)(dwMode & 3);
		if (sTGM_FLAGS == STGM_FLAGS.READ || sTGM_FLAGS == STGM_FLAGS.READWRITE)
		{
			FileAccess fileAccess = FileAccess.Read;
			FileShare fileSharing = FileShare.ReadWrite;
			Invariant.Assert(_package == null || _encryptedPackage == null);
			ReleaseResources();
			_filter = null;
			_xpsFileName = null;
			bool flag = EncryptedPackageEnvelope.IsEncryptedPackageEnvelope(pszFileName);
			try
			{
				_packageStream = FileToStream(pszFileName, fileMode, fileAccess, fileSharing, 1048576L);
				if (flag)
				{
					_encryptedPackage = EncryptedPackageEnvelope.Open(_packageStream);
					_filter = new EncryptedPackageFilter(_encryptedPackage);
				}
				else
				{
					_package = Package.Open(_packageStream);
					_filter = new PackageFilter(_package);
				}
			}
			catch (IOException ex)
			{
				throw new COMException(ex.Message, -2147215613);
			}
			catch (FileFormatException ex2)
			{
				throw new COMException(ex2.Message, -2147215604);
			}
			finally
			{
				if (_filter == null)
				{
					ReleaseResources();
				}
			}
			_xpsFileName = pszFileName;
			return;
		}
		throw new ArgumentException(SR.FilterLoadInvalidModeFlag, "dwMode");
	}

	void IPersistFile.Save(string pszFileName, bool fRemember)
	{
		throw new COMException(SR.FilterIPersistFileIsReadOnly, -2147286781);
	}

	void IPersistFile.SaveCompleted(string pszFileName)
	{
	}

	void IPersistStream.GetClassID(out Guid pClassID)
	{
		pClassID = _filterClsid;
	}

	[PreserveSig]
	int IPersistStream.IsDirty()
	{
		return 1;
	}

	void IPersistStream.Load(IStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Invariant.Assert(_package == null || _encryptedPackage == null);
		ReleaseResources();
		_filter = null;
		_xpsFileName = null;
		try
		{
			_packageStream = new UnsafeIndexingFilterStream(stream);
			if (EncryptedPackageEnvelope.IsEncryptedPackageEnvelope(_packageStream))
			{
				_encryptedPackage = EncryptedPackageEnvelope.Open(_packageStream);
				_filter = new EncryptedPackageFilter(_encryptedPackage);
			}
			else
			{
				_package = Package.Open(_packageStream);
				_filter = new PackageFilter(_package);
			}
		}
		catch (IOException ex)
		{
			throw new COMException(ex.Message, -2147215613);
		}
		catch (Exception ex2)
		{
			throw new COMException(ex2.Message, -2147215604);
		}
		finally
		{
			if (_filter == null)
			{
				ReleaseResources();
			}
		}
	}

	void IPersistStream.Save(IStream stream, bool fClearDirty)
	{
		throw new COMException(SR.FilterIPersistStreamIsReadOnly, -2147286781);
	}

	void IPersistStream.GetSizeMax(out long pcbSize)
	{
		throw new NotSupportedException(SR.FilterIPersistFileIsReadOnly);
	}

	private void ReleaseResources()
	{
		if (_encryptedPackage != null)
		{
			_encryptedPackage.Close();
			_encryptedPackage = null;
		}
		else if (_package != null)
		{
			_package.Close();
			_package = null;
		}
		if (_packageStream != null)
		{
			_packageStream.Close();
			_packageStream = null;
		}
	}

	private static Stream FileToStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileSharing, long maxMemoryStream)
	{
		long length = new FileInfo(filePath).Length;
		Stream stream = new FileStream(filePath, fileMode, fileAccess, fileSharing);
		if (length < maxMemoryStream)
		{
			MemoryStream memoryStream = new MemoryStream((int)length);
			using (stream)
			{
				PackagingUtilities.CopyStream(stream, memoryStream, length, 4096);
			}
			stream = memoryStream;
		}
		return stream;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;

namespace MS.Internal.FontFace;

internal class TrueTypeFontDriver
{
	private struct DirectoryEntry
	{
		internal TrueTypeTags tag;

		internal CheckedPointer pointer;
	}

	private enum TrueTypeTags
	{
		CharToIndexMap = 1668112752,
		ControlValue = 1668707360,
		BitmapData = 1161970772,
		BitmapLocation = 1161972803,
		BitmapScale = 1161974595,
		Editor0 = 1701082160,
		Editor1 = 1701082161,
		Encryption = 1668446576,
		FontHeader = 1751474532,
		FontProgram = 1718642541,
		GridfitAndScanProc = 1734439792,
		GlyphDirectory = 1734633842,
		GlyphData = 1735162214,
		HoriDeviceMetrics = 1751412088,
		HoriHeader = 1751672161,
		HorizontalMetrics = 1752003704,
		IndexToLoc = 1819239265,
		Kerning = 1801810542,
		LinearThreshold = 1280594760,
		MaxProfile = 1835104368,
		NamingTable = 1851878757,
		OS_2 = 1330851634,
		Postscript = 1886352244,
		PreProgram = 1886545264,
		VertDeviceMetrics = 1447316824,
		VertHeader = 1986553185,
		VerticalMetrics = 1986884728,
		PCLT = 1346587732,
		TTO_GSUB = 1196643650,
		TTO_GPOS = 1196445523,
		TTO_GDEF = 1195656518,
		TTO_BASE = 1111577413,
		TTO_JSTF = 1246975046,
		OTTO = 1330926671,
		TTC_TTCF = 1953784678
	}

	private CheckedPointer _fileStream;

	private UnmanagedMemoryStream _unmanagedMemoryStream;

	private Uri _sourceUri;

	private int _numFaces;

	private FontTechnology _technology;

	private int _faceIndex;

	private int _directoryOffset;

	private DirectoryEntry[] _tableDirectory;

	internal int NumFaces => _numFaces;

	private Uri SourceUri => _sourceUri;

	private unsafe static ushort ReadOpenTypeUShort(CheckedPointer pointer)
	{
		byte* ptr = (byte*)pointer.Probe(0, 2);
		return (ushort)((*ptr << 8) + ptr[1]);
	}

	private unsafe static int ReadOpenTypeLong(CheckedPointer pointer)
	{
		byte* ptr = (byte*)pointer.Probe(0, 4);
		return (((*ptr << 8) + ptr[1] << 8) + ptr[2] << 8) + ptr[3];
	}

	internal TrueTypeFontDriver(UnmanagedMemoryStream unmanagedMemoryStream, Uri sourceUri)
	{
		_sourceUri = sourceUri;
		_unmanagedMemoryStream = unmanagedMemoryStream;
		_fileStream = new CheckedPointer(unmanagedMemoryStream);
		try
		{
			CheckedPointer fileStream = _fileStream;
			TrueTypeTags trueTypeTags = (TrueTypeTags)ReadOpenTypeLong(fileStream);
			fileStream += 4;
			switch (trueTypeTags)
			{
			case TrueTypeTags.TTC_TTCF:
				_technology = FontTechnology.TrueTypeCollection;
				fileStream += 4;
				_numFaces = ReadOpenTypeLong(fileStream);
				break;
			case TrueTypeTags.OTTO:
				_technology = FontTechnology.PostscriptOpenType;
				_numFaces = 1;
				break;
			default:
				_technology = FontTechnology.TrueType;
				_numFaces = 1;
				break;
			}
		}
		catch (ArgumentOutOfRangeException innerException)
		{
			throw new FileFormatException(SourceUri, innerException);
		}
	}

	internal void SetFace(int faceIndex)
	{
		if (_technology == FontTechnology.TrueTypeCollection)
		{
			if (faceIndex < 0 || faceIndex >= _numFaces)
			{
				throw new ArgumentOutOfRangeException("faceIndex");
			}
		}
		else if (faceIndex != 0)
		{
			throw new ArgumentOutOfRangeException("faceIndex", SR.FaceIndexValidOnlyForTTC);
		}
		try
		{
			CheckedPointer pointer = _fileStream + 4;
			if (_technology == FontTechnology.TrueTypeCollection)
			{
				pointer += 8 + 4 * faceIndex;
				_directoryOffset = ReadOpenTypeLong(pointer);
				pointer = _fileStream + (_directoryOffset + 4);
			}
			_faceIndex = faceIndex;
			int num = ReadOpenTypeUShort(pointer);
			pointer += 2;
			long num2 = 12 + num * 20;
			if (_fileStream.Size < num2)
			{
				throw new FileFormatException(SourceUri);
			}
			_tableDirectory = new DirectoryEntry[num];
			pointer += 6;
			for (int i = 0; i < _tableDirectory.Length; i++)
			{
				_tableDirectory[i].tag = (TrueTypeTags)ReadOpenTypeLong(pointer);
				pointer += 8;
				int offset = ReadOpenTypeLong(pointer);
				pointer += 4;
				int length = ReadOpenTypeLong(pointer);
				pointer += 4;
				_tableDirectory[i].pointer = _fileStream.CheckedProbe(offset, length);
			}
		}
		catch (ArgumentOutOfRangeException innerException)
		{
			throw new FileFormatException(SourceUri, innerException);
		}
	}

	internal unsafe byte[] ComputeFontSubset(ICollection<ushort> glyphs)
	{
		int size = _fileStream.Size;
		void* ptr = _fileStream.Probe(0, size);
		if (_technology == FontTechnology.PostscriptOpenType)
		{
			byte[] array = new byte[size];
			Marshal.Copy((nint)ptr, array, 0, size);
			return array;
		}
		ushort[] array2;
		if (glyphs == null || glyphs.Count == 0)
		{
			array2 = null;
		}
		else
		{
			array2 = new ushort[glyphs.Count];
			glyphs.CopyTo(array2, 0);
		}
		return TrueTypeSubsetter.ComputeSubset(ptr, size, SourceUri, _directoryOffset, array2);
	}
}

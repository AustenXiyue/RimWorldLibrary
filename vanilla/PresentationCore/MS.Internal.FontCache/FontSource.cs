using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using MS.Internal.IO.Packaging;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal class FontSource : IFontSource
{
	private class PinnedByteArrayStream : UnmanagedMemoryStream
	{
		private GCHandle _memoryHandle;

		internal unsafe PinnedByteArrayStream(byte[] bits)
		{
			_memoryHandle = GCHandle.Alloc(bits, GCHandleType.Pinned);
			Initialize((byte*)_memoryHandle.AddrOfPinnedObject(), bits.Length, bits.Length, FileAccess.Read);
		}

		~PinnedByteArrayStream()
		{
			Dispose(disposing: false);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_memoryHandle.Free();
		}
	}

	private bool _isComposite;

	private bool _isInternalCompositeFont;

	private Uri _fontUri;

	private bool _skipDemand;

	private static SizeLimitedCache<Uri, byte[]> _resourceCache = new SizeLimitedCache<Uri, byte[]>(10);

	private const int MaximumCacheItems = 10;

	private const string ObfuscatedContentType = "application/vnd.ms-package.obfuscated-opentype";

	public bool IsFile
	{
		get
		{
			if (!_isInternalCompositeFont)
			{
				return _fontUri.IsFile;
			}
			return false;
		}
	}

	public bool IsComposite => _isComposite;

	public Uri Uri => _fontUri;

	public bool IsAppSpecific => Util.IsAppSpecificUri(_fontUri);

	public FontSource(Uri fontUri, bool skipDemand)
	{
		Initialize(fontUri, skipDemand, isComposite: false, isInternalCompositeFont: false);
	}

	public FontSource(Uri fontUri, bool skipDemand, bool isComposite)
	{
		Initialize(fontUri, skipDemand, isComposite, isInternalCompositeFont: false);
	}

	public FontSource(Uri fontUri, bool skipDemand, bool isComposite, bool isInternalCompositeFont)
	{
		Initialize(fontUri, skipDemand, isComposite, isInternalCompositeFont);
	}

	private void Initialize(Uri fontUri, bool skipDemand, bool isComposite, bool isInternalCompositeFont)
	{
		_fontUri = fontUri;
		_skipDemand = skipDemand;
		_isComposite = isComposite;
		_isInternalCompositeFont = isInternalCompositeFont;
		Invariant.Assert(_isInternalCompositeFont || _fontUri.IsAbsoluteUri);
	}

	public string GetUriString()
	{
		return _fontUri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
	}

	public string ToStringUpperInvariant()
	{
		return GetUriString().ToUpperInvariant();
	}

	public override int GetHashCode()
	{
		return HashFn.HashString(ToStringUpperInvariant(), 0);
	}

	internal long SkipLastWriteTime()
	{
		return -1L;
	}

	public DateTime GetLastWriteTimeUtc()
	{
		if (IsFile)
		{
			return Directory.GetLastWriteTimeUtc(_fontUri.LocalPath);
		}
		return DateTime.MaxValue;
	}

	public UnmanagedMemoryStream GetUnmanagedStream()
	{
		if (IsFile)
		{
			FileMapping fileMapping = new FileMapping();
			fileMapping.OpenFile(_fontUri.LocalPath);
			return fileMapping;
		}
		byte[] array;
		lock (_resourceCache)
		{
			array = _resourceCache.Get(_fontUri);
		}
		if (array == null)
		{
			Stream stream;
			if (_isInternalCompositeFont)
			{
				stream = GetCompositeFontResourceStream();
			}
			else
			{
				WebResponse webResponse = WpfWebRequestHelper.CreateRequestAndGetResponse(_fontUri);
				stream = webResponse.GetResponseStream();
				if (string.Equals(webResponse.ContentType, "application/vnd.ms-package.obfuscated-opentype", StringComparison.Ordinal))
				{
					stream = new DeobfuscatingStream(stream, _fontUri, leaveOpen: false);
				}
			}
			if (stream is UnmanagedMemoryStream result)
			{
				return result;
			}
			array = StreamToByteArray(stream);
			stream?.Close();
		}
		lock (_resourceCache)
		{
			_resourceCache.Add(_fontUri, array, isPermanent: false);
		}
		return ByteArrayToUnmanagedStream(array);
	}

	public void TestFileOpenable()
	{
		if (IsFile)
		{
			FileMapping fileMapping = new FileMapping();
			fileMapping.OpenFile(_fontUri.LocalPath);
			fileMapping.Close();
		}
	}

	public Stream GetStream()
	{
		if (IsFile)
		{
			FileMapping fileMapping = new FileMapping();
			fileMapping.OpenFile(_fontUri.LocalPath);
			return fileMapping;
		}
		byte[] array;
		lock (_resourceCache)
		{
			array = _resourceCache.Get(_fontUri);
		}
		if (array != null)
		{
			return new MemoryStream(array);
		}
		Stream stream;
		if (_isInternalCompositeFont)
		{
			stream = GetCompositeFontResourceStream();
		}
		else
		{
			WebResponse response = PackWebRequestFactory.CreateWebRequest(_fontUri).GetResponse();
			stream = response.GetResponseStream();
			if (string.Equals(response.ContentType, "application/vnd.ms-package.obfuscated-opentype", StringComparison.Ordinal))
			{
				stream = new DeobfuscatingStream(stream, _fontUri, leaveOpen: false);
			}
		}
		return stream;
	}

	private static UnmanagedMemoryStream ByteArrayToUnmanagedStream(byte[] bits)
	{
		return new PinnedByteArrayStream(bits);
	}

	private static byte[] StreamToByteArray(Stream fontStream)
	{
		byte[] array;
		if (fontStream.CanSeek)
		{
			checked
			{
				array = new byte[(int)fontStream.Length];
				PackagingUtilities.ReliableRead(fontStream, array, 0, (int)fontStream.Length);
			}
		}
		else
		{
			int num = 1048576;
			byte[] array2 = new byte[num];
			int num2 = 0;
			while (true)
			{
				int num3 = num - num2;
				if (num3 < num / 3)
				{
					num *= 2;
					byte[] array3 = new byte[num];
					Array.Copy(array2, array3, num2);
					array2 = array3;
					num3 = num - num2;
				}
				int num4 = fontStream.Read(array2, num2, num3);
				if (num4 == 0)
				{
					break;
				}
				num2 += num4;
			}
			if (num2 == num)
			{
				array = array2;
			}
			else
			{
				array = new byte[num2];
				Array.Copy(array2, array, num2);
			}
		}
		return array;
	}

	private Stream GetCompositeFontResourceStream()
	{
		string text = _fontUri.OriginalString.Substring(_fontUri.OriginalString.LastIndexOf('/') + 1).ToLowerInvariant();
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		return new ResourceManager(executingAssembly.GetName().Name + ".g", executingAssembly)?.GetStream("fonts/" + text);
	}
}

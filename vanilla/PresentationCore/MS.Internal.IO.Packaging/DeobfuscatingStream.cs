using System;
using System.IO;
using System.IO.Packaging;
using MS.Internal.PresentationCore;

namespace MS.Internal.IO.Packaging;

internal class DeobfuscatingStream : Stream
{
	private Stream _obfuscatedStream;

	private byte[] _guid;

	private bool _ownObfuscatedStream;

	private const long ObfuscatedLength = 32L;

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _obfuscatedStream.Position;
		}
		set
		{
			CheckDisposed();
			_obfuscatedStream.Position = value;
		}
	}

	public override long Length => _obfuscatedStream.Length;

	public override bool CanRead
	{
		get
		{
			if (_obfuscatedStream != null)
			{
				return _obfuscatedStream.CanRead;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (_obfuscatedStream != null)
			{
				return _obfuscatedStream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite => false;

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		long position = _obfuscatedStream.Position;
		int num = _obfuscatedStream.Read(buffer, offset, count);
		Deobfuscate(buffer, offset, num, position);
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		throw new NotSupportedException(SR.WriteNotSupported);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		return _obfuscatedStream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		throw new NotSupportedException(SR.SetLengthNotSupported);
	}

	public override void Flush()
	{
		CheckDisposed();
		_obfuscatedStream.Flush();
	}

	internal DeobfuscatingStream(Stream obfuscatedStream, Uri streamUri, bool leaveOpen)
	{
		if (obfuscatedStream == null)
		{
			throw new ArgumentNullException("obfuscatedStream");
		}
		if (System.IO.Packaging.PackUriHelper.GetPartUri(streamUri) == null)
		{
			throw new InvalidOperationException(SR.InvalidPartName);
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(streamUri.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.UriEscaped));
		_guid = GetGuidByteArray(fileNameWithoutExtension);
		_obfuscatedStream = obfuscatedStream;
		_ownObfuscatedStream = !leaveOpen;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				if (_obfuscatedStream != null && _ownObfuscatedStream)
				{
					_obfuscatedStream.Close();
				}
				_obfuscatedStream = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void CheckDisposed()
	{
		if (_obfuscatedStream == null)
		{
			throw new ObjectDisposedException(null, SR.Media_StreamClosed);
		}
	}

	private void Deobfuscate(byte[] buffer, int offset, int count, long readPosition)
	{
		if (readPosition >= 32 || count <= 0)
		{
			return;
		}
		int num = (int)(Math.Min(32L, readPosition + count) - readPosition);
		int num2 = _guid.Length - (int)readPosition % _guid.Length - 1;
		int num3 = offset;
		while (num > 0)
		{
			if (num2 < 0)
			{
				num2 = _guid.Length - 1;
			}
			buffer[num3] ^= _guid[num2];
			num--;
			num3++;
			num2--;
		}
	}

	private static byte[] GetGuidByteArray(string guidString)
	{
		if (!guidString.Contains('-'))
		{
			throw new ArgumentException(SR.InvalidPartName);
		}
		string text = new Guid(guidString).ToString("N");
		byte[] array = new byte[16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
		}
		return array;
	}
}

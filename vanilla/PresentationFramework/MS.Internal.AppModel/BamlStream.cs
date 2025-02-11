using System;
using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace MS.Internal.AppModel;

internal class BamlStream : Stream, IStreamInfo
{
	private MS.Internal.SecurityCriticalDataForSet<Assembly> _assembly;

	private Stream _stream;

	Assembly IStreamInfo.Assembly => _assembly.Value;

	public override bool CanRead => _stream.CanRead;

	public override bool CanSeek => _stream.CanSeek;

	public override bool CanWrite => _stream.CanWrite;

	public override long Length => _stream.Length;

	public override long Position
	{
		get
		{
			return _stream.Position;
		}
		set
		{
			_stream.Position = value;
		}
	}

	internal BamlStream(Stream stream, Assembly assembly)
	{
		_assembly.Value = assembly;
		_stream = stream;
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return _stream.BeginRead(buffer, offset, count, callback, state);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return _stream.BeginWrite(buffer, offset, count, callback, state);
	}

	public override void Close()
	{
		_stream.Close();
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		return _stream.EndRead(asyncResult);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		_stream.EndWrite(asyncResult);
	}

	public override bool Equals(object obj)
	{
		return _stream.Equals(obj);
	}

	public override void Flush()
	{
		_stream.Flush();
	}

	public override int GetHashCode()
	{
		return _stream.GetHashCode();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _stream.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		return _stream.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_stream.SetLength(value);
	}

	public override string ToString()
	{
		return _stream.ToString();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte value)
	{
		_stream.WriteByte(value);
	}
}

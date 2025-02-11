using System;
using System.Collections;
using System.IO;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class StreamWithDictionary : Stream, IDictionary, ICollection, IEnumerable
{
	private Stream baseStream;

	private IDictionary baseDictionary;

	private bool _disposed;

	public override bool CanRead
	{
		get
		{
			if (!_disposed)
			{
				return baseStream.CanRead;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (!_disposed)
			{
				return baseStream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (!_disposed)
			{
				return baseStream.CanWrite;
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			return baseStream.Length;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposed();
			return baseStream.Position;
		}
		set
		{
			CheckDisposed();
			baseStream.Position = value;
		}
	}

	object IDictionary.this[object index]
	{
		get
		{
			CheckDisposed();
			return baseDictionary[index];
		}
		set
		{
			CheckDisposed();
			baseDictionary[index] = value;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			CheckDisposed();
			return baseDictionary.Keys;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			CheckDisposed();
			return baseDictionary.Values;
		}
	}

	bool IDictionary.IsReadOnly
	{
		get
		{
			CheckDisposed();
			return baseDictionary.IsReadOnly;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			CheckDisposed();
			return baseDictionary.IsFixedSize;
		}
	}

	int ICollection.Count
	{
		get
		{
			CheckDisposed();
			return baseDictionary.Count;
		}
	}

	object ICollection.SyncRoot
	{
		get
		{
			CheckDisposed();
			return baseDictionary.SyncRoot;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			CheckDisposed();
			return baseDictionary.IsSynchronized;
		}
	}

	internal bool Disposed => _disposed;

	internal StreamWithDictionary(Stream wrappedStream, IDictionary wrappedDictionary)
	{
		baseStream = wrappedStream;
		baseDictionary = wrappedDictionary;
	}

	public override void Flush()
	{
		CheckDisposed();
		baseStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		return baseStream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		baseStream.SetLength(newLength);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		return baseStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		baseStream.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				baseStream.Close();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	bool IDictionary.Contains(object key)
	{
		CheckDisposed();
		return baseDictionary.Contains(key);
	}

	void IDictionary.Add(object key, object val)
	{
		CheckDisposed();
		baseDictionary.Add(key, val);
	}

	void IDictionary.Clear()
	{
		CheckDisposed();
		baseDictionary.Clear();
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		CheckDisposed();
		return baseDictionary.GetEnumerator();
	}

	void IDictionary.Remove(object key)
	{
		CheckDisposed();
		baseDictionary.Remove(key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		CheckDisposed();
		baseDictionary.CopyTo(array, index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		CheckDisposed();
		return ((IEnumerable)baseDictionary).GetEnumerator();
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("Stream");
		}
	}
}

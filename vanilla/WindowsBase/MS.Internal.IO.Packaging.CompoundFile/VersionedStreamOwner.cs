using System;
using System.Globalization;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class VersionedStreamOwner : VersionedStream
{
	private bool _writeOccurred;

	private bool _readOccurred;

	private FormatVersion _codeVersion;

	private FormatVersion _fileVersion;

	private long _dataOffset;

	public override long Position
	{
		get
		{
			ReadAttempt();
			return checked(base.BaseStream.Position - _dataOffset);
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public override long Length
	{
		get
		{
			ReadAttempt();
			long num = checked(base.BaseStream.Length - _dataOffset);
			Invariant.Assert(num >= 0);
			return num;
		}
	}

	public override bool CanRead
	{
		get
		{
			if (base.BaseStream != null && base.BaseStream.CanRead)
			{
				return IsReadable;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (base.BaseStream != null && base.BaseStream.CanSeek)
			{
				return IsReadable;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (base.BaseStream != null && base.BaseStream.CanWrite)
			{
				return IsUpdatable;
			}
			return false;
		}
	}

	internal bool IsUpdatable
	{
		get
		{
			CheckDisposed();
			EnsureParsed();
			if (_fileVersion != null)
			{
				return _fileVersion.IsUpdatableBy(_codeVersion.UpdaterVersion);
			}
			return true;
		}
	}

	internal bool IsReadable
	{
		get
		{
			CheckDisposed();
			EnsureParsed();
			if (_fileVersion != null)
			{
				return _fileVersion.IsReadableBy(_codeVersion.ReaderVersion);
			}
			return true;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ReadAttempt(throwIfEmpty: true);
		return base.BaseStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		WriteAttempt();
		base.BaseStream.Write(buffer, offset, count);
	}

	public override int ReadByte()
	{
		ReadAttempt(throwIfEmpty: true);
		return base.BaseStream.ReadByte();
	}

	public override void WriteByte(byte b)
	{
		WriteAttempt();
		base.BaseStream.WriteByte(b);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		ReadAttempt();
		long num = -1L;
		checked
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				num = offset;
				break;
			case SeekOrigin.Current:
				num = Position + offset;
				break;
			case SeekOrigin.End:
				num = Length + offset;
				break;
			}
			if (num < 0)
			{
				throw new ArgumentException(SR.SeekNegative);
			}
			base.BaseStream.Position = num + _dataOffset;
			return num;
		}
	}

	public override void SetLength(long newLength)
	{
		if (newLength < 0)
		{
			throw new ArgumentOutOfRangeException("newLength");
		}
		WriteAttempt();
		base.BaseStream.SetLength(checked(newLength + _dataOffset));
	}

	public override void Flush()
	{
		CheckDisposed();
		base.BaseStream.Flush();
	}

	internal VersionedStreamOwner(Stream baseStream, FormatVersion codeVersion)
		: base(baseStream)
	{
		_codeVersion = codeVersion;
	}

	internal void WriteAttempt()
	{
		CheckDisposed();
		if (_writeOccurred)
		{
			return;
		}
		EnsureParsed();
		if (_fileVersion == null)
		{
			PersistVersion(_codeVersion);
		}
		else
		{
			if (!_fileVersion.IsUpdatableBy(_codeVersion.UpdaterVersion))
			{
				throw new FileFormatException(SR.Format(SR.UpdaterVersionError, _fileVersion.UpdaterVersion, _codeVersion));
			}
			if (_codeVersion.UpdaterVersion != _fileVersion.UpdaterVersion)
			{
				_fileVersion.UpdaterVersion = _codeVersion.UpdaterVersion;
				PersistVersion(_fileVersion);
			}
		}
		_writeOccurred = true;
	}

	internal void ReadAttempt()
	{
		ReadAttempt(throwIfEmpty: false);
	}

	internal void ReadAttempt(bool throwIfEmpty)
	{
		CheckDisposed();
		if (_readOccurred)
		{
			return;
		}
		EnsureParsed();
		if (throwIfEmpty || base.BaseStream.Length > 0)
		{
			if (_fileVersion == null)
			{
				throw new FileFormatException(SR.VersionStreamMissing);
			}
			if (!_fileVersion.IsReadableBy(_codeVersion.ReaderVersion))
			{
				throw new FileFormatException(SR.Format(SR.ReaderVersionError, _fileVersion.ReaderVersion, _codeVersion));
			}
		}
		_readOccurred = true;
	}

	private void PersistVersion(FormatVersion version)
	{
		if (!base.BaseStream.CanWrite)
		{
			throw new NotSupportedException(SR.WriteNotSupported);
		}
		checked
		{
			long num = base.BaseStream.Position - _dataOffset;
			base.BaseStream.Seek(0L, SeekOrigin.Begin);
			long num2 = version.SaveToStream(base.BaseStream);
			_fileVersion = version;
			if (_dataOffset != 0L && num2 != _dataOffset)
			{
				throw new FileFormatException(SR.VersionUpdateFailure);
			}
			_dataOffset = num2;
			base.BaseStream.Position = num + _dataOffset;
		}
	}

	private void EnsureParsed()
	{
		if (_fileVersion == null && base.BaseStream.Length > 0)
		{
			if (!base.BaseStream.CanRead)
			{
				throw new NotSupportedException(SR.ReadNotSupported);
			}
			base.BaseStream.Seek(0L, SeekOrigin.Begin);
			_fileVersion = FormatVersion.LoadFromStream(base.BaseStream);
			if (string.CompareOrdinal(_fileVersion.FeatureIdentifier.ToUpper(CultureInfo.InvariantCulture), _codeVersion.FeatureIdentifier.ToUpper(CultureInfo.InvariantCulture)) != 0)
			{
				throw new FileFormatException(SR.Format(SR.InvalidTransformFeatureName, _fileVersion.FeatureIdentifier, _codeVersion.FeatureIdentifier));
			}
			_dataOffset = base.BaseStream.Position;
		}
	}
}

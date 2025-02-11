using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal static class SafeNativeCompoundFileMethods
{
	private class SafeIStorageImplementation : IStorage, IPropertySetStorage, IDisposable
	{
		private UnsafeNativeCompoundFileMethods.UnsafeNativeIPropertySetStorage _unsafePropertySetStorage;

		private UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage _unsafeStorage;

		private UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream _unsafeLockByteStream;

		internal SafeIStorageImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage storage)
			: this(storage, null)
		{
		}

		internal SafeIStorageImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage storage, UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream lockBytesStream)
		{
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}
			_unsafeStorage = storage;
			_unsafePropertySetStorage = (UnsafeNativeCompoundFileMethods.UnsafeNativeIPropertySetStorage)_unsafeStorage;
			_unsafeLockByteStream = lockBytesStream;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _unsafeStorage != null)
				{
					UnsafeNativeMethods.SafeReleaseComObject(_unsafeStorage);
					if (_unsafeLockByteStream != null)
					{
						_unsafeLockByteStream.Dispose();
					}
				}
			}
			finally
			{
				_unsafeStorage = null;
				_unsafePropertySetStorage = null;
				_unsafeLockByteStream = null;
			}
		}

		int IStorage.CreateStream(string pwcsName, int grfMode, int reserved1, int reserved2, out IStream ppstm)
		{
			UnsafeNativeCompoundFileMethods.UnsafeNativeIStream ppstm2;
			int num = _unsafeStorage.CreateStream(pwcsName, grfMode, reserved1, reserved2, out ppstm2);
			if (num == 0)
			{
				ppstm = new SafeIStreamImplementation(ppstm2);
				return num;
			}
			ppstm = null;
			return num;
		}

		int IStorage.OpenStream(string pwcsName, int reserved1, int grfMode, int reserved2, out IStream ppstm)
		{
			UnsafeNativeCompoundFileMethods.UnsafeNativeIStream ppstm2;
			int num = _unsafeStorage.OpenStream(pwcsName, reserved1, grfMode, reserved2, out ppstm2);
			if (num == 0)
			{
				ppstm = new SafeIStreamImplementation(ppstm2);
				return num;
			}
			ppstm = null;
			return num;
		}

		int IStorage.CreateStorage(string pwcsName, int grfMode, int reserved1, int reserved2, out IStorage ppstg)
		{
			UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppstg2;
			int num = _unsafeStorage.CreateStorage(pwcsName, grfMode, reserved1, reserved2, out ppstg2);
			if (num == 0)
			{
				ppstg = new SafeIStorageImplementation(ppstg2);
				return num;
			}
			ppstg = null;
			return num;
		}

		int IStorage.OpenStorage(string pwcsName, IStorage pstgPriority, int grfMode, nint snbExclude, int reserved, out IStorage ppstg)
		{
			UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppstg2;
			int num = _unsafeStorage.OpenStorage(pwcsName, (pstgPriority == null) ? null : ((SafeIStorageImplementation)pstgPriority)._unsafeStorage, grfMode, snbExclude, reserved, out ppstg2);
			if (num == 0)
			{
				ppstg = new SafeIStorageImplementation(ppstg2);
				return num;
			}
			ppstg = null;
			return num;
		}

		void IStorage.CopyTo(int ciidExclude, Guid[] rgiidExclude, nint snbExclude, IStorage ppstg)
		{
			Invariant.Assert(ppstg != null, "ppstg cannot be null");
			_unsafeStorage.CopyTo(ciidExclude, rgiidExclude, snbExclude, ((SafeIStorageImplementation)ppstg)._unsafeStorage);
		}

		void IStorage.MoveElementTo(string pwcsName, IStorage pstgDest, string pwcsNewName, int grfFlags)
		{
			Invariant.Assert(pstgDest != null, "pstgDest cannot be null");
			_unsafeStorage.MoveElementTo(pwcsName, ((SafeIStorageImplementation)pstgDest)._unsafeStorage, pwcsNewName, grfFlags);
		}

		void IStorage.Commit(int grfCommitFlags)
		{
			_unsafeStorage.Commit(grfCommitFlags);
		}

		void IStorage.Revert()
		{
			_unsafeStorage.Revert();
		}

		void IStorage.EnumElements(int reserved1, nint reserved2, int reserved3, out IEnumSTATSTG ppEnum)
		{
			_unsafeStorage.EnumElements(reserved1, reserved2, reserved3, out var ppEnum2);
			if (ppEnum2 != null)
			{
				ppEnum = new SafeIEnumSTATSTGImplementation(ppEnum2);
			}
			else
			{
				ppEnum = null;
			}
		}

		void IStorage.DestroyElement(string pwcsName)
		{
			_unsafeStorage.DestroyElement(pwcsName);
		}

		void IStorage.RenameElement(string pwcsOldName, string pwcsNewName)
		{
			_unsafeStorage.RenameElement(pwcsOldName, pwcsNewName);
		}

		void IStorage.SetElementTimes(string pwcsName, FILETIME pctime, FILETIME patime, FILETIME pmtime)
		{
			_unsafeStorage.SetElementTimes(pwcsName, pctime, patime, pmtime);
		}

		void IStorage.SetClass(ref Guid clsid)
		{
			_unsafeStorage.SetClass(ref clsid);
		}

		void IStorage.SetStateBits(int grfStateBits, int grfMask)
		{
			_unsafeStorage.SetStateBits(grfStateBits, grfMask);
		}

		void IStorage.Stat(out STATSTG pstatstg, int grfStatFlag)
		{
			_unsafeStorage.Stat(out pstatstg, grfStatFlag);
		}

		void IPropertySetStorage.Create(ref Guid rfmtid, ref Guid pclsid, uint grfFlags, uint grfMode, out IPropertyStorage ppprstg)
		{
			_unsafePropertySetStorage.Create(ref rfmtid, ref pclsid, grfFlags, grfMode, out var ppprstg2);
			if (ppprstg2 != null)
			{
				ppprstg = new SafeIPropertyStorageImplementation(ppprstg2);
			}
			else
			{
				ppprstg = null;
			}
		}

		int IPropertySetStorage.Open(ref Guid rfmtid, uint grfMode, out IPropertyStorage ppprstg)
		{
			UnsafeNativeCompoundFileMethods.UnsafeNativeIPropertyStorage ppprstg2;
			int result = _unsafePropertySetStorage.Open(ref rfmtid, grfMode, out ppprstg2);
			if (ppprstg2 != null)
			{
				ppprstg = new SafeIPropertyStorageImplementation(ppprstg2);
				return result;
			}
			ppprstg = null;
			return result;
		}

		void IPropertySetStorage.Delete(ref Guid rfmtid)
		{
			_unsafePropertySetStorage.Delete(ref rfmtid);
		}

		void IPropertySetStorage.Enum(out IEnumSTATPROPSETSTG ppenum)
		{
			_unsafePropertySetStorage.Enum(out var ppenum2);
			if (ppenum2 != null)
			{
				ppenum = new SafeIEnumSTATPROPSETSTGImplementation(ppenum2);
			}
			else
			{
				ppenum = null;
			}
		}
	}

	private class SafeIStreamImplementation : IStream, IDisposable
	{
		private UnsafeNativeCompoundFileMethods.UnsafeNativeIStream _unsafeStream;

		internal SafeIStreamImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIStream stream)
		{
			_unsafeStream = stream;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _unsafeStream != null)
				{
					UnsafeNativeMethods.SafeReleaseComObject(_unsafeStream);
				}
			}
			finally
			{
				_unsafeStream = null;
			}
		}

		void IStream.Read(byte[] pv, int cb, out int pcbRead)
		{
			if (cb < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "cb", cb.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.Read(pv, cb, out pcbRead);
		}

		void IStream.Write(byte[] pv, int cb, out int pcbWritten)
		{
			if (cb < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "cb", cb.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.Write(pv, cb, out pcbWritten);
		}

		void IStream.Seek(long dlibMove, int dwOrigin, out long plibNewPosition)
		{
			if (dwOrigin < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "dwOrigin", dwOrigin.ToString(CultureInfo.InvariantCulture)));
			}
			if (dlibMove < 0 && dwOrigin == 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "dlibMove", dlibMove.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.Seek(dlibMove, dwOrigin, out plibNewPosition);
		}

		void IStream.SetSize(long libNewSize)
		{
			if (libNewSize < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "libNewSize", libNewSize.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.SetSize(libNewSize);
		}

		void IStream.CopyTo(IStream pstm, long cb, out long pcbRead, out long pcbWritten)
		{
			Invariant.Assert(pstm != null, "pstm cannot be null");
			if (cb < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "cb", cb.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.CopyTo(((SafeIStreamImplementation)pstm)._unsafeStream, cb, out pcbRead, out pcbWritten);
		}

		void IStream.Commit(int grfCommitFlags)
		{
			_unsafeStream.Commit(grfCommitFlags);
		}

		void IStream.Revert()
		{
			_unsafeStream.Revert();
		}

		void IStream.LockRegion(long libOffset, long cb, int dwLockType)
		{
			if (libOffset < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "libOffset", libOffset.ToString(CultureInfo.InvariantCulture)));
			}
			if (cb < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "cb", cb.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.LockRegion(libOffset, cb, dwLockType);
		}

		void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
		{
			if (libOffset < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "libOffset", libOffset.ToString(CultureInfo.InvariantCulture)));
			}
			if (cb < 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "cb", cb.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeStream.UnlockRegion(libOffset, cb, dwLockType);
		}

		void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
		{
			_unsafeStream.Stat(out pstatstg, grfStatFlag);
		}

		void IStream.Clone(out IStream ppstm)
		{
			_unsafeStream.Clone(out var ppstm2);
			if (ppstm2 != null)
			{
				ppstm = new SafeIStreamImplementation(ppstm2);
			}
			else
			{
				ppstm = null;
			}
		}
	}

	private class SafeIEnumSTATPROPSETSTGImplementation : IEnumSTATPROPSETSTG, IDisposable
	{
		private UnsafeNativeCompoundFileMethods.UnsafeNativeIEnumSTATPROPSETSTG _unsafeEnumSTATPROPSETSTG;

		internal SafeIEnumSTATPROPSETSTGImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIEnumSTATPROPSETSTG enumSTATPROPSETSTG)
		{
			_unsafeEnumSTATPROPSETSTG = enumSTATPROPSETSTG;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _unsafeEnumSTATPROPSETSTG != null)
				{
					UnsafeNativeMethods.SafeReleaseComObject(_unsafeEnumSTATPROPSETSTG);
				}
			}
			finally
			{
				_unsafeEnumSTATPROPSETSTG = null;
			}
		}

		int IEnumSTATPROPSETSTG.Next(uint celt, STATPROPSETSTG rgelt, out uint pceltFetched)
		{
			return _unsafeEnumSTATPROPSETSTG.Next(celt, rgelt, out pceltFetched);
		}

		void IEnumSTATPROPSETSTG.Skip(uint celt)
		{
			_unsafeEnumSTATPROPSETSTG.Skip(celt);
		}

		void IEnumSTATPROPSETSTG.Reset()
		{
			_unsafeEnumSTATPROPSETSTG.Reset();
		}

		void IEnumSTATPROPSETSTG.Clone(out IEnumSTATPROPSETSTG ppenum)
		{
			_unsafeEnumSTATPROPSETSTG.Clone(out var ppenum2);
			if (ppenum2 != null)
			{
				ppenum = new SafeIEnumSTATPROPSETSTGImplementation(ppenum2);
			}
			else
			{
				ppenum = null;
			}
		}
	}

	private class SafeIPropertyStorageImplementation : IPropertyStorage, IDisposable
	{
		private UnsafeNativeCompoundFileMethods.UnsafeNativeIPropertyStorage _unsafePropertyStorage;

		internal SafeIPropertyStorageImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIPropertyStorage propertyStorage)
		{
			_unsafePropertyStorage = propertyStorage;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _unsafePropertyStorage != null)
				{
					UnsafeNativeMethods.SafeReleaseComObject(_unsafePropertyStorage);
				}
			}
			finally
			{
				_unsafePropertyStorage = null;
			}
		}

		int IPropertyStorage.ReadMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar)
		{
			return _unsafePropertyStorage.ReadMultiple(cpspec, rgpspec, rgpropvar);
		}

		void IPropertyStorage.WriteMultiple(uint cpspec, PROPSPEC[] rgpspec, PROPVARIANT[] rgpropvar, uint propidNameFirst)
		{
			_unsafePropertyStorage.WriteMultiple(cpspec, rgpspec, rgpropvar, propidNameFirst);
		}

		void IPropertyStorage.DeleteMultiple(uint cpspec, PROPSPEC[] rgpspec)
		{
			_unsafePropertyStorage.DeleteMultiple(cpspec, rgpspec);
		}

		void IPropertyStorage.ReadPropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName)
		{
			_unsafePropertyStorage.ReadPropertyNames(cpropid, rgpropid, rglpwstrName);
		}

		void IPropertyStorage.WritePropertyNames(uint cpropid, uint[] rgpropid, string[] rglpwstrName)
		{
			_unsafePropertyStorage.WritePropertyNames(cpropid, rgpropid, rglpwstrName);
		}

		void IPropertyStorage.DeletePropertyNames(uint cpropid, uint[] rgpropid)
		{
			_unsafePropertyStorage.DeletePropertyNames(cpropid, rgpropid);
		}

		void IPropertyStorage.Commit(uint grfCommitFlags)
		{
			_unsafePropertyStorage.Commit(grfCommitFlags);
		}

		void IPropertyStorage.Revert()
		{
			_unsafePropertyStorage.Revert();
		}

		void IPropertyStorage.Enum(out IEnumSTATPROPSTG ppenum)
		{
			ppenum = null;
		}

		void IPropertyStorage.SetTimes(ref FILETIME pctime, ref FILETIME patime, ref FILETIME pmtime)
		{
			_unsafePropertyStorage.SetTimes(ref pctime, ref patime, ref pmtime);
		}

		void IPropertyStorage.SetClass(ref Guid clsid)
		{
			_unsafePropertyStorage.SetClass(ref clsid);
		}

		void IPropertyStorage.Stat(out STATPROPSETSTG pstatpsstg)
		{
			_unsafePropertyStorage.Stat(out pstatpsstg);
		}
	}

	private class SafeIEnumSTATSTGImplementation : IEnumSTATSTG, IDisposable
	{
		private UnsafeNativeCompoundFileMethods.UnsafeNativeIEnumSTATSTG _unsafeEnumSTATSTG;

		internal SafeIEnumSTATSTGImplementation(UnsafeNativeCompoundFileMethods.UnsafeNativeIEnumSTATSTG enumSTATSTG)
		{
			_unsafeEnumSTATSTG = enumSTATSTG;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing && _unsafeEnumSTATSTG != null)
				{
					UnsafeNativeMethods.SafeReleaseComObject(_unsafeEnumSTATSTG);
				}
			}
			finally
			{
				_unsafeEnumSTATSTG = null;
			}
		}

		void IEnumSTATSTG.Next(uint celt, out STATSTG rgelt, out uint pceltFetched)
		{
			if (celt != 1)
			{
				throw new ArgumentException(SR.Format(SR.InvalidArgumentValue, "celt", celt.ToString(CultureInfo.InvariantCulture)));
			}
			_unsafeEnumSTATSTG.Next(celt, out rgelt, out pceltFetched);
		}

		void IEnumSTATSTG.Skip(uint celt)
		{
			_unsafeEnumSTATSTG.Skip(celt);
		}

		void IEnumSTATSTG.Reset()
		{
			_unsafeEnumSTATSTG.Reset();
		}

		void IEnumSTATSTG.Clone(out IEnumSTATSTG ppenum)
		{
			_unsafeEnumSTATSTG.Clone(out var ppenum2);
			if (ppenum2 != null)
			{
				ppenum = new SafeIEnumSTATSTGImplementation(ppenum2);
			}
			else
			{
				ppenum = null;
			}
		}
	}

	internal static void UpdateModeFlagFromFileAccess(FileAccess access, ref int grfMode)
	{
		if (FileAccess.Write == access)
		{
			throw new NotSupportedException(SR.WriteOnlyUnsupported);
		}
		if (FileAccess.ReadWrite == (access & FileAccess.ReadWrite) || FileAccess.ReadWrite == (access & FileAccess.ReadWrite))
		{
			grfMode |= 2;
		}
		else if (FileAccess.Write == (access & FileAccess.Write))
		{
			grfMode |= 1;
		}
		else if (FileAccess.Read != (access & FileAccess.Read))
		{
			throw new ArgumentException(SR.FileAccessInvalid);
		}
	}

	internal static int SafeStgCreateDocfileOnStream(Stream s, int grfMode, out IStorage ppstgOpen)
	{
		Invariant.Assert(s != null, "s cannot be null");
		UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream unsafeLockBytesOnStream = new UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream(s);
		UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppstgOpen2;
		int num = UnsafeNativeCompoundFileMethods.StgCreateDocfileOnILockBytes(unsafeLockBytesOnStream, grfMode, 0, out ppstgOpen2);
		if (num == 0)
		{
			ppstgOpen = new SafeIStorageImplementation(ppstgOpen2, unsafeLockBytesOnStream);
			return num;
		}
		ppstgOpen = null;
		unsafeLockBytesOnStream.Dispose();
		return num;
	}

	internal static int SafeStgOpenStorageOnStream(Stream s, int grfMode, out IStorage ppstgOpen)
	{
		Invariant.Assert(s != null, "s cannot be null");
		UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream unsafeLockBytesOnStream = new UnsafeNativeCompoundFileMethods.UnsafeLockBytesOnStream(s);
		UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppstgOpen2;
		int num = UnsafeNativeCompoundFileMethods.StgOpenStorageOnILockBytes(unsafeLockBytesOnStream, null, grfMode, new IntPtr(0), 0, out ppstgOpen2);
		if (num == 0)
		{
			ppstgOpen = new SafeIStorageImplementation(ppstgOpen2);
			return num;
		}
		ppstgOpen = null;
		unsafeLockBytesOnStream.Dispose();
		return num;
	}

	internal static int SafeStgCreateStorageEx(string pwcsName, int grfMode, int stgfmt, int grfAttrs, nint pStgOptions, nint reserved2, ref Guid riid, out IStorage ppObjectOpen)
	{
		UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppObjectOpen2;
		int num = UnsafeNativeCompoundFileMethods.StgCreateStorageEx(pwcsName, grfMode, stgfmt, grfAttrs, pStgOptions, reserved2, ref riid, out ppObjectOpen2);
		if (num == 0)
		{
			ppObjectOpen = new SafeIStorageImplementation(ppObjectOpen2);
			return num;
		}
		ppObjectOpen = null;
		return num;
	}

	internal static int SafeStgOpenStorageEx(string pwcsName, int grfMode, int stgfmt, int grfAttrs, nint pStgOptions, nint reserved2, ref Guid riid, out IStorage ppObjectOpen)
	{
		UnsafeNativeCompoundFileMethods.UnsafeNativeIStorage ppObjectOpen2;
		int num = UnsafeNativeCompoundFileMethods.StgOpenStorageEx(pwcsName, grfMode, stgfmt, grfAttrs, pStgOptions, reserved2, ref riid, out ppObjectOpen2);
		if (num == 0)
		{
			ppObjectOpen = new SafeIStorageImplementation(ppObjectOpen2);
			return num;
		}
		ppObjectOpen = null;
		return num;
	}

	internal static int SafePropVariantClear(ref PROPVARIANT pvar)
	{
		return UnsafeNativeCompoundFileMethods.PropVariantClear(ref pvar);
	}
}

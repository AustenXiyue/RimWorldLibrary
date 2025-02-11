using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

/// <summary>Provides access and information for manipulating data stores in a <see cref="T:System.IO.Packaging.Package" />.    </summary>
public class StorageInfo
{
	private enum EnumeratorTypes
	{
		Everything,
		OnlyStorages,
		OnlyStreams
	}

	private StorageInfo parentStorage;

	private StorageRoot rootStorage;

	internal StorageInfoCore core;

	private static readonly string sc_compressionTransformName = "CompressionTransform";

	private static readonly string sc_dataspaceLabelNoEncryptionNormalCompression = "NoEncryptionNormalCompression";

	private static readonly string sc_dataspaceLabelRMEncryptionNormalCompression = "RMEncryptionNormalCompression";

	/// <summary>Gets the name of the store.</summary>
	/// <returns>The name of this store.</returns>
	public string Name
	{
		get
		{
			CheckDisposedStatus();
			return core.storageName;
		}
	}

	internal string FullNameInternal
	{
		get
		{
			CheckDisposedStatus();
			return ContainerUtilities.ConvertStringArrayPathToBackSlashPath(BuildFullNameInternalFromParentNameInternal());
		}
	}

	internal StorageRoot Root
	{
		get
		{
			CheckDisposedStatus();
			if (rootStorage == null)
			{
				return (StorageRoot)this;
			}
			return rootStorage;
		}
	}

	internal bool Exists
	{
		get
		{
			CheckDisposedStatus();
			return InternalExists();
		}
	}

	internal IStorage SafeIStorage
	{
		get
		{
			VerifyExists();
			return core.safeIStorage;
		}
	}

	internal bool StorageDisposed
	{
		get
		{
			if (parentStorage != null)
			{
				if (core.storageName == null)
				{
					return true;
				}
				return parentStorage.StorageDisposed;
			}
			if (this is StorageRoot)
			{
				return ((StorageRoot)this).RootDisposed;
			}
			return rootStorage.RootDisposed;
		}
	}

	internal StorageInfo(IStorage safeIStorage)
	{
		core = new StorageInfoCore(null, safeIStorage);
	}

	private void BuildStorageInfoRelativeToStorage(StorageInfo parent, string fileName)
	{
		parentStorage = parent;
		core = parent.CoreForChildStorage(fileName);
		rootStorage = parent.Root;
	}

	internal StorageInfo(StorageInfo parent, string fileName)
	{
		ContainerUtilities.CheckAgainstNull(parent, "parent");
		ContainerUtilities.CheckAgainstNull(fileName, "fileName");
		BuildStorageInfoRelativeToStorage(parent, fileName);
	}

	private StorageInfoCore CoreForChildStorage(string storageNname)
	{
		CheckDisposedStatus();
		object obj = core.elementInfoCores[storageNname];
		if (obj != null && !(obj is StorageInfoCore))
		{
			throw new InvalidOperationException(SR.Format(SR.NameAlreadyInUse, storageNname));
		}
		if (obj == null)
		{
			obj = new StorageInfoCore(storageNname);
			core.elementInfoCores[storageNname] = obj;
		}
		return obj as StorageInfoCore;
	}

	internal StreamInfoCore CoreForChildStream(string streamName)
	{
		CheckDisposedStatus();
		object obj = core.elementInfoCores[streamName];
		if (obj != null && !(obj is StreamInfoCore))
		{
			throw new InvalidOperationException(SR.Format(SR.NameAlreadyInUse, streamName));
		}
		if (obj == null)
		{
			DataSpaceManager dataSpaceManager = Root.GetDataSpaceManager();
			obj = ((dataSpaceManager == null) ? new StreamInfoCore(streamName, null, null) : new StreamInfoCore(streamName, dataSpaceManager.DataSpaceOf(new CompoundFileStreamReference(FullNameInternal, streamName))));
			core.elementInfoCores[streamName] = obj;
		}
		return obj as StreamInfoCore;
	}

	/// <summary>Creates a new stream with a given name, <see cref="T:System.IO.Packaging.CompressionOption" />, and <see cref="T:System.IO.Packaging.EncryptionOption" />.</summary>
	/// <returns>The new stream with the specified <paramref name="name" />, <paramref name="compressionOption" />, and <paramref name="encryptionOption" />.</returns>
	/// <param name="name">The name for the new stream.</param>
	/// <param name="compressionOption">The compression option for the data stream.</param>
	/// <param name="encryptionOption">The encryption option for the data stream.</param>
	public StreamInfo CreateStream(string name, CompressionOption compressionOption, EncryptionOption encryptionOption)
	{
		CheckDisposedStatus();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)name, (object?)EncryptedPackageEnvelope.PackageStreamName))
		{
			throw new ArgumentException(SR.Format(SR.StreamNameNotValid, name));
		}
		StreamInfo streamInfo = new StreamInfo(this, name, compressionOption, encryptionOption);
		if (streamInfo.InternalExists())
		{
			throw new IOException(SR.StreamAlreadyExist);
		}
		DataSpaceManager dataSpaceManager = Root.GetDataSpaceManager();
		string text = null;
		if (dataSpaceManager != null)
		{
			if (compressionOption != CompressionOption.NotCompressed && !dataSpaceManager.TransformLabelIsDefined(sc_compressionTransformName))
			{
				dataSpaceManager.DefineTransform(CompressionTransform.ClassTransformIdentifier, sc_compressionTransformName);
			}
			if (encryptionOption == EncryptionOption.RightsManagement && !dataSpaceManager.TransformLabelIsDefined(EncryptedPackageEnvelope.EncryptionTransformName))
			{
				throw new SystemException(SR.RightsManagementEncryptionTransformNotFound);
			}
			if (compressionOption != CompressionOption.NotCompressed && encryptionOption == EncryptionOption.RightsManagement)
			{
				text = sc_dataspaceLabelRMEncryptionNormalCompression;
				if (!dataSpaceManager.DataSpaceIsDefined(text))
				{
					dataSpaceManager.DefineDataSpace(new string[2]
					{
						EncryptedPackageEnvelope.EncryptionTransformName,
						sc_compressionTransformName
					}, text);
				}
			}
			else if (compressionOption != CompressionOption.NotCompressed && encryptionOption == EncryptionOption.None)
			{
				text = sc_dataspaceLabelNoEncryptionNormalCompression;
				if (!dataSpaceManager.DataSpaceIsDefined(text))
				{
					dataSpaceManager.DefineDataSpace(new string[1] { sc_compressionTransformName }, text);
				}
			}
			else if (encryptionOption == EncryptionOption.RightsManagement)
			{
				text = EncryptedPackageEnvelope.DataspaceLabelRMEncryptionNoCompression;
				if (!dataSpaceManager.DataSpaceIsDefined(text))
				{
					dataSpaceManager.DefineDataSpace(new string[1] { EncryptedPackageEnvelope.EncryptionTransformName }, text);
				}
			}
		}
		if (text == null)
		{
			streamInfo.Create();
		}
		else
		{
			streamInfo.Create(text);
		}
		return streamInfo;
	}

	/// <summary>Creates a new stream with a given name.</summary>
	/// <returns>The new stream with the specified <paramref name="name" />.</returns>
	/// <param name="name">The name for the new stream.</param>
	public StreamInfo CreateStream(string name)
	{
		return CreateStream(name, CompressionOption.NotCompressed, EncryptionOption.None);
	}

	/// <summary>Returns the <see cref="T:System.IO.Packaging.StreamInfo" /> instance with the given name.</summary>
	/// <returns>The stream with the specified <paramref name="name" />.</returns>
	/// <param name="name">The name of the <see cref="T:System.IO.Packaging.StreamInfo" /> instance to return.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.</exception>
	public StreamInfo GetStreamInfo(string name)
	{
		CheckDisposedStatus();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		StreamInfo streamInfo = new StreamInfo(this, name);
		if (streamInfo.InternalExists())
		{
			return streamInfo;
		}
		throw new IOException(SR.StreamNotExist);
	}

	/// <summary>Returns a value that indicates whether a given stream exists.</summary>
	/// <returns>true if a stream with the specified <paramref name="name" /> exists; otherwise, false.</returns>
	/// <param name="name">The <see cref="T:System.IO.Packaging.StreamInfo" /> name to check for.</param>
	public bool StreamExists(string name)
	{
		CheckDisposedStatus();
		return new StreamInfo(this, name).InternalExists();
	}

	/// <summary>Deletes the stream with a specified name. </summary>
	/// <param name="name">The <see cref="T:System.IO.Packaging.StreamInfo" /> name of the stream to delete.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.</exception>
	public void DeleteStream(string name)
	{
		CheckDisposedStatus();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		StreamInfo streamInfo = new StreamInfo(this, name);
		if (streamInfo.InternalExists())
		{
			streamInfo.Delete();
		}
	}

	/// <summary>Creates a new child <see cref="T:System.IO.Packaging.StorageInfo" /> with this <see cref="T:System.IO.Packaging.StorageInfo" /> as the parent.</summary>
	/// <returns>The new child data store.</returns>
	/// <param name="name">The name for the new child data store.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.</exception>
	public StorageInfo CreateSubStorage(string name)
	{
		CheckDisposedStatus();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return CreateStorage(name);
	}

	/// <summary>Returns the child sub-store with a given name.</summary>
	/// <returns>The child sub-store with the specified <paramref name="name" />.</returns>
	/// <param name="name">The name of the sub-store to return.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.</exception>
	public StorageInfo GetSubStorageInfo(string name)
	{
		StorageInfo storageInfo = new StorageInfo(this, name);
		if (storageInfo.InternalExists(name))
		{
			return storageInfo;
		}
		throw new IOException(SR.StorageNotExist);
	}

	/// <summary>Returns a value that indicates whether a given child sub-store exists.</summary>
	/// <returns>true if a child sub-store with the specified <paramref name="name" /> exists; otherwise, false.</returns>
	/// <param name="name">The child <see cref="T:System.IO.Packaging.StorageInfo" /> name to check for.</param>
	public bool SubStorageExists(string name)
	{
		return new StorageInfo(this, name).InternalExists(name);
	}

	/// <summary>Deletes a specified sub-store.</summary>
	/// <param name="name">The name of the sub-store to delete.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null.</exception>
	public void DeleteSubStorage(string name)
	{
		CheckDisposedStatus();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (new StorageInfo(this, name).InternalExists(name))
		{
			InvalidateEnumerators();
			DestroyElement(name);
		}
	}

	/// <summary>Returns an array of the <see cref="T:System.IO.Packaging.StreamInfo" /> instances that are currently contained in this store.</summary>
	/// <returns>An array of the <see cref="T:System.IO.Packaging.StreamInfo" /> objects, each pointing to an I/O steam defined within this store.</returns>
	public StreamInfo[] GetStreams()
	{
		CheckDisposedStatus();
		VerifyExists();
		EnsureArrayForEnumeration(EnumeratorTypes.OnlyStreams);
		ArrayList obj = (ArrayList)core.validEnumerators[EnumeratorTypes.OnlyStreams];
		Invariant.Assert(obj != null);
		return (StreamInfo[])obj.ToArray(typeof(StreamInfo));
	}

	/// <summary>Returns an array of the child sub-stores that are currently contained in this store.</summary>
	/// <returns>An array of <see cref="T:System.IO.Packaging.StorageInfo" /> objects, each pointing to a sub-store defined within this store.</returns>
	public StorageInfo[] GetSubStorages()
	{
		CheckDisposedStatus();
		VerifyExists();
		EnsureArrayForEnumeration(EnumeratorTypes.OnlyStorages);
		ArrayList obj = (ArrayList)core.validEnumerators[EnumeratorTypes.OnlyStorages];
		Invariant.Assert(obj != null);
		return (StorageInfo[])obj.ToArray(typeof(StorageInfo));
	}

	internal void Create()
	{
		CheckDisposedStatus();
		if (parentStorage != null)
		{
			if (!parentStorage.Exists)
			{
				parentStorage.Create();
			}
			if (!InternalExists())
			{
				parentStorage.CreateStorage(core.storageName);
			}
		}
	}

	private StorageInfo CreateStorage(string name)
	{
		StorageInfo storageInfo = new StorageInfo(this, name);
		if (!storageInfo.InternalExists(name))
		{
			StorageInfoCore storageInfoCore = core.elementInfoCores[name] as StorageInfoCore;
			Invariant.Assert(storageInfoCore != null);
			int num = core.safeIStorage.CreateStorage(name, (GetStat().grfMode & 3) | 0x10, 0, 0, out storageInfoCore.safeIStorage);
			switch (num)
			{
			case -2147287035:
				throw new UnauthorizedAccessException(SR.CanNotCreateAccessDenied, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage.CreateStorage"), num));
			default:
				throw new IOException(SR.UnableToCreateStorage, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage.CreateStorage"), num));
			case 0:
				InvalidateEnumerators();
				return storageInfo;
			}
		}
		throw new IOException(SR.StorageAlreadyExist);
	}

	internal bool Delete(bool recursive, string name)
	{
		bool result = false;
		CheckDisposedStatus();
		if (parentStorage == null)
		{
			throw new InvalidOperationException(SR.CanNotDeleteRoot);
		}
		if (InternalExists(name))
		{
			if (!recursive && !StorageIsEmpty())
			{
				throw new IOException(SR.CanNotDeleteNonEmptyStorage);
			}
			InvalidateEnumerators();
			parentStorage.DestroyElement(name);
			result = true;
		}
		return result;
	}

	internal void RemoveSubStorageEntryFromDataSpaceMap(StorageInfo storageInfo)
	{
		StorageInfo[] subStorages = storageInfo.GetSubStorages();
		foreach (StorageInfo storageInfo2 in subStorages)
		{
			RemoveSubStorageEntryFromDataSpaceMap(storageInfo2);
		}
		StreamInfo[] streams = storageInfo.GetStreams();
		DataSpaceManager dataSpaceManager = Root.GetDataSpaceManager();
		StreamInfo[] array = streams;
		foreach (StreamInfo streamInfo in array)
		{
			dataSpaceManager.RemoveContainerFromDataSpaceMap(new CompoundFileStreamReference(storageInfo.FullNameInternal, streamInfo.Name));
		}
	}

	internal void DestroyElement(string elementNameInternal)
	{
		object obj = core.elementInfoCores[elementNameInternal];
		if (FileAccess.Read == Root.OpenAccess)
		{
			throw new UnauthorizedAccessException(SR.CanNotDeleteInReadOnly);
		}
		DataSpaceManager dataSpaceManager = Root.GetDataSpaceManager();
		if (dataSpaceManager != null)
		{
			if (obj is StorageInfoCore)
			{
				string storageName = ((StorageInfoCore)obj).storageName;
				StorageInfo storageInfo = new StorageInfo(this, storageName);
				RemoveSubStorageEntryFromDataSpaceMap(storageInfo);
			}
			else if (obj is StreamInfoCore)
			{
				dataSpaceManager.RemoveContainerFromDataSpaceMap(new CompoundFileStreamReference(FullNameInternal, elementNameInternal));
			}
		}
		try
		{
			core.safeIStorage.DestroyElement(elementNameInternal);
		}
		catch (COMException ex)
		{
			if (ex.ErrorCode == -2147287035)
			{
				throw new UnauthorizedAccessException(SR.CanNotDeleteAccessDenied, ex);
			}
			throw new IOException(SR.CanNotDelete, ex);
		}
		InvalidateEnumerators();
		if (obj is StorageInfoCore)
		{
			StorageInfoCore storageInfoCore = (StorageInfoCore)obj;
			storageInfoCore.storageName = null;
			if (storageInfoCore.safeIStorage != null)
			{
				((IDisposable)storageInfoCore.safeIStorage).Dispose();
				storageInfoCore.safeIStorage = null;
			}
		}
		else if (obj is StreamInfoCore)
		{
			StreamInfoCore streamInfoCore = (StreamInfoCore)obj;
			streamInfoCore.streamName = null;
			try
			{
				if (streamInfoCore.exposedStream != null)
				{
					((Stream)streamInfoCore.exposedStream).Close();
				}
			}
			catch (Exception ex2)
			{
				if (CriticalExceptions.IsCriticalException(ex2))
				{
					throw;
				}
			}
			streamInfoCore.exposedStream = null;
			if (streamInfoCore.safeIStream != null)
			{
				((IDisposable)streamInfoCore.safeIStream).Dispose();
				streamInfoCore.safeIStream = null;
			}
		}
		core.elementInfoCores.Remove(elementNameInternal);
	}

	internal bool FindStatStgOfName(string streamName, out STATSTG statStg)
	{
		bool flag = false;
		IEnumSTATSTG ppEnum = null;
		core.safeIStorage.EnumElements(0, IntPtr.Zero, 0, out ppEnum);
		ppEnum.Reset();
		ppEnum.Next(1u, out statStg, out var pceltFetched);
		while (0 < pceltFetched && !flag)
		{
			if (((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)streamName, (object?)statStg.pwcsName))
			{
				flag = true;
			}
			else
			{
				ppEnum.Next(1u, out statStg, out pceltFetched);
			}
		}
		((IDisposable)ppEnum).Dispose();
		ppEnum = null;
		return flag;
	}

	internal bool StorageIsEmpty()
	{
		IEnumSTATSTG ppEnum = null;
		core.safeIStorage.EnumElements(0, IntPtr.Zero, 0, out ppEnum);
		ppEnum.Reset();
		ppEnum.Next(1u, out var _, out var pceltFetched);
		((IDisposable)ppEnum).Dispose();
		ppEnum = null;
		return pceltFetched == 0;
	}

	internal void InvalidateEnumerators()
	{
		InvalidateEnumerators(core);
	}

	private static void InvalidateEnumerators(StorageInfoCore invalidateCore)
	{
		foreach (ArrayList value in invalidateCore.validEnumerators.Values)
		{
			value.Clear();
		}
		invalidateCore.validEnumerators.Clear();
	}

	internal ArrayList BuildFullNameFromParentName()
	{
		if (parentStorage == null)
		{
			return new ArrayList();
		}
		ArrayList arrayList = parentStorage.BuildFullNameFromParentName();
		arrayList.Add(core.storageName);
		return arrayList;
	}

	internal ArrayList BuildFullNameInternalFromParentNameInternal()
	{
		if (parentStorage == null)
		{
			return new ArrayList();
		}
		ArrayList arrayList = parentStorage.BuildFullNameInternalFromParentNameInternal();
		arrayList.Add(core.storageName);
		return arrayList;
	}

	private bool InternalExists()
	{
		return InternalExists(core.storageName);
	}

	private bool InternalExists(string name)
	{
		if (core.safeIStorage != null)
		{
			return true;
		}
		if (parentStorage == null)
		{
			return true;
		}
		if (!parentStorage.Exists)
		{
			return false;
		}
		return parentStorage.CanOpenStorage(name);
	}

	private bool CanOpenStorage(string nameInternal)
	{
		bool result = false;
		StorageInfoCore storageInfoCore = core.elementInfoCores[nameInternal] as StorageInfoCore;
		int num = 0;
		num = core.safeIStorage.OpenStorage(nameInternal, null, (GetStat().grfMode & 3) | 0x10, IntPtr.Zero, 0, out storageInfoCore.safeIStorage);
		if (num == 0)
		{
			result = true;
		}
		else if (-2147287038 != num)
		{
			throw new IOException(SR.CanNotOpenStorage, new COMException(SR.Format(SR.NamedAPIFailure, "IStorage::OpenStorage"), num));
		}
		return result;
	}

	private void VerifyExists()
	{
		if (!InternalExists())
		{
			throw new DirectoryNotFoundException(SR.CanNotOnNonExistStorage);
		}
	}

	private STATSTG GetStat()
	{
		VerifyExists();
		core.safeIStorage.Stat(out var pstatstg, 0);
		return pstatstg;
	}

	private DateTime ConvertFILETIMEToDateTime(FILETIME time)
	{
		if (time.dwHighDateTime == 0 && time.dwLowDateTime == 0)
		{
			throw new NotSupportedException(SR.TimeStampNotAvailable);
		}
		return DateTime.FromFileTime(((long)time.dwHighDateTime << 32) + (uint)time.dwLowDateTime);
	}

	internal static void RecursiveStorageInfoCoreRelease(StorageInfoCore startCore)
	{
		if (startCore.safeIStorage == null)
		{
			return;
		}
		try
		{
			foreach (object value in startCore.elementInfoCores.Values)
			{
				if (value is StorageInfoCore)
				{
					RecursiveStorageInfoCoreRelease((StorageInfoCore)value);
				}
				else
				{
					if (!(value is StreamInfoCore))
					{
						continue;
					}
					StreamInfoCore streamInfoCore = (StreamInfoCore)value;
					try
					{
						if (streamInfoCore.exposedStream != null)
						{
							((Stream)streamInfoCore.exposedStream).Close();
						}
						streamInfoCore.exposedStream = null;
					}
					finally
					{
						if (streamInfoCore.safeIStream != null)
						{
							((IDisposable)streamInfoCore.safeIStream).Dispose();
							streamInfoCore.safeIStream = null;
						}
						((StreamInfoCore)value).streamName = null;
					}
				}
			}
			InvalidateEnumerators(startCore);
		}
		finally
		{
			if (startCore.safeIStorage != null)
			{
				((IDisposable)startCore.safeIStorage).Dispose();
				startCore.safeIStorage = null;
			}
			startCore.storageName = null;
		}
	}

	internal void CheckDisposedStatus()
	{
		if (StorageDisposed)
		{
			throw new ObjectDisposedException(null, SR.StorageInfoDisposed);
		}
	}

	private void EnsureArrayForEnumeration(EnumeratorTypes desiredArrayType)
	{
		if (core.validEnumerators[desiredArrayType] != null)
		{
			return;
		}
		ArrayList arrayList = new ArrayList();
		string text = null;
		IEnumSTATSTG ppEnum = null;
		core.safeIStorage.EnumElements(0, IntPtr.Zero, 0, out ppEnum);
		ppEnum.Reset();
		ppEnum.Next(1u, out var rgelt, out var pceltFetched);
		while (0 < pceltFetched)
		{
			text = rgelt.pwcsName;
			if (!ContainerUtilities.IsReservedName(text))
			{
				if (1 == rgelt.type)
				{
					if (desiredArrayType == EnumeratorTypes.Everything || desiredArrayType == EnumeratorTypes.OnlyStorages)
					{
						arrayList.Add(new StorageInfo(this, text));
					}
				}
				else
				{
					if (2 != rgelt.type)
					{
						throw new NotSupportedException(SR.UnsupportedTypeEncounteredWhenBuildingStgEnum);
					}
					if (desiredArrayType == EnumeratorTypes.Everything || desiredArrayType == EnumeratorTypes.OnlyStreams)
					{
						arrayList.Add(new StreamInfo(this, text));
					}
				}
			}
			ppEnum.Next(1u, out rgelt, out pceltFetched);
		}
		core.validEnumerators[desiredArrayType] = arrayList;
		((IDisposable)ppEnum).Dispose();
		ppEnum = null;
	}
}

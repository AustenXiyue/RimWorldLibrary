using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides support for reading and writing metadata to and from a bitmap image.</summary>
public class BitmapMetadata : ImageMetadata, IEnumerable, IEnumerable<string>
{
	[ComImport]
	[Guid("FEAA2A8D-B3F3-43E4-B25C-D1DE990A1AE1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IWICMetadataBlockReader
	{
		[PreserveSig]
		int GetContainerFormat(out Guid containerFormat);

		[PreserveSig]
		int GetCount(out uint count);

		[PreserveSig]
		int GetReaderByIndex(uint index, out nint ppIMetadataReader);

		[PreserveSig]
		int GetEnumerator(out nint pIEnumMetadata);
	}

	[ComImport]
	[Guid("08FB9676-B444-41E8-8DBE-6A53A542BFF1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IWICMetadataBlockWriter : IWICMetadataBlockReader
	{
		[PreserveSig]
		new int GetContainerFormat(out Guid containerFormat);

		[PreserveSig]
		new int GetCount(out uint count);

		[PreserveSig]
		new int GetReaderByIndex(uint index, out nint ppIMetadataReader);

		[PreserveSig]
		new int GetEnumerator(out nint pIEnumMetadata);

		[PreserveSig]
		int InitializeFromBlockReader(nint pIBlockReader);

		[PreserveSig]
		int GetWriterByIndex(uint index, out nint ppIMetadataWriter);

		[PreserveSig]
		int AddWriter(nint pIMetadataWriter);

		[PreserveSig]
		int SetWriterByIndex(uint index, nint pIMetadataWriter);

		[PreserveSig]
		int RemoveWriterByIndex(uint index);
	}

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class BitmapMetadataBlockWriter : IWICMetadataBlockWriter, IWICMetadataBlockReader
	{
		private bool _fixedSize;

		private Guid _containerFormat;

		private ArrayList _metadataBlocks;

		internal ArrayList MetadataBlocks => _metadataBlocks;

		internal BitmapMetadataBlockWriter(Guid containerFormat, bool fixedSize)
		{
			_fixedSize = fixedSize;
			_containerFormat = containerFormat;
			_metadataBlocks = new ArrayList();
		}

		internal BitmapMetadataBlockWriter(BitmapMetadataBlockWriter blockWriter, object syncObject)
		{
			Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
			_fixedSize = blockWriter._fixedSize;
			_containerFormat = blockWriter._containerFormat;
			_metadataBlocks = new ArrayList();
			ArrayList metadataBlocks = blockWriter.MetadataBlocks;
			using FactoryMaker factoryMaker = new FactoryMaker();
			foreach (SafeMILHandle item in metadataBlocks)
			{
				lock (syncObject)
				{
					nint metadataWriter = IntPtr.Zero;
					try
					{
						HRESULT.Check(UnsafeNativeMethods.WICComponentFactory.CreateMetadataWriterFromReader(factoryMaker.ImagingFactoryPtr, item, ref guidVendor, out metadataWriter));
						SafeMILHandle value = new SafeMILHandle(metadataWriter);
						metadataWriter = IntPtr.Zero;
						_metadataBlocks.Add(value);
					}
					finally
					{
						if (metadataWriter != IntPtr.Zero)
						{
							UnsafeNativeMethods.MILUnknown.Release(metadataWriter);
						}
					}
				}
			}
		}

		public int GetContainerFormat(out Guid containerFormat)
		{
			containerFormat = _containerFormat;
			return 0;
		}

		public int GetCount(out uint count)
		{
			count = (uint)_metadataBlocks.Count;
			return 0;
		}

		public int GetReaderByIndex(uint index, out nint pIMetadataReader)
		{
			if (index >= _metadataBlocks.Count)
			{
				pIMetadataReader = IntPtr.Zero;
				return -2003292352;
			}
			SafeMILHandle pIUnknown = (SafeMILHandle)_metadataBlocks[(int)index];
			Guid guid = MILGuidData.IID_IWICMetadataReader;
			return UnsafeNativeMethods.MILUnknown.QueryInterface(pIUnknown, ref guid, out pIMetadataReader);
		}

		public int GetEnumerator(out nint pIEnumMetadata)
		{
			BitmapMetadataBlockWriterEnumerator o = new BitmapMetadataBlockWriterEnumerator(this);
			pIEnumMetadata = Marshal.GetComInterfaceForObject(o, typeof(IEnumUnknown));
			return 0;
		}

		public int InitializeFromBlockReader(nint pIBlockReader)
		{
			Invariant.Assert(pIBlockReader != IntPtr.Zero);
			int num = 0;
			uint count = 0u;
			Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
			ArrayList arrayList = new ArrayList();
			num = UnsafeNativeMethods.WICMetadataBlockReader.GetCount(pIBlockReader, out count);
			if (HRESULT.Succeeded(num))
			{
				using (FactoryMaker factoryMaker = new FactoryMaker())
				{
					for (uint num2 = 0u; num2 < count; num2++)
					{
						SafeMILHandle pIMetadataReader = null;
						nint metadataWriter = IntPtr.Zero;
						try
						{
							num = UnsafeNativeMethods.WICMetadataBlockReader.GetReaderByIndex(pIBlockReader, num2, out pIMetadataReader);
							if (!HRESULT.Failed(num))
							{
								num = UnsafeNativeMethods.WICComponentFactory.CreateMetadataWriterFromReader(factoryMaker.ImagingFactoryPtr, pIMetadataReader, ref guidVendor, out metadataWriter);
								if (!HRESULT.Failed(num))
								{
									SafeMILHandle value = new SafeMILHandle(metadataWriter);
									metadataWriter = IntPtr.Zero;
									arrayList.Add(value);
									continue;
								}
							}
						}
						finally
						{
							pIMetadataReader?.Dispose();
							if (metadataWriter != IntPtr.Zero)
							{
								UnsafeNativeMethods.MILUnknown.Release(metadataWriter);
							}
						}
						break;
					}
				}
				_metadataBlocks = arrayList;
			}
			return num;
		}

		public int GetWriterByIndex(uint index, out nint pIMetadataWriter)
		{
			if (index >= _metadataBlocks.Count)
			{
				pIMetadataWriter = IntPtr.Zero;
				return -2003292352;
			}
			SafeMILHandle pIUnknown = (SafeMILHandle)_metadataBlocks[(int)index];
			Guid guid = MILGuidData.IID_IWICMetadataWriter;
			return UnsafeNativeMethods.MILUnknown.QueryInterface(pIUnknown, ref guid, out pIMetadataWriter);
		}

		public int AddWriter(nint pIMetadataWriter)
		{
			if (pIMetadataWriter == IntPtr.Zero)
			{
				return -2147483645;
			}
			if (_fixedSize && _metadataBlocks.Count > 0)
			{
				return -2003292287;
			}
			SafeMILHandle safeMILHandle = new SafeMILHandle(pIMetadataWriter);
			UnsafeNativeMethods.MILUnknown.AddRef(safeMILHandle);
			_metadataBlocks.Add(safeMILHandle);
			return 0;
		}

		public int SetWriterByIndex(uint index, nint pIMetadataWriter)
		{
			if (index >= _metadataBlocks.Count)
			{
				return -2003292352;
			}
			if (pIMetadataWriter == IntPtr.Zero)
			{
				return -2147483645;
			}
			if (_fixedSize)
			{
				return -2003292287;
			}
			SafeMILHandle safeMILHandle = new SafeMILHandle(pIMetadataWriter);
			UnsafeNativeMethods.MILUnknown.AddRef(safeMILHandle);
			_metadataBlocks[(int)index] = safeMILHandle;
			return 0;
		}

		public int RemoveWriterByIndex(uint index)
		{
			if (index >= _metadataBlocks.Count)
			{
				return -2003292352;
			}
			if (_fixedSize)
			{
				return -2003292287;
			}
			_metadataBlocks.Remove(index);
			return 0;
		}
	}

	[ComImport]
	[Guid("00000100-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumUnknown
	{
		[PreserveSig]
		int Next(uint celt, out nint rgelt, ref uint pceltFetched);

		[PreserveSig]
		int Skip(uint celt);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone(ref nint ppEnum);
	}

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class BitmapMetadataBlockWriterEnumerator : IEnumUnknown
	{
		private ArrayList _metadataBlocks;

		private uint _index;

		internal BitmapMetadataBlockWriterEnumerator(BitmapMetadataBlockWriter blockWriter)
		{
			_metadataBlocks = blockWriter.MetadataBlocks;
			_index = 0u;
		}

		public int Next(uint celt, out nint rgelt, ref uint pceltFetched)
		{
			if (celt > 1)
			{
				rgelt = IntPtr.Zero;
				pceltFetched = 0u;
				return -2003292287;
			}
			if (_index >= _metadataBlocks.Count || celt == 0)
			{
				rgelt = IntPtr.Zero;
				pceltFetched = 0u;
				return 1;
			}
			SafeMILHandle pIUnknown = (SafeMILHandle)_metadataBlocks[(int)_index];
			Guid guid = MILGuidData.IID_IWICMetadataReader;
			int num = UnsafeNativeMethods.MILUnknown.QueryInterface(pIUnknown, ref guid, out rgelt);
			if (HRESULT.Succeeded(num))
			{
				pceltFetched = 1u;
				_index++;
			}
			return num;
		}

		public int Skip(uint celt)
		{
			if (_index + celt > _metadataBlocks.Count)
			{
				_index = (uint)_metadataBlocks.Count;
				return 1;
			}
			_index += celt;
			return 0;
		}

		public int Reset()
		{
			_index = 0u;
			return 0;
		}

		public int Clone(ref nint ppEnum)
		{
			ppEnum = IntPtr.Zero;
			return -2003292287;
		}
	}

	private const string policy_Author = "System.Author";

	private const string policy_Title = "System.Title";

	private const string policy_Subject = "System.Subject";

	private const string policy_Comment = "System.Comment";

	private const string policy_Keywords = "System.Keywords";

	private const string policy_DateTaken = "System.Photo.DateTaken";

	private const string policy_ApplicationName = "System.ApplicationName";

	private const string policy_Copyright = "System.Copyright";

	private const string policy_CameraManufacturer = "System.Photo.CameraManufacturer";

	private const string policy_CameraModel = "System.Photo.CameraModel";

	private const string policy_Rating = "System.SimpleRating";

	private SafeMILHandle _metadataHandle;

	private BitmapMetadataBlockWriter _blockWriter;

	private bool _readOnly;

	private bool _fixedSize;

	private object _setQueryValue;

	private string _setQueryString;

	private object _syncObject = new object();

	/// <summary>Gets a value that identifies the format of the image.</summary>
	/// <returns>The format of the bitmap image.</returns>
	public string Format
	{
		get
		{
			EnsureBitmapMetadata();
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			Guid guid = GuidFormat;
			lock (_syncObject)
			{
				HRESULT.Check(UnsafeNativeMethods.WICCodec.WICMapGuidToShortName(ref guid, 0u, stringBuilder, ref pcchActual));
				if (pcchActual != 0)
				{
					stringBuilder = new StringBuilder((int)pcchActual);
					HRESULT.Check(UnsafeNativeMethods.WICCodec.WICMapGuidToShortName(ref guid, pcchActual, stringBuilder, ref pcchActual));
				}
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Gets a value that identifies the base location of the metadata that is associated with an image.</summary>
	/// <returns>The base location of the image metadata.</returns>
	public string Location
	{
		get
		{
			StringBuilder stringBuilder = null;
			uint pcchActual = 0u;
			EnsureBitmapMetadata();
			lock (_syncObject)
			{
				HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryReader.GetLocation(_metadataHandle, 0u, stringBuilder, out pcchActual));
				if (pcchActual != 0)
				{
					stringBuilder = new StringBuilder((int)pcchActual);
					HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryReader.GetLocation(_metadataHandle, pcchActual, stringBuilder, out pcchActual));
				}
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Gets a value that indicates whether the metadata that is associated with an image is read-only.</summary>
	/// <returns>true if the metadata is read-only; otherwise, false. The default value is false.</returns>
	public bool IsReadOnly
	{
		get
		{
			EnsureBitmapMetadata();
			return _readOnly;
		}
	}

	/// <summary>Gets a value that determines whether the <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object is a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> is a fixed size; otherwise, false. The default value is false.</returns>
	public bool IsFixedSize
	{
		get
		{
			EnsureBitmapMetadata();
			return _fixedSize;
		}
	}

	/// <summary>Gets or sets a value that represents the author of an image.</summary>
	/// <returns>The author of the image.</returns>
	public ReadOnlyCollection<string> Author
	{
		get
		{
			if (GetQuery("System.Author") is string[] list)
			{
				return new ReadOnlyCollection<string>(list);
			}
			return null;
		}
		set
		{
			string[] array = null;
			if (value != null)
			{
				array = new string[value.Count];
				value.CopyTo(array, 0);
			}
			SetQuery("System.Author", array);
		}
	}

	/// <summary>Gets or sets a value that identifies the title of an image file.</summary>
	/// <returns>The title of an image file.</returns>
	public string Title
	{
		get
		{
			return GetQuery("System.Title") as string;
		}
		set
		{
			SetQuery("System.Title", value);
		}
	}

	/// <summary>Gets or sets a value that identifies the image rating.</summary>
	/// <returns>The rating value between 0 and 5.</returns>
	public int Rating
	{
		get
		{
			object query = GetQuery("System.SimpleRating");
			if (query != null && query.GetType() == typeof(ushort))
			{
				return Convert.ToInt32(query, CultureInfo.InvariantCulture);
			}
			return 0;
		}
		set
		{
			SetQuery("System.SimpleRating", Convert.ToUInt16(value, CultureInfo.InvariantCulture));
		}
	}

	/// <summary>Gets or sets a value that indicates the subject matter of the bitmap image.</summary>
	/// <returns>The subject of the bitmap image.</returns>
	public string Subject
	{
		get
		{
			return GetQuery("System.Subject") as string;
		}
		set
		{
			SetQuery("System.Subject", value);
		}
	}

	/// <summary>Gets or sets a value that represents a comment that is associated with the image file.</summary>
	/// <returns>A comment that is associated with the image file.</returns>
	public string Comment
	{
		get
		{
			return GetQuery("System.Comment") as string;
		}
		set
		{
			SetQuery("System.Comment", value);
		}
	}

	/// <summary>Gets or sets a value that indicates the date that the image was taken.</summary>
	/// <returns>The date that the image was taken.</returns>
	public string DateTaken
	{
		get
		{
			object query = GetQuery("System.Photo.DateTaken");
			if (query != null && query.GetType() == typeof(FILETIME))
			{
				FILETIME fILETIME = (FILETIME)query;
				return DateTime.FromFileTime(((long)fILETIME.dwHighDateTime << 32) + (uint)fILETIME.dwLowDateTime).ToString();
			}
			return null;
		}
		set
		{
			DateTime dateTime = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
			PROPVARIANT pROPVARIANT = default(PROPVARIANT);
			pROPVARIANT.varType = 64;
			long num = dateTime.ToFileTime();
			pROPVARIANT.filetime.dwLowDateTime = (int)num;
			pROPVARIANT.filetime.dwHighDateTime = (int)((num >> 32) & 0xFFFFFFFFu);
			object value2 = pROPVARIANT.ToObject(_syncObject);
			SetQuery("System.Photo.DateTaken", value2);
		}
	}

	/// <summary>Gets or sets a value that identifies the name of the application that was used to construct or alter an image file.</summary>
	/// <returns>The name of the application that was used to construct or alter an image file.</returns>
	public string ApplicationName
	{
		get
		{
			return GetQuery("System.ApplicationName") as string;
		}
		set
		{
			SetQuery("System.ApplicationName", value);
		}
	}

	/// <summary>Gets or sets a value that indicates copyright information that is associated with the image file.</summary>
	/// <returns>The copyright information that is associated with the image file.</returns>
	public string Copyright
	{
		get
		{
			return GetQuery("System.Copyright") as string;
		}
		set
		{
			SetQuery("System.Copyright", value);
		}
	}

	/// <summary>Gets or sets a value that identifies the camera manufacturer that is associated with an image.</summary>
	/// <returns>The camera manufacturer that is associated with an image.</returns>
	public string CameraManufacturer
	{
		get
		{
			return GetQuery("System.Photo.CameraManufacturer") as string;
		}
		set
		{
			SetQuery("System.Photo.CameraManufacturer", value);
		}
	}

	/// <summary>Gets or sets a value that identifies the camera model that was used to capture the image.</summary>
	/// <returns>The camera model that was used to capture the image.</returns>
	public string CameraModel
	{
		get
		{
			return GetQuery("System.Photo.CameraModel") as string;
		}
		set
		{
			SetQuery("System.Photo.CameraModel", value);
		}
	}

	/// <summary>Gets or sets a collection of keywords that describe the bitmap image.</summary>
	/// <returns>A read-only collection of strings.</returns>
	public ReadOnlyCollection<string> Keywords
	{
		get
		{
			if (GetQuery("System.Keywords") is string[] list)
			{
				return new ReadOnlyCollection<string>(list);
			}
			return null;
		}
		set
		{
			string[] array = null;
			if (value != null)
			{
				array = new string[value.Count];
				value.CopyTo(array, 0);
			}
			SetQuery("System.Keywords", array);
		}
	}

	internal Guid GuidFormat
	{
		get
		{
			Guid pguidContainerFormat = default(Guid);
			EnsureBitmapMetadata();
			HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryReader.GetContainerFormat(_metadataHandle, out pguidContainerFormat));
			return pguidContainerFormat;
		}
	}

	internal SafeMILHandle InternalMetadataHandle => _metadataHandle;

	internal object SyncObject => _syncObject;

	internal BitmapMetadataBlockWriter BlockWriter => _blockWriter;

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> for use with the specified image format.</summary>
	/// <param name="containerFormat">The format of the bitmap image, specified as "gif", "jpg", "png", or "tiff".</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="containerFormat" /> is null.</exception>
	public BitmapMetadata(string containerFormat)
	{
		if (containerFormat == null)
		{
			throw new ArgumentNullException("containerFormat");
		}
		Guid guid = default(Guid);
		HRESULT.Check(UnsafeNativeMethods.WICCodec.WICMapShortNameToGuid(containerFormat, ref guid));
		Init(guid, readOnly: false, fixedSize: false);
	}

	internal BitmapMetadata()
	{
		_metadataHandle = null;
		_readOnly = true;
		_fixedSize = false;
		_blockWriter = null;
	}

	internal BitmapMetadata(SafeMILHandle metadataHandle, bool readOnly, bool fixedSize, object syncObject)
	{
		_metadataHandle = metadataHandle;
		_readOnly = readOnly;
		_fixedSize = fixedSize;
		_blockWriter = null;
		_syncObject = syncObject;
	}

	internal BitmapMetadata(BitmapMetadata bitmapMetadata)
	{
		if (bitmapMetadata == null)
		{
			throw new ArgumentNullException("bitmapMetadata");
		}
		Init(bitmapMetadata.GuidFormat, readOnly: false, bitmapMetadata._fixedSize);
	}

	private void Init(Guid containerFormat, bool readOnly, bool fixedSize)
	{
		int hr = 0;
		nint queryWriter = IntPtr.Zero;
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
			hr = UnsafeNativeMethods.WICImagingFactory.CreateQueryWriter(factoryMaker.ImagingFactoryPtr, ref containerFormat, ref guidVendor, out queryWriter);
		}
		if (HRESULT.Succeeded(hr))
		{
			_readOnly = readOnly;
			_fixedSize = fixedSize;
			_blockWriter = null;
			_metadataHandle = new SafeMILHandle(queryWriter);
			_syncObject = _metadataHandle;
		}
		else
		{
			InitializeFromBlockWriter(containerFormat, readOnly, fixedSize);
		}
	}

	private void InitializeFromBlockWriter(Guid containerFormat, bool readOnly, bool fixedSize)
	{
		nint num = IntPtr.Zero;
		nint ppIQueryWriter = IntPtr.Zero;
		using FactoryMaker factoryMaker = new FactoryMaker();
		try
		{
			_blockWriter = new BitmapMetadataBlockWriter(containerFormat, fixedSize);
			num = Marshal.GetComInterfaceForObject(_blockWriter, typeof(IWICMetadataBlockWriter));
			HRESULT.Check(UnsafeNativeMethods.WICComponentFactory.CreateQueryWriterFromBlockWriter(factoryMaker.ImagingFactoryPtr, num, ref ppIQueryWriter));
			_readOnly = readOnly;
			_fixedSize = fixedSize;
			_metadataHandle = new SafeMILHandle(ppIQueryWriter);
			ppIQueryWriter = IntPtr.Zero;
			_syncObject = _metadataHandle;
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.Release(num);
			}
			if (ppIQueryWriter != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.Release(ppIQueryWriter);
			}
		}
	}

	private void InitializeFromBlockWriter(BitmapMetadataBlockWriter sourceBlockWriter, object syncObject)
	{
		nint num = IntPtr.Zero;
		nint ppIQueryWriter = IntPtr.Zero;
		using FactoryMaker factoryMaker = new FactoryMaker();
		try
		{
			_blockWriter = new BitmapMetadataBlockWriter(sourceBlockWriter, syncObject);
			num = Marshal.GetComInterfaceForObject(_blockWriter, typeof(IWICMetadataBlockWriter));
			HRESULT.Check(UnsafeNativeMethods.WICComponentFactory.CreateQueryWriterFromBlockWriter(factoryMaker.ImagingFactoryPtr, num, ref ppIQueryWriter));
			_readOnly = false;
			_fixedSize = false;
			_metadataHandle = new SafeMILHandle(ppIQueryWriter);
			ppIQueryWriter = IntPtr.Zero;
			_syncObject = _metadataHandle;
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.Release(num);
			}
			if (ppIQueryWriter != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.Release(ppIQueryWriter);
			}
		}
	}

	private void InitializeFromMetadataWriter(SafeMILHandle metadataHandle, object syncObject)
	{
		nint queryWriter = IntPtr.Zero;
		Guid guidVendor = new Guid(MILGuidData.GUID_VendorMicrosoft);
		try
		{
			int hr;
			using (FactoryMaker factoryMaker = new FactoryMaker())
			{
				lock (syncObject)
				{
					hr = UnsafeNativeMethods.WICImagingFactory.CreateQueryWriterFromReader(factoryMaker.ImagingFactoryPtr, metadataHandle, ref guidVendor, out queryWriter);
				}
			}
			if (HRESULT.Succeeded(hr))
			{
				_readOnly = false;
				_fixedSize = false;
				_blockWriter = null;
				_metadataHandle = new SafeMILHandle(queryWriter);
				queryWriter = IntPtr.Zero;
				_syncObject = _metadataHandle;
			}
			else if (!HRESULT.IsWindowsCodecError(hr))
			{
				HRESULT.Check(hr);
			}
		}
		finally
		{
			if (queryWriter != IntPtr.Zero)
			{
				UnsafeNativeMethods.MILUnknown.Release(queryWriter);
			}
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapMetadata Clone()
	{
		return (BitmapMetadata)base.Clone();
	}

	/// <summary>An Implementation of <see cref="M:System.Windows.Freezable.CreateInstance" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new BitmapMetadata();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		BitmapMetadata sourceBitmapMetadata = (BitmapMetadata)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmapMetadata);
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		BitmapMetadata sourceBitmapMetadata = (BitmapMetadata)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmapMetadata);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object to clone and freeze.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapMetadata sourceBitmapMetadata = (BitmapMetadata)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapMetadata);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		BitmapMetadata sourceBitmapMetadata = (BitmapMetadata)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmapMetadata);
	}

	/// <summary>Provides access to a metadata query writer that can write metadata to a bitmap image file.</summary>
	/// <param name="query">Identifies the location of the metadata to be written.</param>
	/// <param name="value">The value of the metadata to be written.</param>
	public void SetQuery(string query, object value)
	{
		WritePreamble();
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (_readOnly)
		{
			throw new InvalidOperationException(SR.Image_MetadataReadOnly);
		}
		_setQueryString = query;
		_setQueryValue = value;
		EnsureBitmapMetadata();
		PROPVARIANT propValue = default(PROPVARIANT);
		try
		{
			propValue.Init(value);
			if (propValue.RequiresSyncObject)
			{
				BitmapMetadata obj = value as BitmapMetadata;
				Invariant.Assert(obj != null);
				obj.VerifyAccess();
				lock (obj._syncObject)
				{
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryWriter.SetMetadataByName(_metadataHandle, query, ref propValue));
					}
				}
			}
			else
			{
				lock (_syncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICMetadataQueryWriter.SetMetadataByName(_metadataHandle, query, ref propValue));
				}
			}
		}
		finally
		{
			propValue.Clear();
		}
		WritePostscript();
	}

	/// <summary>Provides access to a metadata query reader that can extract metadata from a bitmap image file.</summary>
	/// <returns>The metadata at the specified query location.</returns>
	/// <param name="query">Identifies the string that is being queried in the current <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="query" /> is null.</exception>
	public object GetQuery(string query)
	{
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		EnsureBitmapMetadata();
		PROPVARIANT propValue = default(PROPVARIANT);
		try
		{
			propValue.Init(null);
			int metadataByName;
			lock (_syncObject)
			{
				metadataByName = UnsafeNativeMethods.WICMetadataQueryReader.GetMetadataByName(_metadataHandle, query, ref propValue);
			}
			if (metadataByName != -2003292352)
			{
				HRESULT.Check(metadataByName);
				object obj = propValue.ToObject(_syncObject);
				if (base.IsFrozenInternal && obj is BitmapMetadata bitmapMetadata)
				{
					bitmapMetadata.Freeze();
				}
				return obj;
			}
		}
		finally
		{
			propValue.Clear();
		}
		return null;
	}

	/// <summary>Removes a metadata query from an instance of <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" />.</summary>
	/// <param name="query">The metadata query to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="query" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Occurs when image metadata is read-only.</exception>
	public void RemoveQuery(string query)
	{
		WritePreamble();
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		if (_readOnly)
		{
			throw new InvalidOperationException(SR.Image_MetadataReadOnly);
		}
		EnsureBitmapMetadata();
		int num;
		lock (_syncObject)
		{
			num = UnsafeNativeMethods.WICMetadataQueryWriter.RemoveMetadataByName(_metadataHandle, query);
		}
		if (num != -2003292352)
		{
			HRESULT.Check(num);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Collections.IEnumerable.GetEnumerator" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		EnsureBitmapMetadata();
		return new BitmapMetadataEnumerator(_metadataHandle);
	}

	IEnumerator<string> IEnumerable<string>.GetEnumerator()
	{
		EnsureBitmapMetadata();
		return new BitmapMetadataEnumerator(_metadataHandle);
	}

	/// <summary>Determines whether a given query string exists within a <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object.</summary>
	/// <returns>true if the query string is found within the metadata store; otherwise, false.</returns>
	/// <param name="query">Identifies the string that is being queried in the current <see cref="T:System.Windows.Media.Imaging.BitmapMetadata" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="query" /> is null.</exception>
	public bool ContainsQuery(string query)
	{
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		EnsureBitmapMetadata();
		int hr;
		lock (_syncObject)
		{
			hr = UnsafeNativeMethods.WICMetadataQueryReader.ContainsMetadataByName(_metadataHandle, query, IntPtr.Zero);
		}
		if (HRESULT.IsWindowsCodecError(hr))
		{
			return false;
		}
		HRESULT.Check(hr);
		return true;
	}

	private void CopyCommon(BitmapMetadata sourceBitmapMetadata)
	{
		BitmapMetadataBlockWriter blockWriter = sourceBitmapMetadata.BlockWriter;
		if (blockWriter == null)
		{
			InitializeFromMetadataWriter(sourceBitmapMetadata._metadataHandle, sourceBitmapMetadata._syncObject);
		}
		if (_metadataHandle == null)
		{
			if (blockWriter != null)
			{
				InitializeFromBlockWriter(blockWriter, sourceBitmapMetadata._syncObject);
			}
			else
			{
				InitializeFromBlockWriter(sourceBitmapMetadata.GuidFormat, readOnly: false, fixedSize: false);
				SetQuery("/", sourceBitmapMetadata);
			}
		}
		_fixedSize = sourceBitmapMetadata._fixedSize;
	}

	private void EnsureBitmapMetadata()
	{
		ReadPreamble();
		if (_metadataHandle == null)
		{
			throw new InvalidOperationException(SR.Image_MetadataInitializationIncomplete);
		}
	}
}

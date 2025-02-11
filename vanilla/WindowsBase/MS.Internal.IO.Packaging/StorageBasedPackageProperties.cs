using System;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.IO.Packaging.CompoundFile;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class StorageBasedPackageProperties : PackageProperties, IDisposable
{
	private bool _disposed;

	private int _grfMode;

	private IPropertySetStorage _pss;

	private IPropertyStorage _psSummInfo;

	private IPropertyStorage _psDocSummInfo;

	public override string Title
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 2u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 2u, value);
		}
	}

	public override string Subject
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 3u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 3u, value);
		}
	}

	public override string Creator
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 4u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 4u, value);
		}
	}

	public override string Keywords
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 5u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 5u, value);
		}
	}

	public override string Description
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 6u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 6u, value);
		}
	}

	public override string LastModifiedBy
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 8u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 8u, value);
		}
	}

	public override string Revision
	{
		get
		{
			return GetOleProperty(FormatId.SummaryInformation, 9u) as string;
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 9u, value);
		}
	}

	public override DateTime? LastPrinted
	{
		get
		{
			return GetDateTimeProperty(FormatId.SummaryInformation, 11u);
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 11u, value);
		}
	}

	public override DateTime? Created
	{
		get
		{
			return GetDateTimeProperty(FormatId.SummaryInformation, 12u);
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 12u, value);
		}
	}

	public override DateTime? Modified
	{
		get
		{
			return GetDateTimeProperty(FormatId.SummaryInformation, 13u);
		}
		set
		{
			SetOleProperty(FormatId.SummaryInformation, 13u, value);
		}
	}

	public override string Category
	{
		get
		{
			return GetOleProperty(FormatId.DocumentSummaryInformation, 2u) as string;
		}
		set
		{
			SetOleProperty(FormatId.DocumentSummaryInformation, 2u, value);
		}
	}

	public override string Identifier
	{
		get
		{
			return GetOleProperty(FormatId.DocumentSummaryInformation, 18u) as string;
		}
		set
		{
			SetOleProperty(FormatId.DocumentSummaryInformation, 18u, value);
		}
	}

	public override string ContentType
	{
		get
		{
			string text = GetOleProperty(FormatId.DocumentSummaryInformation, 26u) as string;
			if (text == null)
			{
				return text;
			}
			return new ContentType(text).ToString();
		}
		set
		{
			if (value == null)
			{
				SetOleProperty(FormatId.DocumentSummaryInformation, 26u, value);
			}
			else
			{
				SetOleProperty(FormatId.DocumentSummaryInformation, 26u, new ContentType(value).ToString());
			}
		}
	}

	public override string Language
	{
		get
		{
			return GetOleProperty(FormatId.DocumentSummaryInformation, 27u) as string;
		}
		set
		{
			SetOleProperty(FormatId.DocumentSummaryInformation, 27u, value);
		}
	}

	public override string Version
	{
		get
		{
			return GetOleProperty(FormatId.DocumentSummaryInformation, 28u) as string;
		}
		set
		{
			SetOleProperty(FormatId.DocumentSummaryInformation, 28u, value);
		}
	}

	public override string ContentStatus
	{
		get
		{
			return GetOleProperty(FormatId.DocumentSummaryInformation, 29u) as string;
		}
		set
		{
			SetOleProperty(FormatId.DocumentSummaryInformation, 29u, value);
		}
	}

	internal StorageBasedPackageProperties(StorageRoot root)
	{
		_pss = (IPropertySetStorage)root.GetRootIStorage();
		_grfMode = 16;
		SafeNativeCompoundFileMethods.UpdateModeFlagFromFileAccess(root.OpenAccess, ref _grfMode);
		OpenPropertyStorage(ref FormatId.SummaryInformation, out _psSummInfo);
		OpenPropertyStorage(ref FormatId.DocumentSummaryInformation, out _psDocSummInfo);
	}

	~StorageBasedPackageProperties()
	{
		Dispose(disposing: false);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!(!_disposed && disposing))
			{
				return;
			}
			if (_psSummInfo != null)
			{
				try
				{
					((IDisposable)_psSummInfo).Dispose();
				}
				finally
				{
					_psSummInfo = null;
				}
			}
			if (_psDocSummInfo != null)
			{
				try
				{
					((IDisposable)_psDocSummInfo).Dispose();
					return;
				}
				finally
				{
					_psDocSummInfo = null;
				}
			}
		}
		finally
		{
			_disposed = true;
			base.Dispose(disposing);
		}
	}

	private object GetOleProperty(Guid fmtid, uint propId)
	{
		CheckDisposed();
		IPropertyStorage propertyStorage = ((fmtid == FormatId.SummaryInformation) ? _psSummInfo : _psDocSummInfo);
		if (propertyStorage == null)
		{
			return null;
		}
		object result = null;
		PROPSPEC[] array = new PROPSPEC[1];
		PROPVARIANT[] array2 = new PROPVARIANT[1];
		array[0].propType = 1u;
		array[0].union.propId = propId;
		VARTYPE vtFromPropId = GetVtFromPropId(fmtid, propId);
		int num = propertyStorage.ReadMultiple(1u, array, array2);
		switch (num)
		{
		case 0:
			try
			{
				if (array2[0].vt != vtFromPropId)
				{
					throw new FileFormatException(SR.Format(SR.WrongDocumentPropertyVariantType, propId, fmtid.ToString(), array2[0].vt, vtFromPropId));
				}
				switch (array2[0].vt)
				{
				case VARTYPE.VT_LPSTR:
				{
					nint pszVal = array2[0].union.pszVal;
					int length = Marshal.PtrToStringAnsi(pszVal).Length;
					byte[] array3 = new byte[length];
					Marshal.Copy(pszVal, array3, 0, length);
					result = Encoding.UTF8.GetString(array3);
					break;
				}
				case VARTYPE.VT_FILETIME:
					result = DateTime.FromFileTime(array2[0].union.hVal);
					break;
				default:
					throw new FileFormatException(SR.Format(SR.InvalidDocumentPropertyVariantType, array2[0].vt));
				}
			}
			finally
			{
				SafeNativeCompoundFileMethods.SafePropVariantClear(ref array2[0]);
			}
			break;
		default:
			SecurityHelper.ThrowExceptionForHR(num);
			break;
		case 1:
			break;
		}
		return result;
	}

	private void SetOleProperty(Guid fmtid, uint propId, object propVal)
	{
		CheckDisposed();
		IPropertyStorage ppprstg = ((fmtid == FormatId.SummaryInformation) ? _psSummInfo : _psDocSummInfo);
		if (ppprstg == null)
		{
			if (propVal == null)
			{
				return;
			}
			_pss.Create(ref fmtid, ref fmtid, 2u, (uint)_grfMode, out ppprstg);
			if (fmtid == FormatId.SummaryInformation)
			{
				_psSummInfo = ppprstg;
			}
			else
			{
				_psDocSummInfo = ppprstg;
			}
		}
		PROPSPEC[] array = new PROPSPEC[1];
		PROPVARIANT[] array2 = new PROPVARIANT[1];
		array[0].propType = 1u;
		array[0].union.propId = propId;
		if (propVal == null)
		{
			ppprstg.DeleteMultiple(1u, array);
			return;
		}
		nint num = IntPtr.Zero;
		try
		{
			if (propVal is string)
			{
				string text = propVal as string;
				num = Marshal.StringToCoTaskMemAnsi(text);
				string strB = Marshal.PtrToStringAnsi(num);
				if (string.CompareOrdinal(text, strB) != 0)
				{
					byte[] bytes = Encoding.UTF8.GetBytes(text);
					int num2 = bytes.Length;
					if (num != IntPtr.Zero)
					{
						Marshal.FreeCoTaskMem(num);
						num = IntPtr.Zero;
					}
					num = Marshal.AllocCoTaskMem(checked(num2 + 1));
					Marshal.Copy(bytes, 0, num, num2);
					Marshal.WriteByte(num, num2, 0);
				}
				array2[0].vt = VARTYPE.VT_LPSTR;
				array2[0].union.pszVal = num;
			}
			else
			{
				if (!(propVal is DateTime))
				{
					throw new ArgumentException(SR.Format(SR.InvalidDocumentPropertyType, propVal.GetType().ToString()), "propVal");
				}
				array2[0].vt = VARTYPE.VT_FILETIME;
				array2[0].union.hVal = ((DateTime)propVal).ToFileTime();
			}
			ppprstg.WriteMultiple(1u, array, array2, 0u);
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(num);
			}
		}
	}

	private DateTime? GetDateTimeProperty(Guid fmtid, uint propId)
	{
		object oleProperty = GetOleProperty(fmtid, propId);
		if (oleProperty == null)
		{
			return null;
		}
		return (DateTime?)oleProperty;
	}

	private void OpenPropertyStorage(ref Guid fmtid, out IPropertyStorage ips)
	{
		int num = _pss.Open(ref fmtid, (uint)_grfMode, out ips);
		if (num == -2147287038)
		{
			ips = null;
		}
		else
		{
			SecurityHelper.ThrowExceptionForHR(num);
		}
	}

	private VARTYPE GetVtFromPropId(Guid fmtid, uint propId)
	{
		if (fmtid == FormatId.SummaryInformation)
		{
			switch (propId)
			{
			case 2u:
			case 3u:
			case 4u:
			case 5u:
			case 6u:
			case 8u:
			case 9u:
				return VARTYPE.VT_LPSTR;
			case 11u:
			case 12u:
			case 13u:
				return VARTYPE.VT_FILETIME;
			default:
				throw new ArgumentException(SR.Format(SR.UnknownDocumentProperty, fmtid.ToString(), propId), "propId");
			}
		}
		if (fmtid == FormatId.DocumentSummaryInformation)
		{
			if (propId == 2 || propId == 18 || propId - 26 <= 3)
			{
				return VARTYPE.VT_LPSTR;
			}
			throw new ArgumentException(SR.Format(SR.UnknownDocumentProperty, fmtid.ToString(), propId), "propId");
		}
		throw new ArgumentException(SR.Format(SR.UnknownDocumentProperty, fmtid.ToString(), propId), "fmtid");
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.StorageBasedPackagePropertiesDiposed);
		}
	}
}

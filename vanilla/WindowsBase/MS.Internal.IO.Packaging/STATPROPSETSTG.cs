using System;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.IO.Packaging;

internal struct STATPROPSETSTG
{
	private Guid fmtid;

	private Guid clsid;

	private uint grfFlags;

	private FILETIME mtime;

	private FILETIME ctime;

	private FILETIME atime;

	private uint dwOSVersion;
}

using System;

namespace MS.Internal.Interop;

[Flags]
internal enum FOS : uint
{
	OVERWRITEPROMPT = 2u,
	STRICTFILETYPES = 4u,
	NOCHANGEDIR = 8u,
	PICKFOLDERS = 0x20u,
	FORCEFILESYSTEM = 0x40u,
	ALLNONSTORAGEITEMS = 0x80u,
	NOVALIDATE = 0x100u,
	ALLOWMULTISELECT = 0x200u,
	PATHMUSTEXIST = 0x800u,
	FILEMUSTEXIST = 0x1000u,
	CREATEPROMPT = 0x2000u,
	SHAREAWARE = 0x4000u,
	NOREADONLYRETURN = 0x8000u,
	NOTESTFILECREATE = 0x10000u,
	HIDEMRUPLACES = 0x20000u,
	HIDEPINNEDPLACES = 0x40000u,
	NODEREFERENCELINKS = 0x100000u,
	DONTADDTORECENT = 0x2000000u,
	FORCESHOWHIDDEN = 0x10000000u,
	DEFAULTNOMINIMODE = 0x20000000u,
	FORCEPREVIEWPANEON = 0x40000000u
}

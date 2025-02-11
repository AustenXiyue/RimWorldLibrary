using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F9-0000-0000-C000-000000000046")]
internal interface IShellLinkW
{
	void GetPath([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, [In][Out] WIN32_FIND_DATAW pfd, SLGP fFlags);

	nint GetIDList();

	void SetIDList(nint pidl);

	void GetDescription([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);

	void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

	void GetWorkingDirectory([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

	void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

	void GetArguments([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

	void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

	short GetHotKey();

	void SetHotKey(short wHotKey);

	uint GetShowCmd();

	void SetShowCmd(uint iShowCmd);

	void GetIconLocation([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

	void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

	void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

	void Resolve(nint hwnd, uint fFlags);

	void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
}

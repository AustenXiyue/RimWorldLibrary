using System;
using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("1CF2B120-547D-101B-8E65-08002B2BD119")]
internal interface IErrorInfo
{
	Guid GetGuid();

	string GetSource();

	string GetDescription();

	string GetHelpFile();

	string GetHelpFileContent();
}

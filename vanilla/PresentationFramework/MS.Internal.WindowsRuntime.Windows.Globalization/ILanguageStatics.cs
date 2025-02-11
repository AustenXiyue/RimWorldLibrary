using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("B23CD557-0865-46D4-89B8-D59BE8990F0D")]
internal interface ILanguageStatics
{
	string CurrentInputMethodLanguageTag { get; }

	bool IsWellFormed(string languageTag);
}

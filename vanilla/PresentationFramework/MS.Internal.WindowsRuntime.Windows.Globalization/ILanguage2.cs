using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("6A47E5B5-D94D-4886-A404-A5A5B9D5B494")]
internal interface ILanguage2
{
	LanguageLayoutDirection LayoutDirection { get; }
}

using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("30199F6E-914B-4B2A-9D6E-E3B0E27DBE4F")]
internal interface ILanguageStatics2
{
	bool TrySetInputMethodLanguageTag(string languageTag);
}

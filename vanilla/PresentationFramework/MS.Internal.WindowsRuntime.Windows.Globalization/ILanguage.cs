using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("EA79A752-F7C2-4265-B1BD-C4DEC4E4F080")]
internal interface ILanguage
{
	string DisplayName { get; }

	string LanguageTag { get; }

	string NativeName { get; }

	string Script { get; }
}

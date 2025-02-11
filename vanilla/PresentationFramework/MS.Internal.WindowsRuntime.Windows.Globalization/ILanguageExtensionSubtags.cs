using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("7D7DAF45-368D-4364-852B-DEC927037B85")]
internal interface ILanguageExtensionSubtags
{
	IReadOnlyList<string> GetExtensionSubtags(string singleton);
}

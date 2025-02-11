using System.ComponentModel;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ILanguageStatics_Delegates
{
	public delegate int IsWellFormed_0(nint thisPtr, nint languageTag, out byte result);
}

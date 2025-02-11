using System.ComponentModel;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ILanguageFactory_Delegates
{
	public delegate int CreateLanguage_0(nint thisPtr, nint languageTag, out nint result);
}

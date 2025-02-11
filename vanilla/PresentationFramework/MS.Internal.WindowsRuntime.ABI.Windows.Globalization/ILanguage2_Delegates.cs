using System.ComponentModel;
using MS.Internal.WindowsRuntime.Windows.Globalization;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Globalization;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ILanguage2_Delegates
{
	public delegate int get_LayoutDirection_0(nint thisPtr, out LanguageLayoutDirection value);
}

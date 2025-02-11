using System.ComponentModel;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IWordsSegmenterFactory_Delegates
{
	public delegate int CreateWithLanguage_0(nint thisPtr, nint language, out nint result);
}

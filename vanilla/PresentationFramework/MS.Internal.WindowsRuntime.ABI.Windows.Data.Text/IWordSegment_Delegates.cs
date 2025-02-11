using System.ComponentModel;
using MS.Internal.WindowsRuntime.Windows.Data.Text;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IWordSegment_Delegates
{
	public delegate int get_SourceTextSegment_1(nint thisPtr, out TextSegment value);
}

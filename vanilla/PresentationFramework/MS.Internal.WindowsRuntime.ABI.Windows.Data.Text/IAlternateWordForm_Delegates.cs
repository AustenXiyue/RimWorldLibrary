using System.ComponentModel;
using MS.Internal.WindowsRuntime.Windows.Data.Text;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IAlternateWordForm_Delegates
{
	public delegate int get_SourceTextSegment_0(nint thisPtr, out TextSegment value);

	public delegate int get_NormalizationFormat_2(nint thisPtr, out AlternateNormalizationFormat value);
}

using System.ComponentModel;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IWordsSegmenter_Delegates
{
	public delegate int GetTokenAt_1(nint thisPtr, nint text, uint startIndex, out nint result);

	public delegate int GetTokens_2(nint thisPtr, nint text, out nint result);

	public delegate int Tokenize_3(nint thisPtr, nint text, uint startIndex, nint handler);
}

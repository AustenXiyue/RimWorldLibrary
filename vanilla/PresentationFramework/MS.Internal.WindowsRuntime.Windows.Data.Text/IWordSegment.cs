using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[Guid("D2D4BA6D-987C-4CC0-B6BD-D49A11B38F9A")]
internal interface IWordSegment
{
	IReadOnlyList<AlternateWordForm> AlternateForms { get; }

	TextSegment SourceTextSegment { get; }

	string Text { get; }
}

using System.Collections.Generic;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
internal delegate void WordSegmentsTokenizingHandler(IEnumerable<WordSegment> precedingWords, IEnumerable<WordSegment> words);

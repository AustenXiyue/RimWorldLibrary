using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[Guid("86B4D4D1-B2FE-4E34-A81D-66640300454F")]
internal interface IWordsSegmenter
{
	string ResolvedLanguage { get; }

	WordSegment GetTokenAt(string text, uint startIndex);

	IReadOnlyList<WordSegment> GetTokens(string text);

	void Tokenize(string text, uint startIndex, WordSegmentsTokenizingHandler handler);
}

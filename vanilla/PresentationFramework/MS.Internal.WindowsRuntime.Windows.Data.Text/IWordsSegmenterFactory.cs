using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[Guid("E6977274-FC35-455C-8BFB-6D7F4653CA97")]
internal interface IWordsSegmenterFactory
{
	WordsSegmenter CreateWithLanguage(string language);
}

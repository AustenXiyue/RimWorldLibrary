using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[Guid("47396C1E-51B9-4207-9146-248E636A1D1D")]
internal interface IAlternateWordForm
{
	string AlternateText { get; }

	AlternateNormalizationFormat NormalizationFormat { get; }

	TextSegment SourceTextSegment { get; }
}

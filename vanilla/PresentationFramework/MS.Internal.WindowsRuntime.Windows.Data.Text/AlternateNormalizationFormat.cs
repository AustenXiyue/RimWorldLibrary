using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
internal enum AlternateNormalizationFormat
{
	NotNormalized = 0,
	Number = 1,
	Currency = 3,
	Date = 4,
	Time = 5
}

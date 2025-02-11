using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[Guid("9B0252AC-0C27-44F8-B792-9793FB66C63E")]
internal interface ILanguageFactory
{
	Language CreateLanguage(string languageTag);
}

using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("04a2dbf3-df83-116c-0946-0812abf6e07d")]
internal interface ILanguageExceptionErrorInfo
{
	IObjectReference GetLanguageException();
}

namespace System.Windows.Documents;

internal interface IXamlErrorHandler
{
	void Error(string message, XamlToRtfError xamlToRtfError);

	void FatalError(string message, XamlToRtfError xamlToRtfError);

	void IgnorableWarning(string message, XamlToRtfError xamlToRtfError);
}

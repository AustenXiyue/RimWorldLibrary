using System;

namespace WinRT.Interop;

internal class ManagedExceptionErrorInfo : IErrorInfo, ISupportErrorInfo
{
	private readonly Exception _exception;

	public ManagedExceptionErrorInfo(Exception ex)
	{
		_exception = ex;
	}

	public bool InterfaceSupportsErrorInfo(Guid riid)
	{
		return true;
	}

	public Guid GetGuid()
	{
		return default(Guid);
	}

	public string GetSource()
	{
		return _exception.Source;
	}

	public string GetDescription()
	{
		string text = _exception.Message;
		if (string.IsNullOrEmpty(text))
		{
			text = _exception.GetType().FullName;
		}
		return text;
	}

	public string GetHelpFile()
	{
		return _exception.HelpLink;
	}

	public string GetHelpFileContent()
	{
		return string.Empty;
	}
}

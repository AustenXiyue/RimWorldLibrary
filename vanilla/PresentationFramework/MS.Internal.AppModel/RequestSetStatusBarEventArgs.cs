using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.Utility;

namespace MS.Internal.AppModel;

internal sealed class RequestSetStatusBarEventArgs : RoutedEventArgs
{
	private MS.Internal.SecurityCriticalDataForSet<string> _text;

	internal string Text => _text.Value;

	internal static RequestSetStatusBarEventArgs Clear => new RequestSetStatusBarEventArgs(string.Empty);

	internal RequestSetStatusBarEventArgs(string text)
	{
		_text.Value = text;
		base.RoutedEvent = Hyperlink.RequestSetStatusBarEvent;
	}

	internal RequestSetStatusBarEventArgs(Uri targetUri)
	{
		if (targetUri == null)
		{
			_text.Value = string.Empty;
		}
		else
		{
			_text.Value = MS.Internal.Utility.BindUriHelper.UriToString(targetUri);
		}
		base.RoutedEvent = Hyperlink.RequestSetStatusBarEvent;
	}
}

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Controls;
using System.Windows.Navigation;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Internal.IO.Packaging;
using MS.Win32;

namespace MS.Internal.Controls;

[ClassInterface(ClassInterfaceType.None)]
internal class WebBrowserEvent : InternalDispatchObject<MS.Win32.UnsafeNativeMethods.DWebBrowserEvents2>, MS.Win32.UnsafeNativeMethods.DWebBrowserEvents2
{
	private WebBrowser _parent;

	public WebBrowserEvent(WebBrowser parent)
	{
		_parent = parent;
	}

	public void BeforeNavigate2(object pDisp, ref object url, ref object flags, ref object targetFrameName, ref object postData, ref object headers, ref bool cancel)
	{
		bool flag = false;
		bool flag2 = false;
		try
		{
			if (targetFrameName == null)
			{
				targetFrameName = "";
			}
			if (headers == null)
			{
				headers = "";
			}
			string text = (string)url;
			Uri uri = (string.IsNullOrEmpty(text) ? null : new Uri(text));
			MS.Win32.UnsafeNativeMethods.IWebBrowser2 webBrowser = (MS.Win32.UnsafeNativeMethods.IWebBrowser2)pDisp;
			if (_parent.AxIWebBrowser2 == webBrowser)
			{
				if (_parent.NavigatingToAboutBlank && string.Compare(text, "about:blank", StringComparison.OrdinalIgnoreCase) != 0)
				{
					_parent.NavigatingToAboutBlank = false;
				}
				if (_parent.NavigatingToAboutBlank)
				{
					uri = null;
				}
				NavigatingCancelEventArgs navigatingCancelEventArgs = new NavigatingCancelEventArgs(uri, null, null, null, NavigationMode.New, null, null, isNavInitiator: true);
				Guid lastNavigation = _parent.LastNavigation;
				_parent.OnNavigating(navigatingCancelEventArgs);
				if (_parent.LastNavigation != lastNavigation)
				{
					flag = true;
				}
				flag2 = navigatingCancelEventArgs.Cancel;
			}
		}
		catch
		{
			flag2 = true;
		}
		finally
		{
			if (flag2 && !flag)
			{
				_parent.CleanInternalState();
			}
			if (flag2 || flag)
			{
				cancel = true;
			}
		}
	}

	private static bool IsAllowedScriptScheme(Uri uri)
	{
		if (uri != null)
		{
			if (!(uri.Scheme == "javascript"))
			{
				return uri.Scheme == "vbscript";
			}
			return true;
		}
		return false;
	}

	public void NavigateComplete2(object pDisp, ref object url)
	{
		MS.Win32.UnsafeNativeMethods.IWebBrowser2 webBrowser = (MS.Win32.UnsafeNativeMethods.IWebBrowser2)pDisp;
		if (_parent.AxIWebBrowser2 != webBrowser || ShouldIgnoreCompletionEvent(ref url))
		{
			return;
		}
		if (_parent.DocumentStream != null)
		{
			Invariant.Assert(_parent.NavigatingToAboutBlank && string.Compare((string)url, "about:blank", StringComparison.OrdinalIgnoreCase) == 0);
			try
			{
				MS.Win32.UnsafeNativeMethods.IHTMLDocument nativeHTMLDocument = _parent.NativeHTMLDocument;
				if (nativeHTMLDocument != null)
				{
					MS.Win32.UnsafeNativeMethods.IPersistStreamInit obj = nativeHTMLDocument as MS.Win32.UnsafeNativeMethods.IPersistStreamInit;
					System.Runtime.InteropServices.ComTypes.IStream pstm = new ManagedIStream(_parent.DocumentStream);
					obj.Load(pstm);
				}
				return;
			}
			finally
			{
				_parent.DocumentStream = null;
			}
		}
		string text = (string)url;
		if (_parent.NavigatingToAboutBlank)
		{
			Invariant.Assert(string.Compare(text, "about:blank", StringComparison.OrdinalIgnoreCase) == 0);
			text = null;
		}
		NavigationEventArgs e = new NavigationEventArgs(string.IsNullOrEmpty(text) ? null : new Uri(text), null, null, null, null, isNavigationInitiator: true);
		_parent.OnNavigated(e);
	}

	public void DocumentComplete(object pDisp, ref object url)
	{
		MS.Win32.UnsafeNativeMethods.IWebBrowser2 webBrowser = (MS.Win32.UnsafeNativeMethods.IWebBrowser2)pDisp;
		if (_parent.AxIWebBrowser2 == webBrowser && !ShouldIgnoreCompletionEvent(ref url))
		{
			string text = (string)url;
			if (_parent.NavigatingToAboutBlank)
			{
				Invariant.Assert(string.Compare(text, "about:blank", StringComparison.OrdinalIgnoreCase) == 0);
				text = null;
			}
			NavigationEventArgs e = new NavigationEventArgs(string.IsNullOrEmpty(text) ? null : new Uri(text), null, null, null, null, isNavigationInitiator: true);
			_parent.OnLoadCompleted(e);
		}
	}

	private bool ShouldIgnoreCompletionEvent(ref object url)
	{
		string strA = url as string;
		if (_parent.NavigatingToAboutBlank)
		{
			return string.Compare(strA, "about:blank", StringComparison.OrdinalIgnoreCase) != 0;
		}
		return false;
	}

	public void CommandStateChange(long command, bool enable)
	{
		switch (command)
		{
		case 2L:
			_parent._canGoBack = enable;
			break;
		case 1L:
			_parent._canGoForward = enable;
			break;
		}
	}

	public void TitleChange(string text)
	{
	}

	public void SetSecureLockIcon(int secureLockIcon)
	{
	}

	public void NewWindow2(ref object ppDisp, ref bool cancel)
	{
	}

	public void ProgressChange(int progress, int progressMax)
	{
	}

	public void StatusTextChange(string text)
	{
		_parent.RaiseEvent(new RequestSetStatusBarEventArgs(text));
	}

	public void DownloadBegin()
	{
	}

	public void FileDownload(ref bool activeDocument, ref bool cancel)
	{
	}

	public void PrivacyImpactedStateChange(bool bImpacted)
	{
	}

	public void UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
	{
	}

	public void PrintTemplateTeardown(object pDisp)
	{
	}

	public void PrintTemplateInstantiation(object pDisp)
	{
	}

	public void NavigateError(object pDisp, ref object url, ref object frame, ref object statusCode, ref bool cancel)
	{
	}

	public void ClientToHostWindow(ref long cX, ref long cY)
	{
	}

	public void WindowClosing(bool isChildWindow, ref bool cancel)
	{
	}

	public void WindowSetHeight(int height)
	{
	}

	public void WindowSetWidth(int width)
	{
	}

	public void WindowSetTop(int top)
	{
	}

	public void WindowSetLeft(int left)
	{
	}

	public void WindowSetResizable(bool resizable)
	{
	}

	public void OnTheaterMode(bool theaterMode)
	{
	}

	public void OnFullScreen(bool fullScreen)
	{
	}

	public void OnStatusBar(bool statusBar)
	{
	}

	public void OnMenuBar(bool menuBar)
	{
	}

	public void OnToolBar(bool toolBar)
	{
	}

	public void OnVisible(bool visible)
	{
	}

	public void OnQuit()
	{
	}

	public void PropertyChange(string szProperty)
	{
	}

	public void DownloadComplete()
	{
	}

	public void SetPhishingFilterStatus(uint phishingFilterStatus)
	{
	}

	public void WindowStateChanged(uint dwFlags, uint dwValidFlagsMask)
	{
	}
}

using System;
using System.Windows.Interop;
using System.Windows.Threading;

namespace MS.Internal.AppModel;

internal class NativeProgressPageProxy : IProgressPage2, IProgressPage
{
	private INativeProgressPage _npp;

	public Uri DeploymentPath
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
		}
	}

	public DispatcherOperationCallback StopCallback
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
		}
	}

	public DispatcherOperationCallback RefreshCallback
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string ApplicationName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			_npp.SetApplicationName(value);
		}
	}

	public string PublisherName
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			_npp.SetPublisherName(value);
		}
	}

	internal NativeProgressPageProxy(INativeProgressPage npp)
	{
		_npp = npp;
	}

	public void ShowProgressMessage(string message)
	{
		_npp.ShowProgressMessage(message);
	}

	public void UpdateProgress(long bytesDownloaded, long bytesTotal)
	{
		_npp.OnDownloadProgress((ulong)bytesDownloaded, (ulong)bytesTotal);
	}
}

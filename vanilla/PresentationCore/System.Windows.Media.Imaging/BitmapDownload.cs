using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32.SafeHandles;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows.Media.Imaging;

internal static class BitmapDownload
{
	internal static AutoResetEvent _waitEvent;

	internal static Queue _workQueue;

	internal static Hashtable _uriTable;

	internal static AsyncCallback _readCallback;

	internal static AsyncCallback _responseCallback;

	private static Thread _thread;

	private static readonly object _syncLock;

	private const int READ_SIZE = 1024;

	static BitmapDownload()
	{
		_waitEvent = new AutoResetEvent(initialState: false);
		_waitEvent = new AutoResetEvent(initialState: false);
		_workQueue = Queue.Synchronized(new Queue());
		_uriTable = Hashtable.Synchronized(new Hashtable());
		_readCallback = ReadCallback;
		_responseCallback = ResponseCallback;
		_thread = new Thread(DownloadThreadProc);
		_syncLock = new object();
	}

	internal static void BeginDownload(BitmapDecoder decoder, Uri uri, RequestCachePolicy uriCachePolicy, Stream stream)
	{
		lock (_syncLock)
		{
			if (!_thread.IsAlive)
			{
				_thread.IsBackground = true;
				_thread.Start();
			}
		}
		QueueEntry queueEntry;
		if (uri != null)
		{
			lock (_syncLock)
			{
				if (_uriTable[uri] != null)
				{
					queueEntry = (QueueEntry)_uriTable[uri];
					queueEntry.decoders.Add(new WeakReference(decoder));
					return;
				}
			}
		}
		queueEntry = new QueueEntry();
		queueEntry.decoders = new List<WeakReference>();
		lock (_syncLock)
		{
			queueEntry.decoders.Add(new WeakReference(decoder));
		}
		queueEntry.inputUri = uri;
		queueEntry.inputStream = stream;
		string localPath = WinInet.InternetCacheFolder.LocalPath;
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder(260);
		MS.Win32.UnsafeNativeMethods.GetTempFileName(localPath, "WPF", 0u, stringBuilder);
		try
		{
			string text = stringBuilder.ToString();
			SafeFileHandle safeFileHandle = MS.Win32.UnsafeNativeMethods.CreateFile(text, 3221225472u, 0u, null, 2, 67109120, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw new Win32Exception();
			}
			queueEntry.outputStream = new FileStream(safeFileHandle, FileAccess.ReadWrite);
			queueEntry.streamPath = text;
			flag = true;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
		}
		if (!flag)
		{
			throw new IOException(SR.Image_CannotCreateTempFile);
		}
		queueEntry.readBuffer = new byte[1024];
		queueEntry.contentLength = -1L;
		queueEntry.contentType = string.Empty;
		queueEntry.lastPercent = 0;
		if (uri != null)
		{
			lock (_syncLock)
			{
				_uriTable[uri] = queueEntry;
			}
		}
		if (stream == null)
		{
			queueEntry.webRequest = WpfWebRequestHelper.CreateRequest(uri);
			if (uriCachePolicy != null)
			{
				queueEntry.webRequest.CachePolicy = uriCachePolicy;
			}
			queueEntry.webRequest.BeginGetResponse(_responseCallback, queueEntry);
		}
		else
		{
			_workQueue.Enqueue(queueEntry);
			_waitEvent.Set();
		}
	}

	internal static void DownloadThreadProc()
	{
		Queue workQueue = _workQueue;
		while (true)
		{
			_waitEvent.WaitOne();
			while (workQueue.Count != 0)
			{
				QueueEntry queueEntry = (QueueEntry)workQueue.Dequeue();
				try
				{
					queueEntry.inputStream.BeginRead(queueEntry.readBuffer, 0, 1024, _readCallback, queueEntry);
				}
				catch (Exception e)
				{
					MarshalException(queueEntry, e);
				}
				finally
				{
					queueEntry = null;
				}
			}
		}
	}

	private static void ResponseCallback(IAsyncResult result)
	{
		QueueEntry queueEntry = (QueueEntry)result.AsyncState;
		try
		{
			WebResponse webResponse = WpfWebRequestHelper.EndGetResponse(queueEntry.webRequest, result);
			queueEntry.inputStream = webResponse.GetResponseStream();
			queueEntry.contentLength = webResponse.ContentLength;
			queueEntry.contentType = webResponse.ContentType;
			queueEntry.webRequest = null;
			_workQueue.Enqueue(queueEntry);
			_waitEvent.Set();
		}
		catch (Exception e)
		{
			MarshalException(queueEntry, e);
		}
	}

	private static void ReadCallback(IAsyncResult result)
	{
		QueueEntry queueEntry = (QueueEntry)result.AsyncState;
		int num = 0;
		try
		{
			num = queueEntry.inputStream.EndRead(result);
		}
		catch (Exception e)
		{
			MarshalException(queueEntry, e);
		}
		if (num == 0)
		{
			queueEntry.inputStream.Close();
			queueEntry.inputStream = null;
			queueEntry.outputStream.Flush();
			queueEntry.outputStream.Seek(0L, SeekOrigin.Begin);
			lock (_syncLock)
			{
				foreach (WeakReference decoder in queueEntry.decoders)
				{
					if (decoder.Target is LateBoundBitmapDecoder lateBoundBitmapDecoder)
					{
						MarshalEvents(lateBoundBitmapDecoder, lateBoundBitmapDecoder.ProgressCallback, 100);
						MarshalEvents(lateBoundBitmapDecoder, lateBoundBitmapDecoder.DownloadCallback, queueEntry.outputStream);
					}
				}
			}
			if (queueEntry.inputUri != null)
			{
				lock (_syncLock)
				{
					_uriTable[queueEntry.inputUri] = null;
					return;
				}
			}
			return;
		}
		queueEntry.outputStream.Write(queueEntry.readBuffer, 0, num);
		if (queueEntry.contentLength > 0)
		{
			int num2 = (int)Math.Floor(100.0 * (double)queueEntry.outputStream.Length / (double)queueEntry.contentLength);
			if (num2 != queueEntry.lastPercent)
			{
				queueEntry.lastPercent = num2;
				lock (_syncLock)
				{
					foreach (WeakReference decoder2 in queueEntry.decoders)
					{
						if (decoder2.Target is LateBoundBitmapDecoder lateBoundBitmapDecoder2)
						{
							MarshalEvents(lateBoundBitmapDecoder2, lateBoundBitmapDecoder2.ProgressCallback, num2);
						}
					}
				}
			}
		}
		_workQueue.Enqueue(queueEntry);
		_waitEvent.Set();
	}

	private static void MarshalEvents(LateBoundBitmapDecoder decoder, DispatcherOperationCallback doc, object arg)
	{
		decoder.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, doc, arg);
	}

	private static void MarshalException(QueueEntry entry, Exception e)
	{
		lock (_syncLock)
		{
			foreach (WeakReference decoder in entry.decoders)
			{
				if (decoder.Target is LateBoundBitmapDecoder lateBoundBitmapDecoder)
				{
					MarshalEvents(lateBoundBitmapDecoder, lateBoundBitmapDecoder.ExceptionCallback, e);
				}
			}
			if (entry.inputUri != null)
			{
				lock (_syncLock)
				{
					_uriTable[entry.inputUri] = null;
					return;
				}
			}
		}
	}
}

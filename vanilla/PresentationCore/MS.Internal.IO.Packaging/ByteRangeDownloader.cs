using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.IO.Packaging;

[FriendAccessAllowed]
internal class ByteRangeDownloader : IDisposable
{
	private bool _firstRequestMade;

	private bool _disposed;

	private object _syncObject = new object();

	private bool _erroredOut;

	private Exception _erroredOutException;

	private Uri _requestedUri;

	private RequestCachePolicy _cachePolicy;

	private IWebProxy _proxy;

	private ICredentials _credentials;

	private CookieContainer _cookieContainer = new CookieContainer(1);

	private SafeWaitHandle _eventHandle;

	private Mutex _fileMutex;

	private Stream _tempFileStream;

	private ArrayList _byteRangesAvailable = new ArrayList(2);

	private ArrayList _requestsOnWait;

	private int[,] _byteRangesInProgress;

	private HttpWebRequest _webRequest;

	private byte[] _buffer;

	private const int WriteBufferSize = 4096;

	private const int TimeOut = 5000;

	private const int Offset_Index = 0;

	private const int Length_Index = 1;

	private const string ByteRangeUnit = "BYTES ";

	private const string ContentRangeHeader = "Content-Range";

	internal IWebProxy Proxy
	{
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!_firstRequestMade)
			{
				_proxy = value;
				return;
			}
			throw new InvalidOperationException(SR.RequestAlreadyStarted);
		}
	}

	internal ICredentials Credentials
	{
		set
		{
			CheckDisposed();
			_credentials = value;
		}
	}

	internal RequestCachePolicy CachePolicy
	{
		set
		{
			CheckDisposed();
			if (!_firstRequestMade)
			{
				_cachePolicy = value;
				return;
			}
			throw new InvalidOperationException(SR.RequestAlreadyStarted);
		}
	}

	internal Mutex FileMutex
	{
		get
		{
			CheckDisposed();
			return _fileMutex;
		}
	}

	internal bool ErroredOut
	{
		get
		{
			CheckDisposed();
			lock (_syncObject)
			{
				return _erroredOut;
			}
		}
	}

	internal ByteRangeDownloader(Uri requestedUri, string tempFileName, SafeWaitHandle eventHandle)
		: this(requestedUri, eventHandle)
	{
		if (tempFileName == null)
		{
			throw new ArgumentNullException("tempFileName");
		}
		if (tempFileName.Length <= 0)
		{
			throw new ArgumentException(SR.InvalidTempFileName, "tempFileName");
		}
		_tempFileStream = File.Open(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
	}

	internal ByteRangeDownloader(Uri requestedUri, Stream tempStream, SafeWaitHandle eventHandle, Mutex fileMutex)
		: this(requestedUri, eventHandle)
	{
		_tempFileStream = tempStream;
		_fileMutex = fileMutex;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		lock (_syncObject)
		{
			if (_disposed)
			{
				return;
			}
			try
			{
				if (FileMutex == null && _tempFileStream != null)
				{
					_tempFileStream.Close();
				}
			}
			finally
			{
				_requestedUri = null;
				_byteRangesInProgress = null;
				_requestsOnWait = null;
				_byteRangesAvailable = null;
				_tempFileStream = null;
				_eventHandle = null;
				_proxy = null;
				_credentials = null;
				_cachePolicy = null;
				_disposed = true;
			}
		}
	}

	internal int[,] GetDownloadedByteRanges()
	{
		int[,] array = null;
		CheckDisposed();
		lock (_syncObject)
		{
			CheckErroredOutCondition();
			int num = _byteRangesAvailable.Count / 2;
			array = new int[num, 2];
			for (int i = 0; i < num; i++)
			{
				array[i, 0] = (int)_byteRangesAvailable[i * 2];
				array[i, 1] = (int)_byteRangesAvailable[i * 2 + 1];
			}
			_byteRangesAvailable.Clear();
			return array;
		}
	}

	internal void RequestByteRanges(int[,] byteRanges)
	{
		CheckDisposed();
		if (byteRanges == null)
		{
			throw new ArgumentNullException("byteRanges");
		}
		CheckTwoDimensionalByteRanges(byteRanges);
		_firstRequestMade = true;
		lock (_syncObject)
		{
			CheckErroredOutCondition();
			if (_byteRangesInProgress == null)
			{
				_webRequest = CreateHttpWebRequest(byteRanges);
				_byteRangesInProgress = byteRanges;
				_webRequest.BeginGetResponse(ResponseCallback, this);
				return;
			}
			if (_requestsOnWait == null)
			{
				_requestsOnWait = new ArrayList(2);
			}
			for (int i = 0; i < byteRanges.GetLength(0); i++)
			{
				_requestsOnWait.Add(byteRanges[i, 0]);
				_requestsOnWait.Add(byteRanges[i, 1]);
			}
		}
	}

	internal static int[,] ConvertByteRanges(int[] inByteRanges)
	{
		CheckOneDimensionalByteRanges(inByteRanges);
		int[,] array = new int[inByteRanges.Length / 2, 2];
		int num = 0;
		int num2 = 0;
		while (num < inByteRanges.Length)
		{
			array[num2, 0] = inByteRanges[num];
			array[num2, 1] = inByteRanges[num + 1];
			num++;
			num++;
			num2++;
		}
		return array;
	}

	internal static int[] ConvertByteRanges(int[,] inByteRanges)
	{
		int[] array = new int[inByteRanges.Length];
		int num = 0;
		int num2 = 0;
		while (num < inByteRanges.GetLength(0))
		{
			array[num2] = inByteRanges[num, 0];
			array[++num2] = inByteRanges[num, 1];
			num++;
			num2++;
		}
		return array;
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.ByteRangeDownloaderDisposed);
		}
	}

	private ByteRangeDownloader(Uri requestedUri, SafeWaitHandle eventHandle)
	{
		if (requestedUri == null)
		{
			throw new ArgumentNullException("requestedUri");
		}
		if (!string.Equals(requestedUri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) && !string.Equals(requestedUri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
		{
			throw new ArgumentException(SR.InvalidScheme, "requestedUri");
		}
		if (eventHandle == null)
		{
			throw new ArgumentNullException("eventHandle");
		}
		if (eventHandle.IsInvalid || eventHandle.IsClosed)
		{
			throw new ArgumentException(SR.InvalidEventHandle, "eventHandle");
		}
		_requestedUri = requestedUri;
		_eventHandle = eventHandle;
	}

	private void CheckErroredOutCondition()
	{
		if (_erroredOut)
		{
			throw new InvalidOperationException(SR.ByteRangeDownloaderErroredOut, _erroredOutException);
		}
	}

	private HttpWebRequest CreateHttpWebRequest(int[,] byteRanges)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WpfWebRequestHelper.CreateRequest(_requestedUri);
		httpWebRequest.ProtocolVersion = HttpVersion.Version11;
		httpWebRequest.Method = "GET";
		httpWebRequest.Proxy = _proxy;
		httpWebRequest.Credentials = _credentials;
		httpWebRequest.CachePolicy = _cachePolicy;
		for (int i = 0; i < byteRanges.GetLength(0); i++)
		{
			httpWebRequest.AddRange(byteRanges[i, 0], byteRanges[i, 0] + byteRanges[i, 1] - 1);
		}
		return httpWebRequest;
	}

	private void RaiseEvent(bool throwExceptionOnError)
	{
		if (_eventHandle != null && !_eventHandle.IsInvalid && !_eventHandle.IsClosed && MS.Win32.UnsafeNativeMethods.SetEvent(_eventHandle) == 0 && throwExceptionOnError)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	private void ResponseCallback(IAsyncResult ar)
	{
		HttpWebResponse httpWebResponse = null;
		lock (_syncObject)
		{
			try
			{
				if (_disposed)
				{
					return;
				}
				httpWebResponse = (HttpWebResponse)WpfWebRequestHelper.EndGetResponse(_webRequest, ar);
				if (httpWebResponse.StatusCode == HttpStatusCode.PartialContent)
				{
					int num = _byteRangesInProgress[0, 0];
					int endOffset = num + _byteRangesInProgress[0, 1] - 1;
					if (CheckContentRange(httpWebResponse.Headers, num, ref endOffset))
					{
						if (WriteByteRange(httpWebResponse, num, endOffset - num + 1))
						{
							_byteRangesAvailable.Add(num);
							_byteRangesAvailable.Add(endOffset - num + 1);
						}
						else
						{
							_erroredOut = true;
						}
					}
					else
					{
						_erroredOut = true;
						_erroredOutException = new NotSupportedException(SR.ByteRangeRequestIsNotSupported);
					}
				}
				else
				{
					_erroredOut = true;
				}
			}
			catch (Exception erroredOutException)
			{
				_erroredOut = true;
				_erroredOutException = erroredOutException;
				throw;
			}
			catch
			{
				_erroredOut = true;
				_erroredOutException = null;
				throw;
			}
			finally
			{
				httpWebResponse?.Close();
				RaiseEvent(!_erroredOut);
			}
			if (!_erroredOut)
			{
				ProcessWaitQueue();
			}
		}
	}

	private bool Write(Stream s, int offset, int length)
	{
		_tempFileStream.Seek(offset, SeekOrigin.Begin);
		while (length > 0)
		{
			int num = s.Read(_buffer, 0, 4096);
			if (num == 0)
			{
				break;
			}
			_tempFileStream.Write(_buffer, 0, num);
			length -= num;
		}
		if (length != 0)
		{
			return false;
		}
		_tempFileStream.Flush();
		return true;
	}

	private bool WriteByteRange(HttpWebResponse response, int offset, int length)
	{
		bool result = false;
		using (Stream s = response.GetResponseStream())
		{
			if (_buffer == null)
			{
				_buffer = new byte[4096];
			}
			if (_fileMutex != null)
			{
				try
				{
					_fileMutex.WaitOne();
					lock (PackagingUtilities.IsolatedStorageFileLock)
					{
						result = Write(s, offset, length);
					}
				}
				finally
				{
					_fileMutex.ReleaseMutex();
				}
			}
			else
			{
				result = Write(s, offset, length);
			}
		}
		return result;
	}

	private void ProcessWaitQueue()
	{
		if (_requestsOnWait != null && _requestsOnWait.Count > 0)
		{
			_byteRangesInProgress[0, 0] = (int)_requestsOnWait[0];
			_byteRangesInProgress[0, 1] = (int)_requestsOnWait[1];
			_requestsOnWait.RemoveRange(0, 2);
			_webRequest = CreateHttpWebRequest(_byteRangesInProgress);
			_webRequest.BeginGetResponse(ResponseCallback, this);
		}
		else
		{
			_byteRangesInProgress = null;
		}
	}

	private static void CheckOneDimensionalByteRanges(int[] byteRanges)
	{
		if (byteRanges.Length < 2 || byteRanges.Length % 2 != 0)
		{
			throw new ArgumentException(SR.Format(SR.InvalidByteRanges, "byteRanges"));
		}
		int num;
		for (num = 0; num < byteRanges.Length; num++)
		{
			if (byteRanges[num] < 0 || byteRanges[num + 1] <= 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidByteRanges, "byteRanges"));
			}
			num++;
		}
	}

	private static void CheckTwoDimensionalByteRanges(int[,] byteRanges)
	{
		if (byteRanges.GetLength(0) <= 0 || byteRanges.GetLength(1) != 2)
		{
			throw new ArgumentException(SR.Format(SR.InvalidByteRanges, "byteRanges"));
		}
		for (int i = 0; i < byteRanges.GetLength(0); i++)
		{
			if (byteRanges[i, 0] < 0 || byteRanges[i, 1] <= 0)
			{
				throw new ArgumentException(SR.Format(SR.InvalidByteRanges, "byteRanges"));
			}
		}
	}

	private static bool CheckContentRange(WebHeaderCollection responseHeaders, int beginOffset, ref int endOffset)
	{
		string text = responseHeaders["Content-Range"];
		if (text == null)
		{
			return false;
		}
		text = text.ToUpperInvariant();
		if (text.Length == 0 || !text.StartsWith("BYTES ", StringComparison.Ordinal))
		{
			return false;
		}
		int num = text.IndexOf('-');
		if (num == -1)
		{
			return false;
		}
		int num2 = int.Parse(text.AsSpan("BYTES ".Length, num - "BYTES ".Length), NumberStyles.None, NumberFormatInfo.InvariantInfo);
		ReadOnlySpan<char> readOnlySpan = text.AsSpan(num + 1);
		num = readOnlySpan.IndexOf('/');
		if (num == -1)
		{
			return false;
		}
		int num3 = int.Parse(readOnlySpan.Slice(0, num), NumberStyles.None, NumberFormatInfo.InvariantInfo);
		readOnlySpan = readOnlySpan.Slice(num + 1);
		if (!MemoryExtensions.Equals(readOnlySpan, "*", StringComparison.Ordinal))
		{
			int.Parse(readOnlySpan, NumberStyles.None, NumberFormatInfo.InvariantInfo);
		}
		int num4;
		if (num2 <= num3)
		{
			num4 = ((beginOffset == num2) ? 1 : 0);
			if (num4 != 0 && num3 < endOffset)
			{
				endOffset = num3;
			}
		}
		else
		{
			num4 = 0;
		}
		return (byte)num4 != 0;
	}
}

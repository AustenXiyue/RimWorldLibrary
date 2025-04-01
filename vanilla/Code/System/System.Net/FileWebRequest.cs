using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Net;

/// <summary>Provides a file system implementation of the <see cref="T:System.Net.WebRequest" /> class.</summary>
[Serializable]
public class FileWebRequest : WebRequest, ISerializable
{
	private static WaitCallback s_GetRequestStreamCallback = GetRequestStreamCallback;

	private static WaitCallback s_GetResponseCallback = GetResponseCallback;

	private string m_connectionGroupName;

	private long m_contentLength;

	private ICredentials m_credentials;

	private FileAccess m_fileAccess;

	private WebHeaderCollection m_headers;

	private string m_method = "GET";

	private bool m_preauthenticate;

	private IWebProxy m_proxy;

	private ManualResetEvent m_readerEvent;

	private bool m_readPending;

	private WebResponse m_response;

	private Stream m_stream;

	private bool m_syncHint;

	private int m_timeout = 100000;

	private Uri m_uri;

	private bool m_writePending;

	private bool m_writing;

	private LazyAsyncResult m_WriteAResult;

	private LazyAsyncResult m_ReadAResult;

	private int m_Aborted;

	internal bool Aborted => m_Aborted != 0;

	/// <summary>Gets or sets the name of the connection group for the request. This property is reserved for future use.</summary>
	/// <returns>The name of the connection group for the request.</returns>
	public override string ConnectionGroupName
	{
		get
		{
			return m_connectionGroupName;
		}
		set
		{
			m_connectionGroupName = value;
		}
	}

	/// <summary>Gets or sets the content length of the data being sent.</summary>
	/// <returns>The number of bytes of request data being sent.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.FileWebRequest.ContentLength" /> is less than 0. </exception>
	public override long ContentLength
	{
		get
		{
			return m_contentLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException(global::SR.GetString("The Content-Length value must be greater than or equal to zero."), "value");
			}
			m_contentLength = value;
		}
	}

	/// <summary>Gets or sets the content type of the data being sent. This property is reserved for future use.</summary>
	/// <returns>The content type of the data being sent.</returns>
	public override string ContentType
	{
		get
		{
			return m_headers["Content-Type"];
		}
		set
		{
			m_headers["Content-Type"] = value;
		}
	}

	/// <summary>Gets or sets the credentials that are associated with this request. This property is reserved for future use.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> that contains the authentication credentials that are associated with this request. The default is null.</returns>
	public override ICredentials Credentials
	{
		get
		{
			return m_credentials;
		}
		set
		{
			m_credentials = value;
		}
	}

	/// <summary>Gets a collection of the name/value pairs that are associated with the request. This property is reserved for future use.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains header name/value pairs associated with this request.</returns>
	public override WebHeaderCollection Headers => m_headers;

	/// <summary>Gets or sets the protocol method used for the request. This property is reserved for future use.</summary>
	/// <returns>The protocol method to use in this request.</returns>
	/// <exception cref="T:System.ArgumentException">The method is invalid.- or -The method is not supported.- or -Multiple methods were specified.</exception>
	public override string Method
	{
		get
		{
			return m_method;
		}
		set
		{
			if (ValidationHelper.IsBlankString(value))
			{
				throw new ArgumentException(global::SR.GetString("Cannot set null or blank methods on request."), "value");
			}
			m_method = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to preauthenticate a request. This property is reserved for future use.</summary>
	/// <returns>true to preauthenticate; otherwise, false.</returns>
	public override bool PreAuthenticate
	{
		get
		{
			return m_preauthenticate;
		}
		set
		{
			m_preauthenticate = true;
		}
	}

	/// <summary>Gets or sets the network proxy to use for this request. This property is reserved for future use.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> that indicates the network proxy to use for this request.</returns>
	public override IWebProxy Proxy
	{
		get
		{
			return m_proxy;
		}
		set
		{
			m_proxy = value;
		}
	}

	/// <summary>Gets or sets the length of time until the request times out.</summary>
	/// <returns>The time, in milliseconds, until the request times out, or the value <see cref="F:System.Threading.Timeout.Infinite" /> to indicate that the request does not time out.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than or equal to zero and is not <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
	public override int Timeout
	{
		get
		{
			return m_timeout;
		}
		set
		{
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("value", global::SR.GetString("Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value >= 0."));
			}
			m_timeout = value;
		}
	}

	/// <summary>Gets the Uniform Resource Identifier (URI) of the request.</summary>
	/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the request.</returns>
	public override Uri RequestUri => m_uri;

	/// <summary>Always throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Always throws a <see cref="T:System.NotSupportedException" />.</returns>
	/// <exception cref="T:System.NotSupportedException">Default credentials are not supported for file Uniform Resource Identifiers (URIs).</exception>
	public override bool UseDefaultCredentials
	{
		get
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotSupportedException;
		}
	}

	internal FileWebRequest(Uri uri)
	{
		if ((object)uri.Scheme != Uri.UriSchemeFile)
		{
			throw new ArgumentOutOfRangeException("uri");
		}
		m_uri = uri;
		m_fileAccess = FileAccess.Read;
		m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebRequest);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.FileWebRequest" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information that is required to serialize the new <see cref="T:System.Net.FileWebRequest" /> object. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source of the serialized stream that is associated with the new <see cref="T:System.Net.FileWebRequest" /> object. </param>
	[Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
	protected FileWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		m_headers = (WebHeaderCollection)serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
		m_proxy = (IWebProxy)serializationInfo.GetValue("proxy", typeof(IWebProxy));
		m_uri = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
		m_connectionGroupName = serializationInfo.GetString("connectionGroupName");
		m_method = serializationInfo.GetString("method");
		m_contentLength = serializationInfo.GetInt64("contentLength");
		m_timeout = serializationInfo.GetInt32("timeout");
		m_fileAccess = (FileAccess)serializationInfo.GetInt32("fileAccess");
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the required data to serialize the <see cref="T:System.Net.FileWebRequest" />.</summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized data for the <see cref="T:System.Net.FileWebRequest" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the destination of the serialized stream that is associated with the new <see cref="T:System.Net.FileWebRequest" />. </param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" />  that specifies the destination for this serialization. </param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		serializationInfo.AddValue("headers", m_headers, typeof(WebHeaderCollection));
		serializationInfo.AddValue("proxy", m_proxy, typeof(IWebProxy));
		serializationInfo.AddValue("uri", m_uri, typeof(Uri));
		serializationInfo.AddValue("connectionGroupName", m_connectionGroupName);
		serializationInfo.AddValue("method", m_method);
		serializationInfo.AddValue("contentLength", m_contentLength);
		serializationInfo.AddValue("timeout", m_timeout);
		serializationInfo.AddValue("fileAccess", m_fileAccess);
		serializationInfo.AddValue("preauthenticate", value: false);
		base.GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Begins an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.FileWebRequest.Method" /> property is GET and the application writes to the stream. </exception>
	/// <exception cref="T:System.InvalidOperationException">The stream is being used by a previous call to <see cref="M:System.Net.FileWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.ApplicationException">No write stream is available. </exception>
	/// <exception cref="T:System.Net.WebException">The <see cref="T:System.Net.FileWebRequest" /> was aborted. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
	{
		try
		{
			if (Aborted)
			{
				throw ExceptionHelper.RequestAbortedException;
			}
			if (!CanGetRequestStream())
			{
				throw new ProtocolViolationException(global::SR.GetString("Cannot send a content-body with this verb-type."));
			}
			if (m_response != null)
			{
				throw new InvalidOperationException(global::SR.GetString("This operation cannot be performed after the request has been submitted."));
			}
			lock (this)
			{
				if (m_writePending)
				{
					throw new InvalidOperationException(global::SR.GetString("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress."));
				}
				m_writePending = true;
			}
			m_ReadAResult = new LazyAsyncResult(this, state, callback);
			ThreadPool.QueueUserWorkItem(s_GetRequestStreamCallback, m_ReadAResult);
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
		return m_ReadAResult;
	}

	/// <summary>Begins an asynchronous request for a file system resource.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
	/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
	/// <param name="state">An object that contains state information for this request. </param>
	/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.FileWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
	/// <exception cref="T:System.Net.WebException">The <see cref="T:System.Net.FileWebRequest" /> was aborted. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
	{
		try
		{
			if (Aborted)
			{
				throw ExceptionHelper.RequestAbortedException;
			}
			lock (this)
			{
				if (m_readPending)
				{
					throw new InvalidOperationException(global::SR.GetString("Cannot re-call BeginGetRequestStream/BeginGetResponse while a previous call is still in progress."));
				}
				m_readPending = true;
			}
			m_WriteAResult = new LazyAsyncResult(this, state, callback);
			ThreadPool.QueueUserWorkItem(s_GetResponseCallback, m_WriteAResult);
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
		return m_WriteAResult;
	}

	private bool CanGetRequestStream()
	{
		return !KnownHttpVerb.Parse(m_method).ContentBodyNotAllowed;
	}

	/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> instance that the application uses to write data.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> object that the application uses to write data.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that references the pending request for a stream. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	public override Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		try
		{
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (asyncResult == null || lazyAsyncResult == null)
			{
				throw (asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(global::SR.GetString("The AsyncResult is not valid."), "asyncResult");
			}
			object obj = lazyAsyncResult.InternalWaitForCompletion();
			if (obj is Exception)
			{
				throw (Exception)obj;
			}
			Stream result = (Stream)obj;
			m_writePending = false;
			return result;
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
	}

	/// <summary>Ends an asynchronous request for a file system resource.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the file system resource.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> that references the pending request for a response. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	public override WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		try
		{
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (asyncResult == null || lazyAsyncResult == null)
			{
				throw (asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(global::SR.GetString("The AsyncResult is not valid."), "asyncResult");
			}
			object obj = lazyAsyncResult.InternalWaitForCompletion();
			if (obj is Exception)
			{
				throw (Exception)obj;
			}
			WebResponse result = (WebResponse)obj;
			m_readPending = false;
			return result;
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
	}

	/// <summary>Returns a <see cref="T:System.IO.Stream" /> object for writing data to the file system resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> for writing data to the file system resource.</returns>
	/// <exception cref="T:System.Net.WebException">The request times out. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override Stream GetRequestStream()
	{
		IAsyncResult asyncResult;
		try
		{
			asyncResult = BeginGetRequestStream(null, null);
			if (Timeout != -1 && !asyncResult.IsCompleted && (!asyncResult.AsyncWaitHandle.WaitOne(Timeout, exitContext: false) || !asyncResult.IsCompleted))
			{
				if (m_stream != null)
				{
					m_stream.Close();
				}
				throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
			}
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
		return EndGetRequestStream(asyncResult);
	}

	/// <summary>Returns a response to a file system request.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the file system resource.</returns>
	/// <exception cref="T:System.Net.WebException">The request timed out. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override WebResponse GetResponse()
	{
		m_syncHint = true;
		IAsyncResult asyncResult;
		try
		{
			asyncResult = BeginGetResponse(null, null);
			if (Timeout != -1 && !asyncResult.IsCompleted && (!asyncResult.AsyncWaitHandle.WaitOne(Timeout, exitContext: false) || !asyncResult.IsCompleted))
			{
				if (m_response != null)
				{
					m_response.Close();
				}
				throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
			}
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
		return EndGetResponse(asyncResult);
	}

	private static void GetRequestStreamCallback(object state)
	{
		LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)state;
		FileWebRequest fileWebRequest = (FileWebRequest)lazyAsyncResult.AsyncObject;
		try
		{
			if (fileWebRequest.m_stream == null)
			{
				fileWebRequest.m_stream = new FileWebStream(fileWebRequest, fileWebRequest.m_uri.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				fileWebRequest.m_fileAccess = FileAccess.Write;
				fileWebRequest.m_writing = true;
			}
		}
		catch (Exception ex)
		{
			Exception result = new WebException(ex.Message, ex);
			lazyAsyncResult.InvokeCallback(result);
			return;
		}
		lazyAsyncResult.InvokeCallback(fileWebRequest.m_stream);
	}

	private static void GetResponseCallback(object state)
	{
		LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)state;
		FileWebRequest fileWebRequest = (FileWebRequest)lazyAsyncResult.AsyncObject;
		if (fileWebRequest.m_writePending || fileWebRequest.m_writing)
		{
			lock (fileWebRequest)
			{
				if (fileWebRequest.m_writePending || fileWebRequest.m_writing)
				{
					fileWebRequest.m_readerEvent = new ManualResetEvent(initialState: false);
				}
			}
		}
		if (fileWebRequest.m_readerEvent != null)
		{
			fileWebRequest.m_readerEvent.WaitOne();
		}
		try
		{
			if (fileWebRequest.m_response == null)
			{
				fileWebRequest.m_response = new FileWebResponse(fileWebRequest, fileWebRequest.m_uri, fileWebRequest.m_fileAccess, !fileWebRequest.m_syncHint);
			}
		}
		catch (Exception ex)
		{
			Exception result = new WebException(ex.Message, ex);
			lazyAsyncResult.InvokeCallback(result);
			return;
		}
		lazyAsyncResult.InvokeCallback(fileWebRequest.m_response);
	}

	internal void UnblockReader()
	{
		lock (this)
		{
			if (m_readerEvent != null)
			{
				m_readerEvent.Set();
			}
		}
		m_writing = false;
	}

	/// <summary>Cancels a request to an Internet resource.</summary>
	public override void Abort()
	{
		_ = Logging.On;
		try
		{
			if (Interlocked.Increment(ref m_Aborted) != 1)
			{
				return;
			}
			LazyAsyncResult readAResult = m_ReadAResult;
			LazyAsyncResult writeAResult = m_WriteAResult;
			WebException result = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
			Stream stream = m_stream;
			if (readAResult != null && !readAResult.IsCompleted)
			{
				readAResult.InvokeCallback(result);
			}
			if (writeAResult != null && !writeAResult.IsCompleted)
			{
				writeAResult.InvokeCallback(result);
			}
			if (stream != null)
			{
				if (stream is ICloseEx)
				{
					((ICloseEx)stream).CloseEx(CloseExState.Abort);
				}
				else
				{
					stream.Close();
				}
			}
			if (m_response != null)
			{
				((ICloseEx)m_response).CloseEx(CloseExState.Abort);
			}
		}
		catch (Exception)
		{
			_ = Logging.On;
			throw;
		}
		finally
		{
		}
	}
}

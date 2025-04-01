using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net;

/// <summary>Provides common methods for sending data to and receiving data from a resource identified by a URI.</summary>
[ComVisible(true)]
public class WebClient : Component
{
	private class ProgressData
	{
		internal long BytesSent;

		internal long TotalBytesToSend = -1L;

		internal long BytesReceived;

		internal long TotalBytesToReceive = -1L;

		internal bool HasUploadPhase;

		internal void Reset()
		{
			BytesSent = 0L;
			TotalBytesToSend = -1L;
			BytesReceived = 0L;
			TotalBytesToReceive = -1L;
			HasUploadPhase = false;
		}
	}

	private class DownloadBitsState
	{
		internal WebClient WebClient;

		internal Stream WriteStream;

		internal byte[] InnerBuffer;

		internal AsyncOperation AsyncOp;

		internal WebRequest Request;

		internal CompletionDelegate CompletionDelegate;

		internal Stream ReadStream;

		internal ScatterGatherBuffers SgBuffers;

		internal long ContentLength;

		internal long Length;

		private const int Offset = 0;

		internal ProgressData Progress;

		internal bool Async => AsyncOp != null;

		internal DownloadBitsState(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient)
		{
			WriteStream = writeStream;
			Request = request;
			AsyncOp = asyncOp;
			CompletionDelegate = completionDelegate;
			WebClient = webClient;
			Progress = progress;
		}

		internal int SetResponse(WebResponse response)
		{
			ContentLength = response.ContentLength;
			if (ContentLength == -1 || ContentLength > 65536)
			{
				Length = 65536L;
			}
			else
			{
				Length = ContentLength;
			}
			if (WriteStream == null)
			{
				if (ContentLength > int.MaxValue)
				{
					throw new WebException(global::SR.GetString("The message length limit was exceeded"), WebExceptionStatus.MessageLengthLimitExceeded);
				}
				SgBuffers = new ScatterGatherBuffers(Length);
			}
			InnerBuffer = new byte[(int)Length];
			ReadStream = response.GetResponseStream();
			if (Async && response.ContentLength >= 0)
			{
				Progress.TotalBytesToReceive = response.ContentLength;
			}
			if (Async)
			{
				if (ReadStream == null || ReadStream == Stream.Null)
				{
					DownloadBitsReadCallbackState(this, null);
				}
				else
				{
					ReadStream.BeginRead(InnerBuffer, 0, (int)Length, DownloadBitsReadCallback, this);
				}
				return -1;
			}
			if (ReadStream == null || ReadStream == Stream.Null)
			{
				return 0;
			}
			return ReadStream.Read(InnerBuffer, 0, (int)Length);
		}

		internal bool RetrieveBytes(ref int bytesRetrieved)
		{
			if (bytesRetrieved > 0)
			{
				if (WriteStream != null)
				{
					WriteStream.Write(InnerBuffer, 0, bytesRetrieved);
				}
				else
				{
					SgBuffers.Write(InnerBuffer, 0, bytesRetrieved);
				}
				if (Async)
				{
					Progress.BytesReceived += bytesRetrieved;
				}
				if (ContentLength != 0L)
				{
					if (Async)
					{
						WebClient.PostProgressChanged(AsyncOp, Progress);
						ReadStream.BeginRead(InnerBuffer, 0, (int)Length, DownloadBitsReadCallback, this);
					}
					else
					{
						bytesRetrieved = ReadStream.Read(InnerBuffer, 0, (int)Length);
					}
					return false;
				}
			}
			if (Async)
			{
				if (Progress.TotalBytesToReceive < 0)
				{
					Progress.TotalBytesToReceive = Progress.BytesReceived;
				}
				WebClient.PostProgressChanged(AsyncOp, Progress);
			}
			if (ReadStream != null)
			{
				ReadStream.Close();
			}
			if (WriteStream != null)
			{
				WriteStream.Close();
			}
			else if (WriteStream == null)
			{
				byte[] array = new byte[SgBuffers.Length];
				if (SgBuffers.Length > 0)
				{
					BufferOffsetSize[] buffers = SgBuffers.GetBuffers();
					int num = 0;
					foreach (BufferOffsetSize bufferOffsetSize in buffers)
					{
						Buffer.BlockCopy(bufferOffsetSize.Buffer, 0, array, num, bufferOffsetSize.Size);
						num += bufferOffsetSize.Size;
					}
				}
				InnerBuffer = array;
			}
			return true;
		}

		internal void Close()
		{
			if (WriteStream != null)
			{
				WriteStream.Close();
			}
			if (ReadStream != null)
			{
				ReadStream.Close();
			}
		}
	}

	private class UploadBitsState
	{
		private int m_ChunkSize;

		private int m_BufferWritePosition;

		internal WebClient WebClient;

		internal Stream WriteStream;

		internal byte[] InnerBuffer;

		internal byte[] Header;

		internal byte[] Footer;

		internal AsyncOperation AsyncOp;

		internal WebRequest Request;

		internal CompletionDelegate UploadCompletionDelegate;

		internal CompletionDelegate DownloadCompletionDelegate;

		internal Stream ReadStream;

		internal ProgressData Progress;

		internal bool FileUpload => ReadStream != null;

		internal bool Async => AsyncOp != null;

		internal UploadBitsState(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient)
		{
			InnerBuffer = buffer;
			m_ChunkSize = chunkSize;
			m_BufferWritePosition = 0;
			Header = header;
			Footer = footer;
			ReadStream = readStream;
			Request = request;
			AsyncOp = asyncOp;
			UploadCompletionDelegate = uploadCompletionDelegate;
			DownloadCompletionDelegate = downloadCompletionDelegate;
			if (AsyncOp != null)
			{
				Progress = progress;
				Progress.HasUploadPhase = true;
				Progress.TotalBytesToSend = ((request.ContentLength < 0) ? (-1) : request.ContentLength);
			}
			WebClient = webClient;
		}

		internal void SetRequestStream(Stream writeStream)
		{
			WriteStream = writeStream;
			byte[] array = null;
			if (Header != null)
			{
				array = Header;
				Header = null;
			}
			else
			{
				array = new byte[0];
			}
			if (Async)
			{
				Progress.BytesSent += array.Length;
				WriteStream.BeginWrite(array, 0, array.Length, UploadBitsWriteCallback, this);
			}
			else
			{
				WriteStream.Write(array, 0, array.Length);
			}
		}

		internal bool WriteBytes()
		{
			byte[] array = null;
			int num = 0;
			int num2 = 0;
			if (Async)
			{
				WebClient.PostProgressChanged(AsyncOp, Progress);
			}
			if (FileUpload)
			{
				int num3 = 0;
				if (InnerBuffer != null)
				{
					num3 = ReadStream.Read(InnerBuffer, 0, InnerBuffer.Length);
					if (num3 <= 0)
					{
						ReadStream.Close();
						InnerBuffer = null;
					}
				}
				if (InnerBuffer != null)
				{
					num = num3;
					array = InnerBuffer;
				}
				else
				{
					if (Footer == null)
					{
						return true;
					}
					num = Footer.Length;
					array = Footer;
					Footer = null;
				}
			}
			else
			{
				if (InnerBuffer == null)
				{
					return true;
				}
				array = InnerBuffer;
				if (m_ChunkSize != 0)
				{
					num2 = m_BufferWritePosition;
					m_BufferWritePosition += m_ChunkSize;
					num = m_ChunkSize;
					if (m_BufferWritePosition >= InnerBuffer.Length)
					{
						num = InnerBuffer.Length - num2;
						InnerBuffer = null;
					}
				}
				else
				{
					num = InnerBuffer.Length;
					InnerBuffer = null;
				}
			}
			if (Async)
			{
				Progress.BytesSent += num;
				WriteStream.BeginWrite(array, num2, num, UploadBitsWriteCallback, this);
			}
			else
			{
				WriteStream.Write(array, 0, num);
			}
			return false;
		}

		internal void Close()
		{
			if (WriteStream != null)
			{
				WriteStream.Close();
			}
			if (ReadStream != null)
			{
				ReadStream.Close();
			}
		}
	}

	private class WebClientWriteStream : Stream
	{
		private WebRequest m_request;

		private Stream m_stream;

		private WebClient m_WebClient;

		public override bool CanRead => m_stream.CanRead;

		public override bool CanSeek => m_stream.CanSeek;

		public override bool CanWrite => m_stream.CanWrite;

		public override bool CanTimeout => m_stream.CanTimeout;

		public override int ReadTimeout
		{
			get
			{
				return m_stream.ReadTimeout;
			}
			set
			{
				m_stream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return m_stream.WriteTimeout;
			}
			set
			{
				m_stream.WriteTimeout = value;
			}
		}

		public override long Length => m_stream.Length;

		public override long Position
		{
			get
			{
				return m_stream.Position;
			}
			set
			{
				m_stream.Position = value;
			}
		}

		public WebClientWriteStream(Stream stream, WebRequest request, WebClient webClient)
		{
			m_request = request;
			m_stream = stream;
			m_WebClient = webClient;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return m_stream.BeginRead(buffer, offset, size, callback, state);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return m_stream.BeginWrite(buffer, offset, size, callback, state);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					m_stream.Close();
					m_WebClient.GetWebResponse(m_request).Close();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override int EndRead(IAsyncResult result)
		{
			return m_stream.EndRead(result);
		}

		public override void EndWrite(IAsyncResult result)
		{
			m_stream.EndWrite(result);
		}

		public override void Flush()
		{
			m_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return m_stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return m_stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			m_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			m_stream.Write(buffer, offset, count);
		}
	}

	private const int DefaultCopyBufferLength = 8192;

	private const int DefaultDownloadBufferLength = 65536;

	private const string DefaultUploadFileContentType = "application/octet-stream";

	private const string UploadFileContentType = "multipart/form-data";

	private const string UploadValuesContentType = "application/x-www-form-urlencoded";

	private Uri m_baseAddress;

	private ICredentials m_credentials;

	private WebHeaderCollection m_headers;

	private NameValueCollection m_requestParameters;

	private WebResponse m_WebResponse;

	private WebRequest m_WebRequest;

	private Encoding m_Encoding = Encoding.Default;

	private string m_Method;

	private long m_ContentLength = -1L;

	private bool m_InitWebClientAsync;

	private bool m_Cancelled;

	private ProgressData m_Progress;

	private IWebProxy m_Proxy;

	private bool m_ProxySet;

	private RequestCachePolicy m_CachePolicy;

	private int m_CallNesting;

	private AsyncOperation m_AsyncOp;

	private SendOrPostCallback openReadOperationCompleted;

	private SendOrPostCallback openWriteOperationCompleted;

	private SendOrPostCallback downloadStringOperationCompleted;

	private SendOrPostCallback downloadDataOperationCompleted;

	private SendOrPostCallback downloadFileOperationCompleted;

	private SendOrPostCallback uploadStringOperationCompleted;

	private SendOrPostCallback uploadDataOperationCompleted;

	private SendOrPostCallback uploadFileOperationCompleted;

	private SendOrPostCallback uploadValuesOperationCompleted;

	private SendOrPostCallback reportDownloadProgressChanged;

	private SendOrPostCallback reportUploadProgressChanged;

	/// <summary>Gets or sets a value that indicates whether to buffer the data read from the Internet resource for a <see cref="T:System.Net.WebClient" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data received from the Internet resource; false to disable buffering. The default is true.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public bool AllowReadStreamBuffering { get; set; }

	/// <summary>Gets or sets a value that indicates whether to buffer the data written to the Internet resource for a <see cref="T:System.Net.WebClient" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true to enable buffering of the data written to the Internet resource; false to disable buffering. The default is true.</returns>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool AllowWriteStreamBuffering { get; set; }

	/// <summary>Gets and sets the <see cref="T:System.Text.Encoding" /> used to upload and download strings.</summary>
	/// <returns>A <see cref="T:System.Text.Encoding" /> that is used to encode strings. The default value of this property is the encoding returned by <see cref="P:System.Text.Encoding.Default" />.</returns>
	public Encoding Encoding
	{
		get
		{
			return m_Encoding;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Encoding");
			}
			m_Encoding = value;
		}
	}

	/// <summary>Gets or sets the base URI for requests made by a <see cref="T:System.Net.WebClient" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the base URI for requests made by a <see cref="T:System.Net.WebClient" /> or <see cref="F:System.String.Empty" /> if no base address has been specified.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebClient.BaseAddress" /> is set to an invalid URI. The inner exception may contain information that will help you locate the error.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string BaseAddress
	{
		get
		{
			if (!(m_baseAddress == null))
			{
				return m_baseAddress.ToString();
			}
			return string.Empty;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				m_baseAddress = null;
				return;
			}
			try
			{
				m_baseAddress = new Uri(value);
			}
			catch (UriFormatException innerException)
			{
				throw new ArgumentException(global::SR.GetString("The specified value is not a valid base address."), "value", innerException);
			}
		}
	}

	/// <summary>Gets or sets the network credentials that are sent to the host and used to authenticate the request.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> containing the authentication credentials for the request. The default is null.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ICredentials Credentials
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

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether the <see cref="P:System.Net.CredentialCache.DefaultCredentials" /> are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise false. The default value is false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public bool UseDefaultCredentials
	{
		get
		{
			if (!(m_credentials is SystemNetworkCredential))
			{
				return false;
			}
			return true;
		}
		set
		{
			m_credentials = (value ? CredentialCache.DefaultCredentials : null);
		}
	}

	/// <summary>Gets or sets a collection of header name/value pairs associated with the request.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> containing header name/value pairs associated with this request.</returns>
	public WebHeaderCollection Headers
	{
		get
		{
			if (m_headers == null)
			{
				m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
			}
			return m_headers;
		}
		set
		{
			m_headers = value;
		}
	}

	/// <summary>Gets or sets a collection of query name/value pairs associated with the request.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> that contains query name/value pairs associated with the request. If no pairs are associated with the request, the value is an empty <see cref="T:System.Collections.Specialized.NameValueCollection" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public NameValueCollection QueryString
	{
		get
		{
			if (m_requestParameters == null)
			{
				m_requestParameters = new NameValueCollection();
			}
			return m_requestParameters;
		}
		set
		{
			m_requestParameters = value;
		}
	}

	/// <summary>Gets a collection of header name/value pairs associated with the response.</summary>
	/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> containing header name/value pairs associated with the response, or null if no response has been received.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public WebHeaderCollection ResponseHeaders
	{
		get
		{
			if (m_WebResponse != null)
			{
				return m_WebResponse.Headers;
			}
			return null;
		}
	}

	/// <summary>Gets or sets the proxy used by this <see cref="T:System.Net.WebClient" /> object.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> instance used to send requests.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="P:System.Net.WebClient.Proxy" /> is set to null. </exception>
	public IWebProxy Proxy
	{
		get
		{
			if (!m_ProxySet)
			{
				return WebRequest.InternalDefaultWebProxy;
			}
			return m_Proxy;
		}
		set
		{
			m_Proxy = value;
			m_ProxySet = true;
		}
	}

	/// <summary>Gets or sets the application's cache policy for any resources obtained by this WebClient instance using <see cref="T:System.Net.WebRequest" /> objects.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCachePolicy" /> object that represents the application's caching requirements.</returns>
	public RequestCachePolicy CachePolicy
	{
		get
		{
			return m_CachePolicy;
		}
		set
		{
			m_CachePolicy = value;
		}
	}

	/// <summary>Gets whether a Web request is in progress.</summary>
	/// <returns>true if the Web request is still in progress; otherwise false.</returns>
	public bool IsBusy => m_AsyncOp != null;

	/// <summary>Occurs when an asynchronous operation to write data to a resource using a write stream is closed.</summary>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public event WriteStreamClosedEventHandler WriteStreamClosed
	{
		add
		{
		}
		remove
		{
		}
	}

	/// <summary>Occurs when an asynchronous operation to open a stream containing a resource completes.</summary>
	public event OpenReadCompletedEventHandler OpenReadCompleted;

	/// <summary>Occurs when an asynchronous operation to open a stream to write data to a resource completes.</summary>
	public event OpenWriteCompletedEventHandler OpenWriteCompleted;

	/// <summary>Occurs when an asynchronous resource-download operation completes.</summary>
	public event DownloadStringCompletedEventHandler DownloadStringCompleted;

	/// <summary>Occurs when an asynchronous data download operation completes.</summary>
	public event DownloadDataCompletedEventHandler DownloadDataCompleted;

	/// <summary>Occurs when an asynchronous file download operation completes.</summary>
	public event AsyncCompletedEventHandler DownloadFileCompleted;

	/// <summary>Occurs when an asynchronous string-upload operation completes.</summary>
	public event UploadStringCompletedEventHandler UploadStringCompleted;

	/// <summary>Occurs when an asynchronous data-upload operation completes.</summary>
	public event UploadDataCompletedEventHandler UploadDataCompleted;

	/// <summary>Occurs when an asynchronous file-upload operation completes.</summary>
	public event UploadFileCompletedEventHandler UploadFileCompleted;

	/// <summary>Occurs when an asynchronous upload of a name/value collection completes.</summary>
	public event UploadValuesCompletedEventHandler UploadValuesCompleted;

	/// <summary>Occurs when an asynchronous download operation successfully transfers some or all of the data.</summary>
	public event DownloadProgressChangedEventHandler DownloadProgressChanged;

	/// <summary>Occurs when an asynchronous upload operation successfully transfers some or all of the data.</summary>
	public event UploadProgressChangedEventHandler UploadProgressChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebClient" /> class.</summary>
	public WebClient()
	{
		if (GetType() == typeof(WebClient))
		{
			GC.SuppressFinalize(this);
		}
	}

	private void InitWebClientAsync()
	{
		if (!m_InitWebClientAsync)
		{
			openReadOperationCompleted = OpenReadOperationCompleted;
			openWriteOperationCompleted = OpenWriteOperationCompleted;
			downloadStringOperationCompleted = DownloadStringOperationCompleted;
			downloadDataOperationCompleted = DownloadDataOperationCompleted;
			downloadFileOperationCompleted = DownloadFileOperationCompleted;
			uploadStringOperationCompleted = UploadStringOperationCompleted;
			uploadDataOperationCompleted = UploadDataOperationCompleted;
			uploadFileOperationCompleted = UploadFileOperationCompleted;
			uploadValuesOperationCompleted = UploadValuesOperationCompleted;
			reportDownloadProgressChanged = ReportDownloadProgressChanged;
			reportUploadProgressChanged = ReportUploadProgressChanged;
			m_Progress = new ProgressData();
			m_InitWebClientAsync = true;
		}
	}

	private void ClearWebClientState()
	{
		if (AnotherCallInProgress(Interlocked.Increment(ref m_CallNesting)))
		{
			CompleteWebClientState();
			throw new NotSupportedException(global::SR.GetString("WebClient does not support concurrent I/O operations."));
		}
		m_ContentLength = -1L;
		m_WebResponse = null;
		m_WebRequest = null;
		m_Method = null;
		m_Cancelled = false;
		if (m_Progress != null)
		{
			m_Progress.Reset();
		}
	}

	private void CompleteWebClientState()
	{
		Interlocked.Decrement(ref m_CallNesting);
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.WriteStreamClosed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.WriteStreamClosedEventArgs" />  object containing event data.</param>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected virtual void OnWriteStreamClosed(WriteStreamClosedEventArgs e)
	{
	}

	/// <summary>Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.</summary>
	/// <returns>A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.</returns>
	/// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
	protected virtual WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = WebRequest.Create(address);
		CopyHeadersTo(webRequest);
		if (Credentials != null)
		{
			webRequest.Credentials = Credentials;
		}
		if (m_Method != null)
		{
			webRequest.Method = m_Method;
		}
		if (m_ContentLength != -1)
		{
			webRequest.ContentLength = m_ContentLength;
		}
		if (m_ProxySet)
		{
			webRequest.Proxy = m_Proxy;
		}
		if (m_CachePolicy != null)
		{
			webRequest.CachePolicy = m_CachePolicy;
		}
		return webRequest;
	}

	/// <summary>Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.</returns>
	/// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response. </param>
	protected virtual WebResponse GetWebResponse(WebRequest request)
	{
		return m_WebResponse = request.GetResponse();
	}

	/// <summary>Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" /> using the specified <see cref="T:System.IAsyncResult" />.</summary>
	/// <returns>A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.</returns>
	/// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response.</param>
	/// <param name="result">An <see cref="T:System.IAsyncResult" /> object obtained from a previous call to <see cref="M:System.Net.WebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> .</param>
	protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
	{
		return m_WebResponse = request.EndGetResponse(result);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] DownloadData(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return DownloadData(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI represented by the <see cref="T:System.Uri" />  object, from which to download data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	public byte[] DownloadData(Uri address)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		ClearWebClientState();
		try
		{
			WebRequest request;
			byte[] result = DownloadDataInternal(address, out request);
			_ = Logging.On;
			return result;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	private byte[] DownloadDataInternal(Uri address, out WebRequest request)
	{
		_ = Logging.On;
		request = null;
		try
		{
			request = (m_WebRequest = GetWebRequest(GetUri(address)));
			return DownloadBits(request, null, null, null);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
	}

	/// <summary>Downloads the resource with the specified URI to a local file.</summary>
	/// <param name="address">The URI from which to download data. </param>
	/// <param name="fileName">The name of the local file that is to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="filename" /> is null or <see cref="F:System.String.Empty" />.-or-The file does not exist.-or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DownloadFile(string address, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		DownloadFile(GetUri(address), fileName);
	}

	/// <summary>Downloads the resource with the specified URI to a local file.</summary>
	/// <param name="address">The URI specified as a <see cref="T:System.String" />, from which to download data. </param>
	/// <param name="fileName">The name of the local file that is to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="filename" /> is null or <see cref="F:System.String.Empty" />.-or- The file does not exist. -or- An error occurred while downloading data. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	public void DownloadFile(Uri address, string fileName)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		WebRequest request = null;
		FileStream fileStream = null;
		bool flag = false;
		ClearWebClientState();
		try
		{
			fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			request = (m_WebRequest = GetWebRequest(GetUri(address)));
			DownloadBits(request, fileStream, null, null);
			flag = true;
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
				if (!flag)
				{
					File.Delete(fileName);
				}
				fileStream = null;
			}
			CompleteWebClientState();
		}
		_ = Logging.On;
	}

	/// <summary>Opens a readable stream for the data downloaded from a resource with the URI specified as a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI specified as a <see cref="T:System.String" /> from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenRead(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenRead(GetUri(address));
	}

	/// <summary>Opens a readable stream for the data downloaded from a resource with the URI specified as a <see cref="T:System.Uri" /></summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI specified as a <see cref="T:System.Uri" /> from which to download data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, <paramref name="address" /> is invalid.-or- An error occurred while downloading data. </exception>
	public Stream OpenRead(Uri address)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		WebRequest request = null;
		ClearWebClientState();
		try
		{
			request = (m_WebRequest = GetWebRequest(GetUri(address)));
			Stream responseStream = (m_WebResponse = GetWebResponse(request)).GetResponseStream();
			_ = Logging.On;
			return responseStream;
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	/// <summary>Opens a stream for writing data to the specified resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenWrite(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenWrite(GetUri(address), null);
	}

	/// <summary>Opens a stream for writing data to the specified resource.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Stream OpenWrite(Uri address)
	{
		return OpenWrite(address, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Stream OpenWrite(string address, string method)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return OpenWrite(GetUri(address), method);
	}

	/// <summary>Opens a stream for writing data to the specified resource, by using the specified method.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	public Stream OpenWrite(Uri address, string method)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		WebRequest webRequest = null;
		ClearWebClientState();
		try
		{
			m_Method = method;
			webRequest = (m_WebRequest = GetWebRequest(GetUri(address)));
			WebClientWriteStream result = new WebClientWriteStream(webRequest.GetRequestStream(), webRequest, this);
			_ = Logging.On;
			return result;
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(webRequest);
			throw ex;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null. -or-An error occurred while sending the data.-or- There was no response from the server hosting the resource. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadData(string address, byte[] data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadData(GetUri(address), null, data);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null. -or-An error occurred while sending the data.-or- There was no response from the server hosting the resource. </exception>
	public byte[] UploadData(Uri address, byte[] data)
	{
		return UploadData(address, null, data);
	}

	/// <summary>Uploads a data buffer to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The HTTP method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while uploading the data.-or- There was no response from the server hosting the resource. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadData(string address, string method, byte[] data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadData(GetUri(address), method, data);
	}

	/// <summary>Uploads a data buffer to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The HTTP method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while uploading the data.-or- There was no response from the server hosting the resource. </exception>
	public byte[] UploadData(Uri address, string method, byte[] data)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		ClearWebClientState();
		try
		{
			WebRequest request;
			byte[] result = UploadDataInternal(address, method, data, out request);
			_ = Logging.On;
			return result;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	private byte[] UploadDataInternal(Uri address, string method, byte[] data, out WebRequest request)
	{
		request = null;
		try
		{
			m_Method = method;
			m_ContentLength = data.Length;
			request = (m_WebRequest = GetWebRequest(GetUri(address)));
			UploadBits(request, null, data, 0, null, null, null, null, null);
			return DownloadBits(request, null, null, null);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
	}

	private void OpenFileInternal(bool needsHeaderAndBoundary, string fileName, ref FileStream fs, ref byte[] buffer, ref byte[] formHeaderBytes, ref byte[] boundaryBytes)
	{
		fileName = Path.GetFullPath(fileName);
		if (m_headers == null)
		{
			m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
		}
		string text = m_headers["Content-Type"];
		if (text != null)
		{
			if (text.ToLower(CultureInfo.InvariantCulture).StartsWith("multipart/"))
			{
				throw new WebException(global::SR.GetString("The Content-Type header cannot be set to a multipart type for this request."));
			}
		}
		else
		{
			text = "application/octet-stream";
		}
		fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		int num = 8192;
		m_ContentLength = -1L;
		if (m_Method.ToUpper(CultureInfo.InvariantCulture) == "POST")
		{
			if (needsHeaderAndBoundary)
			{
				string text2 = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
				m_headers["Content-Type"] = "multipart/form-data; boundary=" + text2;
				string s = "--" + text2 + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\nContent-Type: " + text + "\r\n\r\n";
				formHeaderBytes = Encoding.UTF8.GetBytes(s);
				boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + text2 + "--\r\n");
			}
			else
			{
				formHeaderBytes = new byte[0];
				boundaryBytes = new byte[0];
			}
			if (fs.CanSeek)
			{
				m_ContentLength = fs.Length + formHeaderBytes.Length + boundaryBytes.Length;
				num = (int)Math.Min(8192L, fs.Length);
			}
		}
		else
		{
			m_headers["Content-Type"] = text;
			formHeaderBytes = null;
			boundaryBytes = null;
			if (fs.CanSeek)
			{
				m_ContentLength = fs.Length;
				num = (int)Math.Min(8192L, fs.Length);
			}
		}
		buffer = new byte[num];
	}

	/// <summary>Uploads the specified local file to a resource with the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file. For example, ftp://localhost/samplefile.txt.</param>
	/// <param name="fileName">The file to send to the resource. For example, "samplefile.txt".</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadFile(string address, string fileName)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadFile(GetUri(address), fileName);
	}

	/// <summary>Uploads the specified local file to a resource with the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file. For example, ftp://localhost/samplefile.txt.</param>
	/// <param name="fileName">The file to send to the resource. For example, "samplefile.txt".</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public byte[] UploadFile(Uri address, string fileName)
	{
		return UploadFile(address, null, fileName);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file.</param>
	/// <param name="method">The method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadFile(string address, string method, string fileName)
	{
		return UploadFile(GetUri(address), method, fileName);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the file.</param>
	/// <param name="method">The method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid characters, or does not exist.-or- An error occurred while uploading the file.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	public byte[] UploadFile(Uri address, string method, string fileName)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		FileStream fs = null;
		WebRequest request = null;
		ClearWebClientState();
		try
		{
			m_Method = method;
			byte[] formHeaderBytes = null;
			byte[] boundaryBytes = null;
			byte[] buffer = null;
			Uri uri = GetUri(address);
			bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
			OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
			request = (m_WebRequest = GetWebRequest(uri));
			UploadBits(request, fs, buffer, 0, formHeaderBytes, boundaryBytes, null, null, null);
			byte[] result = DownloadBits(request, null, null, null);
			_ = Logging.On;
			return result;
		}
		catch (Exception ex)
		{
			if (fs != null)
			{
				fs.Close();
				fs = null;
			}
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	private byte[] UploadValuesInternal(NameValueCollection data)
	{
		if (m_headers == null)
		{
			m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
		}
		string text = m_headers["Content-Type"];
		if (text != null && string.Compare(text, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) != 0)
		{
			throw new WebException(global::SR.GetString("The Content-Type header cannot be changed from its default value for this request."));
		}
		m_headers["Content-Type"] = "application/x-www-form-urlencoded";
		string value = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		string[] allKeys = data.AllKeys;
		foreach (string text2 in allKeys)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(UrlEncode(text2));
			stringBuilder.Append("=");
			stringBuilder.Append(UrlEncode(data[text2]));
			value = "&";
		}
		byte[] bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
		m_ContentLength = bytes.Length;
		return bytes;
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadValues(string address, NameValueCollection data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadValues(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	public byte[] UploadValues(Uri address, NameValueCollection data)
	{
		return UploadValues(address, null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] UploadValues(string address, string method, NameValueCollection data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadValues(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI, using the specified method.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array containing the body of the response from the resource.</returns>
	/// <param name="address">The URI of the resource to receive the collection. </param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- <paramref name="data" /> is null.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	public byte[] UploadValues(Uri address, string method, NameValueCollection data)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		WebRequest request = null;
		ClearWebClientState();
		try
		{
			byte[] buffer = UploadValuesInternal(data);
			m_Method = method;
			request = (m_WebRequest = GetWebRequest(GetUri(address)));
			UploadBits(request, null, buffer, 0, null, null, null, null, null);
			byte[] result = DownloadBits(request, null, null, null);
			_ = Logging.On;
			return result;
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			AbortRequest(request);
			throw ex;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	/// <summary>Uploads the specified string to the specified resource, using the POST method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For Http resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string UploadString(string address, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadString(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the POST method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For Http resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	public string UploadString(Uri address, string data)
	{
		return UploadString(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method. </param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string UploadString(string address, string method, string data)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return UploadString(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified string to the specified resource, using the specified method.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method. </param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	public string UploadString(Uri address, string method, string data)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		ClearWebClientState();
		try
		{
			byte[] bytes = Encoding.GetBytes(data);
			WebRequest request;
			byte[] data2 = UploadDataInternal(address, method, bytes, out request);
			string stringUsingEncoding = GetStringUsingEncoding(request, data2);
			_ = Logging.On;
			return stringUsingEncoding;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	/// <summary>Downloads the requested resource as a <see cref="T:System.String" />. The resource to download is specified as a <see cref="T:System.String" /> containing the URI.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the requested resource.</returns>
	/// <param name="address">A <see cref="T:System.String" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public string DownloadString(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return DownloadString(GetUri(address));
	}

	/// <summary>Downloads the requested resource as a <see cref="T:System.String" />. The resource to download is specified as a <see cref="T:System.Uri" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the requested resource.</returns>
	/// <param name="address">A <see cref="T:System.Uri" /> object containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.NotSupportedException">The method has been called simultaneously on multiple threads.</exception>
	public string DownloadString(Uri address)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		ClearWebClientState();
		try
		{
			WebRequest request;
			byte[] data = DownloadDataInternal(address, out request);
			string stringUsingEncoding = GetStringUsingEncoding(request, data);
			_ = Logging.On;
			return stringUsingEncoding;
		}
		finally
		{
			CompleteWebClientState();
		}
	}

	private static void AbortRequest(WebRequest request)
	{
		try
		{
			request?.Abort();
		}
		catch (Exception ex)
		{
			if (ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException)
			{
				throw;
			}
		}
	}

	private void CopyHeadersTo(WebRequest request)
	{
		if (m_headers != null && request is HttpWebRequest)
		{
			string text = m_headers["Accept"];
			string text2 = m_headers["Connection"];
			string text3 = m_headers["Content-Type"];
			string text4 = m_headers["Expect"];
			string text5 = m_headers["Referer"];
			string text6 = m_headers["User-Agent"];
			string text7 = m_headers["Host"];
			m_headers.RemoveInternal("Accept");
			m_headers.RemoveInternal("Connection");
			m_headers.RemoveInternal("Content-Type");
			m_headers.RemoveInternal("Expect");
			m_headers.RemoveInternal("Referer");
			m_headers.RemoveInternal("User-Agent");
			m_headers.RemoveInternal("Host");
			request.Headers = m_headers;
			if (text != null && text.Length > 0)
			{
				((HttpWebRequest)request).Accept = text;
			}
			if (text2 != null && text2.Length > 0)
			{
				((HttpWebRequest)request).Connection = text2;
			}
			if (text3 != null && text3.Length > 0)
			{
				((HttpWebRequest)request).ContentType = text3;
			}
			if (text4 != null && text4.Length > 0)
			{
				((HttpWebRequest)request).Expect = text4;
			}
			if (text5 != null && text5.Length > 0)
			{
				((HttpWebRequest)request).Referer = text5;
			}
			if (text6 != null && text6.Length > 0)
			{
				((HttpWebRequest)request).UserAgent = text6;
			}
			if (!string.IsNullOrEmpty(text7))
			{
				((HttpWebRequest)request).Host = text7;
			}
		}
	}

	private Uri GetUri(string path)
	{
		Uri result;
		if (m_baseAddress != null)
		{
			if (!Uri.TryCreate(m_baseAddress, path, out result))
			{
				return new Uri(Path.GetFullPath(path));
			}
		}
		else if (!Uri.TryCreate(path, UriKind.Absolute, out result))
		{
			return new Uri(Path.GetFullPath(path));
		}
		return GetUri(result);
	}

	private Uri GetUri(Uri address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		Uri result = address;
		if (!address.IsAbsoluteUri && m_baseAddress != null && !Uri.TryCreate(m_baseAddress, address, out result))
		{
			return address;
		}
		if ((result.Query == null || result.Query == string.Empty) && m_requestParameters != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = string.Empty;
			for (int i = 0; i < m_requestParameters.Count; i++)
			{
				stringBuilder.Append(text + m_requestParameters.AllKeys[i] + "=" + m_requestParameters[i]);
				text = "&";
			}
			result = new UriBuilder(result)
			{
				Query = stringBuilder.ToString()
			}.Uri;
		}
		return result;
	}

	private static void DownloadBitsResponseCallback(IAsyncResult result)
	{
		DownloadBitsState downloadBitsState = (DownloadBitsState)result.AsyncState;
		WebRequest request = downloadBitsState.Request;
		Exception ex = null;
		try
		{
			WebResponse webResponse = downloadBitsState.WebClient.GetWebResponse(request, result);
			downloadBitsState.WebClient.m_WebResponse = webResponse;
			downloadBitsState.SetResponse(webResponse);
		}
		catch (Exception ex2)
		{
			if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
			{
				throw;
			}
			ex = ex2;
			if (!(ex2 is WebException) && !(ex2 is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex2);
			}
			AbortRequest(request);
			if (downloadBitsState != null && downloadBitsState.WriteStream != null)
			{
				downloadBitsState.WriteStream.Close();
			}
		}
		finally
		{
			if (ex != null)
			{
				downloadBitsState.CompletionDelegate(null, ex, downloadBitsState.AsyncOp);
			}
		}
	}

	private static void DownloadBitsReadCallback(IAsyncResult result)
	{
		DownloadBitsReadCallbackState((DownloadBitsState)result.AsyncState, result);
	}

	private static void DownloadBitsReadCallbackState(DownloadBitsState state, IAsyncResult result)
	{
		Stream readStream = state.ReadStream;
		Exception ex = null;
		bool flag = false;
		try
		{
			int bytesRetrieved = 0;
			if (readStream != null && readStream != Stream.Null)
			{
				bytesRetrieved = readStream.EndRead(result);
			}
			flag = state.RetrieveBytes(ref bytesRetrieved);
		}
		catch (Exception ex2)
		{
			flag = true;
			if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
			{
				throw;
			}
			ex = ex2;
			state.InnerBuffer = null;
			if (!(ex2 is WebException) && !(ex2 is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex2);
			}
			AbortRequest(state.Request);
			if (state != null && state.WriteStream != null)
			{
				state.WriteStream.Close();
			}
		}
		finally
		{
			if (flag)
			{
				if (ex == null)
				{
					state.Close();
				}
				state.CompletionDelegate(state.InnerBuffer, ex, state.AsyncOp);
			}
		}
	}

	private byte[] DownloadBits(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp)
	{
		WebResponse webResponse = null;
		DownloadBitsState downloadBitsState = new DownloadBitsState(request, writeStream, completionDelegate, asyncOp, m_Progress, this);
		if (downloadBitsState.Async)
		{
			request.BeginGetResponse(DownloadBitsResponseCallback, downloadBitsState);
			return null;
		}
		int bytesRetrieved = downloadBitsState.SetResponse(m_WebResponse = GetWebResponse(request));
		while (!downloadBitsState.RetrieveBytes(ref bytesRetrieved))
		{
		}
		downloadBitsState.Close();
		return downloadBitsState.InnerBuffer;
	}

	private static void UploadBitsRequestCallback(IAsyncResult result)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)result.AsyncState;
		WebRequest request = uploadBitsState.Request;
		Exception ex = null;
		try
		{
			Stream requestStream = request.EndGetRequestStream(result);
			uploadBitsState.SetRequestStream(requestStream);
		}
		catch (Exception ex2)
		{
			if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
			{
				throw;
			}
			ex = ex2;
			if (!(ex2 is WebException) && !(ex2 is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex2);
			}
			AbortRequest(request);
			if (uploadBitsState != null && uploadBitsState.ReadStream != null)
			{
				uploadBitsState.ReadStream.Close();
			}
		}
		finally
		{
			if (ex != null)
			{
				uploadBitsState.UploadCompletionDelegate(null, ex, uploadBitsState);
			}
		}
	}

	private static void UploadBitsWriteCallback(IAsyncResult result)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)result.AsyncState;
		Stream writeStream = uploadBitsState.WriteStream;
		Exception ex = null;
		bool flag = false;
		try
		{
			writeStream.EndWrite(result);
			flag = uploadBitsState.WriteBytes();
		}
		catch (Exception ex2)
		{
			flag = true;
			if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
			{
				throw;
			}
			ex = ex2;
			if (!(ex2 is WebException) && !(ex2 is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex2);
			}
			AbortRequest(uploadBitsState.Request);
			if (uploadBitsState != null && uploadBitsState.ReadStream != null)
			{
				uploadBitsState.ReadStream.Close();
			}
		}
		finally
		{
			if (flag)
			{
				if (ex == null)
				{
					uploadBitsState.Close();
				}
				uploadBitsState.UploadCompletionDelegate(null, ex, uploadBitsState);
			}
		}
	}

	private void UploadBits(WebRequest request, Stream readStream, byte[] buffer, int chunkSize, byte[] header, byte[] footer, CompletionDelegate uploadCompletionDelegate, CompletionDelegate downloadCompletionDelegate, AsyncOperation asyncOp)
	{
		if (request.RequestUri.Scheme == Uri.UriSchemeFile)
		{
			header = (footer = null);
		}
		UploadBitsState uploadBitsState = new UploadBitsState(request, readStream, buffer, chunkSize, header, footer, uploadCompletionDelegate, downloadCompletionDelegate, asyncOp, m_Progress, this);
		if (uploadBitsState.Async)
		{
			request.BeginGetRequestStream(UploadBitsRequestCallback, uploadBitsState);
			return;
		}
		Stream requestStream = request.GetRequestStream();
		uploadBitsState.SetRequestStream(requestStream);
		while (!uploadBitsState.WriteBytes())
		{
		}
		uploadBitsState.Close();
	}

	private bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
	{
		if (prefix == null || byteArray == null || prefix.Length > byteArray.Length)
		{
			return false;
		}
		for (int i = 0; i < prefix.Length; i++)
		{
			if (prefix[i] != byteArray[i])
			{
				return false;
			}
		}
		return true;
	}

	private string GetStringUsingEncoding(WebRequest request, byte[] data)
	{
		Encoding encoding = null;
		int num = -1;
		string text;
		try
		{
			text = request.ContentType;
		}
		catch (NotImplementedException)
		{
			text = null;
		}
		catch (NotSupportedException)
		{
			text = null;
		}
		if (text != null)
		{
			text = text.ToLower(CultureInfo.InvariantCulture);
			string[] array = text.Split(';', '=', ' ');
			bool flag = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2 == "charset")
				{
					flag = true;
				}
				else if (flag)
				{
					try
					{
						encoding = Encoding.GetEncoding(text2);
					}
					catch (ArgumentException)
					{
						break;
					}
				}
			}
		}
		if (encoding == null)
		{
			Encoding[] array3 = new Encoding[4]
			{
				Encoding.UTF8,
				Encoding.UTF32,
				Encoding.Unicode,
				Encoding.BigEndianUnicode
			};
			for (int j = 0; j < array3.Length; j++)
			{
				byte[] preamble = array3[j].GetPreamble();
				if (ByteArrayHasPrefix(preamble, data))
				{
					encoding = array3[j];
					num = preamble.Length;
					break;
				}
			}
		}
		if (encoding == null)
		{
			encoding = Encoding;
		}
		if (num == -1)
		{
			byte[] preamble2 = encoding.GetPreamble();
			num = (ByteArrayHasPrefix(preamble2, data) ? preamble2.Length : 0);
		}
		return encoding.GetString(data, num, data.Length - num);
	}

	private string MapToDefaultMethod(Uri address)
	{
		Uri uri = ((address.IsAbsoluteUri || !(m_baseAddress != null)) ? address : new Uri(m_baseAddress, address));
		if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "ftp")
		{
			return "STOR";
		}
		return "POST";
	}

	private static string UrlEncode(string str)
	{
		if (str == null)
		{
			return null;
		}
		return UrlEncode(str, Encoding.UTF8);
	}

	private static string UrlEncode(string str, Encoding e)
	{
		if (str == null)
		{
			return null;
		}
		return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
	}

	private static byte[] UrlEncodeToBytes(string str, Encoding e)
	{
		if (str == null)
		{
			return null;
		}
		byte[] bytes = e.GetBytes(str);
		return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, alwaysCreateReturnValue: false);
	}

	private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			char c = (char)bytes[offset + i];
			if (c == ' ')
			{
				num++;
			}
			else if (!IsSafe(c))
			{
				num2++;
			}
		}
		if (!alwaysCreateReturnValue && num == 0 && num2 == 0)
		{
			return bytes;
		}
		byte[] array = new byte[count + num2 * 2];
		int num3 = 0;
		for (int j = 0; j < count; j++)
		{
			byte b = bytes[offset + j];
			char c2 = (char)b;
			if (IsSafe(c2))
			{
				array[num3++] = b;
				continue;
			}
			if (c2 == ' ')
			{
				array[num3++] = 43;
				continue;
			}
			array[num3++] = 37;
			array[num3++] = (byte)IntToHex((b >> 4) & 0xF);
			array[num3++] = (byte)IntToHex(b & 0xF);
		}
		return array;
	}

	private static char IntToHex(int n)
	{
		if (n <= 9)
		{
			return (char)(n + 48);
		}
		return (char)(n - 10 + 97);
	}

	private static bool IsSafe(char ch)
	{
		if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
		{
			return true;
		}
		switch (ch)
		{
		case '!':
		case '\'':
		case '(':
		case ')':
		case '*':
		case '-':
		case '.':
		case '_':
			return true;
		default:
			return false;
		}
	}

	private void InvokeOperationCompleted(AsyncOperation asyncOp, SendOrPostCallback callback, AsyncCompletedEventArgs eventArgs)
	{
		if (Interlocked.CompareExchange(ref m_AsyncOp, null, asyncOp) == asyncOp)
		{
			CompleteWebClientState();
			asyncOp.PostOperationCompleted(callback, eventArgs);
		}
	}

	private bool AnotherCallInProgress(int callNesting)
	{
		return callNesting > 1;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.OpenReadCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.OpenReadCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
	{
		if (this.OpenReadCompleted != null)
		{
			this.OpenReadCompleted(this, e);
		}
	}

	private void OpenReadOperationCompleted(object arg)
	{
		OnOpenReadCompleted((OpenReadCompletedEventArgs)arg);
	}

	private void OpenReadAsyncCallback(IAsyncResult result)
	{
		AsyncOperation asyncOperation = (AsyncOperation)result.AsyncState;
		WebRequest request = ((!(result is WebAsyncResult)) ? ((WebRequest)((LazyAsyncResult)result).AsyncObject) : ((WebAsyncResult)result).AsyncObject);
		Stream result2 = null;
		Exception exception = null;
		try
		{
			result2 = (m_WebResponse = GetWebResponse(request, result)).GetResponseStream();
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			exception = ex;
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				exception = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
		}
		OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(result2, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, openReadOperationCompleted, eventArgs);
	}

	/// <summary>Opens a readable stream containing the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void OpenReadAsync(Uri address)
	{
		OpenReadAsync(address, null);
	}

	/// <summary>Opens a readable stream containing the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void OpenReadAsync(Uri address, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			(m_WebRequest = GetWebRequest(GetUri(address))).BeginGetResponse(OpenReadAsyncCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, openReadOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.OpenWriteCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.OpenWriteCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e)
	{
		if (this.OpenWriteCompleted != null)
		{
			this.OpenWriteCompleted(this, e);
		}
	}

	private void OpenWriteOperationCompleted(object arg)
	{
		OnOpenWriteCompleted((OpenWriteCompletedEventArgs)arg);
	}

	private void OpenWriteAsyncCallback(IAsyncResult result)
	{
		WebAsyncResult obj = (WebAsyncResult)result;
		AsyncOperation asyncOperation = (AsyncOperation)obj.AsyncState;
		WebRequest asyncObject = obj.AsyncObject;
		WebClientWriteStream result2 = null;
		Exception exception = null;
		try
		{
			result2 = new WebClientWriteStream(asyncObject.EndGetRequestStream(result), asyncObject, this);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			exception = ex;
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				exception = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
		}
		OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(result2, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, openWriteOperationCompleted, eventArgs);
	}

	/// <summary>Opens a stream for writing data to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void OpenWriteAsync(Uri address)
	{
		OpenWriteAsync(address, null, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void OpenWriteAsync(Uri address, string method)
	{
		OpenWriteAsync(address, method, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void OpenWriteAsync(Uri address, string method, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			m_Method = method;
			(m_WebRequest = GetWebRequest(GetUri(address))).BeginGetRequestStream(OpenWriteAsyncCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, openWriteOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadStringCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadStringCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
	{
		if (this.DownloadStringCompleted != null)
		{
			this.DownloadStringCompleted(this, e);
		}
	}

	private void DownloadStringOperationCompleted(object arg)
	{
		OnDownloadStringCompleted((DownloadStringCompletedEventArgs)arg);
	}

	private void DownloadStringAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		string result = null;
		try
		{
			if (returnBytes != null)
			{
				result = GetStringUsingEncoding(m_WebRequest, returnBytes);
			}
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			exception = ex;
		}
		DownloadStringCompletedEventArgs eventArgs = new DownloadStringCompletedEventArgs(result, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, downloadStringOperationCompleted, eventArgs);
	}

	/// <summary>Downloads the resource specified as a <see cref="T:System.Uri" />. This method does not block the calling thread.</summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadStringAsync(Uri address)
	{
		DownloadStringAsync(address, null);
	}

	/// <summary>Downloads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadStringAsync(Uri address, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), null, DownloadStringAsyncCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			DownloadStringAsyncCallback(null, ex, asyncOperation);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadDataCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadDataCompletedEventArgs" /> object that contains event data.</param>
	protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
	{
		if (this.DownloadDataCompleted != null)
		{
			this.DownloadDataCompleted(this, e);
		}
	}

	private void DownloadDataOperationCompleted(object arg)
	{
		OnDownloadDataCompleted((DownloadDataCompletedEventArgs)arg);
	}

	private void DownloadDataAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		DownloadDataCompletedEventArgs eventArgs = new DownloadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, downloadDataOperationCompleted, eventArgs);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation. </summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadDataAsync(Uri address)
	{
		DownloadDataAsync(address, null);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation. </summary>
	/// <param name="address">A <see cref="T:System.Uri" /> containing the URI to download.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadDataAsync(Uri address, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), null, DownloadDataAsyncCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			DownloadDataAsyncCallback(null, ex, asyncOperation);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadFileCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
	{
		if (this.DownloadFileCompleted != null)
		{
			this.DownloadFileCompleted(this, e);
		}
	}

	private void DownloadFileOperationCompleted(object arg)
	{
		OnDownloadFileCompleted((AsyncCompletedEventArgs)arg);
	}

	private void DownloadFileAsyncCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, downloadFileOperationCompleted, eventArgs);
	}

	/// <summary>Downloads, to a local file, the resource with the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to download. </param>
	/// <param name="fileName">The name of the file to be placed on the local computer. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadFileAsync(Uri address, string fileName)
	{
		DownloadFileAsync(address, fileName, null);
	}

	/// <summary>Downloads, to a local file, the resource with the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to download. </param>
	/// <param name="fileName">The name of the file to be placed on the local computer. </param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void DownloadFileAsync(Uri address, string fileName, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		FileStream fileStream = null;
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), fileStream, DownloadFileAsyncCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			fileStream?.Close();
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			DownloadFileAsyncCallback(null, ex, asyncOperation);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadStringCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadStringCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e)
	{
		if (this.UploadStringCompleted != null)
		{
			this.UploadStringCompleted(this, e);
		}
	}

	private void UploadStringOperationCompleted(object arg)
	{
		OnUploadStringCompleted((UploadStringCompletedEventArgs)arg);
	}

	private void StartDownloadAsync(UploadBitsState state)
	{
		try
		{
			DownloadBits(state.Request, null, state.DownloadCompletionDelegate, state.AsyncOp);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			state.DownloadCompletionDelegate(null, ex, state.AsyncOp);
		}
	}

	private void UploadStringAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)state;
		if (exception != null)
		{
			UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(null, exception, m_Cancelled, uploadBitsState.AsyncOp.UserSuppliedState);
			InvokeOperationCompleted(uploadBitsState.AsyncOp, uploadStringOperationCompleted, eventArgs);
		}
		else
		{
			StartDownloadAsync(uploadBitsState);
		}
	}

	private void UploadStringAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		string result = null;
		try
		{
			if (returnBytes != null)
			{
				result = GetStringUsingEncoding(m_WebRequest, returnBytes);
			}
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			exception = ex;
		}
		UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(result, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, uploadStringOperationCompleted, eventArgs);
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadStringAsync(Uri address, string data)
	{
		UploadStringAsync(address, null, data, null);
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadStringAsync(Uri address, string method, string data)
	{
		UploadStringAsync(address, method, data, null);
	}

	/// <summary>Uploads the specified string to the specified resource. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadStringAsync(Uri address, string method, string data, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		try
		{
			byte[] bytes = Encoding.GetBytes(data);
			m_Method = method;
			m_ContentLength = bytes.Length;
			UploadBits(m_WebRequest = GetWebRequest(GetUri(address)), null, bytes, 0, null, null, UploadStringAsyncWriteCallback, UploadStringAsyncReadCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, uploadStringOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadDataCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.UploadDataCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e)
	{
		if (this.UploadDataCompleted != null)
		{
			this.UploadDataCompleted(this, e);
		}
	}

	private void UploadDataOperationCompleted(object arg)
	{
		OnUploadDataCompleted((UploadDataCompletedEventArgs)arg);
	}

	private void UploadDataAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)state;
		if (exception != null)
		{
			UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadBitsState.AsyncOp.UserSuppliedState);
			InvokeOperationCompleted(uploadBitsState.AsyncOp, uploadDataOperationCompleted, eventArgs);
		}
		else
		{
			StartDownloadAsync(uploadBitsState);
		}
	}

	private void UploadDataAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, uploadDataOperationCompleted, eventArgs);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data. </param>
	/// <param name="data">The data buffer to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadDataAsync(Uri address, byte[] data)
	{
		UploadDataAsync(address, null, data, null);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadDataAsync(Uri address, string method, byte[] data)
	{
		UploadDataAsync(address, method, data, null);
	}

	/// <summary>Uploads a data buffer to a resource identified by a URI, using the specified method and identifying token.</summary>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		int chunkSize = 0;
		try
		{
			m_Method = method;
			m_ContentLength = data.Length;
			WebRequest request = (m_WebRequest = GetWebRequest(GetUri(address)));
			if (this.UploadProgressChanged != null)
			{
				chunkSize = (int)Math.Min(8192L, data.Length);
			}
			UploadBits(request, null, data, chunkSize, null, null, UploadDataAsyncWriteCallback, UploadDataAsyncReadCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, uploadDataOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadFileCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadFileCompletedEventArgs" /> object containing event data.</param>
	protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e)
	{
		if (this.UploadFileCompleted != null)
		{
			this.UploadFileCompleted(this, e);
		}
	}

	private void UploadFileOperationCompleted(object arg)
	{
		OnUploadFileCompleted((UploadFileCompletedEventArgs)arg);
	}

	private void UploadFileAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)state;
		if (exception != null)
		{
			UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadBitsState.AsyncOp.UserSuppliedState);
			InvokeOperationCompleted(uploadBitsState.AsyncOp, uploadFileOperationCompleted, eventArgs);
		}
		else
		{
			StartDownloadAsync(uploadBitsState);
		}
	}

	private void UploadFileAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, uploadFileOperationCompleted, eventArgs);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadFileAsync(Uri address, string fileName)
	{
		UploadFileAsync(address, null, fileName, null);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page. </param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadFileAsync(Uri address, string method, string fileName)
	{
		UploadFileAsync(address, method, fileName, null);
	}

	/// <summary>Uploads the specified local file to the specified resource, using the POST method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The file to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		FileStream fs = null;
		try
		{
			m_Method = method;
			byte[] formHeaderBytes = null;
			byte[] boundaryBytes = null;
			byte[] buffer = null;
			Uri uri = GetUri(address);
			bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
			OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
			UploadBits(m_WebRequest = GetWebRequest(uri), fs, buffer, 0, formHeaderBytes, boundaryBytes, UploadFileAsyncWriteCallback, UploadFileAsyncReadCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			fs?.Close();
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, uploadFileOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadValuesCompleted" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.UploadValuesCompletedEventArgs" />  object containing event data.</param>
	protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e)
	{
		if (this.UploadValuesCompleted != null)
		{
			this.UploadValuesCompleted(this, e);
		}
	}

	private void UploadValuesOperationCompleted(object arg)
	{
		OnUploadValuesCompleted((UploadValuesCompletedEventArgs)arg);
	}

	private void UploadValuesAsyncWriteCallback(byte[] returnBytes, Exception exception, object state)
	{
		UploadBitsState uploadBitsState = (UploadBitsState)state;
		if (exception != null)
		{
			UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, uploadBitsState.AsyncOp.UserSuppliedState);
			InvokeOperationCompleted(uploadBitsState.AsyncOp, uploadValuesOperationCompleted, eventArgs);
		}
		else
		{
			StartDownloadAsync(uploadBitsState);
		}
	}

	private void UploadValuesAsyncReadCallback(byte[] returnBytes, Exception exception, object state)
	{
		AsyncOperation asyncOperation = (AsyncOperation)state;
		UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOperation.UserSuppliedState);
		InvokeOperationCompleted(asyncOperation, uploadValuesOperationCompleted, eventArgs);
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the default method. See remarks.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadValuesAsync(Uri address, NameValueCollection data)
	{
		UploadValuesAsync(address, null, data, null);
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI, using the specified method. This method does not block the calling thread.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method.</param>
	/// <param name="method">The method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
	{
		UploadValuesAsync(address, method, data, null);
	}

	/// <summary>Uploads the data in the specified name/value collection to the resource identified by the specified URI, using the specified method. This method does not block the calling thread, and allows the caller to pass an object to the method that is invoked when the operation completes.</summary>
	/// <param name="address">The URI of the resource to receive the collection. This URI must identify a resource that can accept a request sent with the <paramref name="method" /> method.</param>
	/// <param name="method">The HTTP method used to send the string to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or-<paramref name="method" /> cannot be used to send content.</exception>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
	{
		_ = Logging.On;
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (method == null)
		{
			method = MapToDefaultMethod(address);
		}
		InitWebClientAsync();
		ClearWebClientState();
		AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
		int chunkSize = 0;
		try
		{
			byte[] array = UploadValuesInternal(data);
			m_Method = method;
			WebRequest request = (m_WebRequest = GetWebRequest(GetUri(address)));
			if (this.UploadProgressChanged != null)
			{
				chunkSize = (int)Math.Min(8192L, array.Length);
			}
			UploadBits(request, null, array, chunkSize, null, null, UploadValuesAsyncWriteCallback, UploadValuesAsyncReadCallback, asyncOperation);
		}
		catch (Exception ex)
		{
			if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
			if (!(ex is WebException) && !(ex is SecurityException))
			{
				ex = new WebException(global::SR.GetString("An exception occurred during a WebClient request."), ex);
			}
			UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, uploadValuesOperationCompleted, eventArgs);
		}
		_ = Logging.On;
	}

	/// <summary>Cancels a pending asynchronous operation.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void CancelAsync()
	{
		WebRequest webRequest = m_WebRequest;
		m_Cancelled = true;
		AbortRequest(webRequest);
	}

	/// <summary>Downloads the resource as a <see cref="T:System.String" /> from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> DownloadStringTaskAsync(string address)
	{
		return DownloadStringTaskAsync(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.String" /> from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> DownloadStringTaskAsync(Uri address)
	{
		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
		DownloadStringCompletedEventHandler handler = null;
		handler = delegate(object sender, DownloadStringCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (DownloadStringCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, DownloadStringCompletedEventHandler completion)
			{
				webClient.DownloadStringCompleted -= completion;
			});
		};
		DownloadStringCompleted += handler;
		try
		{
			DownloadStringAsync(address, tcs);
		}
		catch
		{
			DownloadStringCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Opens a readable stream containing the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenReadTaskAsync(string address)
	{
		return OpenReadTaskAsync(GetUri(address));
	}

	/// <summary>Opens a readable stream containing the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to read data from a resource.</returns>
	/// <param name="address">The URI of the resource to retrieve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and address is invalid.-or- An error occurred while downloading the resource. -or- An error occurred while opening the stream.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenReadTaskAsync(Uri address)
	{
		TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
		OpenReadCompletedEventHandler handler = null;
		handler = delegate(object sender, OpenReadCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (OpenReadCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, OpenReadCompletedEventHandler completion)
			{
				webClient.OpenReadCompleted -= completion;
			});
		};
		OpenReadCompleted += handler;
		try
		{
			OpenReadAsync(address, tcs);
		}
		catch
		{
			OpenReadCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenWriteTaskAsync(string address)
	{
		return OpenWriteTaskAsync(GetUri(address), null);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenWriteTaskAsync(Uri address)
	{
		return OpenWriteTaskAsync(address, null);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenWriteTaskAsync(string address, string method)
	{
		return OpenWriteTaskAsync(GetUri(address), method);
	}

	/// <summary>Opens a stream for writing data to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.IO.Stream" /> used to write data to the resource.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<Stream> OpenWriteTaskAsync(Uri address, string method)
	{
		TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>(address);
		OpenWriteCompletedEventHandler handler = null;
		handler = delegate(object sender, OpenWriteCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (OpenWriteCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, OpenWriteCompletedEventHandler completion)
			{
				webClient.OpenWriteCompleted -= completion;
			});
		};
		OpenWriteCompleted += handler;
		try
		{
			OpenWriteAsync(address, method, tcs);
		}
		catch
		{
			OpenWriteCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> UploadStringTaskAsync(string address, string data)
	{
		return UploadStringTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> UploadStringTaskAsync(Uri address, string data)
	{
		return UploadStringTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> UploadStringTaskAsync(string address, string method, string data)
	{
		return UploadStringTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified string to the specified resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.String" /> containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the string. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The HTTP method used to send the file to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The string to be uploaded.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<string> UploadStringTaskAsync(Uri address, string method, string data)
	{
		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(address);
		UploadStringCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadStringCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadStringCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadStringCompletedEventHandler completion)
			{
				webClient.UploadStringCompleted -= completion;
			});
		};
		UploadStringCompleted += handler;
		try
		{
			UploadStringAsync(address, method, data, tcs);
		}
		catch
		{
			UploadStringCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> DownloadDataTaskAsync(string address)
	{
		return DownloadDataTaskAsync(GetUri(address));
	}

	/// <summary>Downloads the resource as a <see cref="T:System.Byte" /> array from the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the downloaded resource.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> DownloadDataTaskAsync(Uri address)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		DownloadDataCompletedEventHandler handler = null;
		handler = delegate(object sender, DownloadDataCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (DownloadDataCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, DownloadDataCompletedEventHandler completion)
			{
				webClient.DownloadDataCompleted -= completion;
			});
		};
		DownloadDataCompleted += handler;
		try
		{
			DownloadDataAsync(address, tcs);
		}
		catch
		{
			DownloadDataCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Downloads the specified resource to a local file as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <param name="fileName">The name of the file to be placed on the local computer.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task DownloadFileTaskAsync(string address, string fileName)
	{
		return DownloadFileTaskAsync(GetUri(address), fileName);
	}

	/// <summary>Downloads the specified resource to a local file as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
	/// <param name="address">The URI of the resource to download.</param>
	/// <param name="fileName">The name of the file to be placed on the local computer.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while downloading the resource. </exception>
	/// <exception cref="T:System.InvalidOperationException">The local file specified by <paramref name="fileName" /> is in use by another thread.</exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task DownloadFileTaskAsync(Uri address, string fileName)
	{
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(address);
		AsyncCompletedEventHandler handler = null;
		handler = delegate(object sender, AsyncCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (AsyncCompletedEventArgs args) => (object)null, handler, delegate(WebClient webClient, AsyncCompletedEventHandler completion)
			{
				webClient.DownloadFileCompleted -= completion;
			});
		};
		DownloadFileCompleted += handler;
		try
		{
			DownloadFileAsync(address, fileName, tcs);
		}
		catch
		{
			DownloadFileCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadDataTaskAsync(string address, byte[] data)
	{
		return UploadDataTaskAsync(GetUri(address), null, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadDataTaskAsync(Uri address, byte[] data)
	{
		return UploadDataTaskAsync(address, null, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadDataTaskAsync(string address, string method, byte[] data)
	{
		return UploadDataTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads a data buffer that contains a <see cref="T:System.Byte" /> array to the URI specified as an asynchronous operation using a task object.  </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the data buffer was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the data.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The data buffer to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadDataTaskAsync(Uri address, string method, byte[] data)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadDataCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadDataCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadDataCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadDataCompletedEventHandler completion)
			{
				webClient.UploadDataCompleted -= completion;
			});
		};
		UploadDataCompleted += handler;
		try
		{
			UploadDataAsync(address, method, data, tcs);
		}
		catch
		{
			UploadDataCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadFileTaskAsync(string address, string fileName)
	{
		return UploadFileTaskAsync(GetUri(address), null, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object. </summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadFileTaskAsync(Uri address, string fileName)
	{
		return UploadFileTaskAsync(address, null, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadFileTaskAsync(string address, string method, string fileName)
	{
		return UploadFileTaskAsync(GetUri(address), method, fileName);
	}

	/// <summary>Uploads the specified local file to a resource as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the body of the response received from the resource when the file was uploaded.</returns>
	/// <param name="address">The URI of the resource to receive the file. For HTTP resources, this URI must identify a resource that can accept a request sent with the POST method, such as a script or ASP page.</param>
	/// <param name="method">The method used to send the data to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="fileName">The local file to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null. -or-The <paramref name="fileName" /> parameter is null. </exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" /> and <paramref name="address" /> is invalid.-or- <paramref name="fileName" /> is null, is <see cref="F:System.String.Empty" />, contains invalid character, or the specified path to the file does not exist.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header begins with multipart. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadFileTaskAsync(Uri address, string method, string fileName)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadFileCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadFileCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadFileCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadFileCompletedEventHandler completion)
			{
				webClient.UploadFileCompleted -= completion;
			});
		};
		UploadFileCompleted += handler;
		try
		{
			UploadFileAsync(address, method, fileName, tcs);
		}
		catch
		{
			UploadFileCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadValuesTaskAsync(string address, NameValueCollection data)
	{
		return UploadValuesTaskAsync(GetUri(address), null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="method">The HTTP method used to send the collection to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadValuesTaskAsync(string address, string method, NameValueCollection data)
	{
		return UploadValuesTaskAsync(GetUri(address), method, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or- An error occurred while opening the stream.-or- There was no response from the server hosting the resource.-or- The Content-type header value is not null and is not application/x-www-form-urlencoded. </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadValuesTaskAsync(Uri address, NameValueCollection data)
	{
		return UploadValuesTaskAsync(address, null, data);
	}

	/// <summary>Uploads the specified name/value collection to the resource identified by the specified URI as an asynchronous operation using a task object.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns a <see cref="T:System.Byte" /> array containing the response sent by the server.</returns>
	/// <param name="address">The URI of the resource to receive the collection.</param>
	/// <param name="method">The HTTP method used to send the collection to the resource. If null, the default is POST for http and STOR for ftp.</param>
	/// <param name="data">The <see cref="T:System.Collections.Specialized.NameValueCollection" /> to send to the resource.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="address" /> parameter is null.-or-The <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.Net.WebException">The URI formed by combining <see cref="P:System.Net.WebClient.BaseAddress" />, and <paramref name="address" /> is invalid.-or-<paramref name="method" /> cannot be used to send content.-or- There was no response from the server hosting the resource.-or- An error occurred while opening the stream.-or- The Content-type header is not null or "application/x-www-form-urlencoded". </exception>
	[ComVisible(false)]
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public Task<byte[]> UploadValuesTaskAsync(Uri address, string method, NameValueCollection data)
	{
		TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>(address);
		UploadValuesCompletedEventHandler handler = null;
		handler = delegate(object sender, UploadValuesCompletedEventArgs e)
		{
			HandleCompletion(tcs, e, (UploadValuesCompletedEventArgs args) => args.Result, handler, delegate(WebClient webClient, UploadValuesCompletedEventHandler completion)
			{
				webClient.UploadValuesCompleted -= completion;
			});
		};
		UploadValuesCompleted += handler;
		try
		{
			UploadValuesAsync(address, method, data, tcs);
		}
		catch
		{
			UploadValuesCompleted -= handler;
			throw;
		}
		return tcs.Task;
	}

	private void HandleCompletion<TAsyncCompletedEventArgs, TCompletionDelegate, T>(TaskCompletionSource<T> tcs, TAsyncCompletedEventArgs e, Func<TAsyncCompletedEventArgs, T> getResult, TCompletionDelegate handler, Action<WebClient, TCompletionDelegate> unregisterHandler) where TAsyncCompletedEventArgs : AsyncCompletedEventArgs
	{
		if (e.UserState != tcs)
		{
			return;
		}
		try
		{
			unregisterHandler(this, handler);
		}
		finally
		{
			if (e.Error != null)
			{
				tcs.TrySetException(e.Error);
			}
			else if (e.Cancelled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(getResult(e));
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.DownloadProgressChanged" /> event.</summary>
	/// <param name="e">A <see cref="T:System.Net.DownloadProgressChangedEventArgs" /> object containing event data.</param>
	protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
	{
		if (this.DownloadProgressChanged != null)
		{
			this.DownloadProgressChanged(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Net.WebClient.UploadProgressChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.Net.UploadProgressChangedEventArgs" /> object containing event data.</param>
	protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e)
	{
		if (this.UploadProgressChanged != null)
		{
			this.UploadProgressChanged(this, e);
		}
	}

	private void ReportDownloadProgressChanged(object arg)
	{
		OnDownloadProgressChanged((DownloadProgressChangedEventArgs)arg);
	}

	private void ReportUploadProgressChanged(object arg)
	{
		OnUploadProgressChanged((UploadProgressChangedEventArgs)arg);
	}

	private void PostProgressChanged(AsyncOperation asyncOp, ProgressData progress)
	{
		if (asyncOp != null && progress.BytesSent + progress.BytesReceived > 0)
		{
			if (progress.HasUploadPhase)
			{
				asyncOp.Post(arg: new UploadProgressChangedEventArgs((int)((progress.TotalBytesToReceive >= 0 || progress.BytesReceived != 0L) ? ((progress.TotalBytesToSend < 0) ? 50 : ((progress.TotalBytesToReceive == 0L) ? 100 : (50 * progress.BytesReceived / progress.TotalBytesToReceive + 50))) : ((progress.TotalBytesToSend >= 0) ? ((progress.TotalBytesToSend == 0L) ? 50 : (50 * progress.BytesSent / progress.TotalBytesToSend)) : 0)), asyncOp.UserSuppliedState, progress.BytesSent, progress.TotalBytesToSend, progress.BytesReceived, progress.TotalBytesToReceive), d: reportUploadProgressChanged);
				return;
			}
			int progressPercentage = (int)((progress.TotalBytesToReceive >= 0) ? ((progress.TotalBytesToReceive == 0L) ? 100 : (100 * progress.BytesReceived / progress.TotalBytesToReceive)) : 0);
			asyncOp.Post(reportDownloadProgressChanged, new DownloadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesReceived, progress.TotalBytesToReceive));
		}
	}
}

using System.Collections;
using System.IO;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Mono.Net.Security.Private;
using Mono.Security.Authenticode;
using Mono.Security.Interface;

namespace System.Net;

/// <summary>Provides a simple, programmatically controlled HTTP protocol listener. This class cannot be inherited.</summary>
public sealed class HttpListener : IDisposable
{
	/// <summary>A delegate called to determine the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> to use for each <see cref="T:System.Net.HttpListener" /> request.</summary>
	/// <returns>An <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> object that specifies the extended protection policy to use for this request.</returns>
	/// <param name="request">The <see cref="T:System.Net.HttpListenerRequest" /> to determine the extended protection policy that the <see cref="T:System.Net.HttpListener" /> instance will use to provide extended protection.</param>
	public delegate ExtendedProtectionPolicy ExtendedProtectionSelector(HttpListenerRequest request);

	private MonoTlsProvider tlsProvider;

	private MonoTlsSettings tlsSettings;

	private X509Certificate certificate;

	private AuthenticationSchemes auth_schemes;

	private HttpListenerPrefixCollection prefixes;

	private AuthenticationSchemeSelector auth_selector;

	private string realm;

	private bool ignore_write_exceptions;

	private bool unsafe_ntlm_auth;

	private bool listening;

	private bool disposed;

	private readonly object _internalLock;

	private Hashtable registry;

	private ArrayList ctx_queue;

	private ArrayList wait_queue;

	private Hashtable connections;

	private ServiceNameStore defaultServiceNames;

	private ExtendedProtectionPolicy extendedProtectionPolicy;

	private ExtendedProtectionSelector extendedProtectionSelectorDelegate;

	/// <summary>Gets or sets the scheme used to authenticate clients.</summary>
	/// <returns>A bitwise combination of <see cref="T:System.Net.AuthenticationSchemes" /> enumeration values that indicates how clients are to be authenticated. The default value is <see cref="F:System.Net.AuthenticationSchemes.Anonymous" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public AuthenticationSchemes AuthenticationSchemes
	{
		get
		{
			return auth_schemes;
		}
		set
		{
			CheckDisposed();
			auth_schemes = value;
		}
	}

	/// <summary>Gets or sets the delegate called to determine the protocol used to authenticate clients.</summary>
	/// <returns>An <see cref="T:System.Net.AuthenticationSchemeSelector" /> delegate that invokes the method used to select an authentication protocol. The default value is null.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public AuthenticationSchemeSelector AuthenticationSchemeSelectorDelegate
	{
		get
		{
			return auth_selector;
		}
		set
		{
			CheckDisposed();
			auth_selector = value;
		}
	}

	/// <summary>Get or set the delegate called to determine the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> to use for each request. </summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> that specifies the policy to use for extended protection.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionSelectorDelegate" /> property, but the <see cref="P:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.CustomChannelBinding" /> property must be null. </exception>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionSelectorDelegate" /> property to null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionSelectorDelegate" /> property after the <see cref="M:System.Net.HttpListener.Start" /> method was already called.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionSelectorDelegate" /> property on a platform that does not support extended protection.</exception>
	public ExtendedProtectionSelector ExtendedProtectionSelectorDelegate
	{
		get
		{
			return extendedProtectionSelectorDelegate;
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (!AuthenticationManager.OSSupportsExtendedProtection)
			{
				throw new PlatformNotSupportedException(global::SR.GetString("This operation requires OS support for extended protection."));
			}
			extendedProtectionSelectorDelegate = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether your application receives exceptions that occur when an <see cref="T:System.Net.HttpListener" /> sends the response to the client.</summary>
	/// <returns>true if this <see cref="T:System.Net.HttpListener" /> should not return exceptions that occur when sending the response to the client; otherwise false. The default value is false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public bool IgnoreWriteExceptions
	{
		get
		{
			return ignore_write_exceptions;
		}
		set
		{
			CheckDisposed();
			ignore_write_exceptions = value;
		}
	}

	/// <summary>Gets a value that indicates whether <see cref="T:System.Net.HttpListener" /> has been started.</summary>
	/// <returns>true if the <see cref="T:System.Net.HttpListener" /> was started; otherwise, false.</returns>
	public bool IsListening => listening;

	/// <summary>Gets a value that indicates whether <see cref="T:System.Net.HttpListener" /> can be used with the current operating system.</summary>
	/// <returns>true if <see cref="T:System.Net.HttpListener" /> is supported; otherwise, false.</returns>
	public static bool IsSupported => true;

	/// <summary>Gets the Uniform Resource Identifier (URI) prefixes handled by this <see cref="T:System.Net.HttpListener" /> object.</summary>
	/// <returns>An <see cref="T:System.Net.HttpListenerPrefixCollection" /> that contains the URI prefixes that this <see cref="T:System.Net.HttpListener" /> object is configured to handle. </returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public HttpListenerPrefixCollection Prefixes
	{
		get
		{
			CheckDisposed();
			return prefixes;
		}
	}

	/// <summary>The timeout manager for this <see cref="T:System.Net.HttpListener" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.HttpListenerTimeoutManager" />.The timeout manager for this <see cref="T:System.Net.HttpListener" /> instance.</returns>
	[System.MonoTODO]
	public HttpListenerTimeoutManager TimeoutManager
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Get or set the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> to use for extended protection for a session. </summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> that specifies the policy to use for extended protection.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionPolicy" /> property, but the <see cref="P:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.CustomChannelBinding" /> property was not null. </exception>
	/// <exception cref="T:System.ArgumentNullException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionPolicy" /> property to null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An attempt was made to set the <see cref="P:System.Net.HttpListener.ExtendedProtectionPolicy" /> property after the <see cref="M:System.Net.HttpListener.Start" /> method was already called.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="P:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.PolicyEnforcement" /> property was set to <see cref="F:System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Always" /> on a platform that does not support extended protection.</exception>
	[System.MonoTODO("not used anywhere in the implementation")]
	public ExtendedProtectionPolicy ExtendedProtectionPolicy
	{
		get
		{
			return extendedProtectionPolicy;
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!AuthenticationManager.OSSupportsExtendedProtection && value.PolicyEnforcement == PolicyEnforcement.Always)
			{
				throw new PlatformNotSupportedException(global::SR.GetString("This operation requires OS support for extended protection."));
			}
			if (value.CustomChannelBinding != null)
			{
				throw new ArgumentException(global::SR.GetString("Custom channel bindings are not supported."), "CustomChannelBinding");
			}
			extendedProtectionPolicy = value;
		}
	}

	/// <summary>Gets a default list of Service Provider Names (SPNs) as determined by registered prefixes.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ServiceNameCollection" /> that contains a list of SPNs.</returns>
	public ServiceNameCollection DefaultServiceNames => defaultServiceNames.ServiceNames;

	/// <summary>Gets or sets the realm, or resource partition, associated with this <see cref="T:System.Net.HttpListener" /> object.</summary>
	/// <returns>A <see cref="T:System.String" /> value that contains the name of the realm associated with the <see cref="T:System.Net.HttpListener" /> object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public string Realm
	{
		get
		{
			return realm;
		}
		set
		{
			CheckDisposed();
			realm = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether, when NTLM is used, additional requests using the same Transmission Control Protocol (TCP) connection are required to authenticate.</summary>
	/// <returns>true if the <see cref="T:System.Security.Principal.IIdentity" /> of the first request will be used for subsequent requests on the same connection; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	[System.MonoTODO("Support for NTLM needs some loving.")]
	public bool UnsafeConnectionNtlmAuthentication
	{
		get
		{
			return unsafe_ntlm_auth;
		}
		set
		{
			CheckDisposed();
			unsafe_ntlm_auth = value;
		}
	}

	internal HttpListener(X509Certificate certificate, MonoTlsProvider tlsProvider, MonoTlsSettings tlsSettings)
		: this()
	{
		this.certificate = certificate;
		this.tlsProvider = tlsProvider;
		this.tlsSettings = tlsSettings;
	}

	internal X509Certificate LoadCertificateAndKey(IPAddress addr, int port)
	{
		lock (_internalLock)
		{
			if (certificate != null)
			{
				return certificate;
			}
			try
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".mono");
				path = Path.Combine(path, "httplistener");
				string text = Path.Combine(path, $"{port}.cer");
				if (!File.Exists(text))
				{
					return null;
				}
				string text2 = Path.Combine(path, $"{port}.pvk");
				if (!File.Exists(text2))
				{
					return null;
				}
				X509Certificate2 x509Certificate = new X509Certificate2(text);
				x509Certificate.PrivateKey = PrivateKey.CreateFromFile(text2).RSA;
				certificate = x509Certificate;
				return certificate;
			}
			catch
			{
				certificate = null;
				return null;
			}
		}
	}

	internal SslStream CreateSslStream(Stream innerStream, bool ownsStream, RemoteCertificateValidationCallback callback)
	{
		lock (_internalLock)
		{
			if (tlsProvider == null)
			{
				tlsProvider = MonoTlsProviderFactory.GetProvider();
			}
			MonoTlsSettings monoTlsSettings = (tlsSettings ?? MonoTlsSettings.DefaultSettings).Clone();
			monoTlsSettings.RemoteCertificateValidationCallback = CallbackHelpers.PublicToMono(callback);
			return new SslStream(innerStream, ownsStream, tlsProvider, monoTlsSettings);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpListener" /> class.</summary>
	/// <exception cref="T:System.PlatformNotSupportedException">This class cannot be used on the current operating system. Windows Server 2003 or Windows XP SP2 is required to use instances of this class.</exception>
	public HttpListener()
	{
		_internalLock = new object();
		prefixes = new HttpListenerPrefixCollection(this);
		registry = new Hashtable();
		connections = Hashtable.Synchronized(new Hashtable());
		ctx_queue = new ArrayList();
		wait_queue = new ArrayList();
		auth_schemes = AuthenticationSchemes.Anonymous;
		defaultServiceNames = new ServiceNameStore();
		extendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
	}

	/// <summary>Shuts down the <see cref="T:System.Net.HttpListener" /> object immediately, discarding all currently queued requests.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Abort()
	{
		if (!disposed && listening)
		{
			Close(force: true);
		}
	}

	/// <summary>Shuts down the <see cref="T:System.Net.HttpListener" />.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Close()
	{
		if (!disposed)
		{
			if (!listening)
			{
				disposed = true;
				return;
			}
			Close(force: true);
			disposed = true;
		}
	}

	private void Close(bool force)
	{
		CheckDisposed();
		EndPointManager.RemoveListener(this);
		Cleanup(force);
	}

	private void Cleanup(bool close_existing)
	{
		lock (_internalLock)
		{
			if (close_existing)
			{
				ICollection keys = registry.Keys;
				HttpListenerContext[] array = new HttpListenerContext[keys.Count];
				keys.CopyTo(array, 0);
				registry.Clear();
				for (int num = array.Length - 1; num >= 0; num--)
				{
					array[num].Connection.Close(force_close: true);
				}
			}
			lock (connections.SyncRoot)
			{
				ICollection keys2 = connections.Keys;
				HttpConnection[] array2 = new HttpConnection[keys2.Count];
				keys2.CopyTo(array2, 0);
				connections.Clear();
				for (int num2 = array2.Length - 1; num2 >= 0; num2--)
				{
					array2[num2].Close(force_close: true);
				}
			}
			lock (ctx_queue)
			{
				HttpListenerContext[] array3 = (HttpListenerContext[])ctx_queue.ToArray(typeof(HttpListenerContext));
				ctx_queue.Clear();
				for (int num3 = array3.Length - 1; num3 >= 0; num3--)
				{
					array3[num3].Connection.Close(force_close: true);
				}
			}
			lock (wait_queue)
			{
				Exception exc = new ObjectDisposedException("listener");
				foreach (ListenerAsyncResult item in wait_queue)
				{
					item.Complete(exc);
				}
				wait_queue.Clear();
			}
		}
	}

	/// <summary>Begins asynchronously retrieving an incoming request.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that indicates the status of the asynchronous operation.</returns>
	/// <param name="callback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when a client request is available.</param>
	/// <param name="state">A user-defined object that contains information about the operation. This object is passed to the <paramref name="callback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.Net.HttpListenerException">A Win32 function call failed. Check the exception's <see cref="P:System.Net.HttpListenerException.ErrorCode" /> property to determine the cause of the exception.</exception>
	/// <exception cref="T:System.InvalidOperationException">This object has not been started or is currently stopped.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public IAsyncResult BeginGetContext(AsyncCallback callback, object state)
	{
		CheckDisposed();
		if (!listening)
		{
			throw new InvalidOperationException("Please, call Start before using this method.");
		}
		ListenerAsyncResult listenerAsyncResult = new ListenerAsyncResult(callback, state);
		lock (wait_queue)
		{
			lock (ctx_queue)
			{
				HttpListenerContext contextFromQueue = GetContextFromQueue();
				if (contextFromQueue != null)
				{
					listenerAsyncResult.Complete(contextFromQueue, synch: true);
					return listenerAsyncResult;
				}
			}
			wait_queue.Add(listenerAsyncResult);
			return listenerAsyncResult;
		}
	}

	/// <summary>Completes an asynchronous operation to retrieve an incoming client request.</summary>
	/// <returns>An <see cref="T:System.Net.HttpListenerContext" /> object that represents the client request.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> object that was obtained when the asynchronous operation was started.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not obtained by calling the <see cref="M:System.Net.HttpListener.BeginGetContext(System.AsyncCallback,System.Object)" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpListener.EndGetContext(System.IAsyncResult)" /> method was already called for the specified <paramref name="asyncResult" /> object.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public HttpListenerContext EndGetContext(IAsyncResult asyncResult)
	{
		CheckDisposed();
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (!(asyncResult is ListenerAsyncResult listenerAsyncResult))
		{
			throw new ArgumentException("Wrong IAsyncResult.", "asyncResult");
		}
		if (listenerAsyncResult.EndCalled)
		{
			throw new ArgumentException("Cannot reuse this IAsyncResult");
		}
		listenerAsyncResult.EndCalled = true;
		if (!listenerAsyncResult.IsCompleted)
		{
			listenerAsyncResult.AsyncWaitHandle.WaitOne();
		}
		lock (wait_queue)
		{
			int num = wait_queue.IndexOf(listenerAsyncResult);
			if (num >= 0)
			{
				wait_queue.RemoveAt(num);
			}
		}
		HttpListenerContext context = listenerAsyncResult.GetContext();
		context.ParseAuthentication(SelectAuthenticationScheme(context));
		return context;
	}

	internal AuthenticationSchemes SelectAuthenticationScheme(HttpListenerContext context)
	{
		if (AuthenticationSchemeSelectorDelegate != null)
		{
			return AuthenticationSchemeSelectorDelegate(context.Request);
		}
		return auth_schemes;
	}

	/// <summary>Waits for an incoming request and returns when one is received.</summary>
	/// <returns>An <see cref="T:System.Net.HttpListenerContext" /> object that represents a client request.</returns>
	/// <exception cref="T:System.Net.HttpListenerException">A Win32 function call failed. Check the exception's <see cref="P:System.Net.HttpListenerException.ErrorCode" /> property to determine the cause of the exception.</exception>
	/// <exception cref="T:System.InvalidOperationException">This object has not been started or is currently stopped.-or-The <see cref="T:System.Net.HttpListener" /> does not have any Uniform Resource Identifier (URI) prefixes to respond to. See Remarks.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	public HttpListenerContext GetContext()
	{
		if (prefixes.Count == 0)
		{
			throw new InvalidOperationException("Please, call AddPrefix before using this method.");
		}
		ListenerAsyncResult listenerAsyncResult = (ListenerAsyncResult)BeginGetContext(null, null);
		listenerAsyncResult.InGet = true;
		return EndGetContext(listenerAsyncResult);
	}

	/// <summary>Allows this instance to receive incoming requests.</summary>
	/// <exception cref="T:System.Net.HttpListenerException">A Win32 function call failed. Check the exception's <see cref="P:System.Net.HttpListenerException.ErrorCode" /> property to determine the cause of the exception.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object is closed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Start()
	{
		CheckDisposed();
		if (!listening)
		{
			EndPointManager.AddListener(this);
			listening = true;
		}
	}

	/// <summary>Causes this instance to stop receiving incoming requests.</summary>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Stop()
	{
		CheckDisposed();
		listening = false;
		Close(force: false);
	}

	/// <summary>Releases the resources held by this <see cref="T:System.Net.HttpListener" /> object.</summary>
	void IDisposable.Dispose()
	{
		if (!disposed)
		{
			Close(force: true);
			disposed = true;
		}
	}

	/// <summary>Waits for an incoming request as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.HttpListenerContext" /> object that represents a client request.</returns>
	public Task<HttpListenerContext> GetContextAsync()
	{
		return Task<HttpListenerContext>.Factory.FromAsync(BeginGetContext, EndGetContext, null);
	}

	internal void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().ToString());
		}
	}

	private HttpListenerContext GetContextFromQueue()
	{
		if (ctx_queue.Count == 0)
		{
			return null;
		}
		HttpListenerContext result = (HttpListenerContext)ctx_queue[0];
		ctx_queue.RemoveAt(0);
		return result;
	}

	internal void RegisterContext(HttpListenerContext context)
	{
		lock (_internalLock)
		{
			registry[context] = context;
		}
		ListenerAsyncResult listenerAsyncResult = null;
		lock (wait_queue)
		{
			if (wait_queue.Count == 0)
			{
				lock (ctx_queue)
				{
					ctx_queue.Add(context);
				}
			}
			else
			{
				listenerAsyncResult = (ListenerAsyncResult)wait_queue[0];
				wait_queue.RemoveAt(0);
			}
		}
		listenerAsyncResult?.Complete(context);
	}

	internal void UnregisterContext(HttpListenerContext context)
	{
		lock (_internalLock)
		{
			registry.Remove(context);
		}
		lock (ctx_queue)
		{
			int num = ctx_queue.IndexOf(context);
			if (num >= 0)
			{
				ctx_queue.RemoveAt(num);
			}
		}
	}

	internal void AddConnection(HttpConnection cnc)
	{
		connections[cnc] = cnc;
	}

	internal void RemoveConnection(HttpConnection cnc)
	{
		connections.Remove(cnc);
	}
}

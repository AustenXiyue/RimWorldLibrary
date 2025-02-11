using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Mono.Net.Security.Private;
using Mono.Security.Interface;
using Unity;

namespace System.Net.Security;

/// <summary>Provides a stream used for client-server communication that uses the Secure Socket Layer (SSL) security protocol to authenticate the server and optionally the client.</summary>
public class SslStream : AuthenticatedStream
{
	private MonoTlsProvider provider;

	private IMonoSslStream impl;

	internal IMonoSslStream Impl
	{
		get
		{
			CheckDisposed();
			return impl;
		}
	}

	internal MonoTlsProvider Provider
	{
		get
		{
			CheckDisposed();
			return provider;
		}
	}

	/// <summary>Gets the <see cref="T:System.Net.TransportContext" /> used for authentication using extended protection.</summary>
	/// <returns>The <see cref="T:System.Net.TransportContext" /> object that contains the channel binding token (CBT) used for extended protection.</returns>
	public TransportContext TransportContext
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether authentication was successful.</summary>
	/// <returns>true if successful authentication occurred; otherwise, false.</returns>
	public override bool IsAuthenticated => Impl.IsAuthenticated;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether both server and client have been authenticated.</summary>
	/// <returns>true if the server has been authenticated; otherwise false.</returns>
	public override bool IsMutuallyAuthenticated => Impl.IsMutuallyAuthenticated;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether this <see cref="T:System.Net.Security.SslStream" /> uses data encryption.</summary>
	/// <returns>true if data is encrypted before being transmitted over the network and decrypted when it reaches the remote endpoint; otherwise false.</returns>
	public override bool IsEncrypted => Impl.IsEncrypted;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the data sent using this stream is signed.</summary>
	/// <returns>true if the data is signed before being transmitted; otherwise false.</returns>
	public override bool IsSigned => Impl.IsSigned;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the local side of the connection used by this <see cref="T:System.Net.Security.SslStream" /> was authenticated as the server.</summary>
	/// <returns>true if the local endpoint was successfully authenticated as the server side of the authenticated connection; otherwise false.</returns>
	public override bool IsServer => Impl.IsServer;

	/// <summary>Gets a value that indicates the security protocol used to authenticate this connection.</summary>
	/// <returns>The <see cref="T:System.Security.Authentication.SslProtocols" /> value that represents the protocol used for authentication.</returns>
	public virtual SslProtocols SslProtocol => Impl.SslProtocol;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the certificate revocation list is checked during the certificate validation process.</summary>
	/// <returns>true if the certificate revocation list is checked; otherwise, false.</returns>
	public virtual bool CheckCertRevocationStatus => Impl.CheckCertRevocationStatus;

	/// <summary>Gets the certificate used to authenticate the local endpoint.</summary>
	/// <returns>An X509Certificate object that represents the certificate supplied for authentication or null if no certificate was supplied.</returns>
	/// <exception cref="T:System.InvalidOperationException">Authentication failed or has not occurred.</exception>
	public virtual X509Certificate LocalCertificate => Impl.LocalCertificate;

	/// <summary>Gets the certificate used to authenticate the remote endpoint.</summary>
	/// <returns>An X509Certificate object that represents the certificate supplied for authentication or null if no certificate was supplied.</returns>
	/// <exception cref="T:System.InvalidOperationException">Authentication failed or has not occurred.</exception>
	public virtual X509Certificate RemoteCertificate => Impl.RemoteCertificate;

	/// <summary>Gets a value that identifies the bulk encryption algorithm used by this <see cref="T:System.Net.Security.SslStream" />.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.CipherAlgorithmType" /> value.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Net.Security.SslStream.CipherAlgorithm" /> property was accessed before the completion of the authentication process or the authentication process failed.</exception>
	public virtual System.Security.Authentication.CipherAlgorithmType CipherAlgorithm => Impl.CipherAlgorithm;

	/// <summary>Gets a value that identifies the strength of the cipher algorithm used by this <see cref="T:System.Net.Security.SslStream" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that specifies the strength of the algorithm, in bits.</returns>
	public virtual int CipherStrength => Impl.CipherStrength;

	/// <summary>Gets the algorithm used for generating message authentication codes (MACs).</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.HashAlgorithmType" /> value.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Net.Security.SslStream.HashAlgorithm" /> property was accessed before the completion of the authentication process or the authentication process failed.</exception>
	public virtual System.Security.Authentication.HashAlgorithmType HashAlgorithm => Impl.HashAlgorithm;

	/// <summary>Gets a value that identifies the strength of the hash algorithm used by this instance.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that specifies the strength of the <see cref="T:System.Security.Authentication.HashAlgorithmType" /> algorithm, in bits. Valid values are 128 or 160.</returns>
	public virtual int HashStrength => Impl.HashStrength;

	/// <summary>Gets the key exchange algorithm used by this <see cref="T:System.Net.Security.SslStream" />.</summary>
	/// <returns>An <see cref="T:System.Security.Authentication.ExchangeAlgorithmType" /> value.</returns>
	public virtual System.Security.Authentication.ExchangeAlgorithmType KeyExchangeAlgorithm => Impl.KeyExchangeAlgorithm;

	/// <summary>Gets a value that identifies the strength of the key exchange algorithm used by this instance.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that specifies the strength of the <see cref="T:System.Security.Authentication.ExchangeAlgorithmType" /> algorithm, in bits.</returns>
	public virtual int KeyExchangeStrength => Impl.KeyExchangeStrength;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the underlying stream is seekable.</summary>
	/// <returns>This property always returns false.</returns>
	public override bool CanSeek => false;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the underlying stream is readable.</summary>
	/// <returns>true if authentication has occurred and the underlying stream is readable; otherwise false.</returns>
	public override bool CanRead
	{
		get
		{
			if (impl != null)
			{
				return impl.CanRead;
			}
			return false;
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the underlying stream supports time-outs.</summary>
	/// <returns>true if the underlying stream supports time-outs; otherwise, false.</returns>
	public override bool CanTimeout => base.InnerStream.CanTimeout;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether the underlying stream is writable.</summary>
	/// <returns>true if authentication has occurred and the underlying stream is writable; otherwise false.</returns>
	public override bool CanWrite
	{
		get
		{
			if (impl != null)
			{
				return impl.CanWrite;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the amount of time a read operation blocks waiting for data.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time that elapses before a synchronous read operation fails.</returns>
	public override int ReadTimeout
	{
		get
		{
			return Impl.ReadTimeout;
		}
		set
		{
			Impl.ReadTimeout = value;
		}
	}

	/// <summary>Gets or sets the amount of time a write operation blocks waiting for data.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that specifies the amount of time that elapses before a synchronous write operation fails. </returns>
	public override int WriteTimeout
	{
		get
		{
			return Impl.WriteTimeout;
		}
		set
		{
			Impl.WriteTimeout = value;
		}
	}

	/// <summary>Gets the length of the underlying stream.</summary>
	/// <returns>A <see cref="T:System.Int64" />.The length of the underlying stream.</returns>
	/// <exception cref="T:System.NotSupportedException">Getting the value of this property is not supported when the underlying stream is a <see cref="T:System.Net.Sockets.NetworkStream" />.</exception>
	public override long Length => Impl.Length;

	/// <summary>Gets or sets the current position in the underlying stream.</summary>
	/// <returns>A <see cref="T:System.Int64" />.The current position in the underlying stream.</returns>
	/// <exception cref="T:System.NotSupportedException">Setting this property is not supported.-or-Getting the value of this property is not supported when the underlying stream is a <see cref="T:System.Net.Sockets.NetworkStream" />.</exception>
	public override long Position
	{
		get
		{
			return Impl.Position;
		}
		set
		{
			throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
		}
	}

	private static MonoTlsProvider GetProvider()
	{
		return MonoTlsProviderFactory.GetProvider();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Security.SslStream" /> class using the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="innerStream">A <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="innerStream" /> is not readable.-or-<paramref name="innerStream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="innerStream" /> is null.-or-<paramref name="innerStream" /> is equal to <see cref="F:System.IO.Stream.Null" />.</exception>
	public SslStream(Stream innerStream)
		: this(innerStream, leaveInnerStreamOpen: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Security.SslStream" /> class using the specified <see cref="T:System.IO.Stream" /> and stream closure behavior.</summary>
	/// <param name="innerStream">A <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data.</param>
	/// <param name="leaveInnerStreamOpen">A Boolean value that indicates the closure behavior of the <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data. This parameter indicates if the inner stream is left open.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="innerStream" /> is not readable.-or-<paramref name="innerStream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="innerStream" /> is null.-or-<paramref name="innerStream" /> is equal to <see cref="F:System.IO.Stream.Null" />.</exception>
	public SslStream(Stream innerStream, bool leaveInnerStreamOpen)
		: base(innerStream, leaveInnerStreamOpen)
	{
		provider = GetProvider();
		impl = provider.CreateSslStreamInternal(this, innerStream, leaveInnerStreamOpen, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Security.SslStream" /> class using the specified <see cref="T:System.IO.Stream" />, stream closure behavior and certificate validation delegate.</summary>
	/// <param name="innerStream">A <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data.</param>
	/// <param name="leaveInnerStreamOpen">A Boolean value that indicates the closure behavior of the <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data. This parameter indicates if the inner stream is left open.</param>
	/// <param name="userCertificateValidationCallback">A <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" /> delegate responsible for validating the certificate supplied by the remote party.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="innerStream" /> is not readable.-or-<paramref name="innerStream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="innerStream" /> is null.-or-<paramref name="innerStream" /> is equal to <see cref="F:System.IO.Stream.Null" />.</exception>
	public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
		: this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Security.SslStream" /> class using the specified <see cref="T:System.IO.Stream" />, stream closure behavior, certificate validation delegate and certificate selection delegate.</summary>
	/// <param name="innerStream">A <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data.</param>
	/// <param name="leaveInnerStreamOpen">A Boolean value that indicates the closure behavior of the <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data. This parameter indicates if the inner stream is left open.</param>
	/// <param name="userCertificateValidationCallback">A <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" /> delegate responsible for validating the certificate supplied by the remote party.</param>
	/// <param name="userCertificateSelectionCallback">A <see cref="T:System.Net.Security.LocalCertificateSelectionCallback" /> delegate responsible for selecting the certificate used for authentication.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="innerStream" /> is not readable.-or-<paramref name="innerStream" /> is not writable.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="innerStream" /> is null.-or-<paramref name="innerStream" /> is equal to <see cref="F:System.IO.Stream.Null" />.</exception>
	public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
		: base(innerStream, leaveInnerStreamOpen)
	{
		provider = GetProvider();
		MonoTlsSettings monoTlsSettings = MonoTlsSettings.CopyDefaultSettings();
		monoTlsSettings.RemoteCertificateValidationCallback = CallbackHelpers.PublicToMono(userCertificateValidationCallback);
		monoTlsSettings.ClientCertificateSelectionCallback = CallbackHelpers.PublicToMono(userCertificateSelectionCallback);
		impl = provider.CreateSslStream(innerStream, leaveInnerStreamOpen, monoTlsSettings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Security.SslStream" /> class using the specified <see cref="T:System.IO.Stream" /></summary>
	/// <param name="innerStream">A <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data.</param>
	/// <param name="leaveInnerStreamOpen">A Boolean value that indicates the closure behavior of the <see cref="T:System.IO.Stream" /> object used by the <see cref="T:System.Net.Security.SslStream" /> for sending and receiving data. This parameter indicates if the inner stream is left open.</param>
	/// <param name="userCertificateValidationCallback">A <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" /> delegate responsible for validating the certificate supplied by the remote party.</param>
	/// <param name="userCertificateSelectionCallback">A <see cref="T:System.Net.Security.LocalCertificateSelectionCallback" /> delegate responsible for selecting the certificate used for authentication.</param>
	/// <param name="encryptionPolicy">The <see cref="T:System.Net.Security.EncryptionPolicy" /> to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="innerStream" /> is not readable.-or-<paramref name="innerStream" /> is not writable.-or-<paramref name="encryptionPolicy" /> is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="innerStream" /> is null.-or-<paramref name="innerStream" /> is equal to <see cref="F:System.IO.Stream.Null" />.</exception>
	[System.MonoLimitation("encryptionPolicy is ignored")]
	public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
		: this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback)
	{
	}

	internal SslStream(Stream innerStream, bool leaveInnerStreamOpen, MonoTlsProvider provider, MonoTlsSettings settings)
		: base(innerStream, leaveInnerStreamOpen)
	{
		this.provider = provider;
		impl = provider.CreateSslStreamInternal(this, innerStream, leaveInnerStreamOpen, settings);
	}

	internal static IMonoSslStream CreateMonoSslStream(Stream innerStream, bool leaveInnerStreamOpen, MonoTlsProvider provider, MonoTlsSettings settings)
	{
		return new SslStream(innerStream, leaveInnerStreamOpen, provider, settings).Impl;
	}

	/// <summary>Called by clients to authenticate the server and optionally the client in a client-server connection.</summary>
	/// <param name="targetHost">The name of the server that shares this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="targetHost" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Server authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public virtual void AuthenticateAsClient(string targetHost)
	{
		Impl.AuthenticateAsClient(targetHost);
	}

	/// <summary>Called by clients to authenticate the server and optionally the client in a client-server connection. The authentication process uses the specified certificate collection and SSL protocol.</summary>
	/// <param name="targetHost">The name of the server that will share this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <param name="clientCertificates">The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> that contains client certificates.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" /> value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	public virtual void AuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
	{
		Impl.AuthenticateAsClient(targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
	}

	/// <summary>Called by clients to begin an asynchronous operation to authenticate the server and optionally the client.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that indicates the status of the asynchronous operation. </returns>
	/// <param name="targetHost">The name of the server that shares this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the authentication is complete. </param>
	/// <param name="asyncState">A user-defined object that contains information about the operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="targetHost" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Server authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginAuthenticateAsClient(targetHost, asyncCallback, asyncState);
	}

	/// <summary>Called by clients to begin an asynchronous operation to authenticate the server and optionally the client using the specified certificates and security protocol.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that indicates the status of the asynchronous operation. </returns>
	/// <param name="targetHost">The name of the server that shares this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <param name="clientCertificates">The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> containing client certificates.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" /> value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the authentication is complete. </param>
	/// <param name="asyncState">A user-defined object that contains information about the operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="targetHost" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enabledSslProtocols" /> is not a valid <see cref="T:System.Security.Authentication.SslProtocols" />  value.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Server authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginAuthenticateAsClient(targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
	}

	/// <summary>Ends a pending asynchronous server authentication operation started with a previous call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsServer" />.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsServer" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsServer" />.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no pending server authentication to complete.</exception>
	public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
	{
		Impl.EndAuthenticateAsClient(asyncResult);
	}

	/// <summary>Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificate.</summary>
	/// <param name="serverCertificate">The certificate used to authenticate the server.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverCertificate" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Client authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="Overload:System.Net.Security.SslStream.AuthenticateAsServer" /> method is not supported on Windows 95, Windows 98, or Windows Millennium.</exception>
	public virtual void AuthenticateAsServer(X509Certificate serverCertificate)
	{
		Impl.AuthenticateAsServer(serverCertificate);
	}

	/// <summary>Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificates, requirements and security protocol.</summary>
	/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
	/// <param name="clientCertificateRequired">A <see cref="T:System.Boolean" /> value that specifies whether the client must supply a certificate for authentication.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" />  value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverCertificate" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enabledSslProtocols" /> is not a valid <see cref="T:System.Security.Authentication.SslProtocols" /> value.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Client authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="Overload:System.Net.Security.SslStream.AuthenticateAsServer" /> method is not supported on Windows 95, Windows 98, or Windows Millennium.</exception>
	public virtual void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
	{
		Impl.AuthenticateAsServer(serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
	}

	/// <summary>Called by servers to begin an asynchronous operation to authenticate the client and optionally the server in a client-server connection.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object indicating the status of the asynchronous operation. </returns>
	/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the authentication is complete.</param>
	/// <param name="asyncState">A user-defined object that contains information about the operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverCertificate" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Client authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsServer" /> method is not supported on Windows 95, Windows 98, or Windows Millennium.</exception>
	public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginAuthenticateAsServer(serverCertificate, asyncCallback, asyncState);
	}

	/// <summary>Called by servers to begin an asynchronous operation to authenticate the server and optionally the client using the specified certificates, requirements and security protocol.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that indicates the status of the asynchronous operation. </returns>
	/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
	/// <param name="clientCertificateRequired">A <see cref="T:System.Boolean" /> value that specifies whether the client must supply a certificate for authentication.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" />  value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the authentication is complete. </param>
	/// <param name="asyncState">A user-defined object that contains information about the operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverCertificate" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="enabledSslProtocols" /> is not a valid <see cref="T:System.Security.Authentication.SslProtocols" /> value.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Server authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsServer" /> method is not supported on Windows 95, Windows 98, or Windows Millennium.</exception>
	public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginAuthenticateAsServer(serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, asyncCallback, asyncState);
	}

	/// <summary>Ends a pending asynchronous client authentication operation started with a previous call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsClient" />.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsClient" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="Overload:System.Net.Security.SslStream.BeginAuthenticateAsClient" />.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no pending client authentication to complete.</exception>
	public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
	{
		Impl.EndAuthenticateAsServer(asyncResult);
	}

	/// <summary>Called by clients to authenticate the server and optionally the client in a client-server connection as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />The task object representing the asynchronous operation.</returns>
	/// <param name="targetHost">The name of the server that shares this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="targetHost" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Server authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	public virtual Task AuthenticateAsClientAsync(string targetHost)
	{
		return Impl.AuthenticateAsClientAsync(targetHost);
	}

	/// <summary>Called by clients to authenticate the server and optionally the client in a client-server connection as an asynchronous operation. The authentication process uses the specified certificate collection and SSL protocol.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />The task object representing the asynchronous operation.</returns>
	/// <param name="targetHost">The name of the server that will share this <see cref="T:System.Net.Security.SslStream" />.</param>
	/// <param name="clientCertificates">The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> that contains client certificates.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" /> value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	public virtual Task AuthenticateAsClientAsync(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
	{
		return Impl.AuthenticateAsClientAsync(targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation);
	}

	/// <summary>Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificate as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />The task object representing the asynchronous operation.</returns>
	/// <param name="serverCertificate">The certificate used to authenticate the server.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serverCertificate" /> is null.</exception>
	/// <exception cref="T:System.Security.Authentication.AuthenticationException">The authentication failed and left this object in an unusable state.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has already occurred.-or-Client authentication using this <see cref="T:System.Net.Security.SslStream" /> was tried previously.-or- Authentication is already in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The <see cref="Overload:System.Net.Security.SslStream.AuthenticateAsServerAsync" /> method is not supported on Windows 95, Windows 98, or Windows Millennium.</exception>
	public virtual Task AuthenticateAsServerAsync(X509Certificate serverCertificate)
	{
		return Impl.AuthenticateAsServerAsync(serverCertificate);
	}

	/// <summary>Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificates, requirements and security protocol as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />The task object representing the asynchronous operation.</returns>
	/// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
	/// <param name="clientCertificateRequired">A <see cref="T:System.Boolean" /> value that specifies whether the client must supply a certificate for authentication.</param>
	/// <param name="enabledSslProtocols">The <see cref="T:System.Security.Authentication.SslProtocols" />  value that represents the protocol used for authentication.</param>
	/// <param name="checkCertificateRevocation">A <see cref="T:System.Boolean" /> value that specifies whether the certificate revocation list is checked during authentication.</param>
	public virtual Task AuthenticateAsServerAsync(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
	{
		return Impl.AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation);
	}

	public virtual Task ShutdownAsync()
	{
		return Impl.ShutdownAsync();
	}

	/// <summary>Sets the length of the underlying stream.</summary>
	/// <param name="value">An <see cref="T:System.Int64" /> value that specifies the length of the stream.</param>
	public override void SetLength(long value)
	{
		Impl.SetLength(value);
	}

	/// <summary>Throws a <see cref="T:System.NotSupportedException" />.</summary>
	/// <returns>Always throws a <see cref="T:System.NotSupportedException" />.</returns>
	/// <param name="offset">This value is ignored.</param>
	/// <param name="origin">This value is ignored.</param>
	/// <exception cref="T:System.NotSupportedException">Seeking is not supported by <see cref="T:System.Net.Security.SslStream" /> objects.</exception>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(global::SR.GetString("This stream does not support seek operations."));
	}

	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		return base.InnerStream.FlushAsync(cancellationToken);
	}

	/// <summary>Causes any buffered data to be written to the underlying device.</summary>
	public override void Flush()
	{
		base.InnerStream.Flush();
	}

	private void CheckDisposed()
	{
		if (impl == null)
		{
			throw new ObjectDisposedException("SslStream");
		}
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.Security.SslStream" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (impl != null && disposing)
			{
				impl.Dispose();
				impl = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Reads data from this stream and stores it in the specified array.</summary>
	/// <returns>A <see cref="T:System.Int32" /> value that specifies the number of bytes read. When there is no more data to be read, returns 0.</returns>
	/// <param name="buffer">A <see cref="T:System.Byte" /> array that receives the bytes read from this stream.</param>
	/// <param name="offset">A <see cref="T:System.Int32" /> that contains the zero-based location in <paramref name="buffer" /> at which to begin storing the data read from this stream.</param>
	/// <param name="count">A <see cref="T:System.Int32" /> that contains the maximum number of bytes to read from this stream.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" />
	///   <paramref name="&lt;" />
	///   <paramref name="0" />.<paramref name="-or-" /><paramref name="offset" /> &gt; the length of <paramref name="buffer" />.-or-<paramref name="offset" /> + count &gt; the length of <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.IO.IOException">The read operation failed. Check the inner exception, if present to determine the cause of the failure.</exception>
	/// <exception cref="T:System.NotSupportedException">There is already a read operation in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override int Read(byte[] buffer, int offset, int count)
	{
		return Impl.Read(buffer, offset, count);
	}

	/// <summary>Writes the specified data to this stream.</summary>
	/// <param name="buffer">A <see cref="T:System.Byte" /> array that supplies the bytes written to the stream.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">The write operation failed.</exception>
	/// <exception cref="T:System.NotSupportedException">There is already a write operation in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public void Write(byte[] buffer)
	{
		Impl.Write(buffer);
	}

	/// <summary>Write the specified number of <see cref="T:System.Byte" />s to the underlying stream using the specified buffer and offset.</summary>
	/// <param name="buffer">A <see cref="T:System.Byte" /> array that supplies the bytes written to the stream.</param>
	/// <param name="offset">A <see cref="T:System.Int32" /> that contains the zero-based location in <paramref name="buffer" /> at which to begin reading bytes to be written to the stream.</param>
	/// <param name="count">A <see cref="T:System.Int32" /> that contains the number of bytes to read from <paramref name="buffer" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" />
	///   <paramref name="&lt;" />
	///   <paramref name="0" />.<paramref name="-or-" /><paramref name="offset" /> &gt; the length of <paramref name="buffer" />.-or-<paramref name="offset" /> + count &gt; the length of <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.IO.IOException">The write operation failed.</exception>
	/// <exception cref="T:System.NotSupportedException">There is already a write operation in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override void Write(byte[] buffer, int offset, int count)
	{
		Impl.Write(buffer, offset, count);
	}

	/// <summary>Begins an asynchronous read operation that reads data from the stream and stores it in the specified array.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object that indicates the status of the asynchronous operation. </returns>
	/// <param name="buffer">A <see cref="T:System.Byte" /> array that receives the bytes read from the stream.</param>
	/// <param name="offset">The zero-based location in <paramref name="buffer" /> at which to begin storing the data read from this stream.</param>
	/// <param name="count">The maximum number of bytes to read from the stream.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the read operation is complete. </param>
	/// <param name="asyncState">A user-defined object that contains information about the read operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" />
	///   <paramref name="&lt;" />
	///   <paramref name="0" />.<paramref name="-or-" /><paramref name="offset" /> &gt; the length of <paramref name="buffer" />.-or-<paramref name="offset" /> + count &gt; the length of <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.IO.IOException">The read operation failed.-or-Encryption is in use, but the data could not be decrypted.</exception>
	/// <exception cref="T:System.NotSupportedException">There is already a read operation in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginRead(buffer, offset, count, asyncCallback, asyncState);
	}

	/// <summary>Ends an asynchronous read operation started with a previous call to <see cref="M:System.Net.Security.SslStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</summary>
	/// <returns>A <see cref="T:System.Int32" /> value that specifies the number of bytes read from the underlying stream.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to <see cref="M:System.Net.Security.SslStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /></param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="M:System.Net.Security.SslStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no pending read operation to complete.</exception>
	/// <exception cref="T:System.IO.IOException">The read operation failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override int EndRead(IAsyncResult asyncResult)
	{
		return Impl.EndRead(asyncResult);
	}

	/// <summary>Begins an asynchronous write operation that writes <see cref="T:System.Byte" />s from the specified buffer to the stream.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> object indicating the status of the asynchronous operation. </returns>
	/// <param name="buffer">A <see cref="T:System.Byte" /> array that supplies the bytes to be written to the stream.</param>
	/// <param name="offset">The zero-based location in <paramref name="buffer" /> at which to begin reading bytes to be written to the stream.</param>
	/// <param name="count">An <see cref="T:System.Int32" /> value that specifies the number of bytes to read from <paramref name="buffer" />.</param>
	/// <param name="asyncCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the write operation is complete. </param>
	/// <param name="asyncState">A user-defined object that contains information about the write operation. This object is passed to the <paramref name="asyncCallback" /> delegate when the operation completes.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="offset" />
	///   <paramref name="&lt;" />
	///   <paramref name="0" />.<paramref name="-or-" /><paramref name="offset" /> &gt; the length of <paramref name="buffer" />.-or-<paramref name="offset" /> + count &gt; the length of <paramref name="buffer" />.</exception>
	/// <exception cref="T:System.IO.IOException">The write operation failed.</exception>
	/// <exception cref="T:System.NotSupportedException">There is already a write operation in progress.</exception>
	/// <exception cref="T:System.ObjectDisposedException">This object has been closed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
	{
		return Impl.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
	}

	/// <summary>Ends an asynchronous write operation started with a previous call to <see cref="M:System.Net.Security.SslStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</summary>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to <see cref="M:System.Net.Security.SslStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /></param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="asyncResult" /> was not created by a call to <see cref="M:System.Net.Security.SslStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">There is no pending write operation to complete.</exception>
	/// <exception cref="T:System.IO.IOException">The write operation failed.</exception>
	/// <exception cref="T:System.InvalidOperationException">Authentication has not occurred.</exception>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		Impl.EndWrite(asyncResult);
	}

	public virtual void AuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task AuthenticateAsClientAsync(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	public virtual void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual Task AuthenticateAsServerAsync(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
	public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}

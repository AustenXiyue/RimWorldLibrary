using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Security;
using System.Threading;

namespace System.Net;

/// <summary>Manages the collection of <see cref="T:System.Net.ServicePoint" /> objects.</summary>
public class ServicePointManager
{
	private class SPKey
	{
		private Uri uri;

		private Uri proxy;

		private bool use_connect;

		public Uri Uri => uri;

		public bool UseConnect => use_connect;

		public bool UsesProxy => proxy != null;

		public SPKey(Uri uri, Uri proxy, bool use_connect)
		{
			this.uri = uri;
			this.proxy = proxy;
			this.use_connect = use_connect;
		}

		public override int GetHashCode()
		{
			return ((23 * 31 + (use_connect ? 1 : 0)) * 31 + uri.GetHashCode()) * 31 + ((proxy != null) ? proxy.GetHashCode() : 0);
		}

		public override bool Equals(object obj)
		{
			SPKey sPKey = obj as SPKey;
			if (obj == null)
			{
				return false;
			}
			if (!uri.Equals(sPKey.uri))
			{
				return false;
			}
			if (use_connect != sPKey.use_connect || UsesProxy != sPKey.UsesProxy)
			{
				return false;
			}
			if (UsesProxy && !proxy.Equals(sPKey.proxy))
			{
				return false;
			}
			return true;
		}
	}

	private static HybridDictionary servicePoints;

	private static ICertificatePolicy policy;

	private static int defaultConnectionLimit;

	private static int maxServicePointIdleTime;

	private static int maxServicePoints;

	private static int dnsRefreshTimeout;

	private static bool _checkCRL;

	private static SecurityProtocolType _securityProtocol;

	private static bool expectContinue;

	private static bool useNagle;

	private static ServerCertValidationCallback server_cert_cb;

	private static bool tcp_keepalive;

	private static int tcp_keepalive_time;

	private static int tcp_keepalive_interval;

	/// <summary>The default number of non-persistent connections (4) allowed on a <see cref="T:System.Net.ServicePoint" /> object connected to an HTTP/1.0 or later server. This field is constant but is no longer used in the .NET Framework 2.0.</summary>
	public const int DefaultNonPersistentConnectionLimit = 4;

	/// <summary>The default number of persistent connections (2) allowed on a <see cref="T:System.Net.ServicePoint" /> object connected to an HTTP/1.1 or later server. This field is constant and is used to initialize the <see cref="P:System.Net.ServicePointManager.DefaultConnectionLimit" /> property if the value of the <see cref="P:System.Net.ServicePointManager.DefaultConnectionLimit" /> property has not been set either directly or through configuration.</summary>
	public const int DefaultPersistentConnectionLimit = 2;

	private const string configKey = "system.net/connectionManagement";

	private static ConnectionManagementData manager;

	/// <summary>Gets or sets policy for server certificates.</summary>
	/// <returns>An object that implements the <see cref="T:System.Net.ICertificatePolicy" /> interface.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use ServerCertificateValidationCallback instead", false)]
	public static ICertificatePolicy CertificatePolicy
	{
		get
		{
			if (policy == null)
			{
				Interlocked.CompareExchange(ref policy, new DefaultCertificatePolicy(), null);
			}
			return policy;
		}
		set
		{
			policy = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that indicates whether the certificate is checked against the certificate authority revocation list.</summary>
	/// <returns>true if the certificate revocation list is checked; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO("CRL checks not implemented")]
	public static bool CheckCertificateRevocationList
	{
		get
		{
			return _checkCRL;
		}
		set
		{
			_checkCRL = false;
		}
	}

	/// <summary>Gets or sets the maximum number of concurrent connections allowed by a <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>The maximum number of concurrent connections allowed by a <see cref="T:System.Net.ServicePoint" /> object. The default value is 2.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Net.ServicePointManager.DefaultConnectionLimit" /> is less than or equal to 0. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static int DefaultConnectionLimit
	{
		get
		{
			return defaultConnectionLimit;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			defaultConnectionLimit = value;
			if (manager != null)
			{
				manager.Add("*", defaultConnectionLimit);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates how long a Domain Name Service (DNS) resolution is considered valid.</summary>
	/// <returns>The time-out value, in milliseconds. A value of -1 indicates an infinite time-out period. The default value is 120,000 milliseconds (two minutes).</returns>
	public static int DnsRefreshTimeout
	{
		get
		{
			return dnsRefreshTimeout;
		}
		set
		{
			dnsRefreshTimeout = Math.Max(-1, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a Domain Name Service (DNS) resolution rotates among the applicable Internet Protocol (IP) addresses.</summary>
	/// <returns>false if a DNS resolution always returns the first IP address for a particular host; otherwise true. The default is false.</returns>
	[System.MonoTODO]
	public static bool EnableDnsRoundRobin
	{
		get
		{
			throw GetMustImplement();
		}
		set
		{
			throw GetMustImplement();
		}
	}

	/// <summary>Gets or sets the maximum idle time of a <see cref="T:System.Net.ServicePoint" /> object.</summary>
	/// <returns>The maximum idle time, in milliseconds, of a <see cref="T:System.Net.ServicePoint" /> object. The default value is 100,000 milliseconds (100 seconds).</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Net.ServicePointManager.MaxServicePointIdleTime" /> is less than <see cref="F:System.Threading.Timeout.Infinite" /> or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static int MaxServicePointIdleTime
	{
		get
		{
			return maxServicePointIdleTime;
		}
		set
		{
			if (value < -2 || value > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			maxServicePointIdleTime = value;
		}
	}

	/// <summary>Gets or sets the maximum number of <see cref="T:System.Net.ServicePoint" /> objects to maintain at any time.</summary>
	/// <returns>The maximum number of <see cref="T:System.Net.ServicePoint" /> objects to maintain. The default value is 0, which means there is no limit to the number of <see cref="T:System.Net.ServicePoint" /> objects.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="P:System.Net.ServicePointManager.MaxServicePoints" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static int MaxServicePoints
	{
		get
		{
			return maxServicePoints;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("value");
			}
			maxServicePoints = value;
		}
	}

	[System.MonoTODO]
	public static bool ReusePort
	{
		get
		{
			return false;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the security protocol used by the <see cref="T:System.Net.ServicePoint" /> objects managed by the <see cref="T:System.Net.ServicePointManager" /> object.</summary>
	/// <returns>One of the values defined in the <see cref="T:System.Net.SecurityProtocolType" /> enumeration.</returns>
	/// <exception cref="T:System.NotSupportedException">The value specified to set the property is not a valid <see cref="T:System.Net.SecurityProtocolType" /> enumeration value. </exception>
	public static SecurityProtocolType SecurityProtocol
	{
		get
		{
			return _securityProtocol;
		}
		set
		{
			_securityProtocol = value;
		}
	}

	internal static ServerCertValidationCallback ServerCertValidationCallback => server_cert_cb;

	/// <summary>Gets or sets the callback to validate a server certificate.</summary>
	/// <returns>A <see cref="T:System.Net.Security.RemoteCertificateValidationCallback" />. The default value is null.</returns>
	public static RemoteCertificateValidationCallback ServerCertificateValidationCallback
	{
		get
		{
			if (server_cert_cb == null)
			{
				return null;
			}
			return server_cert_cb.ValidationCallback;
		}
		set
		{
			if (value == null)
			{
				server_cert_cb = null;
			}
			else
			{
				server_cert_cb = new ServerCertValidationCallback(value);
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Net.Security.EncryptionPolicy" /> for this <see cref="T:System.Net.ServicePointManager" /> instance.</summary>
	/// <returns>The encryption policy to use for this <see cref="T:System.Net.ServicePointManager" /> instance.</returns>
	[System.MonoTODO("Always returns EncryptionPolicy.RequireEncryption.")]
	public static EncryptionPolicy EncryptionPolicy => EncryptionPolicy.RequireEncryption;

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that determines whether 100-Continue behavior is used.</summary>
	/// <returns>true to enable 100-Continue behavior. The default value is true.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static bool Expect100Continue
	{
		get
		{
			return expectContinue;
		}
		set
		{
			expectContinue = value;
		}
	}

	/// <summary>Determines whether the Nagle algorithm is used by the service points managed by this <see cref="T:System.Net.ServicePointManager" /> object.</summary>
	/// <returns>true to use the Nagle algorithm; otherwise, false. The default value is true.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static bool UseNagleAlgorithm
	{
		get
		{
			return useNagle;
		}
		set
		{
			useNagle = value;
		}
	}

	internal static bool DisableStrongCrypto => false;

	internal static bool DisableSendAuxRecord => false;

	static ServicePointManager()
	{
		servicePoints = new HybridDictionary();
		defaultConnectionLimit = 2;
		maxServicePointIdleTime = 100000;
		maxServicePoints = 0;
		dnsRefreshTimeout = 120000;
		_checkCRL = false;
		_securityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		expectContinue = true;
		if (ConfigurationManager.GetSection("system.net/connectionManagement") is ConnectionManagementSection connectionManagementSection)
		{
			manager = new ConnectionManagementData(null);
			foreach (ConnectionManagementElement item in connectionManagementSection.ConnectionManagement)
			{
				manager.Add(item.Address, item.MaxConnection);
			}
			defaultConnectionLimit = (int)manager.GetMaxConnections("*");
		}
		else
		{
			manager = (ConnectionManagementData)ConfigurationSettings.GetConfig("system.net/connectionManagement");
			if (manager != null)
			{
				defaultConnectionLimit = (int)manager.GetMaxConnections("*");
			}
		}
	}

	private ServicePointManager()
	{
	}

	internal static ICertificatePolicy GetLegacyCertificatePolicy()
	{
		return policy;
	}

	private static Exception GetMustImplement()
	{
		return new NotImplementedException();
	}

	/// <summary>Enables or disables the keep-alive option on a TCP connection.</summary>
	/// <param name="enabled">If set to true, then the TCP keep-alive option on a TCP connection will be enabled using the specified <paramref name="keepAliveTime " />and <paramref name="keepAliveInterval" /> values. If set to false, then the TCP keep-alive option is disabled and the remaining parameters are ignored.The default value is false.</param>
	/// <param name="keepAliveTime">Specifies the timeout, in milliseconds, with no activity until the first keep-alive packet is sent.The value must be greater than 0.  If a value of less than or equal to zero is passed an <see cref="T:System.ArgumentOutOfRangeException" /> is thrown.</param>
	/// <param name="keepAliveInterval">Specifies the interval, in milliseconds, between when successive keep-alive packets are sent if no acknowledgement is received.The value must be greater than 0.  If a value of less than or equal to zero is passed an <see cref="T:System.ArgumentOutOfRangeException" /> is thrown.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for <paramref name="keepAliveTime" /> or <paramref name="keepAliveInterval" /> parameter is less than or equal to 0.</exception>
	public static void SetTcpKeepAlive(bool enabled, int keepAliveTime, int keepAliveInterval)
	{
		if (enabled)
		{
			if (keepAliveTime <= 0)
			{
				throw new ArgumentOutOfRangeException("keepAliveTime", "Must be greater than 0");
			}
			if (keepAliveInterval <= 0)
			{
				throw new ArgumentOutOfRangeException("keepAliveInterval", "Must be greater than 0");
			}
		}
		tcp_keepalive = enabled;
		tcp_keepalive_time = keepAliveTime;
		tcp_keepalive_interval = keepAliveInterval;
	}

	/// <summary>Finds an existing <see cref="T:System.Net.ServicePoint" /> object or creates a new <see cref="T:System.Net.ServicePoint" /> object to manage communications with the specified <see cref="T:System.Uri" /> object.</summary>
	/// <returns>The <see cref="T:System.Net.ServicePoint" /> object that manages communications for the request.</returns>
	/// <param name="address">The <see cref="T:System.Uri" /> object of the Internet resource to contact. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of <see cref="T:System.Net.ServicePoint" /> objects defined in <see cref="P:System.Net.ServicePointManager.MaxServicePoints" /> has been reached. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static ServicePoint FindServicePoint(Uri address)
	{
		return FindServicePoint(address, null);
	}

	/// <summary>Finds an existing <see cref="T:System.Net.ServicePoint" /> object or creates a new <see cref="T:System.Net.ServicePoint" /> object to manage communications with the specified Uniform Resource Identifier (URI).</summary>
	/// <returns>The <see cref="T:System.Net.ServicePoint" /> object that manages communications for the request.</returns>
	/// <param name="uriString">The URI of the Internet resource to be contacted. </param>
	/// <param name="proxy">The proxy data for this request. </param>
	/// <exception cref="T:System.UriFormatException">The URI specified in <paramref name="uriString" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of <see cref="T:System.Net.ServicePoint" /> objects defined in <see cref="P:System.Net.ServicePointManager.MaxServicePoints" /> has been reached. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static ServicePoint FindServicePoint(string uriString, IWebProxy proxy)
	{
		return FindServicePoint(new Uri(uriString), proxy);
	}

	/// <summary>Finds an existing <see cref="T:System.Net.ServicePoint" /> object or creates a new <see cref="T:System.Net.ServicePoint" /> object to manage communications with the specified <see cref="T:System.Uri" /> object.</summary>
	/// <returns>The <see cref="T:System.Net.ServicePoint" /> object that manages communications for the request.</returns>
	/// <param name="address">A <see cref="T:System.Uri" /> object that contains the address of the Internet resource to contact. </param>
	/// <param name="proxy">The proxy data for this request. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The maximum number of <see cref="T:System.Net.ServicePoint" /> objects defined in <see cref="P:System.Net.ServicePointManager.MaxServicePoints" /> has been reached. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static ServicePoint FindServicePoint(Uri address, IWebProxy proxy)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		Uri uri = new Uri(address.Scheme + "://" + address.Authority);
		bool flag = false;
		bool flag2 = false;
		if (proxy != null && !proxy.IsBypassed(address))
		{
			flag = true;
			bool num = address.Scheme == "https";
			address = proxy.GetProxy(address);
			if (address.Scheme != "http")
			{
				throw new NotSupportedException("Proxy scheme not supported.");
			}
			if (num && address.Scheme == "http")
			{
				flag2 = true;
			}
		}
		address = new Uri(address.Scheme + "://" + address.Authority);
		ServicePoint servicePoint = null;
		SPKey key = new SPKey(uri, flag ? address : null, flag2);
		lock (servicePoints)
		{
			if (servicePoints[key] is ServicePoint result)
			{
				return result;
			}
			if (maxServicePoints > 0 && servicePoints.Count >= maxServicePoints)
			{
				throw new InvalidOperationException("maximum number of service points reached");
			}
			string hostOrIP = address.ToString();
			int maxConnections = (int)manager.GetMaxConnections(hostOrIP);
			servicePoint = new ServicePoint(address, maxConnections, maxServicePointIdleTime);
			servicePoint.Expect100Continue = expectContinue;
			servicePoint.UseNagleAlgorithm = useNagle;
			servicePoint.UsesProxy = flag;
			servicePoint.UseConnect = flag2;
			servicePoint.SetTcpKeepAlive(tcp_keepalive, tcp_keepalive_time, tcp_keepalive_interval);
			servicePoints.Add(key, servicePoint);
			return servicePoint;
		}
	}

	internal static void CloseConnectionGroup(string connectionGroupName)
	{
		lock (servicePoints)
		{
			foreach (ServicePoint value in servicePoints.Values)
			{
				value.CloseConnectionGroup(connectionGroupName);
			}
		}
	}
}

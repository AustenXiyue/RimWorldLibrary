using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using Mono.Net;

namespace System.Net;

/// <summary>Contains HTTP proxy settings for the <see cref="T:System.Net.WebRequest" /> class.</summary>
[Serializable]
public class WebProxy : IAutoWebProxy, IWebProxy, ISerializable
{
	private bool _UseRegistry;

	private bool _BypassOnLocal;

	private bool m_EnableAutoproxy;

	private Uri _ProxyAddress;

	private ArrayList _BypassList;

	private ICredentials _Credentials;

	private Regex[] _RegExBypassList;

	private Hashtable _ProxyHostAddresses;

	private AutoWebProxyScriptEngine m_ScriptEngine;

	/// <summary>Gets or sets the address of the proxy server.</summary>
	/// <returns>A <see cref="T:System.Uri" /> instance that contains the address of the proxy server.</returns>
	public Uri Address
	{
		get
		{
			return _ProxyAddress;
		}
		set
		{
			_UseRegistry = false;
			DeleteScriptEngine();
			_ProxyHostAddresses = null;
			_ProxyAddress = value;
		}
	}

	internal bool AutoDetect
	{
		set
		{
			if (ScriptEngine == null)
			{
				ScriptEngine = new AutoWebProxyScriptEngine(this, useRegistry: false);
			}
			ScriptEngine.AutomaticallyDetectSettings = value;
		}
	}

	internal Uri ScriptLocation
	{
		set
		{
			if (ScriptEngine == null)
			{
				ScriptEngine = new AutoWebProxyScriptEngine(this, useRegistry: false);
			}
			ScriptEngine.AutomaticConfigurationScript = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to bypass the proxy server for local addresses.</summary>
	/// <returns>true to bypass the proxy server for local addresses; otherwise, false. The default value is false.</returns>
	public bool BypassProxyOnLocal
	{
		get
		{
			return _BypassOnLocal;
		}
		set
		{
			_UseRegistry = false;
			DeleteScriptEngine();
			_BypassOnLocal = value;
		}
	}

	/// <summary>Gets or sets an array of addresses that do not use the proxy server.</summary>
	/// <returns>An array that contains a list of regular expressions that describe URIs that do not use the proxy server when accessed.</returns>
	public string[] BypassList
	{
		get
		{
			if (_BypassList == null)
			{
				_BypassList = new ArrayList();
			}
			return (string[])_BypassList.ToArray(typeof(string));
		}
		set
		{
			_UseRegistry = false;
			DeleteScriptEngine();
			_BypassList = new ArrayList(value);
			UpdateRegExList(canThrow: true);
		}
	}

	/// <summary>Gets or sets the credentials to submit to the proxy server for authentication.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> instance that contains the credentials to submit to the proxy server for authentication.</returns>
	/// <exception cref="T:System.InvalidOperationException">You attempted to set this property when the <see cref="P:System.Net.WebProxy.UseDefaultCredentials" />  property was set to true. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ICredentials Credentials
	{
		get
		{
			return _Credentials;
		}
		set
		{
			_Credentials = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether the <see cref="P:System.Net.CredentialCache.DefaultCredentials" /> are sent with requests.</summary>
	/// <returns>true if the default credentials are used; otherwise, false. The default value is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">You attempted to set this property when the <see cref="P:System.Net.WebProxy.Credentials" /> property contains credentials other than the default credentials. For more information, see the Remarks section.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public bool UseDefaultCredentials
	{
		get
		{
			if (!(Credentials is SystemNetworkCredential))
			{
				return false;
			}
			return true;
		}
		set
		{
			_Credentials = (value ? CredentialCache.DefaultCredentials : null);
		}
	}

	/// <summary>Gets a list of addresses that do not use the proxy server.</summary>
	/// <returns>An <see cref="T:System.Collections.ArrayList" /> that contains a list of <see cref="P:System.Net.WebProxy.BypassList" /> arrays that represents URIs that do not use the proxy server when accessed.</returns>
	public ArrayList BypassArrayList
	{
		get
		{
			if (_BypassList == null)
			{
				_BypassList = new ArrayList();
			}
			return _BypassList;
		}
	}

	internal AutoWebProxyScriptEngine ScriptEngine
	{
		get
		{
			return m_ScriptEngine;
		}
		set
		{
			m_ScriptEngine = value;
		}
	}

	/// <summary>Initializes an empty instance of the <see cref="T:System.Net.WebProxy" /> class.</summary>
	public WebProxy()
		: this((Uri)null, BypassOnLocal: false, (string[])null, (ICredentials)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class from the specified <see cref="T:System.Uri" /> instance.</summary>
	/// <param name="Address">A <see cref="T:System.Uri" /> instance that contains the address of the proxy server. </param>
	public WebProxy(Uri Address)
		: this(Address, BypassOnLocal: false, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the <see cref="T:System.Uri" /> instance and bypass setting.</summary>
	/// <param name="Address">A <see cref="T:System.Uri" /> instance that contains the address of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	public WebProxy(Uri Address, bool BypassOnLocal)
		: this(Address, BypassOnLocal, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified <see cref="T:System.Uri" /> instance, bypass setting, and list of URIs to bypass.</summary>
	/// <param name="Address">A <see cref="T:System.Uri" /> instance that contains the address of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	/// <param name="BypassList">An array of regular expression strings that contains the URIs of the servers to bypass. </param>
	public WebProxy(Uri Address, bool BypassOnLocal, string[] BypassList)
		: this(Address, BypassOnLocal, BypassList, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified <see cref="T:System.Uri" /> instance, bypass setting, list of URIs to bypass, and credentials.</summary>
	/// <param name="Address">A <see cref="T:System.Uri" /> instance that contains the address of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	/// <param name="BypassList">An array of regular expression strings that contains the URIs of the servers to bypass. </param>
	/// <param name="Credentials">An <see cref="T:System.Net.ICredentials" /> instance to submit to the proxy server for authentication. </param>
	public WebProxy(Uri Address, bool BypassOnLocal, string[] BypassList, ICredentials Credentials)
	{
		_ProxyAddress = Address;
		_BypassOnLocal = BypassOnLocal;
		if (BypassList != null)
		{
			_BypassList = new ArrayList(BypassList);
			UpdateRegExList(canThrow: true);
		}
		_Credentials = Credentials;
		m_EnableAutoproxy = true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified host and port number.</summary>
	/// <param name="Host">The name of the proxy host. </param>
	/// <param name="Port">The port number on <paramref name="Host" /> to use. </param>
	/// <exception cref="T:System.UriFormatException">The URI formed by combining <paramref name="Host" /> and <paramref name="Port" /> is not a valid URI. </exception>
	public WebProxy(string Host, int Port)
		: this(new Uri("http://" + Host + ":" + Port.ToString(CultureInfo.InvariantCulture)), BypassOnLocal: false, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified URI.</summary>
	/// <param name="Address">The URI of the proxy server. </param>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="Address" /> is an invalid URI. </exception>
	public WebProxy(string Address)
		: this(CreateProxyUri(Address), BypassOnLocal: false, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified URI and bypass setting.</summary>
	/// <param name="Address">The URI of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="Address" /> is an invalid URI. </exception>
	public WebProxy(string Address, bool BypassOnLocal)
		: this(CreateProxyUri(Address), BypassOnLocal, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified URI, bypass setting, and list of URIs to bypass.</summary>
	/// <param name="Address">The URI of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	/// <param name="BypassList">An array of regular expression strings that contain the URIs of the servers to bypass. </param>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="Address" /> is an invalid URI. </exception>
	public WebProxy(string Address, bool BypassOnLocal, string[] BypassList)
		: this(CreateProxyUri(Address), BypassOnLocal, BypassList, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebProxy" /> class with the specified URI, bypass setting, list of URIs to bypass, and credentials.</summary>
	/// <param name="Address">The URI of the proxy server. </param>
	/// <param name="BypassOnLocal">true to bypass the proxy for local addresses; otherwise, false. </param>
	/// <param name="BypassList">An array of regular expression strings that contains the URIs of the servers to bypass. </param>
	/// <param name="Credentials">An <see cref="T:System.Net.ICredentials" /> instance to submit to the proxy server for authentication. </param>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="Address" /> is an invalid URI. </exception>
	public WebProxy(string Address, bool BypassOnLocal, string[] BypassList, ICredentials Credentials)
		: this(CreateProxyUri(Address), BypassOnLocal, BypassList, Credentials)
	{
	}

	internal void CheckForChanges()
	{
		if (ScriptEngine != null)
		{
			ScriptEngine.CheckForChanges();
		}
	}

	/// <summary>Returns the proxied URI for a request.</summary>
	/// <returns>The <see cref="T:System.Uri" /> instance of the Internet resource, if the resource is on the bypass list; otherwise, the <see cref="T:System.Uri" /> instance of the proxy.</returns>
	/// <param name="destination">The <see cref="T:System.Uri" /> instance of the requested Internet resource. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destination" /> parameter is null. </exception>
	public Uri GetProxy(Uri destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (GetProxyAuto(destination, out var proxyUri))
		{
			return proxyUri;
		}
		if (IsBypassedManual(destination))
		{
			return destination;
		}
		Hashtable proxyHostAddresses = _ProxyHostAddresses;
		Uri uri = ((proxyHostAddresses != null) ? (proxyHostAddresses[destination.Scheme] as Uri) : _ProxyAddress);
		if (!(uri != null))
		{
			return destination;
		}
		return uri;
	}

	private static Uri CreateProxyUri(string address)
	{
		if (address == null)
		{
			return null;
		}
		if (address.IndexOf("://") == -1)
		{
			address = "http://" + address;
		}
		return new Uri(address);
	}

	private void UpdateRegExList(bool canThrow)
	{
		Regex[] array = null;
		ArrayList bypassList = _BypassList;
		try
		{
			if (bypassList != null && bypassList.Count > 0)
			{
				array = new Regex[bypassList.Count];
				for (int i = 0; i < bypassList.Count; i++)
				{
					array[i] = new Regex((string)bypassList[i], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
				}
			}
		}
		catch
		{
			if (!canThrow)
			{
				_RegExBypassList = null;
				return;
			}
			throw;
		}
		_RegExBypassList = array;
	}

	private bool IsMatchInBypassList(Uri input)
	{
		UpdateRegExList(canThrow: false);
		if (_RegExBypassList == null)
		{
			return false;
		}
		string input2 = input.Scheme + "://" + input.Host + ((!input.IsDefaultPort) ? (":" + input.Port) : "");
		for (int i = 0; i < _BypassList.Count; i++)
		{
			if (_RegExBypassList[i].IsMatch(input2))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsLocal(Uri host)
	{
		string host2 = host.Host;
		if (IPAddress.TryParse(host2, out var address))
		{
			if (!IPAddress.IsLoopback(address))
			{
				return NclUtilities.IsAddressLocal(address);
			}
			return true;
		}
		int num = host2.IndexOf('.');
		if (num == -1)
		{
			return true;
		}
		string text = "." + IPGlobalProperties.InternalGetIPGlobalProperties().DomainName;
		if (text != null && text.Length == host2.Length - num && string.Compare(text, 0, host2, num, text.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return true;
		}
		return false;
	}

	private bool IsLocalInProxyHash(Uri host)
	{
		Hashtable proxyHostAddresses = _ProxyHostAddresses;
		if (proxyHostAddresses != null && (Uri)proxyHostAddresses[host.Scheme] == null)
		{
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether to use the proxy server for the specified host.</summary>
	/// <returns>true if the proxy server should not be used for <paramref name="host" />; otherwise, false.</returns>
	/// <param name="host">The <see cref="T:System.Uri" /> instance of the host to check for proxy use. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="host" /> parameter is null. </exception>
	public bool IsBypassed(Uri host)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (IsBypassedAuto(host, out var isBypassed))
		{
			return isBypassed;
		}
		return IsBypassedManual(host);
	}

	private bool IsBypassedManual(Uri host)
	{
		if (host.IsLoopback)
		{
			return true;
		}
		if ((!(_ProxyAddress == null) || _ProxyHostAddresses != null) && (!_BypassOnLocal || !IsLocal(host)) && !IsMatchInBypassList(host))
		{
			return IsLocalInProxyHash(host);
		}
		return true;
	}

	/// <summary>Reads the Internet Explorer nondynamic proxy settings.</summary>
	/// <returns>A <see cref="T:System.Net.WebProxy" /> instance that contains the nondynamic proxy settings from Internet Explorer 5.5 and later.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.WebPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("This method has been deprecated. Please use the proxy selected for you by default. http://go.microsoft.com/fwlink/?linkid=14202")]
	public static WebProxy GetDefaultProxy()
	{
		return new WebProxy(enableAutoproxy: true);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Net.WebProxy" /> class using previously serialized content.</summary>
	/// <param name="serializationInfo">The serialization data. </param>
	/// <param name="streamingContext">The context for the serialized data. </param>
	protected WebProxy(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		bool flag = false;
		try
		{
			flag = serializationInfo.GetBoolean("_UseRegistry");
		}
		catch
		{
		}
		if (flag)
		{
			UnsafeUpdateFromRegistry();
			return;
		}
		_ProxyAddress = (Uri)serializationInfo.GetValue("_ProxyAddress", typeof(Uri));
		_BypassOnLocal = serializationInfo.GetBoolean("_BypassOnLocal");
		_BypassList = (ArrayList)serializationInfo.GetValue("_BypassList", typeof(ArrayList));
		try
		{
			UseDefaultCredentials = serializationInfo.GetBoolean("_UseDefaultCredentials");
		}
		catch
		{
		}
	}

	/// <summary>Creates the serialization data and context that are used by the system to serialize a <see cref="T:System.Net.WebProxy" /> object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that indicates the destination for this serialization. </param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data that is needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		serializationInfo.AddValue("_BypassOnLocal", _BypassOnLocal);
		serializationInfo.AddValue("_ProxyAddress", _ProxyAddress);
		serializationInfo.AddValue("_BypassList", _BypassList);
		serializationInfo.AddValue("_UseDefaultCredentials", UseDefaultCredentials);
		if (_UseRegistry)
		{
			serializationInfo.AddValue("_UseRegistry", value: true);
		}
	}

	public static IWebProxy CreateDefaultProxy()
	{
		if (Platform.IsMacOS)
		{
			IWebProxy defaultProxy = CFNetwork.GetDefaultProxy();
			if (defaultProxy != null)
			{
				return defaultProxy;
			}
		}
		return new WebProxy(enableAutoproxy: true);
	}

	internal WebProxy(bool enableAutoproxy)
	{
		m_EnableAutoproxy = enableAutoproxy;
		UnsafeUpdateFromRegistry();
	}

	internal void DeleteScriptEngine()
	{
		if (ScriptEngine != null)
		{
			ScriptEngine.Close();
			ScriptEngine = null;
		}
	}

	internal void UnsafeUpdateFromRegistry()
	{
		_UseRegistry = true;
		ScriptEngine = new AutoWebProxyScriptEngine(this, useRegistry: true);
		WebProxyData webProxyData = ScriptEngine.GetWebProxyData();
		Update(webProxyData);
	}

	internal void Update(WebProxyData webProxyData)
	{
		lock (this)
		{
			_BypassOnLocal = webProxyData.bypassOnLocal;
			_ProxyAddress = webProxyData.proxyAddress;
			_ProxyHostAddresses = webProxyData.proxyHostAddresses;
			_BypassList = webProxyData.bypassList;
			ScriptEngine.AutomaticallyDetectSettings = m_EnableAutoproxy && webProxyData.automaticallyDetectSettings;
			ScriptEngine.AutomaticConfigurationScript = (m_EnableAutoproxy ? webProxyData.scriptLocation : null);
		}
	}

	ProxyChain IAutoWebProxy.GetProxies(Uri destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return new ProxyScriptChain(this, destination);
	}

	private bool GetProxyAuto(Uri destination, out Uri proxyUri)
	{
		proxyUri = null;
		if (ScriptEngine == null)
		{
			return false;
		}
		IList<string> proxyList = null;
		if (!ScriptEngine.GetProxies(destination, out proxyList))
		{
			return false;
		}
		if (proxyList.Count > 0)
		{
			if (AreAllBypassed(proxyList, checkFirstOnly: true))
			{
				proxyUri = destination;
			}
			else
			{
				proxyUri = ProxyUri(proxyList[0]);
			}
		}
		return true;
	}

	private bool IsBypassedAuto(Uri destination, out bool isBypassed)
	{
		isBypassed = true;
		if (ScriptEngine == null)
		{
			return false;
		}
		if (!ScriptEngine.GetProxies(destination, out var proxyList))
		{
			return false;
		}
		if (proxyList.Count == 0)
		{
			isBypassed = false;
		}
		else
		{
			isBypassed = AreAllBypassed(proxyList, checkFirstOnly: true);
		}
		return true;
	}

	internal Uri[] GetProxiesAuto(Uri destination, ref int syncStatus)
	{
		if (ScriptEngine == null)
		{
			return null;
		}
		IList<string> proxyList = null;
		if (!ScriptEngine.GetProxies(destination, out proxyList, ref syncStatus))
		{
			return null;
		}
		Uri[] array = null;
		if (proxyList.Count == 0)
		{
			array = new Uri[0];
		}
		else if (AreAllBypassed(proxyList, checkFirstOnly: false))
		{
			array = new Uri[1];
		}
		else
		{
			array = new Uri[proxyList.Count];
			for (int i = 0; i < proxyList.Count; i++)
			{
				array[i] = ProxyUri(proxyList[i]);
			}
		}
		return array;
	}

	internal void AbortGetProxiesAuto(ref int syncStatus)
	{
		if (ScriptEngine != null)
		{
			ScriptEngine.Abort(ref syncStatus);
		}
	}

	internal Uri GetProxyAutoFailover(Uri destination)
	{
		if (IsBypassedManual(destination))
		{
			return null;
		}
		Uri result = _ProxyAddress;
		Hashtable proxyHostAddresses = _ProxyHostAddresses;
		if (proxyHostAddresses != null)
		{
			result = proxyHostAddresses[destination.Scheme] as Uri;
		}
		return result;
	}

	private static bool AreAllBypassed(IEnumerable<string> proxies, bool checkFirstOnly)
	{
		bool flag = true;
		foreach (string proxy in proxies)
		{
			flag = string.IsNullOrEmpty(proxy);
			if (checkFirstOnly || !flag)
			{
				break;
			}
		}
		return flag;
	}

	private static Uri ProxyUri(string proxyName)
	{
		if (proxyName != null && proxyName.Length != 0)
		{
			return new Uri("http://" + proxyName);
		}
		return null;
	}
}

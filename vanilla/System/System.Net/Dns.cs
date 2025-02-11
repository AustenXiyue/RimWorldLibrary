using System.Collections;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Mono.Net.Dns;

namespace System.Net;

/// <summary>Provides simple domain name resolution functionality.</summary>
public static class Dns
{
	private delegate IPHostEntry GetHostByNameCallback(string hostName);

	private delegate IPHostEntry ResolveCallback(string hostName);

	private delegate IPHostEntry GetHostEntryNameCallback(string hostName);

	private delegate IPHostEntry GetHostEntryIPCallback(IPAddress hostAddress);

	private delegate IPAddress[] GetHostAddressesCallback(string hostName);

	private static bool use_mono_dns;

	private static SimpleResolver resolver;

	internal static bool UseMonoDns => use_mono_dns;

	static Dns()
	{
		if (Environment.GetEnvironmentVariable("MONO_DNS") != null)
		{
			resolver = new SimpleResolver();
			use_mono_dns = true;
		}
	}

	private static void OnCompleted(object sender, SimpleResolverEventArgs e)
	{
		DnsAsyncResult dnsAsyncResult = (DnsAsyncResult)e.UserToken;
		IPHostEntry hostEntry = e.HostEntry;
		if (hostEntry == null || e.ResolverError != 0)
		{
			dnsAsyncResult.SetCompleted(synch: false, new Exception("Error: " + e.ResolverError));
		}
		else
		{
			dnsAsyncResult.SetCompleted(synch: false, hostEntry);
		}
	}

	private static IAsyncResult BeginAsyncCallAddresses(string host, AsyncCallback callback, object state)
	{
		SimpleResolverEventArgs simpleResolverEventArgs = new SimpleResolverEventArgs();
		simpleResolverEventArgs.Completed += OnCompleted;
		simpleResolverEventArgs.HostName = host;
		DnsAsyncResult dnsAsyncResult2 = (DnsAsyncResult)(simpleResolverEventArgs.UserToken = new DnsAsyncResult(callback, state));
		if (!resolver.GetHostAddressesAsync(simpleResolverEventArgs))
		{
			dnsAsyncResult2.SetCompleted(synch: true, simpleResolverEventArgs.HostEntry);
		}
		return dnsAsyncResult2;
	}

	private static IAsyncResult BeginAsyncCall(string host, AsyncCallback callback, object state)
	{
		SimpleResolverEventArgs simpleResolverEventArgs = new SimpleResolverEventArgs();
		simpleResolverEventArgs.Completed += OnCompleted;
		simpleResolverEventArgs.HostName = host;
		DnsAsyncResult dnsAsyncResult2 = (DnsAsyncResult)(simpleResolverEventArgs.UserToken = new DnsAsyncResult(callback, state));
		if (!resolver.GetHostEntryAsync(simpleResolverEventArgs))
		{
			dnsAsyncResult2.SetCompleted(synch: true, simpleResolverEventArgs.HostEntry);
		}
		return dnsAsyncResult2;
	}

	private static IPHostEntry EndAsyncCall(DnsAsyncResult ares)
	{
		if (ares == null)
		{
			throw new ArgumentException("Invalid asyncResult");
		}
		if (!ares.IsCompleted)
		{
			ares.AsyncWaitHandle.WaitOne();
		}
		if (ares.Exception != null)
		{
			throw ares.Exception;
		}
		IPHostEntry hostEntry = ares.HostEntry;
		if (hostEntry == null || hostEntry.AddressList == null || hostEntry.AddressList.Length == 0)
		{
			Error_11001(hostEntry.HostName);
		}
		return hostEntry;
	}

	/// <summary>Begins an asynchronous request for <see cref="T:System.Net.IPHostEntry" /> information about the specified DNS host name.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that references the asynchronous request.</returns>
	/// <param name="hostName">The DNS name of the host. </param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete.</param>
	/// <param name="stateObject">A user-defined object that contains information about the operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error was encountered executing the DNS query. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use BeginGetHostEntry instead")]
	public static IAsyncResult BeginGetHostByName(string hostName, AsyncCallback requestCallback, object stateObject)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (use_mono_dns)
		{
			return BeginAsyncCall(hostName, requestCallback, stateObject);
		}
		return new GetHostByNameCallback(GetHostByName).BeginInvoke(hostName, requestCallback, stateObject);
	}

	/// <summary>Begins an asynchronous request to resolve a DNS host name or IP address to an <see cref="T:System.Net.IPAddress" /> instance.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that references the asynchronous request.</returns>
	/// <param name="hostName">The DNS name of the host. </param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="stateObject">A user-defined object that contains information about the operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">The caller does not have permission to access DNS information. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use BeginGetHostEntry instead")]
	public static IAsyncResult BeginResolve(string hostName, AsyncCallback requestCallback, object stateObject)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (use_mono_dns)
		{
			return BeginAsyncCall(hostName, requestCallback, stateObject);
		}
		return new ResolveCallback(Resolve).BeginInvoke(hostName, requestCallback, stateObject);
	}

	/// <summary>Asynchronously returns the Internet Protocol (IP) addresses for the specified host.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that references the asynchronous request.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="state">A user-defined object that contains information about the operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostNameOrAddress" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hostNameOrAddress" /> is an invalid IP address.</exception>
	public static IAsyncResult BeginGetHostAddresses(string hostNameOrAddress, AsyncCallback requestCallback, object state)
	{
		if (hostNameOrAddress == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
		{
			throw new ArgumentException("Addresses 0.0.0.0 (IPv4) and ::0 (IPv6) are unspecified addresses. You cannot use them as target address.", "hostNameOrAddress");
		}
		if (use_mono_dns)
		{
			return BeginAsyncCallAddresses(hostNameOrAddress, requestCallback, state);
		}
		return new GetHostAddressesCallback(GetHostAddresses).BeginInvoke(hostNameOrAddress, requestCallback, state);
	}

	/// <summary>Asynchronously resolves a host name or IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that references the asynchronous request.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="stateObject">A user-defined object that contains information about the operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostNameOrAddress" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hostNameOrAddress" /> is an invalid IP address.</exception>
	public static IAsyncResult BeginGetHostEntry(string hostNameOrAddress, AsyncCallback requestCallback, object stateObject)
	{
		if (hostNameOrAddress == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
		{
			throw new ArgumentException("Addresses 0.0.0.0 (IPv4) and ::0 (IPv6) are unspecified addresses. You cannot use them as target address.", "hostNameOrAddress");
		}
		if (use_mono_dns)
		{
			return BeginAsyncCall(hostNameOrAddress, requestCallback, stateObject);
		}
		return new GetHostEntryNameCallback(GetHostEntry).BeginInvoke(hostNameOrAddress, requestCallback, stateObject);
	}

	/// <summary>Asynchronously resolves an IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
	/// <returns>An <see cref="T:System.IAsyncResult" /> instance that references the asynchronous request.</returns>
	/// <param name="address">The IP address to resolve.</param>
	/// <param name="requestCallback">An <see cref="T:System.AsyncCallback" /> delegate that references the method to invoke when the operation is complete. </param>
	/// <param name="stateObject">A user-defined object that contains information about the operation. This object is passed to the <paramref name="requestCallback" /> delegate when the operation is complete.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="address" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is an invalid IP address.</exception>
	public static IAsyncResult BeginGetHostEntry(IPAddress address, AsyncCallback requestCallback, object stateObject)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (use_mono_dns)
		{
			return BeginAsyncCall(address.ToString(), requestCallback, stateObject);
		}
		return new GetHostEntryIPCallback(GetHostEntry).BeginInvoke(address, requestCallback, stateObject);
	}

	/// <summary>Ends an asynchronous request for DNS information.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> object that contains DNS information about a host.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance that is returned by a call to the <see cref="M:System.Net.Dns.BeginGetHostByName(System.String,System.AsyncCallback,System.Object)" /> method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("Use EndGetHostEntry instead")]
	public static IPHostEntry EndGetHostByName(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (use_mono_dns)
		{
			return EndAsyncCall(asyncResult as DnsAsyncResult);
		}
		return ((GetHostByNameCallback)((AsyncResult)asyncResult).AsyncDelegate).EndInvoke(asyncResult);
	}

	/// <summary>Ends an asynchronous request for DNS information.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> object that contains DNS information about a host.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance that is returned by a call to the <see cref="M:System.Net.Dns.BeginResolve(System.String,System.AsyncCallback,System.Object)" /> method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("Use EndGetHostEntry instead")]
	public static IPHostEntry EndResolve(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (use_mono_dns)
		{
			return EndAsyncCall(asyncResult as DnsAsyncResult);
		}
		return ((ResolveCallback)((AsyncResult)asyncResult).AsyncDelegate).EndInvoke(asyncResult);
	}

	/// <summary>Ends an asynchronous request for DNS information.</summary>
	/// <returns>An array of type <see cref="T:System.Net.IPAddress" /> that holds the IP addresses for the host specified by the <paramref name="hostNameOrAddress" /> parameter of <see cref="M:System.Net.Dns.BeginGetHostAddresses(System.String,System.AsyncCallback,System.Object)" />.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to the <see cref="M:System.Net.Dns.BeginGetHostAddresses(System.String,System.AsyncCallback,System.Object)" /> method.</param>
	public static IPAddress[] EndGetHostAddresses(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (use_mono_dns)
		{
			return EndAsyncCall(asyncResult as DnsAsyncResult)?.AddressList;
		}
		return ((GetHostAddressesCallback)((AsyncResult)asyncResult).AsyncDelegate).EndInvoke(asyncResult);
	}

	/// <summary>Ends an asynchronous request for DNS information.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host.</returns>
	/// <param name="asyncResult">An <see cref="T:System.IAsyncResult" /> instance returned by a call to an <see cref="Overload:System.Net.Dns.BeginGetHostEntry" /> method.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="asyncResult" /> is null. </exception>
	public static IPHostEntry EndGetHostEntry(IAsyncResult asyncResult)
	{
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		if (use_mono_dns)
		{
			return EndAsyncCall(asyncResult as DnsAsyncResult);
		}
		AsyncResult asyncResult2 = (AsyncResult)asyncResult;
		if (asyncResult2.AsyncDelegate is GetHostEntryIPCallback)
		{
			return ((GetHostEntryIPCallback)asyncResult2.AsyncDelegate).EndInvoke(asyncResult);
		}
		return ((GetHostEntryNameCallback)asyncResult2.AsyncDelegate).EndInvoke(asyncResult);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list, int hint);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetHostByAddr_internal(string addr, out string h_name, out string[] h_aliases, out string[] h_addr_list, int hint);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool GetHostName_internal(out string h_name);

	private static void Error_11001(string hostName)
	{
		throw new SocketException(11001, $"Could not resolve host '{hostName}'");
	}

	private static IPHostEntry hostent_to_IPHostEntry(string originalHostName, string h_name, string[] h_aliases, string[] h_addrlist)
	{
		IPHostEntry iPHostEntry = new IPHostEntry();
		ArrayList arrayList = new ArrayList();
		iPHostEntry.HostName = h_name;
		iPHostEntry.Aliases = h_aliases;
		for (int i = 0; i < h_addrlist.Length; i++)
		{
			try
			{
				IPAddress iPAddress = IPAddress.Parse(h_addrlist[i]);
				if ((Socket.SupportsIPv6 && iPAddress.AddressFamily == AddressFamily.InterNetworkV6) || (Socket.SupportsIPv4 && iPAddress.AddressFamily == AddressFamily.InterNetwork))
				{
					arrayList.Add(iPAddress);
				}
			}
			catch (ArgumentNullException)
			{
			}
		}
		if (arrayList.Count == 0)
		{
			Error_11001(originalHostName);
		}
		iPHostEntry.AddressList = arrayList.ToArray(typeof(IPAddress)) as IPAddress[];
		return iPHostEntry;
	}

	/// <summary>Creates an <see cref="T:System.Net.IPHostEntry" /> instance from the specified <see cref="T:System.Net.IPAddress" />.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" />.An <see cref="T:System.Net.IPHostEntry" /> instance.</returns>
	/// <param name="address">An <see cref="T:System.Net.IPAddress" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="address" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use GetHostEntry instead")]
	public static IPHostEntry GetHostByAddress(IPAddress address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return GetHostByAddressFromString(address.ToString(), parse: false);
	}

	/// <summary>Creates an <see cref="T:System.Net.IPHostEntry" /> instance from an IP address.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance.</returns>
	/// <param name="address">An IP address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="address" />. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not a valid IP address. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use GetHostEntry instead")]
	public static IPHostEntry GetHostByAddress(string address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return GetHostByAddressFromString(address, parse: true);
	}

	private static IPHostEntry GetHostByAddressFromString(string address, bool parse)
	{
		if (address.Equals("0.0.0.0"))
		{
			address = "127.0.0.1";
			parse = false;
		}
		if (parse)
		{
			IPAddress.Parse(address);
		}
		if (!GetHostByAddr_internal(address, out var h_name, out var h_aliases, out var h_addr_list, Socket.FamilyHint))
		{
			Error_11001(address);
		}
		return hostent_to_IPHostEntry(address, h_name, h_aliases, h_addr_list);
	}

	/// <summary>Resolves a host name or IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="hostNameOrAddress" />.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="hostNameOrAddress" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> parameter is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error was encountered when resolving the <paramref name="hostNameOrAddress" /> parameter. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="hostNameOrAddress" /> parameter is an invalid IP address. </exception>
	public static IPHostEntry GetHostEntry(string hostNameOrAddress)
	{
		if (hostNameOrAddress == null)
		{
			throw new ArgumentNullException("hostNameOrAddress");
		}
		if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
		{
			throw new ArgumentException("Addresses 0.0.0.0 (IPv4) and ::0 (IPv6) are unspecified addresses. You cannot use them as target address.", "hostNameOrAddress");
		}
		if (hostNameOrAddress.Length > 0 && IPAddress.TryParse(hostNameOrAddress, out var address))
		{
			return GetHostEntry(address);
		}
		return GetHostByName(hostNameOrAddress);
	}

	/// <summary>Resolves an IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="address" />.</returns>
	/// <param name="address">An IP address.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="address" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is an invalid IP address.</exception>
	public static IPHostEntry GetHostEntry(IPAddress address)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		return GetHostByAddressFromString(address.ToString(), parse: false);
	}

	/// <summary>Returns the Internet Protocol (IP) addresses for the specified host.</summary>
	/// <returns>An array of type <see cref="T:System.Net.IPAddress" /> that holds the IP addresses for the host that is specified by the <paramref name="hostNameOrAddress" /> parameter.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostNameOrAddress" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hostNameOrAddress" /> is an invalid IP address.</exception>
	public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
	{
		if (hostNameOrAddress == null)
		{
			throw new ArgumentNullException("hostNameOrAddress");
		}
		if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
		{
			throw new ArgumentException("Addresses 0.0.0.0 (IPv4) and ::0 (IPv6) are unspecified addresses. You cannot use them as target address.", "hostNameOrAddress");
		}
		if (hostNameOrAddress.Length > 0 && IPAddress.TryParse(hostNameOrAddress, out var address))
		{
			return new IPAddress[1] { address };
		}
		return GetHostEntry(hostNameOrAddress).AddressList;
	}

	/// <summary>Gets the DNS information for the specified DNS host name.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> object that contains host information for the address specified in <paramref name="hostName" />.</returns>
	/// <param name="hostName">The DNS name of the host. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostName" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostName" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use GetHostEntry instead")]
	public static IPHostEntry GetHostByName(string hostName)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		if (!GetHostByName_internal(hostName, out var h_name, out var h_aliases, out var h_addr_list, Socket.FamilyHint))
		{
			Error_11001(hostName);
		}
		return hostent_to_IPHostEntry(hostName, h_name, h_aliases, h_addr_list);
	}

	/// <summary>Gets the host name of the local computer.</summary>
	/// <returns>A string that contains the DNS host name of the local computer.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving the local host name. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static string GetHostName()
	{
		if (!GetHostName_internal(out var h_name))
		{
			Error_11001(h_name);
		}
		return h_name;
	}

	/// <summary>Resolves a DNS host name or IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="hostName" />.</returns>
	/// <param name="hostName">A DNS-style host name or IP address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostName" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostName" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use GetHostEntry instead")]
	public static IPHostEntry Resolve(string hostName)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		IPHostEntry iPHostEntry = null;
		try
		{
			iPHostEntry = GetHostByAddress(hostName);
		}
		catch
		{
		}
		if (iPHostEntry == null)
		{
			iPHostEntry = GetHostByName(hostName);
		}
		return iPHostEntry;
	}

	/// <summary>Returns the Internet Protocol (IP) addresses for the specified host as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an array of type <see cref="T:System.Net.IPAddress" /> that holds the IP addresses for the host that is specified by the <paramref name="hostNameOrAddress" /> parameter.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostNameOrAddress" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hostNameOrAddress" /> is an invalid IP address.</exception>
	public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress)
	{
		return Task<IPAddress[]>.Factory.FromAsync(BeginGetHostAddresses, EndGetHostAddresses, hostNameOrAddress, null);
	}

	/// <summary>Resolves an IP address to an <see cref="T:System.Net.IPHostEntry" /> instance as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="address" />.</returns>
	/// <param name="address">An IP address.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="address" />. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is an invalid IP address.</exception>
	public static Task<IPHostEntry> GetHostEntryAsync(IPAddress address)
	{
		return Task<IPHostEntry>.Factory.FromAsync(BeginGetHostEntry, EndGetHostEntry, address, null);
	}

	/// <summary>Resolves a host name or IP address to an <see cref="T:System.Net.IPHostEntry" /> instance as an asynchronous operation.</summary>
	/// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result" /> property on the task object returns an <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="hostNameOrAddress" />.</returns>
	/// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="hostNameOrAddress" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> parameter is greater than 255 characters. </exception>
	/// <exception cref="T:System.Net.Sockets.SocketException">An error was encountered when resolving the <paramref name="hostNameOrAddress" /> parameter. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="hostNameOrAddress" /> parameter is an invalid IP address. </exception>
	public static Task<IPHostEntry> GetHostEntryAsync(string hostNameOrAddress)
	{
		return Task<IPHostEntry>.Factory.FromAsync(BeginGetHostEntry, EndGetHostEntry, hostNameOrAddress, null);
	}
}

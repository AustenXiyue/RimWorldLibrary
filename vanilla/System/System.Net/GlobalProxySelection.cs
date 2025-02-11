namespace System.Net;

/// <summary>Contains a global default proxy instance for all HTTP requests.</summary>
[Obsolete("This class has been deprecated. Please use WebRequest.DefaultWebProxy instead to access and set the global default proxy. Use 'null' instead of GetEmptyWebProxy. http://go.microsoft.com/fwlink/?linkid=14202")]
public class GlobalProxySelection
{
	/// <summary>Gets or sets the global HTTP proxy.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> that every call to <see cref="M:System.Net.HttpWebRequest.GetResponse" /> uses.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation was null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission for the requested operation. </exception>
	public static IWebProxy Select
	{
		get
		{
			IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
			if (defaultWebProxy == null)
			{
				return GetEmptyWebProxy();
			}
			if (defaultWebProxy is WebRequest.WebProxyWrapper webProxyWrapper)
			{
				return webProxyWrapper.WebProxy;
			}
			return defaultWebProxy;
		}
		set
		{
			WebRequest.DefaultWebProxy = value;
		}
	}

	/// <summary>Returns an empty proxy instance.</summary>
	/// <returns>An <see cref="T:System.Net.IWebProxy" /> that contains no information.</returns>
	public static IWebProxy GetEmptyWebProxy()
	{
		return new EmptyWebProxy();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.GlobalProxySelection" /> class.</summary>
	public GlobalProxySelection()
	{
	}
}

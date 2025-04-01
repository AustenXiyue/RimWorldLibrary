using System.Configuration;
using Unity;

namespace System.Net.Configuration;

/// <summary>Represents the HttpListener element in the configuration file. This class cannot be inherited.</summary>
public sealed class HttpListenerElement : ConfigurationElement
{
	/// <summary>Gets the default timeout elements used for an <see cref="T:System.Net.HttpListener" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Net.Configuration.HttpListenerTimeoutsElement" />.The timeout elements used for an <see cref="T:System.Net.HttpListener" /> object.</returns>
	public HttpListenerTimeoutsElement Timeouts
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets a value that indicates if <see cref="T:System.Net.HttpListener" /> uses the raw unescaped URI instead of the converted URI.</summary>
	/// <returns>A Boolean value that indicates if <see cref="T:System.Net.HttpListener" /> uses the raw unescaped URI, rather than the converted URI.</returns>
	public bool UnescapeRequestUrl
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.HttpListenerElement" /> class.</summary>
	public HttpListenerElement()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}

using System.Configuration;
using Unity;

namespace System.Net.Configuration;

/// <summary>Represents the <see cref="T:System.Net.HttpListener" /> timeouts element in the configuration file. This class cannot be inherited.</summary>
public sealed class HttpListenerTimeoutsElement : ConfigurationElement
{
	/// <summary>Gets the time, in seconds, allowed for the <see cref="T:System.Net.HttpListener" />  to drain the entity body on a Keep-Alive connection.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time, in seconds, allowed for the <see cref="T:System.Net.HttpListener" />  to drain the entity body on a Keep-Alive connection.</returns>
	public TimeSpan DrainEntityBody
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
	}

	/// <summary>Gets the time, in seconds, allowed for the request entity body to arrive.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time, in seconds, allowed for the request entity body to arrive.</returns>
	public TimeSpan EntityBody
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
	}

	/// <summary>Gets the time, in seconds, allowed for the <see cref="T:System.Net.HttpListener" /> to parse the request header.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time, in seconds, allowed for the <see cref="T:System.Net.HttpListener" /> to parse the request header.</returns>
	public TimeSpan HeaderWait
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
	}

	/// <summary>Gets the time, in seconds, allowed for an idle connection.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time, in seconds, allowed for an idle connection.</returns>
	public TimeSpan IdleConnection
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
	}

	/// <summary>Gets the minimum send rate, in bytes-per-second, for the response.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The minimum send rate, in bytes-per-second, for the response.</returns>
	public long MinSendBytesPerSecond
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	/// <summary>Gets the time, in seconds, allowed for the request to remain in the request queue before the <see cref="T:System.Net.HttpListener" /> picks it up.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The time, in seconds, allowed for the request to remain in the request queue before the <see cref="T:System.Net.HttpListener" /> picks it up.</returns>
	public TimeSpan RequestQueue
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TimeSpan);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.HttpListenerTimeoutsElement" /> class.</summary>
	public HttpListenerTimeoutsElement()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}

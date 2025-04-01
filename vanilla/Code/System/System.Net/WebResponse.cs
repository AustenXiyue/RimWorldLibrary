using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Provides a response from a Uniform Resource Identifier (URI). This is an abstract class.</summary>
[Serializable]
public abstract class WebResponse : MarshalByRefObject, ISerializable, IDisposable
{
	private bool m_IsCacheFresh;

	private bool m_IsFromCache;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether this response was obtained from the cache.</summary>
	/// <returns>true if the response was taken from the cache; otherwise, false.</returns>
	public virtual bool IsFromCache => m_IsFromCache;

	internal bool InternalSetFromCache
	{
		set
		{
			m_IsFromCache = value;
		}
	}

	internal virtual bool IsCacheFresh => m_IsCacheFresh;

	internal bool InternalSetIsCacheFresh
	{
		set
		{
			m_IsCacheFresh = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Boolean" /> value that indicates whether mutual authentication occurred.</summary>
	/// <returns>true if both client and server were authenticated; otherwise, false.</returns>
	public virtual bool IsMutuallyAuthenticated => false;

	/// <summary>When overridden in a descendant class, gets or sets the content length of data being received.</summary>
	/// <returns>The number of bytes returned from the Internet resource.</returns>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual long ContentLength
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a derived class, gets or sets the content type of the data being received.</summary>
	/// <returns>A string that contains the content type of the response.</returns>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual string ContentType
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
		set
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a derived class, gets the URI of the Internet resource that actually responded to the request.</summary>
	/// <returns>An instance of the <see cref="T:System.Uri" /> class that contains the URI of the Internet resource that actually responded to the request.</returns>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual Uri ResponseUri
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>When overridden in a derived class, gets a collection of header name-value pairs associated with this request.</summary>
	/// <returns>An instance of the <see cref="T:System.Net.WebHeaderCollection" /> class that contains header values associated with this response.</returns>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to get or set the property, when the property is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual WebHeaderCollection Headers
	{
		get
		{
			throw ExceptionHelper.PropertyNotImplementedException;
		}
	}

	/// <summary>Gets a value that indicates if headers are supported.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if headers are supported; otherwise, false.</returns>
	public virtual bool SupportsHeaders => false;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebResponse" /> class.</summary>
	protected WebResponse()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebResponse" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
	/// <param name="serializationInfo">An instance of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class that contains the information required to serialize the new <see cref="T:System.Net.WebRequest" /> instance. </param>
	/// <param name="streamingContext">An instance of the <see cref="T:System.Runtime.Serialization.StreamingContext" /> class that indicates the source of the serialized stream that is associated with the new <see cref="T:System.Net.WebRequest" /> instance. </param>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to access the constructor, when the constructor is not overridden in a descendant class. </exception>
	protected WebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance with the data that is needed to serialize <see cref="T:System.Net.WebResponse" />.  </summary>
	/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that will hold the serialized data for the <see cref="T:System.Net.WebResponse" />. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the destination of the serialized stream that is associated with the new <see cref="T:System.Net.WebResponse" />. </param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		GetObjectData(serializationInfo, streamingContext);
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data that is needed to serialize the target object.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
	/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization. </param>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	protected virtual void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}

	/// <summary>When overridden by a descendant class, closes the response stream.</summary>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual void Close()
	{
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.WebResponse" /> object.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Net.WebResponse" /> object, and optionally disposes of the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		try
		{
			Close();
		}
		catch
		{
		}
	}

	/// <summary>When overridden in a descendant class, returns the data stream from the Internet resource.</summary>
	/// <returns>An instance of the <see cref="T:System.IO.Stream" /> class for reading data from the Internet resource.</returns>
	/// <exception cref="T:System.NotSupportedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual Stream GetResponseStream()
	{
		throw ExceptionHelper.MethodNotImplementedException;
	}
}

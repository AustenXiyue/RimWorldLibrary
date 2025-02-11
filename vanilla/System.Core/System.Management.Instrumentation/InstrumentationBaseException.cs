using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity;

namespace System.Management.Instrumentation;

/// <summary>Represents the base provider-related exception.</summary>
[Serializable]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class InstrumentationBaseException : Exception
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Management.Instrumentation.InstrumentationBaseException" />. class. This is the default constructor.</summary>
	public InstrumentationBaseException()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.Instrumentation.InstrumentationBaseException" /> class with serialization information.</summary>
	/// <param name="info">The data that is required to serialize or deserialize an object.</param>
	/// <param name="context">Description of the source and destination of the specified serialized stream.</param>
	protected InstrumentationBaseException(SerializationInfo info, StreamingContext context)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Management.Instrumentation.InstrumentationBaseException" /> class with a message that describes the exception.</summary>
	/// <param name="message">Message that describes the exception.</param>
	public InstrumentationBaseException(string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new <see cref="T:System.Management.Instrumentation.InstrumentationBaseException" /> class with the specified string and exception.</summary>
	/// <param name="message">Message that describes the exception.</param>
	/// <param name="innerException">The Exception instance that caused the current exception.</param>
	public InstrumentationBaseException(string message, Exception innerException)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}

using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Security.Principal;

/// <summary>Represents an exception for a principal whose identity could not be mapped to a known identity.</summary>
[Serializable]
[ComVisible(false)]
public sealed class IdentityNotMappedException : SystemException
{
	private IdentityReferenceCollection _coll;

	/// <summary>Represents the collection of unmapped identities for an <see cref="T:System.Security.Principal.IdentityNotMappedException" /> exception.</summary>
	/// <returns>The collection of unmapped identities.</returns>
	public IdentityReferenceCollection UnmappedIdentities
	{
		get
		{
			if (_coll == null)
			{
				_coll = new IdentityReferenceCollection();
			}
			return _coll;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.IdentityNotMappedException" /> class.</summary>
	public IdentityNotMappedException()
		: base(Locale.GetText("Couldn't translate some identities."))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.IdentityNotMappedException" /> class by using the specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public IdentityNotMappedException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.IdentityNotMappedException" /> class by using the specified error message and inner exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="inner">The exception that is the cause of the current exception. If <paramref name="inner" /> is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public IdentityNotMappedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	/// <summary>Gets serialization information with the data needed to create an instance of this <see cref="T:System.Security.Principal.IdentityNotMappedException" /> object. </summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization." /><see cref="SerializationInfoobject" /> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="streamingContext">The <see cref="T:System.Runtime.SerializationInfo." /><see cref="StreamingContext" /> object that contains contextual information about the source or destination.</param>
	[MonoTODO("not implemented")]
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
	}
}

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when there is an attempt to dynamically access a field that does not exist.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class MissingFieldException : MissingMemberException, ISerializable
{
	/// <summary>Gets the text string showing the signature of the missing field, the class name, and the field name. This property is read-only.</summary>
	/// <returns>The error message string.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Message
	{
		[SecuritySafeCritical]
		get
		{
			if (ClassName == null)
			{
				return base.Message;
			}
			return Environment.GetResourceString("Field '{0}' not found.", ((Signature != null) ? (MissingMemberException.FormatSignature(Signature) + " ") : "") + ClassName + "." + MemberName);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingFieldException" /> class.</summary>
	public MissingFieldException()
		: base(Environment.GetResourceString("Attempted to access a non-existing field."))
	{
		SetErrorCode(-2146233071);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingFieldException" /> class with a specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that describes the error. </param>
	public MissingFieldException(string message)
		: base(message)
	{
		SetErrorCode(-2146233071);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingFieldException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public MissingFieldException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233071);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingFieldException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected MissingFieldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private MissingFieldException(string className, string fieldName, byte[] signature)
	{
		ClassName = className;
		MemberName = fieldName;
		Signature = signature;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingFieldException" /> class with the specified class name and field name.</summary>
	/// <param name="className">The name of the class in which access to a nonexistent field was attempted. </param>
	/// <param name="fieldName">The name of the field that cannot be accessed. </param>
	public MissingFieldException(string className, string fieldName)
	{
		ClassName = className;
		MemberName = fieldName;
	}
}

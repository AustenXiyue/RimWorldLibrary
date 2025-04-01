using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when there is an attempt to dynamically access a class member that does not exist.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class MissingMemberException : MemberAccessException, ISerializable
{
	/// <summary>Holds the class name of the missing member.</summary>
	protected string ClassName;

	/// <summary>Holds the name of the missing member.</summary>
	protected string MemberName;

	/// <summary>Holds the signature of the missing member.</summary>
	protected byte[] Signature;

	/// <summary>Gets the text string showing the class name, the member name, and the signature of the missing member.</summary>
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
			return Environment.GetResourceString("Member '{0}' not found.", ClassName + "." + MemberName + ((Signature != null) ? (" " + FormatSignature(Signature)) : ""));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingMemberException" /> class.</summary>
	public MissingMemberException()
		: base(Environment.GetResourceString("Attempted to access a missing member."))
	{
		SetErrorCode(-2146233070);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingMemberException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public MissingMemberException(string message)
		: base(message)
	{
		SetErrorCode(-2146233070);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingMemberException" /> class with a specified error message and a reference to the inner exception that is the root cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">An instance of <see cref="T:System.Exception" /> that is the cause of the current Exception. If <paramref name="inner" /> is not a null reference (Nothing in Visual Basic), then the current Exception is raised in a catch block handling <paramref name="inner" />. </param>
	public MissingMemberException(string message, Exception inner)
		: base(message, inner)
	{
		SetErrorCode(-2146233070);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingMemberException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected MissingMemberException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		ClassName = info.GetString("MMClassName");
		MemberName = info.GetString("MMMemberName");
		Signature = (byte[])info.GetValue("MMSignature", typeof(byte[]));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	internal static extern string FormatSignature(byte[] signature);

	private MissingMemberException(string className, string memberName, byte[] signature)
	{
		ClassName = className;
		MemberName = memberName;
		Signature = signature;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.MissingMemberException" /> class with the specified class name and member name.</summary>
	/// <param name="className">The name of the class in which access to a nonexistent member was attempted. </param>
	/// <param name="memberName">The name of the member that cannot be accessed. </param>
	public MissingMemberException(string className, string memberName)
	{
		ClassName = className;
		MemberName = memberName;
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the class name, the member name, the signature of the missing member, and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> object is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		info.AddValue("MMClassName", ClassName, typeof(string));
		info.AddValue("MMMemberName", MemberName, typeof(string));
		info.AddValue("MMSignature", Signature, typeof(byte[]));
	}
}

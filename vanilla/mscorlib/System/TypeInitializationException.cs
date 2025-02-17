using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown as a wrapper around the exception thrown by the class initializer. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class TypeInitializationException : SystemException
{
	private string _typeName;

	/// <summary>Gets the fully qualified name of the type that fails to initialize.</summary>
	/// <returns>The fully qualified name of the type that fails to initialize.</returns>
	/// <filterpriority>2</filterpriority>
	public string TypeName
	{
		get
		{
			if (_typeName == null)
			{
				return string.Empty;
			}
			return _typeName;
		}
	}

	private TypeInitializationException()
		: base(Environment.GetResourceString("Type constructor threw an exception."))
	{
		SetErrorCode(-2146233036);
	}

	private TypeInitializationException(string message)
		: base(message)
	{
		SetErrorCode(-2146233036);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.TypeInitializationException" /> class with the default error message, the specified type name, and a reference to the inner exception that is the root cause of this exception.</summary>
	/// <param name="fullTypeName">The fully qualified name of the type that fails to initialize. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public TypeInitializationException(string fullTypeName, Exception innerException)
		: base(Environment.GetResourceString("The type initializer for '{0}' threw an exception.", fullTypeName), innerException)
	{
		_typeName = fullTypeName;
		SetErrorCode(-2146233036);
	}

	internal TypeInitializationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_typeName = info.GetString("TypeName");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the type name and additional exception information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TypeName", TypeName, typeof(string));
	}
}

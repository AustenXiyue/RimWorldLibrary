using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when one of the arguments provided to a method is not valid.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class ArgumentException : SystemException, ISerializable
{
	private string m_paramName;

	/// <summary>Gets the error message and the parameter name, or only the error message if no parameter name is set.</summary>
	/// <returns>A text string describing the details of the exception. The value of this property takes one of two forms: Condition Value The <paramref name="paramName" /> is a null reference (Nothing in Visual Basic) or of zero length. The <paramref name="message" /> string passed to the constructor. The <paramref name="paramName" /> is not null reference (Nothing in Visual Basic) and it has a length greater than zero. The <paramref name="message" /> string appended with the name of the invalid parameter. </returns>
	/// <filterpriority>1</filterpriority>
	public override string Message
	{
		get
		{
			string message = base.Message;
			if (!string.IsNullOrEmpty(m_paramName))
			{
				string resourceString = Environment.GetResourceString("Parameter name: {0}", m_paramName);
				return message + Environment.NewLine + resourceString;
			}
			return message;
		}
	}

	/// <summary>Gets the name of the parameter that causes this exception.</summary>
	/// <returns>The parameter name.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual string ParamName => m_paramName;

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class.</summary>
	public ArgumentException()
		: base(Environment.GetResourceString("Value does not fall within the expected range."))
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public ArgumentException(string message)
		: base(message)
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public ArgumentException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class with a specified error message, the parameter name, and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="paramName">The name of the parameter that caused the current exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public ArgumentException(string message, string paramName, Exception innerException)
		: base(message, innerException)
	{
		m_paramName = paramName;
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class with a specified error message and the name of the parameter that causes this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="paramName">The name of the parameter that caused the current exception. </param>
	public ArgumentException(string message, string paramName)
		: base(message)
	{
		m_paramName = paramName;
		SetErrorCode(-2147024809);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ArgumentException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected ArgumentException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		m_paramName = info.GetString("ParamName");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the parameter name and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> object is a null reference (Nothing in Visual Basic). </exception>
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
		info.AddValue("ParamName", m_paramName, typeof(string));
	}
}

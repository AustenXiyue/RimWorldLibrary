using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when a floating-point value is positive infinity, negative infinity, or Not-a-Number (NaN).</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public class NotFiniteNumberException : ArithmeticException
{
	private double _offendingNumber;

	/// <summary>Gets the invalid number that is a positive infinity, a negative infinity, or Not-a-Number (NaN).</summary>
	/// <returns>The invalid number.</returns>
	/// <filterpriority>2</filterpriority>
	public double OffendingNumber => _offendingNumber;

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class.</summary>
	public NotFiniteNumberException()
		: base(Environment.GetResourceString("Number encountered was not a finite quantity."))
	{
		_offendingNumber = 0.0;
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with the invalid number.</summary>
	/// <param name="offendingNumber">The value of the argument that caused the exception. </param>
	public NotFiniteNumberException(double offendingNumber)
	{
		_offendingNumber = offendingNumber;
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public NotFiniteNumberException(string message)
		: base(message)
	{
		_offendingNumber = 0.0;
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with a specified error message and the invalid number.</summary>
	/// <param name="message">The message that describes the error. </param>
	/// <param name="offendingNumber">The value of the argument that caused the exception. </param>
	public NotFiniteNumberException(string message, double offendingNumber)
		: base(message)
	{
		_offendingNumber = offendingNumber;
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with a specified error message and a reference to the inner exception that is root cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public NotFiniteNumberException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with a specified error message, the invalid number, and a reference to the inner exception that is root cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="offendingNumber">The value of the argument that caused the exception. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception. </param>
	public NotFiniteNumberException(string message, double offendingNumber, Exception innerException)
		: base(message, innerException)
	{
		_offendingNumber = offendingNumber;
		SetErrorCode(-2146233048);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NotFiniteNumberException" /> class with serialized data.</summary>
	/// <param name="info">The object that holds the serialized object data. </param>
	/// <param name="context">The contextual information about the source or destination. </param>
	protected NotFiniteNumberException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_offendingNumber = info.GetInt32("OffendingNumber");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the invalid number and additional exception information.</summary>
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
		info.AddValue("OffendingNumber", _offendingNumber, typeof(int));
	}
}

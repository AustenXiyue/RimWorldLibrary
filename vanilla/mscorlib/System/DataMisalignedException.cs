using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System;

/// <summary>The exception that is thrown when a unit of data is read from or written to an address that is not a multiple of the data size. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class DataMisalignedException : SystemException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.DataMisalignedException" /> class. </summary>
	public DataMisalignedException()
		: base(Environment.GetResourceString("A datatype misalignment was detected in a load or store instruction."))
	{
		SetErrorCode(-2146233023);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DataMisalignedException" /> class using the specified error message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> object that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	public DataMisalignedException(string message)
		: base(message)
	{
		SetErrorCode(-2146233023);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.DataMisalignedException" /> class using the specified error message and underlying exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> object that describes the error. The content of <paramref name="message" /> is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="innerException">The exception that is the cause of the current <see cref="T:System.DataMisalignedException" />. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public DataMisalignedException(string message, Exception innerException)
		: base(message, innerException)
	{
		SetErrorCode(-2146233023);
	}

	internal DataMisalignedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

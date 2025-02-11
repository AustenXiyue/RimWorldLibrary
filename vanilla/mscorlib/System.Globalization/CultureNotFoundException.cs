using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Globalization;

/// <summary>The exception thrown when a method is invoked which attempts to construct a culture that is not available on the machine.</summary>
[Serializable]
[ComVisible(true)]
public class CultureNotFoundException : ArgumentException, ISerializable
{
	private string m_invalidCultureName;

	private int? m_invalidCultureId;

	/// <summary>Gets the Culture ID that cannot be found.</summary>
	/// <returns>The invalid Culture ID.</returns>
	public virtual int? InvalidCultureId => m_invalidCultureId;

	/// <summary>Gets the Culture Name that cannot be found.</summary>
	/// <returns>The invalid Culture Name.</returns>
	public virtual string InvalidCultureName => m_invalidCultureName;

	private static string DefaultMessage => Environment.GetResourceString("Culture is not supported.");

	private string FormatedInvalidCultureId
	{
		get
		{
			if (InvalidCultureId.HasValue)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} (0x{0:x4})", InvalidCultureId.Value);
			}
			return InvalidCultureName;
		}
	}

	/// <summary>Gets the error message that explains the reason for the exception.</summary>
	/// <returns>A text string describing the details of the exception.</returns>
	public override string Message
	{
		get
		{
			string message = base.Message;
			if (m_invalidCultureId.HasValue || m_invalidCultureName != null)
			{
				string resourceString = Environment.GetResourceString("{0} is an invalid culture identifier.", FormatedInvalidCultureId);
				if (message == null)
				{
					return resourceString;
				}
				return message + Environment.NewLine + resourceString;
			}
			return message;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with its message string set to a system-supplied message.</summary>
	public CultureNotFoundException()
		: base(DefaultMessage)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with the specified error message.</summary>
	/// <param name="message">The error message to display with this exception.</param>
	public CultureNotFoundException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message and the name of the parameter that is the cause this exception.</summary>
	/// <param name="paramName">The name of the parameter that is the cause of the current exception.</param>
	/// <param name="message">The error message to display with this exception.</param>
	public CultureNotFoundException(string paramName, string message)
		: base(message, paramName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message to display with this exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
	public CultureNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message, the invalid Culture ID, and the name of the parameter that is the cause this exception.</summary>
	/// <param name="paramName">The name of the parameter that is the cause the current exception.</param>
	/// <param name="invalidCultureId">The Culture ID that cannot be found.</param>
	/// <param name="message">The error message to display with this exception.</param>
	public CultureNotFoundException(string paramName, int invalidCultureId, string message)
		: base(message, paramName)
	{
		m_invalidCultureId = invalidCultureId;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message, the invalid Culture ID, and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message to display with this exception.</param>
	/// <param name="invalidCultureId">The Culture ID that cannot be found.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
	public CultureNotFoundException(string message, int invalidCultureId, Exception innerException)
		: base(message, innerException)
	{
		m_invalidCultureId = invalidCultureId;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message, the invalid Culture Name, and the name of the parameter that is the cause this exception.</summary>
	/// <param name="paramName">The name of the parameter that is the cause the current exception.</param>
	/// <param name="invalidCultureName">The Culture Name that cannot be found.</param>
	/// <param name="message">The error message to display with this exception.</param>
	public CultureNotFoundException(string paramName, string invalidCultureName, string message)
		: base(message, paramName)
	{
		m_invalidCultureName = invalidCultureName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class with a specified error message, the invalid Culture Name, and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message to display with this exception.</param>
	/// <param name="invalidCultureName">The Culture Name that cannot be found.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
	public CultureNotFoundException(string message, string invalidCultureName, Exception innerException)
		: base(message, innerException)
	{
		m_invalidCultureName = invalidCultureName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureNotFoundException" /> class using the specified serialization data and context.</summary>
	/// <param name="info">The object that holds the serialized object data.</param>
	/// <param name="context">The contextual information about the source or destination.</param>
	protected CultureNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		m_invalidCultureId = (int?)info.GetValue("InvalidCultureId", typeof(int?));
		m_invalidCultureName = (string)info.GetValue("InvalidCultureName", typeof(string));
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the parameter name and additional exception information.</summary>
	/// <param name="info">The object that holds the serialized object data.</param>
	/// <param name="context">The contextual information about the source or destination.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		base.GetObjectData(info, context);
		int? num = null;
		num = m_invalidCultureId;
		info.AddValue("InvalidCultureId", num, typeof(int?));
		info.AddValue("InvalidCultureName", m_invalidCultureName, typeof(string));
	}
}

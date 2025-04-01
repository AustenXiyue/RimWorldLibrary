using System.Runtime.Serialization;

namespace System.Configuration.Provider;

/// <summary>The exception that is thrown when a configuration provider error has occurred. This exception class is also used by providers to throw exceptions when internal errors occur within the provider that do not map to other pre-existing exception classes.</summary>
[Serializable]
public class ProviderException : Exception
{
	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.Provider.ProviderException" /> class.</summary>
	public ProviderException()
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.Provider.ProviderException" /> class.</summary>
	/// <param name="info">The object that holds the information to deserialize.</param>
	/// <param name="context">Contextual information about the source or destination.</param>
	protected ProviderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.Provider.ProviderException" /> class.</summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.Provider.ProviderException" /> was thrown.</param>
	public ProviderException(string message)
		: base(message)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.Provider.ProviderException" /> class.</summary>
	/// <param name="message">A message describing why this <see cref="T:System.Configuration.Provider.ProviderException" /> was thrown.</param>
	/// <param name="innerException">The exception that caused this <see cref="T:System.Configuration.Provider.ProviderException" /> to be thrown.</param>
	public ProviderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}

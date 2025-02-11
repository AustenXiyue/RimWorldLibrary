using System.Runtime.Serialization;

namespace System.Windows;

/// <summary>The exception that is thrown when a resource reference key cannot be found during parsing or serialization of markup extension resources.</summary>
[Serializable]
public class ResourceReferenceKeyNotFoundException : InvalidOperationException
{
	private object _resourceKey;

	/// <summary>Gets the key that was not found and caused the exception to be thrown.</summary>
	/// <returns>The resource key.</returns>
	public object Key => _resourceKey;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ResourceReferenceKeyNotFoundException" /> class.</summary>
	public ResourceReferenceKeyNotFoundException()
	{
		_resourceKey = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ResourceReferenceKeyNotFoundException" /> class with the specified error message and resource key.</summary>
	/// <param name="message">A possible descriptive message.</param>
	/// <param name="resourceKey">The key that was not found.</param>
	public ResourceReferenceKeyNotFoundException(string message, object resourceKey)
		: base(message)
	{
		_resourceKey = resourceKey;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ResourceReferenceKeyNotFoundException" /> class with the specified serialization information and streaming context.</summary>
	/// <param name="info">Specific information from the serialization process.</param>
	/// <param name="context">The context at the time the exception was thrown.</param>
	protected ResourceReferenceKeyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_resourceKey = info.GetValue("Key", typeof(object));
	}

	/// <summary>Reports specifics of the exception to debuggers or dialogs.</summary>
	/// <param name="info">Specific information from the serialization process.</param>
	/// <param name="context">The context at the time the exception was thrown.</param>
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Key", _resourceKey);
	}
}

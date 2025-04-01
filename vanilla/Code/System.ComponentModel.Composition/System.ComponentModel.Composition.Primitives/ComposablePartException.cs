using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Internal.Runtime.Serialization;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>The exception that is thrown when an error occurs when calling methods on a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object.</summary>
[Serializable]
[DebuggerDisplay("{Message}")]
[DebuggerTypeProxy(typeof(ComposablePartExceptionDebuggerProxy))]
public class ComposablePartException : Exception
{
	private readonly ICompositionElement _element;

	/// <summary>Gets the composition element that is the cause of the exception.</summary>
	/// <returns>The compositional element that is the cause of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />. The default is null.</returns>
	public ICompositionElement Element => _element;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class.</summary>
	public ComposablePartException()
		: this(null, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class with the specified error message.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.Message" /> property to its default value.</param>
	public ComposablePartException(string message)
		: this(message, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class with the specified error message and the composition element that is the cause of the exception.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.Message" /> property to its default value.</param>
	/// <param name="element">The composition element that is the cause of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePartException.Element" /> property to null.</param>
	public ComposablePartException(string message, ICompositionElement element)
		: this(message, element, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class with the specified error message and the exception that is the cause of this exception.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.Message" /> property to its default value.</param>
	/// <param name="innerException">The exception that is the underlying cause of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.InnerException" /> property to null.</param>
	public ComposablePartException(string message, Exception innerException)
		: this(message, null, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class with the specified error message, and the composition element and exception that are the cause of this exception.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.Message" /> property to its default value.</param>
	/// <param name="element">The composition element that is the cause of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePartException.Element" /> property to null.</param>
	/// <param name="innerException">The exception that is the underlying cause of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />, or null to set the <see cref="P:System.Exception.InnerException" /> property to null.</param>
	public ComposablePartException(string message, ICompositionElement element, Exception innerException)
		: base(message, innerException)
	{
		_element = element;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" /> class with the specified serialization data.</summary>
	/// <param name="info">An object that holds the serialized object data for the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />.</param>
	/// <param name="context">An object that contains contextual information about the source or destination.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">
	///   <paramref name="info" /> is missing a required value.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="info" /> contains a value that cannot be cast to the correct type.</exception>
	[SecuritySafeCritical]
	protected ComposablePartException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_element = info.GetValue<ICompositionElement>("Element");
	}

	/// <summary>Gets the serialization data for the exception.</summary>
	/// <param name="info">After calling the method, contains serialized object data about the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException" />.</param>
	/// <param name="context">After calling the method, contains contextual information about the source or destination.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Element", _element.ToSerializableElement());
	}
}

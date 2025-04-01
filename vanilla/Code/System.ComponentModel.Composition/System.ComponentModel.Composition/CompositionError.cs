using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;

namespace System.ComponentModel.Composition;

/// <summary>Represents an error that occurred during composition.</summary>
[Serializable]
[DebuggerTypeProxy(typeof(CompositionErrorDebuggerProxy))]
public class CompositionError
{
	private readonly CompositionErrorId _id;

	private readonly string _description;

	private readonly Exception _exception;

	private readonly ICompositionElement _element;

	/// <summary>Gets the composition element that is the cause of the error.</summary>
	/// <returns>The composition element that is the cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" />. The default is null.</returns>
	public ICompositionElement Element => _element;

	/// <summary>Gets a description of the composition error.</summary>
	/// <returns>A message that describes the <see cref="T:System.ComponentModel.Composition.CompositionError" />.</returns>
	public string Description => _description;

	/// <summary>Gets the exception that is the underlying cause of the composition error.</summary>
	/// <returns>The exception that is the underlying cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" />. The default is null.</returns>
	public Exception Exception => _exception;

	internal CompositionErrorId Id => _id;

	internal Exception InnerException => Exception;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> class with the specified error message.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Description" /> property to an empty string ("").</param>
	public CompositionError(string message)
		: this(CompositionErrorId.Unknown, message, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> class with the specified error message and the composition element that is the cause of the composition error.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Description" /> property to an empty string ("").</param>
	/// <param name="element">The composition element that is the cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Element" /> property to null.</param>
	public CompositionError(string message, ICompositionElement element)
		: this(CompositionErrorId.Unknown, message, element, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> class with the specified error message and the exception that is the cause of the composition error.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Description" /> property to an empty string ("").</param>
	/// <param name="exception">The <see cref="P:System.ComponentModel.Composition.CompositionError.Exception" /> that is the underlying cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Exception" /> property to null.</param>
	public CompositionError(string message, Exception exception)
		: this(CompositionErrorId.Unknown, message, null, exception)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> class with the specified error message, and the composition element and exception that are the cause of the composition error.</summary>
	/// <param name="message">A message that describes the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Description" /> property to an empty string ("").</param>
	/// <param name="element">The composition element that is the cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Element" /> property to null.</param>
	/// <param name="exception">The <see cref="P:System.ComponentModel.Composition.CompositionError.Exception" /> that is the underlying cause of the <see cref="T:System.ComponentModel.Composition.CompositionError" /> or null to set the <see cref="P:System.ComponentModel.Composition.CompositionError.Exception" /> property to null.</param>
	public CompositionError(string message, ICompositionElement element, Exception exception)
		: this(CompositionErrorId.Unknown, message, element, exception)
	{
	}

	internal CompositionError(CompositionErrorId id, string description, ICompositionElement element, Exception exception)
	{
		_id = id;
		_description = description ?? string.Empty;
		_element = element;
		_exception = exception;
	}

	/// <summary>Returns a string representation of the composition error.</summary>
	/// <returns>A string that contains the <see cref="P:System.ComponentModel.Composition.CompositionError.Description" /> property.</returns>
	public override string ToString()
	{
		return Description;
	}

	internal static CompositionError Create(CompositionErrorId id, string format, params object[] parameters)
	{
		return Create(id, null, null, format, parameters);
	}

	internal static CompositionError Create(CompositionErrorId id, ICompositionElement element, string format, params object[] parameters)
	{
		return Create(id, element, null, format, parameters);
	}

	internal static CompositionError Create(CompositionErrorId id, ICompositionElement element, Exception exception, string format, params object[] parameters)
	{
		return new CompositionError(id, string.Format(CultureInfo.CurrentCulture, format, parameters), element, exception);
	}
}

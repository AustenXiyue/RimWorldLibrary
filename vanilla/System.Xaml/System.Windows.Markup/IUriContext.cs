using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Represents a service that can use application context to resolve a provided relative URI to an absolute URI.</summary>
[TypeForwardedFrom("PresentationCore, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IUriContext
{
	/// <summary>Gets or sets the base URI of the current application context. </summary>
	/// <returns>The base URI of the application context.</returns>
	Uri BaseUri { get; set; }
}

using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Represents a service that reports situational object-property relationships for markup extension evaluation.</summary>
[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public interface IProvideValueTarget
{
	/// <summary>Gets the target object being reported.</summary>
	/// <returns>The target object being reported.</returns>
	object TargetObject { get; }

	/// <summary>Gets an identifier for the target property being reported.</summary>
	/// <returns>An identifier for the target property being reported.</returns>
	object TargetProperty { get; }
}

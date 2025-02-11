using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies that a XAML namespace can be subsumed by another XAML namespace. Typically, the subsuming XAML namespace is indicated in a previously defined <see cref="T:System.Windows.Markup.XmlnsDefinitionAttribute" />.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsCompatibleWithAttribute : Attribute
{
	/// <summary>Gets the reference namespace identifier reported by this attribute.</summary>
	/// <returns>The reference namespace identifier.</returns>
	public string OldNamespace { get; }

	/// <summary>Gets the subsuming namespace identifier reported by this attribute.</summary>
	/// <returns>The subsuming namespace identifier reported in the attribute.</returns>
	public string NewNamespace { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlnsCompatibleWithAttribute" /> class.</summary>
	/// <param name="oldNamespace">The reference XAML namespace identifier.</param>
	/// <param name="newNamespace">The subsuming XAML namespace identifier.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oldNamespace" /> or <paramref name="newNamespace" /> are null.</exception>
	public XmlnsCompatibleWithAttribute(string oldNamespace, string newNamespace)
	{
		OldNamespace = oldNamespace ?? throw new ArgumentNullException("oldNamespace");
		NewNamespace = newNamespace ?? throw new ArgumentNullException("newNamespace");
	}
}

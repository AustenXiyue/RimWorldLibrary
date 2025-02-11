using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Represents an assembly level attribute that is used to identify the value of the RootNamespace property in a Visual Studio project file.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class RootNamespaceAttribute : Attribute
{
	/// <summary>Gets the string that corresponds to the value of the RootNamespace property in a Visual Studio project file.</summary>
	/// <returns>The string that corresponds to the value of the RootNamespace property in a Visual Studio project file.</returns>
	public string Namespace { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.RootNamespaceAttribute" /> class.</summary>
	/// <param name="nameSpace">The root namespace value.</param>
	public RootNamespaceAttribute(string nameSpace)
	{
		Namespace = nameSpace;
	}
}

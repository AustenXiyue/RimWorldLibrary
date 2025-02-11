using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Specifies one or more types on the associated collection type that will be used to wrap foreign content.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class ContentWrapperAttribute : Attribute
{
	/// <summary>Gets the type that is declared as a content wrapper for the collection type associated with this attribute.</summary>
	/// <returns>The type that is declared as a content wrapper for the collection type.</returns>
	public Type ContentWrapper { get; }

	/// <summary>Gets a unique identifier for this attribute. </summary>
	/// <returns>A unique identifier for the attribute.</returns>
	public override object TypeId => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ContentWrapperAttribute" /> class. </summary>
	/// <param name="contentWrapper">The <see cref="T:System.Type" /> that is declared as a content wrapper for the collection type.</param>
	public ContentWrapperAttribute(Type contentWrapper)
	{
		ContentWrapper = contentWrapper;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Markup.ContentWrapperAttribute" /> is equivalent this <see cref="T:System.Windows.Markup.ContentWrapperAttribute" /> by comparing the <see cref="P:System.Windows.Markup.ContentWrapperAttribute.ContentWrapper" /> properties.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Markup.ContentWrapperAttribute.ContentWrapper" /> properties are equal; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Windows.Markup.ContentWrapperAttribute" /> to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj is ContentWrapperAttribute contentWrapperAttribute)
		{
			return ContentWrapper == contentWrapperAttribute.ContentWrapper;
		}
		return false;
	}

	/// <summary>Gets a hash code for this instance.</summary>
	/// <returns>An integer hash code</returns>
	public override int GetHashCode()
	{
		return ContentWrapper?.GetHashCode() ?? 0;
	}
}

namespace System.Windows.Markup;

/// <summary>Reports the type that a markup extension can return.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MarkupExtensionReturnTypeAttribute : Attribute
{
	/// <summary>Gets the <see cref="T:System.Windows.Markup.MarkupExtension" /> return type that this .NET Framework attribute reports.</summary>
	/// <returns>The type-safe return type of the specific <see cref="M:System.Windows.Markup.MarkupExtension.ProvideValue(System.IServiceProvider)" /> implementation of the markup extension where the <see cref="T:System.Windows.Markup.MarkupExtensionReturnTypeAttribute" />  .NET Framework attribute is applied.</returns>
	public Type ReturnType { get; }

	/// <summary>Do not use; see Remarks.</summary>
	/// <returns>Do not use; see Remarks.</returns>
	[Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
	public Type ExpressionType { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.MarkupExtensionReturnTypeAttribute" />  class using the provided <see cref="T:System.Type" />.</summary>
	/// <param name="returnType">The return type that this  .NET Framework attribute reports.</param>
	public MarkupExtensionReturnTypeAttribute(Type returnType)
	{
		ReturnType = returnType;
	}

	/// <summary>Do not use, see Remarks.</summary>
	/// <param name="returnType">The return type that this  .NET Framework attribute reports.</param>
	/// <param name="expressionType">Do not use; see Remarks.</param>
	[Obsolete("The expressionType argument is not used by the XAML parser. To specify the expected return type, use MarkupExtensionReturnTypeAttribute(Type). To specify custom handling for expression types, use XamlSetMarkupExtensionAttribute.")]
	public MarkupExtensionReturnTypeAttribute(Type returnType, Type expressionType)
	{
		ReturnType = returnType;
		ExpressionType = expressionType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.MarkupExtensionReturnTypeAttribute" /> class.</summary>
	public MarkupExtensionReturnTypeAttribute()
	{
	}
}

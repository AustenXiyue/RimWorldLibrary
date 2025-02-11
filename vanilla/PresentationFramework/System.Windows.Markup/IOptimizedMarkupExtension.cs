namespace System.Windows.Markup;

internal interface IOptimizedMarkupExtension
{
	short ExtensionTypeId { get; }

	short ValueId { get; }

	bool IsValueTypeExtension { get; }

	bool IsValueStaticExtension { get; }
}

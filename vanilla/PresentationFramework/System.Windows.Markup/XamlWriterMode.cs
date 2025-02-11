namespace System.Windows.Markup;

/// <summary>Specifies the XAML writer mode for serializing values that are expressions (such as binding declarations).</summary>
public enum XamlWriterMode
{
	/// <summary>The <see cref="T:System.Windows.Expression" /> is serialized.</summary>
	Expression,
	/// <summary>The evaluated value of the <see cref="T:System.Windows.Expression" /> is serialized.</summary>
	Value
}

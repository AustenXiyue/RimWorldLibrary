namespace System.Windows.Markup.Primitives;

internal class ElementConstructorArgument : ElementPseudoPropertyBase
{
	public override string Name => "Argument";

	public override bool IsConstructorArgument => true;

	internal ElementConstructorArgument(object value, Type type, ElementMarkupObject obj)
		: base(value, type, obj)
	{
	}
}

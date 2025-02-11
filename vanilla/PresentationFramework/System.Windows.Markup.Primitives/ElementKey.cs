namespace System.Windows.Markup.Primitives;

internal class ElementKey : ElementPseudoPropertyBase
{
	public override string Name => "Key";

	public override bool IsKey => true;

	internal ElementKey(object value, Type type, ElementMarkupObject obj)
		: base(value, type, obj)
	{
	}
}

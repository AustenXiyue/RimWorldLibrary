using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Markup.Primitives;

internal class ElementItemsPseudoProperty : ElementPseudoPropertyBase
{
	private IEnumerable _value;

	public override string Name => "Items";

	public override bool IsContent => true;

	public override bool IsComposite => true;

	public override IEnumerable<MarkupObject> Items
	{
		get
		{
			foreach (object item in _value)
			{
				yield return new ElementMarkupObject(item, base.Manager);
			}
		}
	}

	internal ElementItemsPseudoProperty(IEnumerable value, Type type, ElementMarkupObject obj)
		: base(value, type, obj)
	{
		_value = value;
	}
}

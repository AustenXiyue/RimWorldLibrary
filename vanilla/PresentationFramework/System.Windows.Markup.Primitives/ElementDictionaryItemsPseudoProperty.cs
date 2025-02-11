using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Markup.Primitives;

internal class ElementDictionaryItemsPseudoProperty : ElementPseudoPropertyBase
{
	private IDictionary _value;

	public override string Name => "Entries";

	public override bool IsContent => true;

	public override bool IsComposite => true;

	public override IEnumerable<MarkupObject> Items
	{
		get
		{
			foreach (DictionaryEntry item in _value)
			{
				ElementMarkupObject elementMarkupObject = new ElementMarkupObject(item.Value, base.Manager);
				elementMarkupObject.SetKey(new ElementKey(item.Key, typeof(object), elementMarkupObject));
				yield return elementMarkupObject;
			}
		}
	}

	internal ElementDictionaryItemsPseudoProperty(IDictionary value, Type type, ElementMarkupObject obj)
		: base(value, type, obj)
	{
		_value = value;
	}
}

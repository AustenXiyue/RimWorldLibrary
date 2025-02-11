using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class FrameworkElementFactoryContent : ElementPropertyBase
{
	private FrameworkElementFactoryMarkupObject _item;

	private FrameworkElementFactory _factory;

	public override string Name => "Content";

	public override bool IsContent => true;

	public override bool IsComposite => true;

	public override IEnumerable<MarkupObject> Items
	{
		get
		{
			for (FrameworkElementFactory child = _factory.FirstChild; child != null; child = child.NextSibling)
			{
				yield return new FrameworkElementFactoryMarkupObject(child, base.Manager);
			}
		}
	}

	public override AttributeCollection Attributes => new AttributeCollection();

	public override Type PropertyType => typeof(IEnumerable);

	public override object Value => _factory;

	internal FrameworkElementFactoryContent(FrameworkElementFactory factory, FrameworkElementFactoryMarkupObject item)
		: base(item.Manager)
	{
		_item = item;
		_factory = factory;
	}

	protected override IValueSerializerContext GetItemContext()
	{
		return _item.Context;
	}

	protected override Type GetObjectType()
	{
		return _item.ObjectType;
	}
}

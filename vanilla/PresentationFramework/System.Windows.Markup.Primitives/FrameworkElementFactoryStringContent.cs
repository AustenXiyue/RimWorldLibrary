using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class FrameworkElementFactoryStringContent : ElementPropertyBase
{
	private FrameworkElementFactoryMarkupObject _item;

	private FrameworkElementFactory _factory;

	public override string Name => "Content";

	public override bool IsContent => true;

	public override bool IsComposite => false;

	public override bool IsValueAsString => true;

	public override IEnumerable<MarkupObject> Items => Array.Empty<MarkupObject>();

	public override AttributeCollection Attributes => new AttributeCollection();

	public override Type PropertyType => typeof(string);

	public override object Value => _factory.Text;

	internal FrameworkElementFactoryStringContent(FrameworkElementFactory factory, FrameworkElementFactoryMarkupObject item)
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

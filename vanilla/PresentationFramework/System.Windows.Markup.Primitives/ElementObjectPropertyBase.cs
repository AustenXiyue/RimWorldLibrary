namespace System.Windows.Markup.Primitives;

internal abstract class ElementObjectPropertyBase : ElementPropertyBase
{
	protected readonly ElementMarkupObject _object;

	protected ElementObjectPropertyBase(ElementMarkupObject obj)
		: base(obj.Manager)
	{
		_object = obj;
	}

	protected override IValueSerializerContext GetItemContext()
	{
		return _object.Context;
	}

	protected override Type GetObjectType()
	{
		return _object.ObjectType;
	}
}

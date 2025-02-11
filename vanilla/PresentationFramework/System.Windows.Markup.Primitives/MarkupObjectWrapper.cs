using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class MarkupObjectWrapper : MarkupObject
{
	private MarkupObject _baseObject;

	public override AttributeCollection Attributes => _baseObject.Attributes;

	public override Type ObjectType => _baseObject.ObjectType;

	public override object Instance => _baseObject.Instance;

	public MarkupObjectWrapper(MarkupObject baseObject)
	{
		_baseObject = baseObject;
	}

	public override void AssignRootContext(IValueSerializerContext context)
	{
		_baseObject.AssignRootContext(context);
	}

	internal override IEnumerable<MarkupProperty> GetProperties(bool mapToConstructorArgs)
	{
		return _baseObject.GetProperties(mapToConstructorArgs);
	}
}

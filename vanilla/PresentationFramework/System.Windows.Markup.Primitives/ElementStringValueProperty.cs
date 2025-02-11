using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class ElementStringValueProperty : MarkupProperty
{
	private ElementMarkupObject _object;

	public override string Name => "StringValue";

	public override Type PropertyType => typeof(string);

	public override bool IsValueAsString => true;

	public override object Value => StringValue;

	public override string StringValue => ValueSerializer.GetSerializerFor(_object.ObjectType, _object.Context).ConvertToString(_object.Instance, _object.Context);

	public override IEnumerable<MarkupObject> Items => null;

	public override AttributeCollection Attributes => AttributeCollection.Empty;

	public override IEnumerable<Type> TypeReferences => ValueSerializer.GetSerializerFor(_object.ObjectType, _object.Context).TypeReferences(_object.Instance, _object.Context);

	internal ElementStringValueProperty(ElementMarkupObject obj)
	{
		_object = obj;
	}
}

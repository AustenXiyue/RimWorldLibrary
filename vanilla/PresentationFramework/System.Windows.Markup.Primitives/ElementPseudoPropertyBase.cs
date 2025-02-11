using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal abstract class ElementPseudoPropertyBase : ElementObjectPropertyBase
{
	private object _value;

	private Type _type;

	public override Type PropertyType => _type;

	public override object Value => ElementProperty.CheckForMarkupExtension(PropertyType, _value, base.Context, convertEnums: true);

	public override AttributeCollection Attributes => AttributeCollection.Empty;

	public override IEnumerable<Type> TypeReferences => Array.Empty<Type>();

	internal ElementPseudoPropertyBase(object value, Type type, ElementMarkupObject obj)
		: base(obj)
	{
		_value = value;
		_type = type;
	}
}

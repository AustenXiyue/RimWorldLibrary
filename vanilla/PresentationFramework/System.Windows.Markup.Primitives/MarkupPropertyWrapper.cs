using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Markup.Primitives;

internal class MarkupPropertyWrapper : MarkupProperty
{
	private MarkupProperty _baseProperty;

	public override AttributeCollection Attributes => _baseProperty.Attributes;

	public override IEnumerable<MarkupObject> Items => _baseProperty.Items;

	public override string Name => _baseProperty.Name;

	public override Type PropertyType => _baseProperty.PropertyType;

	public override string StringValue => _baseProperty.StringValue;

	public override IEnumerable<Type> TypeReferences => _baseProperty.TypeReferences;

	public override object Value => _baseProperty.Value;

	public override DependencyProperty DependencyProperty => _baseProperty.DependencyProperty;

	public override bool IsAttached => _baseProperty.IsAttached;

	public override bool IsComposite => _baseProperty.IsComposite;

	public override bool IsConstructorArgument => _baseProperty.IsConstructorArgument;

	public override bool IsKey => _baseProperty.IsKey;

	public override bool IsValueAsString => _baseProperty.IsValueAsString;

	public override bool IsContent => _baseProperty.IsContent;

	public override PropertyDescriptor PropertyDescriptor => _baseProperty.PropertyDescriptor;

	public MarkupPropertyWrapper(MarkupProperty baseProperty)
	{
		_baseProperty = baseProperty;
	}

	internal override void VerifyOnlySerializableTypes()
	{
		_baseProperty.VerifyOnlySerializableTypes();
	}
}

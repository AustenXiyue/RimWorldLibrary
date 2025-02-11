using System.Collections.Generic;
using System.ComponentModel;
using MS.Utility;

namespace System.Windows.Markup.Primitives;

internal class FrameworkElementFactoryMarkupObject : MarkupObject
{
	private FrameworkElementFactory _factory;

	private IValueSerializerContext _context;

	private XamlDesignerSerializationManager _manager;

	public override AttributeCollection Attributes => TypeDescriptor.GetAttributes(ObjectType);

	public override Type ObjectType
	{
		get
		{
			if (_factory.Type != null)
			{
				return _factory.Type;
			}
			return typeof(string);
		}
	}

	public override object Instance => _factory;

	internal IValueSerializerContext Context => _context;

	internal XamlDesignerSerializationManager Manager => _manager;

	internal FrameworkElementFactoryMarkupObject(FrameworkElementFactory factory, XamlDesignerSerializationManager manager)
	{
		_factory = factory;
		_manager = manager;
	}

	public override void AssignRootContext(IValueSerializerContext context)
	{
		_context = context;
	}

	internal override IEnumerable<MarkupProperty> GetProperties(bool mapToConstructorArgs)
	{
		if (_factory.Type == null)
		{
			if (_factory.Text != null)
			{
				yield return new FrameworkElementFactoryStringContent(_factory, this);
			}
			yield break;
		}
		FrugalStructList<PropertyValue> propertyValues = _factory.PropertyValues;
		for (int i = 0; i < propertyValues.Count; i++)
		{
			if (propertyValues[i].Property != XmlAttributeProperties.XmlnsDictionaryProperty)
			{
				yield return new FrameworkElementFactoryProperty(propertyValues[i], this);
			}
		}
		ElementMarkupObject elementMarkupObject = new ElementMarkupObject(_factory, Manager);
		foreach (MarkupProperty property in elementMarkupObject.Properties)
		{
			if (property.Name == "Triggers" && property.Name == "Storyboard")
			{
				yield return property;
			}
		}
		if (_factory.FirstChild != null)
		{
			if (_factory.FirstChild.Type == null)
			{
				yield return new FrameworkElementFactoryStringContent(_factory.FirstChild, this);
			}
			else
			{
				yield return new FrameworkElementFactoryContent(_factory, this);
			}
		}
	}
}

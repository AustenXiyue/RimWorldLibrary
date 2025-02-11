using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlPropertyNode : BamlStartComplexPropertyNode
{
	private string _value;

	private BamlAttributeUsage _attributeUsage;

	private int _index;

	internal string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
		}
	}

	internal BamlPropertyNode(string assemblyName, string ownerTypeFullName, string propertyName, string value, BamlAttributeUsage usage)
		: base(assemblyName, ownerTypeFullName, propertyName)
	{
		_value = value;
		_attributeUsage = usage;
		_nodeType = BamlNodeType.Property;
	}

	internal override void Serialize(BamlWriter writer)
	{
		if (!LocComments.IsLocCommentsProperty(_ownerTypeFullName, _propertyName) && !LocComments.IsLocLocalizabilityProperty(_ownerTypeFullName, _propertyName))
		{
			writer.WriteProperty(_assemblyName, _ownerTypeFullName, _propertyName, _value, _attributeUsage);
		}
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlPropertyNode(_assemblyName, _ownerTypeFullName, _propertyName, _value, _attributeUsage);
	}
}

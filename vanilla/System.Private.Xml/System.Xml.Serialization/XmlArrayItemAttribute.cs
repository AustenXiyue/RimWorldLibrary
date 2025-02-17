using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;

namespace System.Xml.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true)]
public class XmlArrayItemAttribute : Attribute
{
	private string _elementName;

	private Type _type;

	private string _ns;

	private string _dataType;

	private bool _nullable;

	private bool _nullableSpecified;

	private XmlSchemaForm _form;

	private int _nestingLevel;

	public Type? Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public string ElementName
	{
		get
		{
			return _elementName ?? string.Empty;
		}
		[param: AllowNull]
		set
		{
			_elementName = value;
		}
	}

	public string? Namespace
	{
		get
		{
			return _ns;
		}
		set
		{
			_ns = value;
		}
	}

	public int NestingLevel
	{
		get
		{
			return _nestingLevel;
		}
		set
		{
			_nestingLevel = value;
		}
	}

	public string DataType
	{
		get
		{
			return _dataType ?? string.Empty;
		}
		[param: AllowNull]
		set
		{
			_dataType = value;
		}
	}

	public bool IsNullable
	{
		get
		{
			return _nullable;
		}
		set
		{
			_nullable = value;
			_nullableSpecified = true;
		}
	}

	internal bool IsNullableSpecified => _nullableSpecified;

	public XmlSchemaForm Form
	{
		get
		{
			return _form;
		}
		set
		{
			_form = value;
		}
	}

	public XmlArrayItemAttribute()
	{
	}

	public XmlArrayItemAttribute(string? elementName)
	{
		_elementName = elementName;
	}

	public XmlArrayItemAttribute(Type? type)
	{
		_type = type;
	}

	public XmlArrayItemAttribute(string? elementName, Type? type)
	{
		_elementName = elementName;
		_type = type;
	}

	internal bool GetIsNullableSpecified()
	{
		return IsNullableSpecified;
	}
}

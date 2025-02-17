using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.XPath;

namespace System.Xml.Schema;

public sealed class XmlAtomicValue : XPathItem, ICloneable
{
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	private struct Union
	{
		[FieldOffset(0)]
		public bool boolVal;

		[FieldOffset(0)]
		public double dblVal;

		[FieldOffset(0)]
		public long i64Val;

		[FieldOffset(0)]
		public int i32Val;

		[FieldOffset(0)]
		public DateTime dtVal;
	}

	private sealed class NamespacePrefixForQName : IXmlNamespaceResolver
	{
		public string prefix;

		public string ns;

		public NamespacePrefixForQName(string prefix, string ns)
		{
			this.ns = ns;
			this.prefix = prefix;
		}

		public string LookupNamespace(string prefix)
		{
			if (prefix == this.prefix)
			{
				return ns;
			}
			return null;
		}

		public string LookupPrefix(string namespaceName)
		{
			if (ns == namespaceName)
			{
				return prefix;
			}
			return null;
		}

		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(1);
			dictionary[prefix] = ns;
			return dictionary;
		}
	}

	private readonly XmlSchemaType _xmlType;

	private readonly object _objVal;

	private readonly TypeCode _clrType;

	private Union _unionVal;

	private readonly NamespacePrefixForQName _nsPrefix;

	public override bool IsNode => false;

	public override XmlSchemaType XmlType => _xmlType;

	public override Type ValueType => _xmlType.Datatype.ValueType;

	public override object TypedValue
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ChangeType(_unionVal.boolVal, ValueType);
				case TypeCode.Int32:
					return valueConverter.ChangeType(_unionVal.i32Val, ValueType);
				case TypeCode.Int64:
					return valueConverter.ChangeType(_unionVal.i64Val, ValueType);
				case TypeCode.Double:
					return valueConverter.ChangeType(_unionVal.dblVal, ValueType);
				case TypeCode.DateTime:
					return valueConverter.ChangeType(_unionVal.dtVal, ValueType);
				}
			}
			return valueConverter.ChangeType(_objVal, ValueType, _nsPrefix);
		}
	}

	public override bool ValueAsBoolean
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return _unionVal.boolVal;
				case TypeCode.Int32:
					return valueConverter.ToBoolean(_unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToBoolean(_unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToBoolean(_unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToBoolean(_unionVal.dtVal);
				}
			}
			return valueConverter.ToBoolean(_objVal);
		}
	}

	public override DateTime ValueAsDateTime
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToDateTime(_unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToDateTime(_unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToDateTime(_unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToDateTime(_unionVal.dblVal);
				case TypeCode.DateTime:
					return _unionVal.dtVal;
				}
			}
			return valueConverter.ToDateTime(_objVal);
		}
	}

	public override double ValueAsDouble
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToDouble(_unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToDouble(_unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToDouble(_unionVal.i64Val);
				case TypeCode.Double:
					return _unionVal.dblVal;
				case TypeCode.DateTime:
					return valueConverter.ToDouble(_unionVal.dtVal);
				}
			}
			return valueConverter.ToDouble(_objVal);
		}
	}

	public override int ValueAsInt
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToInt32(_unionVal.boolVal);
				case TypeCode.Int32:
					return _unionVal.i32Val;
				case TypeCode.Int64:
					return valueConverter.ToInt32(_unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToInt32(_unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToInt32(_unionVal.dtVal);
				}
			}
			return valueConverter.ToInt32(_objVal);
		}
	}

	public override long ValueAsLong
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToInt64(_unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToInt64(_unionVal.i32Val);
				case TypeCode.Int64:
					return _unionVal.i64Val;
				case TypeCode.Double:
					return valueConverter.ToInt64(_unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToInt64(_unionVal.dtVal);
				}
			}
			return valueConverter.ToInt64(_objVal);
		}
	}

	public override string Value
	{
		get
		{
			XmlValueConverter valueConverter = _xmlType.ValueConverter;
			if (_objVal == null)
			{
				switch (_clrType)
				{
				case TypeCode.Boolean:
					return valueConverter.ToString(_unionVal.boolVal);
				case TypeCode.Int32:
					return valueConverter.ToString(_unionVal.i32Val);
				case TypeCode.Int64:
					return valueConverter.ToString(_unionVal.i64Val);
				case TypeCode.Double:
					return valueConverter.ToString(_unionVal.dblVal);
				case TypeCode.DateTime:
					return valueConverter.ToString(_unionVal.dtVal);
				}
			}
			return valueConverter.ToString(_objVal, _nsPrefix);
		}
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, bool value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		_xmlType = xmlType;
		_clrType = TypeCode.Boolean;
		_unionVal.boolVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, DateTime value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		_xmlType = xmlType;
		_clrType = TypeCode.DateTime;
		_unionVal.dtVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, double value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		_xmlType = xmlType;
		_clrType = TypeCode.Double;
		_unionVal.dblVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, int value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		_xmlType = xmlType;
		_clrType = TypeCode.Int32;
		_unionVal.i32Val = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, long value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		_xmlType = xmlType;
		_clrType = TypeCode.Int64;
		_unionVal.i64Val = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, string value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		ArgumentNullException.ThrowIfNull(value, "value");
		_xmlType = xmlType;
		_objVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, string value, IXmlNamespaceResolver nsResolver)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		ArgumentNullException.ThrowIfNull(value, "value");
		_xmlType = xmlType;
		_objVal = value;
		if (nsResolver != null && (_xmlType.TypeCode == XmlTypeCode.QName || _xmlType.TypeCode == XmlTypeCode.Notation))
		{
			string prefixFromQName = GetPrefixFromQName(value);
			_nsPrefix = new NamespacePrefixForQName(prefixFromQName, nsResolver.LookupNamespace(prefixFromQName));
		}
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, object value)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		ArgumentNullException.ThrowIfNull(value, "value");
		_xmlType = xmlType;
		_objVal = value;
	}

	internal XmlAtomicValue(XmlSchemaType xmlType, object value, IXmlNamespaceResolver nsResolver)
	{
		ArgumentNullException.ThrowIfNull(xmlType, "xmlType");
		ArgumentNullException.ThrowIfNull(value, "value");
		_xmlType = xmlType;
		_objVal = value;
		if (nsResolver != null && (_xmlType.TypeCode == XmlTypeCode.QName || _xmlType.TypeCode == XmlTypeCode.Notation))
		{
			XmlQualifiedName xmlQualifiedName = _objVal as XmlQualifiedName;
			string @namespace = xmlQualifiedName.Namespace;
			_nsPrefix = new NamespacePrefixForQName(nsResolver.LookupPrefix(@namespace), @namespace);
		}
	}

	public XmlAtomicValue Clone()
	{
		return this;
	}

	object ICloneable.Clone()
	{
		return this;
	}

	public override object ValueAs(Type type, IXmlNamespaceResolver? nsResolver)
	{
		XmlValueConverter valueConverter = _xmlType.ValueConverter;
		if (type == typeof(XPathItem) || type == typeof(XmlAtomicValue))
		{
			return this;
		}
		if (_objVal == null)
		{
			switch (_clrType)
			{
			case TypeCode.Boolean:
				return valueConverter.ChangeType(_unionVal.boolVal, type);
			case TypeCode.Int32:
				return valueConverter.ChangeType(_unionVal.i32Val, type);
			case TypeCode.Int64:
				return valueConverter.ChangeType(_unionVal.i64Val, type);
			case TypeCode.Double:
				return valueConverter.ChangeType(_unionVal.dblVal, type);
			case TypeCode.DateTime:
				return valueConverter.ChangeType(_unionVal.dtVal, type);
			}
		}
		return valueConverter.ChangeType(_objVal, type, nsResolver);
	}

	public override string ToString()
	{
		return Value;
	}

	private static string GetPrefixFromQName(string value)
	{
		int colonOffset;
		int num = ValidateNames.ParseQName(value, 0, out colonOffset);
		if (num == 0 || num != value.Length)
		{
			return null;
		}
		if (colonOffset != 0)
		{
			return value.Substring(0, colonOffset);
		}
		return string.Empty;
	}
}

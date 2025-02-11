using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Schema;
using MS.Internal.Xml.XPath;

namespace System.Xml.XPath;

/// <summary>Provides a cursor model for navigating and editing XML data.</summary>
[DebuggerDisplay("{debuggerDisplayProxy}")]
public abstract class XPathNavigator : XPathItem, ICloneable, IXPathNavigable, IXmlNamespaceResolver
{
	private class CheckValidityHelper
	{
		private bool isValid;

		private ValidationEventHandler nextEventHandler;

		private XPathNavigatorReader reader;

		internal bool IsValid => isValid;

		internal CheckValidityHelper(ValidationEventHandler nextEventHandler, XPathNavigatorReader reader)
		{
			isValid = true;
			this.nextEventHandler = nextEventHandler;
			this.reader = reader;
		}

		internal void ValidationCallback(object sender, ValidationEventArgs args)
		{
			if (args.Severity == XmlSeverityType.Error)
			{
				isValid = false;
			}
			XmlSchemaValidationException ex = args.Exception as XmlSchemaValidationException;
			if (ex != null && reader != null)
			{
				ex.SetSourceObject(reader.UnderlyingObject);
			}
			if (nextEventHandler != null)
			{
				nextEventHandler(sender, args);
			}
			else if (ex != null && args.Severity == XmlSeverityType.Error)
			{
				throw ex;
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	internal struct DebuggerDisplayProxy
	{
		private XPathNavigator nav;

		public DebuggerDisplayProxy(XPathNavigator nav)
		{
			this.nav = nav;
		}

		public override string ToString()
		{
			string text = nav.NodeType.ToString();
			switch (nav.NodeType)
			{
			case XPathNodeType.Element:
				text = text + ", Name=\"" + nav.Name + "\"";
				break;
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
			case XPathNodeType.ProcessingInstruction:
				text = text + ", Name=\"" + nav.Name + "\"";
				text = text + ", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(nav.Value) + "\"";
				break;
			case XPathNodeType.Text:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Whitespace:
			case XPathNodeType.Comment:
				text = text + ", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(nav.Value) + "\"";
				break;
			}
			return text;
		}
	}

	internal static readonly XPathNavigatorKeyComparer comparer = new XPathNavigatorKeyComparer();

	internal static readonly char[] NodeTypeLetter = new char[10] { 'R', 'E', 'A', 'N', 'T', 'S', 'W', 'P', 'C', 'X' };

	internal static readonly char[] UniqueIdTbl = new char[32]
	{
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
		'U', 'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4',
		'5', '6'
	};

	internal const int AllMask = int.MaxValue;

	internal const int NoAttrNmspMask = 2147483635;

	internal const int TextMask = 112;

	internal static readonly int[] ContentKindMasks = new int[10] { 1, 2, 0, 0, 112, 32, 64, 128, 256, 2147483635 };

	/// <summary>Gets a value indicating if the current node represents an XPath node.</summary>
	/// <returns>Always returns true.</returns>
	public sealed override bool IsNode => true;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaType" /> information for the current node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaType" /> object; default is null.</returns>
	public override XmlSchemaType XmlType
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null && schemaInfo.Validity == XmlSchemaValidity.Valid)
			{
				XmlSchemaType memberType = schemaInfo.MemberType;
				if (memberType != null)
				{
					return memberType;
				}
				return schemaInfo.SchemaType;
			}
			return null;
		}
	}

	/// <summary>Gets the current node as a boxed object of the most appropriate .NET Framework type.</summary>
	/// <returns>The current node as a boxed object of the most appropriate .NET Framework type.</returns>
	public override object TypedValue
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ChangeType(Value, datatype.ValueType, this);
						}
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ChangeType(datatype.ParseValue(Value, NameTable, this), datatype.ValueType, this);
						}
					}
				}
			}
			return Value;
		}
	}

	/// <summary>Gets the .NET Framework <see cref="T:System.Type" /> of the current node.</summary>
	/// <returns>The .NET Framework <see cref="T:System.Type" /> of the current node. The default value is <see cref="T:System.String" />.</returns>
	public override Type ValueType
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return datatype.ValueType;
						}
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return datatype.ValueType;
						}
					}
				}
			}
			return typeof(string);
		}
	}

	/// <summary>Gets the current node's value as a <see cref="T:System.Boolean" />.</summary>
	/// <returns>The current node's value as a <see cref="T:System.Boolean" />.</returns>
	/// <exception cref="T:System.FormatException">The current node's string value cannot be converted to a <see cref="T:System.Boolean" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Boolean" /> is not valid.</exception>
	public override bool ValueAsBoolean
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						return xmlSchemaType.ValueConverter.ToBoolean(Value);
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ToBoolean(datatype.ParseValue(Value, NameTable, this));
						}
					}
				}
			}
			return XmlUntypedConverter.Untyped.ToBoolean(Value);
		}
	}

	/// <summary>Gets the current node's value as a <see cref="T:System.DateTime" />.</summary>
	/// <returns>The current node's value as a <see cref="T:System.DateTime" />.</returns>
	/// <exception cref="T:System.FormatException">The current node's string value cannot be converted to a <see cref="T:System.DateTime" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.DateTime" /> is not valid.</exception>
	public override DateTime ValueAsDateTime
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						return xmlSchemaType.ValueConverter.ToDateTime(Value);
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ToDateTime(datatype.ParseValue(Value, NameTable, this));
						}
					}
				}
			}
			return XmlUntypedConverter.Untyped.ToDateTime(Value);
		}
	}

	/// <summary>Gets the current node's value as a <see cref="T:System.Double" />.</summary>
	/// <returns>The current node's value as a <see cref="T:System.Double" />.</returns>
	/// <exception cref="T:System.FormatException">The current node's string value cannot be converted to a <see cref="T:System.Double" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Double" /> is not valid.</exception>
	public override double ValueAsDouble
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						return xmlSchemaType.ValueConverter.ToDouble(Value);
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ToDouble(datatype.ParseValue(Value, NameTable, this));
						}
					}
				}
			}
			return XmlUntypedConverter.Untyped.ToDouble(Value);
		}
	}

	/// <summary>Gets the current node's value as an <see cref="T:System.Int32" />.</summary>
	/// <returns>The current node's value as an <see cref="T:System.Int32" />.</returns>
	/// <exception cref="T:System.FormatException">The current node's string value cannot be converted to a <see cref="T:System.Int32" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Int32" /> is not valid.</exception>
	public override int ValueAsInt
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						return xmlSchemaType.ValueConverter.ToInt32(Value);
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ToInt32(datatype.ParseValue(Value, NameTable, this));
						}
					}
				}
			}
			return XmlUntypedConverter.Untyped.ToInt32(Value);
		}
	}

	/// <summary>Gets the current node's value as an <see cref="T:System.Int64" />.</summary>
	/// <returns>The current node's value as an <see cref="T:System.Int64" />.</returns>
	/// <exception cref="T:System.FormatException">The current node's string value cannot be converted to a <see cref="T:System.Int64" />.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast to <see cref="T:System.Int64" /> is not valid.</exception>
	public override long ValueAsLong
	{
		get
		{
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				if (schemaInfo.Validity == XmlSchemaValidity.Valid)
				{
					XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
					if (xmlSchemaType == null)
					{
						xmlSchemaType = schemaInfo.SchemaType;
					}
					if (xmlSchemaType != null)
					{
						return xmlSchemaType.ValueConverter.ToInt64(Value);
					}
				}
				else
				{
					XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
					if (xmlSchemaType != null)
					{
						XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
						if (datatype != null)
						{
							return xmlSchemaType.ValueConverter.ToInt64(datatype.ParseValue(Value, NameTable, this));
						}
					}
				}
			}
			return XmlUntypedConverter.Untyped.ToInt64(Value);
		}
	}

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Xml.XmlNameTable" /> of the <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNameTable" /> object enabling you to get the atomized version of a <see cref="T:System.String" /> within the XML document.</returns>
	public abstract XmlNameTable NameTable { get; }

	/// <summary>Gets an <see cref="T:System.Collections.IEqualityComparer" /> used for equality comparison of <see cref="T:System.Xml.XPath.XPathNavigator" /> objects.</summary>
	/// <returns>An <see cref="T:System.Collections.IEqualityComparer" /> used for equality comparison of <see cref="T:System.Xml.XPath.XPathNavigator" /> objects.</returns>
	public static IEqualityComparer NavigatorComparer => comparer;

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Xml.XPath.XPathNodeType" /> of the current node.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XPath.XPathNodeType" /> values representing the current node.</returns>
	public abstract XPathNodeType NodeType { get; }

	/// <summary>When overridden in a derived class, gets the <see cref="P:System.Xml.XPath.XPathNavigator.Name" /> of the current node without any namespace prefix.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the local name of the current node, or <see cref="F:System.String.Empty" /> if the current node does not have a name (for example, text or comment nodes).</returns>
	public abstract string LocalName { get; }

	/// <summary>When overridden in a derived class, gets the qualified name of the current node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the qualified <see cref="P:System.Xml.XPath.XPathNavigator.Name" /> of the current node, or <see cref="F:System.String.Empty" /> if the current node does not have a name (for example, text or comment nodes).</returns>
	public abstract string Name { get; }

	/// <summary>When overridden in a derived class, gets the namespace URI of the current node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the namespace URI of the current node, or <see cref="F:System.String.Empty" /> if the current node has no namespace URI.</returns>
	public abstract string NamespaceURI { get; }

	/// <summary>When overridden in a derived class, gets the namespace prefix associated with the current node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the namespace prefix associated with the current node.</returns>
	public abstract string Prefix { get; }

	/// <summary>When overridden in a derived class, gets the base URI for the current node.</summary>
	/// <returns>The location from which the node was loaded, or <see cref="F:System.String.Empty" /> if there is no value.</returns>
	public abstract string BaseURI { get; }

	/// <summary>When overridden in a derived class, gets a value indicating whether the current node is an empty element without an end element tag.</summary>
	/// <returns>true if the current node is an empty element; otherwise, false.</returns>
	public abstract bool IsEmptyElement { get; }

	/// <summary>Gets the xml:lang scope for the current node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value of the xml:lang scope, or <see cref="F:System.String.Empty" /> if the current node has no xml:lang scope value to return.</returns>
	public virtual string XmlLang
	{
		get
		{
			XPathNavigator xPathNavigator = Clone();
			do
			{
				if (xPathNavigator.MoveToAttribute("lang", "http://www.w3.org/XML/1998/namespace"))
				{
					return xPathNavigator.Value;
				}
			}
			while (xPathNavigator.MoveToParent());
			return string.Empty;
		}
	}

	/// <summary>Used by <see cref="T:System.Xml.XPath.XPathNavigator" /> implementations which provide a "virtualized" XML view over a store, to provide access to underlying objects.</summary>
	/// <returns>The default is null.</returns>
	public virtual object UnderlyingObject => null;

	/// <summary>Gets a value indicating whether the current node has any attributes.</summary>
	/// <returns>Returns true if the current node has attributes; returns false if the current node has no attributes, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node.</returns>
	public virtual bool HasAttributes
	{
		get
		{
			if (!MoveToFirstAttribute())
			{
				return false;
			}
			MoveToParent();
			return true;
		}
	}

	/// <summary>Gets a value indicating whether the current node has any child nodes.</summary>
	/// <returns>true if the current node has any child nodes; otherwise, false.</returns>
	public virtual bool HasChildren
	{
		get
		{
			if (MoveToFirstChild())
			{
				MoveToParent();
				return true;
			}
			return false;
		}
	}

	/// <summary>Gets the schema information that has been assigned to the current node as a result of schema validation.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object that contains the schema information for the current node.</returns>
	public virtual IXmlSchemaInfo SchemaInfo => this as IXmlSchemaInfo;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XPath.XPathNavigator" /> can edit the underlying XML data.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> can edit the underlying XML data; otherwise false.</returns>
	public virtual bool CanEdit => false;

	/// <summary>Gets or sets the markup representing the opening and closing tags of the current node and its child nodes.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the markup representing the opening and closing tags of the current node and its child nodes.</returns>
	public virtual string OuterXml
	{
		get
		{
			if (NodeType == XPathNodeType.Attribute)
			{
				return Name + "=\"" + Value + "\"";
			}
			if (NodeType == XPathNodeType.Namespace)
			{
				if (LocalName.Length == 0)
				{
					return "xmlns=\"" + Value + "\"";
				}
				return "xmlns:" + LocalName + "=\"" + Value + "\"";
			}
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.OmitXmlDeclaration = true;
			xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
			XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
			try
			{
				xmlWriter.WriteNode(this, defattr: true);
			}
			finally
			{
				xmlWriter.Close();
			}
			return stringWriter.ToString();
		}
		set
		{
			ReplaceSelf(value);
		}
	}

	/// <summary>Gets or sets the markup representing the child nodes of the current node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the markup of the child nodes of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Xml.XPath.XPathNavigator.InnerXml" /> property cannot be set.</exception>
	public virtual string InnerXml
	{
		get
		{
			switch (NodeType)
			{
			case XPathNodeType.Root:
			case XPathNodeType.Element:
			{
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.Indent = true;
				xmlWriterSettings.OmitXmlDeclaration = true;
				xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
				XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
				try
				{
					if (MoveToFirstChild())
					{
						do
						{
							xmlWriter.WriteNode(this, defattr: true);
						}
						while (MoveToNext());
						MoveToParent();
					}
				}
				finally
				{
					xmlWriter.Close();
				}
				return stringWriter.ToString();
			}
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				return Value;
			default:
				return string.Empty;
			}
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			switch (NodeType)
			{
			case XPathNodeType.Root:
			case XPathNodeType.Element:
			{
				XPathNavigator xPathNavigator = CreateNavigator();
				while (xPathNavigator.MoveToFirstChild())
				{
					xPathNavigator.DeleteSelf();
				}
				if (value.Length != 0)
				{
					xPathNavigator.AppendChild(value);
				}
				break;
			}
			case XPathNodeType.Attribute:
				SetValue(value);
				break;
			default:
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
			}
		}
	}

	internal uint IndexInParent
	{
		get
		{
			XPathNavigator xPathNavigator = Clone();
			uint num = 0u;
			XPathNodeType nodeType = NodeType;
			if (nodeType != XPathNodeType.Attribute)
			{
				if (nodeType == XPathNodeType.Namespace)
				{
					while (xPathNavigator.MoveToNextNamespace())
					{
						num++;
					}
				}
				else
				{
					while (xPathNavigator.MoveToNext())
					{
						num++;
					}
				}
			}
			else
			{
				while (xPathNavigator.MoveToNextAttribute())
				{
					num++;
				}
			}
			return num;
		}
	}

	internal virtual string UniqueId
	{
		get
		{
			XPathNavigator xPathNavigator = Clone();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(NodeTypeLetter[(int)NodeType]);
			while (true)
			{
				uint num = xPathNavigator.IndexInParent;
				if (!xPathNavigator.MoveToParent())
				{
					break;
				}
				if (num <= 31)
				{
					stringBuilder.Append(UniqueIdTbl[num]);
					continue;
				}
				stringBuilder.Append('0');
				do
				{
					stringBuilder.Append(UniqueIdTbl[num & 0x1F]);
					num >>= 5;
				}
				while (num != 0);
				stringBuilder.Append('0');
			}
			return stringBuilder.ToString();
		}
	}

	private object debuggerDisplayProxy => new DebuggerDisplayProxy(this);

	/// <summary>Gets the text value of the current node.</summary>
	/// <returns>A string that contains the text value of the current node.</returns>
	public override string ToString()
	{
		return Value;
	}

	/// <summary>Sets the value of the current node.</summary>
	/// <param name="value">The new value of the node.</param>
	/// <exception cref="T:System.ArgumentNullException">The value parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on the root node, a namespace node, or the specified value is invalid.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void SetValue(string value)
	{
		throw new NotSupportedException();
	}

	/// <summary>Sets the typed value of the current node.</summary>
	/// <param name="typedValue">The new typed value of the node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support the type of the object specified.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value specified cannot be null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element or attribute node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void SetTypedValue(object typedValue)
	{
		if (typedValue == null)
		{
			throw new ArgumentNullException("typedValue");
		}
		XPathNodeType nodeType = NodeType;
		if ((uint)(nodeType - 1) > 1u)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		string text = null;
		IXmlSchemaInfo schemaInfo = SchemaInfo;
		if (schemaInfo != null)
		{
			XmlSchemaType schemaType = schemaInfo.SchemaType;
			if (schemaType != null)
			{
				text = schemaType.ValueConverter.ToString(typedValue, this);
				schemaType.Datatype?.ParseValue(text, NameTable, this);
			}
		}
		if (text == null)
		{
			text = XmlUntypedConverter.Untyped.ToString(typedValue, this);
		}
		SetValue(text);
	}

	/// <summary>Gets the current node's value as the <see cref="T:System.Type" /> specified, using the <see cref="T:System.Xml.IXmlNamespaceResolver" /> object specified to resolve namespace prefixes.</summary>
	/// <returns>The value of the current node as the <see cref="T:System.Type" /> requested.</returns>
	/// <param name="returnType">The <see cref="T:System.Type" /> to return the current node's value as.</param>
	/// <param name="nsResolver">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object used to resolve namespace prefixes.</param>
	/// <exception cref="T:System.FormatException">The current node's value is not in the correct format for the target type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	public override object ValueAs(Type returnType, IXmlNamespaceResolver nsResolver)
	{
		if (nsResolver == null)
		{
			nsResolver = this;
		}
		IXmlSchemaInfo schemaInfo = SchemaInfo;
		if (schemaInfo != null)
		{
			if (schemaInfo.Validity == XmlSchemaValidity.Valid)
			{
				XmlSchemaType xmlSchemaType = schemaInfo.MemberType;
				if (xmlSchemaType == null)
				{
					xmlSchemaType = schemaInfo.SchemaType;
				}
				if (xmlSchemaType != null)
				{
					return xmlSchemaType.ValueConverter.ChangeType(Value, returnType, nsResolver);
				}
			}
			else
			{
				XmlSchemaType xmlSchemaType = schemaInfo.SchemaType;
				if (xmlSchemaType != null)
				{
					XmlSchemaDatatype datatype = xmlSchemaType.Datatype;
					if (datatype != null)
					{
						return xmlSchemaType.ValueConverter.ChangeType(datatype.ParseValue(Value, NameTable, nsResolver), returnType, nsResolver);
					}
				}
			}
		}
		return XmlUntypedConverter.Untyped.ChangeType(Value, returnType, nsResolver);
	}

	/// <summary>Creates a new copy of the <see cref="T:System.Xml.XPath.XPathNavigator" /> object.</summary>
	/// <returns>A new copy of the <see cref="T:System.Xml.XPath.XPathNavigator" /> object.</returns>
	object ICloneable.Clone()
	{
		return Clone();
	}

	/// <summary>Returns a copy of the <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> copy of this <see cref="T:System.Xml.XPath.XPathNavigator" />.</returns>
	public virtual XPathNavigator CreateNavigator()
	{
		return Clone();
	}

	/// <summary>Gets the namespace URI for the specified prefix.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the namespace URI assigned to the namespace prefix specified; null if no namespace URI is assigned to the prefix specified. The <see cref="T:System.String" /> returned is atomized.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass <see cref="F:System.String.Empty" />.</param>
	public virtual string LookupNamespace(string prefix)
	{
		if (prefix == null)
		{
			return null;
		}
		if (NodeType != XPathNodeType.Element)
		{
			XPathNavigator xPathNavigator = Clone();
			if (xPathNavigator.MoveToParent())
			{
				return xPathNavigator.LookupNamespace(prefix);
			}
		}
		else if (MoveToNamespace(prefix))
		{
			string value = Value;
			MoveToParent();
			return value;
		}
		if (prefix.Length == 0)
		{
			return string.Empty;
		}
		if (prefix == "xml")
		{
			return "http://www.w3.org/XML/1998/namespace";
		}
		if (prefix == "xmlns")
		{
			return "http://www.w3.org/2000/xmlns/";
		}
		return null;
	}

	/// <summary>Gets the prefix declared for the specified namespace URI.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the namespace prefix assigned to the namespace URI specified; otherwise, <see cref="F:System.String.Empty" /> if no prefix is assigned to the namespace URI specified. The <see cref="T:System.String" /> returned is atomized.</returns>
	/// <param name="namespaceURI">The namespace URI to resolve for the prefix.</param>
	public virtual string LookupPrefix(string namespaceURI)
	{
		if (namespaceURI == null)
		{
			return null;
		}
		XPathNavigator xPathNavigator = Clone();
		if (NodeType != XPathNodeType.Element)
		{
			if (xPathNavigator.MoveToParent())
			{
				return xPathNavigator.LookupPrefix(namespaceURI);
			}
		}
		else if (xPathNavigator.MoveToFirstNamespace(XPathNamespaceScope.All))
		{
			do
			{
				if (namespaceURI == xPathNavigator.Value)
				{
					return xPathNavigator.LocalName;
				}
			}
			while (xPathNavigator.MoveToNextNamespace(XPathNamespaceScope.All));
		}
		if (namespaceURI == LookupNamespace(string.Empty))
		{
			return string.Empty;
		}
		if (namespaceURI == "http://www.w3.org/XML/1998/namespace")
		{
			return "xml";
		}
		if (namespaceURI == "http://www.w3.org/2000/xmlns/")
		{
			return "xmlns";
		}
		return null;
	}

	/// <summary>Returns the in-scope namespaces of the current node.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> collection of namespace names keyed by prefix.</returns>
	/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope" /> value specifying the namespaces to return.</param>
	public virtual IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
	{
		XPathNodeType nodeType = NodeType;
		if ((nodeType != XPathNodeType.Element && scope != XmlNamespaceScope.Local) || nodeType == XPathNodeType.Attribute || nodeType == XPathNodeType.Namespace)
		{
			XPathNavigator xPathNavigator = Clone();
			if (xPathNavigator.MoveToParent())
			{
				return xPathNavigator.GetNamespacesInScope(scope);
			}
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (scope == XmlNamespaceScope.All)
		{
			dictionary["xml"] = "http://www.w3.org/XML/1998/namespace";
		}
		if (MoveToFirstNamespace((XPathNamespaceScope)scope))
		{
			do
			{
				string localName = LocalName;
				string value = Value;
				if (localName.Length != 0 || value.Length != 0 || scope == XmlNamespaceScope.Local)
				{
					dictionary[localName] = value;
				}
			}
			while (MoveToNextNamespace((XPathNamespaceScope)scope));
			MoveToParent();
		}
		return dictionary;
	}

	/// <summary>When overridden in a derived class, creates a new <see cref="T:System.Xml.XPath.XPathNavigator" /> positioned at the same node as this <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>A new <see cref="T:System.Xml.XPath.XPathNavigator" /> positioned at the same node as this <see cref="T:System.Xml.XPath.XPathNavigator" />.</returns>
	public abstract XPathNavigator Clone();

	/// <summary>Returns an <see cref="T:System.Xml.XmlReader" /> object that contains the current node and its child nodes.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object that contains the current node and its child nodes.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node or the root node.</exception>
	public virtual XmlReader ReadSubtree()
	{
		XPathNodeType nodeType = NodeType;
		if ((uint)nodeType > 1u)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		return CreateReader();
	}

	/// <summary>Streams the current node and its child nodes to the <see cref="T:System.Xml.XmlWriter" /> object specified.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> object to stream to.</param>
	public virtual void WriteSubtree(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteNode(this, defattr: true);
	}

	/// <summary>Gets the value of the attribute with the specified local name and namespace URI.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value of the specified attribute; <see cref="F:System.String.Empty" /> if a matching attribute is not found, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node.</returns>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceURI">The namespace URI of the attribute.</param>
	public virtual string GetAttribute(string localName, string namespaceURI)
	{
		if (!MoveToAttribute(localName, namespaceURI))
		{
			return "";
		}
		string value = Value;
		MoveToParent();
		return value;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the attribute with the matching local name and namespace URI.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the attribute; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceURI">The namespace URI of the attribute; null for an empty namespace.</param>
	public virtual bool MoveToAttribute(string localName, string namespaceURI)
	{
		if (MoveToFirstAttribute())
		{
			do
			{
				if (localName == LocalName && namespaceURI == NamespaceURI)
				{
					return true;
				}
			}
			while (MoveToNextAttribute());
			MoveToParent();
		}
		return false;
	}

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the first attribute of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the first attribute of the current node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToFirstAttribute();

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next attribute.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next attribute; false if there are no more attributes. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToNextAttribute();

	/// <summary>Returns the value of the namespace node corresponding to the specified local name.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value of the namespace node; <see cref="F:System.String.Empty" /> if a matching namespace node is not found, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node.</returns>
	/// <param name="name">The local name of the namespace node.</param>
	public virtual string GetNamespace(string name)
	{
		if (!MoveToNamespace(name))
		{
			if (name == "xml")
			{
				return "http://www.w3.org/XML/1998/namespace";
			}
			if (name == "xmlns")
			{
				return "http://www.w3.org/2000/xmlns/";
			}
			return string.Empty;
		}
		string value = Value;
		MoveToParent();
		return value;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the namespace node with the specified namespace prefix.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the specified namespace; false if a matching namespace node was not found, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="name">The namespace prefix of the namespace node.</param>
	public virtual bool MoveToNamespace(string name)
	{
		if (MoveToFirstNamespace(XPathNamespaceScope.All))
		{
			do
			{
				if (name == LocalName)
				{
					return true;
				}
			}
			while (MoveToNextNamespace(XPathNamespaceScope.All));
			MoveToParent();
		}
		return false;
	}

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the first namespace node that matches the <see cref="T:System.Xml.XPath.XPathNamespaceScope" /> specified.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the first namespace node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="namespaceScope">An <see cref="T:System.Xml.XPath.XPathNamespaceScope" /> value describing the namespace scope. </param>
	public abstract bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope);

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next namespace node matching the <see cref="T:System.Xml.XPath.XPathNamespaceScope" /> specified.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next namespace node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="namespaceScope">An <see cref="T:System.Xml.XPath.XPathNamespaceScope" /> value describing the namespace scope. </param>
	public abstract bool MoveToNextNamespace(XPathNamespaceScope namespaceScope);

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to first namespace node of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the first namespace node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public bool MoveToFirstNamespace()
	{
		return MoveToFirstNamespace(XPathNamespaceScope.All);
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next namespace node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next namespace node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public bool MoveToNextNamespace()
	{
		return MoveToNextNamespace(XPathNamespaceScope.All);
	}

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next sibling node of the current node.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next sibling node; otherwise, false if there are no more siblings or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute node. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToNext();

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the previous sibling node of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the previous sibling node; otherwise, false if there is no previous sibling node or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute node. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToPrevious();

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the first sibling node of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the first sibling node of the current node; false if there is no first sibling, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute node. If the <see cref="T:System.Xml.XPath.XPathNavigator" /> is already positioned on the first sibling, <see cref="T:System.Xml.XPath.XPathNavigator" /> will return true and will not move its position.If <see cref="T:System.Xml.XPath.XPathNavigator.MoveToFirst" /> returns false because there is no first sibling, or if <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public virtual bool MoveToFirst()
	{
		XPathNodeType nodeType = NodeType;
		if ((uint)(nodeType - 2) <= 1u)
		{
			return false;
		}
		if (!MoveToParent())
		{
			return false;
		}
		return MoveToFirstChild();
	}

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the first child node of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the first child node of the current node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToFirstChild();

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the parent node of the current node.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the parent node of the current node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	public abstract bool MoveToParent();

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the root node that the current node belongs to.</summary>
	public virtual void MoveToRoot()
	{
		while (MoveToParent())
		{
		}
	}

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the same position as the specified <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the same position as the specified <see cref="T:System.Xml.XPath.XPathNavigator" />; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="other">The <see cref="T:System.Xml.XPath.XPathNavigator" /> positioned on the node that you want to move to. </param>
	public abstract bool MoveTo(XPathNavigator other);

	/// <summary>When overridden in a derived class, moves to the node that has an attribute of type ID whose value matches the specified <see cref="T:System.String" />.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving; otherwise, false. If false, the position of the navigator is unchanged.</returns>
	/// <param name="id">A <see cref="T:System.String" /> representing the ID value of the node to which you want to move.</param>
	public abstract bool MoveToId(string id);

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the child node with the local name and namespace URI specified.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the child node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="localName">The local name of the child node to move to.</param>
	/// <param name="namespaceURI">The namespace URI of the child node to move to.</param>
	public virtual bool MoveToChild(string localName, string namespaceURI)
	{
		if (MoveToFirstChild())
		{
			do
			{
				if (NodeType == XPathNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
				{
					return true;
				}
			}
			while (MoveToNext());
			MoveToParent();
		}
		return false;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the child node of the <see cref="T:System.Xml.XPath.XPathNodeType" /> specified.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the child node; otherwise, false. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the child node to move to.</param>
	public virtual bool MoveToChild(XPathNodeType type)
	{
		if (MoveToFirstChild())
		{
			int contentKindMask = GetContentKindMask(type);
			do
			{
				if (((1 << (int)NodeType) & contentKindMask) != 0)
				{
					return true;
				}
			}
			while (MoveToNext());
			MoveToParent();
		}
		return false;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the element with the local name and namespace URI specified in document order.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> moved successfully; otherwise false.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	public virtual bool MoveToFollowing(string localName, string namespaceURI)
	{
		return MoveToFollowing(localName, namespaceURI, null);
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the element with the local name and namespace URI specified, to the boundary specified, in document order.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> moved successfully; otherwise false.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <param name="end">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the element boundary which the current <see cref="T:System.Xml.XPath.XPathNavigator" /> will not move past while searching for the following element.</param>
	public virtual bool MoveToFollowing(string localName, string namespaceURI, XPathNavigator end)
	{
		XPathNavigator other = Clone();
		XPathNodeType nodeType;
		if (end != null)
		{
			nodeType = end.NodeType;
			if ((uint)(nodeType - 2) <= 1u)
			{
				end = end.Clone();
				end.MoveToNonDescendant();
			}
		}
		nodeType = NodeType;
		if ((uint)(nodeType - 2) <= 1u && !MoveToParent())
		{
			return false;
		}
		do
		{
			if (!MoveToFirstChild())
			{
				while (!MoveToNext())
				{
					if (!MoveToParent())
					{
						MoveTo(other);
						return false;
					}
				}
			}
			if (end != null && IsSamePosition(end))
			{
				MoveTo(other);
				return false;
			}
		}
		while (NodeType != XPathNodeType.Element || localName != LocalName || namespaceURI != NamespaceURI);
		return true;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the following element of the <see cref="T:System.Xml.XPath.XPathNodeType" /> specified in document order.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> moved successfully; otherwise false.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the element. The <see cref="T:System.Xml.XPath.XPathNodeType" /> cannot be <see cref="F:System.Xml.XPath.XPathNodeType.Attribute" /> or <see cref="F:System.Xml.XPath.XPathNodeType.Namespace" />.</param>
	public virtual bool MoveToFollowing(XPathNodeType type)
	{
		return MoveToFollowing(type, null);
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the following element of the <see cref="T:System.Xml.XPath.XPathNodeType" /> specified, to the boundary specified, in document order.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> moved successfully; otherwise false.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the element. The <see cref="T:System.Xml.XPath.XPathNodeType" /> cannot be <see cref="F:System.Xml.XPath.XPathNodeType.Attribute" /> or <see cref="F:System.Xml.XPath.XPathNodeType.Namespace" />.</param>
	/// <param name="end">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the element boundary which the current <see cref="T:System.Xml.XPath.XPathNavigator" /> will not move past while searching for the following element.</param>
	public virtual bool MoveToFollowing(XPathNodeType type, XPathNavigator end)
	{
		XPathNavigator other = Clone();
		int contentKindMask = GetContentKindMask(type);
		XPathNodeType nodeType;
		if (end != null)
		{
			nodeType = end.NodeType;
			if ((uint)(nodeType - 2) <= 1u)
			{
				end = end.Clone();
				end.MoveToNonDescendant();
			}
		}
		nodeType = NodeType;
		if ((uint)(nodeType - 2) <= 1u && !MoveToParent())
		{
			return false;
		}
		do
		{
			if (!MoveToFirstChild())
			{
				while (!MoveToNext())
				{
					if (!MoveToParent())
					{
						MoveTo(other);
						return false;
					}
				}
			}
			if (end != null && IsSamePosition(end))
			{
				MoveTo(other);
				return false;
			}
		}
		while (((1 << (int)NodeType) & contentKindMask) == 0);
		return true;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next sibling node with the local name and namespace URI specified.</summary>
	/// <returns>Returns true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next sibling node; false if there are no more siblings, or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute node. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="localName">The local name of the next sibling node to move to.</param>
	/// <param name="namespaceURI">The namespace URI of the next sibling node to move to.</param>
	public virtual bool MoveToNext(string localName, string namespaceURI)
	{
		XPathNavigator other = Clone();
		while (MoveToNext())
		{
			if (NodeType == XPathNodeType.Element && localName == LocalName && namespaceURI == NamespaceURI)
			{
				return true;
			}
		}
		MoveTo(other);
		return false;
	}

	/// <summary>Moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> to the next sibling node of the current node that matches the <see cref="T:System.Xml.XPath.XPathNodeType" /> specified.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is successful moving to the next sibling node; otherwise, false if there are no more siblings or if the <see cref="T:System.Xml.XPath.XPathNavigator" /> is currently positioned on an attribute node. If false, the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> is unchanged.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the sibling node to move to.</param>
	public virtual bool MoveToNext(XPathNodeType type)
	{
		XPathNavigator other = Clone();
		int contentKindMask = GetContentKindMask(type);
		while (MoveToNext())
		{
			if (((1 << (int)NodeType) & contentKindMask) != 0)
			{
				return true;
			}
		}
		MoveTo(other);
		return false;
	}

	/// <summary>When overridden in a derived class, determines whether the current <see cref="T:System.Xml.XPath.XPathNavigator" /> is at the same position as the specified <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>true if the two <see cref="T:System.Xml.XPath.XPathNavigator" /> objects have the same position; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Xml.XPath.XPathNavigator" /> to compare to this <see cref="T:System.Xml.XPath.XPathNavigator" />.</param>
	public abstract bool IsSamePosition(XPathNavigator other);

	/// <summary>Determines whether the specified <see cref="T:System.Xml.XPath.XPathNavigator" /> is a descendant of the current <see cref="T:System.Xml.XPath.XPathNavigator" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Xml.XPath.XPathNavigator" /> is a descendant of the current <see cref="T:System.Xml.XPath.XPathNavigator" />; otherwise, false.</returns>
	/// <param name="nav">The <see cref="T:System.Xml.XPath.XPathNavigator" /> to compare to this <see cref="T:System.Xml.XPath.XPathNavigator" />.</param>
	public virtual bool IsDescendant(XPathNavigator nav)
	{
		if (nav != null)
		{
			nav = nav.Clone();
			while (nav.MoveToParent())
			{
				if (nav.IsSamePosition(this))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Compares the position of the current <see cref="T:System.Xml.XPath.XPathNavigator" /> with the position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> specified.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeOrder" /> value representing the comparative position of the two <see cref="T:System.Xml.XPath.XPathNavigator" /> objects.</returns>
	/// <param name="nav">The <see cref="T:System.Xml.XPath.XPathNavigator" /> to compare against.</param>
	public virtual XmlNodeOrder ComparePosition(XPathNavigator nav)
	{
		if (nav == null)
		{
			return XmlNodeOrder.Unknown;
		}
		if (IsSamePosition(nav))
		{
			return XmlNodeOrder.Same;
		}
		XPathNavigator xPathNavigator = Clone();
		XPathNavigator xPathNavigator2 = nav.Clone();
		int num = GetDepth(xPathNavigator.Clone());
		int num2 = GetDepth(xPathNavigator2.Clone());
		if (num > num2)
		{
			while (num > num2)
			{
				xPathNavigator.MoveToParent();
				num--;
			}
			if (xPathNavigator.IsSamePosition(xPathNavigator2))
			{
				return XmlNodeOrder.After;
			}
		}
		if (num2 > num)
		{
			while (num2 > num)
			{
				xPathNavigator2.MoveToParent();
				num2--;
			}
			if (xPathNavigator.IsSamePosition(xPathNavigator2))
			{
				return XmlNodeOrder.Before;
			}
		}
		XPathNavigator xPathNavigator3 = xPathNavigator.Clone();
		XPathNavigator xPathNavigator4 = xPathNavigator2.Clone();
		while (true)
		{
			if (!xPathNavigator3.MoveToParent() || !xPathNavigator4.MoveToParent())
			{
				return XmlNodeOrder.Unknown;
			}
			if (xPathNavigator3.IsSamePosition(xPathNavigator4))
			{
				break;
			}
			xPathNavigator.MoveToParent();
			xPathNavigator2.MoveToParent();
		}
		_ = xPathNavigator.GetType().ToString() != "Microsoft.VisualStudio.Modeling.StoreNavigator";
		return CompareSiblings(xPathNavigator, xPathNavigator2);
	}

	/// <summary>Verifies that the XML data in the <see cref="T:System.Xml.XPath.XPathNavigator" /> conforms to the XML Schema definition language (XSD) schema provided.</summary>
	/// <returns>true if no schema validation errors occurred; otherwise, false.</returns>
	/// <param name="schemas">The <see cref="T:System.Xml.Schema.XmlSchemaSet" /> containing the schemas used to validate the XML data contained in the <see cref="T:System.Xml.XPath.XPathNavigator" />.</param>
	/// <param name="validationEventHandler">The <see cref="T:System.Xml.Schema.ValidationEventHandler" /> that receives information about schema validation warnings and errors.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">A schema validation error occurred, and no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> was specified to handle validation errors.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on a node that is not an element, attribute, or the root node or there is not type information to perform validation.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="M:System.Xml.XPath.XPathNavigator.CheckValidity(System.Xml.Schema.XmlSchemaSet,System.Xml.Schema.ValidationEventHandler)" /> method was called with an <see cref="T:System.Xml.Schema.XmlSchemaSet" /> parameter when the <see cref="T:System.Xml.XPath.XPathNavigator" /> was not positioned on the root node of the XML data.</exception>
	public virtual bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
	{
		XmlSchemaType xmlSchemaType = null;
		XmlSchemaElement xmlSchemaElement = null;
		XmlSchemaAttribute xmlSchemaAttribute = null;
		switch (NodeType)
		{
		case XPathNodeType.Root:
			if (schemas == null)
			{
				throw new InvalidOperationException(Res.GetString("An XmlSchemaSet must be provided to validate the document."));
			}
			xmlSchemaType = null;
			break;
		case XPathNodeType.Element:
		{
			if (schemas == null)
			{
				throw new InvalidOperationException(Res.GetString("An XmlSchemaSet must be provided to validate the document."));
			}
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				xmlSchemaType = schemaInfo.SchemaType;
				xmlSchemaElement = schemaInfo.SchemaElement;
			}
			if (xmlSchemaType == null && xmlSchemaElement == null)
			{
				throw new InvalidOperationException(Res.GetString("Element should have prior schema information to call this method.", null));
			}
			break;
		}
		case XPathNodeType.Attribute:
		{
			if (schemas == null)
			{
				throw new InvalidOperationException(Res.GetString("An XmlSchemaSet must be provided to validate the document."));
			}
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null)
			{
				xmlSchemaType = schemaInfo.SchemaType;
				xmlSchemaAttribute = schemaInfo.SchemaAttribute;
			}
			if (xmlSchemaType == null && xmlSchemaAttribute == null)
			{
				throw new InvalidOperationException(Res.GetString("Element should have prior schema information to call this method.", null));
			}
			break;
		}
		default:
			throw new InvalidOperationException(Res.GetString("Validate and CheckValidity are only allowed on Root or Element nodes.", null));
		}
		XmlReader xmlReader = CreateReader();
		CheckValidityHelper checkValidityHelper = new CheckValidityHelper(validationEventHandler, xmlReader as XPathNavigatorReader);
		validationEventHandler = checkValidityHelper.ValidationCallback;
		XmlReader validatingReader = GetValidatingReader(xmlReader, schemas, validationEventHandler, xmlSchemaType, xmlSchemaElement, xmlSchemaAttribute);
		while (validatingReader.Read())
		{
		}
		return checkValidityHelper.IsValid;
	}

	private XmlReader GetValidatingReader(XmlReader reader, XmlSchemaSet schemas, ValidationEventHandler validationEvent, XmlSchemaType schemaType, XmlSchemaElement schemaElement, XmlSchemaAttribute schemaAttribute)
	{
		if (schemaAttribute != null)
		{
			return schemaAttribute.Validate(reader, null, schemas, validationEvent);
		}
		if (schemaElement != null)
		{
			return schemaElement.Validate(reader, null, schemas, validationEvent);
		}
		if (schemaType != null)
		{
			return schemaType.Validate(reader, null, schemas, validationEvent);
		}
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
		xmlReaderSettings.ValidationType = ValidationType.Schema;
		xmlReaderSettings.Schemas = schemas;
		xmlReaderSettings.ValidationEventHandler += validationEvent;
		return XmlReader.Create(reader, xmlReaderSettings);
	}

	/// <summary>Compiles a string representing an XPath expression and returns an <see cref="T:System.Xml.XPath.XPathExpression" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathExpression" /> object representing the XPath expression.</returns>
	/// <param name="xpath">A string representing an XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="xpath" /> parameter contains an XPath expression that is not valid.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual XPathExpression Compile(string xpath)
	{
		return XPathExpression.Compile(xpath);
	}

	/// <summary>Selects a single node in the <see cref="T:System.Xml.XPath.XPathNavigator" /> using the specified XPath query.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object that contains the first matching node for the XPath query specified; otherwise, null if there are no query results.</returns>
	/// <param name="xpath">A <see cref="T:System.String" /> representing an XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">An error was encountered in the XPath query or the return type of the XPath expression is not a node.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath query is not valid.</exception>
	public virtual XPathNavigator SelectSingleNode(string xpath)
	{
		return SelectSingleNode(XPathExpression.Compile(xpath));
	}

	/// <summary>Selects a single node in the <see cref="T:System.Xml.XPath.XPathNavigator" /> object using the specified XPath query with the <see cref="T:System.Xml.IXmlNamespaceResolver" /> object specified to resolve namespace prefixes.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object that contains the first matching node for the XPath query specified; otherwise null if there are no query results.</returns>
	/// <param name="xpath">A <see cref="T:System.String" /> representing an XPath expression.</param>
	/// <param name="resolver">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object used to resolve namespace prefixes in the XPath query.</param>
	/// <exception cref="T:System.ArgumentException">An error was encountered in the XPath query or the return type of the XPath expression is not a node.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath query is not valid.</exception>
	public virtual XPathNavigator SelectSingleNode(string xpath, IXmlNamespaceResolver resolver)
	{
		return SelectSingleNode(XPathExpression.Compile(xpath, resolver));
	}

	/// <summary>Selects a single node in the <see cref="T:System.Xml.XPath.XPathNavigator" /> using the specified <see cref="T:System.Xml.XPath.XPathExpression" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object that contains the first matching node for the XPath query specified; otherwise null if there are no query results.</returns>
	/// <param name="expression">An <see cref="T:System.Xml.XPath.XPathExpression" /> object containing the compiled XPath query.</param>
	/// <exception cref="T:System.ArgumentException">An error was encountered in the XPath query or the return type of the XPath expression is not a node.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath query is not valid.</exception>
	public virtual XPathNavigator SelectSingleNode(XPathExpression expression)
	{
		XPathNodeIterator xPathNodeIterator = Select(expression);
		if (xPathNodeIterator.MoveNext())
		{
			return xPathNodeIterator.Current;
		}
		return null;
	}

	/// <summary>Selects a node set, using the specified XPath expression.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> pointing to the selected node set.</returns>
	/// <param name="xpath">A <see cref="T:System.String" /> representing an XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">The XPath expression contains an error or its return type is not a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual XPathNodeIterator Select(string xpath)
	{
		return Select(XPathExpression.Compile(xpath));
	}

	/// <summary>Selects a node set using the specified XPath expression with the <see cref="T:System.Xml.IXmlNamespaceResolver" /> object specified to resolve namespace prefixes.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that points to the selected node set.</returns>
	/// <param name="xpath">A <see cref="T:System.String" /> representing an XPath expression.</param>
	/// <param name="resolver">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object used to resolve namespace prefixes.</param>
	/// <exception cref="T:System.ArgumentException">The XPath expression contains an error or its return type is not a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual XPathNodeIterator Select(string xpath, IXmlNamespaceResolver resolver)
	{
		return Select(XPathExpression.Compile(xpath, resolver));
	}

	/// <summary>Selects a node set using the specified <see cref="T:System.Xml.XPath.XPathExpression" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that points to the selected node set.</returns>
	/// <param name="expr">An <see cref="T:System.Xml.XPath.XPathExpression" /> object containing the compiled XPath query.</param>
	/// <exception cref="T:System.ArgumentException">The XPath expression contains an error or its return type is not a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual XPathNodeIterator Select(XPathExpression expr)
	{
		return (Evaluate(expr) as XPathNodeIterator) ?? throw XPathException.Create("Expression must evaluate to a node-set.");
	}

	/// <summary>Evaluates the specified XPath expression and returns the typed result.</summary>
	/// <returns>The result of the expression (Boolean, number, string, or node set). This maps to <see cref="T:System.Boolean" />, <see cref="T:System.Double" />, <see cref="T:System.String" />, or <see cref="T:System.Xml.XPath.XPathNodeIterator" /> objects respectively.</returns>
	/// <param name="xpath">A string representing an XPath expression that can be evaluated.</param>
	/// <exception cref="T:System.ArgumentException">The return type of the XPath expression is a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual object Evaluate(string xpath)
	{
		return Evaluate(XPathExpression.Compile(xpath), null);
	}

	/// <summary>Evaluates the specified XPath expression and returns the typed result, using the <see cref="T:System.Xml.IXmlNamespaceResolver" /> object specified to resolve namespace prefixes in the XPath expression.</summary>
	/// <returns>The result of the expression (Boolean, number, string, or node set). This maps to <see cref="T:System.Boolean" />, <see cref="T:System.Double" />, <see cref="T:System.String" />, or <see cref="T:System.Xml.XPath.XPathNodeIterator" /> objects respectively.</returns>
	/// <param name="xpath">A string representing an XPath expression that can be evaluated.</param>
	/// <param name="resolver">The <see cref="T:System.Xml.IXmlNamespaceResolver" /> object used to resolve namespace prefixes in the XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">The return type of the XPath expression is a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual object Evaluate(string xpath, IXmlNamespaceResolver resolver)
	{
		return Evaluate(XPathExpression.Compile(xpath, resolver));
	}

	/// <summary>Evaluates the <see cref="T:System.Xml.XPath.XPathExpression" /> and returns the typed result.</summary>
	/// <returns>The result of the expression (Boolean, number, string, or node set). This maps to <see cref="T:System.Boolean" />, <see cref="T:System.Double" />, <see cref="T:System.String" />, or <see cref="T:System.Xml.XPath.XPathNodeIterator" /> objects respectively.</returns>
	/// <param name="expr">An <see cref="T:System.Xml.XPath.XPathExpression" /> that can be evaluated.</param>
	/// <exception cref="T:System.ArgumentException">The return type of the XPath expression is a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual object Evaluate(XPathExpression expr)
	{
		return Evaluate(expr, null);
	}

	/// <summary>Uses the supplied context to evaluate the <see cref="T:System.Xml.XPath.XPathExpression" />, and returns the typed result.</summary>
	/// <returns>The result of the expression (Boolean, number, string, or node set). This maps to <see cref="T:System.Boolean" />, <see cref="T:System.Double" />, <see cref="T:System.String" />, or <see cref="T:System.Xml.XPath.XPathNodeIterator" /> objects respectively.</returns>
	/// <param name="expr">An <see cref="T:System.Xml.XPath.XPathExpression" /> that can be evaluated.</param>
	/// <param name="context">An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that points to the selected node set that the evaluation is to be performed on.</param>
	/// <exception cref="T:System.ArgumentException">The return type of the XPath expression is a node set.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual object Evaluate(XPathExpression expr, XPathNodeIterator context)
	{
		Query query = Query.Clone(((expr as CompiledXpathExpr) ?? throw XPathException.Create("This is an invalid object. Only objects returned from Compile() can be passed as input.")).QueryTree);
		query.Reset();
		if (context == null)
		{
			context = new XPathSingletonIterator(Clone(), moved: true);
		}
		object obj = query.Evaluate(context);
		if (obj is XPathNodeIterator)
		{
			return new XPathSelectionIterator(context.Current, query);
		}
		return obj;
	}

	/// <summary>Determines whether the current node matches the specified <see cref="T:System.Xml.XPath.XPathExpression" />.</summary>
	/// <returns>true if the current node matches the <see cref="T:System.Xml.XPath.XPathExpression" />; otherwise, false.</returns>
	/// <param name="expr">An <see cref="T:System.Xml.XPath.XPathExpression" /> object containing the compiled XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">The XPath expression cannot be evaluated.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual bool Matches(XPathExpression expr)
	{
		if (!(expr is CompiledXpathExpr compiledXpathExpr))
		{
			throw XPathException.Create("This is an invalid object. Only objects returned from Compile() can be passed as input.");
		}
		Query query = Query.Clone(compiledXpathExpr.QueryTree);
		try
		{
			return query.MatchNode(this) != null;
		}
		catch (XPathException)
		{
			throw XPathException.Create("'{0}' is an invalid XSLT pattern.", compiledXpathExpr.Expression);
		}
	}

	/// <summary>Determines whether the current node matches the specified XPath expression.</summary>
	/// <returns>true if the current node matches the specified XPath expression; otherwise, false.</returns>
	/// <param name="xpath">The XPath expression.</param>
	/// <exception cref="T:System.ArgumentException">The XPath expression cannot be evaluated.</exception>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression is not valid.</exception>
	public virtual bool Matches(string xpath)
	{
		return Matches(CompileMatchPattern(xpath));
	}

	/// <summary>Selects all the child nodes of the current node that have the matching <see cref="T:System.Xml.XPath.XPathNodeType" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the child nodes.</param>
	public virtual XPathNodeIterator SelectChildren(XPathNodeType type)
	{
		return new XPathChildIterator(Clone(), type);
	}

	/// <summary>Selects all the child nodes of the current node that have the local name and namespace URI specified.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes.</returns>
	/// <param name="name">The local name of the child nodes. </param>
	/// <param name="namespaceURI">The namespace URI of the child nodes. </param>
	/// <exception cref="T:System.ArgumentNullException">null cannot be passed as a parameter.</exception>
	public virtual XPathNodeIterator SelectChildren(string name, string namespaceURI)
	{
		return new XPathChildIterator(Clone(), name, namespaceURI);
	}

	/// <summary>Selects all the ancestor nodes of the current node that have a matching <see cref="T:System.Xml.XPath.XPathNodeType" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes. The returned nodes are in reverse document order.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the ancestor nodes.</param>
	/// <param name="matchSelf">To include the context node in the selection, true; otherwise, false.</param>
	public virtual XPathNodeIterator SelectAncestors(XPathNodeType type, bool matchSelf)
	{
		return new XPathAncestorIterator(Clone(), type, matchSelf);
	}

	/// <summary>Selects all the ancestor nodes of the current node that have the specified local name and namespace URI.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes. The returned nodes are in reverse document order.</returns>
	/// <param name="name">The local name of the ancestor nodes.</param>
	/// <param name="namespaceURI">The namespace URI of the ancestor nodes.</param>
	/// <param name="matchSelf">To include the context node in the selection, true; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentNullException">null cannot be passed as a parameter.</exception>
	public virtual XPathNodeIterator SelectAncestors(string name, string namespaceURI, bool matchSelf)
	{
		return new XPathAncestorIterator(Clone(), name, namespaceURI, matchSelf);
	}

	/// <summary>Selects all the descendant nodes of the current node that have a matching <see cref="T:System.Xml.XPath.XPathNodeType" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes.</returns>
	/// <param name="type">The <see cref="T:System.Xml.XPath.XPathNodeType" /> of the descendant nodes.</param>
	/// <param name="matchSelf">true to include the context node in the selection; otherwise, false.</param>
	public virtual XPathNodeIterator SelectDescendants(XPathNodeType type, bool matchSelf)
	{
		return new XPathDescendantIterator(Clone(), type, matchSelf);
	}

	/// <summary>Selects all the descendant nodes of the current node with the local name and namespace URI specified.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNodeIterator" /> that contains the selected nodes.</returns>
	/// <param name="name">The local name of the descendant nodes. </param>
	/// <param name="namespaceURI">The namespace URI of the descendant nodes. </param>
	/// <param name="matchSelf">true to include the context node in the selection; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">null cannot be passed as a parameter.</exception>
	public virtual XPathNodeIterator SelectDescendants(string name, string namespaceURI, bool matchSelf)
	{
		return new XPathDescendantIterator(Clone(), name, namespaceURI, matchSelf);
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlWriter" /> object used to create a new child node at the beginning of the list of child nodes of the current node.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to create a new child node at the beginning of the list of child nodes of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on does not allow a new child node to be prepended.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual XmlWriter PrependChild()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlWriter" /> object used to create one or more new child nodes at the end of the list of child nodes of the current node. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to create new child nodes at the end of the list of child nodes of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on is not the root node or an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual XmlWriter AppendChild()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlWriter" /> object used to create a new sibling node after the currently selected node.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to create a new sibling node after the currently selected node.</returns>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted after the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual XmlWriter InsertAfter()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlWriter" /> object used to create a new sibling node before the currently selected node.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to create a new sibling node before the currently selected node.</returns>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted before the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual XmlWriter InsertBefore()
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlWriter" /> object used to create new attributes on the current element.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to create new attributes on the current element.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual XmlWriter CreateAttributes()
	{
		throw new NotSupportedException();
	}

	/// <summary>Replaces a range of sibling nodes from the current node to the node specified.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> object used to specify the replacement range.</returns>
	/// <param name="lastSiblingToReplace">An <see cref="T:System.Xml.XPath.XPathNavigator" /> positioned on the last sibling node in the range to replace.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> specified is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.InvalidOperationException">The last node to replace specified is not a valid sibling node of the current node.</exception>
	public virtual XmlWriter ReplaceRange(XPathNavigator lastSiblingToReplace)
	{
		throw new NotSupportedException();
	}

	/// <summary>Replaces the current node with the content of the string specified.</summary>
	/// <param name="newNode">The XML data string for the new node.</param>
	/// <exception cref="T:System.ArgumentNullException">The XML string parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element, text, processing instruction, or comment node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML string parameter is not well-formed.</exception>
	public virtual void ReplaceSelf(string newNode)
	{
		XmlReader newNode2 = CreateContextReader(newNode, fromCurrentNode: false);
		ReplaceSelf(newNode2);
	}

	/// <summary>Replaces the current node with the contents of the <see cref="T:System.Xml.XmlReader" /> object specified.</summary>
	/// <param name="newNode">An <see cref="T:System.Xml.XmlReader" /> object positioned on the XML data for the new node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlReader" /> object is in an error state or closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element, text, processing instruction, or comment node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XmlReader" /> object parameter is not well-formed.</exception>
	public virtual void ReplaceSelf(XmlReader newNode)
	{
		if (newNode == null)
		{
			throw new ArgumentNullException("newNode");
		}
		XPathNodeType nodeType = NodeType;
		if (nodeType == XPathNodeType.Root || nodeType == XPathNodeType.Attribute || nodeType == XPathNodeType.Namespace)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		XmlWriter xmlWriter = ReplaceRange(this);
		BuildSubtree(newNode, xmlWriter);
		xmlWriter.Close();
	}

	/// <summary>Replaces the current node with the contents of the <see cref="T:System.Xml.XPath.XPathNavigator" /> object specified.</summary>
	/// <param name="newNode">An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the new node.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element, text, processing instruction, or comment node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is not well-formed.</exception>
	public virtual void ReplaceSelf(XPathNavigator newNode)
	{
		if (newNode == null)
		{
			throw new ArgumentNullException("newNode");
		}
		XmlReader newNode2 = newNode.CreateReader();
		ReplaceSelf(newNode2);
	}

	/// <summary>Creates a new child node at the end of the list of child nodes of the current node using the XML data string specified.</summary>
	/// <param name="newChild">The XML data string for the new child node.</param>
	/// <exception cref="T:System.ArgumentNullException">The XML data string parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on is not the root node or an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML data string parameter is not well-formed.</exception>
	public virtual void AppendChild(string newChild)
	{
		XmlReader newChild2 = CreateContextReader(newChild, fromCurrentNode: true);
		AppendChild(newChild2);
	}

	/// <summary>Creates a new child node at the end of the list of child nodes of the current node using the XML contents of the <see cref="T:System.Xml.XmlReader" /> object specified.</summary>
	/// <param name="newChild">An <see cref="T:System.Xml.XmlReader" /> object positioned on the XML data for the new child node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlReader" /> object is in an error state or closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on is not the root node or an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XmlReader" /> object parameter is not well-formed.</exception>
	public virtual void AppendChild(XmlReader newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		XmlWriter xmlWriter = AppendChild();
		BuildSubtree(newChild, xmlWriter);
		xmlWriter.Close();
	}

	/// <summary>Creates a new child node at the end of the list of child nodes of the current node using the nodes in the <see cref="T:System.Xml.XPath.XPathNavigator" /> specified.</summary>
	/// <param name="newChild">An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the node to add as the new child node.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on is not the root node or an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void AppendChild(XPathNavigator newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (!IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		XmlReader newChild2 = newChild.CreateReader();
		AppendChild(newChild2);
	}

	/// <summary>Creates a new child node at the beginning of the list of child nodes of the current node using the XML string specified.</summary>
	/// <param name="newChild">The XML data string for the new child node.</param>
	/// <exception cref="T:System.ArgumentNullException">The XML string parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on does not allow a new child node to be prepended.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML string parameter is not well-formed.</exception>
	public virtual void PrependChild(string newChild)
	{
		XmlReader newChild2 = CreateContextReader(newChild, fromCurrentNode: true);
		PrependChild(newChild2);
	}

	/// <summary>Creates a new child node at the beginning of the list of child nodes of the current node using the XML contents of the <see cref="T:System.Xml.XmlReader" /> object specified.</summary>
	/// <param name="newChild">An <see cref="T:System.Xml.XmlReader" /> object positioned on the XML data for the new child node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlReader" /> object is in an error state or closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on does not allow a new child node to be prepended.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XmlReader" /> object parameter is not well-formed.</exception>
	public virtual void PrependChild(XmlReader newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		XmlWriter xmlWriter = PrependChild();
		BuildSubtree(newChild, xmlWriter);
		xmlWriter.Close();
	}

	/// <summary>Creates a new child node at the beginning of the list of child nodes of the current node using the nodes in the <see cref="T:System.Xml.XPath.XPathNavigator" /> object specified.</summary>
	/// <param name="newChild">An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the node to add as the new child node.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on does not allow a new child node to be prepended.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void PrependChild(XPathNavigator newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (!IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		XmlReader newChild2 = newChild.CreateReader();
		PrependChild(newChild2);
	}

	/// <summary>Creates a new sibling node before the currently selected node using the XML string specified.</summary>
	/// <param name="newSibling">The XML data string for the new sibling node.</param>
	/// <exception cref="T:System.ArgumentNullException">The XML string parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted before the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML string parameter is not well-formed.</exception>
	public virtual void InsertBefore(string newSibling)
	{
		XmlReader newSibling2 = CreateContextReader(newSibling, fromCurrentNode: false);
		InsertBefore(newSibling2);
	}

	/// <summary>Creates a new sibling node before the currently selected node using the XML contents of the <see cref="T:System.Xml.XmlReader" /> object specified.</summary>
	/// <param name="newSibling">An <see cref="T:System.Xml.XmlReader" /> object positioned on the XML data for the new sibling node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlReader" /> object is in an error state or closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted before the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XmlReader" /> object parameter is not well-formed.</exception>
	public virtual void InsertBefore(XmlReader newSibling)
	{
		if (newSibling == null)
		{
			throw new ArgumentNullException("newSibling");
		}
		XmlWriter xmlWriter = InsertBefore();
		BuildSubtree(newSibling, xmlWriter);
		xmlWriter.Close();
	}

	/// <summary>Creates a new sibling node before the currently selected node using the nodes in the <see cref="T:System.Xml.XPath.XPathNavigator" /> specified.</summary>
	/// <param name="newSibling">An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the node to add as the new sibling node.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted before the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void InsertBefore(XPathNavigator newSibling)
	{
		if (newSibling == null)
		{
			throw new ArgumentNullException("newSibling");
		}
		if (!IsValidSiblingType(newSibling.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		XmlReader newSibling2 = newSibling.CreateReader();
		InsertBefore(newSibling2);
	}

	/// <summary>Creates a new sibling node after the currently selected node using the XML string specified.</summary>
	/// <param name="newSibling">The XML data string for the new sibling node.</param>
	/// <exception cref="T:System.ArgumentNullException">The XML string parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted after the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML string parameter is not well-formed.</exception>
	public virtual void InsertAfter(string newSibling)
	{
		XmlReader newSibling2 = CreateContextReader(newSibling, fromCurrentNode: false);
		InsertAfter(newSibling2);
	}

	/// <summary>Creates a new sibling node after the currently selected node using the XML contents of the <see cref="T:System.Xml.XmlReader" /> object specified.</summary>
	/// <param name="newSibling">An <see cref="T:System.Xml.XmlReader" /> object positioned on the XML data for the new sibling node.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlReader" /> object is in an error state or closed.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted after the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML contents of the <see cref="T:System.Xml.XmlReader" /> object parameter is not well-formed.</exception>
	public virtual void InsertAfter(XmlReader newSibling)
	{
		if (newSibling == null)
		{
			throw new ArgumentNullException("newSibling");
		}
		XmlWriter xmlWriter = InsertAfter();
		BuildSubtree(newSibling, xmlWriter);
		xmlWriter.Close();
	}

	/// <summary>Creates a new sibling node after the currently selected node using the nodes in the <see cref="T:System.Xml.XPath.XPathNavigator" /> object specified.</summary>
	/// <param name="newSibling">An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the node to add as the new sibling node.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> object parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted after the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void InsertAfter(XPathNavigator newSibling)
	{
		if (newSibling == null)
		{
			throw new ArgumentNullException("newSibling");
		}
		if (!IsValidSiblingType(newSibling.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		XmlReader newSibling2 = newSibling.CreateReader();
		InsertAfter(newSibling2);
	}

	/// <summary>Deletes a range of sibling nodes from the current node to the node specified.</summary>
	/// <param name="lastSiblingToDelete">An <see cref="T:System.Xml.XPath.XPathNavigator" /> positioned on the last sibling node in the range to delete.</param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> specified is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	/// <exception cref="T:System.InvalidOperationException">The last node to delete specified is not a valid sibling node of the current node.</exception>
	public virtual void DeleteRange(XPathNavigator lastSiblingToDelete)
	{
		throw new NotSupportedException();
	}

	/// <summary>Deletes the current node and its child nodes.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on a node that cannot be deleted such as the root node or a namespace node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void DeleteSelf()
	{
		DeleteRange(this);
	}

	/// <summary>Creates a new child element at the beginning of the list of child nodes of the current node using the namespace prefix, local name, and namespace URI specified with the value specified.</summary>
	/// <param name="prefix">The namespace prefix of the new child element (if any).</param>
	/// <param name="localName">The local name of the new child element (if any).</param>
	/// <param name="namespaceURI">The namespace URI of the new child element (if any). <see cref="F:System.String.Empty" /> and null are equivalent.</param>
	/// <param name="value">The value of the new child element. If <see cref="F:System.String.Empty" /> or null are passed, an empty element is created.</param>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on does not allow a new child node to be prepended.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void PrependChildElement(string prefix, string localName, string namespaceURI, string value)
	{
		XmlWriter xmlWriter = PrependChild();
		xmlWriter.WriteStartElement(prefix, localName, namespaceURI);
		if (value != null)
		{
			xmlWriter.WriteString(value);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	/// <summary>Creates a new child element node at the end of the list of child nodes of the current node using the namespace prefix, local name and namespace URI specified with the value specified.</summary>
	/// <param name="prefix">The namespace prefix of the new child element node (if any).</param>
	/// <param name="localName">The local name of the new child element node (if any).</param>
	/// <param name="namespaceURI">The namespace URI of the new child element node (if any). <see cref="F:System.String.Empty" /> and null are equivalent.</param>
	/// <param name="value">The value of the new child element node. If <see cref="F:System.String.Empty" /> or null are passed, an empty element is created.</param>
	/// <exception cref="T:System.InvalidOperationException">The current node the <see cref="T:System.Xml.XPath.XPathNavigator" /> is positioned on is not the root node or an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void AppendChildElement(string prefix, string localName, string namespaceURI, string value)
	{
		XmlWriter xmlWriter = AppendChild();
		xmlWriter.WriteStartElement(prefix, localName, namespaceURI);
		if (value != null)
		{
			xmlWriter.WriteString(value);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	/// <summary>Creates a new sibling element before the current node using the namespace prefix, local name, and namespace URI specified, with the value specified.</summary>
	/// <param name="prefix">The namespace prefix of the new child element (if any).</param>
	/// <param name="localName">The local name of the new child element (if any).</param>
	/// <param name="namespaceURI">The namespace URI of the new child element (if any). <see cref="F:System.String.Empty" /> and null are equivalent.</param>
	/// <param name="value">The value of the new child element. If <see cref="F:System.String.Empty" /> or null are passed, an empty element is created.</param>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted before the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void InsertElementBefore(string prefix, string localName, string namespaceURI, string value)
	{
		XmlWriter xmlWriter = InsertBefore();
		xmlWriter.WriteStartElement(prefix, localName, namespaceURI);
		if (value != null)
		{
			xmlWriter.WriteString(value);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	/// <summary>Creates a new sibling element after the current node using the namespace prefix, local name and namespace URI specified, with the value specified.</summary>
	/// <param name="prefix">The namespace prefix of the new child element (if any).</param>
	/// <param name="localName">The local name of the new child element (if any).</param>
	/// <param name="namespaceURI">The namespace URI of the new child element (if any). <see cref="F:System.String.Empty" /> and null are equivalent.</param>
	/// <param name="value">The value of the new child element. If <see cref="F:System.String.Empty" /> or null are passed, an empty element is created.</param>
	/// <exception cref="T:System.InvalidOperationException">The position of the <see cref="T:System.Xml.XPath.XPathNavigator" /> does not allow a new sibling node to be inserted after the current node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void InsertElementAfter(string prefix, string localName, string namespaceURI, string value)
	{
		XmlWriter xmlWriter = InsertAfter();
		xmlWriter.WriteStartElement(prefix, localName, namespaceURI);
		if (value != null)
		{
			xmlWriter.WriteString(value);
		}
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	/// <summary>Creates an attribute node on the current element node using the namespace prefix, local name and namespace URI specified with the value specified.</summary>
	/// <param name="prefix">The namespace prefix of the new attribute node (if any).</param>
	/// <param name="localName">The local name of the new attribute node which cannot <see cref="F:System.String.Empty" /> or null.</param>
	/// <param name="namespaceURI">The namespace URI for the new attribute node (if any).</param>
	/// <param name="value">The value of the new attribute node. If <see cref="F:System.String.Empty" /> or null are passed, an empty attribute node is created.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> is not positioned on an element node.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XPath.XPathNavigator" /> does not support editing.</exception>
	public virtual void CreateAttribute(string prefix, string localName, string namespaceURI, string value)
	{
		XmlWriter xmlWriter = CreateAttributes();
		xmlWriter.WriteStartAttribute(prefix, localName, namespaceURI);
		if (value != null)
		{
			xmlWriter.WriteString(value);
		}
		xmlWriter.WriteEndAttribute();
		xmlWriter.Close();
	}

	internal bool MoveToPrevious(string localName, string namespaceURI)
	{
		XPathNavigator other = Clone();
		localName = ((localName != null) ? NameTable.Get(localName) : null);
		while (MoveToPrevious())
		{
			if (NodeType == XPathNodeType.Element && (object)localName == LocalName && namespaceURI == NamespaceURI)
			{
				return true;
			}
		}
		MoveTo(other);
		return false;
	}

	internal bool MoveToPrevious(XPathNodeType type)
	{
		XPathNavigator other = Clone();
		int contentKindMask = GetContentKindMask(type);
		while (MoveToPrevious())
		{
			if (((1 << (int)NodeType) & contentKindMask) != 0)
			{
				return true;
			}
		}
		MoveTo(other);
		return false;
	}

	internal bool MoveToNonDescendant()
	{
		if (NodeType == XPathNodeType.Root)
		{
			return false;
		}
		if (MoveToNext())
		{
			return true;
		}
		XPathNavigator xPathNavigator = Clone();
		if (!MoveToParent())
		{
			return false;
		}
		XPathNodeType nodeType = xPathNavigator.NodeType;
		if ((uint)(nodeType - 2) <= 1u && MoveToFirstChild())
		{
			return true;
		}
		while (!MoveToNext())
		{
			if (!MoveToParent())
			{
				MoveTo(xPathNavigator);
				return false;
			}
		}
		return true;
	}

	private static XPathExpression CompileMatchPattern(string xpath)
	{
		bool needContext;
		return new CompiledXpathExpr(new QueryBuilder().BuildPatternQuery(xpath, out needContext), xpath, needContext);
	}

	private static int GetDepth(XPathNavigator nav)
	{
		int num = 0;
		while (nav.MoveToParent())
		{
			num++;
		}
		return num;
	}

	private XmlNodeOrder CompareSiblings(XPathNavigator n1, XPathNavigator n2)
	{
		int num = 0;
		switch (n1.NodeType)
		{
		case XPathNodeType.Attribute:
			num++;
			break;
		default:
			num += 2;
			break;
		case XPathNodeType.Namespace:
			break;
		}
		switch (n2.NodeType)
		{
		case XPathNodeType.Namespace:
			if (num != 0)
			{
				break;
			}
			while (n1.MoveToNextNamespace())
			{
				if (n1.IsSamePosition(n2))
				{
					return XmlNodeOrder.Before;
				}
			}
			break;
		case XPathNodeType.Attribute:
			num--;
			if (num != 0)
			{
				break;
			}
			while (n1.MoveToNextAttribute())
			{
				if (n1.IsSamePosition(n2))
				{
					return XmlNodeOrder.Before;
				}
			}
			break;
		default:
			num -= 2;
			if (num != 0)
			{
				break;
			}
			while (n1.MoveToNext())
			{
				if (n1.IsSamePosition(n2))
				{
					return XmlNodeOrder.Before;
				}
			}
			break;
		}
		if (num >= 0)
		{
			return XmlNodeOrder.After;
		}
		return XmlNodeOrder.Before;
	}

	internal static XmlNamespaceManager GetNamespaces(IXmlNamespaceResolver resolver)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
		foreach (KeyValuePair<string, string> item in resolver.GetNamespacesInScope(XmlNamespaceScope.All))
		{
			if (item.Key != "xmlns")
			{
				xmlNamespaceManager.AddNamespace(item.Key, item.Value);
			}
		}
		return xmlNamespaceManager;
	}

	internal static int GetContentKindMask(XPathNodeType type)
	{
		return ContentKindMasks[(int)type];
	}

	internal static int GetKindMask(XPathNodeType type)
	{
		return type switch
		{
			XPathNodeType.All => int.MaxValue, 
			XPathNodeType.Text => 112, 
			_ => 1 << (int)type, 
		};
	}

	internal static bool IsText(XPathNodeType type)
	{
		return (uint)(type - 4) <= 2u;
	}

	private bool IsValidChildType(XPathNodeType type)
	{
		switch (NodeType)
		{
		case XPathNodeType.Root:
			if (type == XPathNodeType.Element || (uint)(type - 5) <= 3u)
			{
				return true;
			}
			break;
		case XPathNodeType.Element:
			if (type == XPathNodeType.Element || (uint)(type - 4) <= 4u)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private bool IsValidSiblingType(XPathNodeType type)
	{
		XPathNodeType nodeType = NodeType;
		if ((nodeType == XPathNodeType.Element || (uint)(nodeType - 4) <= 4u) && (type == XPathNodeType.Element || (uint)(type - 4) <= 4u))
		{
			return true;
		}
		return false;
	}

	private XmlReader CreateReader()
	{
		return XPathNavigatorReader.Create(this);
	}

	private XmlReader CreateContextReader(string xml, bool fromCurrentNode)
	{
		if (xml == null)
		{
			throw new ArgumentNullException("xml");
		}
		XPathNavigator xPathNavigator = CreateNavigator();
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(NameTable);
		if (!fromCurrentNode)
		{
			xPathNavigator.MoveToParent();
		}
		if (xPathNavigator.MoveToFirstNamespace(XPathNamespaceScope.All))
		{
			do
			{
				xmlNamespaceManager.AddNamespace(xPathNavigator.LocalName, xPathNavigator.Value);
			}
			while (xPathNavigator.MoveToNextNamespace(XPathNamespaceScope.All));
		}
		XmlParserContext context = new XmlParserContext(NameTable, xmlNamespaceManager, null, XmlSpace.Default);
		return new XmlTextReader(xml, XmlNodeType.Element, context)
		{
			WhitespaceHandling = WhitespaceHandling.Significant
		};
	}

	internal void BuildSubtree(XmlReader reader, XmlWriter writer)
	{
		string text = "http://www.w3.org/2000/xmlns/";
		ReadState readState = reader.ReadState;
		if (readState != 0 && readState != ReadState.Interactive)
		{
			throw new ArgumentException(Res.GetString("Operation is not valid due to the current state of the object."), "reader");
		}
		int num = 0;
		if (readState == ReadState.Initial)
		{
			if (!reader.Read())
			{
				return;
			}
			num++;
		}
		do
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
			{
				writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
				bool isEmptyElement = reader.IsEmptyElement;
				while (reader.MoveToNextAttribute())
				{
					if ((object)reader.NamespaceURI == text)
					{
						if (reader.Prefix.Length == 0)
						{
							writer.WriteAttributeString("", "xmlns", text, reader.Value);
						}
						else
						{
							writer.WriteAttributeString("xmlns", reader.LocalName, text, reader.Value);
						}
					}
					else
					{
						writer.WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
						writer.WriteString(reader.Value);
						writer.WriteEndAttribute();
					}
				}
				reader.MoveToElement();
				if (isEmptyElement)
				{
					writer.WriteEndElement();
				}
				else
				{
					num++;
				}
				break;
			}
			case XmlNodeType.EndElement:
				writer.WriteFullEndElement();
				num--;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
				writer.WriteString(reader.Value);
				break;
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				writer.WriteString(reader.Value);
				break;
			case XmlNodeType.Comment:
				writer.WriteComment(reader.Value);
				break;
			case XmlNodeType.ProcessingInstruction:
				writer.WriteProcessingInstruction(reader.LocalName, reader.Value);
				break;
			case XmlNodeType.EntityReference:
				reader.ResolveEntity();
				break;
			case XmlNodeType.Attribute:
				if ((object)reader.NamespaceURI == text)
				{
					if (reader.Prefix.Length == 0)
					{
						writer.WriteAttributeString("", "xmlns", text, reader.Value);
					}
					else
					{
						writer.WriteAttributeString("xmlns", reader.LocalName, text, reader.Value);
					}
				}
				else
				{
					writer.WriteStartAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI);
					writer.WriteString(reader.Value);
					writer.WriteEndAttribute();
				}
				break;
			}
		}
		while (reader.Read() && num > 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathNavigator" /> class.</summary>
	protected XPathNavigator()
	{
	}
}

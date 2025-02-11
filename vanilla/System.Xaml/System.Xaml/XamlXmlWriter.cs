using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

/// <summary>Uses a <see cref="T:System.IO.TextWriter" /> or <see cref="T:System.Xml.XmlWriter" /> support class to write a XAML node stream to a text or markup serialized form.</summary>
public class XamlXmlWriter : XamlWriter
{
	private class Frame
	{
		private Dictionary<string, string> namespaceMap = new Dictionary<string, string>();

		private Dictionary<string, string> prefixMap = new Dictionary<string, string>();

		public XamlType Type { get; set; }

		public XamlMember Member { get; set; }

		public XamlPropertySet Members { get; set; }

		public XamlNodeType AllocatingNodeType { get; set; }

		public bool IsObjectFromMember { get; set; }

		public bool IsContent { get; set; }

		public bool TryLookupPrefix(string ns, out string prefix)
		{
			if (ns == "http://www.w3.org/XML/1998/namespace")
			{
				prefix = "xml";
				return true;
			}
			return namespaceMap.TryGetValue(ns, out prefix);
		}

		public bool TryLookupNamespace(string prefix, out string ns)
		{
			if (prefix == "xml")
			{
				ns = "http://www.w3.org/XML/1998/namespace";
				return true;
			}
			return prefixMap.TryGetValue(prefix, out ns);
		}

		public void AssignNamespacePrefix(string ns, string prefix)
		{
			if (prefixMap.ContainsKey(prefix))
			{
				throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterPrefixAlreadyDefinedInCurrentScope, prefix));
			}
			if (namespaceMap.ContainsKey(ns))
			{
				throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterNamespaceAlreadyHasPrefixInCurrentScope, ns));
			}
			prefixMap[prefix] = ns;
			namespaceMap[ns] = prefix;
		}

		public bool IsEmpty()
		{
			return namespaceMap.Count == 0;
		}

		public List<KeyValuePair<string, string>> GetSortedPrefixMap()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, string> item in prefixMap)
			{
				list.Add(item);
			}
			list.Sort(CompareByKey);
			return list;
		}

		private static int CompareByKey(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
		{
			return string.Compare(x.Key, y.Key, ignoreCase: false, TypeConverterHelper.InvariantEnglishUS);
		}
	}

	private abstract class WriterState
	{
		public virtual void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteObject"));
		}

		public virtual void WriteEndObject(XamlXmlWriter writer)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndObject"));
		}

		public virtual void WriteStartMember(XamlXmlWriter writer, XamlMember property)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
		}

		public virtual void WriteEndMember(XamlXmlWriter writer)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndMember"));
		}

		public virtual void WriteValue(XamlXmlWriter writer, string value)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteValue"));
		}

		public virtual void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteNamespace"));
		}

		protected static void WriteMemberAsElement(XamlXmlWriter writer)
		{
			Frame frame = writer.namespaceScopes.Peek();
			XamlType type = frame.Type;
			XamlMember member = frame.Member;
			XamlType type2 = (member.IsAttachable ? member.DeclaringType : type);
			string chosenNamespace;
			string prefix = ((member.IsAttachable || member.IsDirective) ? writer.FindPrefix(member.GetXamlNamespaces(), out chosenNamespace) : writer.FindPrefix(type.GetXamlNamespaces(), out chosenNamespace));
			string localName = (member.IsDirective ? member.Name : (GetTypeName(type2) + "." + member.Name));
			writer.output.WriteStartElement(prefix, localName, chosenNamespace);
		}

		protected static void WriteMemberAsAttribute(XamlXmlWriter writer)
		{
			Frame frame = writer.namespaceScopes.Peek();
			XamlType type = frame.Type;
			XamlMember member = frame.Member;
			string name = member.Name;
			if (member.IsDirective)
			{
				string chosenNamespace;
				string prefix = writer.FindPrefix(member.GetXamlNamespaces(), out chosenNamespace);
				WriteStartAttribute(writer, prefix, name, chosenNamespace);
			}
			else if (member.IsAttachable)
			{
				string chosenNamespace2;
				string prefix2 = writer.FindPrefix(member.GetXamlNamespaces(), out chosenNamespace2);
				name = ((!(member.DeclaringType == type)) ? (GetTypeName(member.DeclaringType) + "." + member.Name) : member.Name);
				WriteStartAttribute(writer, prefix2, name, chosenNamespace2);
			}
			else
			{
				writer.output.WriteStartAttribute(name);
			}
		}

		protected static void WriteStartElementForObject(XamlXmlWriter writer, XamlType type)
		{
			string typeName = GetTypeName(type);
			string chosenNamespace;
			string prefix = writer.FindPrefix(type.GetXamlNamespaces(), out chosenNamespace);
			writer.output.WriteStartElement(prefix, typeName, chosenNamespace);
		}

		private static void WriteStartAttribute(XamlXmlWriter writer, string prefix, string local, string ns)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				writer.output.WriteStartAttribute(local);
			}
			else
			{
				writer.output.WriteStartAttribute(prefix, local, ns);
			}
		}

		protected internal void WriteNode(XamlXmlWriter writer, XamlNode node)
		{
			switch (node.NodeType)
			{
			case XamlNodeType.NamespaceDeclaration:
				writer.currentState.WriteNamespace(writer, node.NamespaceDeclaration);
				break;
			case XamlNodeType.StartObject:
				writer.currentState.WriteObject(writer, node.XamlType, isObjectFromMember: false);
				break;
			case XamlNodeType.GetObject:
			{
				XamlType type = null;
				Frame frame = writer.namespaceScopes.Peek();
				if (frame.AllocatingNodeType == XamlNodeType.StartMember)
				{
					type = frame.Member.Type;
				}
				writer.currentState.WriteObject(writer, type, isObjectFromMember: true);
				break;
			}
			case XamlNodeType.EndObject:
				writer.currentState.WriteEndObject(writer);
				break;
			case XamlNodeType.StartMember:
				writer.currentState.WriteStartMember(writer, node.Member);
				break;
			case XamlNodeType.EndMember:
				writer.currentState.WriteEndMember(writer);
				break;
			case XamlNodeType.Value:
				writer.currentState.WriteValue(writer, node.Value as string);
				break;
			default:
				throw new NotSupportedException(System.SR.MissingCaseXamlNodes);
			case XamlNodeType.None:
				break;
			}
		}
	}

	private class Start : WriterState
	{
		private static WriterState state = new Start();

		public static WriterState State => state;

		private Start()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.namespaceScopes.Peek().Type = type;
			writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;
			if (isObjectFromMember)
			{
				throw new XamlXmlWriterException(System.SR.XamlXmlWriterWriteObjectNotSupportedInCurrentState);
			}
			WriterState.WriteStartElementForObject(writer, type);
			writer.currentState = InRecordTryAttributes.State;
		}
	}

	private class End : WriterState
	{
		private static WriterState state = new End();

		public static WriterState State => state;

		private End()
		{
		}
	}

	private class InRecord : WriterState
	{
		private static WriterState state = new InRecord();

		public static WriterState State => state;

		private InRecord()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
			{
				writer.namespaceScopes.Push(new Frame
				{
					AllocatingNodeType = XamlNodeType.StartMember,
					Type = writer.namespaceScopes.Peek().Type
				});
			}
			writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
		}

		public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
		{
			writer.CheckMemberForUniqueness(property);
			if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
			{
				writer.namespaceScopes.Push(new Frame
				{
					AllocatingNodeType = XamlNodeType.StartMember,
					Type = writer.namespaceScopes.Peek().Type
				});
			}
			writer.namespaceScopes.Peek().Member = property;
			XamlType type = writer.namespaceScopes.Peek().Type;
			if ((property == XamlLanguage.Items && type != null && type.IsWhitespaceSignificantCollection) || property == XamlLanguage.UnknownContent)
			{
				writer.isFirstElementOfWhitespaceSignificantCollection = true;
			}
			XamlType type2 = writer.namespaceScopes.Peek().Type;
			if (IsImplicit(property))
			{
				if (!writer.namespaceScopes.Peek().IsEmpty())
				{
					throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
				}
			}
			else
			{
				if (property == type2.ContentProperty)
				{
					if (!writer.namespaceScopes.Peek().IsEmpty())
					{
						throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteStartMember"));
					}
					writer.currentState = TryContentProperty.State;
					return;
				}
				WriterState.WriteMemberAsElement(writer);
				writer.WriteDeferredNamespaces(XamlNodeType.StartMember);
			}
			if (property == XamlLanguage.PositionalParameters)
			{
				writer.namespaceScopes.Pop();
				if (type2 != null && type2.ConstructionRequiresArguments)
				{
					throw new XamlXmlWriterException(System.SR.ExpandPositionalParametersinTypeWithNoDefaultConstructor);
				}
				writer.ppStateInfo.ReturnState = State;
				writer.currentState = ExpandPositionalParameters.State;
			}
			else
			{
				writer.currentState = InMember.State;
			}
		}

		public override void WriteEndObject(XamlXmlWriter writer)
		{
			Frame frame = writer.namespaceScopes.Pop();
			if (frame.AllocatingNodeType != XamlNodeType.StartObject && frame.AllocatingNodeType != XamlNodeType.GetObject)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteEndObject"));
			}
			if (!frame.IsObjectFromMember)
			{
				writer.output.WriteEndElement();
			}
			if (writer.namespaceScopes.Count > 0)
			{
				writer.currentState = InMemberAfterEndObject.State;
				return;
			}
			writer.Flush();
			writer.currentState = End.State;
		}
	}

	private class InRecordTryAttributes : WriterState
	{
		private static WriterState state = new InRecordTryAttributes();

		public static WriterState State => state;

		private InRecordTryAttributes()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.currentState = InRecord.State;
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
		{
			XamlType type = writer.namespaceScopes.Peek().Type;
			if ((property == XamlLanguage.Items && type != null && type.IsWhitespaceSignificantCollection) || property == XamlLanguage.UnknownContent)
			{
				writer.isFirstElementOfWhitespaceSignificantCollection = true;
			}
			if (property.IsAttachable || property.IsDirective)
			{
				string chosenNamespace;
				string text = writer.LookupPrefix(property.GetXamlNamespaces(), out chosenNamespace);
				if (text == null || writer.IsShadowed(chosenNamespace, text))
				{
					writer.currentState = InRecord.State;
					writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
					writer.currentState.WriteStartMember(writer, property);
					return;
				}
			}
			writer.CheckMemberForUniqueness(property);
			writer.namespaceScopes.Push(new Frame
			{
				AllocatingNodeType = XamlNodeType.StartMember,
				Type = writer.namespaceScopes.Peek().Type,
				Member = property
			});
			XamlType type2 = writer.namespaceScopes.Peek().Type;
			if (property == XamlLanguage.PositionalParameters)
			{
				writer.namespaceScopes.Pop();
				if (type2 != null && type2.ConstructionRequiresArguments)
				{
					throw new XamlXmlWriterException(System.SR.ExpandPositionalParametersinTypeWithNoDefaultConstructor);
				}
				writer.ppStateInfo.ReturnState = State;
				writer.currentState = ExpandPositionalParameters.State;
			}
			else if (IsImplicit(property))
			{
				writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
				writer.currentState = InMember.State;
			}
			else if (property == type2.ContentProperty)
			{
				writer.currentState = TryContentPropertyInTryAttributesState.State;
			}
			else if (property.IsDirective && property.Type != null && (property.Type.IsCollection || property.Type.IsDictionary))
			{
				writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
				WriterState.WriteMemberAsElement(writer);
				writer.currentState = InMember.State;
			}
			else
			{
				writer.currentState = InMemberTryAttributes.State;
			}
		}

		public override void WriteEndObject(XamlXmlWriter writer)
		{
			writer.currentState = InRecord.State;
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			writer.currentState.WriteEndObject(writer);
		}
	}

	private class InMember : WriterState
	{
		private static WriterState state = new InMember();

		public static WriterState State => state;

		private InMember()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject && writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject)
			{
				writer.namespaceScopes.Push(new Frame
				{
					AllocatingNodeType = XamlNodeType.StartObject
				});
			}
			writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			Frame frame = writer.namespaceScopes.Peek();
			if (frame.AllocatingNodeType != XamlNodeType.StartMember)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteValue"));
			}
			if (frame.Member.DeclaringType == XamlLanguage.XData)
			{
				writer.output.WriteRaw(value);
				writer.currentState = InMemberAfterValue.State;
			}
			else if (HasSignificantWhitespace(value))
			{
				XamlType containingXamlType = GetContainingXamlType(writer);
				if (containingXamlType != null && !containingXamlType.IsWhitespaceSignificantCollection)
				{
					WriteXmlSpaceOrThrow(writer, value);
					writer.output.WriteValue(value);
					writer.currentState = InMemberAfterValue.State;
				}
				else if (ContainsConsecutiveInnerSpaces(value) || ContainsWhitespaceThatIsNotSpace(value))
				{
					if (!writer.isFirstElementOfWhitespaceSignificantCollection)
					{
						throw new InvalidOperationException(System.SR.Format(System.SR.WhiteSpaceInCollection, value, containingXamlType.Name));
					}
					WriteXmlSpaceOrThrow(writer, value);
					writer.output.WriteValue(value);
					writer.currentState = InMemberAfterValue.State;
				}
				else
				{
					if (ContainsLeadingSpace(value) && writer.isFirstElementOfWhitespaceSignificantCollection)
					{
						WriteXmlSpaceOrThrow(writer, value);
						writer.output.WriteValue(value);
						writer.currentState = InMemberAfterValue.State;
					}
					if (ContainsTrailingSpace(value))
					{
						writer.deferredValue = value;
						writer.currentState = InMemberAfterValueWithSignificantWhitespace.State;
					}
					else
					{
						writer.output.WriteValue(value);
						writer.currentState = InMemberAfterValue.State;
					}
				}
			}
			else
			{
				writer.output.WriteValue(value);
				writer.currentState = InMemberAfterValue.State;
			}
			if (writer.currentState != InMemberAfterValueWithSignificantWhitespace.State)
			{
				writer.isFirstElementOfWhitespaceSignificantCollection = false;
			}
		}

		private void WriteXmlSpaceOrThrow(XamlXmlWriter writer, string value)
		{
			Frame frame = FindFrameWithXmlSpacePreserve(writer);
			if (frame.AllocatingNodeType == XamlNodeType.StartMember)
			{
				throw new XamlXmlWriterException(System.SR.Format(System.SR.CannotWriteXmlSpacePreserveOnMember, frame.Member, value));
			}
			WriteXmlSpace(writer);
		}

		private Frame FindFrameWithXmlSpacePreserve(XamlXmlWriter writer)
		{
			Stack<Frame>.Enumerator enumerator = writer.namespaceScopes.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Frame current = enumerator.Current;
				if (current.AllocatingNodeType != XamlNodeType.GetObject && (current.AllocatingNodeType != XamlNodeType.StartMember || (!current.IsContent && !IsImplicit(current.Member))))
				{
					break;
				}
			}
			return enumerator.Current;
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject && writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject)
			{
				writer.namespaceScopes.Push(new Frame
				{
					AllocatingNodeType = ((!isObjectFromMember) ? XamlNodeType.StartObject : XamlNodeType.GetObject)
				});
			}
			writer.namespaceScopes.Peek().Type = type;
			writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;
			writer.isFirstElementOfWhitespaceSignificantCollection = false;
			if (isObjectFromMember)
			{
				if (!writer.namespaceScopes.Peek().IsEmpty())
				{
					throw new InvalidOperationException(System.SR.XamlXmlWriterWriteObjectNotSupportedInCurrentState);
				}
				Frame item = writer.namespaceScopes.Pop();
				Frame frame = writer.namespaceScopes.Peek();
				writer.namespaceScopes.Push(item);
				if (frame.AllocatingNodeType == XamlNodeType.StartMember)
				{
					XamlType type2 = frame.Member.Type;
					if (type2 != null && !type2.IsCollection && !type2.IsDictionary)
					{
						throw new InvalidOperationException(System.SR.XamlXmlWriterIsObjectFromMemberSetForArraysOrNonCollections);
					}
				}
				writer.currentState = InRecord.State;
			}
			else
			{
				WriterState.WriteStartElementForObject(writer, type);
				writer.currentState = InRecordTryAttributes.State;
			}
		}
	}

	private class InMemberAfterValue : WriterState
	{
		private static WriterState state = new InMemberAfterValue();

		public static WriterState State => state;

		private InMemberAfterValue()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			Frame frame = writer.namespaceScopes.Pop();
			if (!IsImplicit(frame.Member) && !frame.IsContent)
			{
				writer.output.WriteEndElement();
			}
			writer.currentState = InRecord.State;
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.currentState = InMember.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class InMemberAfterValueWithSignificantWhitespace : WriterState
	{
		private static WriterState state = new InMemberAfterValueWithSignificantWhitespace();

		public static WriterState State => state;

		private InMemberAfterValueWithSignificantWhitespace()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.currentState = InMemberAfterValue.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			if (writer.isFirstElementOfWhitespaceSignificantCollection)
			{
				WriteXmlSpace(writer);
				writer.output.WriteValue(writer.deferredValue);
				writer.currentState = InMemberAfterValue.State;
				writer.currentState.WriteEndMember(writer);
				writer.isFirstElementOfWhitespaceSignificantCollection = false;
				return;
			}
			XamlType containingXamlType = GetContainingXamlType(writer);
			throw new InvalidOperationException(System.SR.Format(System.SR.WhiteSpaceInCollection, writer.deferredValue, containingXamlType.Name));
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.output.WriteValue(writer.deferredValue);
			writer.currentState = InMemberAfterValue.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class InMemberAfterEndObject : WriterState
	{
		private static WriterState state = new InMemberAfterEndObject();

		public static WriterState State => state;

		private InMemberAfterEndObject()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			writer.currentState = InMember.State;
			writer.currentState.WriteValue(writer, value);
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			writer.currentState = InMemberAfterValue.State;
			writer.currentState.WriteEndMember(writer);
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.currentState = InMember.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class InMemberAttributedMember : WriterState
	{
		private static WriterState state = new InMemberAttributedMember();

		public static WriterState State => state;

		private InMemberAttributedMember()
		{
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			WriterState.WriteMemberAsAttribute(writer);
			if (!writer.deferredValueIsME && StringStartsWithCurly(writer.deferredValue))
			{
				writer.output.WriteValue("{}" + writer.deferredValue);
			}
			else
			{
				writer.output.WriteValue(writer.deferredValue);
			}
			writer.namespaceScopes.Pop();
			writer.output.WriteEndAttribute();
			writer.currentState = InRecordTryAttributes.State;
		}
	}

	private class InMemberTryAttributes : WriterState
	{
		private static WriterState state = new InMemberTryAttributes();

		public static WriterState State => state;

		private InMemberTryAttributes()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			WriterState.WriteMemberAsElement(writer);
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			writer.deferredValue = value;
			writer.deferredValueIsME = false;
			writer.currentState = InMemberTryAttributesAfterValue.State;
			writer.isFirstElementOfWhitespaceSignificantCollection = false;
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			if (type != null && type.IsMarkupExtension && !type.IsGeneric)
			{
				writer.meWriter.Reset();
				writer.meNodesStack.Push(new List<XamlNode>());
				writer.currentState = TryCurlyForm.State;
				writer.currentState.WriteObject(writer, type, isObjectFromMember);
			}
			else
			{
				writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
				WriterState.WriteMemberAsElement(writer);
				writer.currentState = InMember.State;
				writer.currentState.WriteObject(writer, type, isObjectFromMember);
			}
			writer.isFirstElementOfWhitespaceSignificantCollection = false;
		}
	}

	private class InMemberTryAttributesAfterValue : WriterState
	{
		private static WriterState state = new InMemberTryAttributesAfterValue();

		public static WriterState State => state;

		private InMemberTryAttributesAfterValue()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			WriterState.WriteMemberAsElement(writer);
			writer.output.WriteValue(writer.deferredValue);
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			writer.currentState = InMemberAttributedMember.State;
			writer.currentState.WriteEndMember(writer);
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			WriterState.WriteMemberAsElement(writer);
			writer.output.WriteValue(writer.deferredValue);
			writer.isFirstElementOfWhitespaceSignificantCollection = false;
			writer.currentState = InMember.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class TryContentProperty : WriterState
	{
		private static WriterState state = new TryContentProperty();

		public static WriterState State => state;

		private TryContentProperty()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.namespaceScopes.Peek().IsContent = true;
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			XamlMember member = writer.namespaceScopes.Peek().Member;
			if (XamlLanguage.String.CanAssignTo(member.Type))
			{
				writer.namespaceScopes.Peek().IsContent = true;
			}
			else
			{
				writer.namespaceScopes.Peek().IsContent = false;
				WriterState.WriteMemberAsElement(writer);
			}
			writer.currentState = InMember.State;
			writer.currentState.WriteValue(writer, value);
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.namespaceScopes.Peek().IsContent = true;
			writer.currentState = InMember.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class TryContentPropertyInTryAttributesState : WriterState
	{
		private static WriterState state = new TryContentPropertyInTryAttributesState();

		public static WriterState State => state;

		private TryContentPropertyInTryAttributesState()
		{
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.namespaceScopes.Peek().IsContent = true;
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			writer.currentState = InMember.State;
			writer.currentState.WriteNamespace(writer, namespaceDeclaration);
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			XamlMember member = writer.namespaceScopes.Peek().Member;
			if (XamlLanguage.String.CanAssignTo(member.Type) && !string.IsNullOrEmpty(value))
			{
				writer.namespaceScopes.Peek().IsContent = true;
				writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
				writer.currentState = InMember.State;
				writer.currentState.WriteValue(writer, value);
			}
			else
			{
				writer.namespaceScopes.Peek().IsContent = false;
				writer.currentState = InMemberTryAttributes.State;
				writer.currentState.WriteValue(writer, value);
			}
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			writer.namespaceScopes.Peek().IsContent = true;
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			writer.currentState = InMember.State;
			writer.currentState.WriteObject(writer, type, isObjectFromMember);
		}
	}

	private class TryCurlyForm : WriterState
	{
		private static WriterState state = new TryCurlyForm();

		public static WriterState State => state;

		private TryCurlyForm()
		{
		}

		private void WriteNodesInXmlForm(XamlXmlWriter writer)
		{
			writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
			WriterState.WriteMemberAsElement(writer);
			writer.currentState = InMember.State;
			foreach (XamlNode item in writer.meNodesStack.Pop())
			{
				writer.currentState.WriteNode(writer, item);
			}
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			if (!isObjectFromMember)
			{
				writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartObject, type));
				writer.meWriter.WriteStartObject(type);
			}
			else
			{
				writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.GetObject));
				writer.meWriter.WriteGetObject();
			}
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
		}

		public override void WriteEndObject(XamlXmlWriter writer)
		{
			writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndObject));
			writer.meWriter.WriteEndObject();
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
			if (writer.meWriter.MarkupExtensionString != null)
			{
				writer.meNodesStack.Pop();
				writer.deferredValue = writer.meWriter.MarkupExtensionString;
				writer.deferredValueIsME = true;
				writer.currentState = InMemberTryAttributesAfterValue.State;
			}
		}

		public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
		{
			writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartMember, property));
			writer.meWriter.WriteStartMember(property);
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndMember));
			writer.meWriter.WriteEndMember();
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
		}

		public override void WriteNamespace(XamlXmlWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.NamespaceDeclaration, namespaceDeclaration));
			writer.meWriter.WriteNamespace(namespaceDeclaration);
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.Value, value));
			writer.meWriter.WriteValue(value);
			if (writer.meWriter.Failed)
			{
				WriteNodesInXmlForm(writer);
			}
		}
	}

	private class ExpandPositionalParameters : WriterState
	{
		private static WriterState state = new ExpandPositionalParameters();

		public static WriterState State => state;

		private ExpandPositionalParameters()
		{
		}

		private void ExpandPositionalParametersIntoProperties(XamlXmlWriter writer)
		{
			XamlType type = writer.namespaceScopes.Peek().Type;
			if (type.UnderlyingType == null)
			{
				throw new XamlXmlWriterException(System.SR.Format(System.SR.ExpandPositionalParametersWithoutUnderlyingType, type.GetQualifiedName()));
			}
			int count = writer.ppStateInfo.NodesList.Count;
			ParameterInfo[] parametersInfo = GetParametersInfo(type, count);
			List<XamlMember> allPropertiesWithCAA = GetAllPropertiesWithCAA(type);
			if (parametersInfo.Length != allPropertiesWithCAA.Count)
			{
				throw new XamlXmlWriterException(System.SR.ConstructorNotFoundForGivenPositionalParameters);
			}
			for (int i = 0; i < parametersInfo.Length; i++)
			{
				ParameterInfo parameterInfo = parametersInfo[i];
				XamlMember xamlMember = null;
				foreach (XamlMember item in allPropertiesWithCAA)
				{
					if (item.Type.UnderlyingType == parameterInfo.ParameterType && XamlObjectReader.GetConstructorArgument(item) == parameterInfo.Name)
					{
						xamlMember = item;
						break;
					}
				}
				if (xamlMember == null)
				{
					throw new XamlXmlWriterException(System.SR.ConstructorNotFoundForGivenPositionalParameters);
				}
				XamlMember member = type.GetMember(xamlMember.Name);
				if (member.IsReadOnly)
				{
					throw new XamlXmlWriterException(System.SR.ExpandPositionalParametersWithReadOnlyProperties);
				}
				writer.ppStateInfo.NodesList[i].Insert(0, new XamlNode(XamlNodeType.StartMember, member));
				writer.ppStateInfo.NodesList[i].Add(new XamlNode(XamlNodeType.EndMember));
			}
		}

		private ParameterInfo[] GetParametersInfo(XamlType objectXamlType, int numOfParameters)
		{
			IList<XamlType> obj = objectXamlType.GetPositionalParameters(numOfParameters) ?? throw new XamlXmlWriterException(System.SR.ConstructorNotFoundForGivenPositionalParameters);
			Type[] array = new Type[numOfParameters];
			int num = 0;
			foreach (XamlType item in obj)
			{
				Type underlyingType = item.UnderlyingType;
				if (underlyingType != null)
				{
					array[num++] = underlyingType;
					continue;
				}
				throw new XamlXmlWriterException(System.SR.ConstructorNotFoundForGivenPositionalParameters);
			}
			ConstructorInfo constructor = objectXamlType.GetConstructor(array);
			if (constructor == null)
			{
				throw new XamlXmlWriterException(System.SR.ConstructorNotFoundForGivenPositionalParameters);
			}
			return constructor.GetParameters();
		}

		private List<XamlMember> GetAllPropertiesWithCAA(XamlType objectXamlType)
		{
			ICollection<XamlMember> allMembers = objectXamlType.GetAllMembers();
			ICollection<XamlMember> allExcludedReadOnlyMembers = objectXamlType.GetAllExcludedReadOnlyMembers();
			List<XamlMember> list = new List<XamlMember>();
			foreach (XamlMember item in allMembers)
			{
				if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(item)))
				{
					list.Add(item);
				}
			}
			foreach (XamlMember item2 in allExcludedReadOnlyMembers)
			{
				if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(item2)))
				{
					list.Add(item2);
				}
			}
			return list;
		}

		private void WriteNodes(XamlXmlWriter writer)
		{
			List<List<XamlNode>> nodesList = writer.ppStateInfo.NodesList;
			writer.ppStateInfo.Reset();
			writer.currentState = writer.ppStateInfo.ReturnState;
			foreach (List<XamlNode> item in nodesList)
			{
				foreach (XamlNode item2 in item)
				{
					writer.currentState.WriteNode(writer, item2);
				}
			}
		}

		private void ThrowIfFailed(bool fail, string operation)
		{
			if (fail)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, operation));
			}
		}

		public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
		{
			if (!isObjectFromMember)
			{
				writer.ppStateInfo.Writer.WriteStartObject(type);
				ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartObject");
				XamlNode item = new XamlNode(XamlNodeType.StartObject, type);
				if (writer.ppStateInfo.CurrentDepth == 0)
				{
					writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
				}
				else
				{
					writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
				}
				writer.ppStateInfo.CurrentDepth++;
				return;
			}
			throw new InvalidOperationException(System.SR.Format(System.SR.XamlXmlWriterWriteNotSupportedInCurrentState, "WriteGetObject"));
		}

		public override void WriteEndObject(XamlXmlWriter writer)
		{
			writer.ppStateInfo.Writer.WriteEndObject();
			ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndObject");
			XamlNode item = new XamlNode(XamlNodeType.EndObject);
			writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
			writer.ppStateInfo.CurrentDepth--;
		}

		public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
		{
			writer.ppStateInfo.Writer.WriteStartMember(property);
			ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartMember");
			XamlNode item = new XamlNode(XamlNodeType.StartMember, property);
			if (writer.ppStateInfo.CurrentDepth == 0)
			{
				writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
			}
			else
			{
				writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
			}
		}

		public override void WriteEndMember(XamlXmlWriter writer)
		{
			writer.ppStateInfo.Writer.WriteEndMember();
			ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndMember");
			if (writer.ppStateInfo.CurrentDepth == 0)
			{
				ExpandPositionalParametersIntoProperties(writer);
				WriteNodes(writer);
			}
		}

		public override void WriteValue(XamlXmlWriter writer, string value)
		{
			writer.ppStateInfo.Writer.WriteValue(value);
			ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteValue");
			XamlNode item = new XamlNode(XamlNodeType.Value, value);
			if (writer.ppStateInfo.CurrentDepth == 0)
			{
				writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
			}
			else
			{
				writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
			}
		}
	}

	private class PositionalParameterStateInfo
	{
		public List<List<XamlNode>> NodesList { get; set; }

		public XamlMarkupExtensionWriter Writer { get; set; }

		public int CurrentDepth { get; set; }

		public WriterState ReturnState { get; set; }

		public PositionalParameterStateInfo(XamlXmlWriter xamlXmlWriter)
		{
			Writer = new XamlMarkupExtensionWriter(xamlXmlWriter, new XamlMarkupExtensionWriterSettings
			{
				ContinueWritingWhenPrefixIsNotFound = true
			});
			Reset();
		}

		public void Reset()
		{
			NodesList = new List<List<XamlNode>>();
			Writer.Reset();
			Writer.WriteStartObject(XamlLanguage.MarkupExtension);
			Writer.WriteStartMember(XamlLanguage.PositionalParameters);
			CurrentDepth = 0;
		}
	}

	private WriterState currentState;

	private XmlWriter output;

	private XamlXmlWriterSettings settings;

	private Stack<Frame> namespaceScopes;

	private Stack<List<XamlNode>> meNodesStack;

	private XamlMarkupExtensionWriter meWriter;

	private PositionalParameterStateInfo ppStateInfo;

	private string deferredValue;

	private bool deferredValueIsME;

	private bool isFirstElementOfWhitespaceSignificantCollection;

	private XamlSchemaContext schemaContext;

	private Dictionary<string, string> prefixAssignmentHistory;

	/// <summary>Gets the writer settings that this <see cref="T:System.Xaml.XamlXmlWriter" /> uses for XAML processing.</summary>
	/// <returns>The writer settings that this <see cref="T:System.Xaml.XamlXmlWriter" /> uses for XAML processing.</returns>
	public XamlXmlWriterSettings Settings => settings.Copy();

	/// <summary>Gets the XAML schema context that this <see cref="T:System.Xaml.XamlXmlWriter" /> uses for processing.</summary>
	/// <returns>The XAML schema context that this <see cref="T:System.Xaml.XamlXmlWriter" /> uses for XAML processing.</returns>
	public override XamlSchemaContext SchemaContext => schemaContext;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a stream.</summary>
	/// <param name="stream">The stream to write.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.-or-<paramref name="schemaContext" /> is null.</exception>
	public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext)
		: this(stream, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a stream using a writer settings object.</summary>
	/// <param name="stream">The stream to write.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <param name="settings">An instance of <see cref="T:System.Xaml.XamlXmlWriterSettings" />, which typically has specific non-default settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="schemaContext" /> is null</exception>
	public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		if (settings != null && settings.CloseOutput)
		{
			InitializeXamlXmlWriter(XmlWriter.Create(stream, new XmlWriterSettings
			{
				CloseOutput = true
			}), schemaContext, settings);
		}
		else
		{
			InitializeXamlXmlWriter(XmlWriter.Create(stream), schemaContext, settings);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a <see cref="T:System.IO.TextWriter" /> basis.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> that writes the output.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textWriter" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext)
		: this(textWriter, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a <see cref="T:System.IO.TextWriter" /> basis using a settings object.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> that writes the output.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <param name="settings">An instance of <see cref="T:System.Xaml.XamlXmlWriterSettings" />, which typically has specific non-default settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textWriter" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(textWriter, "textWriter");
		if (settings != null && settings.CloseOutput)
		{
			InitializeXamlXmlWriter(XmlWriter.Create(textWriter, new XmlWriterSettings
			{
				CloseOutput = true
			}), schemaContext, settings);
		}
		else
		{
			InitializeXamlXmlWriter(XmlWriter.Create(textWriter), schemaContext, settings);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a <see cref="T:System.Xml.XmlWriter" /> basis.</summary>
	/// <param name="xmlWriter">The <see cref="T:System.Xml.XmlWriter" /> that writes the output.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlWriter" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext)
		: this(xmlWriter, schemaContext, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlXmlWriter" /> class from a <see cref="T:System.Xml.XmlWriter" /> basis using a settings object.</summary>
	/// <param name="xmlWriter">The <see cref="T:System.Xml.XmlWriter" /> that writes the output.</param>
	/// <param name="schemaContext">The XAML schema context for the XAML writer.</param>
	/// <param name="settings">An instance of <see cref="T:System.Xaml.XamlXmlWriterSettings" />, which typically has specific non-default settings.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlWriter" /> or <paramref name="schemaContext" /> is null.</exception>
	public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(xmlWriter, "xmlWriter");
		InitializeXamlXmlWriter(xmlWriter, schemaContext, settings);
	}

	private void InitializeXamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
	{
		this.schemaContext = schemaContext ?? throw new ArgumentNullException("schemaContext");
		output = xmlWriter;
		this.settings = ((settings == null) ? new XamlXmlWriterSettings() : settings.Copy());
		currentState = Start.State;
		namespaceScopes = new Stack<Frame>();
		namespaceScopes.Push(new Frame
		{
			AllocatingNodeType = XamlNodeType.StartObject
		});
		prefixAssignmentHistory = new Dictionary<string, string> { { "xml", "http://www.w3.org/XML/1998/namespace" } };
		meNodesStack = new Stack<List<XamlNode>>();
		meWriter = new XamlMarkupExtensionWriter(this);
		ppStateInfo = new PositionalParameterStateInfo(this);
	}

	/// <summary>Releases the unmanaged resources used by <see cref="T:System.Xaml.XamlXmlWriter" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release the unmanaged resources; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !base.IsDisposed)
			{
				if (settings.CloseOutput)
				{
					output.Close();
				}
				else
				{
					Flush();
				}
				((IDisposable)meWriter).Dispose();
			}
		}
		finally
		{
			((IDisposable)output).Dispose();
			base.Dispose(disposing);
		}
	}

	/// <summary>Calls the Flush method of the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />, which writes anything that is currently in the buffer, and then closes the writer.</summary>
	public void Flush()
	{
		output.Flush();
	}

	/// <summary>Writes an object for cases where the specified object is a default or implicit value of the property that is being written, instead of being specified as an object value in the input XAML node set.</summary>
	public override void WriteGetObject()
	{
		CheckIsDisposed();
		XamlType type = null;
		Frame frame = namespaceScopes.Peek();
		if (frame.AllocatingNodeType == XamlNodeType.StartMember)
		{
			type = frame.Member.Type;
		}
		currentState.WriteObject(this, type, isObjectFromMember: true);
	}

	/// <summary>Writes a XAML start object node to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. Throws an exception if the current position of the XAML node stream is not in a scope where a start object can be written, or if the writer is not in a state that can write a start object.</summary>
	/// <param name="type">The <see cref="T:System.Xaml.XamlType" /> (XAML type identifier) for the object to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="type" /> is not a valid <see cref="T:System.Xaml.XamlType" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current position of the XAML node stream is not valid for writing a new start object.</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The state of the XAML writer is not valid for writing a new start object.</exception>
	public override void WriteStartObject(XamlType type)
	{
		CheckIsDisposed();
		ArgumentNullException.ThrowIfNull(type, "type");
		if (!type.IsNameValid)
		{
			throw new ArgumentException(System.SR.Format(System.SR.TypeHasInvalidXamlName, type.Name), "type");
		}
		currentState.WriteObject(this, type, isObjectFromMember: false);
		if (type.TypeArguments != null)
		{
			WriteTypeArguments(type);
		}
	}

	/// <summary>Writes a XAML end object node to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. Throws an exception if the current position of the XAML node stream that is being processed is incompatible with writing an end object.</summary>
	/// <exception cref="T:System.InvalidOperationException">The current position of the XAML node stream is not in a scope where an end member can be written.</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The current writer state does not support writing an end object.</exception>
	public override void WriteEndObject()
	{
		CheckIsDisposed();
		currentState.WriteEndObject(this);
	}

	/// <summary>Writes a XAML start member node to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. Throws an exception if the current position of the XAML node stream is within another member, or if it is not in a scope or writer state where a start member can be written.</summary>
	/// <param name="property">The XAML member identifier for the member to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="property" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="property" /> is not a valid <see cref="T:System.Xaml.XamlMember" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current position of the XAML node stream is invalid for writing a start member.</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The writer state is not valid for writing a start member.-or-The XAML writer attempted to write a duplicate member. This exception may have a more precise inner exception.</exception>
	public override void WriteStartMember(XamlMember property)
	{
		CheckIsDisposed();
		ArgumentNullException.ThrowIfNull(property, "property");
		if (!property.IsNameValid)
		{
			throw new ArgumentException(System.SR.Format(System.SR.MemberHasInvalidXamlName, property.Name), "property");
		}
		currentState.WriteStartMember(this, property);
	}

	/// <summary>Writes a XAML end member node to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. Throws an exception if the current position of the XAML node stream is not within a member, or if the internal writer state does not support writing to an end member.</summary>
	/// <exception cref="T:System.InvalidOperationException">The current position of the XAML node stream is not within a member.</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The current writer state does not support writing an end member.</exception>
	public override void WriteEndMember()
	{
		CheckIsDisposed();
		currentState.WriteEndMember(this);
	}

	/// <summary>Writes a XAML value node to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. Throws an exception if the current position of the XAML node stream is invalid for writing a value, or the writer is in a state where a value cannot be written.</summary>
	/// <param name="value">The value information to write.</param>
	/// <exception cref="T:System.InvalidOperationException">The current position of the XAML node stream is not valid for writing a value.</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The XAML writer state does not support the writing of a value node.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> specifies a value that is not null or a string.</exception>
	public override void WriteValue(object value)
	{
		CheckIsDisposed();
		if (value == null)
		{
			WriteStartObject(XamlLanguage.Null);
			WriteEndObject();
			return;
		}
		if (!(value is string value2))
		{
			throw new ArgumentException(System.SR.XamlXmlWriterCannotWriteNonstringValue, "value");
		}
		currentState.WriteValue(this, value2);
	}

	/// <summary>Writes namespace information to the underlying <see cref="T:System.Xml.XmlWriter" /> or <see cref="T:System.IO.TextWriter" />. May throw an exception for certain states; however, may instead defer writing the namespace information until the writer and the XAML node stream that is being processed reaches a position where a XAML namespace declaration can be inserted.</summary>
	/// <param name="namespaceDeclaration">The XAML namespace declaration to write.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="namespaceDeclaration" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="namespaceDeclaration" /> is not a valid XAML namespace declaration (has a null prefix or null identifier component).</exception>
	/// <exception cref="T:System.Xaml.XamlXmlWriterException">The current writer state does not support writing a XAML namespace declaration.</exception>
	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		CheckIsDisposed();
		ArgumentNullException.ThrowIfNull(namespaceDeclaration, "namespaceDeclaration");
		if (namespaceDeclaration.Prefix == null)
		{
			throw new ArgumentException(System.SR.NamespaceDeclarationPrefixCannotBeNull, "namespaceDeclaration");
		}
		if (namespaceDeclaration.Namespace == null)
		{
			throw new ArgumentException(System.SR.NamespaceDeclarationNamespaceCannotBeNull, "namespaceDeclaration");
		}
		if (namespaceDeclaration.Prefix == "xml")
		{
			throw new ArgumentException(System.SR.NamespaceDeclarationCannotBeXml, "namespaceDeclaration");
		}
		currentState.WriteNamespace(this, namespaceDeclaration);
	}

	private void CheckIsDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlXmlWriter");
		}
	}

	private static bool StringStartsWithCurly(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		if (s[0] == '{')
		{
			return true;
		}
		return false;
	}

	internal static bool IsImplicit(XamlMember xamlMember)
	{
		if (xamlMember.IsDirective)
		{
			if (!(xamlMember == XamlLanguage.Items) && !(xamlMember == XamlLanguage.Initialization) && !(xamlMember == XamlLanguage.PositionalParameters))
			{
				return xamlMember == XamlLanguage.UnknownContent;
			}
			return true;
		}
		return false;
	}

	internal static bool HasSignificantWhitespace(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		if (!ContainsLeadingSpace(s) && !ContainsTrailingSpace(s) && !ContainsConsecutiveInnerSpaces(s))
		{
			return ContainsWhitespaceThatIsNotSpace(s);
		}
		return true;
	}

	internal static bool ContainsLeadingSpace(string s)
	{
		return s[0] == ' ';
	}

	internal static bool ContainsTrailingSpace(string s)
	{
		return s[s.Length - 1] == ' ';
	}

	internal static bool ContainsConsecutiveInnerSpaces(string s)
	{
		for (int i = 1; i < s.Length - 1; i++)
		{
			if (s[i] == ' ' && s[i + 1] == ' ')
			{
				return true;
			}
		}
		return false;
	}

	internal static bool ContainsWhitespaceThatIsNotSpace(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '\t' || s[i] == '\n' || s[i] == '\r')
			{
				return true;
			}
		}
		return false;
	}

	private static void WriteXmlSpace(XamlXmlWriter writer)
	{
		writer.output.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
	}

	private static XamlType GetContainingXamlType(XamlXmlWriter writer)
	{
		Stack<Frame>.Enumerator enumerator = writer.namespaceScopes.GetEnumerator();
		XamlType result = null;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.AllocatingNodeType == XamlNodeType.StartMember && enumerator.Current.Member != XamlLanguage.Items)
			{
				result = ((enumerator.Current.Member == null || enumerator.Current.Member.IsUnknown) ? null : enumerator.Current.Member.Type);
				break;
			}
			if (enumerator.Current.AllocatingNodeType == XamlNodeType.StartObject)
			{
				result = enumerator.Current.Type;
				break;
			}
		}
		return result;
	}

	private void AssignNamespacePrefix(string ns, string prefix)
	{
		namespaceScopes.Peek().AssignNamespacePrefix(ns, prefix);
		if (prefixAssignmentHistory.TryGetValue(prefix, out var value))
		{
			if (value != ns)
			{
				prefixAssignmentHistory[prefix] = null;
			}
		}
		else
		{
			prefixAssignmentHistory.Add(prefix, ns);
		}
	}

	private bool IsShadowed(string ns, string prefix)
	{
		foreach (Frame namespaceScope in namespaceScopes)
		{
			if (namespaceScope.TryLookupNamespace(prefix, out var ns2))
			{
				return ns2 != ns;
			}
		}
		throw new InvalidOperationException(System.SR.Format(System.SR.PrefixNotInFrames, prefix));
	}

	private string FindPrefix(IList<string> namespaces, out string chosenNamespace)
	{
		string text = LookupPrefix(namespaces, out chosenNamespace);
		if (text == null)
		{
			chosenNamespace = namespaces[0];
			text = DefinePrefix(chosenNamespace);
			AssignNamespacePrefix(chosenNamespace, text);
		}
		else if (IsShadowed(chosenNamespace, text))
		{
			text = DefinePrefix(chosenNamespace);
			AssignNamespacePrefix(chosenNamespace, text);
		}
		return text;
	}

	internal string LookupPrefix(IList<string> namespaces, out string chosenNamespace)
	{
		chosenNamespace = null;
		foreach (Frame namespaceScope in namespaceScopes)
		{
			foreach (string @namespace in namespaces)
			{
				if (namespaceScope.TryLookupPrefix(@namespace, out var prefix))
				{
					chosenNamespace = @namespace;
					return prefix;
				}
			}
		}
		return null;
	}

	private bool IsPrefixEverUsedForAnotherNamespace(string prefix, string ns)
	{
		if (prefixAssignmentHistory.TryGetValue(prefix, out var value))
		{
			return ns != value;
		}
		return false;
	}

	private string DefinePrefix(string ns)
	{
		if (!IsPrefixEverUsedForAnotherNamespace(string.Empty, ns))
		{
			return string.Empty;
		}
		string preferredPrefix = SchemaContext.GetPreferredPrefix(ns);
		string text = preferredPrefix;
		int num = 0;
		while (IsPrefixEverUsedForAnotherNamespace(text, ns))
		{
			num++;
			text = preferredPrefix + num.ToString(TypeConverterHelper.InvariantEnglishUS);
		}
		if (!string.IsNullOrEmpty(text))
		{
			XmlConvert.VerifyNCName(text);
		}
		return text;
	}

	private void CheckMemberForUniqueness(XamlMember property)
	{
		if (!settings.AssumeValidInput)
		{
			Frame frame = namespaceScopes.Peek();
			if (frame.AllocatingNodeType != XamlNodeType.StartObject && frame.AllocatingNodeType != XamlNodeType.GetObject)
			{
				Frame item = namespaceScopes.Pop();
				frame = namespaceScopes.Peek();
				namespaceScopes.Push(item);
			}
			if (frame.Members == null)
			{
				frame.Members = new XamlPropertySet();
			}
			else if (frame.Members.Contains(property))
			{
				throw new XamlXmlWriterException(System.SR.Format(System.SR.XamlXmlWriterDuplicateMember, property.Name));
			}
			frame.Members.Add(property);
		}
	}

	private void WriteDeferredNamespaces(XamlNodeType nodeType)
	{
		Frame frame = namespaceScopes.Peek();
		if (frame.AllocatingNodeType != nodeType)
		{
			Frame item = namespaceScopes.Pop();
			frame = namespaceScopes.Peek();
			namespaceScopes.Push(item);
		}
		foreach (KeyValuePair<string, string> item2 in frame.GetSortedPrefixMap())
		{
			output.WriteAttributeString("xmlns", item2.Key, null, item2.Value);
		}
	}

	private void WriteTypeArguments(XamlType type)
	{
		if (TypeArgumentsContainNamespaceThatNeedsDefinition(type))
		{
			WriteUndefinedNamespaces(type);
		}
		WriteStartMember(XamlLanguage.TypeArguments);
		WriteValue(BuildTypeArgumentsString(type.TypeArguments));
		WriteEndMember();
	}

	private void WriteUndefinedNamespaces(XamlType type)
	{
		IList<string> xamlNamespaces = type.GetXamlNamespaces();
		string text = LookupPrefix(xamlNamespaces, out var chosenNamespace);
		if (text == null)
		{
			chosenNamespace = xamlNamespaces[0];
			text = DefinePrefix(chosenNamespace);
			currentState.WriteNamespace(this, new NamespaceDeclaration(chosenNamespace, text));
		}
		else if (IsShadowed(chosenNamespace, text))
		{
			text = DefinePrefix(chosenNamespace);
			currentState.WriteNamespace(this, new NamespaceDeclaration(chosenNamespace, text));
		}
		if (type.TypeArguments == null)
		{
			return;
		}
		foreach (XamlType typeArgument in type.TypeArguments)
		{
			WriteUndefinedNamespaces(typeArgument);
		}
	}

	private bool TypeArgumentsContainNamespaceThatNeedsDefinition(XamlType type)
	{
		string chosenNamespace;
		string text = LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);
		if (text == null || IsShadowed(chosenNamespace, text))
		{
			return true;
		}
		if (type.TypeArguments != null)
		{
			foreach (XamlType typeArgument in type.TypeArguments)
			{
				if (TypeArgumentsContainNamespaceThatNeedsDefinition(typeArgument))
				{
					return true;
				}
			}
		}
		return false;
	}

	private string BuildTypeArgumentsString(IList<XamlType> typeArguments)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (XamlType typeArgument in typeArguments)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(ConvertXamlTypeToString(typeArgument));
		}
		return stringBuilder.ToString();
	}

	private string ConvertXamlTypeToString(XamlType typeArgument)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ConvertXamlTypeToStringHelper(typeArgument, stringBuilder);
		return stringBuilder.ToString();
	}

	private void ConvertXamlTypeToStringHelper(XamlType type, StringBuilder builder)
	{
		string chosenNamespace;
		string text = LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);
		string typeName = GetTypeName(type);
		string typeName2 = (string.IsNullOrEmpty(text) ? typeName : (text + ":" + typeName));
		typeName2 = GenericTypeNameScanner.StripSubscript(typeName2, out var subscript);
		builder.Append(typeName2);
		if (type.TypeArguments != null)
		{
			bool flag = false;
			builder.Append('(');
			foreach (XamlType typeArgument in type.TypeArguments)
			{
				if (flag)
				{
					builder.Append(", ");
				}
				ConvertXamlTypeToStringHelper(typeArgument, builder);
				flag = true;
			}
			builder.Append(')');
		}
		if (subscript != null)
		{
			builder.Append(subscript);
		}
	}

	internal static string GetTypeName(XamlType type)
	{
		string result = type.Name;
		if (type.IsMarkupExtension && type.Name.EndsWith("Extension", ignoreCase: false, TypeConverterHelper.InvariantEnglishUS))
		{
			result = type.Name.Substring(0, type.Name.Length - "Extension".Length);
		}
		return result;
	}
}

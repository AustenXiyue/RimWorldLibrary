using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace MS.Internal.Annotations;

internal class Serializer
{
	private XmlRootAttribute _attribute;

	private ConstructorInfo _ctor;

	public Serializer(Type type)
	{
		Invariant.Assert(type != null);
		object[] customAttributes = type.GetCustomAttributes(inherit: false);
		foreach (object obj in customAttributes)
		{
			_attribute = obj as XmlRootAttribute;
			if (_attribute != null)
			{
				break;
			}
		}
		Invariant.Assert(_attribute != null, "Internal Serializer used for a type with no XmlRootAttribute.");
		_ctor = type.GetConstructor(Array.Empty<Type>());
	}

	public void Serialize(XmlWriter writer, object obj)
	{
		Invariant.Assert(writer != null && obj != null);
		IXmlSerializable obj2 = obj as IXmlSerializable;
		Invariant.Assert(obj2 != null, "Internal Serializer used for a type that isn't IXmlSerializable.");
		writer.WriteStartElement(_attribute.ElementName, _attribute.Namespace);
		obj2.WriteXml(writer);
		writer.WriteEndElement();
	}

	public object Deserialize(XmlReader reader)
	{
		Invariant.Assert(reader != null);
		IXmlSerializable obj = (IXmlSerializable)_ctor.Invoke(Array.Empty<object>());
		if (reader.ReadState == ReadState.Initial)
		{
			reader.Read();
		}
		obj.ReadXml(reader);
		return obj;
	}
}

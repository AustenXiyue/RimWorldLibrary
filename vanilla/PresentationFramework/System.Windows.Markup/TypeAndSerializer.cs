using System.Reflection;

namespace System.Windows.Markup;

internal class TypeAndSerializer
{
	public Type ObjectType;

	public Type SerializerType;

	public bool IsSerializerTypeSet;

	public PropertyInfo XmlLangProperty;
}

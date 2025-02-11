namespace System.Windows.Markup;

internal class AttributeData : DefAttributeData
{
	internal string PropertyName;

	internal Type SerializerType;

	internal short ExtensionTypeId;

	internal bool IsValueNestedExtension;

	internal bool IsValueTypeExtension;

	internal object Info;

	internal bool IsTypeExtension => ExtensionTypeId == 691;

	internal bool IsStaticExtension => ExtensionTypeId == 602;

	internal AttributeData(string targetAssemblyName, string targetFullName, Type targetType, string args, Type declaringType, string propertyName, object info, Type serializerType, int lineNumber, int linePosition, int depth, string targetNamespaceUri, short extensionTypeId, bool isValueNestedExtension, bool isValueTypeExtension, bool isSimple)
		: base(targetAssemblyName, targetFullName, targetType, args, declaringType, targetNamespaceUri, lineNumber, linePosition, depth, isSimple)
	{
		PropertyName = propertyName;
		SerializerType = serializerType;
		ExtensionTypeId = extensionTypeId;
		IsValueNestedExtension = isValueNestedExtension;
		IsValueTypeExtension = isValueTypeExtension;
		Info = info;
	}
}

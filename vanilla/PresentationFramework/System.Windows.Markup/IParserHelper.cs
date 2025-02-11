namespace System.Windows.Markup;

internal interface IParserHelper
{
	string LookupNamespace(string prefix);

	bool GetElementType(bool extensionFirst, string localName, string namespaceURI, ref string assemblyName, ref string typeFullName, ref Type baseType, ref Type serializerType);

	bool CanResolveLocalAssemblies();
}

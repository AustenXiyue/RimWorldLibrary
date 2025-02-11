namespace System.Windows.Markup;

internal class BamlWriterXamlTypeMapper : XamlTypeMapper
{
	internal BamlWriterXamlTypeMapper(string[] assemblyNames, NamespaceMapEntry[] namespaceMaps)
		: base(assemblyNames, namespaceMaps)
	{
	}

	protected sealed override bool AllowInternalType(Type type)
	{
		return true;
	}
}

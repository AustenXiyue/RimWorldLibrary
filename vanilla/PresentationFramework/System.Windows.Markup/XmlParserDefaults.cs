namespace System.Windows.Markup;

internal static class XmlParserDefaults
{
	private static readonly string[] _defaultAssemblies = new string[3] { "WindowsBase", "PresentationCore", "PresentationFramework" };

	private static readonly NamespaceMapEntry[] _defaultNamespaceMapTable = new NamespaceMapEntry[0];

	internal static XamlTypeMapper DefaultMapper => new XamlTypeMapper(GetDefaultAssemblyNames(), GetDefaultNamespaceMaps());

	internal static string[] GetDefaultAssemblyNames()
	{
		return (string[])_defaultAssemblies.Clone();
	}

	internal static NamespaceMapEntry[] GetDefaultNamespaceMaps()
	{
		return (NamespaceMapEntry[])_defaultNamespaceMapTable.Clone();
	}
}

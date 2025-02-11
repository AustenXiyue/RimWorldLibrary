namespace System.Windows.Markup;

internal struct ClrNamespaceAssemblyPair
{
	private string _assemblyName;

	private string _clrNamespace;

	internal string AssemblyName => _assemblyName;

	internal string ClrNamespace => _clrNamespace;

	internal ClrNamespaceAssemblyPair(string clrNamespace, string assemblyName)
	{
		_clrNamespace = clrNamespace;
		_assemblyName = assemblyName;
	}
}

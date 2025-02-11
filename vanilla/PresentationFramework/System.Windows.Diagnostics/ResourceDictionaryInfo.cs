using System.Diagnostics;
using System.Reflection;

namespace System.Windows.Diagnostics;

[DebuggerDisplay("Assembly = {Assembly?.GetName()?.Name}, ResourceDictionary SourceUri = {SourceUri?.AbsoluteUri}")]
public class ResourceDictionaryInfo
{
	public Assembly Assembly { get; private set; }

	public Assembly ResourceDictionaryAssembly { get; private set; }

	public ResourceDictionary ResourceDictionary { get; private set; }

	public Uri SourceUri { get; private set; }

	internal ResourceDictionaryInfo(Assembly assembly, Assembly resourceDictionaryAssembly, ResourceDictionary resourceDictionary, Uri sourceUri)
	{
		Assembly = assembly;
		ResourceDictionaryAssembly = resourceDictionaryAssembly;
		ResourceDictionary = resourceDictionary;
		SourceUri = sourceUri;
	}
}

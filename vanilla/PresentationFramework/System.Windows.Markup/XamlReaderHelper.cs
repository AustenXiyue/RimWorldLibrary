using System.Xaml;

namespace System.Windows.Markup;

internal class XamlReaderHelper
{
	internal const string DefinitionNamespaceURI = "http://schemas.microsoft.com/winfx/2006/xaml";

	internal const string DefinitionUid = "Uid";

	internal const string DefinitionType = "Type";

	internal const string DefinitionTypeArgs = "TypeArguments";

	internal const string DefinitionName = "Key";

	internal const string DefinitionRuntimeName = "Name";

	internal const string DefinitionShared = "Shared";

	internal const string DefinitionSynchronousMode = "SynchronousMode";

	internal const string DefinitionAsyncRecords = "AsyncRecords";

	internal const string DefinitionContent = "Content";

	internal const string DefinitionClass = "Class";

	internal const string DefinitionSubclass = "Subclass";

	internal const string DefinitionClassModifier = "ClassModifier";

	internal const string DefinitionFieldModifier = "FieldModifier";

	internal const string DefinitionCodeTag = "Code";

	internal const string DefinitionXDataTag = "XData";

	internal const string MappingProtocol = "clr-namespace:";

	internal const string MappingAssembly = ";assembly=";

	internal const string PresentationOptionsFreeze = "Freeze";

	internal const string DefaultNamespaceURI = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	internal const string DefinitionMetroNamespaceURI = "http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key";

	internal const string PresentationOptionsNamespaceURI = "http://schemas.microsoft.com/winfx/2006/xaml/presentation/options";

	private static XamlDirective _freezeDirective;

	internal static XamlDirective Freeze
	{
		get
		{
			if (_freezeDirective == null)
			{
				_freezeDirective = new XamlDirective("http://schemas.microsoft.com/winfx/2006/xaml/presentation/options", "Freeze");
			}
			return _freezeDirective;
		}
	}
}

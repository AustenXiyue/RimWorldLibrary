using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlRoutedEventNode : BamlTreeNode
{
	private string _assemblyName;

	private string _ownerTypeFullName;

	private string _eventIdName;

	private string _handlerName;

	internal BamlRoutedEventNode(string assemblyName, string ownerTypeFullName, string eventIdName, string handlerName)
		: base(BamlNodeType.RoutedEvent)
	{
		_assemblyName = assemblyName;
		_ownerTypeFullName = ownerTypeFullName;
		_eventIdName = eventIdName;
		_handlerName = handlerName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteRoutedEvent(_assemblyName, _ownerTypeFullName, _eventIdName, _handlerName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlRoutedEventNode(_assemblyName, _ownerTypeFullName, _eventIdName, _handlerName);
	}
}

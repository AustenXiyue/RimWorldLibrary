namespace System.Windows.Markup;

internal class XamlRoutedEventNode : XamlAttributeNode
{
	private RoutedEvent _routedEvent;

	private string _assemblyName;

	private string _typeFullName;

	private string _routedEventName;

	internal RoutedEvent Event => _routedEvent;

	internal string AssemblyName => _assemblyName;

	internal string TypeFullName => _typeFullName;

	internal string EventName => _routedEventName;

	internal XamlRoutedEventNode(int lineNumber, int linePosition, int depth, RoutedEvent routedEvent, string assemblyName, string typeFullName, string routedEventName, string value)
		: base(XamlNodeType.RoutedEvent, lineNumber, linePosition, depth, value)
	{
		_routedEvent = routedEvent;
		_assemblyName = assemblyName;
		_typeFullName = typeFullName;
		_routedEventName = routedEventName;
	}
}

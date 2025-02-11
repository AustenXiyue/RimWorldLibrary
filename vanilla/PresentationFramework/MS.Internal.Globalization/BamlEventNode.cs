using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlEventNode : BamlTreeNode
{
	private string _eventName;

	private string _handlerName;

	internal BamlEventNode(string eventName, string handlerName)
		: base(BamlNodeType.Event)
	{
		_eventName = eventName;
		_handlerName = handlerName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteEvent(_eventName, _handlerName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlEventNode(_eventName, _handlerName);
	}
}

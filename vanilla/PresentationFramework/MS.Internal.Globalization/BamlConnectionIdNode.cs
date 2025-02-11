using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlConnectionIdNode : BamlTreeNode
{
	private int _connectionId;

	internal BamlConnectionIdNode(int connectionId)
		: base(BamlNodeType.ConnectionId)
	{
		_connectionId = connectionId;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteConnectionId(_connectionId);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlConnectionIdNode(_connectionId);
	}
}

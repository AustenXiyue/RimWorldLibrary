using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;
using System.Windows.Markup.Localizer;

namespace MS.Internal.Globalization;

internal sealed class BamlResourceSerializer
{
	private BamlWriter _writer;

	private Stack<BamlTreeNode> _bamlTreeStack;

	internal static void Serialize(BamlLocalizer localizer, BamlTree tree, Stream output)
	{
		new BamlResourceSerializer().SerializeImp(localizer, tree, output);
	}

	private BamlResourceSerializer()
	{
	}

	private void SerializeImp(BamlLocalizer localizer, BamlTree tree, Stream output)
	{
		_writer = new BamlWriter(output);
		_bamlTreeStack = new Stack<BamlTreeNode>();
		_bamlTreeStack.Push(tree.Root);
		while (_bamlTreeStack.Count > 0)
		{
			BamlTreeNode bamlTreeNode = _bamlTreeStack.Pop();
			if (!bamlTreeNode.Visited)
			{
				bamlTreeNode.Visited = true;
				bamlTreeNode.Serialize(_writer);
				PushChildrenToStack(bamlTreeNode.Children);
			}
			else if (bamlTreeNode is BamlStartElementNode node)
			{
				localizer.RaiseErrorNotifyEvent(new BamlLocalizerErrorNotifyEventArgs(BamlTreeMap.GetKey(node), BamlLocalizerError.DuplicateElement));
			}
		}
	}

	private void PushChildrenToStack(List<BamlTreeNode> children)
	{
		if (children != null)
		{
			for (int num = children.Count - 1; num >= 0; num--)
			{
				_bamlTreeStack.Push(children[num]);
			}
		}
	}
}
